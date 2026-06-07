using System;
using Emilia.Kit;
using Emilia.Kit.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Emilia.Toolbar.Editor
{
    [InitializeOnLoad]
    public static class UnityToolbarUtility
    {
        private static Type _toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
        private static ScriptableObject _currentToolbar;

        private static VisualElement leftRoot;
        private static VisualElement rightRoot;

        private static Action _onToolbarGUILeft;
        private static Action _onToolbarGUIRight;

        public static event Action onToolbarGUILeft
        {
            add => _onToolbarGUILeft += value;
            remove => _onToolbarGUILeft -= value;
        }

        public static event Action onToolbarGUIRight
        {
            add => _onToolbarGUIRight += value;
            remove => _onToolbarGUIRight -= value;
        }

        static UnityToolbarUtility()
        {
            EditorKit.UnityInvoke(Initialize);
            EditorApplication.update += OnUpdate;
        }

        private static void OnUpdate()
        {
            if (leftRoot?.panel != null && rightRoot?.panel != null) return;

            leftRoot = null;
            rightRoot = null;
            EditorKit.UnityInvoke(Initialize);
        }

        private static void Initialize()
        {
            if (_currentToolbar != null) return;

            Object[] toolbars = Resources.FindObjectsOfTypeAll(_toolbarType);
            _currentToolbar = toolbars.Length > 0 ? (ScriptableObject) toolbars[0] : null;

            if (_currentToolbar == null) return;

            object rawRoot = ReflectUtility.GetValue(_currentToolbar, "m_Root");

            VisualElement mRoot = rawRoot as VisualElement;
            leftRoot = RegisterCallback("ToolbarZoneLeftAlign", _onToolbarGUILeft);
            rightRoot = RegisterCallback("ToolbarZoneRightAlign", _onToolbarGUIRight);

            return;

            VisualElement RegisterCallback(string rootName, Action onGUI)
            {
                VisualElement toolbarZone = mRoot.Q(rootName);

                VisualElement parent = new VisualElement();
                parent.style.flexGrow = 1;
                parent.style.flexDirection = FlexDirection.Row;

                IMGUIContainer container = new IMGUIContainer();
                container.style.flexGrow = 1;
                container.onGUIHandler += () => { onGUI?.Invoke(); };
                parent.Add(container);
                toolbarZone.Add(parent);

                return parent;
            }
        }

        public static void RepaintToolbar()
        {
            if (_currentToolbar == null) return;
            ReflectUtility.Invoke(null, _toolbarType, "RepaintToolbar");
        }
    }
}