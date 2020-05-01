using System.Collections.Generic;
using btsmon.Model;
using NLog;

namespace btsmon.Notification
{
    public class Emailer
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Send(List<Remediation> remediationList)
        {
            //TODO: Send an email summarising remediation activities
        }
    }
}