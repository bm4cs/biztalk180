using System;
using System.Collections.Generic;
using btsmon.Model;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Group
{
    public class SendPortGroupsCheck : BaseGroupCheck, ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public SendPortGroupsCheck(Environment environment) : base(environment)
        {
        }

        public List<Remediation> Execute()
        {
            throw new NotImplementedException();
        }
    }
}