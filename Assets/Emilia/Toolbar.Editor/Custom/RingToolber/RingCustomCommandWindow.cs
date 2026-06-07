using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class RingCustomCommandWindow : OdinMenuEditorWindow
    {
        [MenuItem("Emilia/Toolbar/Setting/Ring")]
        public static void Open()
        {
            EditorImGUIKit.OpenWindow<RingCustomCommandWindow>("RingCustomCommand", 600, 600);
        }

        [NonSerialized]
        private RingCustomCommandInfoGroup ringGroup;

        [NonSerialized]
        private FixedCustomCommandInfoGroup leftFixedGroup;

        [NonSerialized]
        private FixedCustomCommandInfoGroup rightFixedGroup;

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();

            ringGroup = new RingCustomCommandInfoGroup();
            leftFixedGroup = new FixedCustomCommandInfoGroup(RingCommandCustomSetting.instance.leftFixedCustomCommandInfos);
            rightFixedGroup = new FixedCustomCommandInfoGroup(RingCommandCustomSetting.instance.rightFixedCustomCommandInfos);

            tree.Add("圆环命令", ringGroup);
            tree.Add("左侧固定命令", leftFixedGroup);
            tree.Add("右侧固定命令", rightFixedGroup);

            return tree;
        }

        protected override void OnBeginDrawEditors()
        {
            if (MenuTree == null) return;
            base.OnBeginDrawEditors();

            int toolbarHeight = MenuTree.Config.SearchToolbarHeight;

            SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);

            GUI.color = Color.green;

            if (SirenixEditorGUI.ToolbarButton("保存"))
            {
                OnSave();
                ForceMenuTreeRebuild();
                UnityToolbarUtility.RepaintToolbar();
            }

            GUI.color = Color.white;

            SirenixEditorGUI.EndHorizontalToolbar();
        }

        private void OnSave()
        {
            RingCommandCustomSetting.instance.ringCustomCommandInfos.Clear();
            RingCommandCustomSetting.instance.leftFixedCustomCommandInfos.Clear();
            RingCommandCustomSetting.instance.rightFixedCustomCommandInfos.Clear();

            for (var i = 0; i < this.ringGroup.items.Count; i++)
            {
                RingCustomCommandInfoItem item = this.ringGroup.items[i];

                RingCustomCommandInfo info = new RingCustomCommandInfo();
                info.name = item.name;
                info.description = item.description;
                info.icon = item.icon;
                info.sdfIcon = item.sdfIcon;
                info.color = item.color;
                info.commandName = item.commandNameSelector.commandName;

                RingCommandCustomSetting.instance.ringCustomCommandInfos.Add(info);
            }

            for (var i = 0; i < this.leftFixedGroup.items.Count; i++)
            {
                FixedCustomCommandInfoItem item = this.leftFixedGroup.items[i];

                FixedCustomCommandInfo info = new FixedCustomCommandInfo();
                info.name = item.name;
                info.icon = item.icon;
                info.sdfIcon = item.sdfIcon;
                info.color = item.color;
                info.commandName = item.commandNameSelector.commandName;

                RingCommandCustomSetting.instance.leftFixedCustomCommandInfos.Add(info);
            }

            for (var i = 0; i < this.rightFixedGroup.items.Count; i++)
            {
                FixedCustomCommandInfoItem item = this.rightFixedGroup.items[i];

                FixedCustomCommandInfo info = new FixedCustomCommandInfo();
                info.name = item.name;
                info.icon = item.icon;
                info.sdfIcon = item.sdfIcon;
                info.color = item.color;
                info.commandName = item.commandNameSelector.commandName;

                RingCommandCustomSetting.instance.rightFixedCustomCommandInfos.Add(info);
            }

            RingCommandCustomSetting.Save();
        }

        [Serializable]
        public class RingCustomCommandInfoGroup
        {
            [LabelText("命令列表")]
            public List<RingCustomCommandInfoItem> items = new List<RingCustomCommandInfoItem>();

            public RingCustomCommandInfoGroup()
            {
                for (var i = 0; i < RingCommandCustomSetting.instance.ringCustomCommandInfos.Count; i++)
                {
                    RingCustomCommandInfo info = RingCommandCustomSetting.instance.ringCustomCommandInfos[i];
                    RingCustomCommandInfoItem item = new RingCustomCommandInfoItem();
                    item.name = info.name;
                    item.description = info.description;
                    item.icon = info.icon;
                    item.sdfIcon = info.sdfIcon;
                    item.color = info.color;
                    item.commandNameSelector = new CommandNameSelector(info.commandName);
                    this.items.Add(item);
                }
            }
        }

        [Serializable]
        public class RingCustomCommandInfoItem
        {
            [LabelText("名称")]
            public string name;

            [LabelText("描述")]
            public string description;

            [LabelText("自定义图标")]
            public Texture icon;

            [LabelText("预设图标"), HideIf(nameof(icon))]
            public SdfIconType sdfIcon;

            [LabelText("颜色")]
            public Color color = Color.white;

            public CommandNameSelector commandNameSelector;
        }

        [Serializable]
        public class FixedCustomCommandInfoGroup
        {
            [LabelText("命令列表")]
            public List<FixedCustomCommandInfoItem> items = new List<FixedCustomCommandInfoItem>();

            public FixedCustomCommandInfoGroup(List<FixedCustomCommandInfo> list)
            {
                if (list == null) return;

                for (var i = 0; i < list.Count; i++)
                {
                    FixedCustomCommandInfo info = list[i];

                    FixedCustomCommandInfoItem item = new FixedCustomCommandInfoItem();
                    item.name = info.name;
                    item.icon = info.icon;
                    item.sdfIcon = info.sdfIcon;
                    item.color = info.color;
                    item.commandNameSelector = new CommandNameSelector(info.commandName);

                    this.items.Add(item);
                }
            }
        }

        [Serializable]
        public class FixedCustomCommandInfoItem
        {
            [LabelText("名称")]
            public string name;

            [LabelText("自定义图标")]
            public Texture icon;

            [LabelText("预设图标"), HideIf(nameof(icon))]
            public SdfIconType sdfIcon;

            [LabelText("颜色")]
            public Color color = Color.white;

            public CommandNameSelector commandNameSelector;
        }
    }
}