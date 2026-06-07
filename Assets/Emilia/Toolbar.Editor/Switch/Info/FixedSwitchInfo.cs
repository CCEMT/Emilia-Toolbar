using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class FixedSwitchInfo
    {
        public string label;

        public Color color = Color.white;
        public Texture icon;
        public SdfIconType sdfIcon;

        public KeyCode keyCode = KeyCode.None;

        public Action action;
    }
}