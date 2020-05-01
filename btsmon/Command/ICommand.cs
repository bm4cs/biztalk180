using System.Collections.Generic;
using btsmon.Model;

namespace btsmon.Command
{
    public interface ICommand
    {
        List<Remediation> Execute();
        // void Undo();
        // void Redo();
    }
}