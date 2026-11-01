#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CursorDashTests
{
    public sealed class CursorDashMainLogoViewTests
    {
        private const string PrefabPath = "Assets/Prefabs/UI/MainLogo.prefab";
        private const string ScenePath = "Assets/Scenes/Cyberpunk.unity";
        private const string Prefix = "CursorDash.LogoViewReview.";
        private static readonly string[] StringPrefs = { "CursorDash.v2.user", "CursorDash.v2.presets", "CursorDash.v2.tutorial", "CursorDash.v3.feverGuide", "CursorDash_BestPoints_v1_Cyberpunk" };
        private static Type Runtime(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
        private static Component Find(string name) => Object.FindFirstObjectByType(Runtime(name)) as Component;
        private static object Get(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
        private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name).Invoke(target, args);
        private static Component View() => Find("CursorDashMainLogoView");
        private static RectTransform Placement(Component view) => (RectTransform)Get(view, "PlacementRoot");
        private static RectTransform Animation(Component view) => (RectTransform)Get(view, "AnimationRoot");
        private static Image Artwork(Component view) => (Image)Get(view, "Artwork");

        [SetUp]
        public void OpenSavedTargetWhenTestRunnerUsesItsEmptyScene()
        {
            if (EditorApplication.isPlaying) return;
            for (int n = 0; n < SceneManager.sceneCount; n++)
                Assert.IsFalse(SceneManager.GetSceneAt(n).isDirty, "User scene changes must not be saved or discarded by the test.");
            if (SceneManager.GetActiveScene().path != ScenePath) EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        [Test]
        public void SavedPrefabHasOriginalArtworkAndExplicitNonInteractiveCanvas()
        {
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(root);
            var view = root.GetComponent(Runtime("CursorDashMainLogoView"));
            Assert.IsTrue((bool)Get(view, "IsConfigured"));
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, root.GetComponent<Canvas>().renderMode);
            Assert.AreEqual(25, root.GetComponent<Canvas>().sortingOrder);
            var scaler = root.GetComponent<CanvasScaler>();
            Assert.AreEqual(new Vector2(1080, 1920), scaler.referenceResolution);
            Assert.AreEqual(.5f, scaler.matchWidthOrHeight);
            Assert.AreEqual(new Vector2(.5f, .79f), Placement(view).anchorMin);
            Assert.AreEqual(new Vector2(840, 840 * 280f / 994), Placement(view).sizeDelta);
            Assert.AreEqual("Assets/Art/UI/Branding/Logo.png", AssetDatabase.GetAssetPath(Artwork(view).sprite));
            Assert.AreEqual(Color.white, Artwork(view).color);
            Assert.IsFalse(Artwork(view).raycastTarget);
            Assert.IsNull(root.GetComponent<GraphicRaycaster>());
            Assert.IsFalse(Animation(view).GetComponent<CanvasGroup>().blocksRaycasts);
            Assert.AreEqual(0, Artwork(view).gameObject.layer);
        }

        [Test]
        public void RelativePoseNeverRewritesAuthoredPlacementOrImage()
        {
            var root = PrefabUtility.LoadPrefabContents(PrefabPath);
            try
            {
                var view = root.GetComponent(Runtime("CursorDashMainLogoView"));
                var placement = Placement(view); var art = Artwork(view);
                placement.anchoredPosition = new Vector2(37, -26);
                placement.sizeDelta = new Vector2(710, 215);
                placement.localScale = new Vector3(.9f, 1.1f, 1);
                art.color = new Color(.8f, .9f, 1, .7f);
                for (int n = 0; n <= 10; n++) Call(view, "Present", n / 10f, 1 - n / 10f);
                Assert.AreEqual(new Vector2(37, -26), placement.anchoredPosition);
                Assert.AreEqual(new Vector2(710, 215), placement.sizeDelta);
                Assert.AreEqual(new Vector3(.9f, 1.1f, 1), placement.localScale);
                Assert.AreEqual(new Color(.8f, .9f, 1, .7f), art.color);
                Assert.AreEqual(new Vector2(150, 0), Animation(view).anchoredPosition);
                Assert.AreEqual(0, Animation(view).GetComponent<CanvasGroup>().alpha);
                Call(view, "Present", 0f, 1f);
                Assert.AreEqual(Vector2.zero, Animation(view).anchoredPosition);
                Assert.AreEqual(1, Animation(view).GetComponent<CanvasGroup>().alpha);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [Test]
        public void CyberpunkUsesOneSavedExplicitLogoReference()
        {
            Assert.AreEqual(ScenePath, SceneManager.GetActiveScene().path);
            Assert.IsFalse(EditorApplication.isPlaying);
            var views = Object.FindObjectsByType(Runtime("CursorDashMainLogoView"), FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(1, views.Length);
            Assert.AreSame(views[0], Get(Find("StageManager"), "MainLogoView"));
            Assert.AreEqual(PrefabPath, PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot((Component)views[0]));
            Assert.IsTrue(((Component)views[0]).gameObject.activeInHierarchy);
            Assert.AreEqual(Vector3.one, Placement((Component)views[0]).localScale);
        }

        private static void CaptureBeforePlay()
        {
            Assert.AreEqual(ScenePath, SceneManager.GetActiveScene().path);
            Assert.IsFalse(SceneManager.GetActiveScene().isDirty, "Do not save or discard user changes.");
            var view = View(); var placement = Placement(view); var image = Artwork(view);
            SessionState.SetString(Prefix + "rect", EditorJsonUtility.ToJson(placement));
            SessionState.SetString(Prefix + "image", EditorJsonUtility.ToJson(image));
            foreach (string key in StringPrefs)
            {
                SessionState.SetBool(Prefix + key + ".exists", PlayerPrefs.HasKey(key));
                SessionState.SetString(Prefix + key, PlayerPrefs.GetString(key));
            }
            SessionState.SetBool(Prefix + "captured", true);
        }

        [UnityTearDown]
        public IEnumerator RestoreOnlyCapturedLogoAndPreferences()
        {
            if (!SessionState.GetBool(Prefix + "captured", false)) yield break;
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            var view = View();
            EditorJsonUtility.FromJsonOverwrite(SessionState.GetString(Prefix + "rect", ""), Placement(view));
            EditorJsonUtility.FromJsonOverwrite(SessionState.GetString(Prefix + "image", ""), Artwork(view));
            PrefabUtility.RecordPrefabInstancePropertyModifications(Placement(view));
            PrefabUtility.RecordPrefabInstancePropertyModifications(Artwork(view));
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Assert.IsTrue(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()));
            foreach (string key in StringPrefs)
                if (SessionState.GetBool(Prefix + key + ".exists", false)) PlayerPrefs.SetString(key, SessionState.GetString(Prefix + key, ""));
                else PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            SessionState.SetBool(Prefix + "captured", false);
        }

        [UnityTest]
        public IEnumerator EditedPositionSizeAndImageSurvivePlayAndRepeatedHome()
        {
            CaptureBeforePlay();
            var view = View(); var placement = Placement(view); var image = Artwork(view);
            var alternate = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/TutorialPages/TIP_BUTTON_RIght.png");
            Assert.IsNotNull(alternate, "Use an existing sprite temporarily; never change its source asset.");
            placement.anchoredPosition = new Vector2(31, -26); placement.sizeDelta = new Vector2(720, 203);
            image.sprite = alternate; image.color = new Color(.9f, .85f, 1, .8f);
            PrefabUtility.RecordPrefabInstancePropertyModifications(placement); PrefabUtility.RecordPrefabInstancePropertyModifications(image);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); Assert.IsTrue(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()));
            yield return new EnterPlayMode(); yield return null; yield return null;
            view = View(); placement = Placement(view); image = Artwork(view);
            var stage = Find("StageManager"); var flow = Get(stage, "Space");
            for (int n = 0; n < 2; n++)
            {
                Assert.AreEqual(new Vector2(31, -26), placement.anchoredPosition);
                Assert.AreEqual(new Vector2(720, 203), placement.sizeDelta);
                Assert.AreEqual("Assets/Art/UI/TutorialPages/TIP_BUTTON_RIght.png", AssetDatabase.GetAssetPath(image.sprite));
                Assert.AreEqual(new Color(.9f, .85f, 1, .8f), image.color);
                Assert.IsTrue((bool)Call(flow, "Begin", true, Vector2.up));
                yield return null; Call(flow, "Home"); yield return null;
                Assert.AreEqual(Vector2.zero, Animation(view).anchoredPosition);
                Assert.AreEqual(1, Animation(view).GetComponent<CanvasGroup>().alpha);
            }
            Assert.IsNull(GameObject.Find("Shared-space main UI/Existing title logo"), "The saved view replaces the runtime logo, not the other menu UI.");
            yield return new ExitPlayMode();
            Assert.AreEqual(new Vector2(31, -26), Placement(View()).anchoredPosition);
            Assert.AreEqual(new Vector2(720, 203), Placement(View()).sizeDelta);
            Assert.AreEqual("Assets/Art/UI/TutorialPages/TIP_BUTTON_RIght.png", AssetDatabase.GetAssetPath(Artwork(View()).sprite));
        }

        [UnityTest]
        public IEnumerator NativeStartAndReturnUseExistingClockWithoutDuplicateLogo()
        {
            CaptureBeforePlay(); yield return new EnterPlayMode(); yield return null; yield return null;
            var stage = Find("StageManager"); var flow = Get(stage, "Space"); var view = View();
            Assert.IsTrue((bool)Call(flow, "Begin", false, Vector2.up));
            Assert.IsFalse((bool)Call(flow, "Begin", false, Vector2.up), "Repeated start remains rejected.");
            float deadline = Time.realtimeSinceStartup + 5; int measured = 0;
            while ((bool)Get(flow, "IsStarting"))
            {
                Assert.Less(Time.realtimeSinceStartup, deadline);
                yield return null;
                if (!(bool)Get(flow, "IsStarting")) break;
                object clock = flow.GetType().GetField("timeline", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(flow);
                float travel = Convert.ToSingle(Get(clock, "LogoTravel")), alpha = Convert.ToSingle(Get(clock, "LogoAlpha"));
                Assert.That(Animation(view).anchoredPosition.x, Is.EqualTo(150 * travel).Within(.001));
                Assert.That(Animation(view).GetComponent<CanvasGroup>().alpha, Is.EqualTo(alpha).Within(.001));
                Assert.AreEqual(new Vector2(840, 840 * 280f / 994), Placement(view).sizeDelta);
                measured++;
            }
            Assert.Greater(measured, 1); Assert.IsFalse(Animation(view).gameObject.activeSelf);
            Call(stage, "SetPaused", true); Call(flow, "Home"); yield return null;
            Assert.IsTrue(Animation(view).gameObject.activeSelf);
            Assert.AreEqual(Vector2.zero, Animation(view).anchoredPosition);
            Assert.AreEqual(1, Animation(view).GetComponent<CanvasGroup>().alpha);
            // Exercise the existing result acknowledgement/reveal without score settlement or death.
            Call(flow, "ReturnFromRound", "0", false);
            float returnDeadline = Time.realtimeSinceStartup + 5;
            while (!(bool)Get(flow, "AwaitingReturnClick")) { Assert.Less(Time.realtimeSinceStartup, returnDeadline); yield return null; }
            yield return null;
            Assert.IsTrue((bool)Call(flow, "RequestReturnContinue", new Vector2(Screen.width * .95f, Screen.height * .5f)));
            while ((bool)Get(flow, "IsReturning")) { Assert.Less(Time.realtimeSinceStartup, returnDeadline); yield return null; }
            Assert.AreEqual(Vector2.zero, Animation(view).anchoredPosition);
            Assert.AreEqual(1, Animation(view).GetComponent<CanvasGroup>().alpha);
            Assert.AreEqual(1, Object.FindObjectsByType(Runtime("CursorDashMainLogoView"), FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
            Assert.IsNull(GameObject.Find("Shared-space main UI/Existing title logo"));
        }
    }
}
#endif
