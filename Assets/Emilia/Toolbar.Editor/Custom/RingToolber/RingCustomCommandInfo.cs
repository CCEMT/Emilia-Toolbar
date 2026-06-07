using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    [Serializable]
    public class RingCustomCommandInfo
    {
        public string name;
        public string description;

        public Texture icon;
        public SdfIconType sdfIcon;

        public Color color = Color.white;
        public string commandName;
    }
}