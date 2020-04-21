using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace btsmon
{
    public class BizTalkApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void ListAndStartHostInstances()
        {
            try
            {
                EnumerationOptions enumOptions = new EnumerationOptions();
                enumOptions.ReturnImmediately = false;

                //Search for all HostInstances of 'InProcess' type in the Biztalk namespace scope  
                ManagementObjectSearcher searchObject = new ManagementObjectSearcher(
                    "root\\MicrosoftBizTalkServer", 
                    "Select * from MSBTS_HostInstance where HostType=1", 
                    enumOptions);

                foreach (ManagementObject inst in searchObject.Get())
                {
                    //Check if ServiceState is 'Stopped'  
                    if (inst["ServiceState"].ToString() == "1")
                    {
                        inst.InvokeMethod("Start", null);
                        Logger.Debug("HostInstance of Host: " + inst["HostName"] + " and Server: " + inst["RunningServer"] + " was started successfully");
                    }
                }
            }
            catch (Exception excep)
            {
                Logger.Error("Failure while starting HostInstances - " + excep.Message);
            }
        }

    }

}

