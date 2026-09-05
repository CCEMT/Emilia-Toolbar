using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Toolbar.Editor
{
    public class RingCommandCustomSetting : ProjectLocalSetting<RingCommandCustomSetting>
    {
        public List<RingCustomCommandInfo> ringCustomCommandInfos = new List<RingCustomCommandInfo>();
        public List<FixedCustomCommandInfo> leftFixedCustomCommandInfos = new List<FixedCustomCommandInfo>();
        public List<FixedCustomCommandInfo> rightFixedCustomCommandInfos = new List<FixedCustomCommandInfo>();
    }
}