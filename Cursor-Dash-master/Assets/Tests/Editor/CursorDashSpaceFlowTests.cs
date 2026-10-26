#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using System.IO;

namespace CursorDashTests
{
    // Same reflection boundary as the existing suite: never add an Assembly-CSharp asmdef reference.
    public sealed class CursorDashSpaceFlowTests
    {
        [MenuItem("Tools/CursorDash/Run Shared Space Review")]
        public static void RunFocusedReview()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("대상 Editor가 편집 대기 상태여야 합니다.");
            for(int n=0;n<SceneManager.sceneCount;n++)if(SceneManager.GetSceneAt(n).isDirty)throw new InvalidOperationException("사용자 미저장 씬을 먼저 보존하세요. 저장·폐기하지 않습니다.");
            ScriptableObject.CreateInstance<TestRunnerApi>().Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashSpaceFlowTests","CursorDashTests.CursorDashCircuitTests",
                "CursorDashTests.CursorDashSceneRegression.ExplicitTrainingFromMainKeepsItsModeBeforeFirstGuide",
                "CursorDashTests.CursorDashSceneRegression.SharedSpaceStartsOnceAndHomeKeepsItsManagers",
                "CursorDashTests.CursorDashSceneRegression.ConfirmedHealthLossOwnsRedAndPauseClearsItsOptics",
                "CursorDashTests.CursorDashSceneRegression.SharedScoreAndDamagePresentationEvidence"}}));
        }
        private static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Call(object value,string name,params object[] args)=>value.GetType().GetMethod(name).Invoke(value,args);
        private static double Get(object value,string name)=>(double)value.GetType().GetProperty(name).GetValue(value);
        private static object Timeline()=>Activator.CreateInstance(Runtime("CursorDashStartTimeline"),new object[]{1.35d,.28d,.82d,.42d,.64d});
        [TestCase(.01)] [TestCase(.033)] [TestCase(.28)] [TestCase(4)]
        public void SkippedFramesReachOneConsistentFinalPose(double step)
        {
            var clock=Timeline();double previous=0;
            while(Get(clock,"Progress")<1){Call(clock,"Advance",step);double p=Get(clock,"Progress");Assert.GreaterOrEqual(p,previous);previous=p;}
            Assert.AreEqual(1,Get(clock,"Nexus"));Assert.AreEqual(1,Get(clock,"Cursor"));Assert.AreEqual(1,Get(clock,"Rebuild"));
            Assert.AreEqual(0,Get(clock,"LogoAlpha"));Assert.AreEqual(0,Get(clock,"BestAlpha"));Assert.AreEqual(1,Get(clock,"CurrentScoreAlpha"));
        }
        [Test] public void CircuitsWithdrawBeforeAnyRebuildAndOtherElementsOverlap()
        {
            var clock=Timeline();Call(clock,"Advance",1.35d*.28d);
            Assert.AreEqual(1,Get(clock,"Retreat"),1e-8);Assert.AreEqual(0,Get(clock,"Rebuild"),1e-8);
            Call(clock,"Advance",1.35d*.40d);
            Assert.Greater(Get(clock,"Rebuild"),0);Assert.Greater(Get(clock,"Nexus"),0);Assert.Greater(Get(clock,"Cursor"),0);
            Assert.Greater(Get(clock,"BestAlpha"),0);Assert.Greater(Get(clock,"CurrentScoreAlpha"),0);
        }
        [Test] public void RewindReadsFreshChargeAndRepeatedHitsNeverRestoreAnOldTarget()
        {
            var rewind=Activator.CreateInstance(Runtime("NexusGaugeRewind"));Call(rewind,"Begin",.875d,.16d);
            double a=(double)Call(rewind,"Tick",0d,.04d);Assert.Less(a,.875);Assert.Greater(a,0);
            Assert.AreEqual(a,Call(rewind,"Tick",0d,0d));
            double charged=(double)Call(rewind,"Tick",.125d,.04d);Assert.GreaterOrEqual(charged,.125);
            Call(rewind,"Begin",charged,.16d);Assert.AreEqual(.25d,Call(rewind,"Tick",.25d,.2d));
            Call(rewind,"Clear");Assert.AreEqual(.125d,Call(rewind,"Tick",.125d,2d));
        }
        [TestCase("0")] [TestCase("8")] [TestCase("1234567890123456789012345678901234567890")]
        public void DisplayCountdownIsBoundedMonotoneAndDoesNotMutateItsInput(string value)
        {
            var best=System.Numerics.BigInteger.Parse(value);var previous=best;var method=Runtime("CursorDashScoreCountdown").GetMethod("At");
            for(int n=0;n<=1000;n++){var now=(System.Numerics.BigInteger)method.Invoke(null,new object[]{best,n/1000d});Assert.GreaterOrEqual(now,System.Numerics.BigInteger.Zero);Assert.IsTrue(now<=previous);previous=now;}
            Assert.AreEqual(System.Numerics.BigInteger.Zero,previous);Assert.AreEqual(value,best.ToString());
        }
        [Test] public void RingPrecedesBodyAndCursorHasOneAccelerationAndNoEndpointJump()
        {
            var clock=Timeline();Call(clock,"Advance",1.35*.37);Assert.Greater(Get(clock,"Ring"),0);Assert.AreEqual(0,Get(clock,"Nexus"));
            var launch=Runtime("CursorDashStartTimeline").GetMethod("Launch");
            double a=(double)launch.Invoke(null,new object[]{.03,.18}),b=(double)launch.Invoke(null,new object[]{.06,.18});Assert.Greater(b-a,a);
            Assert.AreEqual(1d,launch.Invoke(null,new object[]{1d,.18}));
        }
        [Test] public void DamageShaderDisplacesRealPixelsAndProtectsScoreStrokes()
        {
            var shader=Resources.Load<Shader>("CursorDash/FeverImpact");Assert.IsNotNull(shader);Assert.IsTrue(shader.isSupported);
            var source=new Texture2D(128,192,TextureFormat.RGBA32,false);var colors=new Color[128*192];for(int y=0;y<192;y++)for(int x=0;x<128;x++)colors[y*128+x]=new Color((x%16)/15f,(y%16)/15f,((x+y)%17)/16f);source.SetPixels(colors);source.Apply();
            var material=new Material(shader);var output=RenderTexture.GetTemporary(128,192);var capture=new Texture2D(128,192,TextureFormat.RGBA32,false);var prior=RenderTexture.active;
            Color[] Render(float age,Texture mask){material.SetVector("_NexusDamage",new Vector4(.37f,.56f,age,1));material.SetTexture("_DamageTextMask",mask);Graphics.Blit(source,output,material);RenderTexture.active=output;capture.ReadPixels(new Rect(0,0,128,192),0,0);capture.Apply();return capture.GetPixels();}
            try{
                material.SetVector("_LensAspect",new Vector4(1,1.5f));material.SetVector("_DamageCore",new Vector4(.08f,.0055f,.24f));material.SetVector("_DamageField",new Vector4(.008f,.12f,2.6f,.72f));material.SetVector("_DamageShape",new Vector4(.85f,1.4f));
                var plain=Render(1,Texture2D.blackTexture);var hit=Render(.18f,Texture2D.blackTexture);var protectedText=Render(.18f,Texture2D.whiteTexture);
                float changed=0,guard=0;for(int n=0;n<hit.Length;n++){changed+=Mathf.Abs(hit[n].r-plain[n].r)+Mathf.Abs(hit[n].g-plain[n].g);guard+=Mathf.Abs(protectedText[n].r-plain[n].r)+Mathf.Abs(protectedText[n].g-plain[n].g);}
                Assert.Greater(changed,2f,"actual GPU sampling must move pixels");Assert.Less(guard,.001f,"glyph mask protects text without disabling the whole scene");
            }finally{RenderTexture.active=prior;RenderTexture.ReleaseTemporary(output);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(capture);}
        }
    }
    public partial class CursorDashSceneRegression
    {
        [UnityTest] public IEnumerator ExplicitTrainingFromMainKeepsItsModeBeforeFirstGuide()
        {
            PlayerPrefs.SetString("CursorDash.v2.tutorial","new");
            var stage=Find("StageManager");Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");
            yield return WaitForScene("Cyberpunk");yield return null;
            Assert.AreSame(stage,Find("StageManager"));Assert.AreEqual(3,Get(Find("CursorDashPractice"),"Step"));
            Assert.IsFalse((bool)Get(stage,"RecordEligible"));Assert.AreEqual("new",PlayerPrefs.GetString("CursorDash.v2.tutorial"));
            Call(stage,"ReturnHome");yield return WaitForScene("01.StageSelectScene");
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");yield return null;
            Assert.AreEqual(0,Get(Find("CursorDashPractice"),"Step"));
        }
        [UnityTest] public IEnumerator SharedSpaceStartsOnceAndHomeKeepsItsManagers()
        {
            var stage=Find("StageManager");var space=Find("CursorDashSpaceFlow");var camera=Camera.main;
            Assert.AreEqual("Main",Get(stage,"State").ToString());
            yield return WaitRealtime(.2f);Assert.AreEqual(0f,Get(Get(stage,"scoreManager"),"TimeScore"));
            Assert.AreEqual("0",Get(Get(Get(stage,"Fever"),"Model"),"PointsText"));Assert.IsTrue(AllNodes().All(n=>!n.gameObject.activeInHierarchy));
            Click("RemakeMenu/Start");Assert.AreEqual("Starting",Get(stage,"State").ToString());
            Assert.IsFalse((bool)Call(space,"Begin",false,Vector2.up));yield return WaitRealtime(.2f);
            Assert.AreEqual(0f,Get(Get(stage,"scoreManager"),"TimeScore"));Assert.IsTrue(AllNodes().All(n=>!n.gameObject.activeInHierarchy));
            yield return WaitForScene("Cyberpunk");DisablePatterns();Assert.AreSame(stage,Find("StageManager"));Assert.AreSame(camera,Camera.main);
            Assert.AreEqual("0",Get(Get(Get(stage,"Fever"),"Model"),"PointsText"));
            Call(stage,"ReturnHome");yield return WaitForScene("01.StageSelectScene");Assert.AreSame(stage,Find("StageManager"));Assert.AreSame(camera,Camera.main);Assert.AreEqual("Cyberpunk",SceneManager.GetActiveScene().name);
        }
        [UnityTest] public IEnumerator ConfirmedHealthLossOwnsRedAndPauseClearsItsOptics()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            var stage=Find("StageManager");Call(stage,"DisqualifyRecord");var feedback=Get(stage,"Feedback");var world=Get(Get(stage,"Fever"),"World");
            Call(feedback,"Emit","hexBlock",Vector3.zero,null,null,Vector2.zero);Assert.IsFalse((bool)Get(world,"DamageActive"));
            Call(stage,"DamageNexus",float.NaN);Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));Assert.IsFalse((bool)Get(world,"DamageActive"));
            Call(stage,"DamageNexus",1f);Assert.AreEqual(2f,Get(Get(stage,"scoreManager"),"Hp"));Assert.IsTrue((bool)Get(world,"DamageActive"));
            var flash=Find("CameraFlash");var image=(UnityEngine.UI.Image)Get(flash,"DamageImage");Assert.IsNull(image.sprite);Assert.Greater(image.color.r,image.color.g);Assert.Greater(image.color.r,image.color.b);Assert.AreEqual(0,((UnityEngine.UI.Image)Get(flash,"flashImage")).color.a);
            Call(stage,"SetPaused",true);Assert.IsFalse((bool)Get(world,"DamageActive"));Assert.AreEqual(0,image.color.a);
        }
        [UnityTest] public IEnumerator SharedScoreAndDamagePresentationEvidence()
        {
            var stage=Find("StageManager");var space=Find("CursorDashSpaceFlow");var fever=Get(stage,"Fever");var model=Get(fever,"Model");var renderer=(MeshRenderer)Get(fever,"BackgroundScoreRenderer");var text=renderer.GetComponent(TypeFor("TMPro.TMP_Text"));
            string saved=PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk","");
            Set(space,"bestValue","12345678901234567890");Set(space,"bestNumber",System.Numerics.BigInteger.Parse("12345678901234567890"));
            string dir=Path.GetFullPath("Logs/space-polish-20261027/NativeCaptures");Directory.CreateDirectory(dir);
            void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));
            yield return WaitRealtime(.1f);Assert.AreEqual("12345678901234567890",Get(text,"text"));Assert.IsTrue((bool)Get(text,"enableAutoSizing"));Capture("00-main-long-best");
            yield return WaitRealtime(3.2f);Assert.Greater((int)Get(Get(fever,"Circuits"),"AmbientCount"),0);Assert.AreEqual(0f,Get(Get(stage,"scoreManager"),"TimeScore"));Capture("01-main-flow");yield return null;
            Click("RemakeMenu/Start");Assert.IsFalse((bool)Call(space,"Begin",false,Vector2.up));
            var previous=System.Numerics.BigInteger.Parse("12345678901234567890");
            for(int n=0;n<20;n++){yield return WaitRealtime(.075f);Assert.AreSame(renderer,Get(fever,"BackgroundScoreRenderer"));var now=System.Numerics.BigInteger.Parse((string)Get(text,"text"));Assert.IsTrue(now<=previous);previous=now;if(n%2==0)Capture("start-"+n.ToString("D2"));}
            yield return WaitForScene("Cyberpunk");DisablePatterns();Call(stage,"DisqualifyRecord");Assert.AreEqual("0",Get(text,"text"));Assert.AreEqual("0",Get(model,"PointsText"));Assert.AreEqual(saved,PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));
            Assert.IsFalse(stage.GetComponentsInChildren<MeshRenderer>(true).Any(r=>r.name=="Shared-space best score"));
            var ui=Find("CursorDashRuntimeUI");Call(ui,"Open",true);Call(ui,"Close");for(int i=0;i<6;i++)Call(fever,"Review","kill");yield return WaitRealtime(.15f);Capture("02-charged-internal-model");yield return null;
            Call(stage,"DamageNexus",1f);Assert.AreEqual(0d,Get(model,"Gauge"));for(int n=0;n<6;n++){Capture("hit-"+n.ToString("D2"));yield return null;}
            Call(stage,"SetPaused",true);Assert.IsFalse((bool)Get(Get(fever,"World"),"DamageActive"));Call(stage,"SetPaused",false);Call(fever,"Review","kill");yield return WaitRealtime(.2f);Assert.That((float)Get(Get(fever,"View"),"HeadTurn"),Is.EqualTo((float)(double)Get(model,"Gauge")).Within(.025f));
            Call(stage,"DamageNexus",1f);yield return null;Call(stage,"DamageNexus",1f);yield return WaitRealtime(.7f);Assert.AreEqual("Ended",Get(stage,"State").ToString());Capture("03-result");yield return null;
            Call(stage,"Retry");yield return WaitForScene("Cyberpunk");DisablePatterns();Call(stage,"DisqualifyRecord");Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
            Call(fever,"Review","fill");yield return WaitRealtime(1.35f);Assert.AreEqual("Fever",Get(model,"Current").ToString());Capture("05-fever");yield return null;
            Call(fever,"Review","step");Assert.AreEqual("Exiting",Get(model,"Current").ToString());Assert.AreEqual(0d,Get(model,"Gauge"));yield return WaitRealtime(.23f);
            Call(stage,"SetPaused",true);double held=(double)Get(model,"Elapsed");yield return WaitRealtime(.12f);Assert.AreEqual(held,Get(model,"Elapsed"));Call(stage,"SetPaused",false);Capture("06-fever-exit");
            yield return WaitRealtime(1.05f);Assert.AreEqual("Normal",Get(model,"Current").ToString());Assert.AreEqual(2,Get(model,"Multiplier"));Capture("07-color-restored");yield return null;
            Call(stage,"ReturnHome");yield return WaitForScene("01.StageSelectScene");yield return null;Capture("04-home");yield return null;Assert.AreEqual(saved,PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));
        }
    }
}
#endif
