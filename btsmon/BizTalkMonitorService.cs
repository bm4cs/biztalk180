using System;
using System.Threading;
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

            while (true)
            {
                // the ping code is just for fun until we get things going
                Logger.Debug("Ping " + i);
                i++;
                Thread.Sleep(5000);

                // 2020-04-21 13:02 this works
                BizTalkApi.ListAndStartHostInstances();

                //TODO: have fun Rob
                // im enjoying not having to use the windows event log (which is SLOW) and text logs are nice
                // 
            }
        }
    }
}