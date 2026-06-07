using System;
using System.Collections.Generic;
using System.Reflection;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class PresetCommandWindow : OdinEditorWindow
    {
        [MenuItem("Emilia/Toolbar/Setting/Preset")]
        public static void Open()
        {
            EditorImGUIKit.OpenWindow<PresetCommandWindow>("Preset Command", 600, 600);
        }

        [LabelText("预设命令列表"), ListDrawerSettings(CustomAddFunction = nameof(Add)), HideReferenceObjectPicker, NonSerialized, OdinSerialize]
        public List<PresetCommandInfoItem> items = new List<PresetCommandInfoItem>();

        private void Add()
        {
            PresetCommandInfoItem item = new PresetCommandInfoItem();
            item.path = "NewCommand";
            items.Add(item);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (items == null) items = new List<PresetCommandInfoItem>();
            else items.Clear();

            if (PresetCommandSetting.instance.presetCommandInfos == null) PresetCommandSetting.instance.presetCommandInfos = new List<PresetCommandInfo>();

            foreach (PresetCommandInfo info in PresetCommandSetting.instance.presetCommandInfos)
            {
                PresetCommandInfoItem item = new PresetCommandInfoItem();
                item.path = string.IsNullOrEmpty(info.category) ? info.alias : $"{info.category}/{info.alias}";
                item.description = info.description;
                item.commandName = info.commandName;
                item.args = info.args;
                item.Initialize();
                items.Add(item);
            }
        }

        [Button("保存", ButtonSizes.Large), GUIColor(0, 1, 0), PropertyOrder(-1)]
        public void Save()
        {
            PresetCommandSetting.instance.presetCommandInfos.Clear();

            for (var i = 0; i < this.items.Count; i++)
            {
                PresetCommandInfoItem item = this.items[i];

                PresetCommandInfo info = new PresetCommandInfo();
                EditorKit.PathToNameAndCategory(item.path, out info.alias, out info.category);
                info.description = item.description;
                info.commandName = item.commandName;
                info.args = item.args;

                PresetCommandSetting.instance.presetCommandInfos.Add(info);
            }

            PresetCommandSetting.Save();
            CommandCache.instance.ResetCache();
        }

        [Serializable]
        public class PresetCommandInfoItem
        {
            [LabelText("名称")]
            public string path;

            [LabelText("描述")]
            public string description;

            [LabelText("命令"), OnValueChanged(nameof(OnCommandChange)), ValueDropdown(nameof(GetCommandNames))]
            public string commandName;

            [LabelText("参数列表"), ShowIf("GetCommandVisible"), NonSerialized, OdinSerialize, HideReferenceObjectPicker,
             ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false,
                 OnBeginListElementGUI = nameof(OnBeginListElementGUI), OnEndListElementGUI = nameof(OnEndListElementGUI))]
            public object[] args;

            [SerializeField, HideInInspector]
            public string[] argLabels;

            private bool GetCommandVisible()
            {
                if (string.IsNullOrEmpty(this.commandName)) return false;
                return CommandCache.instance.commandInfoByName.GetValueOrDefault(commandName) != null;
            }

            public void Initialize()
            {
                CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(commandName);
                if (commandInfo?.actionInfo?.methodInfo == null) return;

                argLabels = ExtractParameterLabels(commandInfo.actionInfo.methodInfo);
            }

            private void OnCommandChange()
            {
                args = null;
                if (string.IsNullOrEmpty(commandName)) return;

                CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(commandName);
                if (commandInfo?.actionInfo?.methodInfo == null) return;

                var parameterData = ProcessMethodParameters(commandInfo.actionInfo.methodInfo);
                args = parameterData.args;
                argLabels = parameterData.labels;
            }

            private (object[] args, string[] labels) ProcessMethodParameters(MethodInfo methodInfo)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                var collectArgs = new List<object>(parameters.Length);
                var collectArgLabels = new List<string>(parameters.Length);

                foreach (ParameterInfo parameter in parameters)
                {
                    object defaultValue = GetParameterDefaultValue(parameter);
                    string label = GetParameterLabel(parameter);

                    collectArgs.Add(defaultValue);
                    collectArgLabels.Add(label);
                }

                return (collectArgs.ToArray(), collectArgLabels.ToArray());
            }

            private static object GetParameterDefaultValue(ParameterInfo parameter)
            {
                Type parameterType = parameter.ParameterType;

                if (parameter.HasDefaultValue) return parameter.DefaultValue;
                if (parameterType.IsValueType) return Activator.CreateInstance(parameterType);
                if (parameterType == typeof(string)) return string.Empty;
                if (parameterType.GetConstructor(Type.EmptyTypes) != null) return Activator.CreateInstance(parameterType);

                return null;
            }

            private static string[] ExtractParameterLabels(MethodInfo methodInfo)
            {
                ParameterInfo[] parameters = methodInfo.GetParameters();
                var labels = new string[parameters.Length];

                for (int i = 0; i < parameters.Length; i++)
                {
                    labels[i] = GetParameterLabel(parameters[i]);
                }

                return labels;
            }

            private static string GetParameterLabel(ParameterInfo parameter)
            {
                TextAttribute textAttribute = parameter.GetCustomAttribute<TextAttribute>();
                return textAttribute?.text ?? string.Empty;
            }

            private void OnBeginListElementGUI(int index)
            {
                GUILayout.BeginHorizontal();
                string label = index < argLabels?.Length ? argLabels[index] : $"Arg {index + 1}";
                GUILayout.Label(label);
            }

            private void OnEndListElementGUI()
            {
                GUILayout.EndHorizontal();
            }

            private ValueDropdownList<string> GetCommandNames()
            {
                ValueDropdownList<string> list = new ValueDropdownList<string>();

                foreach (CommandInfo command in CommandCache.instance.commandInfos)
                {
                    string itemText = $"{command.alias}|{command.name}|{command.description}";
                    list.Add(itemText, command.name);
                }

                return list;
            }
        }
    }
}