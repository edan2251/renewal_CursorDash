using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        [MenuItem("Tools/CursorDash Remake/Run Ghost Guide Focused")]
        private static void RunGhostGuide()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Requires idle target Editor; no forced Play stop.");
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("User scene is unsaved; guide checks did not save or discard it.");
            if(api==null)Register();
            CaptureGuideState("before");SessionState.SetBool("CursorDash.GuideFocused",true);
            api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashGuideTests",
                "CursorDashTests.CursorDashSceneRegression.CP3_ConcurrentTutorialCountsOnlyRealContactsAndReturnsThenStartsCleanCombat",
                "CursorDashTests.CursorDashSceneRegression.CP3_AutonomousGhostUsesPhysicsContactAndActualReflectedReturnsWithoutPlayerRewards",
                "CursorDashTests.CursorDashSceneRegression.CP3_TutorialOwnershipRejectsOtherPlayerContactAndTrailAndRetryStartsFresh",
                "CursorDashTests.CursorDashSceneRegression.CP3_SingleAlternatingPracticeWaitsAfterMissAndShowsProtectedNexusFeedback",
                "CursorDashTests.CursorDashSceneRegression.CP3_FeverNoticeAfterBuiltWorldPreviewsNodesAndCleansHomeRetry",
                "CursorDashTests.CursorDashSceneRegression.CP3_PanelOwnsInputAndPreservesIndependentPause"}}));
        }
        [MenuItem("Tools/CursorDash Remake/Run Settings UI Focused")]
        private static void RunSettingsUI()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Requires idle target Editor; no forced Play stop.");
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("User scene is unsaved; UI checks did not save or discard it.");
            if(api==null)Register();SessionState.SetString("CursorDash.FocusedOutput","Logs/settings-input-fix-20261028");
            CaptureGuideState("before");SessionState.SetBool("CursorDash.GuideFocused",true);
            api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashSceneRegression.UI_MainSheetRealPointerEventsBlockStartAndSaveOnlyAtGestureEnd",
                "CursorDashTests.CursorDashSceneRegression.UI_MasterAndReductionAreSingleMultipliersAndSelectiveResetProtectsRecords",
                "CursorDashTests.CursorDashSceneRegression.UI_PausePreservesStateAndSharedSlidersRetryHomeUseExistingFlow",
                "CursorDashTests.CursorDashSceneRegression.UI_TutorialReentryDoesNotEraseFlagsAndZeroEffectsKeepFeverGuideWhite",
                "CursorDashTests.CursorDashSceneRegression.CP3_ConcurrentTutorialCountsOnlyRealContactsAndReturnsThenStartsCleanCombat",
                "CursorDashTests.CursorDashSceneRegression.CP3_FeverNoticeAfterBuiltWorldPreviewsNodesAndCleansHomeRetry",
                "CursorDashTests.CursorDashSceneRegression.CP3_PanelOwnsInputAndPreservesIndependentPause"}}));
        }
        [Serializable] private sealed class GuideState {public bool playing;public float timeScale;public string[] scenes,dirty,preferenceHashes;}
        [MenuItem("Tools/CursorDash Remake/Run Settings Pause Recheck")]
        private static void RecheckSettingsPause()
        {
            if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)throw new InvalidOperationException("Requires idle target Editor.");
            for(int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)if(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("User scene is unsaved; no save or discard.");
            if(api==null)Register();SessionState.SetString("CursorDash.FocusedOutput","Logs/settings-input-fix-20261028");CaptureGuideState("before");SessionState.SetBool("CursorDash.GuideFocused",true);
            api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashSceneRegression.UI_PausePreservesStateAndSharedSlidersRetryHomeUseExistingFlow",
                "CursorDashTests.CursorDashSceneRegression.UI_TutorialReentryDoesNotEraseFlagsAndZeroEffectsKeepFeverGuideWhite"}}));
        }
        private static void CaptureGuideState(string suffix)
        {
            var keys=new[]{"CursorDash.v2.tutorial","CursorDash.v3.feverGuide","CursorDash.v2.user","CursorDash.v2.presets","CursorDash_BestPoints_v1_Cyberpunk","CursorDash_BestPoints_v1_Flame"};
            string Hash(string key){using(var sha=System.Security.Cryptography.SHA256.Create())return key+"|"+PlayerPrefs.HasKey(key)+"|"+BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(PlayerPrefs.GetString(key,""))));}
            var snapshot=new GuideState{playing=EditorApplication.isPlaying,timeScale=Time.timeScale,
                scenes=Enumerable.Range(0,UnityEngine.SceneManagement.SceneManager.sceneCount).Select(i=>{var s=UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);return s.path+"|dirty="+s.isDirty;}).ToArray(),
                dirty=Resources.FindObjectsOfTypeAll<UnityEngine.Object>().Where(x=>x!=null&&EditorUtility.IsPersistent(x)&&EditorUtility.IsDirty(x)).Select(x=>x.GetType().Name+"|"+x.name+"|"+AssetDatabase.GetAssetPath(x)).OrderBy(x=>x).ToArray(),preferenceHashes=keys.Select(Hash).ToArray()};
            var folder=Path.GetFullPath(SessionState.GetString("CursorDash.FocusedOutput","Logs/tutorial-poweron-fever-order-20261028"));Directory.CreateDirectory(folder);File.WriteAllText(Path.Combine(folder,"native-"+suffix+".json"),JsonUtility.ToJson(snapshot,true));
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
                if(SessionState.GetBool("CursorDash.GuideFocused",false)){CaptureGuideState("after");SessionState.SetBool("CursorDash.GuideFocused",false);}
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
