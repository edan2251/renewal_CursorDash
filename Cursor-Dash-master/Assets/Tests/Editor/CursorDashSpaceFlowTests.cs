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
        [MenuItem("Tools/CursorDash/Run Nexus Damage Review")]
        public static void RunNexusDamageReview()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("Editor must be idle.");
            for(int n=0;n<SceneManager.sceneCount;n++)if(SceneManager.GetSceneAt(n).isDirty)throw new InvalidOperationException("Unsaved scene; no save/discard is performed.");
            ScriptableObject.CreateInstance<TestRunnerApi>().Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{"CursorDashTests.CursorDashSceneRegression.ActualCircleScoreSurvivesPlayExitAndReentryOnIsolatedMap","CursorDashTests.CursorDashSpaceFlowTests.NexusRedLightIsLocalAndDoesNotTintProtectedGlyphs","CursorDashTests.CursorDashSceneRegression.BothRealAttackDamagePathsHaveStrongSpatialRedAndChromatic"}}));
        }
        [MenuItem("Tools/CursorDash/Run Score Nexus Review")]
        public static void RunScoreNexusReview()
        {
            RunFocusedReview();
        }
        [MenuItem("Tools/CursorDash/Run Shared Space Review")]
        public static void RunFocusedReview()
        {
            if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new InvalidOperationException("대상 Editor가 편집 대기 상태여야 합니다.");
            for(int n=0;n<SceneManager.sceneCount;n++)if(SceneManager.GetSceneAt(n).isDirty)throw new InvalidOperationException("사용자 미저장 씬을 먼저 보존하세요. 저장·폐기하지 않습니다.");
            ScriptableObject.CreateInstance<TestRunnerApi>().Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashSpaceFlowTests","CursorDashTests.CursorDashCircuitTests","CursorDashTests.CursorDashFeverRulesTests",
                "CursorDashTests.CursorDashSceneRegression.OrdinaryConfirmedDamageKeepsNormalRecordEligibility",
                "CursorDashTests.CursorDashSceneRegression.NormalFramesAndRealCircleKillKeepRecordEligibility",
                "CursorDashTests.CursorDashSceneRegression.ActualCircleScoreSurvivesPlayExitAndReentryOnIsolatedMap",
                "CursorDashTests.CursorDashSceneRegression.BothRealAttackDamagePathsHaveStrongSpatialRedAndChromatic",
                "CursorDashTests.CursorDashSceneRegression.ProjectedScoreStaysCenteredThroughNormalAndFeverAwards",
                "CursorDashTests.CursorDashSceneRegression.DamageGaugeKeepsItsFirstFrameAndRewindsBeforeSettling",
                "CursorDashTests.CursorDashSceneRegression.ReturnScoreBreathesOnlyAfterSettlingWithoutChangingPoints",
                "CursorDashTests.CursorDashSceneRegression.SharedScorePoseDoesNotSnapAtGameplayHandoff",
                "CursorDashTests.CursorDashSceneRegression.VisibleNexusResponseHasFiniteAmplitudeAndKeepsBoundary",
                "CursorDashTests.CursorDashSceneRegression.ExplicitTrainingFromMainKeepsItsModeBeforeFirstGuide",
                "CursorDashTests.CursorDashSceneRegression.SharedSpaceStartsOnceAndHomeKeepsItsManagers",
                "CursorDashTests.CursorDashSceneRegression.ConfirmedHealthLossOwnsRedAndPauseClearsItsOptics",
                "CursorDashTests.CursorDashSceneRegression.SharedScoreAndDamagePresentationEvidence",
                "CursorDashTests.CursorDashSceneRegression.ContinuousReturnKeepsExactPointsAndNoCursorOrResult",
                "CursorDashTests.CursorDashSceneRegression.ReturnInterruptionCannotResurrectOldMenu",
                "CursorDashTests.CursorDashSceneRegression.ProjectedBestCentersAcrossDigitCounts",
                "CursorDashTests.CursorDashSceneRegression.RecordSettlementUsesOnlyIsolatedQaMapAndCommitsOnce",
                "CursorDashTests.CursorDashSceneRegression.DisablingNexusOwnersNeverRestartsLegacyRingTweens",
                "CursorDashTests.CursorDashSceneRegression.FinaleToMainContinuousEvidence"}}));
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
        [Test] public void DamageGaugeRewindHasReadableMiddleAndRetargetsWithoutAnOldCallback()
        {
            var rewind=Activator.CreateInstance(Runtime("NexusGaugeRewind"));Call(rewind,"Begin",.75d,.38d);
            Assert.AreEqual(.75d,Call(rewind,"Tick",0d,0d),"First rendered frame retains the lost arc");
            double early=(double)Call(rewind,"Tick",0d,.095d);Assert.Greater(early,.60d);
            Assert.AreEqual(.375d,(double)Call(rewind,"Tick",0d,.095d),1e-9);
            Assert.AreEqual(.375d,(double)Call(rewind,"Tick",0d,0d),1e-9,"Pause freezes the rewind");
            double fresh=(double)Call(rewind,"Tick",.125d,.04d);Assert.GreaterOrEqual(fresh,.125d);
            Call(rewind,"Begin",fresh,.38d);Assert.AreEqual(fresh,Call(rewind,"Tick",0d,0d));
            Assert.AreEqual(.25d,Call(rewind,"Tick",.25d,.4d));
        }
        [Test] public void FinalScoreBreathIsBoundedAndFadesBackToExactlyOne()
        {
            var sample=Runtime("FeverFlowMath").GetMethod("FinalScoreBreath");double low=1,high=1;
            for(int n=0;n<1000;n++){double scale=(double)sample.Invoke(null,new object[]{n*.01,.025d,2.4d,1d});Assert.That(scale,Is.InRange(.975d,1.025d));low=Math.Min(low,scale);high=Math.Max(high,scale);}
            Assert.Less(low,.98);Assert.Greater(high,1.02);Assert.AreEqual(1d,sample.Invoke(null,new object[]{4d,.025d,2.4d,0d}));Assert.AreEqual(1d,sample.Invoke(null,new object[]{0d,.025d,2.4d,1d}));
        }
        [Test] public void OuterArcsHaveStableDifferentPeakTimesAndExactlyScaledDisplacements()
        {
            var visual=Activator.CreateInstance(Runtime("NexusMagnetism"));Call(visual,"Charge",1d);
            var peaks=new System.Collections.Generic.HashSet<int>();
            for(int i=0;i<5;i++){
                double radians=(7d/12+(i+.5)/5)*Math.PI*2;double peak=-1;int peakAt=0;
                for(int n=0;n<300;n++){
                    object[] args={1d+n*.002,radians,.5d,.028d,.24d,.32d,.08d,.48d,1d,.12d};
                    double full=(double)Call(visual,"ArcShaped",args);args[8]=.8d;double reduced=(double)Call(visual,"ArcShaped",args);
                    Assert.AreEqual((full-1)*.8,reduced-1,1e-10);
                    if(full>peak){peak=full;peakAt=n;}
                    Assert.AreEqual(reduced,Call(visual,"ArcShaped",args),"Sampling does not draw new random values");
                }
                peaks.Add(peakAt);
            }
            Assert.GreaterOrEqual(peaks.Count,4,"Charging arcs must not all expand as one plate");
            Call(visual,"Clear");Assert.AreEqual(1d,Call(visual,"ArcShaped",9d,0d,0d,0d,.24d,.32d,.08d,.48d,.8d,.12d));
        }
        [Test] public void FeverRgbIncludesScorePixelsWhileDamageOnlyGuardRemainsSeparate()
        {
            var shader=Resources.Load<Shader>("CursorDash/FeverImpact");Assert.IsTrue(shader!=null&&shader.isSupported);
            var source=new Texture2D(128,64,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};var colors=new Color[128*64];for(int y=0;y<64;y++)for(int x=0;x<128;x++)colors[y*128+x]=x%16<8?Color.white:Color.black;source.SetPixels(colors);source.Apply();
            var material=new Material(shader);var target=RenderTexture.GetTemporary(128,64);var read=new Texture2D(128,64,TextureFormat.RGBA32,false);var prior=RenderTexture.active;
            Color[] Render(float gain,Texture mask){material.SetVector("_Optics",new Vector4(0,0,.0012f,gain));material.SetTexture("_DamageTextMask",mask);Graphics.Blit(source,target,material);RenderTexture.active=target;read.ReadPixels(new Rect(0,0,128,64),0,0);read.Apply();return read.GetPixels();}
            try{
                material.SetVector("_LensAspect",new Vector4(1,1));var plain=Render(0,Texture2D.whiteTexture);var protectedArea=Render(1,Texture2D.whiteTexture);var noGuard=Render(1,Texture2D.blackTexture);
                double split=0,maskDelta=0;for(int n=0;n<plain.Length;n++){split+=Math.Abs(plain[n].r-protectedArea[n].r)+Math.Abs(plain[n].b-protectedArea[n].b);maskDelta+=Math.Abs(protectedArea[n].r-noGuard[n].r)+Math.Abs(protectedArea[n].b-noGuard[n].b);}
                Assert.Greater(split,.1,"Fever RGB must affect completed-board score pixels too");Assert.Less(maskDelta,.001,"Damage-only glyph protection must not disable fever RGB");
                material.SetFloat("_ScoreChromaticGain",3);var scoreBoosted=Render(1,Texture2D.whiteTexture);double stronger=0;
                for(int n=0;n<plain.Length;n++)stronger+=Math.Abs(plain[n].r-scoreBoosted[n].r)+Math.Abs(plain[n].b-scoreBoosted[n].b);
                Assert.Greater(stronger,split*2,"Score-specific RGB amplification must sample different pixels");
                var disabled=Render(0,Texture2D.whiteTexture);double off=0;for(int n=0;n<plain.Length;n++)off+=Math.Abs(disabled[n].r-plain[n].r)+Math.Abs(disabled[n].b-plain[n].b);Assert.Less(off,.001,"User effect gain zero still disables the boost");
            }finally{RenderTexture.active=prior;RenderTexture.ReleaseTemporary(target);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(read);}
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
        [Test] public void StartScoreTravelNeverOvershootsAndCountdownFinishesBeforeTravel()
        {
            var clock=Timeline();double prior=0;
            for(int n=0;n<200;n++){Call(clock,"Advance",1.35/200);double travel=Get(clock,"ScoreTravel");Assert.That(travel,Is.InRange(prior,1d));prior=travel;}
            clock=Timeline();Call(clock,"Advance",1.35*.35);Assert.AreEqual(1d,Get(clock,"ScoreCountProgress"));Assert.Less(Get(clock,"ScoreTravel"),1d);
        }
        [Test] public void NexusRedLightIsLocalAndDoesNotTintProtectedGlyphs()
        {
            var shader=Resources.Load<Shader>("CursorDash/FeverImpact");Assert.IsTrue(shader!=null&&shader.isSupported);
            var source=new Texture2D(128,192,TextureFormat.RGBA32,false);var colors=new Color[128*192];
            for(int y=0;y<192;y++)for(int x=0;x<128;x++)colors[y*128+x]=new Color(.12f+x/512f,.12f+y/768f,.20f);
            source.SetPixels(colors);source.Apply();var material=new Material(shader);var output=RenderTexture.GetTemporary(128,192);var capture=new Texture2D(128,192,TextureFormat.RGBA32,false);var prior=RenderTexture.active;
            Color[] Render(float age,float light,Texture mask){material.SetVector("_NexusDamage",new Vector4(.37f,.56f,age,1));material.SetFloat("_DamageLightUser",light);material.SetTexture("_DamageTextMask",mask);Graphics.Blit(source,output,material);RenderTexture.active=output;capture.ReadPixels(new Rect(0,0,128,192),0,0);capture.Apply();return capture.GetPixels();}
            try{
                material.SetVector("_LensAspect",new Vector4(1,1.5f));material.SetVector("_DamageNexusCenter",new Vector4(.41f,.56f,1,0));material.SetVector("_DamageCore",new Vector4(.08f,.018f,.48f));material.SetVector("_DamageField",new Vector4(.03f,.24f,4.2f,1.25f));material.SetVector("_DamageShape",new Vector4(1.10f,.85f,1.15f,0));
                var plain=Render(1,0,Texture2D.blackTexture);var warp=Render(.16f,0,Texture2D.blackTexture);
                material.SetVector("_DamageCore",new Vector4(.08f,0,.48f));var zeroSplit=Render(.16f,0,Texture2D.blackTexture);
                material.SetVector("_DamageCore",new Vector4(.08f,.0055f,.48f));var oldSplit=Render(.16f,0,Texture2D.blackTexture);
                material.SetVector("_DamageCore",new Vector4(.08f,.018f,.48f));double strongSplit=0,weakSplit=0;
                for(int n=0;n<warp.Length;n++){strongSplit+=Mathf.Abs(warp[n].r-zeroSplit[n].r);weakSplit+=Mathf.Abs(oldSplit[n].r-zeroSplit[n].r);}Assert.Greater(strongSplit,weakSplit*2,"Actual channel separation must grow, not only the Inspector number");var red=Render(.16f,1,Texture2D.blackTexture);var guard=Render(.16f,1,Texture2D.whiteTexture);
                double added=0,far=0,protectedDelta=0;int farCount=0;
                for(int n=0;n<red.Length;n++){added+=Mathf.Max(0,red[n].r-warp[n].r);if(n/128<4&&n%128<4){far+=Mathf.Abs(red[n].r-warp[n].r);farCount++;}protectedDelta+=Mathf.Abs(guard[n].r-plain[n].r)+Mathf.Abs(guard[n].g-plain[n].g)+Mathf.Abs(guard[n].b-plain[n].b);}
                Assert.Greater(added,.25,"Actual local red light, not only different metadata");Assert.Less(far/farCount,.002,"No screen-wide red wash");Assert.Less(protectedDelta,.001,"Text strokes remain unchanged");
                Directory.CreateDirectory("Logs/nexus-hit-save-20261027");File.WriteAllText("Logs/nexus-hit-save-20261027/nexus-spatial-gpu.txt",$"GPU DX11: summed local red={added}; distant mean={far/farCount}; protected delta={protectedDelta}; RGB old={weakSplit}; strong={strongSplit}");
            }finally{RenderTexture.active=prior;RenderTexture.ReleaseTemporary(output);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(capture);}
        }
        [Test] public void ActualScoreRollRetargetsAndBoundedSpringHasOneSmallUndershoot()
        {
            var roll=Activator.CreateInstance(Runtime("CursorDashScoreRoll"));var target=System.Numerics.BigInteger.Parse("123456789012345678901234567890");Call(roll,"Snap",System.Numerics.BigInteger.Zero);Call(roll,"Aim",target,.24d);
            Call(roll,"Advance",.12d);var current=(System.Numerics.BigInteger)roll.GetType().GetProperty("Value").GetValue(roll);Assert.IsTrue(current>0&&current<target);
            Call(roll,"Aim",target+7,.24d);Call(roll,"Advance",.12d);Assert.AreEqual(target+7,roll.GetType().GetProperty("Value").GetValue(roll));
            var pulse=Activator.CreateInstance(Runtime("ScoreProjectionPulse"));Call(pulse,"Award",new System.Numerics.BigInteger(4));double low=1,high=1;
            for(int n=0;n<400;n++){Call(pulse,"Advance",.005d,.32d);double value=(double)Call(pulse,"Scale",.22d,1.22d);low=Math.Min(low,value);high=Math.Max(high,value);Assert.That(value,Is.InRange(.98d,1.22d));}
            Assert.Less(low,.999);Assert.Greater(high,1.04);Assert.AreEqual(1d,Call(pulse,"Scale",.22d,1.22d));
        }
        [Test] public void SinglePointIsImmediateAndLargerAwardStepsTowardExactTarget()
        {
            var roll=Activator.CreateInstance(Runtime("CursorDashScoreRoll"));var value=roll.GetType().GetProperty("Value");
            Call(roll,"Award",new System.Numerics.BigInteger(1),new System.Numerics.BigInteger(1),.24d);Assert.AreEqual(new System.Numerics.BigInteger(1),value.GetValue(roll));
            Call(roll,"Award",new System.Numerics.BigInteger(4),new System.Numerics.BigInteger(5),.24d);
            var observed=new System.Collections.Generic.HashSet<string>();for(int i=0;i<24;i++){Call(roll,"Advance",.01d);observed.Add(value.GetValue(roll).ToString());}
            CollectionAssert.IsSubsetOf(new[]{"2","3","4","5"},observed);
            Call(roll,"Award",new System.Numerics.BigInteger(4),new System.Numerics.BigInteger(9),.24d);Call(roll,"Advance",.1d);Call(roll,"Award",new System.Numerics.BigInteger(1),new System.Numerics.BigInteger(10),.24d);Call(roll,"Advance",.14d);Assert.AreEqual(new System.Numerics.BigInteger(10),value.GetValue(roll));
        }
        [Test] public void MagneticPosesAreFiniteDirectionalAndRestoreWithoutDrift()
        {
            var visual=Activator.CreateInstance(Runtime("NexusMagnetism"));Call(visual,"Charge",1d);Call(visual,"Hit",1.05d,true,0d);
            double facing=(double)Call(visual,"Arc",1.15d,0d,.5d,0d,.045d,.095d,.025d,.34d),back=(double)Call(visual,"Arc",1.15d,Math.PI,.5d,0d,.045d,.095d,.025d,.34d);Assert.Less(facing,back);
            for(int n=0;n<1000;n++){Call(visual,"Charge",n*.01);Call(visual,"Hit",n*.01,false,0d);Assert.That((double)Call(visual,"Body",n*.01+.1,0d,.045d,.095d,.34d),Is.InRange(.85d,1.1d));}
            Call(visual,"Clear");Assert.AreEqual(1d,Call(visual,"Body",12d,0d,.045d,.095d,.34d));Assert.AreEqual(1d,Call(visual,"Ring",12d,.045d,.095d,.34d));
        }
    }
    public partial class CursorDashSceneRegression
    {
        [UnityTest] public IEnumerator ReturnScoreBreathesOnlyAfterSettlingWithoutChangingPoints()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");Call(stage,"DisqualifyRecord");var fever=Get(stage,"Fever");var model=Get(fever,"Model");Call(model,"Kill",int.MinValue+82,1ul);Call(stage,"Death");
            var space=Find("CursorDashSpaceFlow");yield return WaitRealtime(.9f);Assert.AreEqual(1f,Get(space,"FinalScoreScale"));yield return WaitRealtime(1.4f);Assert.IsTrue((bool)Get(space,"AwaitingReturnClick"));
            string points=Get(model,"PointsText").ToString();float low=1,high=1;for(int n=0;n<26;n++){float scale=(float)Get(space,"FinalScoreScale");low=Mathf.Min(low,scale);high=Mathf.Max(high,scale);Assert.That(scale,Is.InRange(.9749f,1.0251f));yield return WaitRealtime(.1f);}
            Assert.Greater(high-low,.025f);Assert.AreEqual(points,Get(model,"PointsText"));Assert.IsTrue((bool)Call(space,"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));yield return WaitRealtime(.55f);Assert.AreEqual(1f,Get(space,"FinalScoreScale"));
        }
        [UnityTest] public IEnumerator ProjectedScoreStaysCenteredThroughNormalAndFeverAwards()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");Call(stage,"DisqualifyRecord");var fever=Get(stage,"Fever");var model=Get(fever,"Model");var projection=Get(fever,"projection");var renderer=(MeshRenderer)Get(fever,"BackgroundScoreRenderer");
            void Centered(){Call(projection,"Draw");Vector3 uv=Camera.main.WorldToViewportPoint(renderer.bounds.center);Assert.That(uv.x,Is.EqualTo(.5f).Within(.003f));Assert.That(uv.y,Is.EqualTo(.5f).Within(.003f));}
            Centered();Call(model,"Kill",int.MinValue+71,1ul);yield return null;Centered();
            Call(model,"FillForReview");Call(model,"Tick",(double)Get(model,"PhaseSeconds")+.001);Call(model,"Tick",(double)Get(model,"PhaseSeconds")+.001);yield return null;Assert.AreEqual("Fever",Get(model,"Current").ToString());
            for(ulong n=2;n<6;n++){Call(model,"Kill",int.MinValue+71,n);yield return null;Centered();}
        }
        [UnityTest] public IEnumerator DamageGaugeKeepsItsFirstFrameAndRewindsBeforeSettling()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");Call(stage,"DisqualifyRecord");var fever=Get(stage,"Fever");var model=Get(fever,"Model");var view=Get(fever,"View");
            for(ulong n=1;n<=6;n++)Call(model,"Kill",int.MinValue+72,n);yield return WaitRealtime(.25f);
            float before=(float)Get(view,"displayCharge");Assert.Greater(before,.65f);Call(stage,"DamageNexus",1f);Assert.AreEqual(0d,Get(model,"Gauge"));
            Call(view,"LateUpdate");Assert.AreEqual(before,(float)Get(view,"displayCharge"),.00001f);Assert.IsTrue((bool)Get(view,"Rewinding"));
            yield return WaitRealtime(.10f);float during=(float)Get(view,"displayCharge");Assert.Greater(during,before*.5f);Assert.Less(during,before);
            Call(stage,"SetPaused",true);float paused=(float)Get(view,"displayCharge");yield return WaitRealtime(.12f);Assert.AreEqual(paused,(float)Get(view,"displayCharge"),.00001f);
            Call(stage,"SetPaused",false);yield return WaitRealtime(.45f);Assert.AreEqual(0f,(float)Get(view,"displayCharge"),.00001f);Assert.IsFalse((bool)Get(view,"Rewinding"));
        }
        [UnityTest] public IEnumerator NormalFramesAndRealCircleKillKeepRecordEligibility()
        {
            var stage=Find("StageManager");var tuning=TypeFor("CursorDashTuning");var field=tuning.GetField("RuleSessionChanged");string trace="";int count=0;
            Action watch=()=>{if((bool)Get(stage,"IsPlaying")&&count++<4)trace+=tuning.GetProperty("LastRuleReadMismatch").GetValue(null)+"\n"+Environment.StackTrace+"\n";};
            field.SetValue(null,(Action)field.GetValue(null)+watch);
            try{
                Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");yield return WaitRealtime(1.2f);
                Assert.IsTrue((bool)Get(stage,"RecordEligible"),"Normal frames invalidated eligibility:\n"+trace);
                DisablePatterns();var normal=AllNodes().FirstOrDefault(n=>n.GetType().Name=="NormalNode"&&n.gameObject.activeSelf);
                if(normal==null){var creator=Get(stage,"NodeCreator");creator.GetType().GetMethod("CreateNode",new[]{typeof(int),typeof(int)}).Invoke(creator,new object[]{0,0});normal=AllNodes().First(n=>n.GetType().Name=="NormalNode"&&n.gameObject.activeSelf);}
                Call(normal,"ContactAt",(Vector2)normal.transform.position,Vector2.up);yield return WaitRealtime(.3f);
                Assert.IsTrue((bool)Get(stage,"RecordEligible"),"Actual circle contact invalidated eligibility:\n"+trace);
                Assert.AreEqual("1",Get(Get(Get(stage,"Fever"),"Model"),"PointsText"));
            }finally{field.SetValue(null,(Action)field.GetValue(null)-watch);}
        }
        [UnityTest] public IEnumerator SharedScorePoseDoesNotSnapAtGameplayHandoff()
        {
            var stage=Find("StageManager");var fever=Get(stage,"Fever");var space=Find("CursorDashSpaceFlow");var projection=Get(fever,"projection");var renderer=(MeshRenderer)Get(fever,"BackgroundScoreRenderer");
            string dir=Path.GetFullPath("Logs/nexus-hit-save-20261027/NativeCaptures");Directory.CreateDirectory(dir);
            foreach(string digits in new[]{"0","8","12345678901234567890"}){
                Call(stage,"ReturnHome");Set(space,"bestValue",digits);Set(space,"bestNumber",System.Numerics.BigInteger.Parse(digits));Call(projection,"Draw");
                Assert.IsTrue((bool)Call(space,"Begin",false,Vector2.up));Call(space,"Advance",1.3499f);Call(projection,"Draw");
                Bounds before=renderer.bounds;var tmp=renderer.GetComponent(TypeFor("TMPro.TMP_Text"));Color tint=(Color)Get(tmp,"color");Assert.AreEqual("0",Get(fever,"BackgroundScoreText"));
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,"handoff-before-"+digits.Length+".png"));yield return null;
                Call(space,"Advance",.41f);Call(projection,"Draw");Assert.AreEqual("Playing",Get(stage,"State").ToString());
                Assert.Less(Vector3.Distance(before.center,renderer.bounds.center),.0001f);Assert.Less(Vector3.Distance(before.size,renderer.bounds.size),.0001f,"AutoSize handoff must not change the final glyph geometry");Assert.AreEqual(tint,(Color)Get(tmp,"color"));
                Assert.AreEqual("0",Get(Get(fever,"Model"),"PointsText"));Assert.AreEqual(Vector3.one,renderer.transform.localScale);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,"handoff-after-"+digits.Length+".png"));yield return null;
            }
        }
        [UnityTest] public IEnumerator VisibleNexusResponseHasFiniteAmplitudeAndKeepsBoundary()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");var fever=Get(stage,"Fever");var model=Get(fever,"Model");float radius=(float)Get(stage,"NexusRadius");
            string dir=Path.GetFullPath("Logs/nexus-hit-save-20261027/NativeCaptures");Directory.CreateDirectory(dir);float high=1,low=1;
            Call(model,"Kill",int.MinValue+17,1ul);yield return null;
            for(int n=0;n<8;n++){high=Mathf.Max(high,(float)Get(fever,"MagneticBody"));ScreenCapture.CaptureScreenshot(Path.Combine(dir,"nexus-charge-"+n+".png"));yield return WaitRealtime(.04f);}
            Assert.Greater(high,1.18f,"Readable visual charge impulse, without changing fill or collider");
            Vector2 contact=(Vector2)Get(stage,"NexusPosition")+Vector2.left*radius;Call(stage,"DamageNexusAt",1f,contact);
            for(int n=0;n<9;n++){low=Mathf.Min(low,(float)Get(fever,"MagneticBody"));ScreenCapture.CaptureScreenshot(Path.Combine(dir,"nexus-contact-"+n+".png"));yield return WaitRealtime(.04f);}
            Assert.Less(low,.80f);Assert.AreEqual(radius,Get(stage,"NexusRadius"));Assert.AreEqual(2f,Get(Get(stage,"scoreManager"),"Hp"));yield return WaitRealtime(.6f);Assert.That((float)Get(fever,"MagneticBody"),Is.InRange(.959f,1.041f));
        }
        [UnityTest] public IEnumerator ActualCircleScoreSurvivesPlayExitAndReentryOnIsolatedMap()
        {
            var stage=Find("StageManager");var game=Find("GameManager");var original=(UnityEngine.Object)Get(game,"MapInformation");var copy=UnityEngine.Object.Instantiate(original);copy.hideFlags=HideFlags.DontSave;
            string qa="SaveReentryQA_"+Guid.NewGuid().ToString("N"),pointKey="CursorDash_BestPoints_v1_"+qa,timeKey="CursorDash_Best_"+qa;SessionState.SetString("CursorDash.SaveReentryQA",qa);
            string actualBefore=PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk","");SessionState.SetString("CursorDash.SaveReentryUserBefore",actualBefore);
            try{
                Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();Set(copy,"mapName",qa);Set(copy,"highScore",0f);Set(game,"MapInformation",copy);
                var creator=Get(stage,"NodeCreator");creator.GetType().GetMethod("CreateNode",new[]{typeof(int),typeof(int)}).Invoke(creator,new object[]{0,0});var node=AllNodes().First(n=>n.GetType().Name=="NormalNode"&&n.gameObject.activeSelf);
                Call(node,"ContactAt",(Vector2)node.transform.position,Vector2.up);yield return WaitRealtime(.4f);Assert.IsTrue((bool)Get(stage,"RecordEligible"));
                Call(stage,"DamageNexusFromCircle",1f,(Vector2)Get(stage,"NexusPosition")+Vector2.left*(float)Get(stage,"NexusRadius"),Vector2.right,Color.magenta,.4f);yield return WaitRealtime(.55f);Assert.IsTrue((bool)Get(stage,"RecordEligible"));
                Call(stage,"DamageNexus",2f);Assert.AreEqual("1",PlayerPrefs.GetString(pointKey));yield return WaitRealtime(2.3f);
                var space=Find("CursorDashSpaceFlow");Assert.IsTrue((bool)Call(space,"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));yield return WaitRealtime(.55f);Assert.AreEqual("1",Get(space,"BestText"));
                Set(game,"MapInformation",original);UnityEngine.Object.Destroy(copy);copy=null;
                yield return new ExitPlayMode();qa=SessionState.GetString("CursorDash.SaveReentryQA","");Assert.IsNotEmpty(qa);pointKey="CursorDash_BestPoints_v1_"+qa;timeKey="CursorDash_Best_"+qa;Assert.AreEqual("1",PlayerPrefs.GetString(pointKey));
                yield return new EnterPlayMode();yield return null;yield return null;
                string resumedQa=SessionState.GetString("CursorDash.SaveReentryQA","");
                Directory.CreateDirectory("Logs/nexus-hit-save-20261027");File.WriteAllText("Logs/nexus-hit-save-20261027/reentry-diagnostic.txt","local="+qa+"; session="+resumedQa+"; localRaw="+PlayerPrefs.GetString("CursorDash_BestPoints_v1_"+qa,"MISSING")+"; sessionRaw="+PlayerPrefs.GetString("CursorDash_BestPoints_v1_"+resumedQa,"MISSING"));
                Assert.IsNotEmpty(resumedQa,"The QA key must survive test domain reload");qa=resumedQa;
                Assert.AreEqual("1",TypeFor("CursorDashPointsRecord").GetMethod("Read").Invoke(null,new object[]{qa}).ToString());
                Assert.AreEqual(SessionState.GetString("CursorDash.SaveReentryUserBefore",""),PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));
                Directory.CreateDirectory("Logs/nexus-hit-save-20261027");File.WriteAllText("Logs/nexus-hit-save-20261027/play-reentry-record.txt","PASS actual circle contact -> ordinary damage -> settlement 1 -> Main BEST 1 -> ExitPlayMode -> EnterPlayMode -> persisted 1. Isolated QA keys only; real record unchanged.");
            }finally{if(copy!=null){if(game!=null)Set(game,"MapInformation",original);UnityEngine.Object.Destroy(copy);}string owned=SessionState.GetString("CursorDash.SaveReentryQA",qa);if(!string.IsNullOrEmpty(owned)){PlayerPrefs.DeleteKey("CursorDash_BestPoints_v1_"+owned);PlayerPrefs.DeleteKey("CursorDash_Best_"+owned);}SessionState.EraseString("CursorDash.SaveReentryQA");SessionState.EraseString("CursorDash.SaveReentryUserBefore");PlayerPrefs.Save();}
        }
        [UnityTest] public IEnumerator BothRealAttackDamagePathsHaveStrongSpatialRedAndChromatic()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");Call(stage,"DisqualifyRecord");var fever=Get(stage,"Fever");var world=Get(fever,"World");var score=Get(stage,"scoreManager");var flash=Find("CameraFlash");
            string dir=Path.GetFullPath("Logs/nexus-hit-save-20261027/NativeCaptures");Directory.CreateDirectory(dir);Vector2 center=(Vector2)Get(stage,"NexusPosition");float radius=(float)Get(stage,"NexusRadius");int before=(int)Get(world,"DamageExecutions");
            Call(stage,"DamageNexusFromCircle",1f,center+Vector2.left*radius,Vector2.right,Color.magenta,.4f);
            Assert.AreEqual(before+1,Get(world,"DamageExecutions"));Assert.AreEqual(2f,Get(score,"Hp"));Assert.GreaterOrEqual((float)Get(world,"DamageChromatic"),.018f);Assert.IsTrue((bool)Get(world,"DamageUsesSpatialWave"));Assert.AreEqual(0,((UnityEngine.UI.Image)Get(flash,"DamageImage")).color.a,"Ordinary damage uses a moving red optical wave, no global flash");
            int opticalBefore=(int)Get(world,"OpticsFrames");var samples=new System.Collections.Generic.List<string>();float minBody=1;for(int n=0;n<12;n++){minBody=Mathf.Min(minBody,(float)Get(fever,"MagneticBody"));ScreenCapture.CaptureScreenshot(Path.Combine(dir,"circle-damage-"+n.ToString("D2")+".png"));yield return WaitRealtime(.04f);var optical=(Material)Get(world,"optics");samples.Add(n+" active="+Get(world,"DamageActive")+" render="+Get(world,"OpticsFrames")+" input="+optical.GetVector("_NexusDamage")+" shape="+optical.GetVector("_DamageShape")+" light="+optical.GetFloat("_DamageLightUser"));}File.WriteAllLines("Logs/nexus-hit-save-20261027/live-optics.txt",samples);Assert.Greater((int)Get(world,"OpticsFrames"),opticalBefore,"Damage optics must execute in the actual camera path");Assert.Less(minBody,.80f);Assert.AreEqual(before+1,Get(world,"DamageExecutions"));
            Call(stage,"DamageNexusFromLaser",1f,center+Vector2.right*radius,Vector2.left,Color.yellow);
            Assert.AreEqual(before+2,Get(world,"DamageExecutions"));Assert.AreEqual(1f,Get(score,"Hp"));Assert.IsTrue((bool)Get(world,"DamageUsesSpatialWave"));
            for(int n=0;n<12;n++){ScreenCapture.CaptureScreenshot(Path.Combine(dir,"laser-damage-"+n.ToString("D2")+".png"));yield return WaitRealtime(.04f);}Assert.AreEqual(radius,Get(stage,"NexusRadius"));
            Call(stage,"DamageNexus",0f);Assert.AreEqual(before+2,Get(world,"DamageExecutions"));Call(stage,"SetPaused",true);Assert.IsFalse((bool)Get(world,"DamageActive"));Call(stage,"SetPaused",false);
            // D-059 replaces the finale-only legacy wash/RGB envelope with the same confirmed-hit optics.
            Call(stage,"DamageNexusFromLaser",1f,center+Vector2.down*radius,Vector2.up,Color.yellow);Assert.AreEqual(0f,Get(score,"Hp"));Assert.AreEqual(before+3,Get(world,"DamageExecutions"));Assert.IsTrue((bool)Get(world,"DamageUsesSpatialWave"));Assert.AreEqual(.018f,Get(world,"DamageChromatic"));Assert.AreEqual(0,((UnityEngine.UI.Image)Get(flash,"DamageImage")).color.a);Assert.AreEqual(center+Vector2.down*radius,(Vector2)Get(world,"damageWorldPoint"));yield return WaitRealtime(.7f);Assert.AreEqual("Returning",Get(stage,"State").ToString());Assert.AreEqual(1f,Time.timeScale);
        }
        [UnityTest] public IEnumerator OrdinaryConfirmedDamageKeepsNormalRecordEligibility()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");
            Assert.IsTrue((bool)Get(stage,"RecordEligible"));
            var field=TypeFor("CursorDashTuning").GetField("RuleSessionChanged");string stack="";
            Action trace=()=>stack+=Environment.StackTrace+"\n";field.SetValue(null,(Action)field.GetValue(null)+trace);
            try {Call(stage,"DamageNexusAt",1f,(Vector2)Get(stage,"NexusPosition")+Vector2.left*(float)Get(stage,"NexusRadius"));}
            finally {field.SetValue(null,(Action)field.GetValue(null)-trace);}
            Assert.IsTrue((bool)Get(stage,"RecordEligible"),"Ordinary confirmed damage must not be treated as a developer change:\n"+stack);
            Call(Find("CursorDashRuntimeUI"),"Open",true);Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"devOff\"}");Call(Find("CursorDashRuntimeUI"),"Close");
            Assert.IsFalse((bool)Get(stage,"RecordEligible"),"Explicit developer use remains excluded after turning Dev off.");
        }
        [UnityTest] public IEnumerator DisablingNexusOwnersNeverRestartsLegacyRingTweens()
        {
            var stage=Find("StageManager");var fever=(Behaviour)Get(stage,"Fever");var images=(UnityEngine.UI.Image[])Get(Get(stage,"NexusPresentation"),"SourceImages");
            var dotted=(Behaviour)images.First(i=>i.name=="HpCentral").GetComponent(TypeFor("Rotator"));var frame=images.First(i=>i.name=="HpFrame");var outer=(Behaviour)frame.GetComponent(TypeFor("Rotator"));
            Assert.IsFalse(dotted.enabled);Assert.IsFalse(outer.enabled);fever.enabled=false;yield return null;Assert.IsFalse(dotted.enabled,"Suspending the owner must not create another tween");
            ((Behaviour)frame.GetComponent(TypeFor("NexusRingRhythm"))).enabled=false;yield return null;Assert.IsFalse(outer.enabled,"The stationary outer ring must not resume on disable");
            fever.enabled=true;yield return null;Call(stage,"ReturnHome");yield return null;Assert.IsTrue((bool)Get(fever,"DottedControlled"));Assert.IsFalse(dotted.enabled);Assert.IsFalse(outer.enabled);
        }
        [UnityTest] public IEnumerator ProjectedBestCentersAcrossDigitCounts()
        {
            var stage=Find("StageManager");var fever=Get(stage,"Fever");var space=Find("CursorDashSpaceFlow");var renderer=(MeshRenderer)Get(fever,"BackgroundScoreRenderer");
            string saved=PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk","");string dir=Path.GetFullPath("Logs/nexus-hit-save-20261027/NativeCaptures");Directory.CreateDirectory(dir);
            foreach(string digits in new[]{"0","8","12345678901234567890"}){
                Set(space,"bestValue",digits);Set(space,"bestNumber",System.Numerics.BigInteger.Parse(digits));yield return WaitRealtime(.08f);
                Vector2 center=Camera.main.WorldToViewportPoint(renderer.bounds.center);Assert.That(center.x,Is.EqualTo(.5f).Within(.003f));Assert.That(center.y,Is.EqualTo(.5f).Within(.003f));
                Vector2 lo=Camera.main.WorldToViewportPoint(renderer.bounds.min),hi=Camera.main.WorldToViewportPoint(renderer.bounds.max);Assert.GreaterOrEqual(lo.x,.075f);Assert.LessOrEqual(hi.x,.925f);
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,"center-"+digits.Length+"-digits-"+digits[0]+".png"));yield return null;
            }
            Assert.AreEqual(saved,PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));Assert.AreEqual("0",Get(Get(fever,"Model"),"PointsText"));
        }
        [UnityTest] public IEnumerator RecordSettlementUsesOnlyIsolatedQaMapAndCommitsOnce()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");var game=Find("GameManager");var fever=Get(stage,"Fever");var model=Get(fever,"Model");
            var original=(UnityEngine.Object)Get(game,"MapInformation");var copy=UnityEngine.Object.Instantiate(original);copy.hideFlags=HideFlags.DontSave;
            string qa="ContinuousQA_"+Guid.NewGuid().ToString("N"),pointKey="CursorDash_BestPoints_v1_"+qa,timeKey="CursorDash_Best_"+qa;
            string userRecord=PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk","");Assert.IsFalse(PlayerPrefs.HasKey(pointKey));Assert.IsFalse(PlayerPrefs.HasKey(timeKey));
            try{
                Set(copy,"mapName",qa);Set(copy,"highScore",0f);Set(game,"MapInformation",copy);Assert.IsTrue((bool)Get(stage,"RecordEligible"));
                Call(stage,"DamageNexus",1f);Assert.IsTrue((bool)Get(stage,"RecordEligible"));
                for(int n=0;n<3;n++)Call(model,"Kill",int.MinValue+1,(ulong)(n+1));Assert.AreEqual("3",Get(model,"PointsText"));Call(stage,"Death");Call(stage,"Death");
                Assert.AreEqual("3",PlayerPrefs.GetString(pointKey));Assert.IsTrue((bool)Get(stage,"lastRoundRecord"));
                yield return WaitRealtime(2.8f);Assert.AreEqual("Returning",Get(stage,"State").ToString());Assert.IsTrue((bool)Get(Find("CursorDashSpaceFlow"),"AwaitingReturnClick"));Assert.IsTrue((bool)Call(Find("CursorDashSpaceFlow"),"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));yield return WaitRealtime(.55f);Assert.AreEqual("Main",Get(stage,"State").ToString());Assert.AreEqual("3",Get(Find("CursorDashSpaceFlow"),"BestText"));
                var caption=GameObject.Find("BEST caption").GetComponent(TypeFor("TMPro.TMP_Text"));Assert.AreEqual("NEW BEST",Get(caption,"text"));
                Call(Find("CursorDashSpaceFlow"),"LaunchSelectedMode");yield return WaitForScene("Cyberpunk");DisablePatterns();Call(stage,"DisqualifyRecord");
                for(int n=0;n<5;n++)Call(model,"Kill",int.MinValue+2,(ulong)(n+1));Call(stage,"Death");Assert.AreEqual("3",PlayerPrefs.GetString(pointKey));Assert.IsFalse((bool)Get(stage,"lastRoundRecord"));
                yield return WaitRealtime(2.8f);Assert.IsTrue((bool)Call(Find("CursorDashSpaceFlow"),"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));yield return WaitRealtime(.55f);Assert.AreEqual("BEST",Get(caption,"text"));Assert.AreEqual(userRecord,PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));
            }finally{Set(game,"MapInformation",original);UnityEngine.Object.Destroy(copy);PlayerPrefs.DeleteKey(pointKey);PlayerPrefs.DeleteKey(timeKey);PlayerPrefs.Save();}
        }
        [UnityTest] public IEnumerator ContinuousReturnKeepsExactPointsAndNoCursorOrResult()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");var fever=Get(stage,"Fever");var model=Get(fever,"Model");
            string saved=PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk","");Call(stage,"DisqualifyRecord");
            Call(Find("CursorDashRuntimeUI"),"Open",true);Call(Find("CursorDashRuntimeUI"),"Close");Call(fever,"Review","kill");Call(fever,"Review","kill");
            string final=(string)Get(model,"PointsText");float radius=(float)Get(stage,"NexusRadius");yield return WaitRealtime(.4f);Assert.AreEqual(final,Get(fever,"BackgroundScoreText"));Assert.AreEqual(radius,Get(stage,"NexusRadius"));
            Call(stage,"DamageNexus",3f);Assert.AreEqual("Ended",Get(stage,"State").ToString());Call(stage,"Death");yield return WaitRealtime(.70f);
            Assert.AreEqual("Returning",Get(stage,"State").ToString());Assert.AreEqual(final,Get(fever,"BackgroundScoreText"));Assert.IsNull(GameObject.Find("RemakeHUD/ResultPanel"));Assert.IsTrue(AllNodes().All(n=>!n.gameObject.activeSelf));
            Assert.IsFalse(Find("PlayerCharacterController").GetComponentsInChildren<SpriteRenderer>(true).Any(r=>r.enabled));Assert.AreEqual(1f,Time.timeScale);
            Call(stage,"SetPaused",true);yield return WaitRealtime(2.1f);Assert.AreEqual("Returning",Get(stage,"State").ToString());Assert.AreEqual(final,Get(fever,"BackgroundScoreText"));Assert.IsTrue((bool)Call(Find("CursorDashSpaceFlow"),"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));Assert.IsFalse((bool)Call(Find("CursorDashSpaceFlow"),"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));yield return WaitRealtime(.55f);Assert.AreEqual("Main",Get(stage,"State").ToString());Assert.AreEqual(saved,PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));Assert.AreEqual("BEST",GameObject.Find("BEST caption").GetComponent(TypeFor("TMPro.TMP_Text")).GetType().GetProperty("text").GetValue(GameObject.Find("BEST caption").GetComponent(TypeFor("TMPro.TMP_Text"))));
        }
        [UnityTest] public IEnumerator ReturnInterruptionCannotResurrectOldMenu()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var stage=Find("StageManager");Call(stage,"DisqualifyRecord");Call(stage,"DamageNexus",3f);yield return WaitRealtime(.72f);
            Assert.AreEqual("Returning",Get(stage,"State").ToString());Call(stage,"ReturnHome");yield return null;Assert.AreEqual("Main",Get(stage,"State").ToString());
            Call(stage,"Retry");yield return WaitForScene("Cyberpunk");DisablePatterns();Call(stage,"DisqualifyRecord");yield return WaitRealtime(2.2f);Assert.AreEqual("Playing",Get(stage,"State").ToString());Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
        }
        [UnityTest] public IEnumerator FinaleToMainContinuousEvidence()
        {
            var stage=Find("StageManager");var fever=Get(stage,"Fever");var space=Find("CursorDashSpaceFlow");var model=Get(fever,"Model");var saved=PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk","");
            string dir=Path.GetFullPath("Logs/nexus-hit-save-20261027/NativeCaptures");Directory.CreateDirectory(dir);void Capture(string name)=>ScreenCapture.CaptureScreenshot(Path.Combine(dir,name+".png"));
            yield return WaitRealtime(2.4f);Capture("flow-main-wait");yield return null;Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();Call(stage,"DisqualifyRecord");
            var ui=Find("CursorDashRuntimeUI");Call(ui,"Open",true);Call(ui,"Close");for(int n=0;n<4;n++)Call(fever,"Review","kill");yield return WaitRealtime(.08f);Capture("flow-score-charge");yield return null;
            Vector2 center=(Vector2)Get(stage,"NexusPosition"),contact=center+Vector2.left*(float)Get(stage,"NexusRadius");Call(stage,"DamageNexusAt",1f,contact);
            var flash=Find("CameraFlash");var red=(UnityEngine.UI.Image)Get(flash,"DamageImage");if(red!=null)red.color=Color.clear; // This runtime-only red-free inspection never edits profile/PlayerPrefs.
            for(int n=0;n<6;n++){Capture("flow-no-red-pressure-"+n);yield return new WaitForSecondsRealtime(.025f);}
            Assert.Less(Vector2.Distance((Vector2)Get(Get(fever,"World"),"DamageViewportCenter"),(Vector2)Camera.main.WorldToViewportPoint(contact)),.002f);
            yield return WaitRealtime(.25f);Call(fever,"Review","fill");yield return WaitRealtime(1.35f);Assert.AreEqual("Fever",Get(model,"Current").ToString());Capture("flow-fever");yield return null;
            Call(fever,"Review","step");yield return WaitRealtime(1.1f);Assert.AreEqual("Normal",Get(model,"Current").ToString());Capture("flow-fever-restored");yield return null;
            Call(stage,"DamageNexus",2f);for(int n=0;n<28;n++){Capture("return-"+n.ToString("D2"));yield return WaitRealtime(.10f);}
            Assert.AreEqual("Returning",Get(stage,"State").ToString());Assert.IsTrue((bool)Call(space,"RequestReturnContinue",new Vector2(Screen.width*.5f,Screen.height*.5f)));for(int n=0;n<7;n++){Capture("ack-"+n);yield return WaitRealtime(.10f);}Assert.AreEqual("Main",Get(stage,"State").ToString());Assert.AreEqual(saved,PlayerPrefs.GetString("CursorDash_BestPoints_v1_Cyberpunk",""));Assert.AreEqual(1f,Time.timeScale);
        }
        [UnityTest] public IEnumerator ExplicitTrainingFromMainKeepsItsModeBeforeFirstGuide()
        {
            PlayerPrefs.SetString("CursorDash.v2.tutorial","new");
            var stage=Find("StageManager");Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");
            yield return WaitForScene("Cyberpunk");yield return null;
            Assert.AreSame(stage,Find("StageManager"));Assert.AreEqual(3,Get(Find("CursorDashPractice"),"Step"));
            Assert.IsFalse((bool)Get(stage,"RecordEligible"));Assert.AreEqual("new",PlayerPrefs.GetString("CursorDash.v2.tutorial"));
            Call(stage,"ReturnHome");yield return WaitForScene("01.StageSelectScene");
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");yield return null;
            Assert.AreEqual(1,Get(Find("CursorDashPractice"),"Step"));Assert.IsTrue((bool)Get(Find("CursorDashPractice"),"Guided"));
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
            string dir=Path.GetFullPath("Logs/nexus-hit-save-20261027/NativeCaptures");Directory.CreateDirectory(dir);
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
            Call(stage,"DamageNexus",1f);yield return null;Call(stage,"DamageNexus",1f);yield return WaitRealtime(.7f);Assert.AreEqual("Returning",Get(stage,"State").ToString());Capture("03-return-final");yield return null;
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
