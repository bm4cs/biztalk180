using System;
using System.Collections.Generic;
using System.Linq;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
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

                foreach (var sendPort in ListSendPorts())
                {
                    var sendPortMonitoringConfig = Environment.SendPorts?.FirstOrDefault(p => p.Name.ToLower() == sendPort.Name.ToLower());

                    if (
                        (sendPortMonitoringConfig == null
                         || sendPortMonitoringConfig.ExpectedState == "Up")
                        && sendPort.Status == PortStatus.Stopped)
                    {
                        try
                        {
                            sendPort.Status = PortStatus.Started;
                            BtsCatExplorer.SaveChanges();

                            remediationList.Add(new Remediation
                            {
                                Name = sendPort.Name,
                                Type = ArtifactType.SendPort,
                                ExpectedState = "Up",
                                ActualState = "Down",
                                RepairedTime = DateTime.Now,
                                Success = true
                            });
                        }
                        catch (Exception sendPortStartException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error($"Failed to start send port {sendPort.Name}");
                            Logger.Error(sendPortStartException);

                            remediationList.Add(new Remediation
                            {
                                Name = sendPort.Name,
                                Type = ArtifactType.SendPort,
                                ExpectedState = "Up",
                                ActualState = "Down",
                                RepairedTime = DateTime.Now,
                                Success = false
                            });
                        }
                    }
                    else if (sendPortMonitoringConfig != null &&
                             sendPortMonitoringConfig.ExpectedState == "Down" &&
                             sendPort.Status == PortStatus.Started)
                    {
                        try
                        {
                            sendPort.Status = PortStatus.Stopped;
                            BtsCatExplorer.SaveChanges();

                            remediationList.Add(new Remediation
                            {
                                Name = sendPort.Name,
                                Type = ArtifactType.SendPort,
                                ExpectedState = "Down",
                                ActualState = "Up",
                                RepairedTime = DateTime.Now,
                                Success = true
                            });
                        }
                        catch (Exception sendPortStopException)
                        {
                            BtsCatExplorer.DiscardChanges();
                            Logger.Error($"Failed to stop send port {sendPort.Name}");
                            Logger.Error(sendPortStopException);

                            remediationList.Add(new Remediation
                            {
                                Name = sendPort.Name,
                                Type = ArtifactType.SendPort,
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

        private List<SendPort> ListSendPorts()
        {
            try
            {
                return BtsCatExplorer.SendPorts.Cast<SendPort>().Where(sendPort => !sendPort.IsDynamic).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }
    }
}