using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class CommandAttributeCollect : ICommandCollect
    {
        public IEnumerable<CommandInfo> Collect()
        {
            Dictionary<string, CommandValidationInfo> validationInfos = new Dictionary<string, CommandValidationInfo>();

            IList<MethodInfo> validationMethodInfo = TypeCache.GetMethodsWithAttribute<CommandValidationAttribute>();
            int validationCount = validationMethodInfo.Count;
            for (int i = 0; i < validationCount; i++)
            {
                MethodInfo method = validationMethodInfo[i];
                CommandValidationAttribute attribute = method.GetCustomAttribute<CommandValidationAttribute>();

                CommandValidationInfo validationInfo = new CommandValidationInfo(method);
                if (validationInfos.TryAdd(attribute.name, validationInfo) == false) Debug.LogError($"命令验证重复注册 name={attribute.name}");
            }

            IList<MethodInfo> actionMethodInfo = TypeCache.GetMethodsWithAttribute<CommandAttribute>();
            int actionCount = actionMethodInfo.Count;
            for (int i = 0; i < actionCount; i++)
            {
                MethodInfo method = actionMethodInfo[i];
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();

                CommandInfo commandInfo = new CommandInfo();
                commandInfo.name = attribute.name;
                commandInfo.alias = attribute.alias;
                commandInfo.description = attribute.description;
                commandInfo.category = attribute.category;
                commandInfo.order = attribute.order;
                commandInfo.actionInfo = new CommandActionInfo(method);
                commandInfo.validationInfo = validationInfos.GetValueOrDefault(attribute.name);
                yield return commandInfo;
            }
        }
    }
}