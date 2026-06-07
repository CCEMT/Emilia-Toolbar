using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class SwitchInfo
    {
        public string label;

        public Color color = Color.white;
        public Texture icon;
        public SdfIconType sdfIcon;

        public Action action;
    }
}