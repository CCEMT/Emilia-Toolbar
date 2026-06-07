using Emilia.Reflection.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public static class SwitchHotkey
    {
        [InitializeOnLoadMethod]
        public static void EditorInitialize()
        {
            EditorApplication_Internals.globalEventHandler_Internals += OnGlobalEventHandler;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        }

        private static void OnBeforeAssemblyReload()
        {
            EditorApplication_Internals.globalEventHandler_Internals -= OnGlobalEventHandler;
            if (SwitchWindow.window) SwitchWindow.CloseWindow(false);
            isActive = false;
        }

        private static bool isActive = false;

        private static void OnGlobalEventHandler()
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint || evt.type == EventType.Layout) return;

            bool canActive = evt.control && evt.keyCode == KeyCode.BackQuote && evt.type == EventType.KeyDown;
            if (canActive && isActive == false)
            {
                SwitchContext context = SwitchContext.Create(EditorWindow.focusedWindow);
                SwitchInfoCollection switchInfos = SwitchInfoUtility.GetSwitchInfos(context);
                if (switchInfos.HasItems)
                {
                    isActive = true;
                    SwitchWindow.OpenWindow(switchInfos);
                }
            }

            if (isActive)
            {
                if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.BackQuote) SwitchWindow.window?.Switch();
            }

            bool canDisable = evt.control == false && evt.keyCode != KeyCode.BackQuote && evt.type != EventType.KeyDown;
            if (canDisable && isActive)
            {
                isActive = false;
                SwitchWindow.CloseWindow(true);
            }
        }
    }
}
