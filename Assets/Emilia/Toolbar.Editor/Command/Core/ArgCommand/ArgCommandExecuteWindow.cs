using System;
using System.Collections.Generic;
using System.Reflection;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class ArgCommandExecuteWindow : OdinEditorWindow
    {
        public static void Open(string commandName)
        {
            ArgCommandExecuteWindow window = EditorImGUIKit.OpenWindow<ArgCommandExecuteWindow>("Arg Command", 600, 600);
            window.commandName = commandName;
        }

        [SerializeField, HideInInspector]
        private string commandName;

        [NonSerialized]
        private CommandInfo commandInfo;

        [LabelText("参数列表"), HideReferenceObjectPicker, NonSerialized, OdinSerialize,
         ListDrawerSettings(HideAddButton = true, HideRemoveButton = true, DraggableItems = false,
             OnBeginListElementGUI = nameof(OnBeginListElementGUI), OnEndListElementGUI = nameof(OnEndListElementGUI))]
        public object[] args;

        [SerializeField, HideInInspector]
        public string[] argLabels;

        [Button("执行", ButtonSizes.Large), GUIColor(0, 1, 0), PropertyOrder(-1)]
        public void OnExecute()
        {
            this.commandInfo.ExecuteCommand(this.args);
            Close();
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

        protected override void OnImGUI()
        {
            base.OnImGUI();

            if (this.commandInfo == null)
            {
                this.commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(this.commandName);
                if (this.commandInfo == null)
                {
                    Close();
                    return;
                }

                InitializeParameters();
            }
        }

        private void InitializeParameters()
        {
            MethodInfo methodInfo = commandInfo.actionInfo.methodInfo;
            if (methodInfo == null) return;

            ParameterInfo[] parameters = methodInfo.GetParameters();
            var collectArgs = new List<object>(parameters.Length);
            var collectArgLabels = new List<string>(parameters.Length);

            for (var i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                object defaultValue = GetParameterDefaultValue(parameter);
                string label = GetParameterLabel(parameter);

                collectArgs.Add(defaultValue);
                collectArgLabels.Add(label);
            }

            args = collectArgs.ToArray();
            argLabels = collectArgLabels.ToArray();
        }

        private static object GetParameterDefaultValue(ParameterInfo parameter)
        {
            if (parameter.HasDefaultValue) return parameter.DefaultValue;

            Type parameterType = parameter.ParameterType;

            if (parameterType == typeof(string)) return string.Empty;
            if (parameterType.IsValueType || parameterType.GetConstructor(Type.EmptyTypes) != null) return Activator.CreateInstance(parameterType);

            return null;
        }

        private static string GetParameterLabel(ParameterInfo parameter)
        {
            TextAttribute textAttribute = parameter.GetCustomAttribute<TextAttribute>();
            return textAttribute?.text ?? string.Empty;
        }
    }
}