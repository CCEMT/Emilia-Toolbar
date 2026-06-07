using System;
using System.Reflection;
using Emilia.Kit.Editor;

namespace Emilia.Toolbar.Editor
{
    public class CommandActionInfo
    {
        public MethodInfo methodInfo { get; private set; }

        public Action<object[]> actionInfo { get; private set; }

        public CommandActionInfo(MethodInfo methodInfo)
        {
            this.methodInfo = methodInfo;
            actionInfo = (arg) => ReflectUtility.Invoke(null, methodInfo, arg);
        }

        public CommandActionInfo(MethodInfo methodInfo, Action<object[]> action)
        {
            this.methodInfo = methodInfo;
            actionInfo = action;
        }

        public CommandActionInfo(Type type, string methodName, Type[] argTypes)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            methodInfo = type.GetMethod(methodName, bindingFlags, null, argTypes, null);
            actionInfo = (arg) => ReflectUtility.Invoke(null, methodInfo, arg);
        }
    }
}