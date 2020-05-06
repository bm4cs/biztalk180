using System.ServiceProcess;
using NLog;

namespace btsmon.Service
{
    public partial class Service : ServiceBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly BizTalkMonitorService s;
        public Service()
        {
            InitializeComponent();
            s = new BizTalkMonitorService();
        }

        protected override void OnStart(string[] args)
        {
            Logger.Info("Start event");
            s.Start();
        }

        protected override void OnStop()
        {
            Logger.Info("Stop event");
            s.Stop();
        }

        protected override void OnShutdown()
        {
            Logger.Info("Windows is going shutdown");
            Stop();
        }


        public void Start()
        {
            OnStart(null);
        }
    }
}
