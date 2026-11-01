#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CursorDashTests
{
    /// <summary>All tests stay in EditMode. No game bootstrap, PlayerPrefs writes, or EnterPlayMode.</summary>
    public sealed class CursorDashAuthoringTests
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static Type TypeFor(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
        private static Component Find(string name) => Object.FindFirstObjectByType(TypeFor(name)) as Component;
        private static object Get(object target, string name) => target.GetType().GetProperty(name).GetValue(target);
        private static void Set(object target, string name, object value) => target.GetType().GetProperty(name).SetValue(target, value);
        private static object Call(object target, string name, params object[] args) => target.GetType().GetMethods().Single(m=>m.Name==name&&m.GetParameters().Length==args.Length&&m.GetParameters().Select((p,n)=>args[n]==null||p.ParameterType.IsInstanceOfType(args[n])).All(ok=>ok)).Invoke(target, args);
        private object window;
        private string beforePrefs;
        private int rootCount;

        private static string Prefs() => string.Join("|", new[] { "CursorDash.v2.user", "CursorDash.v2.presets", "CursorDash.v2.tutorial", "CursorDash.v3.feverGuide", "CursorDash_BestPoints_v1_Cyberpunk" }.Select(k => k + ":" + PlayerPrefs.HasKey(k) + ":" + PlayerPrefs.GetString(k, "")));
        [SetUp]
        public void SetUp()
        {
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);
            for (int n = 0; n < SceneManager.sceneCount; n++) Assert.IsFalse(SceneManager.GetSceneAt(n).isDirty, "Do not save/discard user work.");
            if (SceneManager.GetActiveScene().path != "Assets/Scenes/Cyberpunk.unity") EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity", OpenSceneMode.Single);
            beforePrefs = Prefs(); rootCount = SceneManager.GetActiveScene().rootCount;
        }
        [TearDown]
        public void TearDown()
        {
            if (window != null) { Call(window, "Cleanup"); ((EditorWindow)window).Close(); window = null; }
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);
            Assert.AreEqual(beforePrefs, Prefs(), "Preview must not change any captured game preference.");
            Assert.AreEqual(rootCount, SceneManager.GetActiveScene().rootCount);
            Assert.IsFalse(SceneManager.GetActiveScene().isDirty, "Screen selection must not dirty the original scene.");
        }

        [Test]
        public void SceneReferencesAndSavedFontsAreExplicitAndComplete()
        {
            var stage = Find("StageManager");
            foreach (string name in new[] { "MainLogoView", "MainMenuView", "SettingsView", "ProjectedScoreView" })
            {
                var view = Get(stage, name); Assert.IsNotNull(view); Assert.IsTrue((bool)Get(view, "IsConfigured"));
                Assert.IsTrue(PrefabUtility.IsPartOfPrefabInstance((Component)view));
                foreach(var child in ((Component)view).GetComponentsInChildren<Transform>(true))Assert.AreEqual(0,GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject));
            }
            var settings = (Component)Get(stage, "SettingsView");
            var sliders = settings.GetComponentsInChildren<Slider>(true); Assert.AreEqual(9, sliders.Length);
            foreach (var slider in sliders) Assert.AreEqual(0, slider.onValueChanged.GetPersistentEventCount());
            foreach (var button in settings.GetComponentsInChildren<Button>(true)) Assert.AreEqual(0, button.onClick.GetPersistentEventCount());
            foreach (var view in new[] { (Component)Get(stage, "MainMenuView"), settings, (Component)Get(stage, "ProjectedScoreView") })
                foreach (var text in view.GetComponentsInChildren(TypeFor("TMPro.TMP_Text"), true))
                {
                    var font = (Object)Get(text, "font"); Assert.IsTrue(EditorUtility.IsPersistent(font));
                    AssertSameUnityAsset((Object)Get(font,"atlasTexture"), ((Material)Get(font,"material")).mainTexture);
                    Assert.IsTrue(AssetDatabase.GetAssetPath(font).Contains("esamanru"));
                }
            Assert.IsNull(Find("CursorDashSpaceFlow")); Assert.IsNull(Find("CursorDashSettingsUI"));
        }

        private static void AssertSameUnityAsset(Object expected, Object actual)
        {
            // Import/build can recreate managed wrappers for the same native subasset.
            // Verify the resource identity, rather than ReferenceEquals on those wrappers.
            Assert.IsNotNull(expected); Assert.IsNotNull(actual);
            Assert.IsTrue(expected == actual, "Font material must use the actual font atlas.");
            Assert.AreEqual(expected.GetInstanceID(), actual.GetInstanceID());
            string expectedGuid, actualGuid; long expectedId, actualId;
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(expected, out expectedGuid, out expectedId));
            Assert.IsTrue(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(actual, out actualGuid, out actualId));
            Assert.AreEqual(expectedGuid, actualGuid); Assert.AreEqual(expectedId, actualId);
        }

        [Test]
        public void MenuRelativePresentationPreservesAuthoredPoseTintAndText()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI/MainMenuView.prefab");
            try
            {
                var view = root.GetComponent(TypeFor("CursorDashMainMenuView"));
                var prompt = (RectTransform)Get(view, "PromptPlacement"); var caption = (RectTransform)Get(view, "CaptionPlacement");
                Call(view,"PrepareRendering"); // Private atlas for the isolated text-edit fixture too.
                prompt.anchoredPosition = new Vector2(13, -17); prompt.sizeDelta = new Vector2(850, 75); caption.anchoredPosition = new Vector2(-11, 9);
                var text = Get(view, "Prompt"); Set(text,"text","편집한 시작 문구"); Set(text,"color",new Color(.8f,.9f,1,.7f));
                Call(view, "PresentPrompt", .2f); Call(view, "PresentCaption", new Vector2(0, 100), false, true, false, .7f, .3f);
                Assert.AreEqual(new Vector2(13, -17), prompt.anchoredPosition); Assert.AreEqual(new Vector2(850, 75), prompt.sizeDelta);
                Assert.AreEqual(new Vector2(-11, 9), caption.anchoredPosition); Assert.AreEqual("편집한 시작 문구", Get(text,"text")); Assert.AreEqual(new Color(.8f, .9f, 1, .7f), Get(text,"color"));
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [Test]
        public void SettingsLegacyDimensionsAndKeysHaveNotChanged()
        {
            var view = (Component)Get(Find("StageManager"), "SettingsView");
            Assert.AreEqual(9,view.GetComponentsInChildren<Slider>(true).Length);
            foreach (var slider in view.GetComponentsInChildren<Slider>(true))
            {
                Assert.AreEqual(new Vector2(12, 12), slider.handleRect.sizeDelta);
                Assert.AreEqual(48, slider.GetComponent<RectTransform>().rect.height);
                Assert.AreEqual(4, slider.transform.Find("Track").GetComponent<RectTransform>().rect.height);
                Assert.AreEqual(0, slider.minValue); Assert.AreEqual(1, slider.maxValue);
            }
            var rows = ((Array)Get(view, "Rows")).Cast<object>().Select(r => (string)r.GetType().GetField("key").GetValue(r)).ToArray();
            CollectionAssert.AreEqual(new[] { "user.master", "user.bgm", "user.sfx", "user.vfxReduction", "user.shake", "user.flash", "user.distortion", "user.master", "user.vfxReduction" }, rows);
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay, ((Canvas)Get(view, "Canvas")).renderMode); Assert.AreEqual(90, ((Canvas)Get(view, "Canvas")).sortingOrder);
        }

        [Test]
        public void IsolatedPrefabUndoRedoKeepsTheAuthoredRootSeparate()
        {
            var root = PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI/MainMenuView.prefab");
            try
            {
                var view = root.GetComponent(TypeFor("CursorDashMainMenuView")); var rect = (RectTransform)Get(view, "PromptPlacement");
                Vector2 initial = rect.anchoredPosition; Undo.IncrementCurrentGroup(); Undo.SetCurrentGroupName("CursorDash isolated authoring check");
                Undo.RecordObject(rect, "Move authored prompt"); rect.anchoredPosition += new Vector2(27, 19); Undo.FlushUndoRecordObjects();
                Call(view, "PresentPrompt", .5f); Assert.AreEqual(initial + new Vector2(27, 19), rect.anchoredPosition);
                Undo.PerformUndo(); Assert.AreEqual(initial, rect.anchoredPosition);
                Undo.PerformRedo(); Assert.AreEqual(initial + new Vector2(27, 19), rect.anchoredPosition);
                Undo.PerformUndo();
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }

        [TestCase(false)] [TestCase(true)]
        public void PreviewSelectionAndSamplesDoNotRunGameOrDirtySources(bool landscape)
        {
            var stage = Find("StageManager"); var score = (Component)Get(stage, "ProjectedScoreView"); var artwork = Get(score, "Artwork");
            var font = Get(artwork,"font"); int glyphCount = ((ICollection)Get(font,"glyphTable")).Count; var atlas = Get(font,"atlasTexture");
            window = TypeFor("CursorDashScreenAuthoringWindow").GetMethod("Open").Invoke(null, null);
            var modes = TypeFor("CursorDashScreenAuthoringWindow+Screen");
            foreach (string name in new[] { "Main", "MainSettings", "InGame", "InGamePause", "Returning", "Tutorial", "FeverNotice", "Main" })
            {
                Call(window, "Show", stage, Enum.Parse(modes, name), landscape, "123456789012345");
                Assert.IsTrue((bool)Get(window, "HasPreview"));
                var scene = (Scene)window.GetType().GetField("previewScene", Flags).GetValue(window);
                foreach (var root in scene.GetRootGameObjects())
                {
                    Assert.IsEmpty(root.GetComponentsInChildren(TypeFor("StageManager"), true));
                    Assert.IsEmpty(root.GetComponentsInChildren(TypeFor("GameManager"), true));
                    Assert.IsEmpty(root.GetComponentsInChildren<UnityEngine.EventSystems.EventSystem>(true));
                    Assert.IsEmpty(root.GetComponentsInChildren<Collider2D>(true));
                }
                var target = (RenderTexture)Get(window, "PreviewTexture"); Assert.AreEqual(landscape ? 1920 : 1080, target.width); Assert.AreEqual(landscape ? 1080 : 1920, target.height);
                Assert.AreEqual("0", Get(artwork,"text")); Assert.AreSame(font, Get(artwork,"font")); Assert.AreSame(atlas, Get(font,"atlasTexture")); Assert.AreEqual(glyphCount, ((ICollection)Get(font,"glyphTable")).Count);
                Assert.IsFalse(SceneManager.GetActiveScene().isDirty);
            }
            Call(window, "Cleanup"); Assert.IsFalse((bool)Get(window, "HasPreview"));
        }

        [Test]
        public void OneNumericRendererHasTheApprovedRectFontLimitsAndSorting()
        {
            var view = (Component)Get(Find("StageManager"), "ProjectedScoreView");
            var text = (Component)Get(view, "Artwork"); var root = (RectTransform)Get(view, "PlacementRoot");
            Assert.AreEqual(1, view.GetComponentsInChildren(TypeFor("TMPro.TextMeshPro"),true).Length);
            Assert.AreEqual(new Vector2(4, 4), root.sizeDelta); Assert.AreEqual(Vector3.one, root.localScale);
            Assert.AreEqual(1, Get(text,"fontSizeMin")); Assert.AreEqual(3.5f, Get(text,"fontSizeMax")); Assert.IsTrue((bool)Get(text,"enableAutoSizing"));
            Assert.AreEqual(-900, text.GetComponent<MeshRenderer>().sortingOrder);
        }
    }
}
#endif
