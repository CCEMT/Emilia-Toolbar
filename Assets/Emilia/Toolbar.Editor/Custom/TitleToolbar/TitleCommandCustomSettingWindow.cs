using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using OdinMenuEditorWindow = Sirenix.OdinInspector.Editor.OdinMenuEditorWindow;
using OdinMenuTree = Sirenix.OdinInspector.Editor.OdinMenuTree;

namespace Emilia.Toolbar.Editor
{
    public class TitleCommandCustomSettingWindow : OdinMenuEditorWindow
    {
        [MenuItem("Emilia/Toolbar/Setting/Title")]
        public static void Open()
        {
            EditorImGUIKit.OpenWindow<TitleCommandCustomSettingWindow>("Title Custom Setting", 600, 600);
        }

        [NonSerialized]
        private List<TitleCustomCommandInfoGroup> groups = new List<TitleCustomCommandInfoGroup>();

        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree tree = new OdinMenuTree();
            groups.Clear();
            tree.Add("标题栏左侧", AddGroup(TitleCustomCommandInfo.TitlePositionType.LeftLeft));
            tree.Add("播放栏左侧", AddGroup(TitleCustomCommandInfo.TitlePositionType.LeftRight));
            tree.Add("播放栏右侧", AddGroup(TitleCustomCommandInfo.TitlePositionType.RightLeft));
            tree.Add("标题栏右侧", AddGroup(TitleCustomCommandInfo.TitlePositionType.RightRight));
            return tree;
        }

        private TitleCustomCommandInfoGroup AddGroup(TitleCustomCommandInfo.TitlePositionType positionType)
        {
            TitleCustomCommandInfoGroup group = new TitleCustomCommandInfoGroup(positionType);
            if (groups == null) this.groups = new List<TitleCustomCommandInfoGroup>();
            this.groups.Add(group);
            return group;
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
            if (TitleCommandCustomSetting.instance.customCommands == null) TitleCommandCustomSetting.instance.customCommands = new List<TitleCustomCommandInfo>();
            else TitleCommandCustomSetting.instance.customCommands.Clear();

            int priority = 0;

            foreach (var group in groups)
            {
                foreach (var item in group.customCommands)
                {
                    priority++;

                    TitleCustomCommandInfo info = new TitleCustomCommandInfo();
                    info.positionType = group.positionType;
                    info.color = item.color;
                    info.icon = item.icon;
                    info.sdfIcon = item.sdfIcon;
                    info.text = item.text;
                    info.priority = priority;
                    info.commandName = item.commandNameSelector.commandName;

                    TitleCommandCustomSetting.instance.customCommands.Add(info);
                }
            }

            TitleCommandCustomSetting.Save();
        }

        [Serializable]
        public class TitleCustomCommandInfoGroup
        {
            [HideInInspector]
            public TitleCustomCommandInfo.TitlePositionType positionType;

            [LabelText("自定义命令列表")]
            public List<TitleCustomCommandInfoItem> customCommands = new List<TitleCustomCommandInfoItem>();

            public TitleCustomCommandInfoGroup(TitleCustomCommandInfo.TitlePositionType positionType)
            {
                this.positionType = positionType;

                if (TitleCommandCustomSetting.instance.customCommands == null) return;

                foreach (TitleCustomCommandInfo titleCustomCommandInfo in TitleCommandCustomSetting.instance.customCommands)
                {
                    if (titleCustomCommandInfo.positionType != positionType) continue;
                    TitleCustomCommandInfoItem item = new TitleCustomCommandInfoItem();
                    item.color = titleCustomCommandInfo.color;
                    item.icon = titleCustomCommandInfo.icon;
                    item.sdfIcon = titleCustomCommandInfo.sdfIcon;
                    item.text = titleCustomCommandInfo.text;
                    item.commandNameSelector = new CommandNameSelector(titleCustomCommandInfo.commandName);
                    customCommands.Add(item);
                }
            }
        }

        [Serializable]
        public class TitleCustomCommandInfoItem
        {
            [LabelText("颜色")]
            public Color color = Color.white;

            [LabelText("自定义图标")]
            public Texture icon;

            [LabelText("预设图标"), HideIf(nameof(icon))]
            public SdfIconType sdfIcon;

            [LabelText("文本")]
            public string text;

            public CommandNameSelector commandNameSelector;
        }
    }
}