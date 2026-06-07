using System.Collections.Generic;

namespace Emilia.Toolbar.Editor
{
    public interface ICommandCollect
    {
        IEnumerable<CommandInfo> Collect();
    }
}