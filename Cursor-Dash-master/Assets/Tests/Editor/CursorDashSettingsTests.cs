#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CursorDashTests
{
    public partial class CursorDashSceneRegression
    {
        private static readonly string SettingsFrames="Logs/settings-pause-20261028/Frames";
        private static void SettingsFrame(string name){System.IO.Directory.CreateDirectory(SettingsFrames);ScreenCapture.CaptureScreenshot(SettingsFrames+"/"+name+".png");}
        private static Slider Setting(Component ui,string key,int index=0)=>ui.GetComponentsInChildren<Slider>(true).Where(s=>s.name==key+" slider").ElementAt(index);
        private static float Gain(string key)=>(float)TypeFor("CursorDashTuning").GetMethod("EffectGain").Invoke(null,new object[]{key});
        private static object StaticSetting(string name)=>TypeFor("CursorDashTuning").GetMethod("Requested").Invoke(null,new object[]{name});
        // Route through the real EventSystem's sorted raycasts before dispatch. Direct
        // ExecuteEvents on a known target alone cannot detect a full-screen blocker.
        private static PointerEventData SettingsPointer(RectTransform target,float fraction=.5f)
        {
            Canvas.ForceUpdateCanvases();
            Vector2 point=RectTransformUtility.WorldToScreenPoint(null,target.TransformPoint(new Vector3(Mathf.Lerp(target.rect.xMin,target.rect.xMax,fraction),target.rect.center.y,0)));
            var e=new PointerEventData(EventSystem.current){position=point,pointerId=-1,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);
            Assert.IsNotEmpty(hits,"No UI raycast at "+target.name+" / "+point);
            var hit=hits[0];Assert.IsTrue(hit.gameObject.transform==target||hit.gameObject.transform.IsChildOf(target),"UI input for "+target.name+" is blocked by "+hit.gameObject.name);
            e.pointerCurrentRaycast=e.pointerPressRaycast=hit;return e;
        }
        private static void ClickSettings(Component ui,string name)
        {
            var button=ui.GetComponentsInChildren<Button>(true).Single(b=>b.name==name);
            var e=SettingsPointer(button.GetComponent<RectTransform>());
            ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerDownHandler);ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerUpHandler);ExecuteEvents.Execute(button.gameObject,e,ExecuteEvents.pointerClickHandler);
        }

        [UnityTest] public IEnumerator UI_MainSheetRealPointerEventsBlockStartAndSaveOnlyAtGestureEnd()
        {
            var stage=Find("StageManager");var ui=(Component)Get(stage,"SettingsUI");var space=Get(stage,"Space");yield return null;
            Assert.AreEqual("Main",Get(stage,"State").ToString());Assert.IsFalse((bool)Get(ui,"SheetOpen"));
            SettingsFrame("main-closed");yield return null;
            var handle=((RectTransform)Get(ui,"handle")).gameObject;var e=SettingsPointer(handle.GetComponent<RectTransform>());
            ExecuteEvents.Execute(handle,e,ExecuteEvents.pointerDownHandler);
            Assert.IsFalse((bool)Call(space,"RequestStart",new Vector2(Screen.width*.5f,Screen.height*.5f)));
            e.position=new Vector2(Screen.width*.5f,Screen.height*.85f);ExecuteEvents.Execute(handle,e,ExecuteEvents.dragHandler);ExecuteEvents.Execute(handle,e,ExecuteEvents.pointerUpHandler);
            yield return new WaitForSecondsRealtime(.3f);Assert.IsTrue((bool)Get(ui,"SheetOpen"));Assert.AreEqual("Main",Get(stage,"State").ToString());
            Assert.AreEqual(9,ui.GetComponentsInChildren<Slider>(true).Length);
            string before=PlayerPrefs.GetString("CursorDash.v2.user");var slider=Setting(ui,"user.master");var sliderPointer=SettingsPointer(slider.GetComponent<RectTransform>(),.31f);
            ExecuteEvents.Execute(slider.gameObject,sliderPointer,ExecuteEvents.pointerDownHandler);
            Assert.That(slider.value,Is.EqualTo(.31f).Within(.002f),"Real raycast and Slider pointer mapping must agree.");
            Assert.AreEqual(before,PlayerPrefs.GetString("CursorDash.v2.user"),"Dragging previews in memory; not a write each frame.");
            ExecuteEvents.Execute(slider.gameObject,sliderPointer,ExecuteEvents.pointerUpHandler);
            Assert.That(slider.handleRect.rect.width,Is.EqualTo(12).Within(.01));Assert.That(slider.handleRect.rect.height,Is.EqualTo(12).Within(.01),"Slider must not stretch the handle to its touch area height.");
            Assert.That(slider.transform.Find("Track").GetComponent<RectTransform>().rect.height,Is.EqualTo(4));
            Assert.GreaterOrEqual(slider.GetComponent<RectTransform>().rect.height,48);
            // Every main control must be above the shade, not just the first row.
            foreach(var control in ui.GetComponentsInChildren<Slider>(true).Where(s=>s.gameObject.activeInHierarchy))SettingsPointer(control.GetComponent<RectTransform>());
            SettingsPointer(ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Replay tutorial").GetComponent<RectTransform>());
            SettingsPointer(ui.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Reset audio and VFX").GetComponent<RectTransform>());
            SettingsPointer(handle.GetComponent<RectTransform>());
            Assert.That((double)JObject.Parse(PlayerPrefs.GetString("CursorDash.v2.user"))["user.master"],Is.EqualTo(.31).Within(.0001));
            SettingsFrame("main-open");yield return null;
            Call(ui,"SetSheet",false);Assert.IsFalse((bool)Call(space,"RequestStart",new Vector2(Screen.width*.5f,Screen.height*.5f)));
            yield return new WaitForSecondsRealtime(.3f);Assert.AreEqual("Main",Get(stage,"State").ToString());Assert.AreEqual(0f,(float)Get(ui,"SheetProgress"));
            // Small drags settle closed, larger upward drags settle open. No duplicate start.
            Call(ui,"BeginHandle",new Vector2(100,0));Call(ui,"DragHandle",new Vector2(100,2));Set(ui,"velocity",0f);Call(ui,"EndHandle",true);
            yield return new WaitForSecondsRealtime(.3f);Assert.IsFalse((bool)Get(ui,"SheetOpen"));
            Call(ui,"SetSheet",true);for(int i=0;i<12;i++){SettingsFrame("sheet-open-"+i.ToString("00"));yield return new WaitForSecondsRealtime(.03f);}
            Call(ui,"SetSheet",false);for(int i=0;i<12;i++){SettingsFrame("sheet-close-"+i.ToString("00"));yield return new WaitForSecondsRealtime(.03f);}
            var rect=(RectTransform)Get(ui,"content");Assert.Greater(rect.lossyScale.x,0);Assert.LessOrEqual(rect.rect.width*rect.localScale.x,((RectTransform)Get(ui,"safe")).rect.width);
        }

        [UnityTest] public IEnumerator UI_MasterAndReductionAreSingleMultipliersAndSelectiveResetProtectsRecords()
        {
            var ui=(Component)Get(Find("StageManager"),"SettingsUI");var sound=Find("SoundManager");
            string[] keys={"CursorDash.v2.tutorial","CursorDash.v3.feverGuide","CursorDash_BestPoints_v1_Cyberpunk","CursorDash.v2.presets"};var before=keys.Select(k=>PlayerPrefs.GetString(k,"<missing>")).ToArray();
            Call(ui,"SetValue","user.shake",.8f);Call(ui,"SetValue","user.flash",.6f);Call(ui,"SetValue","user.distortion",.4f);
            foreach(float reduction in new[]{0f,.5f,1f,0f}){
                Call(ui,"SetValue","user.vfxReduction",reduction);
                foreach(var detail in new[]{("shake",.8f),("flash",.6f),("distortion",.4f)}){
                    Assert.That(Gain("user."+detail.Item1),Is.EqualTo(detail.Item2*(1-reduction)).Within(.0001));
                    Assert.That((double)(JToken)StaticSetting("user."+detail.Item1),Is.EqualTo(detail.Item2).Within(.0001));
                }
            }
            Call(ui,"SetValue","user.sfx",.6f);Call(ui,"SetValue","user.bgm",.7f);Call(ui,"SetValue","user.master",.37f);
            Call(sound,"PlaySFX",Enum.Parse(TypeFor("ClipTag"),"nodeExplosion"),.8f,1f);
            var source=(AudioSource)Get(sound,"nodeExplosion");Assert.That(source.volume,Is.EqualTo(.6f*.37f*.8f).Within(.0001));
            Call(ui,"SetValue","user.master",0f);Assert.AreEqual(0,source.volume);Call(ui,"SetValue","user.master",1f);Assert.That(source.volume,Is.EqualTo(.6f*.8f).Within(.0001));
            Assert.That((float)Get(sound,"BGMVolume"),Is.EqualTo(.7).Within(.0001));Assert.That((float)Get(sound,"SFXVolume"),Is.EqualTo(.6).Within(.0001));
            Call(ui,"Save");var packed=JObject.Parse(PlayerPrefs.GetString("CursorDash.v2.user"));Assert.AreEqual(1,(double)packed["user.master"]);
            Call(ui,"SetValue","user.deadZone",.21f);Call(ui,"ResetSettings");Assert.That((double)(JToken)StaticSetting("user.deadZone"),Is.EqualTo(.21).Within(.0001));
            Assert.AreEqual(0,(double)(JToken)StaticSetting("user.vfxReduction"));Assert.AreEqual(1,(double)(JToken)StaticSetting("user.master"));
            for(int i=0;i<keys.Length;i++)Assert.AreEqual(before[i],PlayerPrefs.GetString(keys[i],"<missing>"),keys[i]);
            yield return null;
        }

        [UnityTest] public IEnumerator UI_PausePreservesStateAndSharedSlidersRetryHomeUseExistingFlow()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            var stage=Find("StageManager");var ui=(Component)Get(stage,"SettingsUI");var player=Find("PlayerCharacterController");var fever=Get(stage,"Fever");
            Call(Get(stage,"NodeCreator"),"CreateNode",0,0);var node=NodesOfType("NormalNode").First(n=>n.gameObject.activeSelf);
            Call(stage,"SetPaused",true);yield return null;float time=Time.time;Vector3 pos=player.transform.position;float charge=(float)Get(node,"PreparationProgress");
            Setting(ui,"user.master",1).value=.42f;Setting(ui,"user.vfxReduction",1).value=.5f;
            yield return new WaitForSecondsRealtime(.35f);Assert.AreEqual(0,Time.timeScale);Assert.IsTrue((bool)Get(ui,"PauseVisible"));Assert.AreEqual(time,Time.time);Assert.AreEqual(pos,player.transform.position);Assert.AreEqual(charge,Get(node,"PreparationProgress"));
            Assert.That(Setting(ui,"user.master").value,Is.EqualTo(.42).Within(.0001));Assert.That(Setting(ui,"user.vfxReduction").value,Is.EqualTo(.5).Within(.0001));
            SettingsPointer(Setting(ui,"user.master",1).GetComponent<RectTransform>());SettingsPointer(Setting(ui,"user.vfxReduction",1).GetComponent<RectTransform>());
            SettingsFrame("pause-open");yield return null;ClickSettings(ui,"Continue");
            for(int i=0;i<10;i++){SettingsFrame("pause-resume-"+i.ToString("00"));yield return new WaitForSecondsRealtime(.03f);}
            Assert.AreEqual(1,Time.timeScale);Assert.IsTrue((bool)Get(stage,"IsPlaying"));
            Call(stage,"SetPaused",true);yield return new WaitForSecondsRealtime(.2f);ClickSettings(ui,"Retry");yield return WaitForScene("Cyberpunk");DisablePatterns();Assert.AreEqual(1,Time.timeScale);
            Call(stage,"SetPaused",true);yield return new WaitForSecondsRealtime(.2f);ClickSettings(ui,"Home");yield return WaitForScene("01.StageSelectScene");Assert.AreEqual(1,Time.timeScale);Assert.AreEqual(0,(int)Get(stage,"NodeCreator").GetType().GetProperty("ActiveCount").GetValue(Get(stage,"NodeCreator")));
            Assert.IsTrue((bool)Get(Get(stage,"Space"),"IsMenu"));Assert.AreEqual("0",Get(Get(fever,"Model"),"PointsText"));
        }

        [UnityTest] public IEnumerator UI_TutorialReentryDoesNotEraseFlagsAndZeroEffectsKeepFeverGuideWhite()
        {
            var stage=Find("StageManager");var ui=(Component)Get(stage,"SettingsUI");string tutorial=PlayerPrefs.GetString("CursorDash.v2.tutorial"),guide=PlayerPrefs.GetString("CursorDash.v3.feverGuide");
            Call(ui,"SetSheet",true);yield return new WaitForSecondsRealtime(.3f);ClickSettings(ui,"Replay tutorial");Call(ui,"ReplayTutorial");yield return WaitForScene("Cyberpunk");
            Assert.IsTrue((bool)Get(Get(stage,"Practice"),"Guided"));Assert.AreEqual(tutorial,PlayerPrefs.GetString("CursorDash.v2.tutorial"));Assert.AreEqual(guide,PlayerPrefs.GetString("CursorDash.v3.feverGuide"));
            Call(stage,"SetPaused",true);yield return new WaitForSecondsRealtime(.2f);Assert.AreEqual(0,Time.timeScale);Call(ui,"Resume");yield return null;Assert.AreEqual(1,Time.timeScale);
            Call(ui,"Home");yield return WaitForScene("01.StageSelectScene");Call(Get(stage,"Space"),"LaunchSelectedMode");yield return WaitForScene("Cyberpunk");DisablePatterns();
            Call(ui,"SetValue","user.vfxReduction",1f);Call(ui,"SetValue","user.flash",0f);
            TypeFor("CursorDashSession").GetField("ReplayFeverNotice",BindingFlags.Public|BindingFlags.Static).SetValue(null,true);
            var fever=Get(stage,"Fever");Call(Get(fever,"Model"),"FillForReview");float deadline=Time.realtimeSinceStartup+5;
            while(!(bool)Get(fever,"NoticeActive")){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            Assert.AreEqual(0,Time.timeScale);Assert.AreEqual(1d,Get(Get(fever,"Model"),"Gauge"));
            // EditMode TestRunner resumes its enumerator on editor updates, not the
            // render callback. Read a fresh, asynchronously captured Game View PNG.
            string capture=System.IO.Path.GetFullPath(SettingsFrames+"/fever-guide-zero-effects-"+Guid.NewGuid().ToString("N")+".png");
            System.IO.Directory.CreateDirectory(SettingsFrames);ScreenCapture.CaptureScreenshot(capture);float captureDeadline=Time.realtimeSinceStartup+5;
            while(!System.IO.File.Exists(capture)){Assert.Less(Time.realtimeSinceStartup,captureDeadline);yield return null;}
            yield return null;var texture=new Texture2D(2,2);try{Assert.IsTrue(ImageConversion.LoadImage(texture,System.IO.File.ReadAllBytes(capture)));var pixel=texture.GetPixel(texture.width/10,texture.height/10);Assert.Greater(pixel.r,.85f);Assert.Greater(pixel.g,.85f);Assert.Greater(pixel.b,.85f);}finally{UnityEngine.Object.Destroy(texture);}
            Call(stage,"SetPaused",true);yield return new WaitForSecondsRealtime(.2f);Call(ui,"Resume");yield return null;Assert.AreEqual(0,Time.timeScale,"Resume restores notice-owned freeze, not an unconditional scale of one.");
            Call(ui,"Home");yield return WaitForScene("01.StageSelectScene");Assert.AreEqual(1,Time.timeScale);Assert.AreEqual(0,(int)Get(fever,"ActiveBonuses"));
        }
    }
}
#endif
