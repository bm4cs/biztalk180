using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using NLog;
using Environment = btsmon.Model.Environment;
using BTS = Microsoft.BizTalk.ExplorerOM;

namespace btsmon.Command.Group
{
    public class SendPortGroupsCheck : BaseGroupCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendPortGroupsCheck(Environment environment) : base(environment)
        {
        }

        public List<Remediation> Execute()
        {
            try
            {
                var remediationList = new List<Remediation>();

                foreach (var sendPortGroup in ListSendPortGroups())
                {
                    var config = Environment.Applications?.SelectMany(a => a.SendPortGroups)
                        .FirstOrDefault(p => p.Name.ToLower() == sendPortGroup.Name.ToLower());

                    if (config?.ExpectedState == ExpectedInstanceState.DontCare) continue;

                    String expectedState;
                    PortStatus newStatus;
                    String failText;
                    String actualState = sendPortGroup.Status == BTS.PortStatus.Bound ? "Enlisted"
                        : sendPortGroup.Status == BTS.PortStatus.Bound ? "Unenlisted"
                        : "Started";

                    if (config == null || config.ExpectedState == ExpectedInstanceState.Started)
                    {
                        expectedState = "Started";
                        newStatus = BTS.PortStatus.Started;
                        failText = $"Failed to start send port group {sendPortGroup.Name}";
                    }
                    else
                    {
                        expectedState = "Unenlisted";
                        newStatus = BTS.PortStatus.Stopped;
                        failText = $"Failed to unenlist send port group {sendPortGroup.Name}";
                    }

                    if (actualState != expectedState)
                    {
                        Remediation remediation = new Remediation
                        {
                            Name = sendPortGroup.Name,
                            Type = ArtifactType.SendPort,
                            ExpectedState = expectedState,
                            ActualState = actualState,
                            RepairedTime = DateTime.Now,
                            Success = false
                        };

                        try
                        {
                            sendPortGroup.Status = newStatus;
                            BtsCatExplorer.SaveChanges();
                            remediation.Success = true;
                        }
                        catch (Exception startException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error(failText);
                            Logger.Error(startException);
                        }
                        remediationList.Add(remediation);
                    }
                }

                return remediationList;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }

            return new List<Remediation>();
        }

        private List<BTS.SendPortGroup> ListSendPortGroups()
        {
            try
            {
                List<BTS.SendPortGroup> result = new List<BTS.SendPortGroup>();
                foreach (BTS.SendPortGroup spg in BtsCatExplorer.SendPortGroups)
                    result.Add(spg);
                return result;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}