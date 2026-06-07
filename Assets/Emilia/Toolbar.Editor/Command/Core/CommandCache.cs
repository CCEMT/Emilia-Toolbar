using System;
using System.Collections.Generic;
using Emilia.Kit;
using UnityEditor;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class CommandCache
    {
        private static CommandCache _instance;
        public static CommandCache instance => _instance ??= new CommandCache();

        private List<CommandInfo> _commandInfos = new List<CommandInfo>();
        private Dictionary<string, CommandInfo> _commandInfoByName = new Dictionary<string, CommandInfo>();

        private List<string> _rootCategories = new List<string>();
        private Dictionary<string, CommandCategoryInfo> _commandCategoryInfoByName = new Dictionary<string, CommandCategoryInfo>();

        public IReadOnlyList<CommandInfo> commandInfos => this._commandInfos;
        public IReadOnlyDictionary<string, CommandInfo> commandInfoByName => this._commandInfoByName;

        public IReadOnlyList<string> rootCategories => this._rootCategories;
        public IReadOnlyDictionary<string, CommandCategoryInfo> commandCategoryInfoByName => this._commandCategoryInfoByName;

        private CommandCache()
        {
            ResetCache();
        }

        public void ResetCache()
        {
            _commandInfos.Clear();
            _commandInfoByName.Clear();
            _rootCategories.Clear();
            _commandCategoryInfoByName.Clear();

            List<CommandInfo> collectedCommandInfos = new List<CommandInfo>();
            IList<Type> types = TypeCache.GetTypesDerivedFrom<ICommandCollect>();
            int count = types.Count;
            for (int i = 0; i < count; i++)
            {
                Type type = types[i];
                if (type.IsAbstract || type.IsInterface) continue;

                ICommandCollect commandCollect = Activator.CreateInstance(type) as ICommandCollect;
                foreach (CommandInfo commandInfo in commandCollect.Collect()) collectedCommandInfos.Add(commandInfo);
            }

            List<CommandInfo> selectedCommandInfos = SelectCommandInfos(collectedCommandInfos);
            foreach (CommandInfo commandInfo in selectedCommandInfos) AddCommandInfo(commandInfo);

            _commandInfos.Sort((a, b) => a.order.CompareTo(b.order));

            int commandCount = this._commandInfos.Count;
            for (int i = 0; i < commandCount; i++)
            {
                CommandInfo commandInfo = this._commandInfos[i];
                if (string.IsNullOrEmpty(commandInfo.category)) continue;
                AddCategoryInfoByCommandInfo(commandInfo);
            }
        }

        private List<CommandInfo> SelectCommandInfos(List<CommandInfo> collectedCommandInfos)
        {
            List<CommandInfo> selectedCommandInfos = new List<CommandInfo>();
            Dictionary<string, CommandInfo> selectedCommandInfoByName = new Dictionary<string, CommandInfo>();
            Dictionary<string, bool> selectedCommandValidityByName = new Dictionary<string, bool>();

            int count = collectedCommandInfos.Count;
            for (int i = 0; i < count; i++)
            {
                CommandInfo commandInfo = collectedCommandInfos[i];
                if (commandInfo == null) continue;
                if (string.IsNullOrEmpty(commandInfo.name)) continue;

                if (selectedCommandInfoByName.TryGetValue(commandInfo.name, out CommandInfo selectedCommandInfo))
                {
                    if (selectedCommandValidityByName.TryGetValue(commandInfo.name, out bool selectedCommandIsValid) == false)
                    {
                        selectedCommandIsValid = IsValidCommandInfo(selectedCommandInfo);
                        selectedCommandValidityByName[commandInfo.name] = selectedCommandIsValid;
                    }

                    if (selectedCommandIsValid) continue;

                    bool isCommandValid = IsValidCommandInfo(commandInfo);
                    if (isCommandValid == false) continue;

                    int selectedIndex = selectedCommandInfos.IndexOf(selectedCommandInfo);
                    if (selectedIndex >= 0) selectedCommandInfos[selectedIndex] = commandInfo;
                    selectedCommandInfoByName[commandInfo.name] = commandInfo;
                    selectedCommandValidityByName[commandInfo.name] = true;
                    continue;
                }

                selectedCommandInfos.Add(commandInfo);
                selectedCommandInfoByName.Add(commandInfo.name, commandInfo);
            }

            return selectedCommandInfos;
        }

        private bool IsValidCommandInfo(CommandInfo commandInfo)
        {
            if (commandInfo == null) return false;
            if (commandInfo.validationInfo == null) return true;

            try
            {
                return commandInfo.validationInfo.Validate();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        private void AddCategoryInfoByCommandInfo(CommandInfo commandInfo)
        {
            string[] categoryParts = commandInfo.category.Split('/');
            string currentCategoryPath = "";
            string parentCategory = null;

            for (int j = 0; j < categoryParts.Length; j++)
            {
                string categoryPart = categoryParts[j].Trim();
                if (string.IsNullOrEmpty(categoryPart)) continue;

                currentCategoryPath = j == 0 ? categoryPart : currentCategoryPath + "/" + categoryPart;
                AddCategoryInfo(currentCategoryPath, categoryPart, parentCategory);
                parentCategory = currentCategoryPath;
            }

            if (this._commandCategoryInfoByName.ContainsKey(commandInfo.category) == false) return;

            CommandCategoryInfo finalCategoryInfo = this._commandCategoryInfoByName[commandInfo.category];
            if (finalCategoryInfo.commands.Contains(commandInfo.name) == false) finalCategoryInfo.commands.Add(commandInfo.name);
        }

        private void AddCategoryInfo(string currentCategoryPath, string categoryPart, string parentCategory)
        {
            if (this._commandCategoryInfoByName.ContainsKey(currentCategoryPath)) return;
            CommandCategoryInfo categoryInfo = new CommandCategoryInfo();
            categoryInfo.categoryName = categoryPart;
            categoryInfo.parent = parentCategory;

            this._commandCategoryInfoByName[currentCategoryPath] = categoryInfo;

            bool isAddRoot = parentCategory == null && this._rootCategories.Contains(currentCategoryPath) == false;
            bool isAddChild = parentCategory != null && this._commandCategoryInfoByName.ContainsKey(parentCategory);

            if (isAddRoot) this._rootCategories.Add(currentCategoryPath);
            else if (isAddChild)
            {
                CommandCategoryInfo parentCategoryInfo = this._commandCategoryInfoByName[parentCategory];
                if (parentCategoryInfo.children.Contains(currentCategoryPath) == false) parentCategoryInfo.children.Add(currentCategoryPath);
            }
        }

        public void AddCommandInfo(CommandInfo commandInfo)
        {
            if (this._commandInfoByName.TryAdd(commandInfo.name, commandInfo) == false) Debug.LogError($"命令重复注册 name={commandInfo.name}");
            else
            {
                this._commandInfos.Add(commandInfo);
                if (string.IsNullOrEmpty(commandInfo.alias) == false)
                {
                    if (this._commandInfoByName.TryAdd(commandInfo.alias, commandInfo) == false) Debug.LogError($"命令别名重复注册 alias={commandInfo.alias}");
                }

                bool containsChinese = CommandPinyinSearchCache.ContainsChinese(commandInfo.name);
                if (containsChinese == false) return;

                if (ConsoleSetting.instance.pinyinSearch == false) return;
                string pinyin = CommandPinyinSearchCache.ConvertToAllSpell(commandInfo.name);
                this._commandInfoByName.TryAdd(pinyin, commandInfo);
            }
        }
    }
}
