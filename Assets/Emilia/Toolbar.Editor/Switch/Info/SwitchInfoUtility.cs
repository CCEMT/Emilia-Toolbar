using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public static class SwitchInfoUtility
    {
        private const string DefaultTitle = "Switch";

        private static readonly List<SwitchSelectorEntry> selectorEntries = new List<SwitchSelectorEntry>();

        static SwitchInfoUtility()
        {
            IList<Type> types = TypeCache.GetTypesDerivedFrom<ISwitchSelector>();

            int amount = types.Count;
            for (int i = 0; i < amount; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;
                if (type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) == null) continue;

                try
                {
                    ISwitchSelector selector = (ISwitchSelector) Activator.CreateInstance(type);
                    if (selector == null) continue;
                    selectorEntries.Add(new SwitchSelectorEntry(selector, selectorEntries.Count));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        public static bool Has(SwitchContext context)
        {
            return GetSwitchInfos(context).HasItems;
        }

        public static bool Has(SwitchContext context, IEnumerable<ISwitchSelector> selectors)
        {
            return GetSwitchInfos(context, selectors).HasItems;
        }

        public static SwitchInfoCollection GetSwitchInfos(SwitchContext context)
        {
            return GetSwitchInfos(context, selectorEntries);
        }

        public static SwitchInfoCollection GetSwitchInfos(SwitchContext context, IEnumerable<ISwitchSelector> selectors)
        {
            List<SwitchSelectorEntry> entries = new List<SwitchSelectorEntry>();
            if (selectors != null)
            {
                foreach (ISwitchSelector selector in selectors)
                {
                    if (selector == null) continue;
                    entries.Add(new SwitchSelectorEntry(selector, entries.Count));
                }
            }
            return GetSwitchInfos(context, entries);
        }

        private static SwitchInfoCollection GetSwitchInfos(SwitchContext context, IEnumerable<SwitchSelectorEntry> entries)
        {
            SwitchInfoCollection collection = new SwitchInfoCollection();
            if (context == null || entries == null) return collection;

            List<SwitchSelectorEntry> orderedEntries = entries.ToList();
            orderedEntries.Sort(CompareSelectorEntries);

            List<SwitchGroupEntry> groupEntries = new List<SwitchGroupEntry>();
            foreach (SwitchSelectorEntry entry in orderedEntries)
            {
                AddFixedSwitchInfos(collection.fixedSwitchInfos, entry.selector, context);
                AddSwitchGroup(groupEntries, entry, context);
            }

            groupEntries.Sort(CompareGroupEntries);
            foreach (SwitchGroupEntry entry in groupEntries) collection.switchGroups.Add(entry.group);

            SwitchGroup titleGroup = collection.switchGroups.FirstOrDefault(group => string.IsNullOrEmpty(group.title) == false);
            collection.title = titleGroup?.title ?? DefaultTitle;

            return collection;
        }

        private static void AddFixedSwitchInfos(List<FixedSwitchInfo> fixedSwitchInfos, ISwitchSelector selector, SwitchContext context)
        {
            try
            {
                FixedSwitchInfo[] infos = selector.GetFixedSwitchInfos(context);
                if (infos == null) return;

                for (int i = 0; i < infos.Length; i++)
                {
                    if (infos[i] != null) fixedSwitchInfos.Add(infos[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void AddSwitchGroup(List<SwitchGroupEntry> groupEntries, SwitchSelectorEntry selectorEntry, SwitchContext context)
        {
            try
            {
                SwitchGroup group = selectorEntry.selector.GetSwitchGroup(context);
                if (group == null || group.HasItems == false) return;
                groupEntries.Add(new SwitchGroupEntry(group, selectorEntry.registrationIndex));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static int CompareSelectorEntries(SwitchSelectorEntry left, SwitchSelectorEntry right)
        {
            int priorityComparison = right.selector.priority.CompareTo(left.selector.priority);
            if (priorityComparison != 0) return priorityComparison;
            return left.registrationIndex.CompareTo(right.registrationIndex);
        }

        private static int CompareGroupEntries(SwitchGroupEntry left, SwitchGroupEntry right)
        {
            int priorityComparison = right.group.priority.CompareTo(left.group.priority);
            if (priorityComparison != 0) return priorityComparison;
            return left.registrationIndex.CompareTo(right.registrationIndex);
        }

        private readonly struct SwitchSelectorEntry
        {
            public readonly ISwitchSelector selector;
            public readonly int registrationIndex;

            public SwitchSelectorEntry(ISwitchSelector selector, int registrationIndex)
            {
                this.selector = selector;
                this.registrationIndex = registrationIndex;
            }
        }

        private readonly struct SwitchGroupEntry
        {
            public readonly SwitchGroup group;
            public readonly int registrationIndex;

            public SwitchGroupEntry(SwitchGroup group, int registrationIndex)
            {
                this.group = group;
                this.registrationIndex = registrationIndex;
            }
        }
    }
}
