using System;
using System.Reflection;
using Emilia.Kit.Editor;

namespace Emilia.Toolbar.Editor
{
    public class CommandValidationInfo
    {
        public Func<bool> validationFunc { get; private set; }

        public CommandValidationInfo(MethodInfo methodInfo)
        {
            validationFunc = () => (bool) ReflectUtility.Invoke(null, methodInfo, Array.Empty<object>());
        }

        public CommandValidationInfo(Func<bool> func)
        {
            this.validationFunc = func;
        }

        public CommandValidationInfo(Type type, string methodName)
        {
            validationFunc = () => (bool) ReflectUtility.Invoke(null, type, methodName);
        }

        public bool Validate() => validationFunc?.Invoke() ?? false;
    }
}
