using System;
using System.Collections.Generic;
using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class SwitchWindow : EditorWindow
    {
        private const int WindowWidth = 600;
        private const int WindowHeight = 650;
        private const int TitleHeight = 30;
        private const int GroupTitleHeight = 22;
        private const int MaxItemCount = 30;
        private const int ItemHeight = 20;
        private const int FixedKeyWidth = 25;
        private const int FixedIconWidth = 50;
        private const int SwitchIconWidth = 20;

        public static SwitchWindow window;

        private static GUIStyle titleStyle;
        private static GUIStyle groupTitleStyle;

        public static void OpenWindow(SwitchInfoCollection switchInfoCollection)
        {
            window = CreateInstance<SwitchWindow>();
            window.SetSwitchInfos(switchInfoCollection);
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(WindowWidth, WindowHeight);
            window.ShowPopup();
            window.Focus();
        }

        public static void CloseWindow(bool execute)
        {
            if (window)
            {
                if (execute) window.Execute();
                window.Close();
            }
            window = null;
        }

        private bool isFocus;
        private int selectedIndex;
        private Vector2 switchScrollPosition;
        private bool scrollSelectedSwitchIntoView;
        private string titleText;
        private List<FixedSwitchInfo> fixedSwitchInfos;
        private List<SwitchGroup> switchGroups;
        private List<SwitchInfo> switchInfosInDisplayOrder;

        public void SetSwitchInfos(SwitchInfoCollection switchInfoCollection)
        {
            this.titleText = string.IsNullOrEmpty(switchInfoCollection?.title) ? "Switch" : switchInfoCollection.title;
            this.fixedSwitchInfos = switchInfoCollection?.fixedSwitchInfos ?? new List<FixedSwitchInfo>();
            this.switchGroups = switchInfoCollection?.switchGroups ?? new List<SwitchGroup>();
            this.switchInfosInDisplayOrder = switchInfoCollection?.GetSwitchInfosInDisplayOrder() ?? new List<SwitchInfo>();
            this.selectedIndex = 0;
            this.switchScrollPosition = Vector2.zero;
            this.scrollSelectedSwitchIntoView = true;
        }

        public void Switch()
        {
            if (switchInfosInDisplayOrder.Count == 0) return;
            selectedIndex++;
            if (selectedIndex >= switchInfosInDisplayOrder.Count) selectedIndex = 0;
            scrollSelectedSwitchIntoView = true;
            Repaint();
        }

        public void Execute()
        {
            if (selectedIndex >= 0 && selectedIndex < switchInfosInDisplayOrder.Count) switchInfosInDisplayOrder[selectedIndex].action?.Invoke();
        }

        private void OnGUI()
        {
            Repaint();

            InitializeStyle();

            DrawTitle();
            DrawContent();
            HandleCloseEvent();
        }

        private void InitializeStyle()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label);
                titleStyle.alignment = TextAnchor.MiddleCenter;
                titleStyle.fontSize = 16;
            }

            if (groupTitleStyle == null)
            {
                groupTitleStyle = new GUIStyle(GUI.skin.label);
                groupTitleStyle.alignment = TextAnchor.MiddleLeft;
                groupTitleStyle.fontSize = 12;
                groupTitleStyle.normal.textColor = new Color(1, 1, 1, 0.7f);
                groupTitleStyle.padding = new RectOffset(5, 0, 0, 0);
            }
        }

        private void DrawTitle()
        {
            Rect rect = GUILayoutUtility.GetRect(1, TitleHeight, GUILayout.ExpandHeight(true));

            GUI.color = new Color(0.2f, 0.2f, 0.2f);
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.5f);
            GUI.color = Color.white;

            GUI.Label(rect, titleText, titleStyle);
        }

        private void DrawContent()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
            DrawFixedSwitchItems();
            GUILayout.EndVertical();

            Rect lineRect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandHeight(true));
            GUI.color = new Color(1, 1, 1, 0.2f);
            GUI.DrawTexture(lineRect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.5f);
            GUI.color = Color.white;

            GUILayout.BeginVertical(GUILayout.Width(position.width * 0.5f));
            DrawSwitchItems();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void HandleCloseEvent()
        {
            Event evt = Event.current;
            bool canDisable = ! evt.control && evt.keyCode != KeyCode.Tab && evt.type != EventType.KeyDown;
            if (canDisable)
            {
                rootVisualElement.schedule.Execute(() => CloseWindow(false)).ExecuteLater(1);
            }

            if (this != focusedWindow) CloseWindow(false);
        }

        private void DrawFixedSwitchItems()
        {
            if (fixedSwitchInfos.Count == 0)
            {
                GUILayout.FlexibleSpace();
                return;
            }

            Event evt = Event.current;
            int itemCount = Mathf.Min(fixedSwitchInfos.Count, MaxItemCount);

            for (int i = 0; i < itemCount; i++)
            {
                FixedSwitchInfo info = fixedSwitchInfos[i];
                Rect rect = GUILayoutUtility.GetRect(1, ItemHeight, GUILayout.ExpandWidth(true));
                bool isMouseOver = rect.Contains(evt.mousePosition);

                DrawItemBackground(rect, isMouseOver, false);
                DrawFixedSwitchItemContent(rect, info);
                HandleItemClick(evt, isMouseOver, info.action);
            }
        }

        private void DrawSwitchItems()
        {
            if (switchInfosInDisplayOrder.Count == 0)
            {
                GUILayout.FlexibleSpace();
                return;
            }

            Event evt = Event.current;
            int displayIndex = 0;

            Rect scrollViewRect = GUILayoutUtility.GetRect(1, 1, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            float contentHeight = GetSwitchContentHeight();
            float contentWidth = GetSwitchContentWidth(scrollViewRect.width, contentHeight, scrollViewRect.height);
            Rect contentRect = new Rect(0, 0, contentWidth, contentHeight);

            EnsureSelectedSwitchVisible(scrollViewRect.height, contentHeight);
            switchScrollPosition = GUI.BeginScrollView(scrollViewRect, switchScrollPosition, contentRect);

            float y = 0;

            for (int groupIndex = 0; groupIndex < switchGroups.Count; groupIndex++)
            {
                SwitchGroup group = switchGroups[groupIndex];
                if (group?.switchInfos == null || group.switchInfos.Count == 0) continue;

                if (string.IsNullOrEmpty(group.title) == false)
                {
                    Rect titleRect = new Rect(0, y, contentWidth, GroupTitleHeight);
                    DrawSwitchGroupTitle(titleRect, group);
                    y += GroupTitleHeight;
                }

                for (int infoIndex = 0; infoIndex < group.switchInfos.Count; infoIndex++)
                {
                    SwitchInfo info = group.switchInfos[infoIndex];
                    if (info == null) continue;

                    Rect rect = new Rect(0, y, contentWidth, ItemHeight);
                    bool isSelected = displayIndex == selectedIndex;
                    bool isMouseOver = rect.Contains(evt.mousePosition);

                    DrawItemBackground(rect, isMouseOver, isSelected);
                    DrawSwitchItemContent(rect, info);
                    HandleItemClick(evt, isMouseOver, info.action);

                    y += ItemHeight;
                    displayIndex++;
                }
            }

            GUI.EndScrollView();
        }

        private float GetSwitchContentHeight()
        {
            float height = 0;

            for (int groupIndex = 0; groupIndex < switchGroups.Count; groupIndex++)
            {
                SwitchGroup group = switchGroups[groupIndex];
                if (group?.switchInfos == null || group.switchInfos.Count == 0) continue;

                if (string.IsNullOrEmpty(group.title) == false) height += GroupTitleHeight;

                for (int infoIndex = 0; infoIndex < group.switchInfos.Count; infoIndex++)
                {
                    if (group.switchInfos[infoIndex] != null) height += ItemHeight;
                }
            }

            return height;
        }

        private float GetSwitchContentWidth(float scrollViewWidth, float contentHeight, float viewportHeight)
        {
            if (contentHeight <= viewportHeight) return scrollViewWidth;

            float scrollBarWidth = GUI.skin.verticalScrollbar.fixedWidth;
            if (scrollBarWidth <= 0) scrollBarWidth = 16;
            return Mathf.Max(0, scrollViewWidth - scrollBarWidth);
        }

        private void EnsureSelectedSwitchVisible(float viewportHeight, float contentHeight)
        {
            if (scrollSelectedSwitchIntoView == false) return;
            if (Event.current.type != EventType.Repaint) return;

            bool hasItemPosition = TryGetSwitchItemPosition(selectedIndex, out float itemTop, out float itemBottom);
            if (hasItemPosition == false || viewportHeight <= 0)
            {
                scrollSelectedSwitchIntoView = false;
                return;
            }

            switchScrollPosition.y = CalculateScrollYToKeepItemVisible(switchScrollPosition.y, itemTop, itemBottom, viewportHeight, contentHeight);
            switchScrollPosition.x = 0;
            scrollSelectedSwitchIntoView = false;
        }

        private bool TryGetSwitchItemPosition(int index, out float itemTop, out float itemBottom)
        {
            itemTop = 0;
            itemBottom = 0;

            if (index < 0) return false;

            float y = 0;
            int displayIndex = 0;

            for (int groupIndex = 0; groupIndex < switchGroups.Count; groupIndex++)
            {
                SwitchGroup group = switchGroups[groupIndex];
                if (group?.switchInfos == null || group.switchInfos.Count == 0) continue;

                if (string.IsNullOrEmpty(group.title) == false) y += GroupTitleHeight;

                for (int infoIndex = 0; infoIndex < group.switchInfos.Count; infoIndex++)
                {
                    SwitchInfo info = group.switchInfos[infoIndex];
                    if (info == null) continue;

                    if (displayIndex == index)
                    {
                        itemTop = y;
                        itemBottom = y + ItemHeight;
                        return true;
                    }

                    y += ItemHeight;
                    displayIndex++;
                }
            }

            return false;
        }

        private static float CalculateScrollYToKeepItemVisible(float scrollY, float itemTop, float itemBottom, float viewportHeight, float contentHeight)
        {
            if (viewportHeight <= 0 || contentHeight <= viewportHeight) return 0;

            float nextScrollY = scrollY;

            if (itemTop < nextScrollY)
            {
                nextScrollY = itemTop;
            }
            else if (itemBottom > nextScrollY + viewportHeight)
            {
                nextScrollY = itemBottom - viewportHeight;
            }

            float maxScrollY = Mathf.Max(0, contentHeight - viewportHeight);
            return Mathf.Clamp(nextScrollY, 0, maxScrollY);
        }

        private void DrawSwitchGroupTitle(Rect rect, SwitchGroup group)
        {
            GUI.color = new Color(1, 1, 1, 0.06f);
            GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.5f);
            GUI.color = Color.white;
            GUI.Label(rect, group.title, groupTitleStyle);
        }

        private void DrawItemBackground(Rect rect, bool isMouseOver, bool isSelected)
        {
            if (isSelected)
            {
                ColorUtility.TryParseHtmlString("#2C5D87", out Color color);
                GUI.color = color;
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.5f);
                GUI.color = Color.white;
            }
            else if (isMouseOver)
            {
                GUI.color = new Color(1, 1, 1, 0.1f);
                GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture, ScaleMode.StretchToFill, true, 0.5f);
                GUI.color = Color.white;
            }
        }

        private void DrawFixedSwitchItemContent(Rect rect, FixedSwitchInfo info)
        {
            GUI.color = info.color;

            string keyCode = KeyCodeUtility.GetKeyString(info.keyCode);
            Rect keyCodeRect = new Rect(rect.x, rect.y, FixedKeyWidth, ItemHeight);
            GUI.Label(keyCodeRect, keyCode);

            Rect iconRect = new Rect(keyCodeRect.xMax, rect.y, FixedIconWidth, ItemHeight);
            DrawIcon(iconRect, info.icon, info.sdfIcon);

            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.xMax - iconRect.xMax, ItemHeight);
            GUI.Label(labelRect, info.label);

            GUI.color = Color.white;
        }

        private void DrawSwitchItemContent(Rect rect, SwitchInfo info)
        {
            GUI.color = info.color;

            Rect iconRect = new Rect(rect.x, rect.y, SwitchIconWidth, ItemHeight);
            DrawIcon(iconRect, info.icon, info.sdfIcon);

            Rect labelRect = new Rect(iconRect.xMax, rect.y, rect.xMax - iconRect.xMax, ItemHeight);
            GUI.Label(labelRect, info.label);

            GUI.color = Color.white;
        }

        private void DrawIcon(Rect rect, Texture icon, SdfIconType sdfIcon)
        {
            if (icon != null)
            {
                GUI.DrawTexture(rect, icon);
            }
            else if (sdfIcon != SdfIconType.None)
            {
                SdfIcons.DrawIcon(rect, sdfIcon);
            }
        }

        private void HandleItemClick(Event evt, bool isMouseOver, Action action)
        {
            bool isExecute = isMouseOver && evt.type == EventType.MouseDown && evt.button == 0;
            if (isExecute == false) return;

            action?.Invoke();
            evt.Use();
            CloseWindow(false);
        }
    }
}
