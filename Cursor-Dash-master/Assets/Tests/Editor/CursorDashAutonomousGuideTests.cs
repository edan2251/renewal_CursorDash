#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections;
using System.Linq;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CursorDashTests
{
    // Same fixture/UnitySetUp+TearDown as the existing scene tests; this test changes no persisted completion key.
    public partial class CursorDashSceneRegression
    {
        [System.Serializable] private class GhostPathEvidence {public int circleKills,hexKills,reflections,motionFrames,stoppedFrames;public float lastCircleCharge,input;public string[] lowMotion;}
        [UnityTest] public IEnumerator CP3_SingleAlternatingPracticeWaitsAfterMissAndShowsProtectedNexusFeedback()
        {
            TypeFor("CursorDashSession").GetField("ReplayTutorial",BindingFlags.Public|BindingFlags.Static).SetValue(null,true);
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"tutorial\"}");float deadline=Time.realtimeSinceStartup+5;Component practice;
            while((practice=Find("CursorDashPractice"))==null){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            var stage=Find("StageManager");var fever=Get(stage,"Fever");float hp=(float)Get(Get(stage,"scoreManager"),"Hp");
            ((Behaviour)Find("VirtualJoyStick")).enabled=false;Call(Find("PlayerCharacterController"),"MoveStop");Call(Find("PlayerCharacterAfterImage"),"ClearTrail");
            // This case intentionally lets attacks reach the nexus. Keep the stationary fixture body
            // out of random dash paths; other cases verify actual player contacts and ownership.
            Find("PlayerCharacterController").GetComponent<Collider2D>().enabled=false;
            for(int kind=0;kind<3;kind++){
                deadline=Time.realtimeSinceStartup+6;Component node;
                while((node=(Component)Get(practice,"LastSpawned"))==null){Assert.Less(Time.realtimeSinceStartup,deadline);Assert.IsFalse(((Renderer)Get(fever,"BackgroundScoreRenderer")).enabled);yield return null;}
                Assert.IsTrue(TypeFor(kind%2==0?"NormalNode":"ArrowNode").IsInstanceOfType(node));
                var player=Find("PlayerCharacterController");var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(-1,-2);player.transform.position=body.position;Physics2D.SyncTransforms();
                deadline=Time.realtimeSinceStartup+6;
                while(Get(practice,"LastSpawned")!=null){Assert.Less(Time.realtimeSinceStartup,deadline);Assert.LessOrEqual(((System.Collections.IDictionary)Get(practice,"PracticeTargets")).Count,1);yield return null;}
                Assert.AreEqual(hp,Get(Get(stage,"scoreManager"),"Hp"));
                var hits=((System.Collections.IEnumerable)Get(Get(stage,"Feedback"),"Recent")).Cast<object>().Where(x=>(string)x.GetType().GetField("kind").GetValue(x)=="nexusDamage").ToArray();
                Assert.GreaterOrEqual(hits.Length,kind+1);Assert.IsTrue((bool)hits.Last().GetType().GetField("protectedContact").GetValue(hits.Last()));Assert.IsFalse((bool)hits.Last().GetType().GetField("confirmedDamage").GetValue(hits.Last()));
                Assert.IsTrue((bool)Get(Get(fever,"World"),"DamageActive"));
                if(kind==0){string evidence=Path.GetFullPath("Logs/tutorial-poweron-fever-order-20261028/NativeFrames");Directory.CreateDirectory(evidence);ScreenCapture.CaptureScreenshot(Path.Combine(evidence,"tutorial-protected-nexus-hit.png"));}
                yield return WaitGameplay(.8f);Assert.IsNull(Get(practice,"LastSpawned"),"A cleared/missed node must leave a readable gap before the next target.");
            }
            Assert.AreEqual(0,Get(Get(practice,"Flow"),"CircleWins"));Assert.AreEqual(0,Get(Get(practice,"Flow"),"HexWins"));
        }
        [UnityTest] public IEnumerator CP3_FeverNoticeAfterBuiltWorldPreviewsNodesAndCleansHomeRetry()
        {
            var stage=Find("StageManager");var fever=Get(stage,"Fever");Assert.AreEqual("Main",Get(stage,"State").ToString());
            Call(fever,"ReplayNotice");bool sawWhiteStart=false;float deadline=Time.realtimeSinceStartup+5;
            while(!(bool)Get(fever,"NoticeActive")){Assert.Less(Time.realtimeSinceStartup,deadline);
                var entryModel=Get(fever,"Model");
                if(Get(entryModel,"Current").ToString()=="Entering"&&(double)Get(entryModel,"Progress")>.66d){
                    Assert.IsTrue((bool)Get(fever,"NoticeBlocking"),"Instructions replace the core from the FIRST white expansion frame, before Fever state.");
                    if(!sawWhiteStart){string first=Path.GetFullPath("Logs/tutorial-poweron-fever-order-20261028/NativeFrames");Directory.CreateDirectory(first);ScreenCapture.CaptureScreenshot(Path.Combine(first,"notice-first-white-expansion.png"));}sawWhiteStart=true;
                }
                if((bool)Get(fever,"NoticeBlocking")){Assert.IsFalse((bool)Get(stage,"IsPlaying"));Assert.AreEqual(0f,Get(Get(stage,"NexusPresentation"),"EntryBodyAlpha"));Assert.AreEqual(1f,((Color)Get(Get(Get(fever,"GuideView"),"notice"),"color")).a);}
                yield return null;}
            Assert.IsTrue(sawWhiteStart,"Observed the actual entry phase, not only the built Fever state.");
            Assert.AreEqual(0,Time.timeScale);Assert.AreEqual("Fever",Get(Get(fever,"Model"),"Current").ToString());Assert.IsTrue((bool)Get(Get(fever,"Circuits"),"FeverConstructionComplete"));Assert.AreEqual(0f,Get(Get(stage,"NexusPresentation"),"EntryBodyAlpha"));Assert.IsFalse((bool)Get(stage,"RecordEligible"));
            yield return WaitRealtime(.4f);Assert.AreEqual(0,Time.timeScale);Assert.AreEqual(1d,Get(Get(fever,"Model"),"Gauge"));Assert.AreEqual(0,Get(fever,"ActiveBonuses"));
            string frames=Path.GetFullPath("Logs/tutorial-poweron-fever-order-20261028/NativeFrames");Directory.CreateDirectory(frames);ScreenCapture.CaptureScreenshot(Path.Combine(frames,"notice-full-stop.png"));yield return null;
            Call(stage,"SetPaused",true);double age=(double)Get(Get(fever,"notice"),"Elapsed");yield return WaitRealtime(.15f);Assert.AreEqual(age,Get(Get(fever,"notice"),"Elapsed"));Call(stage,"SetPaused",false);
            Call(stage,"Retry");yield return WaitRealtime(.4f);Assert.IsFalse((bool)Get(fever,"NoticeActive"));Assert.AreEqual(1,Time.timeScale);
            Call(fever,"ReplayNotice");deadline=Time.realtimeSinceStartup+5;
            while(!(bool)Get(fever,"NoticeActive")){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            while((double)Get(Get(fever,"notice"),"Elapsed")<2.7d){Assert.Less(Time.realtimeSinceStartup,deadline+3);yield return null;}
            Assert.AreEqual(0,Time.timeScale);Assert.Greater((int)Get(fever,"ActiveBonuses"),0);Assert.AreEqual(0,Get(fever,"BonusCollections"));Assert.AreEqual(0d,Get(Get(fever,"Model"),"Elapsed"));ScreenCapture.CaptureScreenshot(Path.Combine(frames,"notice-release.png"));yield return null;Call(stage,"ReturnHome");yield return null;Assert.IsFalse((bool)Get(fever,"NoticeActive"));Assert.AreEqual(1,Time.timeScale);
            Call(fever,"ReplayNotice");deadline=Time.realtimeSinceStartup+10;
            while(!(bool)Get(fever,"NoticeActive")){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            float scale=0;
            while((bool)Get(fever,"NoticeActive")){Assert.Less(Time.realtimeSinceStartup,deadline);Assert.GreaterOrEqual(Time.timeScale+.00001f,scale);scale=Time.timeScale;yield return null;}
            Assert.AreEqual("Fever",Get(Get(fever,"Model"),"Current").ToString());Assert.AreEqual(1,Time.timeScale);Assert.That((double)Get(Get(fever,"Model"),"Elapsed"),Is.LessThan(.05));Assert.AreEqual(1d,Get(Get(fever,"Model"),"Gauge"));Assert.Greater((int)Get(fever,"ActiveBonuses"),0,"Fade-preview nodes survive the control handoff.");
        }
        [UnityTest] public IEnumerator CP3_TutorialOwnershipRejectsOtherPlayerContactAndTrailAndRetryStartsFresh()
        {
            TypeFor("CursorDashSession").GetField("ReplayTutorial",BindingFlags.Public|BindingFlags.Static).SetValue(null,true);
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"tutorial\"}");
            float deadline=Time.realtimeSinceStartup+5;Component practice;
            while((practice=Find("CursorDashPractice"))==null){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            var stage=Find("StageManager");var player=Find("PlayerCharacterController");var realTrail=Find("PlayerCharacterAfterImage");var ghost=Get(practice,"Ghost");var copy=(Component)Get(ghost,"actor");var ghostTrail=Get(ghost,"Trail");
            ((Behaviour)Find("VirtualJoyStick")).enabled=false;Call(player,"MoveStop");Call(copy,"MoveStop");Set(practice,"nextSpawn",float.MaxValue);Set(ghost,"nextSpawn",float.MaxValue);Set(ghost,"chooseAfter",float.MaxValue);
            var real=(Component)Call(practice,"Spawn",0);Assert.IsNotNull(real);
            object[] placement={0,ghost,Vector2.zero};Assert.IsTrue((bool)practice.GetType().GetMethod("TryPosition").Invoke(practice,placement));
            var demo=(Component)Call(Get(stage,"NodeCreator"),"SpawnDemonstrationAt",0,(Vector2)placement[2],ghost);Assert.IsNotNull(demo);
            yield return WaitGameplay(.26f);
            var realBody=player.GetComponent<Rigidbody2D>();var ghostBody=copy.GetComponent<Rigidbody2D>();
            realBody.position=demo.transform.position;player.transform.position=realBody.position;ghostBody.position=real.transform.position;copy.transform.position=ghostBody.position;Physics2D.SyncTransforms();
            yield return new WaitForFixedUpdate();yield return WaitGameplay(.08f);
            Assert.AreEqual("Preparing",Get(real,"Phase").ToString());Assert.AreEqual("Preparing",Get(demo,"Phase").ToString());Assert.AreEqual(0,Get(Get(practice,"Flow"),"CircleWins"));
            Call(real,"ResetObject");Call(demo,"ResetObject");Call(player,"MoveStop");Call(copy,"MoveStop");
            realBody.position=new Vector2(-1,-2);player.transform.position=realBody.position;ghostBody.position=new Vector2(1,-2);copy.transform.position=ghostBody.position;Physics2D.SyncTransforms();
            for(int pass=0;pass<2;pass++){
                Call(realTrail,"ClearTrail");Call(ghostTrail,"ClearTrail");Component hex;
                if(pass==0){Set(ghost,"nextKind",1);Set(ghost,"nextSpawn",0f);yield return null;Set(ghost,"nextSpawn",float.MaxValue);hex=AllNodes().FirstOrDefault(n=>n.gameObject.activeSelf&&TypeFor("ArrowNode").IsInstanceOfType(n)&&Get(n,"Demonstration")!=null);}
                else hex=(Component)Call(practice,"Spawn",1);
                Assert.IsNotNull(hex);ulong execution=(ulong)Get(hex,"ExecutionId");int misses=(int)Get(ghost,"Misses");var wrongTrail=pass==0?realTrail:ghostTrail;
                Vector2 direction=(Vector2)Get(stage,"NexusPosition")-(Vector2)hex.transform.position,middle=(Vector2)hex.transform.position+direction*.35f,side=new Vector2(-direction.y,direction.x).normalized*.5f;
                deadline=Time.realtimeSinceStartup+6;
                while(hex.gameObject.activeSelf&&(ulong)Get(hex,"ExecutionId")==execution){Set(practice,"nextSpawn",float.MaxValue);Assert.Less(Time.realtimeSinceStartup,deadline);Call(wrongTrail,"RecordMovement",middle-side,middle+side,Time.time);Assert.AreEqual(0,Get(hex,"BlocksThisExecution"));yield return null;}
                if(pass==0)Assert.AreEqual(misses+1,Get(ghost,"Misses"));
            }
            Assert.AreEqual(0,Get(Get(practice,"Flow"),"HexWins"));Assert.AreEqual(0,Get(stage,"BlockedBeams"));Assert.AreEqual("0",Get(Get(Get(stage,"Fever"),"Model"),"PointsText"));
            Call(stage,"Retry");deadline=Time.realtimeSinceStartup+5;Component next;
            while((next=Find("CursorDashPractice"))==null||ReferenceEquals(next,practice)){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            Assert.AreNotSame(ghost,Get(next,"Ghost"));Assert.AreEqual(0,Get(Get(next,"Flow"),"CircleWins"));Assert.AreEqual(0,Get(Get(next,"Flow"),"HexWins"));Assert.IsTrue(copy==null||!copy.gameObject.activeInHierarchy);
        }
        [UnityTest] public IEnumerator CP3_AutonomousGhostUsesPhysicsContactAndActualReflectedReturnsWithoutPlayerRewards()
        {
            TypeFor("CursorDashSession").GetField("ReplayTutorial",BindingFlags.Public|BindingFlags.Static).SetValue(null,true);
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"tutorial\"}");
            float deadline=Time.realtimeSinceStartup+5;Component practice;
            while((practice=Find("CursorDashPractice"))==null){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            var player=Find("PlayerCharacterController");var realTrail=Find("PlayerCharacterAfterImage");var stage=Find("StageManager");
            ((Behaviour)Find("VirtualJoyStick")).enabled=false;Call(player,"MoveStop");Call(realTrail,"ClearTrail");
            var ghost=Get(practice,"Ghost");var copy=(Component)Get(ghost,"actor");
            string evidence=Path.GetFullPath("Logs/tutorial-poweron-fever-order-20261028/NativeFrames");Directory.CreateDirectory(evidence);int reflected=0,broken=0;
            Assert.AreSame(player,Find("PlayerCharacterController"));Assert.AreSame(realTrail,Find("PlayerCharacterAfterImage"));
            Assert.AreSame(ghost,Get(copy,"Demonstration"));Assert.AreNotSame(realTrail,Get(ghost,"Trail"));
            Assert.IsTrue(Physics2D.GetIgnoreCollision(player.GetComponent<Collider2D>(),copy.GetComponent<Collider2D>()));
            var lowMotion=new System.Collections.Generic.List<string>();int motionFrames=0,stoppedFrames=0;deadline=Time.realtimeSinceStartup+40;
            while((int)Get(ghost,"CircleKills")<2||(int)Get(ghost,"HexKills")<2){
                Assert.Less(Time.realtimeSinceStartup,deadline,"The real bot must continue after misses/pool reuse without user progress.");
                motionFrames++;if(((Vector2)Get(copy,"MoveVector")).sqrMagnitude<.001f){stoppedFrames++;
                    var t=Get(ghost,"target") as Component;if(lowMotion.Count<220)lowMotion.Add($"{Time.time:F2} {(t==null?"idle":t.GetType().Name+":"+Get(t,"Phase"))} route={(float)Get(ghost,"clock")-(float)Get(ghost,"routeAt"):F2}/{Get(ghost,"routeSeconds")} endDistance={Vector2.Distance(copy.transform.position,(Vector2)Get(ghost,"routeEnd")):F3}");
                }
                int r=(int)Get(ghost,"Reflections"),k=(int)Get(ghost,"CircleKills")+(int)Get(ghost,"HexKills");
                if(r>reflected){reflected=r;ScreenCapture.CaptureScreenshot(Path.Combine(evidence,"ghost-reflected-"+r+".png"));}
                if(k>broken){if((int)Get(ghost,"CircleKills")>0)Assert.That((float)Get(ghost,"LastCircleContactCharge"),Is.InRange(.88f,1f),"Circle contact is an actual late-charge collision.");broken=k;ScreenCapture.CaptureScreenshot(Path.Combine(evidence,"ghost-kill-"+k+".png"));}
                yield return null;
            }
            Assert.GreaterOrEqual((int)Get(ghost,"Reflections"),2);Assert.AreEqual(0,Get(Get(practice,"Flow"),"CircleWins"));Assert.AreEqual(0,Get(Get(practice,"Flow"),"HexWins"));
            Assert.AreEqual("0",Get(Get(Get(stage,"Fever"),"Model"),"PointsText"));Assert.AreEqual(0,Get(stage,"BlockedBeams"));Assert.AreEqual(0,Get(stage,"HexDestroyed"));
            File.WriteAllText(Path.Combine(evidence,"ghost-actual-path.json"),JsonUtility.ToJson(new GhostPathEvidence{
                circleKills=(int)Get(ghost,"CircleKills"),hexKills=(int)Get(ghost,"HexKills"),reflections=(int)Get(ghost,"Reflections"),
                lastCircleCharge=(float)Get(ghost,"LastCircleContactCharge"),input=(float)Get(Get(Get(stage,"Fever"),"Visuals"),"tutorialGhostInput"),motionFrames=motionFrames,stoppedFrames=stoppedFrames,lowMotion=lowMotion.ToArray()},true));
            Assert.Less(stoppedFrames,Mathf.Max(3,motionFrames*.04f),"Idle and reflected-return phases should keep curved analog movement rather than repeatedly stopping.");
            var trace=((System.Collections.IEnumerable)Get(practice,"BeamTrace")).Cast<string>().ToArray();Assert.IsTrue(trace.Any(x=>x.Contains("ghost reflected")));Assert.IsTrue(trace.Any(x=>x.Contains("ghost returned kill")));
            Call(stage,"SetPaused",true);var before=copy.transform.position;yield return new WaitForSecondsRealtime(.15f);Assert.AreEqual(before,copy.transform.position);Call(stage,"SetPaused",false);
            Call(stage,"ReturnHome");yield return null;Assert.IsNull(Find("CursorDashPractice"));Assert.AreSame(player,Find("PlayerCharacterController"));Assert.IsFalse(AllNodes().Any(n=>n.gameObject.activeSelf));
            foreach(var node in AllNodes()){
                var style=node.GetComponent(TypeFor("CursorDashGuideStyle")) as Behaviour;if(style!=null)Assert.IsFalse(style.enabled);
                foreach(var renderer in node.GetComponentsInChildren<Renderer>(true))if(renderer.sharedMaterial!=null)Assert.AreNotEqual("CursorDash/GuideInk",renderer.sharedMaterial.shader.name,"A pooled actor must not leak guide materials into normal play.");
            }
        }
    }
}
#endif
