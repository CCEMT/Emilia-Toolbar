using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public partial class ConsoleCommandWindow
    {
        private Vector2 scrollPosition;

        private void OnSearchGUI()
        {
            EditorGUILayout.BeginHorizontal(Style.instance.SearchStyle);
            OnSearchFieldGUI();
            EditorGUILayout.EndHorizontal();
        }

        private void OnSearchFieldGUI()
        {
            EditorGUILayout.BeginVertical(Style.instance.SearchLabelGroupStyle);

            EditorGUILayout.BeginVertical(Style.instance.SearchLabelStyle);

            GUILayout.BeginHorizontal(Style.instance.SearchLabelStyle);
            GUILayout.BeginHorizontal(Style.instance.SearchIconBackgroundStyle);
            GUI.color = Color.black;
            EditorGUILayout.LabelField("", Style.instance.SearchIconStyle);
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            OnInputFieldGUI();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void OnInputFieldGUI()
        {
            const string controlName = "searchCommand";

            GUI.SetNextControlName(controlName);
            bool wasEmpty = string.IsNullOrEmpty(searchTerms);
            GUIStyle textStyle = wasEmpty ? Style.instance.SearchLabelStyleEmpty : Style.instance.SearchLabelStyle;

            searchTerms = ConsoleText.TextField(searchTerms ?? "", textStyle);

            if (string.IsNullOrEmpty(searchTerms))
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.Label(lastRect, Style.CommandSearchLabel, Style.instance.SearchLabelHelpStyle);
            }

            if (isFocus) GUI.FocusControl(controlName);
        }

        private void OnTitleGUI()
        {
            EditorGUILayout.BeginHorizontal(Style.instance.TitleStyle);
            string label = string.IsNullOrEmpty(currentCategory) ? "" : currentCategory;
            EditorGUILayout.LabelField(label, Style.instance.TitleLabelStyle);
            EditorGUILayout.EndHorizontal();

            if (string.IsNullOrEmpty(currentCategory)) return;

            Rect lastRect = GUILayoutUtility.GetLastRect();

            Rect iconRect = new Rect(lastRect);
            iconRect.x += 10f;
            iconRect.width = 30;
            iconRect.height = 25;

            GUI.Label(iconRect, new GUIContent(Style.instance.LeftIcon));

            Vector2 mousePosition = Event.current.mousePosition;
            if (lastRect.Contains(mousePosition) && Event.current.type == EventType.MouseDown)
            {
                hoveredCategory = null;
                currentCategory = backCategory;

                if (string.IsNullOrEmpty(currentCategory) == false)
                {
                    CommandCategoryInfo commandCategoryInfo = CommandCache.instance.commandCategoryInfoByName.GetValueOrDefault(currentCategory);
                    backCategory = commandCategoryInfo?.parent;
                }

                RefreshCommandResult(true);
            }
        }

        private void OnCommandGUI()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            GUI.skin = Style.instance.ScrollBarStyle;

            this.scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (string.IsNullOrEmpty(searchTerms)) OnAllCommandListGUI();
            else OnCommandListGUI();

            EditorGUILayout.EndScrollView();

            GUI.skin = Style.instance.DefaultSkin;

            EditorGUILayout.EndVertical();
        }

        private void OnAllCommandListGUI()
        {
            if (string.IsNullOrEmpty(currentCategory))
            {
                int count = CommandCache.instance.rootCategories.Count;
                for (int i = 0; i < count; i++)
                {
                    string id = CommandCache.instance.rootCategories[i];
                    CommandCategoryInfo categoryInfo = CommandCache.instance.commandCategoryInfoByName.GetValueOrDefault(id);
                    OnCategoryGUI(id, categoryInfo.categoryName);
                }
            }
            else
            {
                CommandCategoryInfo categoryInfo = CommandCache.instance.commandCategoryInfoByName.GetValueOrDefault(currentCategory);
                if (categoryInfo != null)
                {
                    int count = categoryInfo.children.Count;
                    for (int i = 0; i < count; i++)
                    {
                        string id = categoryInfo.children[i];
                        CommandCategoryInfo childrenCategoryInfo = CommandCache.instance.commandCategoryInfoByName.GetValueOrDefault(id);
                        OnCategoryGUI(id, childrenCategoryInfo.categoryName);
                    }
                }
            }

            OnCommandListGUI();
        }

        private void OnCommandListGUI()
        {
            if (commandResult == null || commandResult.Count == 0) return;

            int count = commandResult.Count;
            for (int i = 0; i < count; i++)
            {
                CommandInfo commandInfo = commandResult[i];
                OnItemGUI(commandInfo, i);

                Rect rect = GUILayoutUtility.GetLastRect();
                bool isMouseOver = rect.Contains(Event.current.mousePosition);

                if (this.eventType == EventType.MouseMove)
                {
                    if (isMouseOver)
                    {
                        selectedIndex = i;
                        Repaint();
                    }
                }

                if (isMouseOver && this.eventType == EventType.MouseDown) this.executeCommand = true;
            }
        }

        private void OnCategoryGUI(string id, string name)
        {
            int height = Style.ItemHeight;

            bool selected = id == hoveredCategory;
            GUIStyle style = selected ? Style.instance.CommandResultLayoutForcedHighlightStyle : Style.instance.CommandResultLayoutNoHighlightStyle;

            EditorGUILayout.BeginVertical(style, GUILayout.ExpandHeight(true), GUILayout.MinHeight(height));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(Style.instance.CommandResultGroupStyle);

            EditorGUILayout.BeginVertical(Style.instance.ParamIconGroupStyle, GUILayout.Height(height), GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();
            GUILayout.Label("", Style.instance.CategoryIconStyle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.FlexibleSpace();

            string nameLabel = $"<color=white><b>{name}</b></color>";
            GUILayout.Label(nameLabel, Style.instance.CommandNameStyle);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition)) hoveredCategory = id;
        }

        private void OnItemGUI(CommandInfo commandInfo, int index)
        {
            int height = Style.ItemHeight;

            bool selected = index == selectedIndex;
            GUIStyle style = selected ? Style.instance.CommandResultLayoutForcedHighlightStyle : Style.instance.CommandResultLayoutNoHighlightStyle;

            EditorGUILayout.BeginVertical(style, GUILayout.ExpandHeight(true), GUILayout.MinHeight(height));

            EditorGUILayout.BeginVertical(Style.instance.CommandResultInsideLayoutStyle, GUILayout.ExpandHeight(true));

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(Style.instance.CommandResultGroupStyle);

            EditorGUILayout.BeginVertical(Style.instance.ParamIconGroupStyle, GUILayout.Height(height), GUILayout.ExpandHeight(true));
            GUILayout.FlexibleSpace();

            GUILayout.Label("", Style.instance.RunIconStyle);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            GUILayout.FlexibleSpace();

            DisplayCommandTitle(commandInfo);
            DisplayCommandHelp(commandInfo, selected);

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        void DisplayCommandHelp(CommandInfo commandInfo, bool selected)
        {
            if (string.IsNullOrEmpty(commandInfo.description)) return;

            GUIStyle style = selected ? Style.instance.CommandHelpStyleSelected : Style.instance.CommandHelpStyle;
            GUILayout.Label(commandInfo.description, style);
        }

        void DisplayCommandTitle(CommandInfo commandInfo)
        {
            string commandTitle = commandInfo.name;

            if (searchTerms == null)
            {
                string colorHtml = ColorUtility.ToHtmlStringRGBA(Style.instance.HighlightOnSelectedTextColor);
                commandTitle = $"<color=#{colorHtml}>{commandTitle}</color>";
            }
            else
            {
                commandTitle = Highlight(commandTitle, searchTerms, true,
                    $"<color=#{ColorUtility.ToHtmlStringRGBA(Style.instance.SearchResultTextColor)}>",
                    "</color>",
                    $"<color=#{ColorUtility.ToHtmlStringRGBA(Style.instance.HighlightOnSelectedTextColor)}>",
                    "</color>"
                );
            }

            commandTitle = AppendQuickName(commandInfo, commandTitle, searchTerms);
            commandTitle = AppendCollector(commandInfo, commandTitle);

            commandTitle = $"<b>{commandTitle}</b>";
            GUILayout.Label(commandTitle, Style.instance.CommandNameStyle);
        }

        string AppendQuickName(CommandInfo commandInfo, string commandTitle, string searchTerms)
        {
            if (string.IsNullOrEmpty(commandInfo.alias) == false)
            {
                string quick = commandInfo.alias.ToUpper();
                if (searchTerms != null)
                {
                    quick = Highlight(quick, searchTerms, true,
                        $"<color=#{ColorUtility.ToHtmlStringRGBA(Style.instance.QuickNameTextColor)}>",
                        "</color>",
                        $"<color=#{ColorUtility.ToHtmlStringRGBA(Style.instance.HighlightOnSelectedTextColor)}>",
                        "</color>"
                    );
                }
                else
                {
                    string colorHtml = ColorUtility.ToHtmlStringRGBA(Style.instance.QuickNameTextColor);
                    quick = $"<color=#{colorHtml}>{quick}</color>";
                }

                string commandHelpTextColorHtml = ColorUtility.ToHtmlStringRGBA(Style.instance.CommandHelpTextColor);
                commandTitle = commandTitle.Insert(0, quick + $"<color=#{commandHelpTextColorHtml}> | </color>");
            }
            return commandTitle;
        }

        string AppendCollector(CommandInfo commandInfo, string commandTitle)
        {
            bool isPreset = PresetCommandSetting.instance.presetCommandInfos.Any((i) => i.alias == commandInfo.name);
            if (isPreset) commandTitle = $@"<color=yellow>★{commandTitle}★</color>";
            return commandTitle;
        }

        public string Highlight(string text, string searchTerms, bool considerSearchTermsAsWords = true,
            string nonHighlightPrefix = "",
            string nonHighlightSuffix = "",
            string highlightPrefix = "<b><color=#00EE00>",
            string highlightSuffix = "</color></b>")
        {

            List<int> highlighIDs = new List<int>();

            if (considerSearchTermsAsWords)
            {
                foreach (var term in searchTerms.Split(new[] {' '},
                             StringSplitOptions.RemoveEmptyEntries))
                {
                    int index = text.ToLower().IndexOf(term.ToLower(), StringComparison.Ordinal);
                    if (index >= 0 && ! highlighIDs.Contains(index))
                    {
                        for (int i = 0; i < term.Length; i++)
                        {
                            highlighIDs.Add(index + i);
                        }
                    }
                }
            }
            else
            {
                highlighIDs.AddRange(AllSubIndexesOf(text, searchTerms));

            }

            if (! highlighIDs.Any())
            {
                return nonHighlightPrefix + text + nonHighlightSuffix;
            }

            StringBuilder sb = new StringBuilder();
            int previousID = -2;

            highlighIDs.Sort();

            foreach (var id in highlighIDs)
            {
                if (id > 0 && previousID == -2)
                {
                    sb.Append(nonHighlightPrefix);
                    sb.Append(text.Substring(0, id));
                    sb.Append(nonHighlightSuffix);
                }

                if (id - 1 != previousID)
                {
                    if (previousID != -2)
                    {
                        sb.Append(highlightSuffix);
                        sb.Append(nonHighlightPrefix);
                        if (id - previousID - 1 > 0) sb.Append(text.Substring(previousID + 1, id - previousID - 1));

                        sb.Append(nonHighlightSuffix);
                    }
                    sb.Append(highlightPrefix);
                    sb.Append(text[id]);
                }
                else
                {
                    sb.Append(text[id]);
                }
                previousID = id;
            }

            sb.Append(highlightSuffix);
            if (previousID != text.Length - 1)
            {
                sb.Append(nonHighlightPrefix);
                sb.Append(text.Substring(previousID + 1, text.Length - previousID - 1));
                sb.Append(nonHighlightSuffix);
            }

            return sb.ToString();
        }

        List<int> AllSubIndexesOf(string item, string toSearchFor, bool searchInitials = true)
        {
            List<int> idMatches = new List<int>();
            List<int> indexes = AllIndexesOf(item, toSearchFor);

            foreach (var index in indexes)
            {
                if (index == -1) continue;

                int to = index + toSearchFor.Length - 1;
                for (int i = index; i <= to; i++)
                {
                    idMatches.Add(i);
                }
            }

            if (searchInitials == false) return idMatches;

            List<int> initialsIds = new List<int>();

            var initials = GetVariableInitials(item, initialsIds);
            if (initials.Equals(toSearchFor, StringComparison.OrdinalIgnoreCase) == false) return idMatches;

            foreach (var id in initialsIds)
            {
                if (idMatches.Contains(id)) continue;
                idMatches.Add(id);
            }

            return idMatches;
        }

        List<int> AllIndexesOf(string str, string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException("the string to find may not be empty", "value");

            List<int> indexes = new List<int>();
            for (int index = 0;; index += value.Length)
            {
                index = str.IndexOf(value, index, StringComparison.OrdinalIgnoreCase);
                if (index == -1) return indexes;

                indexes.Add(index);
            }
        }

        string GetVariableInitials(string variable, List<int> initialIds = null)
        {
            StringBuilder initials = new StringBuilder();
            initials.Append(variable[0].ToString().ToUpper());

            if (initialIds != null)
            {
                initialIds.Clear();
                initialIds.Add(0);
            }

            for (int i = 1; i < variable.Length; i++)
            {
                if (char.IsUpper(variable[i]) == false) continue;

                initials.Append(variable[i]);
                if (initialIds != null) initialIds.Add(i);
            }

            return initials.ToString();
        }
    }
}