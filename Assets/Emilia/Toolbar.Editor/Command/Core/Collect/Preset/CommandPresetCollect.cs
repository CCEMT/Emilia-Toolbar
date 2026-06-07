using System;
using System.Collections.Generic;
using Emilia.Kit;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class CommandPresetCollect : ICommandCollect
    {
        const string CategoryName = "Preset";

        public IEnumerable<CommandInfo> Collect()
        {
            for (int i = 0; i < PresetCommandSetting.instance.presetCommandInfos.Count; i++)
            {
                PresetCommandInfo info = PresetCommandSetting.instance.presetCommandInfos[i];

                CommandInfo commandInfo = new CommandInfo();
                commandInfo.name = info.alias;
                commandInfo.description = info.description;

                if (string.IsNullOrEmpty(info.category)) commandInfo.category = CategoryName;
                else commandInfo.category = CategoryName + "/" + info.category;

                Action action = () => PresetAction(info);
                Func<bool> validation = () => PresetValidation(info);

                commandInfo.actionInfo = new CommandActionInfo(action.Method, (_) => action());
                commandInfo.validationInfo = new CommandValidationInfo(validation);

                yield return commandInfo;
            }
        }

        static void PresetAction(PresetCommandInfo presetInfo)
        {
            CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(presetInfo.commandName);
            if (commandInfo == null) return;

            try
            {
                if (presetInfo.args != null) commandInfo.ExecuteCommand(presetInfo.args);
                else
                {
                    if (commandInfo.actionInfo.methodInfo.GetParameters().Length == 0) commandInfo.ExecuteCommand();
                    else ArgCommandExecuteWindow.Open(commandInfo.name);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToUnityLogString());
            }
        }

        static bool PresetValidation(PresetCommandInfo presetInfo)
        {
            CommandInfo commandInfo = CommandCache.instance.commandInfoByName.GetValueOrDefault(presetInfo.commandName);
            if (commandInfo == null) return false;

            if (commandInfo.validationInfo == null) return true;

            try
            {
                return commandInfo.validationInfo.Validate();
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToUnityLogString());
                return false;
            }
        }
    }
}