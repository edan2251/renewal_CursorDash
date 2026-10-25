using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CursorDashTests
{
    // Authored/compiled in C. NOT executed automatically: the shared fixture enters Play and protects prefs.
    public partial class CursorDashSceneRegression
    {
        [UnityTest] public IEnumerator Fever_EntryClearsThreatsWithoutRewardsDamageOrDelayedAttacks()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Set(stage,"PracticeInvulnerable",false);
            var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var creator=(Component)Get(stage,"NodeCreator");Call(creator,"SpawnPreviewAt",0,new Vector2(-1,2));Call(creator,"SpawnPreviewAt",1,new Vector2(1,2));
            Assert.AreEqual(2,Get(creator,"ActiveCount"));var fever=(Component)Get(stage,"Fever");var model=Get(fever,"Model");
            Call(fever,"Review","fill");Call(fever,"Review","step");Assert.AreEqual("Entering",Get(model,"Current").ToString());
            Assert.AreEqual(2,Get(fever,"LastClearedThreats"));Assert.AreEqual(0,Get(creator,"ActiveCount"));Assert.AreEqual("0",Get(model,"PointsText"));
            Assert.IsNull(Call(creator,"SpawnPreviewAt",0,Vector2.up));Call(stage,"DamageNexus",1f);
            Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
            double clock=(double)Get(stage,"NormalCombatTime");Call(fever,"Review","step");yield return WaitGameplay(.3f);
            Assert.AreEqual("Fever",Get(model,"Current").ToString());Assert.Greater((int)Get(fever,"ActiveBonuses"),0);
            Assert.That((double)Get(stage,"NormalCombatTime"),Is.EqualTo(clock).Within(.00001));
            Assert.AreEqual("0",Get(model,"PointsText"));Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
            foreach(var node in AllNodes())Assert.AreEqual("Inactive",Get(node,"Phase").ToString());
        }
        [UnityTest] public IEnumerator Fever_BonusBodyContactAwardsOnceButAfterimageExpiryAndCleanupDoNot()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");var model=Get(fever,"Model");
            Call(fever,"Review","fill");Call(fever,"Review","step");Call(fever,"Review","step");yield return WaitGameplay(.32f);
            var bonus=Resources.FindObjectsOfTypeAll(TypeFor("FeverBonusNode")).Cast<Component>().First(b=>b.gameObject.activeInHierarchy);
            double gauge=(double)Get(model,"Gauge");var fakeTrail=new GameObject("Fever test afterimage");var trailCollider=fakeTrail.AddComponent<BoxCollider2D>();
            try{Call(bonus,"OnTriggerEnter2D",trailCollider);Assert.AreEqual("0",Get(model,"PointsText"));Assert.IsTrue(bonus.gameObject.activeSelf);}
            finally{Object.Destroy(fakeTrail);}
            var player=Find("PlayerCharacterController");Call(stage,"SetPaused",true);Call(bonus,"OnTriggerEnter2D",player.GetComponent<Collider2D>());
            Assert.AreEqual("0",Get(model,"PointsText"));Assert.IsTrue(bonus.gameObject.activeSelf);Call(stage,"SetPaused",false);
            Call(bonus,"OnTriggerStay2D",player.GetComponent<Collider2D>());Call(bonus,"OnTriggerStay2D",player.GetComponent<Collider2D>());
            Assert.AreEqual("1",Get(model,"PointsText"));Assert.AreEqual(gauge,Get(model,"Gauge"));Assert.IsFalse(bonus.gameObject.activeSelf);
            var another=Resources.FindObjectsOfTypeAll(TypeFor("FeverBonusNode")).Cast<Component>().First(b=>b.gameObject.activeInHierarchy);
            Call(another,"Advance",100f);Assert.AreEqual("1",Get(model,"PointsText"));
            Call(fever,"Review","abort");Assert.AreEqual(0,Get(fever,"ActiveBonuses"));Assert.AreEqual("1",Get(model,"PointsText"));Assert.AreEqual(0,Get(model,"CompletedFevers"));
        }
        [UnityTest] public IEnumerator Fever_PauseFreezesPhaseAndCancelRestoresDottedTransformAndThreatGate()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");var model=Get(fever,"Model");
            var dotted=GameObject.Find("UI/HpCentral").transform;Vector3 original=dotted.localScale;
            Call(fever,"Review","fill");Call(fever,"Review","step");yield return WaitGameplay(.2f);
            Assert.Less(dotted.localScale.magnitude,original.magnitude);Call(stage,"SetPaused",true);yield return null;
            Vector3 paused=dotted.localScale;double elapsed=(double)Get(model,"Elapsed");yield return WaitRealtime(.12f);
            Assert.AreEqual(paused,dotted.localScale);Assert.AreEqual(elapsed,Get(model,"Elapsed"));
            Call(fever,"Review","abort");Assert.AreEqual(original,dotted.localScale);Assert.IsFalse((bool)Get(stage,"NormalThreatsSuppressed"));Assert.AreEqual(1,Get(model,"Multiplier"));
            Call(stage,"SetPaused",false);yield return WaitGameplay(.05f);Assert.AreEqual(1f,Time.timeScale);
            Assert.IsTrue((bool)Get(fever,"DottedControlled"));Assert.IsFalse(((Behaviour)dotted.GetComponent(TypeFor("Rotator"))).enabled,"New bounded dotted-ring owner replaces the constant rotator during this round.");
            Assert.IsFalse((bool)Get(stage,"RecordEligible"));
        }
    }

    // Standalone EditMode test: no shared Play fixture, scene load/save or PlayerPrefs changes.
    public sealed class CursorDashFeverCompositionTests
    {
        [Test] public void BonusVisualUsesSeparateChildAndSurvivesPoolReuse()
        {
            var runtime=System.AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("FeverBonusNode")).First(t=>t!=null);
            var root=new GameObject("Fever composition test"){hideFlags=HideFlags.HideAndDontSave};root.SetActive(false);
            Mesh mesh=null;Material material=null;
            try{
                var sprite=root.AddComponent<SpriteRenderer>();var collider=root.AddComponent<CircleCollider2D>();collider.isTrigger=true;
                var node=root.AddComponent(runtime);
                // EditMode does not depend on automatic Play lifecycle dispatch for the base caches.
                runtime.GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.DeclaredOnly).Invoke(node,null);
                mesh=new Mesh{hideFlags=HideFlags.HideAndDontSave};
                var shader=Shader.Find("Sprites/Default");Assert.IsNotNull(shader);
                material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
                Assert.DoesNotThrow(()=>runtime.GetMethod("Bind").Invoke(node,new object[]{null,mesh,material}));
                Assert.IsNull(root.GetComponent<MeshFilter>());Assert.IsNull(root.GetComponent<MeshRenderer>());
                var filter=root.GetComponentInChildren<MeshFilter>(true);Assert.IsNotNull(filter);Assert.AreNotSame(root,filter.gameObject);
                var view=filter.GetComponent<MeshRenderer>();Assert.IsNotNull(view);
                Assert.AreSame(mesh,filter.sharedMesh);Assert.AreSame(material,view.sharedMaterial);Assert.AreEqual(22,view.sortingOrder);
                Assert.AreSame(root.transform,filter.transform.parent);Assert.AreEqual(Vector3.zero,filter.transform.localPosition);
                Assert.AreEqual(Quaternion.identity,filter.transform.localRotation);Assert.AreEqual(Vector3.one,filter.transform.localScale);
                for(int cycle=1;cycle<=2;cycle++){
                    runtime.GetMethod("Spawn").Invoke(node,new object[]{new Vector2(1,2),.2f,2.8f,.3f});
                    Assert.AreEqual((ulong)cycle,runtime.GetProperty("ExecutionId").GetValue(node));
                    Assert.IsTrue(root.activeSelf&&view.gameObject.activeInHierarchy&&collider.enabled);
                    Assert.AreEqual(Color.clear,sprite.color);Assert.AreEqual(new Vector3(1,2,0),root.transform.position);
                    Assert.AreEqual(Vector3.one*.14f,root.transform.localScale);
                    runtime.GetMethod("ResetObject").Invoke(node,null);
                    Assert.IsFalse(root.activeSelf);Assert.AreEqual("Inactive",runtime.GetProperty("Phase").GetValue(node).ToString());
                    Assert.AreSame(view,root.GetComponentInChildren<MeshRenderer>(true));Assert.AreEqual(1,root.transform.childCount);
                }
            }finally{
                Object.DestroyImmediate(root);if(mesh!=null)Object.DestroyImmediate(mesh);if(material!=null)Object.DestroyImmediate(material);
            }
        }
    }
}
