using System.Collections.Generic;
using btsmon.Model;
using NLog;

namespace btsmon.Command.Machine
{
    public class WebServerProcessesCheck : BaseMachineCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public WebServerProcessesCheck(Environment environment) : base(environment)
        {
        }

        public List<Remediation> Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}
