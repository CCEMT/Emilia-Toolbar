using System.Collections.Generic;
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
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
                if (sceneAsset == null) continue;

                infos.Add(new SwitchInfo {
                    label = sceneAsset.name,
                    icon = EditorGUIUtility.FindTexture("UnityLogo"),
                    action = () => OpenAssetUtility.Open(sceneAsset)
                });
            }

            return new SwitchGroup("最近打开的场景", priority, infos);
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