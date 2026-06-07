using System;
using System.Collections.Generic;
using Emilia.Kit;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [InitializeOnLoad]
    public class TitleCustomCommandUtility
    {
        static TitleCustomCommandUtility()
        {
            UnityToolbarUtility.onToolbarGUILeft += OnToolbarGUILeft;
            UnityToolbarUtility.onToolbarGUIRight += OnToolbarGUIRight;
        }

        private static void OnToolbarGUILeft()
        {
            RenderToolbarSection(TitleCustomCommandInfo.TitlePositionType.LeftLeft, TitleCustomCommandInfo.TitlePositionType.LeftRight);
        }

        private static void OnToolbarGUIRight()
        {
            RenderToolbarSection(TitleCustomCommandInfo.TitlePositionType.RightLeft, TitleCustomCommandInfo.TitlePositionType.RightRight);
        }

        private static void RenderToolbarSection(TitleCustomCommandInfo.TitlePositionType leftType, TitleCustomCommandInfo.TitlePositionType rightType)
        {
            TitleCommandCustomSetting setting = TitleCommandCustomSetting.instance;
            if (setting.customCommands == null) return;

            List<TitleCustomCommandInfo> leftInfos = new List<TitleCustomCommandInfo>();
            List<TitleCustomCommandInfo> rightInfos = new List<TitleCustomCommandInfo>();

            for (var i = 0; i < setting.customCommands.Count; i++)
            {
                TitleCustomCommandInfo info = setting.customCommands[i];
                if (info.positionType == leftType) leftInfos.Add(info);
                else if (info.positionType == rightType) rightInfos.Add(info);
            }

            GUILayout.BeginHorizontal();

            foreach (TitleCustomCommandInfo info in leftInfos) OnTitleButtonGUI(info);

            GUILayout.FlexibleSpace();

            foreach (TitleCustomCommandInfo info in rightInfos) OnTitleButtonGUI(info);

            GUILayout.EndHorizontal();
        }

        private static void OnTitleButtonGUI(TitleCustomCommandInfo info)
        {
            if (string.IsNullOrEmpty(info.commandName)) return;

            CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(info.commandName);
            if (commandInfo == null) return;

            GUI.color = info.color == default ? Color.white : info.color;

            GUIContent content = new GUIContent(info.text, info.commandName);

            Rect rect = GUILayoutUtility.GetRect(content, SirenixGUIStyles.Button);
            if (info.icon != null || info.sdfIcon != SdfIconType.None)
            {
                rect.width += 20;
                GUILayout.Space(20);
            }

            if (info.icon != null)
            {
                if (SirenixEditorGUI.SDFIconButton(rect, content, info.icon)) OnButtonEvent(commandInfo);
            }
            else
            {
                if (SirenixEditorGUI.SDFIconButton(rect, content, info.sdfIcon)) OnButtonEvent(commandInfo);
            }
            
            GUI.color = Color.white;
        }

        static void OnButtonEvent(CommandInfo commandInfo)
        {
            try
            {
                if (commandInfo.actionInfo.methodInfo.GetParameters().Length == 0) commandInfo.ExecuteCommand();
                else ArgCommandExecuteWindow.Open(commandInfo.name);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToUnityLogString());
            }
        }
    }
}