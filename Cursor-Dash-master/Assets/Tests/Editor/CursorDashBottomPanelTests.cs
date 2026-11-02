#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    /// <summary>No Play, game bootstrap, scene saves or PlayerPrefs writes.</summary>
    public sealed class CursorDashBottomPanelTests
    {
        private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
        private GameObject root;
        private Component view;
        private object userValues;
        private string prefs;
        private static Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Get(object obj,string name)=>obj.GetType().GetProperty(name).GetValue(obj);
        private static object Call(object obj,string name,params object[] args)=>obj.GetType().GetMethods().Single(m=>m.Name==name&&m.GetParameters().Length==args.Length&&m.GetParameters().Select((p,i)=>args[i]==null||p.ParameterType.IsInstanceOfType(args[i])).All(v=>v)).Invoke(obj,args);
        private static object Static(string name,params object[] args)=>T("CursorDashTuning").GetMethods().Single(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(null,args);
        private static object Panel(string value)=>Enum.Parse(T("CursorDashSettingsView+MenuPanel"),value);
        private static string Prefs()=>string.Join("|",new[]{"CursorDash.v2.user","CursorDash.v2.presets","CursorDash.v2.tutorial","CursorDash.v3.feverGuide","CursorDash_BestPoints_v1_Cyberpunk"}.Select(k=>PlayerPrefs.HasKey(k)+":"+PlayerPrefs.GetString(k,"")));
        private void IsolateUser(string json){var field=T("CursorDashTuning").GetField("userValues",Flags);field.SetValue(null,field.FieldType.GetMethod("Parse",new[]{typeof(string)}).Invoke(null,new object[]{json}));}
        [SetUp]public void Setup()
        {
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
            prefs=Prefs();userValues=T("CursorDashTuning").GetField("userValues",Flags).GetValue(null);
            root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/UI/SettingsView.prefab");view=root.GetComponent(T("CursorDashSettingsView"));
        }
        [TearDown]public void Cleanup()
        {
            if(root!=null)PrefabUtility.UnloadPrefabContents(root);
            T("CursorDashTuning").GetField("userValues",Flags).SetValue(null,userValues);
            Assert.AreEqual(prefs,Prefs());Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);Assert.IsFalse(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty);
        }
        [Test]public void SavedViewHasTwoNamedHeadersAndRenderableIcons()
        {
            Assert.IsTrue((bool)Get(view,"HasMenuPanels"));Assert.IsTrue((bool)Get(view,"IsConfigured"));Assert.AreEqual(9,((Array)Get(view,"Rows")).Length);
            var nav=(RectTransform)Get(view,"NavigationPlacement");var grips=nav.GetComponentsInChildren(T("CursorDashSheetHandle"),true);Assert.AreEqual(2,grips.Length);
            CollectionAssert.AreEquivalent(new[]{"Ranking","Settings"},grips.Select(g=>g.GetType().GetField("panel").GetValue(g).ToString()));
            foreach(var icon in root.GetComponentsInChildren(T("CursorDashMenuIcon"),true)){Assert.IsNotNull(icon.GetComponent<CanvasRenderer>(),icon.name);Assert.IsNotNull(((Image)icon).sprite,icon.name);Assert.IsTrue(((Image)icon).preserveAspect);Assert.Greater(icon.GetComponent<RectTransform>().rect.width,10);}
            Assert.IsNull(root.GetComponentsInChildren<Button>(true).FirstOrDefault(b=>b.name=="Reset audio and VFX"));
        }
        [TestCase("Ranking")][TestCase("Settings")]
        public void OnlyChosenHeaderTravelsAndRisingBodyCoversOpaqueOppositeHeader(string panel)
        {
            Call(view,"Layout",new Rect(0,0,1080,1920),new Vector2(1080,1920));Call(view,"SelectPanel",Panel(panel));
            var transforms=((Array)Get(view,"MenuAnimationTransforms")).Cast<RectTransform>().ToArray();var groups=((Array)Get(view,"MenuOpacityGroups")).Cast<CanvasGroup>().ToArray();int chosen=panel=="Ranking"?0:1,other=1-chosen;
            Call(view,"Present",true,0f,0f,true);Assert.AreEqual(0,transforms[chosen].anchoredPosition.y);Assert.AreEqual(1,groups[other].alpha);
            var arrows=((Array)Get(view,"MenuDirectionGraphics")).Cast<Graphic>().ToArray();Assert.IsTrue(arrows.All(g=>g!=null&&!g.enabled),"Closed tabs have no down arrows");
            foreach(float progress in new[]{.01f,.05f,.2f,.6f}){
                Call(view,"Present",true,progress,0f,true);Assert.Greater(transforms[chosen].anchoredPosition.y,0);Assert.AreEqual(0,transforms[other].anchoredPosition.y);Assert.AreEqual(1,groups[other].alpha,"The opposite header must be covered, not faded");
            }
            Assert.IsFalse(groups[other].blocksRaycasts);
            var nav=(RectTransform)Get(view,"NavigationPlacement");var sheet=(RectTransform)Get(view,"SheetAnimation");Assert.AreSame(nav.parent,sheet.parent);Assert.Less(nav.GetSiblingIndex(),sheet.GetSiblingIndex(),"Body draws and receives raycasts above the stationary header");Assert.AreEqual(1,((Image)Get(view,"SheetSurface")).color.a);
            Call(view,"Present",true,1f,0f,true);Assert.That(groups[chosen].alpha,Is.EqualTo(1));Assert.That(((CanvasGroup)Get(view,"ContentOpacity")).alpha,Is.EqualTo(panel=="Settings"?1:0));Assert.That(groups[2].alpha,Is.EqualTo(panel=="Ranking"?1:0));
            Assert.IsTrue(arrows[chosen].enabled);Assert.IsFalse(arrows[other].enabled);
            Canvas.ForceUpdateCanvases();var surface=(Image)Get(view,"SheetSurface");Assert.IsNotNull(surface);Assert.IsFalse(((RectTransform)Get(view,"SheetAnimation")).GetComponent<Image>().enabled,"No full top-strip rectangle behind the other tab");
            var bodyCorners=new Vector3[4];var titleCorners=new Vector3[4];surface.rectTransform.GetWorldCorners(bodyCorners);transforms[chosen].GetWorldCorners(titleCorners);
            Assert.That(bodyCorners[1].y,Is.EqualTo(titleCorners[0].y).Within(.02f),"The full-width body starts at the selected title's bottom edge");
            var safe=(RectTransform)Get(view,"SafeArea");var safeCorners=new Vector3[4];safe.GetWorldCorners(safeCorners);Assert.That(titleCorners[1].y,Is.EqualTo(safeCorners[1].y).Within(.02f),"The selected title reaches the top edge");
            Call(view,"Present",false,0f,0f,false);Assert.IsFalse(((RectTransform)Get(view,"NavigationPlacement")).gameObject.activeSelf);
        }
        [Test]public void PanelMotionKeepsAuthoredPlacementAndPauseControls()
        {
            var placement=(RectTransform)new SerializedObject(view).FindProperty("contentPlacement").objectReferenceValue;placement.anchoredPosition+=new Vector2(9,-11);var before=placement.anchoredPosition;
            var pause=(RectTransform)new SerializedObject(view).FindProperty("pausePlacement").objectReferenceValue;var p=pause.anchoredPosition;var size=pause.sizeDelta;
            foreach(float t in new[]{0f,.3f,1f,.4f,0f})Call(view,"Present",true,t,0f,true);
            Assert.AreEqual(before,placement.anchoredPosition);Assert.AreEqual(p,pause.anchoredPosition);Assert.AreEqual(size,pause.sizeDelta);
            Call(view,"Present",false,0f,1f,true);Assert.IsTrue(((CanvasGroup)Get(view,"PauseOpacity")).gameObject.activeSelf);Assert.AreEqual(2,pause.GetComponentsInChildren<Slider>(true).Length);
        }
        [Test]public void SoundResetPreservesEffectsLanguageAndUnrelatedPreferences()
        {
            IsolateUser("{\"user.master\":0.1,\"user.bgm\":0.2,\"user.sfx\":0.3,\"user.shake\":0.47,\"user.flash\":0.38,\"menu.language\":\"ja\",\"custom.keep\":42}");Static("ResetAudio");
            foreach(string key in new[]{"user.master","user.bgm","user.sfx"}){var parameter=Static("Find",key);Assert.AreEqual(parameter.GetType().GetField("defaultValue").GetValue(parameter).ToString(),Static("Requested",key).ToString());}
            Assert.AreEqual("0.47",Static("Requested","user.shake").ToString());Assert.AreEqual("ja",T("CursorDashTuning").GetProperty("MenuLanguage").GetValue(null));Assert.IsTrue(T("CursorDashTuning").GetField("userValues",Flags).GetValue(null).ToString().Contains("custom.keep"));
        }
        [Test]public void EffectsResetPreservesAllSoundValuesAndLanguage()
        {
            IsolateUser("{\"user.master\":0.1,\"user.bgm\":0.2,\"user.sfx\":0.3,\"user.shake\":0.47,\"menu.language\":\"en\"}");Static("ResetVisual");
            Assert.AreEqual("0.1",Static("Requested","user.master").ToString());Assert.AreEqual("0.2",Static("Requested","user.bgm").ToString());Assert.AreEqual("0.3",Static("Requested","user.sfx").ToString());
            foreach(string key in new[]{"user.vfxReduction","user.shake","user.flash","user.distortion"}){var parameter=Static("Find",key);Assert.AreEqual(parameter.GetType().GetField("defaultValue").GetValue(parameter).ToString(),Static("Requested",key).ToString());}
            Assert.AreEqual("en",T("CursorDashTuning").GetProperty("MenuLanguage").GetValue(null));
        }
        [Test]public void LanguageCyclesInExistingUserDataWithoutChangingDeveloperSchemaOrStorage()
        {
            IsolateUser("{\"menu.language\":\"ko\",\"user.master\":0.63}");int count=((System.Collections.ICollection)T("CursorDashTuning").GetField("Parameters").GetValue(null)).Count;
            foreach(string expected in new[]{"en","ja","zh-Hans","ko"}){Static("CycleMenuLanguage");Assert.AreEqual(expected,T("CursorDashTuning").GetProperty("MenuLanguage").GetValue(null));}
            Assert.AreEqual(count,((System.Collections.ICollection)T("CursorDashTuning").GetField("Parameters").GetValue(null)).Count);Assert.AreEqual("0.63",Static("Requested","user.master").ToString());
            IsolateUser("{\"menu.language\":{\"bad\":true}}");Assert.AreEqual("ko",T("CursorDashTuning").GetProperty("MenuLanguage").GetValue(null));
        }
        [Test]public void RankingDisplayDoesNotChangeActualBestScore()
        {
            Call(view,"SetRankingBest","12345678901234567890");Call(view,"SelectPanel",Panel("Ranking"));Call(view,"Present",true,1f,0f,false);Assert.AreEqual(prefs,Prefs());
            var label=(Component)new SerializedObject(view).FindProperty("rankingBest").objectReferenceValue;Assert.AreEqual("12345678901234567890",Get(label,"text"));Assert.IsTrue((bool)Get(label,"enableAutoSizing"));
        }
        [Test]public void FourMenuLanguagesHaveAllGlyphsAndRestoreAuthoredKoreanFont()
        {
            var data=new SerializedObject(view);Assert.IsTrue(data.FindProperty("localizeMenuCopy").boolValue);
            var entries=data.FindProperty("menuTexts");var first=(Component)entries.GetArrayElementAtIndex(0).FindPropertyRelative("label").objectReferenceValue;
            string korean=(string)Get(first,"text");Call(view,"ApplyMenuLanguage","ko");var original=Get(first,"font");
            foreach(string language in new[]{"en","ja","zh-Hans","ko"})
            {
                Call(view,"ApplyMenuLanguage",language);
                var languageCode=(Component)data.FindProperty("languageLabel").objectReferenceValue;Assert.AreEqual(language=="ja"?"JP":language=="zh-Hans"?"CN":language=="en"?"EN":"KO",Get(languageCode,"text"));
                for(int n=0;n<entries.arraySize;n++)
                {
                    var label=(Component)entries.GetArrayElementAtIndex(n).FindPropertyRelative("label").objectReferenceValue;var font=Get(label,"font");
                    label.GetType().GetMethod("ForceMeshUpdate").Invoke(label,new object[]{true,true});
                    if(language=="ja"||language=="zh-Hans")Assert.IsTrue(((Object)font).name.StartsWith("Sarasa UI"));
                    var glyphs=(System.Collections.IDictionary)Get(font,"characterLookupTable");
                    foreach(char c in (string)Get(label,"text"))if(!char.IsControl(c))Assert.IsTrue(glyphs.Contains((uint)c),language+" missing "+c+" in "+label.name);
                    Assert.IsFalse(EditorUtility.IsPersistent((Object)font),"Rendering must use the owned font copy.");
                }
            }
            Assert.AreEqual(korean,Get(first,"text"));Assert.AreSame(original,Get(first,"font"));
            foreach(string field in new[]{"japaneseMenuFont","chineseMenuFont"}){var source=data.FindProperty(field).objectReferenceValue;Assert.IsNotNull(source);Assert.IsFalse(AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(source)).Any(p=>p.StartsWith("Assets/Editor/")),"Runtime static atlas must not depend on editor-only TTF source");}
        }
        [Test]public void FooterHasEqualNonOverlappingTargetsAndMatchingArtworkHeight()
        {
            var data=new SerializedObject(view);var tutorial=(Button)data.FindProperty("replayTutorial").objectReferenceValue;var locale=(Button)data.FindProperty("languageButton").objectReferenceValue;
            var left=(RectTransform)tutorial.transform;var right=(RectTransform)locale.transform;
            Assert.AreEqual(left.rect.width,right.rect.width);Assert.AreEqual(left.rect.height,right.rect.height);Assert.AreEqual(-left.anchoredPosition.x,right.anchoredPosition.x);Assert.AreEqual(left.anchoredPosition.y,right.anchoredPosition.y);
            Assert.That(left.anchoredPosition.x+left.rect.width*.5f,Is.LessThanOrEqualTo(right.anchoredPosition.x-right.rect.width*.5f));
            var globe=(RectTransform)locale.transform.Find("Language globe");var flag=(RectTransform)locale.transform.Find("Selected language flag");var code=(Component)data.FindProperty("languageLabel").objectReferenceValue;
            Assert.AreEqual(18,globe.rect.height);Assert.AreEqual(globe.rect.height,flag.rect.height);Assert.AreEqual(18,Get(code,"fontSize"));Assert.AreEqual(globe.anchoredPosition.y,flag.anchoredPosition.y);Assert.AreEqual(0,((RectTransform)code.transform).anchoredPosition.y);
        }
        [TestCase(false)][TestCase(true)]public void FullHeightMenuKeepsFooterWithinReducedSafeArea(bool wide)
        {
            var pixels=wide?new Vector2(1920,1080):new Vector2(1080,1920);Call(view,"Layout",new Rect(0,pixels.y*.05f,pixels.x,pixels.y*.9f),pixels);Call(view,"Present",true,1f,0f,true);Canvas.ForceUpdateCanvases();
            var safe=(RectTransform)Get(view,"SafeArea");var sheet=(RectTransform)Get(view,"SheetAnimation");Assert.That(sheet.rect.height,Is.EqualTo(safe.rect.height).Within(.01f));
            var corners=new Vector3[4];safe.GetWorldCorners(corners);var data=new SerializedObject(view);
            foreach(var field in new[]{"replayTutorial","languageButton"}){var button=(Button)data.FindProperty(field).objectReferenceValue;var target=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(target);Assert.That(target[0].y,Is.GreaterThanOrEqualTo(corners[0].y),field+" below safe area");Assert.That(target[1].y,Is.LessThanOrEqualTo(corners[1].y));}
        }
    }
}
#endif
