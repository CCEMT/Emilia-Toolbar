using System;
using System.Collections.Generic;
using Emilia.Kit;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class PresetCommandSetting : ProjectLocalSetting<PresetCommandSetting>
    {
        [SerializeField]
        public List<PresetCommandInfo> presetCommandInfos = new List<PresetCommandInfo>();
    }
}