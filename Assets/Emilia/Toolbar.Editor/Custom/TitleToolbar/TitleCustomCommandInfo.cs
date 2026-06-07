using Sirenix.OdinInspector;
using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public class TitleCustomCommandInfo
    {
        public enum TitlePositionType
        {
            [LabelText("左左")]
            LeftLeft,

            [LabelText("左右")]
            LeftRight,

            [LabelText("右左")]
            RightLeft,

            [LabelText("右右")]
            RightRight,
        }

        public TitlePositionType positionType;

        public Color color = Color.white;

        public Texture icon;
        public SdfIconType sdfIcon;

        public string text;

        public int priority;
        public string commandName;
    }
}