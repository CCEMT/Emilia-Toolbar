using System.Collections.Generic;
using System.Reflection;
using Emilia.Kit;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class CommandMenuItemCollect : ICommandCollect
    {
        public IEnumerable<CommandInfo> Collect()
        {
            if (ConsoleSetting.instance.isCollectMenuItem == false) yield break;

            IList<MethodInfo> methodInfos = TypeCache.GetMethodsWithAttribute<MenuItem>();

            Dictionary<string, CommandValidationInfo> validationInfos = new();
            List<MethodInfo> actionInfos = new();

            int count = methodInfos.Count;
            for (int i = 0; i < count; i++)
            {
                MethodInfo methodInfo = methodInfos[i];
                foreach (MenuItem menuItem in methodInfo.GetCustomAttributes<MenuItem>())
                {
                    if (menuItem.menuItem.Contains("internal")) continue;

                    if (menuItem.validate)
                    {
                        if (validationInfos.TryAdd(menuItem.menuItem, new CommandValidationInfo(methodInfo)) == false)
                        {
                            Debug.LogError($"命令验证重复注册 name={menuItem.menuItem}");
                        }
                    }
                    else
                    {
                        actionInfos.Add(methodInfo);
                    }
                }
            }

            int actionCount = actionInfos.Count;
            for (int i = 0; i < actionCount; i++)
            {
                MethodInfo methodInfo = actionInfos[i];
                foreach (MenuItem menuItem in methodInfo.GetCustomAttributes<MenuItem>())
                {
                    CommandInfo commandInfo = new();
                    commandInfo.name = menuItem.menuItem;
                    commandInfo.category += "MenuItem/" + EditorKit.PathToCategory(menuItem.menuItem);
                    commandInfo.order = menuItem.priority;

                    commandInfo.actionInfo = new CommandActionInfo(methodInfo);
                    commandInfo.validationInfo = validationInfos.GetValueOrDefault(menuItem.menuItem);
                    yield return commandInfo;
                }
            }
        }
    }
}