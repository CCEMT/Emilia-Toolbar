using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class FixedCustomCommandInfo
    {
        public string name;

        public Texture icon;
        public SdfIconType sdfIcon;

        public Color color = Color.white;
        public string commandName;
    }
}