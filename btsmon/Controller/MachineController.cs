using System.Collections.Generic;
using btsmon.Command;
using btsmon.Command.Machine;
using btsmon.Model;
using NLog;

namespace btsmon.Controller
{
    /// <summary>
    ///     Manages computer scoped checks such as:
    ///     * host instances
    ///     * performance counters
    ///     * IIS worker processes
    ///     * windows event logs
    /// </summary>
    public class MachineController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Environment _environment;

        private MachineController()
        {
        }

        public MachineController(Environment environment)
        {
            _environment = environment;
        }

        public List<Remediation> Execute()
        {
            var remediationList = new List<Remediation>();

            ICommand hostInstancesCheck = new HostInstancesCheck(_environment);
            var hostInstancesRemediation = hostInstancesCheck.Execute();
            remediationList.AddRange(hostInstancesRemediation);

            //TODO: Add more machine level checks

            return remediationList;
        }
    }
}