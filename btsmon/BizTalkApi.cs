using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using btsmon.Model;
using NLog;

namespace btsmon
{
    public class BizTalkApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void ListAndStartHostInstances(Configuration config)
        {
            try
            {
                EnumerationOptions enumOptions = new EnumerationOptions();
                enumOptions.ReturnImmediately = false;


                foreach (Model.Environment env in config?.Environments)
                {
                    foreach(Model.Server srvr in env.Servers)
                    {
                        //Search for all HostInstances of 'InProcess' type in the Biztalk namespace scope  
                        ManagementObjectSearcher searchObject = new ManagementObjectSearcher(
                            $@"\\{srvr.Name}\root\MicrosoftBizTalkServer",
                            "Select * from MSBTS_HostInstance where HostType=1",
                            enumOptions);

                        foreach (ManagementObject inst in searchObject.Get()) // Should be only one
                        {
                            String serviceState = inst["ServiceState"].ToString(); // 1 = stopped, 
                            String isDisabled = inst["IsDisabled"].ToString(); // FALSE or TRUE
                            String hostName = inst["HostName"].ToString(); 
                            String runningServer = inst["RunningServer"].ToString(); 

                            HostInstance hi = env.HostInstances?.FirstOrDefault(h => h.Name.ToLower() == hostName);
                            if ((hi == null // no configuraton for the host instance - assume service wants to be enabled and up
                                    || hi.ExpectedState == "Up" )
                                && serviceState == "1") // stopped
                            {
                                inst.InvokeMethod("Start", null);
                                Logger.Debug($"HostInstance of Host: {hostName} and Server: {runningServer} was started successfully");
                            }
                            else if (hi.ExpectedState == "Down" && serviceState == "4")
                            {
                                inst.InvokeMethod("Stop", null);
                                Logger.Debug($"HostInstance of Host: {hostName} and Server: {runningServer} was stopped successfully");
                            }
                            // else all good
                        }
                    }
                }
            }
            catch (COMException ex)
            {
                Logger.Error("COM Failure while starting HostInstances - " + ex.Message);
                if (ex.Message.StartsWith("BizTalk Server cannot access SQL server"))
                    throw;
            }
            catch (Exception excep)
            {
                Logger.Error("Failure while starting HostInstances - " + excep.Message);
            }
        }

    }

}

