using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using btsmon.Model;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Machine
{
    public class HostInstancesCheck : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Environment _environment;

        public HostInstancesCheck(Environment env)
        {
            _environment = env;
        }

        public List<Remediation> Execute()
        {
            try
            {
                var remediationList = new List<Remediation>();

                var enumOptions = new EnumerationOptions();
                enumOptions.ReturnImmediately = false;

                foreach (var srvr in _environment.Servers)
                {
                    //Search for all HostInstances of 'InProcess' type in the Biztalk namespace scope  
                    var searchObject = new ManagementObjectSearcher(
                        $@"\\{srvr.Name}\root\MicrosoftBizTalkServer",
                        "Select * from MSBTS_HostInstance where HostType=1",
                        enumOptions);

                    foreach (ManagementObject inst in searchObject.Get()) // Should be only one
                    {
                        var serviceState = inst["ServiceState"].ToString(); // 1 = stopped, 
                        var isDisabled = inst["IsDisabled"].ToString(); // FALSE or TRUE
                        var hostName = inst["HostName"].ToString();
                        var runningServer = inst["RunningServer"].ToString();

                        var hi = _environment.HostInstances?.FirstOrDefault(h => h.Name.ToLower() == hostName.ToLower());
                        if (
                            (hi == null // no configuraton for the host instance - assume service wants to be enabled and up
                             || hi.ExpectedState == "Up")
                            && serviceState == "1") // stopped
                        {
                            try
                            {
                                inst.InvokeMethod("Start", null);
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
                        else if (hi != null && hi.ExpectedState == "Down" && serviceState == "4")
                        {
                            try
                            {
                                inst.InvokeMethod("Stop", null);
                                Logger.Debug($"HostInstance of Host: {hostName} and Server: {runningServer} was stopped successfully");

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
            catch (COMException ex)
            {
                Logger.Error("COM Failure while starting HostInstances - " + ex.Message);
                if (ex.Message.StartsWith("BizTalk Server cannot access SQL server"))
                    throw; //QUESTION: why bubble this and risk killing the service?
            }
            catch (Exception excep)
            {
                Logger.Error("Failure while starting HostInstances - " + excep.Message);
            }

            return new List<Remediation>();
        }
    }
}
