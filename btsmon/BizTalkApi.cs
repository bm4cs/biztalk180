using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using btsmon.Model;
using Microsoft.BizTalk.ExplorerOM;
using NLog;

namespace btsmon
{
    public class BizTalkApi
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private BtsCatalogExplorer _btsCatalogExplorer; //HACK: only currently supports one instance
        private Configuration _config;

        public void SetConfiguration(Configuration config)
        {
            _config = config;

            if (_btsCatalogExplorer == null)
            {
                var env = _config.Environments[0]; //HACK: refactor

                _btsCatalogExplorer = new BtsCatalogExplorer
                {
                    ConnectionString = $"Integrated Security=SSPI;database={env.MgmtDatabase};server={env.GroupServer}" +
                                       (!string.IsNullOrEmpty(env.GroupInstance) ? $"instance={env.GroupInstance}" : "")
                };
            }
        }

        public void ListAndStartHostInstances()
        {
            try
            {
                var enumOptions = new EnumerationOptions();
                enumOptions.ReturnImmediately = false;


                foreach (var env in _config?.Environments)
                foreach (var srvr in env.Servers)
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

                        var hi = env.HostInstances?.FirstOrDefault(h => h.Name.ToLower() == hostName);
                        if (
                            (hi == null // no configuraton for the host instance - assume service wants to be enabled and up
                             || hi.ExpectedState == "Up")
                            && serviceState == "1") // stopped
                        {
                            inst.InvokeMethod("Start", null);
                            Logger.Debug(
                                $"HostInstance of Host: {hostName} and Server: {runningServer} was started successfully");
                        }
                        else if (hi.ExpectedState == "Down" && serviceState == "4")
                        {
                            inst.InvokeMethod("Stop", null);
                            Logger.Debug(
                                $"HostInstance of Host: {hostName} and Server: {runningServer} was stopped successfully");
                        }

                        // else all good
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

        public List<ReceiveLocation> ListReceiveLocations()
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

        public bool EnableReceiveLocation(string receiveLocationName)
        {
            try
            {
                var receivePort = _btsCatalogExplorer.ReceivePorts[receiveLocationName];

                foreach (ReceiveLocation receiveLocation in receivePort.ReceiveLocations)
                    receiveLocation.Enable = true;

                _btsCatalogExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _btsCatalogExplorer.DiscardChanges();
                return false;
            }
        }

        public bool DisableReceiveLocation(string receiveLocationName)
        {
            try
            {
                var receivePort = _btsCatalogExplorer.ReceivePorts[receiveLocationName];

                foreach (ReceiveLocation receiveLocation in receivePort.ReceiveLocations)
                    receiveLocation.Enable = false;

                _btsCatalogExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _btsCatalogExplorer.DiscardChanges();
                return false;
            }
        }


        public List<SendPort> ListSendPorts()
        {
            try
            {
                return _btsCatalogExplorer.SendPorts.Cast<SendPort>().Where(sendPort => !sendPort.IsDynamic).ToList();
            }
            catch (Exception e)
            {
                Logger.Error(e);
                return null;
            }
        }

        public bool StartSendPort(string sendPortName)
        {
            try
            {
                var sendPort = _btsCatalogExplorer.SendPorts[sendPortName];
                sendPort.Status = PortStatus.Started;
                _btsCatalogExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _btsCatalogExplorer.DiscardChanges();
                return false;
            }
        }

        public bool UnenlistSendPort(string sSendPortName)
        {
            try
            {
                var sp = _btsCatalogExplorer.SendPorts[sSendPortName];
                sp.Status = PortStatus.Bound;

                _btsCatalogExplorer.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                _btsCatalogExplorer.DiscardChanges();
                return false;
            }
        }

        public bool StartOrchestration(string sOrchestrationName)
        {
            var btsAssemblyCollection = _btsCatalogExplorer.Assemblies;

            foreach (BtsAssembly btsAssembly in btsAssemblyCollection)
                if (sOrchestrationName.Split(',')[1] == btsAssembly.DisplayName.Split(',')[0])
                    foreach (BtsOrchestration btsOrchestration in btsAssembly.Orchestrations)
                        if (sOrchestrationName == btsOrchestration.AssemblyQualifiedName)
                        {
                            btsOrchestration.Status = OrchestrationStatus.Started;
                            try
                            {
                                _btsCatalogExplorer.SaveChanges();
                                return true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                _btsCatalogExplorer.DiscardChanges();
                                return false;
                            }
                        }

            return false;
        }

        public bool UnenlistOrchestration(string sOrchestrationName)
        {
            var btsAssemblyCollection = _btsCatalogExplorer.Assemblies;

            foreach (BtsAssembly btsAssembly in btsAssemblyCollection)
                if (sOrchestrationName.Split(',')[1] == btsAssembly.DisplayName.Split(',')[0])
                    foreach (BtsOrchestration btsOrchestration in btsAssembly.Orchestrations)
                        if (sOrchestrationName == btsOrchestration.AssemblyQualifiedName)
                        {
                            btsOrchestration.AutoTerminateInstances = true;
                            btsOrchestration.Status = OrchestrationStatus.Unenlisted;
                            try
                            {
                                _btsCatalogExplorer.SaveChanges();
                                return true;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                _btsCatalogExplorer.DiscardChanges();
                                return false;
                            }
                        }

            return false;
        }
    }
}