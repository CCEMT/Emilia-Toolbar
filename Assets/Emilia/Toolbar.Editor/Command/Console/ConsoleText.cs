using UnityEngine;

namespace Emilia.Toolbar.Editor
{
    public static class ConsoleText
    {
        public static string TextField(string value, GUIStyle style, params GUILayoutOption[] options)
        {
            if (Event.current.type == EventType.KeyUp && Event.current.modifiers == EventModifiers.Control)
            {
                if (Event.current.keyCode == KeyCode.C)
                {
                    Event.current.Use();
                    TextEditor editor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    editor.Copy();
                }
                else if (Event.current.keyCode == KeyCode.V)
                {
                    Event.current.Use();
                    TextEditor editor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    return editor.text;
                }
                else if (Event.current.keyCode == KeyCode.A)
                {
                    Event.current.Use();
                    TextEditor editor = (TextEditor) GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                    editor.SelectAll();
                    return editor.text;
                }
            }

            return GUILayout.TextField(value, style, options);
        }
    }
}