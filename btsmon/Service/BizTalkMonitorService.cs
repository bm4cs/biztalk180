using System;
using System.Collections.Generic;
using System.Threading;
using btsmon.Controller;
using btsmon.Model;
using btsmon.Notification;
using NLog;

namespace btsmon.Service
{
    /// <summary>
    ///     Monitors BizTalk artifacts (hosts, ports) and will
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
            try
            {
                var pollingInterval = AppSettings.PollingIntervalSeconds * 1000;

                var configNoLoady = false;
                var continueWork = true;

                while (continueWork)
                {
                    Thread.Sleep(pollingInterval);

                    var config = Configuration.LoadLocalFile("Configuration.json");

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
                            var remediationList = new List<Remediation>();

                            var machineController = new MachineController(environment);
                            var machineRemediationList = machineController.Execute();
                            remediationList.AddRange(machineRemediationList);

                            var groupController = new GroupController(environment);
                            var groupRemediationList = groupController.Execute();
                            remediationList.AddRange(groupRemediationList);

                            if (remediationList.Count > 0) Emailer.Send(environment, remediationList);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }
        }
    }
}