using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Emilia.Toolbar.Editor
{
    public class SwitchContext
    {
        public EditorWindow focusedWindow;
        public UnityEngine.Object[] selectionObjects;
        public UnityEngine.Object activeObject;
        public GameObject activeGameObject;
        public bool isPlaying;
        public PrefabStage prefabStage;
        public Scene activeScene;

        public Type focusedWindowType => focusedWindow?.GetType();

        public static SwitchContext Create(EditorWindow focusedWindow)
        {
            return new SwitchContext
            {
                focusedWindow = focusedWindow,
                selectionObjects = Selection.objects,
                activeObject = Selection.activeObject,
                activeGameObject = Selection.activeGameObject,
                isPlaying = EditorApplication.isPlaying,
                prefabStage = PrefabStageUtility.GetCurrentPrefabStage(),
                activeScene = SceneManager.GetActiveScene()
            };
        }

        public bool IsFocusedWindow(Type windowType)
        {
            if (windowType == null || focusedWindow == null) return false;
            return windowType.IsInstanceOfType(focusedWindow);
        }

        public bool IsFocusedWindow(string windowTypeName)
        {
            Type type = Type.GetType(windowTypeName);
            if (type != null) return IsFocusedWindow(type);

            Type currentType = focusedWindowType;
            if (currentType == null || string.IsNullOrEmpty(windowTypeName)) return false;

            string fullName = windowTypeName;
            int assemblySeparatorIndex = fullName.IndexOf(",", StringComparison.Ordinal);
            if (assemblySeparatorIndex >= 0) fullName = fullName.Substring(0, assemblySeparatorIndex).Trim();

            return string.Equals(currentType.FullName, fullName, StringComparison.Ordinal) ||
                   string.Equals(currentType.AssemblyQualifiedName, windowTypeName, StringComparison.Ordinal);
        }
    }
}
