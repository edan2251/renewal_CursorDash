#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    public sealed class CursorDashWorldAuthoringTests
    {
        private static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Get(object value,string key)=>value.GetType().GetProperty(key).GetValue(value);
        private static object Field(object value,string key)=>value.GetType().GetField(key).GetValue(value);
        private static Component Stage()=>Object.FindFirstObjectByType(Runtime("StageManager")) as Component;
        [SetUp] public void SavedSceneOnly()
        {
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);
            for(int i=0;i<SceneManager.sceneCount;i++)Assert.IsFalse(SceneManager.GetSceneAt(i).isDirty);
            if(SceneManager.GetActiveScene().path!="Assets/Scenes/Cyberpunk.unity")EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity");
        }
        [TestCase(1d,1.7777778d,12)] [TestCase(1.7777778d,1d,12)] [TestCase(1d,1d,4)]
        public void AuthoredCircuitPreservesTopologyLengthsAndCoordinates(double x,double y,int density)
        {
            var layout=Resources.Load("CursorDash/CircuitLayout",Runtime("CursorDashCircuitLayout"));Assert.IsNotNull(layout);
            var actual=layout.GetType().GetMethod("CreateGraph").Invoke(layout,new object[]{x,y,density});
            var expected=Activator.CreateInstance(Runtime("CircuitPathGraph"),new object[]{x,y,density});
            CollectionAssert.AreEqual((int[])Field(expected,"Roots"),(int[])Field(actual,"Roots"));
            var points=(Array)Field(expected,"Points");var edited=(Array)Field(actual,"Points");Assert.AreEqual(points.Length,edited.Length);
            for(int i=0;i<points.Length;i++)foreach(string axis in new[]{"X","Y"})Assert.That((double)Field(edited.GetValue(i),axis),Is.EqualTo((double)Field(points.GetValue(i),axis)).Within(.0000002));
            var edges=(Array)Field(expected,"Edges");var mapped=(Array)Field(actual,"Edges");Assert.AreEqual(edges.Length,mapped.Length);
            for(int i=0;i<edges.Length;i++){
                foreach(string field in new[]{"A","B","Root"})Assert.AreEqual(Field(edges.GetValue(i),field),Field(mapped.GetValue(i),field));
                foreach(string field in new[]{"Length","Depth"})Assert.That((double)Field(mapped.GetValue(i),field),Is.EqualTo((double)Field(edges.GetValue(i),field)).Within(.000001));
            }
        }
        [Test] public void NexusAndGuidanceUseExplicitSourcesWithoutExtraControllers()
        {
            var stage=Stage();var nexus=(Component)Get(stage,"NexusView");var guide=(Component)Get(stage,"GuidanceView");Assert.IsTrue((bool)Get(nexus,"IsConfigured"));Assert.IsTrue((bool)Get(guide,"IsConfigured"));
            Assert.AreSame(Get(stage,"NexusBodySource"),Get(nexus,"Body"));Assert.AreSame(Get(stage,"NexusFrameSource"),Get(nexus,"Outer"));
            Assert.AreEqual(.14f,Get(nexus,"GaugeThickness"));Assert.AreEqual(4f,Get(nexus,"GaugeGapDegrees"));
            Assert.AreEqual(1.86f,Get(nexus,"GaugeRadiusRatio"));
            Assert.IsTrue(PrefabUtility.IsPartOfPrefabInstance(guide));Assert.IsNull(Object.FindFirstObjectByType(Runtime("CursorDashFever")));
            Assert.AreEqual("원형노드 파괴 1/2\n육각노드로부터 수비 2/2  [완료]",guide.GetType().GetMethod("Checklist").Invoke(guide,new object[]{1,2}));
            foreach(var child in guide.GetComponentsInChildren<Transform>(true))Assert.AreEqual(0,GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject));
        }
        [Test] public void NodeArtworkDefaultsMatchApprovedStyleAndAreNotCombatValues()
        {
            var style=Resources.Load("CursorDash/NodeAppearance",Runtime("CursorDashNodeAppearance"));Assert.IsNotNull(style);
            Assert.AreEqual(.035f,Field(style,"outline"));Assert.AreEqual(0f,Field(style,"core"));Assert.AreEqual(.82f,Field(style,"secondHexRingBrightness"));
            Assert.AreEqual((Color)new Color32(203,73,255,255),Field(style,"circularFill"));
            foreach(var field in style.GetType().GetFields())Assert.IsFalse(new[]{"damage","score","speed","attack","prepare"}.Contains(field.Name));
        }
    }
}
#endif
