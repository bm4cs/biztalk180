using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using btsmon.Model;
using NLog;
using Environment = btsmon.Model.Environment;

namespace btsmon.Command.Group
{
    public class SendPortGroupsCheck : ICommand
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Environment _environment;

        public SendPortGroupsCheck(Environment environment)
        {
            _environment = environment;
        }

        public List<Remediation> Execute()
        {
            throw new NotImplementedException();
        }
    }
}
