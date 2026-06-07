using System.Collections.Generic;
using System.IO;
using Emilia.Kit.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Emilia.Toolbar.Editor
{
    [InitializeOnLoad]
    public class RecentlyOpenedSceneSwitchSelector : ISwitchSelector
    {
        private static List<string> onRecentlyOpenScenePaths = new();

        static RecentlyOpenedSceneSwitchSelector()
        {
            onRecentlyOpenScenePaths = RecentlyOpenHistoryStore.LoadScenePaths();
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        public int priority => 0;

        public FixedSwitchInfo[] GetFixedSwitchInfos(SwitchContext context) => null;

        public SwitchGroup GetSwitchGroup(SwitchContext context)
        {
            if (context == null || context.IsFocusedWindow(typeof(SceneView)) == false) return null;
            if (onRecentlyOpenScenePaths == null) return null;

            List<SwitchInfo> infos = new();
            for (var i = 0; i < onRecentlyOpenScenePaths.Count; i++)
            {
                string scenePath = onRecentlyOpenScenePaths[i];
                if (string.IsNullOrEmpty(scenePath)) continue;
                if (string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(scenePath))) continue;

                string label = Path.GetFileNameWithoutExtension(scenePath);
                if (string.IsNullOrEmpty(label)) continue;

                string capturedScenePath = scenePath;

                infos.Add(new SwitchInfo {
                    label = label,
                    icon = EditorGUIUtility.FindTexture("UnityLogo"),
                    action = () => OpenScene(capturedScenePath)
                });
            }

            return new SwitchGroup("最近打开的场景", priority, infos);
        }

        private static void OpenScene(string assetPath)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(assetPath);
            if (sceneAsset == null) return;

            OpenAssetUtility.Open(sceneAsset);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            string scenePath = scene.path;

            if (onRecentlyOpenScenePaths == null) onRecentlyOpenScenePaths = new List<string>();
            if (onRecentlyOpenScenePaths.Contains(scenePath)) onRecentlyOpenScenePaths.Remove(scenePath);
            onRecentlyOpenScenePaths.Insert(0, scenePath);

            RecentlyOpenHistoryStore.SaveScenePaths(onRecentlyOpenScenePaths);
        }
    }
}
