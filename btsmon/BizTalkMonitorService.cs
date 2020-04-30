using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
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
        private static readonly BizTalkApi BizTalkApi = new BizTalkApi();

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

            Boolean configNoLoady = false;
            Boolean continueWork = true;

            while (continueWork)
            {
                // the ping code is just for fun until we get things going
                Logger.Debug("Ping " + i);
                i++;
                Thread.Sleep(5000);

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

                    BizTalkApi.SetConfiguration(config);
                    BizTalkApi.ListAndStartHostInstances();
                }
            }
        }
    }
}