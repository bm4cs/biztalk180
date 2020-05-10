using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using BTS = Microsoft.BizTalk.ExplorerOM;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Group
{
    public class SendPortsCheck : BaseGroupCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendPortsCheck(Environment environment) : base(environment)
        { }

        public List<Remediation> Execute()
        {
            try
            {
                var remediationList = new List<Remediation>();

                foreach (BTS.SendPort sendPort in ListSendPorts())
                {
                    var config = Environment.Applications?.SelectMany(a => a.SendPorts)
                        .FirstOrDefault(p => p.Name.ToLower() == sendPort.Name.ToLower());

                    if (config?.ExpectedState == ExpectedInstanceState.DontCare) continue;

                    String expectedState;
                    PortStatus newStatus;
                    String failText;
                    String actualState = sendPort.Status == PortStatus.Bound ? "Enlisted"
                        : sendPort.Status == PortStatus.Bound ? "Unenlisted"
                        : "Started";

                    if (config == null || config.ExpectedState == ExpectedInstanceState.Started)
                    {
                        expectedState = "Started";
                        newStatus = PortStatus.Started;
                        failText = $"Failed to start send port {sendPort.Name}";
                    }
                    else
                    {
                        expectedState = "Unenlisted";
                        newStatus = PortStatus.Stopped;
                        failText = $"Failed to unenlist send port {sendPort.Name}";
                    }

                    if (actualState != expectedState)
                    {
                        Remediation remediation = new Remediation
                            {
                                Name = sendPort.Name,
                                Type = ArtifactType.SendPort,
                                ExpectedState = expectedState,
                                ActualState = actualState,
                                RepairedTime = DateTime.Now,
                                Success = false
                            };

                        try
                        {
                            sendPort.Status = newStatus;
                            BtsCatExplorer.SaveChanges();
                            remediation.Success = true;
                        }
                        catch (Exception sendPortStartException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error(failText);
                            Logger.Error(sendPortStartException);
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

        private List<BTS.SendPort> ListSendPorts()
        {
            try
            {
                return BtsCatExplorer.SendPorts.Cast<BTS.SendPort>().Where(sendPort => !sendPort.IsDynamic).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}