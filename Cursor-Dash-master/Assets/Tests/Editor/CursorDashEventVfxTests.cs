using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    public class CursorDashOriginalBurstAssetTests
    {
        [Test]
        public void OriginalCircleBurst_HasEightKeysTwelveFpsAndOnePointFiveControllerSpeed()
        {
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animations/Nodes/NormalNode_Interaction.anim");
            Assert.IsNotNull(clip);Assert.AreEqual(12,clip.frameRate);Assert.That(clip.length,Is.EqualTo(8f/12).Within(.0001f));
            Assert.IsEmpty(AnimationUtility.GetAnimationEvents(clip),"Reusing this visual must never duplicate gameplay callbacks.");
            var binding=AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
            var keys=AnimationUtility.GetObjectReferenceCurve(clip,binding);Assert.AreEqual(8,keys.Length);Assert.IsTrue(keys.All(k=>k.value is Sprite));
            for(int i=0;i<keys.Length;i++)Assert.That(keys[i].time,Is.EqualTo(i/12f).Within(.0001f));
            var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animations/Nodes/NormalNodeEffect.controller");
            var state=controller.layers[0].stateMachine.defaultState;Assert.AreEqual(clip,state.motion);Assert.AreEqual(1.5f,state.speed);
            Assert.That(clip.length/state.speed,Is.EqualTo(8f/18).Within(.0001f));
        }
    }
    public partial class CursorDashSceneRegression
    {
        [UnityTest]
        public IEnumerator CircleA_IrregularChunksHaveNoSheetLeaveCorridorAndOwnPoolLifetime()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var circle=NodesOfType("NormalNode").First();circle.gameObject.SetActive(true);circle.transform.position=new Vector2(0,2);Call(circle,"Execute");
            Call(circle,"ContactAt",new Vector2(-.2f,2),Vector2.right);
            var effect=Resources.FindObjectsOfTypeAll(TypeFor("CircularBreakVfx")).Cast<Component>().Single(c=>c.gameObject.scene.IsValid());
            Assert.IsFalse((bool)Get(effect,"UsesOriginalSheet"));Assert.IsNull(effect.GetComponentInChildren<Animator>());Assert.IsNull(effect.GetComponentInChildren<SpriteRenderer>());
            Assert.Greater((float)Get(effect,"ClearCorridorHalfWidth"),0);
            var velocities=(Vector3[])Get(effect,"velocities");Assert.IsTrue(velocities.Any(v=>v.y>0)&&velocities.Any(v=>v.y<0),"Shards must open both sides of the travel corridor.");
            var basis=(Vector3[])Get(effect,"basis");var owners=(int[])Get(effect,"owners");var center=(Vector3)Get(effect,"center");float radius=(float)Get(effect,"radius");
            Assert.IsFalse(owners.Contains(-2),"Remove the two solid split-blast masses.");
            Assert.Greater(basis.Where((v,i)=>owners[i]>=0).Max(v=>Vector3.Distance(v,center)),radius*1.1f,"Chunks start wider than the body, with no delayed growing plume.");
            Assert.IsTrue(basis.Where((v,i)=>owners[i]>=0).All(v=>Mathf.Abs(v.y-center.y)>=radius*.15f-.00001f));
            Assert.Greater(velocities.Max(v=>v.magnitude)-velocities.Min(v=>v.magnitude),1,"Irregular chunk velocities, not mirrored plates.");
            Call(effect,"Draw",.12f);var color=(Color[])Get(effect,"colors");
            Assert.IsTrue(color.Where((v,i)=>owners[i]>=0).Any(v=>v.a>.75f),"Visible fragments outlast the initial cursor hit stop/crossing.");
            Call(circle,"ResetObject");yield return WaitRealtime(.1f);Assert.IsTrue(effect!=null,"Independent visual survives node pool reset.");
            Call(Find("StageManager"),"SetPaused",true);yield return null;Assert.IsTrue(effect==null,"Pause cleanup must also own the flat blast.");
        }
        [UnityTest]
        public IEnumerator NexusC_ReboundsOppositeIncomingFromFourSidesAndCommitsOneDamagePerCall()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Set(stage,"PracticeInvulnerable",false);var score=Get(stage,"scoreManager");float start=(float)Get(score,"Hp");
            var feedback=Get(stage,"Feedback");var center=(Vector2)Get(stage,"NexusPosition");float radius=(float)Get(stage,"NexusRadius");
            int step=0;foreach(Vector2 direction in new[]{Vector2.up,Vector2.left,Vector2.down,Vector2.right}){
                Call(feedback,"Clear");yield return null;
                Call(stage,"DamageNexusFromCircle",.1f,center-direction*radius,direction,Color.magenta,.4f);
                var effect=Resources.FindObjectsOfTypeAll(TypeFor("NexusImpactVfx")).Cast<Component>().Single(c=>c.gameObject.scene.IsValid());
                Assert.AreEqual("Circle",Get(effect,"SourceCause").ToString());Assert.AreEqual(22,Get(effect,"PieceCount"));
                Assert.Less(Vector2.Dot((Vector3)Get(effect,"MeanVelocity"),direction),-.5f);
                Assert.That((float)Get(score,"Hp"),Is.EqualTo(start-(++step)*.1f).Within(.00001f));
            }
        }
        [UnityTest]
        public IEnumerator NexusD_ActualUnblockedBeamEmitsOneContactWithoutChangingDamage()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Set(stage,"PracticeInvulnerable",false);var player=Find("PlayerCharacterController");
            player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);Call(Find("PlayerCharacterAfterImage"),"ClearTrail");
            var feedback=Get(stage,"Feedback");int Count()=>((IEnumerable)Get(feedback,"Recent")).Cast<object>().Count(e=>(string)Get(e,"cause")=="D_laserNexus");int count=Count();
            var hex=NodesOfType("ArrowNode").First();hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2.3f);
            ConfigureHexPreparation(hex,.05f);Set(hex,"warningDuration",.05f);Call(hex,"Execute");
            float deadline=Time.realtimeSinceStartup+4;while((int)Get(hex,"ImpactsThisExecution")==0){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            Assert.AreEqual(2f,Get(Get(stage,"scoreManager"),"Hp"));Assert.AreEqual(count+1,Count());
            yield return WaitRealtime(.12f);Assert.AreEqual(count+1,Count());
            var contact=Resources.FindObjectsOfTypeAll(TypeFor("NexusImpactVfx")).Cast<Component>().Single(c=>c.gameObject.scene.IsValid());
            Assert.AreEqual("Laser",Get(contact,"SourceCause").ToString());Assert.AreEqual(18,Get(contact,"PieceCount"));
        }
        [UnityTest]
        public IEnumerator NexusFinal_IsExclusiveUsesOneSlowClockAndRetryRestoresTimeAndInput()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Set(stage,"PracticeInvulnerable",false);var score=Get(stage,"scoreManager");Set(score,"Hp",.5f);
            var feedback=Get(stage,"Feedback");object[] Events()=>((IEnumerable)Get(feedback,"Recent")).Cast<object>().ToArray();
            int before=Events().Length;var center=(Vector2)Get(stage,"NexusPosition");float radius=(float)Get(stage,"NexusRadius");
            Call(stage,"DamageNexusFromCircle",1f,center-Vector2.right*radius,Vector2.right,Color.magenta,.4f);
            Assert.IsTrue((bool)Get(stage,"IsDeath"));Assert.AreEqual(0f,Get(score,"Hp"));Assert.AreEqual(1,Time.timeScale);
            var events=Events().Skip(before).ToArray();Assert.AreEqual(1,events.Length);Assert.AreEqual("nexusFinal",Get(events[0],"kind"));
            var finale=(Component)Get(stage,"Finale");Assert.IsNotNull(finale);Assert.IsTrue((bool)Get(finale,"IsFinal"));Assert.AreEqual(36,Get(finale,"PieceCount"));
            Assert.Greater(Vector2.Dot((Vector3)Get(finale,"MeanVelocity"),Vector2.right),0,"D-026 reverses the prior final-only emission. Ordinary C remains rebound.");
            var type=TypeFor("NexusImpactVfx");var clock=type.GetMethod("FinaleMotionAge",BindingFlags.Static|BindingFlags.Public);
            Assert.That((float)clock.Invoke(null,new object[]{.016f}),Is.Zero.Within(.00001f));
            Assert.That((float)clock.Invoke(null,new object[]{.066f}),Is.EqualTo(.11f).Within(.00001f),"First 50ms of breakup are fast BEFORE the slow tail.");
            float slow=(float)clock.Invoke(null,new object[]{.166f})-(float)clock.Invoke(null,new object[]{.066f});
            Assert.That(slow,Is.EqualTo(.025f).Within(.00001f));
            Assert.Greater((float)clock.Invoke(null,new object[]{.35f})-(float)clock.Invoke(null,new object[]{.25f}),.1f);
            yield return WaitRealtime(.30f);Assert.IsTrue(finale!=null);Assert.AreEqual(1,Time.timeScale);
            Call(stage,"Retry");yield return WaitForScene("Cyberpunk");yield return null;yield return null;DisablePatterns();
            stage=Find("StageManager");Assert.IsFalse((bool)Get(stage,"IsDeath"));Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));Assert.AreEqual(1,Time.timeScale);
            Assert.AreEqual(Vector2.zero,Get(Find("PlayerCharacterController"),"MoveVector"));Assert.IsNull(Get(stage,"Finale"));
            Assert.IsFalse(Resources.FindObjectsOfTypeAll(type).Cast<Component>().Any(c=>c.gameObject.scene.IsValid()));
            Set(stage,"PracticeInvulnerable",false);Set(Get(stage,"scoreManager"),"Hp",.5f);
            Call(stage,"DamageNexusFromLaser",1f,center+Vector2.up*radius,Vector2.down,Color.yellow);
            yield return WaitRealtime(.72f);
            Assert.IsTrue(((Component)Get(stage,"deathUIController")).gameObject.activeInHierarchy,"Result follows the complete .64s finale.");
            Assert.IsNull(Get(stage,"Finale"));Assert.AreEqual(1,Time.timeScale);
            Call(stage,"ReturnHome");yield return WaitForScene("01.StageSelectScene");Assert.AreSame(stage,Find("StageManager"));Assert.AreEqual("Main",Get(stage,"State").ToString());Assert.AreEqual(1,Time.timeScale);
        }
    }
}
