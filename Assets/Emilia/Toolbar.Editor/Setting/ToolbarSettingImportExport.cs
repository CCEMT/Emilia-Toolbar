using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Emilia.Kit;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public static class ToolbarSettingImportExport
    {
        private const int CurrentVersion = 1;
        private const string DefaultFileName = "ToolbarSettings.json";

        [MenuItem("Emilia/Toolbar/Setting/Export All")]
        public static void ExportAll()
        {
            string path = EditorUtility.SaveFilePanel("Export Toolbar Settings", string.Empty, DefaultFileName, "json");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string json = ExportToJson();
                File.WriteAllText(path, json, Encoding.UTF8);
                EditorUtility.DisplayDialog("Export Toolbar Settings", "Export completed.", "OK");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Export Toolbar Settings", e.Message, "OK");
            }
        }

        [MenuItem("Emilia/Toolbar/Setting/Import All")]
        public static void ImportAll()
        {
            string path = EditorUtility.OpenFilePanel("Import Toolbar Settings", string.Empty, "json");
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                ImportFromJson(json);
                EditorUtility.DisplayDialog("Import Toolbar Settings", "Import completed.", "OK");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                EditorUtility.DisplayDialog("Import Toolbar Settings", e.Message, "OK");
            }
        }

        public static string ExportToJson()
        {
            ToolbarSettingExportData data = new ToolbarSettingExportData();
            data.version = CurrentVersion;
            data.exportedAt = DateTime.UtcNow.ToString("O");
            data.consoleSetting = Serialize(ConsoleSetting.instance);
            data.presetCommandSetting = Serialize(PresetCommandSetting.instance);
            data.ringCommandCustomSetting = Serialize(CreateExportCopy(RingCommandCustomSetting.instance));
            data.titleCommandCustomSetting = Serialize(CreateExportCopy(TitleCommandCustomSetting.instance));
            data.ringIconPaths = CollectRingIconPaths(RingCommandCustomSetting.instance);
            data.titleIconPaths = CollectTitleIconPaths(TitleCommandCustomSetting.instance);

            return JsonUtility.ToJson(data, true);
        }

        public static void ImportFromJson(string json)
        {
            ToolbarSettingExportData data = JsonUtility.FromJson<ToolbarSettingExportData>(json);
            Validate(data);

            ConsoleSetting consoleSetting = Deserialize<ConsoleSetting>(data.consoleSetting);
            PresetCommandSetting presetCommandSetting = Deserialize<PresetCommandSetting>(data.presetCommandSetting);
            RingCommandCustomSetting ringCommandCustomSetting = Deserialize<RingCommandCustomSetting>(data.ringCommandCustomSetting);
            TitleCommandCustomSetting titleCommandCustomSetting = Deserialize<TitleCommandCustomSetting>(data.titleCommandCustomSetting);

            RestoreRingIconPaths(ringCommandCustomSetting, data.ringIconPaths);
            RestoreTitleIconPaths(titleCommandCustomSetting, data.titleIconPaths);

            ConsoleSetting previousConsoleSetting = CloneSetting(ConsoleSetting.instance);
            PresetCommandSetting previousPresetCommandSetting = CloneSetting(PresetCommandSetting.instance);
            RingCommandCustomSetting previousRingCommandCustomSetting = CreateRuntimeCopy(RingCommandCustomSetting.instance);
            TitleCommandCustomSetting previousTitleCommandCustomSetting = CreateRuntimeCopy(TitleCommandCustomSetting.instance);

            try
            {
                CopySetting(consoleSetting, ConsoleSetting.instance);
                CopySetting(presetCommandSetting, PresetCommandSetting.instance);
                CopySetting(ringCommandCustomSetting, RingCommandCustomSetting.instance);
                CopySetting(titleCommandCustomSetting, TitleCommandCustomSetting.instance);

                SaveAll();

                CommandCache.instance.ResetCache();
                UnityToolbarUtility.RepaintToolbar();
            }
            catch
            {
                CopySetting(previousConsoleSetting, ConsoleSetting.instance);
                CopySetting(previousPresetCommandSetting, PresetCommandSetting.instance);
                CopySetting(previousRingCommandCustomSetting, RingCommandCustomSetting.instance);
                CopySetting(previousTitleCommandCustomSetting, TitleCommandCustomSetting.instance);
                SaveAll();
                throw;
            }
        }

        private static string Serialize<T>(T setting)
        {
            return OdinSerializableUtility.ToJsonString(setting);
        }

        private static T Deserialize<T>(string json) where T : class, new()
        {
            if (string.IsNullOrEmpty(json)) return new T();
            return OdinSerializableUtility.FromJsonString<T>(json) ?? new T();
        }

        private static void Validate(ToolbarSettingExportData data)
        {
            if (data == null) throw new InvalidDataException("Invalid toolbar settings file.");
            if (data.version != CurrentVersion) throw new InvalidDataException($"Unsupported toolbar settings version: {data.version}.");
            if (string.IsNullOrEmpty(data.consoleSetting)) throw new InvalidDataException("Missing console settings.");
            if (string.IsNullOrEmpty(data.presetCommandSetting)) throw new InvalidDataException("Missing preset command settings.");
            if (string.IsNullOrEmpty(data.ringCommandCustomSetting)) throw new InvalidDataException("Missing ring command settings.");
            if (string.IsNullOrEmpty(data.titleCommandCustomSetting)) throw new InvalidDataException("Missing title command settings.");
        }

        private static void CopySetting<T>(T source, T destination)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (destination == null) throw new ArgumentNullException(nameof(destination));

            foreach (FieldInfo fieldInfo in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                fieldInfo.SetValue(destination, fieldInfo.GetValue(source));
            }
        }

        private static void SaveAll()
        {
            ConsoleSetting.Save();
            PresetCommandSetting.Save();
            RingCommandCustomSetting.Save();
            TitleCommandCustomSetting.Save();
        }

        private static T CloneSetting<T>(T setting)
        {
            string json = OdinSerializableUtility.ToJsonString(setting);
            return OdinSerializableUtility.FromJsonString<T>(json);
        }

        private static RingCommandCustomSetting CreateExportCopy(RingCommandCustomSetting setting)
        {
            RingCommandCustomSetting copy = new RingCommandCustomSetting();
            copy.ringCustomCommandInfos = CopyRingCommandInfos(setting.ringCustomCommandInfos, false);
            copy.leftFixedCustomCommandInfos = CopyFixedCommandInfos(setting.leftFixedCustomCommandInfos, false);
            copy.rightFixedCustomCommandInfos = CopyFixedCommandInfos(setting.rightFixedCustomCommandInfos, false);
            return copy;
        }

        private static TitleCommandCustomSetting CreateExportCopy(TitleCommandCustomSetting setting)
        {
            TitleCommandCustomSetting copy = new TitleCommandCustomSetting();
            copy.customCommands = CopyTitleCommandInfos(setting.customCommands, false);
            return copy;
        }

        private static RingCommandCustomSetting CreateRuntimeCopy(RingCommandCustomSetting setting)
        {
            RingCommandCustomSetting copy = new RingCommandCustomSetting();
            copy.ringCustomCommandInfos = CopyRingCommandInfos(setting.ringCustomCommandInfos, true);
            copy.leftFixedCustomCommandInfos = CopyFixedCommandInfos(setting.leftFixedCustomCommandInfos, true);
            copy.rightFixedCustomCommandInfos = CopyFixedCommandInfos(setting.rightFixedCustomCommandInfos, true);
            return copy;
        }

        private static TitleCommandCustomSetting CreateRuntimeCopy(TitleCommandCustomSetting setting)
        {
            TitleCommandCustomSetting copy = new TitleCommandCustomSetting();
            copy.customCommands = CopyTitleCommandInfos(setting.customCommands, true);
            return copy;
        }

        private static List<RingCustomCommandInfo> CopyRingCommandInfos(List<RingCustomCommandInfo> infos, bool includeIcon)
        {
            if (infos == null) return null;

            List<RingCustomCommandInfo> copy = new List<RingCustomCommandInfo>();
            for (int i = 0; i < infos.Count; i++)
            {
                RingCustomCommandInfo info = infos[i];
                if (info == null)
                {
                    copy.Add(null);
                    continue;
                }

                copy.Add(new RingCustomCommandInfo {
                    name = info.name,
                    description = info.description,
                    icon = includeIcon ? info.icon : null,
                    sdfIcon = info.sdfIcon,
                    color = info.color,
                    commandName = info.commandName
                });
            }

            return copy;
        }

        private static List<FixedCustomCommandInfo> CopyFixedCommandInfos(List<FixedCustomCommandInfo> infos, bool includeIcon)
        {
            if (infos == null) return null;

            List<FixedCustomCommandInfo> copy = new List<FixedCustomCommandInfo>();
            for (int i = 0; i < infos.Count; i++)
            {
                FixedCustomCommandInfo info = infos[i];
                if (info == null)
                {
                    copy.Add(null);
                    continue;
                }

                copy.Add(new FixedCustomCommandInfo {
                    name = info.name,
                    icon = includeIcon ? info.icon : null,
                    sdfIcon = info.sdfIcon,
                    color = info.color,
                    commandName = info.commandName
                });
            }

            return copy;
        }

        private static List<TitleCustomCommandInfo> CopyTitleCommandInfos(List<TitleCustomCommandInfo> infos, bool includeIcon)
        {
            if (infos == null) return null;

            List<TitleCustomCommandInfo> copy = new List<TitleCustomCommandInfo>();
            for (int i = 0; i < infos.Count; i++)
            {
                TitleCustomCommandInfo info = infos[i];
                if (info == null)
                {
                    copy.Add(null);
                    continue;
                }

                copy.Add(new TitleCustomCommandInfo {
                    positionType = info.positionType,
                    color = info.color,
                    icon = includeIcon ? info.icon : null,
                    sdfIcon = info.sdfIcon,
                    text = info.text,
                    priority = info.priority,
                    commandName = info.commandName
                });
            }

            return copy;
        }

        private static RingIconPathData CollectRingIconPaths(RingCommandCustomSetting setting)
        {
            RingIconPathData data = new RingIconPathData();
            data.ringCustomCommandIconPaths = CollectIconPaths(setting.ringCustomCommandInfos);
            data.leftFixedCustomCommandIconPaths = CollectIconPaths(setting.leftFixedCustomCommandInfos);
            data.rightFixedCustomCommandIconPaths = CollectIconPaths(setting.rightFixedCustomCommandInfos);
            return data;
        }

        private static TitleIconPathData CollectTitleIconPaths(TitleCommandCustomSetting setting)
        {
            TitleIconPathData data = new TitleIconPathData();
            data.customCommandIconPaths = CollectIconPaths(setting.customCommands);
            return data;
        }

        private static List<string> CollectIconPaths(List<RingCustomCommandInfo> infos)
        {
            List<string> paths = new List<string>();
            if (infos == null) return paths;

            for (int i = 0; i < infos.Count; i++)
            {
                paths.Add(GetAssetPath(infos[i]?.icon));
            }

            return paths;
        }

        private static List<string> CollectIconPaths(List<FixedCustomCommandInfo> infos)
        {
            List<string> paths = new List<string>();
            if (infos == null) return paths;

            for (int i = 0; i < infos.Count; i++)
            {
                paths.Add(GetAssetPath(infos[i]?.icon));
            }

            return paths;
        }

        private static List<string> CollectIconPaths(List<TitleCustomCommandInfo> infos)
        {
            List<string> paths = new List<string>();
            if (infos == null) return paths;

            for (int i = 0; i < infos.Count; i++)
            {
                paths.Add(GetAssetPath(infos[i]?.icon));
            }

            return paths;
        }

        private static string GetAssetPath(Texture icon)
        {
            return icon == null ? string.Empty : AssetDatabase.GetAssetPath(icon);
        }

        private static void RestoreRingIconPaths(RingCommandCustomSetting setting, RingIconPathData data)
        {
            if (setting == null || data == null) return;
            RestoreIconPaths(setting.ringCustomCommandInfos, data.ringCustomCommandIconPaths);
            RestoreIconPaths(setting.leftFixedCustomCommandInfos, data.leftFixedCustomCommandIconPaths);
            RestoreIconPaths(setting.rightFixedCustomCommandInfos, data.rightFixedCustomCommandIconPaths);
        }

        private static void RestoreTitleIconPaths(TitleCommandCustomSetting setting, TitleIconPathData data)
        {
            if (setting == null || data == null) return;
            RestoreIconPaths(setting.customCommands, data.customCommandIconPaths);
        }

        private static void RestoreIconPaths(List<RingCustomCommandInfo> infos, List<string> paths)
        {
            if (infos == null) return;

            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i] == null) continue;
                infos[i].icon = LoadIcon(paths, i);
            }
        }

        private static void RestoreIconPaths(List<FixedCustomCommandInfo> infos, List<string> paths)
        {
            if (infos == null) return;

            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i] == null) continue;
                infos[i].icon = LoadIcon(paths, i);
            }
        }

        private static void RestoreIconPaths(List<TitleCustomCommandInfo> infos, List<string> paths)
        {
            if (infos == null) return;

            for (int i = 0; i < infos.Count; i++)
            {
                if (infos[i] == null) continue;
                infos[i].icon = LoadIcon(paths, i);
            }
        }

        private static Texture LoadIcon(List<string> paths, int index)
        {
            if (paths == null || index < 0 || index >= paths.Count) return null;
            string path = paths[index];
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Texture>(path);
        }

        [Serializable]
        private class ToolbarSettingExportData
        {
            public int version;
            public string exportedAt;
            public string consoleSetting;
            public string presetCommandSetting;
            public string ringCommandCustomSetting;
            public string titleCommandCustomSetting;
            public RingIconPathData ringIconPaths;
            public TitleIconPathData titleIconPaths;
        }

        [Serializable]
        private class RingIconPathData
        {
            public List<string> ringCustomCommandIconPaths = new List<string>();
            public List<string> leftFixedCustomCommandIconPaths = new List<string>();
            public List<string> rightFixedCustomCommandIconPaths = new List<string>();
        }

        [Serializable]
        private class TitleIconPathData
        {
            public List<string> customCommandIconPaths = new List<string>();
        }
    }
}
