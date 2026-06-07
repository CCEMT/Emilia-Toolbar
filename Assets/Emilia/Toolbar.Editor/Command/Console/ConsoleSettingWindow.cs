using Emilia.Kit.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace Emilia.Toolbar.Editor
{
    public class ConsoleSettingWindow : OdinEditorWindow
    {
        [HideLabel, HideReferenceObjectPicker, OnValueChanged(nameof(OnChange), true)]
        public ConsoleSetting setting;

        [MenuItem("Emilia/Toolbar/Setting/Console")]
        public static void Open()
        {
            EditorImGUIKit.OpenWindow<ConsoleSettingWindow>("Toolbar Setting", 400, 400);
        }

        public void OnChange()
        {
            ConsoleSetting.Save();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.setting = ConsoleSetting.instance;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            ConsoleSetting.Save();
        }
    }
}