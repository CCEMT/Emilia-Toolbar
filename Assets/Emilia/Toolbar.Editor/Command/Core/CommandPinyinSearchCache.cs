using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Emilia.Kit;
using FuzzySharp;
using UnityEditor;

[assembly: InternalsVisibleTo("Emilia.Toolbar.Editor.Tests")]

namespace Emilia.Toolbar.Editor
{
    internal static class CommandPinyinSearchCache
    {
        private const int CacheVersion = 1;
        private const string CacheKeyPrefix = "##Emilia.Toolbar.Editor.CommandPinyinSearchCache";

        internal const int MaxSearchScore = 100;
        internal const int MinSearchScore = 0;

        internal static Func<string, bool> ContainsChineseResolver = PinYinConverterUtility.ContainsChinese;
        internal static Func<string, string> ConvertToAllSpellResolver = PinYinConverterUtility.ConvertToAllSpell;
        internal static string prefsKeyOverrideForTests;

        private static bool isLoaded;
        private static bool isDirty;
        private static bool isFlushScheduled;

        private static CacheData cacheData;
        private static Dictionary<string, CacheEntry> entryByCommandName;

        static CommandPinyinSearchCache()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Flush;
            EditorApplication.quitting += Flush;
        }

        public static bool ContainsChinese(string commandName)
        {
            if (string.IsNullOrEmpty(commandName)) return false;

            CacheEntry entry = GetOrCreateEntry(commandName);
            if (entry.hasContainsChinese) return entry.containsChinese;

            Func<string, bool> resolver = ContainsChineseResolver ?? PinYinConverterUtility.ContainsChinese;
            bool result = resolver(commandName);

            entry.hasContainsChinese = true;
            entry.containsChinese = result;
            MarkDirty();

            return result;
        }

        public static string ConvertToAllSpell(string commandName)
        {
            if (string.IsNullOrEmpty(commandName)) return string.Empty;

            CacheEntry entry = GetOrCreateEntry(commandName);
            if (entry.hasPinyin) return entry.pinyin ?? string.Empty;

            Func<string, string> resolver = ConvertToAllSpellResolver ?? PinYinConverterUtility.ConvertToAllSpell;
            string result = resolver(commandName) ?? string.Empty;

            entry.hasPinyin = true;
            entry.pinyin = result;
            MarkDirty();

            return result;
        }

        public static int SmartSearch(string target, string input, bool inputNullResult = true, bool ignoreCase = true, float pinYinWeight = 0.6f)
        {
            if (string.IsNullOrEmpty(input)) return inputNullResult ? MaxSearchScore : MinSearchScore;
            if (string.IsNullOrEmpty(target)) return MinSearchScore;

            string searchTarget = ignoreCase ? target.ToLower() : target;
            string searchInput = ignoreCase ? input.ToLower() : input;

            int directScore = Fuzz.WeightedRatio(searchInput, searchTarget);
            if (pinYinWeight <= 0) return directScore;

            string pinYin = ConvertToAllSpell(searchTarget);
            int pinYinScore = Fuzz.WeightedRatio(searchInput, pinYin);
            pinYinScore = (int) (pinYinScore * pinYinWeight);

            return directScore > pinYinScore ? directScore : pinYinScore;
        }

        internal static void ResetForTests(bool clearPersistentCache)
        {
            if (isFlushScheduled) EditorApplication.delayCall -= Flush;

            isLoaded = false;
            isDirty = false;
            isFlushScheduled = false;
            cacheData = null;
            entryByCommandName = null;

            ContainsChineseResolver = PinYinConverterUtility.ContainsChinese;
            ConvertToAllSpellResolver = PinYinConverterUtility.ConvertToAllSpell;

            if (clearPersistentCache) EditorPrefs.DeleteKey(GetCacheKey());
        }

        internal static void FlushForTests()
        {
            if (isFlushScheduled) EditorApplication.delayCall -= Flush;
            isFlushScheduled = false;
            Flush();
        }

        private static CacheEntry GetOrCreateEntry(string commandName)
        {
            EnsureLoaded();

            if (entryByCommandName.TryGetValue(commandName, out CacheEntry entry)) return entry;

            entry = new CacheEntry { commandName = commandName };
            cacheData.entries.Add(entry);
            entryByCommandName[commandName] = entry;

            return entry;
        }

        private static void EnsureLoaded()
        {
            if (isLoaded) return;

            cacheData = LoadData();
            entryByCommandName = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < cacheData.entries.Count; i++)
            {
                CacheEntry entry = cacheData.entries[i];
                if (entry == null || string.IsNullOrEmpty(entry.commandName)) continue;
                if (entryByCommandName.ContainsKey(entry.commandName)) continue;

                entryByCommandName.Add(entry.commandName, entry);
            }

            isLoaded = true;
        }

        private static CacheData LoadData()
        {
            string key = GetCacheKey();

            try
            {
                if (OdinEditorPrefs.HasValue(key) == false) return CreateEmptyData();

                CacheData loadedData = OdinEditorPrefs.GetValue<CacheData>(key);
                if (loadedData == null || loadedData.version != CacheVersion || loadedData.entries == null)
                {
                    EditorPrefs.DeleteKey(key);
                    return CreateEmptyData();
                }

                return loadedData;
            }
            catch
            {
                EditorPrefs.DeleteKey(key);
                return CreateEmptyData();
            }
        }

        private static CacheData CreateEmptyData()
        {
            return new CacheData
            {
                version = CacheVersion,
                entries = new List<CacheEntry>()
            };
        }

        private static void MarkDirty()
        {
            isDirty = true;
            if (isFlushScheduled) return;

            isFlushScheduled = true;
            EditorApplication.delayCall += Flush;
        }

        private static void Flush()
        {
            isFlushScheduled = false;
            if (isLoaded == false || isDirty == false || cacheData == null) return;

            cacheData.version = CacheVersion;
            OdinEditorPrefs.SetValue(GetCacheKey(), cacheData);
            isDirty = false;
        }

        private static string GetCacheKey()
        {
            if (string.IsNullOrEmpty(prefsKeyOverrideForTests) == false) return prefsKeyOverrideForTests;

            string projectPath = EditorAssetKit.dataParentPath;
            string projectHash = GetProjectHash(projectPath);
            return $"{CacheKeyPrefix}.{projectHash}";
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

        [Serializable]
        private class CacheData
        {
            public int version = CacheVersion;
            public List<CacheEntry> entries = new List<CacheEntry>();
        }

        [Serializable]
        private class CacheEntry
        {
            public string commandName;
            public bool hasContainsChinese;
            public bool containsChinese;
            public bool hasPinyin;
            public string pinyin;
        }
    }
}
