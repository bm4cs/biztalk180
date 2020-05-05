using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
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

                foreach (var receiveLocation in ListReceiveLocations())
                {
                    var receiveLocationMonitoringConfig = Environment.ReceiveLocations?.FirstOrDefault(h => h.Name.ToLower() == receiveLocation.Name);

                    if (
                        (receiveLocationMonitoringConfig == null
                         || receiveLocationMonitoringConfig.ExpectedState == "Up")
                        && receiveLocation.Enable == false)
                    {
                        try
                        {
                            receiveLocation.Enable = true;
                            BtsCatExplorer.SaveChanges();

                            remediationList.Add(new Remediation
                            {
                                Name = receiveLocation.Name,
                                Type = ArtifactType.ReceiveLocation,
                                ExpectedState = "Up",
                                ActualState = "Down",
                                RepairedTime = DateTime.Now,
                                Success = true
                            });
                        }
                        catch (Exception receiveLocationStartException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error($"Failed to start receive location {receiveLocation.Name}");
                            Logger.Error(receiveLocationStartException);

                            remediationList.Add(new Remediation
                            {
                                Name = receiveLocation.Name,
                                Type = ArtifactType.ReceiveLocation,
                                ExpectedState = "Up",
                                ActualState = "Down",
                                RepairedTime = DateTime.Now,
                                Success = false
                            });
                        }
                    }
                    else if (receiveLocationMonitoringConfig.ExpectedState == "Down" &&
                             receiveLocation.Enable == true)
                    {
                        try
                        {
                            receiveLocation.Enable = false;
                            BtsCatExplorer.SaveChanges();

                            remediationList.Add(new Remediation
                            {
                                Name = receiveLocation.Name,
                                Type = ArtifactType.ReceiveLocation,
                                ExpectedState = "Down",
                                ActualState = "Up",
                                RepairedTime = DateTime.Now,
                                Success = true
                            });
                        }
                        catch (Exception receiveLocationStopException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error($"Failed to stop receive location {receiveLocation.Name}");
                            Logger.Error(receiveLocationStopException);

                            remediationList.Add(new Remediation
                            {
                                Name = receiveLocation.Name,
                                Type = ArtifactType.ReceiveLocation,
                                ExpectedState = "Down",
                                ActualState = "Up",
                                RepairedTime = DateTime.Now,
                                Success = false
                            });
                        }
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