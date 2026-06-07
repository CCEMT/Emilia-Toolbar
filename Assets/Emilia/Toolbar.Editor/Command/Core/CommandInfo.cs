namespace Emilia.Toolbar.Editor
{
    public class CommandInfo
    {
        public string name;
        public string alias;
        public string category;
        public string description;

        public int order;
        public CommandActionInfo actionInfo;
        public CommandValidationInfo validationInfo;

        public void ExecuteCommand(params object[] parameters)
        {
            this.actionInfo.actionInfo?.Invoke(parameters);
        }
    }
}