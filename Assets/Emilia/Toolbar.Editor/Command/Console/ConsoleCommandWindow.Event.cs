using System;
using Emilia.Kit;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public partial class ConsoleCommandWindow
    {
        private EventType eventType;
        private int mouseButton;

        private bool executeCommand;
        private string hoveredCategory;

        private void OnBeforeGUIEvent()
        {
            Event evt = Event.current;

            this.eventType = evt.type;
            this.mouseButton = evt.button;

            if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Escape)
            {
                Close();
            }
            else if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.DownArrow)
            {
                MoveSelection(1);
            }
            else if (evt.type == EventType.KeyDown && evt.keyCode == KeyCode.UpArrow)
            {
                MoveSelection(-1);
            }

            this.executeCommand = evt.type == EventType.KeyDown && evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter or KeyCode.Tab;
        }

        private void MoveSelection(int offset)
        {
            if (commandResult == null || commandResult.Count == 0) return;

            int nextSelectedIndex = Mathf.Clamp(selectedIndex + offset, 0, commandResult.Count - 1);
            if (nextSelectedIndex == selectedIndex) return;

            selectedIndex = nextSelectedIndex;
            scrollToCommandIndex = nextSelectedIndex;
            scrollToCommandSearchTerms = searchTerms;
            scrollToCommandCategory = currentCategory;
            Repaint();
        }

        private void OnAfterGUIEvent()
        {
            OnCommandHandle();
            OnCategoryHandle();

            this.executeCommand = false;
        }

        private void OnCommandHandle()
        {
            if (isFocus == false) return;
            if (this.executeCommand == false) return;
            if (this.mouseButton != 0) return;

            bool valid = commandResult != null && selectedIndex >= 0 && commandResult.Count > selectedIndex;
            if (valid) CommandHandle(commandResult[selectedIndex]);
        }

        public void CommandHandle(CommandInfo commandInfo)
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

            Close();
        }

        private void OnCategoryHandle()
        {
            if (isFocus == false) return;
            if (string.IsNullOrEmpty(this.hoveredCategory)) return;
            if (this.eventType != EventType.MouseDown || this.mouseButton != 0) return;

            backCategory = currentCategory;
            currentCategory = this.hoveredCategory;
            this.hoveredCategory = "";

            RefreshCommandResult(true);
            Repaint();
        }
    }
}