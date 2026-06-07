namespace Emilia.Toolbar.Editor
{
    public interface ISwitchSelector
    {
        int priority { get; }
        FixedSwitchInfo[] GetFixedSwitchInfos(SwitchContext context);
        SwitchGroup GetSwitchGroup(SwitchContext context);
    }
}
