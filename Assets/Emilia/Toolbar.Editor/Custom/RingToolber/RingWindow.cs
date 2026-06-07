using System;
using System.Collections.Generic;
using Emilia.Kit;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class RingWindow : TransparentEditorWindow
    {
        private const float RingWidth = 500f;
        private const float RingHeight = 500f;
        private const float RingMinValidityDistanceR = 125f;
        private const float RingMaxValidityDistanceR = 350f;
        private const float RingInnerRadius = 0.5f;
        private const float RingOuterRadius = 1f;

        private const float SettingButtonWidth = 200f;
        private const float SettingButtonHeight = 50f;

        private const float FixedWidth = 200f;
        private const float FixedItemHeight = 30f;
        private const float FixedLeftPadding = 100f;
        private const float IconSize = 20f;
        private const float RingIconSize = 24f;

        public static RingWindow window;
        
        private static GUIStyle itemStyle;
        private static GUIStyle descriptionStyle;
        private static GUIStyle fixedStyle;

        private int selectedIndex;

        private Vector2 windowCenter;
        private bool isWindowCenterCached;

        public static void OpenWindow()
        {
            if (! window) window = CreateInstance<RingWindow>();
            window.OpenInPopup();
        }

        public static void CloseWindow(bool executeRingAction = true)
        {
            if (window)
            {
                if (executeRingAction) window.ExecuteRingAction();
                window.Close();
            }

            window = null;
        }

        private void ExecuteRingAction()
        {
            if (selectedIndex == -1 || RingCommandCustomSetting.instance.ringCustomCommandInfos?.Count == 0) return;

            try
            {
                RingCustomCommandInfo info = RingCommandCustomSetting.instance.ringCustomCommandInfos[selectedIndex];
                if (info == null) return;

                CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(info.commandName);
                if (commandInfo == null) return;

                if (commandInfo.actionInfo.methodInfo.GetParameters().Length == 0) commandInfo.ExecuteCommand();
                else ArgCommandExecuteWindow.Open(commandInfo.name);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToUnityLogString());
            }
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
            Repaint();
            
            InitializeStyle();
            OnRingToolbarGUI();
            OnSettingButton();
            OnFixedGUI();

            Event evt = Event.current;

            bool canDisable = evt.alt == false && evt.keyCode != KeyCode.BackQuote && evt.type != EventType.KeyDown;
            if (canDisable)
            {
                rootVisualElement.schedule.Execute(() => CloseWindow(false)).ExecuteLater(1);
            }
        }

        private void InitializeStyle()
        {
            if (itemStyle == null)
            {
                itemStyle = new GUIStyle(GUI.skin.label);
                itemStyle.alignment = TextAnchor.MiddleCenter;
                itemStyle.normal.textColor = Color.white;
                itemStyle.fontSize = 16;
                itemStyle.wordWrap = true;
            }

            if (descriptionStyle == null)
            {
                descriptionStyle = new GUIStyle(GUI.skin.label);
                descriptionStyle.alignment = TextAnchor.MiddleCenter;
                descriptionStyle.normal.textColor = Color.white;
                descriptionStyle.fontSize = 16;
                descriptionStyle.wordWrap = true;
            }

            if (fixedStyle == null)
            {
                fixedStyle = new GUIStyle(GUI.skin.label);
                fixedStyle.alignment = TextAnchor.MiddleLeft;
                fixedStyle.normal.textColor = Color.white;
                fixedStyle.fontSize = 16;
            }
        }

        private void OnRingToolbarGUI()
        {
            Event evt = Event.current;
            var ringCommands = RingCommandCustomSetting.instance.ringCustomCommandInfos;
            int count = ringCommands.Count;

            if (count == 0) return;

            Rect rect = GetCenteredRect(RingWidth, RingHeight);
            Vector2 center = GetWindowCenter();

            selectedIndex = CalculateSelectedIndex(evt.mousePosition, center, count);

            DrawRingElements(rect, count, selectedIndex);
            HandleRingInteraction(evt, count);
        }

        private Rect GetCenteredRect(float width, float height) => new(position.width / 2 - width / 2, position.height / 2 - height / 2, width, height);

        private Vector2 GetWindowCenter()
        {
            if (! isWindowCenterCached)
            {
                windowCenter = new Vector2(position.width / 2, position.height / 2);
                isWindowCenterCached = true;
            }
            return windowCenter;
        }

        private int CalculateSelectedIndex(Vector2 mousePosition, Vector2 center, int count)
        {
            Vector2 direction = mousePosition - center;
            float distance = direction.magnitude;

            if (distance <= RingMinValidityDistanceR || distance >= RingMaxValidityDistanceR) return -1;

            float angle = Vector2.SignedAngle(Vector2.right, direction);
            if (angle < 0) angle += 360f;

            float itemAngle = 360f / count;
            float offset = -(itemAngle / 2f) - 90f;
            float adjustedAngle = (angle - offset) % 360f;
            if (adjustedAngle < 0) adjustedAngle += 360f;

            return Mathf.FloorToInt(adjustedAngle / itemAngle);
        }

        private void DrawRingElements(Rect rect, int count, int selectedIndex)
        {
            Color[] colors = CreateRingColors(count, selectedIndex);
            float itemAngle = 360f / count;
            float offset = -(itemAngle / 2f) - 90f;

            Color centerColor = Color.black;
            centerColor.a = count > 0 ? 0.2f : 0;

            RingGUIDrawer.DrawRing(rect, RingInnerRadius, RingOuterRadius, count, colors, offset, centerColor);
            RingGUIDrawer.DrawCustomGUI(rect, RingInnerRadius, RingOuterRadius, count, DrawCustomGUI, offset);
        }

        private Color[] CreateRingColors(int count, int selectedIndex)
        {
            Color[] colors = new Color[count];
            for (int i = 0; i < count; i++)
            {
                Color color = Color.black;
                color.a = i == selectedIndex ? 1f : 0.2f;
                colors[i] = color;
            }
            return colors;
        }

        private void HandleRingInteraction(Event evt, int count)
        {
            if (selectedIndex == -1 || count == 0) return;

            var ringCommands = RingCommandCustomSetting.instance.ringCustomCommandInfos;
            RingCustomCommandInfo info = ringCommands[selectedIndex];
            if (info == null) return;

            DrawTitle(info);
            DrawCenterDescription(info);

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                CloseWindow();
                evt.Use();
            }
        }

        private void DrawTitle(RingCustomCommandInfo info)
        {
            Vector2 center = GetWindowCenter();
            float size = RingMinValidityDistanceR * 2;
            Vector2 titlePosition = center - Vector2.one * RingMinValidityDistanceR;
            Rect titleRect = new Rect(titlePosition, Vector2.one * size);
            titleRect.y += -RingMaxValidityDistanceR - 50;

            GUI.Label(titleRect, info.commandName, descriptionStyle);
        }

        private void DrawCenterDescription(RingCustomCommandInfo info)
        {
            Vector2 center = GetWindowCenter();
            float size = RingMinValidityDistanceR * 2;
            Vector2 descriptionPosition = center - Vector2.one * RingMinValidityDistanceR;
            Rect descriptionRect = new Rect(descriptionPosition, Vector2.one * size);
            GUI.Label(descriptionRect, info.description, descriptionStyle);
        }

        private void DrawCustomGUI(int index, Rect rect)
        {
            if (index < 0 || index >= RingCommandCustomSetting.instance.ringCustomCommandInfos.Count) return;

            RingCustomCommandInfo info = RingCommandCustomSetting.instance.ringCustomCommandInfos[index];
            if (info == null) return;

            Color oldColor = GUI.color;
            GUI.color = info.color;

            float textWidth = itemStyle.CalcSize(new GUIContent(info.name)).x;
            Rect adjustedRect = new Rect(rect);

            if (textWidth > rect.width)
            {
                adjustedRect.width = rect.width * 1.2f;
                adjustedRect.height = rect.height * 1.2f;
                adjustedRect.center = rect.center;
            }

            if (info.icon)
            {
                Rect iconRect = new Rect(adjustedRect);
                iconRect.width = RingIconSize;
                iconRect.height = RingIconSize;
                iconRect.center = new Vector2(adjustedRect.center.x, adjustedRect.center.y - 15);

                GUI.DrawTexture(iconRect, info.icon);
                adjustedRect.y += 15;
            }

            GUI.Label(adjustedRect, info.name, itemStyle);
            GUI.color = oldColor;
        }

        private void OnSettingButton()
        {
            Rect rect = new Rect(position.width / 2 - SettingButtonWidth / 2, position.height / 2 - SettingButtonHeight / 2, SettingButtonWidth, SettingButtonHeight);
            rect.y += RingMaxValidityDistanceR + SettingButtonHeight;

            if (GUI.Button(rect, "设置"))
            {
                RingCustomCommandWindow.Open();
                CloseWindow();
            }
        }

        private void OnFixedGUI()
        {
            DrawFixedPanel(RingCommandCustomSetting.instance.leftFixedCustomCommandInfos, true);
            DrawFixedPanel(RingCommandCustomSetting.instance.rightFixedCustomCommandInfos, false);
        }

        private void DrawFixedPanel(List<FixedCustomCommandInfo> commandInfos, bool isLeft)
        {
            if (commandInfos == null || commandInfos.Count == 0) return;

            Rect rect = CalculateFixedPanelRect(isLeft);

            for (int i = 0; i < commandInfos.Count; i++)
            {
                FixedCustomCommandInfo info = commandInfos[i];
                if (info == null) continue;

                DrawFixedItem(rect, i, info);
            }
        }

        private Rect CalculateFixedPanelRect(bool isLeft)
        {
            Rect rect = new Rect(position.width / 2, FixedLeftPadding, FixedWidth, position.height - FixedLeftPadding * 2);
            rect.x += isLeft ? -RingMaxValidityDistanceR - FixedWidth : RingMaxValidityDistanceR;
            return rect;
        }

        private void DrawFixedItem(Rect panelRect, int index, FixedCustomCommandInfo info)
        {
            Rect itemRect = new Rect(panelRect.x, panelRect.y + index * FixedItemHeight, panelRect.width, FixedItemHeight);
            bool isMouseOver = itemRect.Contains(Event.current.mousePosition);

            GUI.color = info.color;
            DrawItemBackground(itemRect, isMouseOver);
            DrawFixedItemIcon(itemRect, info);
            DrawFixedItemLabel(itemRect, info);
            GUI.color = Color.white;

            HandleFixedItemClick(isMouseOver, info.commandName);
        }

        private void DrawItemBackground(Rect rect, bool isMouseOver)
        {
            if (isMouseOver)
            {
                GUI.color = new Color(1, 1, 1, 0.1f);
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.5f);
                GUI.color = Color.white;
            }
        }

        private void DrawFixedItemIcon(Rect itemRect, FixedCustomCommandInfo info)
        {
            Rect iconRect = new Rect(itemRect.x, itemRect.y, IconSize, FixedItemHeight);

            if (info.icon != null)
            {
                GUI.DrawTexture(iconRect, info.icon);
            }
            else if (info.sdfIcon != SdfIconType.None)
            {
                SdfIcons.DrawIcon(iconRect, info.sdfIcon);
            }
        }

        private void DrawFixedItemLabel(Rect itemRect, FixedCustomCommandInfo info)
        {
            Rect labelRect = new Rect(itemRect.x + IconSize, itemRect.y, itemRect.width - IconSize, FixedItemHeight);
            GUI.Label(labelRect, new GUIContent(info.name, info.commandName), fixedStyle);
        }

        private void HandleFixedItemClick(bool isMouseOver, string commandName)
        {
            bool isExecute = isMouseOver && Event.current.type == EventType.MouseDown && Event.current.button == 0;
            if (isExecute == false) return;

            ExecuteCommand(commandName);
            Event.current.Use();
            Close();
        }

        void ExecuteCommand(string commandName)
        {
            CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(commandName);
            if (commandInfo == null) return;

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