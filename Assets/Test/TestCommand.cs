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

        [Command("Test1", "打开资源", "测试")]
        public static void Test1() { }
        
        [Command("Test2", "打开资源", "测试")]
        public static void Test2() { }
        [Command("Test3", "打开资源", "测试")]
        public static void Test3() { }
        [Command("Test4", "打开资源", "测试")]
        public static void Test4() { }
        [Command("Test5", "打开资源", "测试")]
        public static void Test5() { }
        [Command("Test6", "打开资源", "测试")]
        public static void Test6() { }
        [Command("Test7", "打开资源", "测试")]
        public static void Test7() { }
        [Command("Test8", "打开资源", "测试")]
        public static void Test8() { }
        [Command("Test9", "打开资源", "测试")]
        public static void Test9() { }
        [Command("Test10", "打开资源", "测试")]
        public static void Test10() { }
        [Command("Test11", "打开资源", "测试")]
        public static void Test11() { }
        [Command("Test12", "打开资源", "测试")]
        public static void Test12() { }
        [Command("Test13", "打开资源", "测试")]
        public static void Test13() { }
        [Command("Test14", "打开资源", "测试")]
        public static void Test14() { }
        [Command("Test15", "打开资源", "测试")]
        public static void Test15() { }
        [Command("Test16", "打开资源", "测试")]
        public static void Test16() { }
        [Command("Test17", "打开资源", "测试")]
        public static void Test17() { }
        [Command("Test18", "打开资源", "测试")]
        public static void Test18() { }
        [Command("Test19", "打开资源", "测试")]
        public static void Test19() { }
        [Command("Test20", "打开资源", "测试")]
        public static void Test20() { }
    }
}