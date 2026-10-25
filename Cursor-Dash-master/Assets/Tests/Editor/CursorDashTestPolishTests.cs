using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CursorDashTests
{
    public partial class CursorDashSceneRegression
    {
        [UnityTest]
        public IEnumerator Hex_PreFireHoldsEnergyAndCancelsWithoutDelayedBeam()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Set(stage,"PracticeInvulnerable",false);var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var hex=NodesOfType("ArrowNode").First();hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2);
            Assert.That((float)Get(hex,"preFireDuration"),Is.EqualTo(.025f));
            ConfigureHexPreparation(hex,3.08f);Set(hex,"warningDuration",.05f);Set(hex,"preFireDuration",.16f);Call(hex,"Execute");
            float deadline=Time.realtimeSinceStartup+5;while(!(bool)Get(hex,"IsPreFiring")){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            var beam=hex.GetComponent<LineRenderer>();Assert.IsFalse(beam.enabled);Assert.AreEqual(1f,Get(hex,"EnergyRemaining"));
            var node=hex.transform.Find("Node");Vector3 origin=node.localPosition;yield return WaitGameplay(.04f);
            Assert.That(Vector3.Distance(node.localPosition,origin),Is.LessThan(.0001f),"Hold the aiming pose; main recoil starts with the actual beam.");Assert.IsFalse(beam.enabled);Assert.AreEqual(1f,Get(hex,"EnergyRemaining"));
            Call(stage,"SetPaused",true);Vector3 paused=node.localPosition;yield return WaitRealtime(.08f);Assert.AreEqual(paused,node.localPosition);Assert.IsFalse(beam.enabled);
            Call(hex,"ResetObject");Call(stage,"SetPaused",false);yield return WaitGameplay(.45f);
            Assert.AreEqual("Inactive",Get(hex,"Phase").ToString());Assert.IsFalse(beam.enabled);Assert.AreEqual(0,Get(hex,"ImpactsThisExecution"));Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
            hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2);float began=Time.time;Call(hex,"Execute");yield return WaitCirclePhase(hex,"Fired");
            Assert.That(Time.time-began,Is.EqualTo(3.08f).Within(Mathf.Max(.06f,2*Time.deltaTime)),"Fire on the END of actual turn 6, with the test aiming pose inside the last half-turn.");Assert.IsTrue(beam.enabled);Assert.Greater(node.localPosition.sqrMagnitude,.0000001f,"Recoil and beam begin together.");
            Assert.That((float)Get(hex,"attackDuration"),Is.EqualTo(.06f));Assert.That((float)Get(hex,"reflectionDuration"),Is.EqualTo(.06f));Call(hex,"ResetObject");
            Assert.That(node.localPosition.sqrMagnitude,Is.LessThan(.000001f));
        }

        [UnityTest]
        public IEnumerator Hex_FastSweepReflectsOnceAndArrivalAfterglowCannotHitAgain()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Set(stage,"PracticeInvulnerable",false);var player=Find("PlayerCharacterController");
            player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var trail=Find("PlayerCharacterAfterImage");Call(trail,"ClearTrail");
            var hex=NodesOfType("ArrowNode").First();hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2);
            ConfigureHexPreparation(hex,.05f);Set(hex,"warningDuration",.05f);Set(hex,"attackDuration",.01f);Call(hex,"Execute");
            yield return WaitCirclePhase(hex,"Fired");Vector2 start=(Vector2)Get(hex,"origin"),end=(Vector2)Get(hex,"target");
            Vector2 mid=Vector2.Lerp(start,end,.5f),side=Vector2.Perpendicular((end-start).normalized)*.4f;
            Call(trail,"RecordMovement",mid-side,mid+side,Time.time);
            float until=Time.realtimeSinceStartup+3;while((int)Get(stage,"HexDestroyed")==0){Assert.Less(Time.realtimeSinceStartup,until);yield return null;}
            Assert.AreEqual(1,Get(hex,"BlocksThisExecution"));Assert.AreEqual(1,Get(hex,"ImpactsThisExecution"));Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
            Call(hex,"ResetObject");Call(trail,"ClearTrail");hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2);Call(hex,"Execute");
            until=Time.realtimeSinceStartup+3;while(!(bool)Get(hex,"AfterglowOnly")){Assert.Less(Time.realtimeSinceStartup,until);yield return null;}
            Assert.AreEqual(2f,Get(Get(stage,"scoreManager"),"Hp"));Assert.AreEqual(1,Get(hex,"ImpactsThisExecution"));
            start=(Vector2)Get(hex,"origin");end=(Vector2)Get(hex,"target");mid=Vector2.Lerp(start,end,.5f);
            Call(trail,"RecordMovement",mid-side,mid+side,Time.time);
            yield return WaitCirclePhase(hex,"Inactive");
            Assert.AreEqual(0,Get(hex,"BlocksThisExecution"),"A new trail cannot reflect a visual-only tail.");
            Assert.AreEqual(1,Get(hex,"ImpactsThisExecution"));Assert.AreEqual(2f,Get(Get(stage,"scoreManager"),"Hp"));
            Assert.AreEqual(1,Get(stage,"HexDestroyed"));Assert.IsFalse(hex.GetComponent<LineRenderer>().enabled);
        }

        [UnityTest]
        public IEnumerator Circle_ForwardMajorPiecesDecelerateAndHexPathIsSeparate()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var circle=NodesOfType("NormalNode").First();circle.gameObject.SetActive(true);circle.transform.position=new Vector2(0,2);Call(circle,"Execute");Call(circle,"ContactAt",new Vector2(-.2f,2),Vector2.right);
            var fx=Resources.FindObjectsOfTypeAll(TypeFor("CircularBreakVfx")).Cast<Component>().Single(c=>c.gameObject.scene.IsValid()&&c.gameObject.activeInHierarchy);
            Assert.IsNull(fx.GetComponent(TypeFor("ImpactFragments")),"Circle artwork never changes the approved hex component.");
            var velocity=(Vector3[])Get(fx,"velocities");Assert.That(velocity.Length,Is.InRange(4,16));Assert.IsTrue(velocity.All(v=>v.x>0));
            Assert.Greater(velocity.Max(v=>v.magnitude)-velocity.Min(v=>v.magnitude),1);
            var owners=(int[])Get(fx,"owners");var basis=(Vector3[])Get(fx,"basis");
            Vector3 Center(Vector3[] values){Vector3 sum=Vector3.zero;int count=0;for(int i=0;i<values.Length;i++)if(owners[i]==0){sum+=values[i];count++;}return sum/count;}
            Call(fx,"Draw",.04f);Vector3 a=Center((Vector3[])Get(fx,"points"));Call(fx,"Draw",.10f);Vector3 b=Center((Vector3[])Get(fx,"points"));Call(fx,"Draw",.16f);Vector3 c=Center((Vector3[])Get(fx,"points"));
            Assert.Greater((b-a).x,(c-b).x,"Equal-duration travel becomes much shorter after the initial forward burst.");
            Assert.IsFalse(owners.Contains(-2),"No mirrored blast plates.");Assert.IsTrue(owners.Contains(-1),"Separate contact flash remains.");
            Assert.IsTrue(basis.All(v=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)));Call(Find("StageManager"),"SetPaused",true);yield return null;
            Assert.IsTrue(fx==null,"Pause cleanup owns circle effects too.");
        }

        [UnityTest]
        public IEnumerator Nexus_OnlyOuterRingStaysStillWhileInnerDecorationRotates()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var frame=GameObject.Find("UI/HpFrame");var holder=(Behaviour)frame.GetComponent(TypeFor("NexusRingRhythm"));Assert.IsNotNull(holder);
            var inner=GameObject.Find("UI/HpCentral");Assert.IsNull(inner.GetComponent(TypeFor("NexusRingRhythm")));Assert.IsTrue(((Behaviour)inner.GetComponent(TypeFor("Rotator"))).enabled);
            Assert.IsFalse(((Behaviour)frame.GetComponent(TypeFor("Rotator"))).enabled);
            Quaternion first=frame.transform.localRotation,innerBefore=inner.transform.localRotation;yield return WaitGameplay(.8f);
            Assert.That(Quaternion.Angle(first,frame.transform.localRotation),Is.LessThan(.001f));Assert.Greater(Quaternion.Angle(innerBefore,inner.transform.localRotation),1);
            Call(Find("StageManager"),"SetPaused",true);yield return WaitRealtime(.08f);Call(Find("StageManager"),"SetPaused",false);yield return WaitGameplay(.3f);
            Assert.That(Quaternion.Angle(first,frame.transform.localRotation),Is.LessThan(.001f));Assert.IsTrue(frame.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator Cursor_FinalGameViewShowsCursorAboveNexusAndBelowSettings()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Call(Get(stage,"Feedback"),"Clear");var player=Find("PlayerCharacterController");var renderer=(SpriteRenderer)Get(player,"cursorVisual");
            var savedSprite=renderer.sprite;Color savedColor=renderer.color;bool savedEnabled=renderer.enabled,savedPlayerEnabled=((Behaviour)player).enabled;Vector3 savedPosition=player.transform.position;
            var sprite=Sprite.Create(Texture2D.whiteTexture,new Rect(0,0,Texture2D.whiteTexture.width,Texture2D.whiteTexture.height),Vector2.one*.5f,2);
            Texture2D before=null,after=null,settings=null;var ui=Find("CursorDashRuntimeUI");
            var uiCamera=(Camera)Get(Get(Get(stage,"Feedback"),"Distortion"),"UiCamera");Vector3 stable=uiCamera.transform.position;
            try{
                Call(player,"MoveStop");((Behaviour)player).enabled=false;
                Vector2 center=(Vector2)Get(stage,"NexusPosition");player.GetComponent<Rigidbody2D>().position=center;player.transform.position=new Vector3(center.x,center.y,savedPosition.z);
                renderer.sprite=sprite;renderer.color=Color.magenta;renderer.enabled=false;
                yield return new WaitForEndOfFrame();before=ScreenCapture.CaptureScreenshotAsTexture();
                renderer.enabled=true;yield return new WaitForEndOfFrame();after=ScreenCapture.CaptureScreenshotAsTexture();
                Assert.AreEqual(before.width,after.width);Assert.AreEqual(before.height,after.height);
                var first=before.GetPixels32();var second=after.GetPixels32();int visible=0;
                for(int i=0;i<second.Length;i++)if(second[i].r>first[i].r+40&&second[i].g<first[i].g-40)visible++;
                Assert.Greater(visible,3,"Final Game View includes BOTH cameras and overlay HUD; camera-only pixels cannot prove final visibility.");Assert.AreEqual(stable,uiCamera.transform.position);
                var layer=Get(Get(Get(stage,"Feedback"),"Distortion"),"CursorLayer");Assert.IsTrue((bool)Get(layer,"Bound"));Assert.Greater((int)Get(layer,"RenderedFrames"),0);
                Assert.That((Camera.main.cullingMask&(1<<renderer.gameObject.layer)),Is.Zero,"Cursor is not also drawn under the nexus.");
                Call(ui,"Open",false);yield return new WaitForEndOfFrame();settings=ScreenCapture.CaptureScreenshotAsTexture();
                Vector3 uv=Camera.main.WorldToViewportPoint(center);int cx=Mathf.RoundToInt(uv.x*settings.width),cy=Mathf.RoundToInt(uv.y*settings.height);
                var withSettings=settings.GetPixels32();int covered=0,magenta=0;
                for(int y=Mathf.Max(0,cy-10);y<Mathf.Min(settings.height,cy+10);y++)for(int x=Mathf.Max(0,cx-10);x<Mathf.Min(settings.width,cx+10);x++){
                    int k=y*settings.width+x;if(second[k].r>220&&second[k].b>220&&second[k].g<40){magenta++;if(withSettings[k].g>40||withSettings[k].r<220||withSettings[k].b<220)covered++;}
                }
                Assert.Greater(magenta,3);Assert.Greater(covered,magenta*.8f,"Settings must visibly cover the cursor in the final framebuffer.");
                string dir=Path.Combine(GraphicsEvidenceRoot,"D025Cursor");Directory.CreateDirectory(dir);
                File.WriteAllBytes(Path.Combine(dir,"cursor-over-nexus-final-gameview.png"),after.EncodeToPNG());File.WriteAllBytes(Path.Combine(dir,"settings-over-cursor-final-gameview.png"),settings.EncodeToPNG());
            }finally{
                Call(ui,"Close");renderer.sprite=savedSprite;renderer.color=savedColor;renderer.enabled=savedEnabled;((Behaviour)player).enabled=savedPlayerEnabled;
                player.GetComponent<Rigidbody2D>().position=savedPosition;player.transform.position=savedPosition;
                if(before!=null)Object.DestroyImmediate(before);if(after!=null)Object.DestroyImmediate(after);if(settings!=null)Object.DestroyImmediate(settings);Object.DestroyImmediate(sprite);
            }
            yield return null;
        }
    }
}
