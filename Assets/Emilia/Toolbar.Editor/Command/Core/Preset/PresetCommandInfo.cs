using System;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class PresetCommandInfo
    {
        public string alias;
        public string category;
        public string description;

        public string commandName;
        public object[] args;
    }
}