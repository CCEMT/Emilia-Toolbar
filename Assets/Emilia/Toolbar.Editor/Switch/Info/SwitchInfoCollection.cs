using System.Collections.Generic;

namespace Emilia.Toolbar.Editor
{
    public class SwitchInfoCollection
    {
        public string title = "Switch";
        public List<FixedSwitchInfo> fixedSwitchInfos = new List<FixedSwitchInfo>();
        public List<SwitchGroup> switchGroups = new List<SwitchGroup>();

        public bool HasItems => fixedSwitchInfos.Count > 0 || GetSwitchInfoCount() > 0;

        public List<SwitchInfo> GetSwitchInfosInDisplayOrder()
        {
            List<SwitchInfo> infos = new List<SwitchInfo>();
            for (int groupIndex = 0; groupIndex < switchGroups.Count; groupIndex++)
            {
                SwitchGroup group = switchGroups[groupIndex];
                if (group?.switchInfos == null) continue;

                for (int infoIndex = 0; infoIndex < group.switchInfos.Count; infoIndex++)
                {
                    SwitchInfo info = group.switchInfos[infoIndex];
                    if (info != null) infos.Add(info);
                }
            }
            return infos;
        }

        private int GetSwitchInfoCount()
        {
            int count = 0;
            for (int i = 0; i < switchGroups.Count; i++)
            {
                SwitchGroup group = switchGroups[i];
                if (group?.switchInfos == null) continue;
                count += group.switchInfos.Count;
            }
            return count;
        }
    }
}
