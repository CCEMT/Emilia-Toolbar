using System;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class ConsoleSetting : LocalSetting<ConsoleSetting>
    {
        [LabelText("收集MenuItem")]
        public bool isCollectMenuItem;

        [LabelText("打开时暂停游戏")]
        public bool openPause;

        [LabelText("Console强制聚焦")]
        public bool consoleForceFocus;

        [LabelText("拼音搜索")]
        public bool pinyinSearch = true;

        [LabelText("控制台快捷键")]
        public HotkeyConfig hotkeyConfig = new HotkeyConfig {keyCode = KeyCode.Quote};
    }
}