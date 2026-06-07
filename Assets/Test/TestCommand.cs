using Emilia.Kit;
using Emilia.Toolbar.Editor;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Test
{
    public static class TestCommand
    {
        [Command("StartGame（开始游戏）", "开始游戏", "通用工具")]
        public static void StartGame()
        {
            EditorApplication.isPlaying = true;
        }

        [Command("ClearPlayerPrefs", "清除PlayerPrefs", "工具/Prefs", "CPP")]
        public static void ClearPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [Command("ClearEditorPrefs", "清理EditorPrefs", "工具/Prefs")]
        public static void ClearEditorPrefs()
        {
            EditorPrefs.DeleteAll();
            Debug.Log("EditorPrefs已清理");
        }

        [Command("ReCompilation", "重新编译", "工具", "RC")]
        public static void ReCompilation()
        {
            CompilationPipeline.RequestScriptCompilation();
        }

        [Command("LogTest", "测试日志输出", "测试")]
        public static void LogTest([Text("消息")] string message)
        {
            Debug.Log(message);
        }

        [Command("OpenAsset", "打开资源", "测试")]
        public static void OpenAsset([Text("资源")] Object asset)
        {
            AssetDatabase.OpenAsset(asset);
        }
    }
}