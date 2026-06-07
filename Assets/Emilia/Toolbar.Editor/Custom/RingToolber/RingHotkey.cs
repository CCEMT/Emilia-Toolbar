using Emilia.Reflection.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public static class RingHotkey
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
            if (RingWindow.window) RingWindow.CloseWindow(false);
            isActive = false;
        }

        private static bool isActive = false;

        private static void OnGlobalEventHandler()
        {
            Event evt = Event.current;

            if (evt.type == EventType.Repaint || evt.type == EventType.Layout) return;

            bool currentActive = evt.alt && evt.keyCode == KeyCode.BackQuote && evt.type == EventType.KeyDown;
            if (currentActive != isActive)
            {
                isActive = currentActive;
                if (currentActive) RingWindow.OpenWindow();
                else RingWindow.CloseWindow();
            }
        }
    }
}