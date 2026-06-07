using System;
using Sirenix.OdinInspector;

namespace Emilia.Toolbar.Editor
{
    [Serializable, HideReferenceObjectPicker, HideLabel]
    public class CommandNameSelector
    {
        [LabelText("命令"), ValueDropdown(nameof(GetCommandNames))]
        public string commandName;

        public CommandNameSelector(string commandName)
        {
            this.commandName = commandName;
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