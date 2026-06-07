using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Emilia.Kit;

namespace Emilia.Toolbar.Editor
{
    internal static class RecentlyOpenHistoryStore
    {
        private const string SceneHistoryKeyPrefix = "##Emilia.Toolbar.Editor.RecentlyOpen.Scene";
        private const string PrefabHistoryKeyPrefix = "##Emilia.Toolbar.Editor.RecentlyOpen.Prefab";

        public static List<string> LoadScenePaths()
        {
            return LoadPaths(SceneHistoryKeyPrefix);
        }

        public static void SaveScenePaths(List<string> paths)
        {
            SavePaths(SceneHistoryKeyPrefix, paths);
        }

        public static List<string> LoadPrefabPaths()
        {
            return LoadPaths(PrefabHistoryKeyPrefix);
        }

        public static void SavePrefabPaths(List<string> paths)
        {
            SavePaths(PrefabHistoryKeyPrefix, paths);
        }

        private static List<string> LoadPaths(string keyPrefix)
        {
            string key = GetKey(keyPrefix);
            return OdinEditorPrefs.GetValue(key, new List<string>());
        }

        private static void SavePaths(string keyPrefix, List<string> paths)
        {
            string key = GetKey(keyPrefix);
            OdinEditorPrefs.SetValue(key, paths ?? new List<string>());
        }

        private static string GetKey(string keyPrefix)
        {
            string projectPath = EditorAssetKit.dataParentPath;
            string projectHash = GetProjectHash(projectPath);
            return $"{keyPrefix}.{projectHash}";
        }

        private static string GetProjectHash(string value)
        {
            if (string.IsNullOrEmpty(value)) return "empty";

            byte[] bytes = Encoding.UTF8.GetBytes(value);
            byte[] hashBytes;
            using (MD5 md5 = MD5.Create())
            {
                hashBytes = md5.ComputeHash(bytes);
            }

            StringBuilder builder = new StringBuilder(hashBytes.Length * 2);
            for (int i = 0; i < hashBytes.Length; i++)
            {
                builder.Append(hashBytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }
}
