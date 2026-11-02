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
    public sealed class CursorDashSceneScreenTests
    {
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
        private string copy;
        private EditorWindow window;
        private static Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Get(object o,string name)=>o.GetType().GetProperty(name,Flags).GetValue(o);
        private static void Stop()=>T("CursorDashUIAuthoringPreview").GetMethod("Stop").Invoke(null,null);
        private static string Prefs()=>string.Join("|",new[]{"CursorDash.v2.user","CursorDash.v2.presets","CursorDash.v2.tutorial","CursorDash.v3.feverGuide","CursorDash_BestPoints_v1_Cyberpunk"}.Select(k=>PlayerPrefs.HasKey(k)+PlayerPrefs.GetString(k,"")));
        private string prefs;
        [SetUp]public void SetUp()
        {
            Stop();Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);Assert.IsNull(PrefabStageUtility.GetCurrentPrefabStage());
            Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Scenes/Cyberpunk.unity")EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity");
            prefs=Prefs();
        }
        [TearDown]public void TearDown()
        {
            Stop();if(window!=null)window.Close();
            if(copy!=null){Assert.AreEqual(copy,UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity");AssetDatabase.DeleteAsset(copy);copy=null;}
            Assert.AreEqual(prefs,Prefs());Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
        }
        [Test]public void AllStateButtonsUseRealSceneSourcesWithoutRunningGameOrLeakingPresentation()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();int roots=scene.rootCount;
            var stage=Object.FindFirstObjectByType(T("StageManager"));var score=(Component)Get(stage,"ProjectedScoreView");var text=(Component)Get(score,"Artwork");
            var data=new SerializedObject(text);var oldText=data.FindProperty("m_text").stringValue;var oldPos=text.transform.position;
            var logo=(Component)Get(stage,"MainLogoView");var baseLogo=((RectTransform)Get(logo,"PlacementRoot")).anchoredPosition;
            window=(EditorWindow)T("CursorDashScreenAuthoringWindow").GetMethod("Open").Invoke(null,null);
            window.GetType().GetField("animateBackground",Flags).SetValue(window,false);
            foreach(bool wide in new[]{false,true})foreach(string state in new[]{"Main","MainSettings","InGame","InGamePause","Returning","Tutorial","FeverNotice"})
            {
                window.GetType().GetMethod("SelectScreen").Invoke(window,new[]{stage,Enum.Parse(T("CursorDashScreenAuthoringWindow+Screen"),state),(object)wide,"100",false});
                Assert.IsTrue((bool)T("CursorDashUIAuthoringPreview").GetProperty("ScreenActive").GetValue(null));
                Assert.AreEqual("100",Get(text,"text"));Assert.IsNull(Get(stage,"Space"));Assert.IsNull(Get(stage,"Fever"));
                Assert.IsFalse(scene.isDirty);Assert.AreEqual(baseLogo,((RectTransform)Get(logo,"PlacementRoot")).anchoredPosition);
                Stop();Assert.AreEqual(roots,scene.rootCount);Assert.AreEqual(oldText,Get(text,"text"));Assert.AreEqual(oldPos,text.transform.position);
                foreach(var canvas in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Canvas>(true)))Assert.AreNotEqual(RenderMode.WorldSpace,canvas.renderMode);
            }
        }
        [Test]public void ActualSceneSaveKeepsDesignerEditsAndExcludesTemporaryPresentation()
        {
            copy="Assets/Tests/Editor/SceneScreenFixture_"+Guid.NewGuid().ToString("N")+".unity";Assert.IsTrue(AssetDatabase.CopyAsset("Assets/Scenes/Cyberpunk.unity",copy));
            var scene=EditorSceneManager.OpenScene(copy);var stage=Object.FindFirstObjectByType(T("StageManager"));
            var view=(Component)Get(stage,"SettingsView");var layout=new SerializedObject(view);var placement=(RectTransform)layout.FindProperty("contentPlacement").objectReferenceValue;
            var expected=placement.anchoredPosition+new Vector2(17,-11);
            T("CursorDashUIAuthoringPreview").GetMethod("ShowScreen").Invoke(null,new[]{stage,Enum.Parse(T("CursorDashScreenAuthoringWindow+Screen"),"MainSettings"),1080,1920,"99",1f,.5f,.6f,0f,0f,false,false});
            var label=((Component)Get(view,"ContentAnimation")).GetComponentsInChildren(T("TMPro.TMP_Text"),true).First();var size=T("TMPro.TMP_Text").GetProperty("fontSize");float expectedFont=(float)size.GetValue(label)+2;
            var image=(UnityEngine.UI.Image)Get(view,"SheetSurface");var expectedTint=new Color(.05f,.08f,.12f,.95f);
            Undo.RecordObjects(new Object[]{placement,label,image},"Scene appearance edit");placement.anchoredPosition=expected;size.SetValue(label,expectedFont);image.color=expectedTint;Undo.FlushUndoRecordObjects();Undo.PerformUndo();Undo.PerformRedo();Assert.AreEqual(expected,placement.anchoredPosition);
            EditorSceneManager.MarkSceneDirty(scene);Assert.IsTrue(EditorSceneManager.SaveScene(scene));Stop();
            scene=EditorSceneManager.OpenScene(copy);stage=Object.FindFirstObjectByType(T("StageManager"));view=(Component)Get(stage,"SettingsView");layout=new SerializedObject(view);placement=(RectTransform)layout.FindProperty("contentPlacement").objectReferenceValue;
            Assert.AreEqual(expected,placement.anchoredPosition);Assert.AreEqual(RenderMode.ScreenSpaceOverlay,((Canvas)Get(view,"Canvas")).renderMode);
            label=((Component)Get(view,"ContentAnimation")).GetComponentsInChildren(T("TMPro.TMP_Text"),true).First();Assert.AreEqual(expectedFont,size.GetValue(label));Assert.AreEqual(expectedTint,((UnityEngine.UI.Image)Get(view,"SheetSurface")).color);
            Assert.IsFalse(scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Any(t=>t.name.StartsWith("EDITOR ONLY")||t.name.StartsWith("PREVIEW ONLY")));
            Assert.IsFalse(scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true)).Any(r=>r.sharedMaterial!=null&&(r.sharedMaterial.hideFlags&HideFlags.DontSave)!=0));
            Assert.AreEqual("0",Get(Get(Get(stage,"ProjectedScoreView"),"Artwork"),"text"));
        }
        [Test]public void SavedBoardUsesRuntimeStartupInputsAndSeparatesActuallyUsedFlashFromLegacyDecoration()
        {
            var stage=Object.FindFirstObjectByType(T("StageManager"));var board=(Component)Get(stage,"BoardView");Assert.IsTrue((bool)Get(board,"IsConfigured"));
            var renderer=(MeshRenderer)Get(board,"ColorField");var expected=new Material(renderer.sharedMaterial.shader);
            try{
                T("CursorDashLivingBackground").GetMethod("ApplyFrame").Invoke(null,new[]{expected,Get(board,"Profile"),0f,0f,Vector2.zero});
                foreach(string key in new[]{"_Navy","_Teal","_Violet","_HitDrift"})Assert.AreEqual(expected.GetVector(key),renderer.sharedMaterial.GetVector(key));
                foreach(string key in new[]{"_Clock","_Mix","_Bend","_FlowSpeed","_HitColor"})Assert.AreEqual(expected.GetFloat(key),renderer.sharedMaterial.GetFloat(key));
            }finally{Object.DestroyImmediate(expected);}
            Assert.IsNull(GameObject.Find("Cyberpunk_Background"),"The replaced empty static-background object was removed from this authored scene.");
            var anchor=(UnityEngine.UI.Image)Get(board,"DecorationAnchor");Assert.IsNull(anchor.sprite);Assert.IsFalse(anchor.enabled);
            Assert.IsNotNull(Get(board,"ComparisonDecoration"));
            Assert.IsNotNull(GameObject.Find("Flash/FlashImage").GetComponent<UnityEngine.UI.Image>().sprite,"Approved success flash is genuinely used and must be preserved.");
            Assert.AreEqual("CursorDash/NexusEnergy",((UnityEngine.UI.Image)Get(stage,"NexusBodySource")).material.shader.name);
        }
        [UnityTest]public IEnumerator SceneViewCanRemainVisibleAcrossEditorUpdatesWithoutPersistingPreviewValues()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();var stage=Object.FindFirstObjectByType(T("StageManager"));
            window=(EditorWindow)T("CursorDashScreenAuthoringWindow").GetMethod("Open").Invoke(null,null);
            foreach(string state in new[]{"MainSettings","InGamePause","Tutorial","FeverNotice","Main"}){
                window.GetType().GetMethod("SelectScreen").Invoke(window,new[]{stage,Enum.Parse(T("CursorDashScreenAuthoringWindow+Screen"),state),(object)false,"100",false});
                for(int i=0;i<20;i++)yield return null;
                Stop();Assert.IsFalse(scene.isDirty,state+" left a persistent preview edit");
            }
        }
    }
    public partial class CursorDashSceneRegression
    {
        [UnityTest]public IEnumerator UI_ActiveBoardUsesProceduralBackgroundAndKeepsLegacyPNGsOutOfFinalComposition()
        {
            var stage=Find("StageManager");var living=Find("CursorDashLivingBackground");var distortion=Get(Get(stage,"Feedback"),"Distortion");var legacy=Get(distortion,"OriginalBackground");
            Assert.IsTrue((bool)Get(legacy,"Ready"));Assert.IsFalse(((UnityEngine.UI.Image)Get(legacy,"SourceImage")).enabled);
            Assert.IsTrue((bool)Get(Get(Get(stage,"Fever"),"Circuits"),"Active"));
            Assert.IsNull(GameObject.Find("Cyberpunk_Background"));
            var bound=(Component)Get(stage,"BoardView");Assert.IsTrue((bool)Get(bound,"IsConfigured"));Assert.AreSame(((MeshRenderer)Get(bound,"ColorField")).gameObject,Get(living,"quad"));
            Assert.IsNull(((UnityEngine.UI.Image)Get(legacy,"SourceImage")).sprite);Assert.IsNotNull(Get(legacy,"SourceSprite"));
            var material=(Material)Get(living,"material");var expected=new Material(material.shader);
            try{
                TypeFor("CursorDashLivingBackground").GetMethod("ApplyFrame").Invoke(null,new[]{expected,Get(living,"profile"),Get(living,"Clock"),Get(living,"hitEnergy"),Get(living,"drift")});
                foreach(string key in new[]{"_Navy","_Teal","_Violet"})Assert.AreEqual(expected.GetVector(key),material.GetVector(key));
                foreach(string key in new[]{"_Mix","_Bend","_FlowSpeed","_Clock"})Assert.AreEqual(expected.GetFloat(key),material.GetFloat(key));
            }finally{Object.Destroy(expected);}
            System.IO.Directory.CreateDirectory("Logs/scene-screen-authoring-20261102");ScreenCapture.CaptureScreenshot("Logs/scene-screen-authoring-20261102/board-runtime.png");yield return WaitRealtime(.2f);
            var circuits=Get(Get(stage,"Fever"),"Circuits");Set(circuits,"Enabled",false);yield return null;yield return null;
            Assert.IsFalse((bool)Get(circuits,"Active"));Assert.IsTrue((bool)Get(legacy,"CanComposite"),"Explicit PNG comparison must still have the old decoration and anchor.");
            Set(circuits,"Enabled",true);yield return null;Assert.IsTrue((bool)Get(circuits,"Active"));
        }
    }
}
#endif
