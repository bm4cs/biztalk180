using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using btsmon.Controller;
using btsmon.Model;
using NLog;

namespace btsmon
{
    /// <summary>
    /// Monitors BizTalk artifacts (hosts, ports) and will 
    /// </summary>
    public class BizTalkMonitorService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly Thread _thread;
        public BizTalkMonitorService()
        {
            _thread = new Thread(DoWork);
        }

        public void Start()
        {
            _thread.Start();
        }
        public void Stop()
        {
            _thread.Abort();
        }


        private void DoWork()
        {
            var i = AppSettings.StartFrom;
            var pollingInterval = AppSettings.PollingIntervalSeconds * 1000;

            Boolean configNoLoady = false;
            Boolean continueWork = true;

            while (continueWork)
            {
                Logger.Debug("Ping " + i++);
                Thread.Sleep(pollingInterval);

                Configuration config = Configuration.LoadLocalFile("Configuration.json");

                if (config == null && !configNoLoady)
                {
                    Logger.Error("Could not parse configuration file.");
                    configNoLoady = true;
                }
                else if (config != null)
                {
                    configNoLoady = false;
                    Logger.Debug("Configuration file parsed OK. Continuing to health check biztalk services");


                    foreach (var environment in config.Environments)
                    {
                        var machineController = new MachineController(environment);
                        machineController.Execute();


                    }

                    
                }
            }
        }
    }
}