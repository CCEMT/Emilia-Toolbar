using System.Collections.Generic;
using System.Linq;

namespace Emilia.Toolbar.Editor
{
    public class SwitchGroup
    {
        public string title;
        public int priority;
        public List<SwitchInfo> switchInfos = new List<SwitchInfo>();

        public bool HasItems => switchInfos != null && switchInfos.Count > 0;

        public SwitchGroup() { }

        public SwitchGroup(string title, int priority, IEnumerable<SwitchInfo> switchInfos)
        {
            this.title = title;
            this.priority = priority;
            this.switchInfos = switchInfos?.Where(info => info != null).ToList() ?? new List<SwitchInfo>();
        }
    }
}
