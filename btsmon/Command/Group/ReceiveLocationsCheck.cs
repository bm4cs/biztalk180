using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using BTS = Microsoft.BizTalk.ExplorerOM;
using NLog;
using Environment = btsmon.Model.Environment;
using ReceiveLocation = Microsoft.BizTalk.ExplorerOM.ReceiveLocation;

namespace btsmon.Command.Group
{
    public class ReceiveLocationsCheck : BaseGroupCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public ReceiveLocationsCheck(Environment environment) : base(environment)
        { }

        public List<Remediation> Execute()
        {
            try
            {
                var remediationList = new List<Remediation>();

                foreach (BTS.ReceiveLocation receiveLocation in ListReceiveLocations())
                {
                    Model.ReceiveLocation config = Environment.Applications?.SelectMany(a => a.ReceiveLocations)?
                        .FirstOrDefault(h => h.Name.ToLower() == receiveLocation.Name.ToLower());

                    if (config?.ExpectedState == ExpectedEnableState.DontCare) continue;

                    String expectedState;
                    Boolean newStatus;
                    String failText;
                    String actualState = receiveLocation.Enable == true ? "Enabled"
                        : "Disabled";

                    if (config == null || config.ExpectedState == ExpectedEnableState.Enabled)
                    {
                        expectedState = "Enabled";
                        newStatus = true;
                        failText = $"Failed to enable receive location {receiveLocation.Name}";
                    }
                    else
                    {
                        expectedState = "Disabled";
                        newStatus = false;
                        failText = $"Failed to disable receive location {receiveLocation.Name}";
                    }

                    if (expectedState != actualState)
                    {
                        Remediation remediation = new Remediation
                        {
                            Name = receiveLocation.Name,
                            Type = ArtifactType.ReceiveLocation,
                            ExpectedState = expectedState,
                            ActualState = actualState,
                            RepairedTime = DateTime.Now,
                            Success = false
                        };

                        try
                        {
                            receiveLocation.Enable = newStatus;
                            BtsCatExplorer.SaveChanges();
                            remediation.Success = true;
                        }
                        catch (Exception receiveLocationStartException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error(failText);
                            Logger.Error(receiveLocationStartException);
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

        private List<ReceiveLocation> ListReceiveLocations()
        {
            try
            {
                return (
                        from ReceivePort receivePort in BtsCatExplorer.ReceivePorts
                        from ReceiveLocation receiveLocation in receivePort.ReceiveLocations
                        select receiveLocation)
                    .ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}