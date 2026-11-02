#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    public sealed class CursorDashEditorUsabilityTests
    {
        private const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
        private string copy;
        private static Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Get(object o,string name)=>o.GetType().GetProperty(name).GetValue(o);
        private static void CallStatic(string type,string name,params object[] args)=>T(type).GetMethods().Single(m=>m.Name==name&&m.GetParameters().Length==args.Length&&m.GetParameters().Select((p,i)=>args[i]==null||p.ParameterType.IsInstanceOfType(args[i])).All(x=>x)).Invoke(null,args);
        private static string Prefs()=>string.Join("|",new[]{"CursorDash.v2.user","CursorDash.v2.presets","CursorDash.v2.tutorial","CursorDash.v3.feverGuide","CursorDash_BestPoints_v1_Cyberpunk"}.Select(k=>PlayerPrefs.HasKey(k)+PlayerPrefs.GetString(k,"")));
        private string prefs;
        [SetUp]public void SetUp()
        {
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);
            Assert.IsNull(PrefabStageUtility.GetCurrentPrefabStage(),"Do not close user prefab editing.");
            Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Cyberpunk.unity")EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity");
            prefs=Prefs();
        }
        [TearDown]public void TearDown()
        {
            CallStatic("CursorDashUIAuthoringPreview","Stop");
            var stage=PrefabStageUtility.GetCurrentPrefabStage();
            if(stage!=null){Assert.AreEqual(copy,stage.assetPath);stage.ClearDirtiness();StageUtility.GoToMainStage();}
            if(copy!=null){Assert.IsTrue(copy.StartsWith("Assets/Tests/Editor/UsabilityFixture_"));AssetDatabase.DeleteAsset(copy);copy=null;}
            Assert.AreEqual(prefs,Prefs());Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
        }
        private Component OpenCopy(string source,string component)
        {
            copy="Assets/Tests/Editor/UsabilityFixture_"+Guid.NewGuid().ToString("N")+".prefab";
            Assert.IsFalse(System.IO.File.Exists(copy));Assert.IsTrue(AssetDatabase.CopyAsset(source,copy));
            return PrefabStageUtility.OpenPrefab(copy).prefabContentsRoot.GetComponent(T(component));
        }
        [Test]public void SettingsAndPauseUseRealVisibleGeometryAndRestoreOnlyPresentation()
        {
            var view=OpenCopy("Assets/Prefabs/UI/SettingsView.prefab","CursorDashSettingsView");
            var canvas=(Canvas)Get(view,"Canvas");var data=new SerializedObject(view);var placement=(RectTransform)data.FindProperty("contentPlacement").objectReferenceValue;
            Assert.AreEqual(RenderMode.WorldSpace,canvas.renderMode);Assert.Greater(view.transform.lossyScale.x,0);
            Assert.Greater(((RectTransform)view.transform).rect.height,0);Assert.AreEqual(1,Get(Get(view,"ContentOpacity"),"alpha"));
            Assert.IsFalse(PrefabStageUtility.GetCurrentPrefabStage().scene.isDirty);
            var label=((Component)Get(view,"ContentAnimation")).GetComponentsInChildren(T("TMPro.TMP_Text"),true).First();
            var labelType=T("TMPro.TMP_Text");var fontProperty=labelType.GetProperty("font");var sizeProperty=labelType.GetProperty("fontSize");var textProperty=labelType.GetProperty("text");
            var originalText=textProperty.GetValue(label);var font=AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/CursorDash/Fonts/esamanru Bold View SDF.asset");
            var image=(UnityEngine.UI.Image)Get(view,"SheetSurface");
            var sprite=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/MainLogo.prefab").GetComponentInChildren<UnityEngine.UI.Image>(true).sprite;
            var before=placement.anchoredPosition;Undo.IncrementCurrentGroup();Undo.RecordObjects(new Object[]{placement,label,image},"Isolated editable UI");
            placement.anchoredPosition+=new Vector2(17,-13);sizeProperty.SetValue(label,32f);fontProperty.SetValue(label,font);textProperty.SetValue(label,"편집 확인");image.sprite=sprite;image.color=new Color(.1f,.15f,.2f,.9f);Undo.FlushUndoRecordObjects();
            CallStatic("CursorDashUIAuthoringPreview","Show",view,true);
            Assert.AreEqual(before+new Vector2(17,-13),placement.anchoredPosition);
            Assert.IsTrue(((CanvasGroup)Get(view,"PauseOpacity")).gameObject.activeSelf);
            Assert.AreEqual(1,((CanvasGroup)Get(view,"PauseOpacity")).alpha);
            CallStatic("CursorDashUIAuthoringPreview","Stop");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay,canvas.renderMode);Assert.AreEqual(before+new Vector2(17,-13),placement.anchoredPosition);
            Undo.PerformUndo();Assert.AreEqual(before,placement.anchoredPosition);Assert.AreEqual(originalText,textProperty.GetValue(label));Undo.PerformRedo();Assert.AreEqual(before+new Vector2(17,-13),placement.anchoredPosition);
            CallStatic("CursorDashUIAuthoringPreview","Show",view,false);
            Assert.IsTrue(EditorApplication.ExecuteMenuItem("File/Save"),"Use the actual Prefab Mode Save path while preview is active.");
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay,AssetDatabase.LoadAssetAtPath<GameObject>(copy).GetComponent<Canvas>().renderMode,"Transient Scene-view rendering must not be saved into the prefab.");
            PrefabStageUtility.GetCurrentPrefabStage().ClearDirtiness();StageUtility.GoToMainStage();
            var reopened=PrefabStageUtility.OpenPrefab(copy).prefabContentsRoot.GetComponent(T("CursorDashSettingsView"));
            Assert.AreEqual(before+new Vector2(17,-13),((RectTransform)new SerializedObject(reopened).FindProperty("contentPlacement").objectReferenceValue).anchoredPosition);
            Assert.Greater(reopened.transform.lossyScale.x,0);Assert.AreEqual(RenderMode.WorldSpace,((Canvas)Get(reopened,"Canvas")).renderMode);
            var savedLabel=((Component)Get(reopened,"ContentAnimation")).GetComponentsInChildren(T("TMPro.TMP_Text"),true).First();Assert.AreEqual("편집 확인",textProperty.GetValue(savedLabel));Assert.AreEqual(32f,sizeProperty.GetValue(savedLabel));Assert.AreEqual(font,fontProperty.GetValue(savedLabel));
            var savedImage=(UnityEngine.UI.Image)Get(reopened,"SheetSurface");Assert.AreEqual(sprite,savedImage.sprite);Assert.AreEqual(new Color(.1f,.15f,.2f,.9f),savedImage.color);
        }
        [Test]public void GuidanceGetsOnlyTemporaryCanvasAndKeepsEditedTypography()
        {
            var view=OpenCopy("Assets/Prefabs/UI/GuidanceView.prefab","CursorDashGuidanceView");
            var hint=(Component)Get(view,"Hint");var type=T("TMPro.TMP_Text");var size=type.GetProperty("fontSize");float original=(float)size.GetValue(hint);size.SetValue(hint,original+3);
            Assert.IsTrue((bool)Get(hint,"enabled"));Assert.IsNotNull(view.GetComponentInParent<Canvas>());
            Assert.AreEqual(RenderMode.WorldSpace,view.GetComponentInParent<Canvas>().renderMode);
            Assert.Greater(view.GetComponent<RectTransform>().rect.height,0);
            CallStatic("CursorDashUIAuthoringPreview","Show",view,true);Assert.IsTrue((bool)Get(Get(view,"Notice"),"enabled"));
            CallStatic("CursorDashUIAuthoringPreview","Stop");Assert.IsNull(view.GetComponent<Canvas>());Assert.AreEqual(original+3,size.GetValue(hint));
            Assert.IsFalse((bool)Get(hint,"enabled"));size.SetValue(hint,original);
        }
        [Test]public void SavedMenuAndScoreHaveVisibleDesignDefaults()
        {
            foreach(string path in new[]{"Assets/Prefabs/UI/MainMenuView.prefab","Assets/Prefabs/UI/SettingsView.prefab"})
            {
                var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);Assert.Greater(prefab.transform.localScale.x,0);Assert.Greater(prefab.GetComponent<RectTransform>().sizeDelta.y,0);Assert.IsTrue(prefab.GetComponent<Canvas>().enabled);
            }
            var score=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ProjectedScoreView.prefab");Assert.IsTrue(score.GetComponentInChildren<MeshRenderer>(true).enabled);
        }
        [Test]public void PreviewBackgroundUsesLiveFrameInputsInsteadOfFixedFourSeconds()
        {
            var stage=Object.FindFirstObjectByType(T("StageManager"));Assert.IsNotNull(stage);var window=(EditorWindow)T("CursorDashScreenAuthoringWindow").GetMethod("Open").Invoke(null,null);
            var profile=Resources.Load("CursorDash/VisualPrototype");var expected=new Material(Resources.Load<Shader>("CursorDash/LivingBackground"));
            try
            {
                var wt=window.GetType();wt.GetField("animateBackground",Flags).SetValue(window,false);wt.GetField("backgroundSeconds",Flags).SetValue(window,23f);
                wt.GetMethods().Single(m=>m.Name=="Show"&&m.GetParameters().Length==4).Invoke(window,new[]{stage,Enum.Parse(T("CursorDashScreenAuthoringWindow+Screen"),"Main"),(object)false,"131"});
                var material=(Material)wt.GetField("backgroundMaterial",Flags).GetValue(window);
                CallStatic("CursorDashLivingBackground","ApplyFrame",expected,profile,23f,0f,Vector2.zero);
                Assert.AreEqual(expected.shader,material.shader);
                foreach(string prop in new[]{"_Navy","_Teal","_Violet","_HitDrift"})Assert.AreEqual(expected.GetVector(prop),material.GetVector(prop));
                foreach(string prop in new[]{"_Clock","_FlowSpeed","_Mix","_Bend","_HitColor"})Assert.AreEqual(expected.GetFloat(prop),material.GetFloat(prop));
            }
            finally{window.GetType().GetMethod("Cleanup").Invoke(window,null);window.Close();Object.DestroyImmediate(expected);}
        }
    }

    public partial class CursorDashSceneRegression
    {
        [UnityTest]public IEnumerator UI_AuthoredStylesAndSharedBackgroundReachTheLiveMainAndSettings()
        {
            var stage=Find("StageManager");Assert.AreEqual("Main",Get(stage,"State").ToString());
            var living=Find("CursorDashLivingBackground");Assert.IsNotNull(living);var material=(Material)Get(living,"material");
            var profile=(Object)Get(living,"profile");var expected=new Material(material.shader);
            try
            {
                TypeFor("CursorDashLivingBackground").GetMethod("ApplyFrame").Invoke(null,new[]{expected,profile,Get(living,"Clock"),Get(living,"hitEnergy"),Get(living,"drift")});
                foreach(string prop in new[]{"_Navy","_Teal","_Violet","_HitDrift"})Assert.AreEqual(expected.GetVector(prop),material.GetVector(prop));
                foreach(string prop in new[]{"_Clock","_FlowSpeed","_Mix","_Bend","_HitColor"})Assert.AreEqual(expected.GetFloat(prop),material.GetFloat(prop));
            }
            finally{Object.Destroy(expected);}
            string dir=System.IO.Path.GetFullPath("Logs/editor-usability-20261102");System.IO.Directory.CreateDirectory(dir);ScreenCapture.CaptureScreenshot(dir+"/main-runtime.png");yield return WaitRealtime(.15f);
            var ui=(Component)Get(stage,"SettingsUI");var view=(Component)Get(stage,"SettingsView");
            var placement=(RectTransform)Get(view,"contentPlacement");var pos=placement.anchoredPosition;placement.anchoredPosition+=new Vector2(7,-9);var edited=placement.anchoredPosition;
            Call(ui,"SetSheet",true);yield return WaitRealtime(.3f);
            Assert.AreEqual(edited,placement.anchoredPosition);Assert.AreEqual(1,((CanvasGroup)Get(view,"ContentOpacity")).alpha);
            Assert.AreEqual(RenderMode.ScreenSpaceOverlay,((Canvas)Get(view,"Canvas")).renderMode);
            ScreenCapture.CaptureScreenshot(dir+"/settings-runtime.png");yield return WaitRealtime(.12f);
            Call(ui,"SetSheet",false);yield return WaitRealtime(.3f);Assert.AreEqual(edited,placement.anchoredPosition);
            placement.anchoredPosition=pos;Assert.AreEqual("Main",Get(stage,"State").ToString());
        }
    }
}
#endif
