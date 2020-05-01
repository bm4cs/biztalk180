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
    public class ReceiveLocationsCheck : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly BtsCatalogExplorer _btsCatalogExplorer;
        private readonly Environment _environment;

        public ReceiveLocationsCheck(Environment environment)
        {
            _environment = environment;

            var connectionString =
                $"Integrated Security=SSPI;database={environment.MgmtDatabase};server={environment.GroupServer}" +
                (!string.IsNullOrEmpty(environment.GroupInstance) ? $"instance={environment.GroupInstance}" : "");

            Logger.Debug($"ReceiveLocationsCheck connection string '{connectionString}'");

            _btsCatalogExplorer = new BtsCatalogExplorer
            {
                ConnectionString = connectionString
            };
        }

        public List<Remediation> Execute()
        {
            try
            {
                var remediationList = new List<Remediation>();

                foreach (var receiveLocation in ListReceiveLocations())
                {
                    var receiveLocationMonitoringConfig = _environment.ReceiveLocations?.FirstOrDefault(h => h.Name.ToLower() == receiveLocation.Name);

                    if (
                        (receiveLocationMonitoringConfig == null
                         || receiveLocationMonitoringConfig.ExpectedState == "Up")
                        && receiveLocation.Enable == false)
                    {
                        try
                        {
                            receiveLocation.Enable = true;
                            _btsCatalogExplorer.SaveChanges();

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
                            _btsCatalogExplorer.DiscardChanges();
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
                            _btsCatalogExplorer.SaveChanges();

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
                            _btsCatalogExplorer.DiscardChanges();
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
                        from ReceivePort receivePort in _btsCatalogExplorer.ReceivePorts
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