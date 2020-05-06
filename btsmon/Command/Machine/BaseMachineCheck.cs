using btsmon.Model;

namespace btsmon.Command.Machine
{
    /// <summary>
    ///     Base behaviour for a machine related command.
    /// </summary>
    public class BaseMachineCheck
    {
        public BaseMachineCheck(Environment environment)
        {
            Environment = environment;
        }

        protected Environment Environment { get; }
    }
}