using System.Collections.Generic;
using System.Linq;
using Emilia.Kit;
using Emilia.Kit.Editor;
using UnityEditor;

namespace Emilia.Toolbar.Editor
{
    public partial class ConsoleCommandWindow : EditorWindow
    {
        [MenuItem("Emilia/Toolbar/Console")]
        public static void Open()
        {
            EditorImGUIKit.OpenWindow<ConsoleCommandWindow>("Console Command", 600, 600);
        }

        private string previousSearchTerms { get; set; }
        private string searchTerms { get; set; }
        private string currentCategory { get; set; }
        private string backCategory { get; set; }
        private int selectedIndex { get; set; }

        private bool isFocus { get; set; }

        private List<CommandInfo> commandResult { get; set; }

        private void OnEnable()
        {
            wantsMouseMove = true;
            if (ConsoleSetting.instance.openPause) EditorApplication.isPaused = true;
        }

        private void OnFocus()
        {
            isFocus = true;
            Repaint();
        }

        private void OnLostFocus()
        {
            isFocus = false;
            Repaint();
        }

        private void OnGUI()
        {
            Style.Initialize();

            if (isFocus) Repaint();
            if (ConsoleSetting.instance.consoleForceFocus) Focus();

            RefreshCommandResult();

            OnBeforeGUIEvent();

            OnSearchGUI();
            OnTitleGUI();
            OnCommandGUI();

            OnAfterGUIEvent();
        }

        private void RefreshCommandResult(bool force = false)
        {
            if (commandResult == null) commandResult = new List<CommandInfo>();
            if (force == false && previousSearchTerms == searchTerms) return;

            previousSearchTerms = searchTerms;
            selectedIndex = 0;
            commandResult.Clear();

            if (string.IsNullOrEmpty(searchTerms))
            {
                if (string.IsNullOrEmpty(currentCategory)) return;

                CommandCategoryInfo categoryInfo = CommandCache.instance.commandCategoryInfoByName.GetValueOrDefault(currentCategory);
                if (categoryInfo == null) return;

                int count = categoryInfo.commands.Count;
                for (int i = 0; i < count; i++)
                {
                    string commandName = categoryInfo.commands[i];
                    CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(commandName);
                    if (commandInfo == null) continue;
                    commandResult.Add(commandInfo);
                }
                return;
            }

            List<(CommandInfo, int)> searchResults = new();

            foreach (var pair in CommandCache.instance.commandInfoByName)
            {
                int score = CommandPinyinSearchCache.SmartSearch(pair.Key, searchTerms);
                if (score == 0) continue;
                searchResults.Add((pair.Value, score));
            }

            searchResults.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            commandResult.AddRange(searchResults.Select((i) => i.Item1).Distinct());
        }
    }
}
