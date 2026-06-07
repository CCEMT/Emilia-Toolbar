using System;

namespace Emilia.Toolbar.Editor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandValidationAttribute : Attribute
    {
        public string name;

        public CommandValidationAttribute(string name)
        {
            this.name = name;
        }
    }
}