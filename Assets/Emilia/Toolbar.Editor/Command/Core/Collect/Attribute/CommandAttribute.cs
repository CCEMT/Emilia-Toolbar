using System;

namespace Emilia.Toolbar.Editor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string name;
        public string alias;
        public string category;
        public string description;

        public int order;

        public CommandAttribute(string name, string description = "", string category = "Default", string alias = "", int order = int.MaxValue)
        {
            this.name = name;
            this.alias = alias;
            this.category = category;
            this.description = description;
            this.order = order;
        }
    }
}