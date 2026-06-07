using Emilia.Kit.Editor;
using Emilia.Reflection.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public static class ConsoleHotkey
    {
        private static HotkeyConfig hotkeyConfig;

        [InitializeOnLoadMethod]
        public static void EditorInitialize()
        {
            EditorApplication_Internals.globalEventHandler_Internals += OnGlobalEventHandler;
            hotkeyConfig = ConsoleSetting.instance.hotkeyConfig;
        }

        private static void OnGlobalEventHandler()
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint || evt.type == EventType.Layout) return;
            if (hotkeyConfig != ConsoleSetting.instance.hotkeyConfig)
            {
                hotkeyConfig = ConsoleSetting.instance.hotkeyConfig;
                return;
            }

            bool isTrigger = hotkeyConfig.Check(evt);
            if (isTrigger && evt.type == EventType.KeyDown) ConsoleCommandWindow.Open();
        }
    }
}