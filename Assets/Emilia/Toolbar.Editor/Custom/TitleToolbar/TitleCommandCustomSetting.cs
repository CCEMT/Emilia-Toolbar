using System;
using System.Collections.Generic;
using Emilia.Kit;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class TitleCommandCustomSetting : LocalSetting<TitleCommandCustomSetting>
    {
        public List<TitleCustomCommandInfo> customCommands = new List<TitleCustomCommandInfo>();
    }
}