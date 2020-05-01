using System.Collections.Generic;
using btsmon.Command;
using btsmon.Command.Group;
using btsmon.Model;
using NLog;

namespace btsmon.Controller
{
    /// <summary>
    ///     Manages BizTalk Group scoped checks and operations:
    ///     * Receive Locations
    ///     * Send Ports
    ///     * Send Port Groups
    ///     * Orchestrations
    /// </summary>
    public class GroupController
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Environment _environment;

        private GroupController()
        {
        }

        public GroupController(Environment environment)
        {
            _environment = environment;
        }

        public List<Remediation> Execute()
        {
            var remediationList = new List<Remediation>();

            ICommand receiveLocationsCheck = new ReceiveLocationsCheck(_environment);
            var receiveLocationsRemediation = receiveLocationsCheck.Execute();
            remediationList.AddRange(receiveLocationsRemediation);

            ICommand sendPortsCheck = new SendPortsCheck(_environment);
            var sendPortsRemediation = sendPortsCheck.Execute();
            remediationList.AddRange(sendPortsRemediation);

            return remediationList;
        }
    }
}