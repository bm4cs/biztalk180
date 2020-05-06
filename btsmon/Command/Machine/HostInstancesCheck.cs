using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using btsmon.Model;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Machine
{
    public class HostInstancesCheck : BaseMachineCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HostInstancesCheck(Environment environment) : base(environment)
        {
        }

        public List<Remediation> Execute()
        {
            try
            {
                var remediationList = new List<Remediation>();

                var enumOptions = new EnumerationOptions {ReturnImmediately = false};

                foreach (var server in Environment.Servers)
                {
                    //Search for all HostInstances of 'InProcess' type in the Biztalk namespace scope  
                    var searchObject = new ManagementObjectSearcher(
                        $@"\\{server.Name}\root\MicrosoftBizTalkServer",
                        "Select * from MSBTS_HostInstance where HostType=1",
                        enumOptions);

                    foreach (ManagementObject hostInstanceManagementObject in searchObject.Get()) // Should be only one
                    {
                        var serviceState = hostInstanceManagementObject["ServiceState"].ToString(); // 1 = stopped, 
                        var isDisabled = hostInstanceManagementObject["IsDisabled"].ToString(); // FALSE or TRUE
                        var hostName = hostInstanceManagementObject["HostName"].ToString();
                        var runningServer = hostInstanceManagementObject["RunningServer"].ToString();

                        var hostInstanceMonitoringConfig = Environment.HostInstances?.FirstOrDefault(h =>
                            h.Name.ToLower() == hostName.ToLower());


                        if (
                            (hostInstanceMonitoringConfig == null // no configuraton for the host instance - assume service wants to be enabled and up
                             || hostInstanceMonitoringConfig.ExpectedState == "Up")
                            && serviceState == "1") // stopped
                        {
                            try
                            {
                                hostInstanceManagementObject.InvokeMethod("Start", null);
                                Logger.Debug($"HostInstance of Host: {hostName} and Server: {runningServer} was started successfully");

                                remediationList.Add(new Remediation
                                {
                                    Name = hostName,
                                    Type = ArtifactType.HostInstance,
                                    ExpectedState = "Up",
                                    ActualState = "Down",
                                    RepairedTime = DateTime.Now,
                                    Success = true
                                });
                            }
                            catch (Exception startHostInstanceException)
                            {
                                Logger.Error($"Failed to start host instance {hostName}");
                                Logger.Error(startHostInstanceException);

                                remediationList.Add(new Remediation
                                {
                                    Name = hostName,
                                    Type = ArtifactType.HostInstance,
                                    ExpectedState = "Up",
                                    ActualState = "Down",
                                    RepairedTime = DateTime.Now,
                                    Success = false
                                });
                            }
                        }
                        else if (hostInstanceMonitoringConfig != null && hostInstanceMonitoringConfig.ExpectedState == "Down" && serviceState == "4")
                        {
                            try
                            {
                                hostInstanceManagementObject.InvokeMethod("Stop", null);
                                Logger.Debug(
                                    $"HostInstance of Host: {hostName} and Server: {runningServer} was stopped successfully");

                                remediationList.Add(new Remediation
                                {
                                    Name = hostName,
                                    Type = ArtifactType.HostInstance,
                                    ExpectedState = "Down",
                                    ActualState = "Up",
                                    RepairedTime = DateTime.Now,
                                    Success = true
                                });
                            }
                            catch (Exception stopHostInstanceException)
                            {
                                Logger.Error($"Failed to stop host instance {hostName}");
                                Logger.Error(stopHostInstanceException);

                                remediationList.Add(new Remediation
                                {
                                    Name = hostName,
                                    Type = ArtifactType.HostInstance,
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
            }
            catch (COMException comException)
            {
                Logger.Error("COM Failure while starting HostInstances - " + comException.Message);
                Logger.Error(comException);
            }
            catch (Exception exception)
            {
                Logger.Error("Failure while starting HostInstances - " + exception.Message);
                Logger.Error(exception);
            }

            return new List<Remediation>();
        }
    }
}