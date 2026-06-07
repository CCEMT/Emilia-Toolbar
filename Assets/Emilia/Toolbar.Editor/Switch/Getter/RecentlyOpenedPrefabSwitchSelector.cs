using System.Collections.Generic;
using System.IO;
using Emilia.Kit.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [InitializeOnLoad]
    public class RecentlyOpenedPrefabSwitchSelector : ISwitchSelector
    {
        private const string HierarchyWindowTypeName = "UnityEditor.SceneHierarchyWindow, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        private static List<string> onRecentlyOpenPrefabPaths = new();

        static RecentlyOpenedPrefabSwitchSelector()
        {
            onRecentlyOpenPrefabPaths = RecentlyOpenHistoryStore.LoadPrefabPaths();
            PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        }

        public int priority => 0;

        public FixedSwitchInfo[] GetFixedSwitchInfos(SwitchContext context) => null;

        public SwitchGroup GetSwitchGroup(SwitchContext context)
        {
            if (context == null || context.IsFocusedWindow(HierarchyWindowTypeName) == false) return null;
            if (onRecentlyOpenPrefabPaths == null) return null;

            List<SwitchInfo> infos = new();
            for (var i = 0; i < onRecentlyOpenPrefabPaths.Count; i++)
            {
                string prefabPath = onRecentlyOpenPrefabPaths[i];
                if (string.IsNullOrEmpty(prefabPath)) continue;
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(prefabPath))) continue;

                string label = Path.GetFileNameWithoutExtension(prefabPath);
                if (string.IsNullOrEmpty(label)) continue;

                string capturedPrefabPath = prefabPath;

                infos.Add(new SwitchInfo {
                    label = label,
                    icon = EditorGUIUtility.FindTexture("Prefab Icon"),
                    action = () => OpenPrefab(capturedPrefabPath)
                });
            }

            return new SwitchGroup("最近打开的预制体", priority, infos);
        }

        private static void OpenPrefab(string assetPath)
        {
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefabAsset == null) return;

            OpenAssetUtility.Open(prefabAsset);
        }

        private static void OnPrefabStageOpened(PrefabStage prefabStage)
        {
            string prefabPath = prefabStage.assetPath;

            if (onRecentlyOpenPrefabPaths == null) onRecentlyOpenPrefabPaths = new List<string>();
            if (onRecentlyOpenPrefabPaths.Contains(prefabPath)) onRecentlyOpenPrefabPaths.Remove(prefabPath);
            onRecentlyOpenPrefabPaths.Insert(0, prefabPath);

            RecentlyOpenHistoryStore.SavePrefabPaths(onRecentlyOpenPrefabPaths);
        }
    }
}
