using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace CursorDashTests
{
    // Re-register after EnterPlayMode/ExitPlayMode domain reloads so completed
    // native results remain available even if an external job tracker loses callbacks.
    [InitializeOnLoad]
    internal static class CursorDashTestReport
    {
        private static TestRunnerApi api;
        static CursorDashTestReport() => Register();

        private static void Register()
        {
            api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new Reporter());
        }

        [MenuItem("Tools/CursorDash Remake/Run Scene Regression")]
        private static void Run()
        {
            if (EditorApplication.isPlaying)
                throw new InvalidOperationException("Wait until the editor and test runner are idle.");
            if (api == null) Register();
            api.Execute(new ExecutionSettings(new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { "CursorDash.EditorTests" }
            }));
        }

        [Serializable]
        private sealed class Report
        {
            public string finishedUtc;
            public string state;
            public int passed, failed, skipped;
            public double durationSeconds;
            public List<Case> cases = new List<Case>();
        }
        [Serializable]
        private sealed class Case
        {
            public string name, state, message, stackTrace;
            public double durationSeconds;
        }
        private sealed class Reporter : ICallbacks
        {
            public void RunStarted(ITestAdaptor tests) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
            public void RunFinished(ITestResultAdaptor result)
            {
                var report = new Report
                {
                    finishedUtc = DateTime.UtcNow.ToString("O"),
                    state = result.ResultState,
                    passed = result.PassCount,
                    failed = result.FailCount,
                    skipped = result.SkipCount,
                    durationSeconds = result.Duration
                };
                Collect(result, report.cases);
                if (!report.cases.Exists(item => item.name.StartsWith("CursorDashTests."))) return;
                string directory = Path.GetFullPath("Logs");
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "CursorDash-regression.json"), JsonUtility.ToJson(report, true));
                TestRunnerApi.SaveResultToFile(result, Path.Combine(directory, "CursorDash-regression.xml"));
                Debug.Log($"CursorDash regression finished: {report.passed} passed, {report.failed} failed. Logs/CursorDash-regression.json");
            }
            private static void Collect(ITestResultAdaptor result, List<Case> cases)
            {
                if (result.HasChildren)
                {
                    foreach (ITestResultAdaptor child in result.Children) Collect(child, cases);
                    return;
                }
                cases.Add(new Case { name = result.FullName, state = result.ResultState,
                    durationSeconds = result.Duration, message = result.Message, stackTrace = result.StackTrace });
            }
        }
    }
}
