using System;
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
        public IEnumerator Graphics_BroadShaderBendsRealPixelsAndEdgesRecoverWithoutMovingUI()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var ui=Find("CursorDashRuntimeUI");Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"visualPreset\",\"name\":\"strong\"}");Call(ui,"Close");
            const int w=256,h=448;var source=new Texture2D(w,h,TextureFormat.RGBA32,false);var input=new Color[w*h];
            for(int y=0;y<h;y++)for(int x=0;x<w;x++)input[y*w+x]=new Color(x/(float)(w-1),y/(float)(h-1),.25f,1);
            source.SetPixels(input);source.Apply();var target=new RenderTexture(w,h,0,RenderTextureFormat.ARGB32);var pixels=new Texture2D(w,h,TextureFormat.RGBA32,false);
            var mat=new Material(Resources.Load<Shader>("CursorDash/LocalImpact"));var previous=RenderTexture.active;
            try{
                mat.SetVector("_Aspect",new Vector4(1,h/(float)w));mat.SetFloat("_SuccessActive",1);mat.SetFloat("_OffsetCap",.06f);
                var hits=new Vector4[8];var shapes=new Vector4[8];hits[0]=new Vector4(.5f,.5f,1.15f,.06f);shapes[0]=new Vector4(1,.18f,1.4f,0);
                mat.SetVectorArray("_Hits",hits);mat.SetVectorArray("_Shapes",shapes);UnityEngine.Graphics.Blit(source,target,mat);
                RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();var changed=pixels.GetPixels();int broad=0;
                for(int y=2;y<h-2;y++)for(int x=2;x<w-2;x++){
                    float distance=new Vector2((x/(float)w-.5f),(y/(float)h-.5f)*h/w).magnitude;
                    if(distance>.25f&&distance<.60f&&Mathf.Abs(changed[y*w+x].r-input[y*w+x].r)>.005f)broad++;
                }
                Assert.Greater(broad,1000,"Broad shader must actually sample displaced world pixels far from the contact, not draw a ring.");
                for(int x=0;x<w;x+=16){Assert.That(changed[x].r,Is.EqualTo(input[x].r).Within(.009f));Assert.That(changed[(h-1)*w+x].r,Is.EqualTo(input[(h-1)*w+x].r).Within(.009f));}
                string dir=Path.Combine(GraphicsEvidenceRoot,"Shader");Directory.CreateDirectory(dir);File.WriteAllBytes(Path.Combine(dir,"broad-ramp.png"),pixels.EncodeToPNG());
                // D-021 circle lens is a separate branch; keep the existing hex-wave assertions above.
                hits[0]=new Vector4(.28f,.72f,2.2f,.045f);shapes[0]=new Vector4(2,0,2,2);
                mat.SetVectorArray("_Hits",hits);mat.SetVectorArray("_Shapes",shapes);UnityEngine.Graphics.Blit(source,target,mat);
                pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();var circle=pixels.GetPixels();
                int circleMoved=0;
                for(int i=0;i<circle.Length;i++){
                    Assert.IsFalse(float.IsNaN(circle[i].r)||float.IsNaN(circle[i].g),"No singularity at the lens center.");
                    if(Mathf.Abs(circle[i].r-input[i].r)>.005f||Mathf.Abs(circle[i].g-input[i].g)>.005f)circleMoved++;
                }
                Assert.Greater(circleMoved,1000,"The circle lens must displace a broad part of the sampled image.");
                File.WriteAllBytes(Path.Combine(dir,"circle-lens-ramp.png"),pixels.EncodeToPNG());
                shapes[0].y=1;mat.SetVectorArray("_Shapes",shapes);UnityEngine.Graphics.Blit(source,target,mat);
                pixels.ReadPixels(new Rect(0,0,w,h),0,0);pixels.Apply();var settled=pixels.GetPixels();
                for(int i=0;i<settled.Length;i++){
                    Assert.That(settled[i].r,Is.EqualTo(input[i].r).Within(.009f));
                    Assert.That(settled[i].g,Is.EqualTo(input[i].g).Within(.009f));
                }
            }finally{RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(source);UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(mat);}
            var stage=Find("StageManager");var feedback=Get(stage,"Feedback");var distortion=Get(feedback,"Distortion");var uiCamera=(Camera)Get(distortion,"UiCamera");Vector3 stableUI=uiCamera.transform.position;
            int before=(int)Get(distortion,"RenderedEffectFrames");
            for(int i=0;i<12;i++)Call(stage,"RegisterCircleDestroyed",(Vector2)Camera.main.ViewportToWorldPoint(new Vector3(i%2==0?.04f:.96f,i%3==0?.05f:.95f,10)));
            Assert.LessOrEqual((int)Get(distortion,"ActiveCount"),4);yield return WaitRealtime(.045f);
            ScreenCapture.CaptureScreenshot(Path.Combine(GraphicsEvidenceRoot,"Shader/edge-hits.png"));yield return null;
            Assert.Greater((int)Get(distortion,"RenderedEffectFrames"),before);Assert.AreEqual(stableUI,uiCamera.transform.position);
            yield return WaitRealtime(.7f);Assert.AreEqual(0,Get(distortion,"ActiveCount"));Assert.AreEqual(stableUI,uiCamera.transform.position);Assert.That(Camera.main.transform.position.x,Is.EqualTo(stableUI.x).Within(.0001f));Assert.AreEqual(1,Time.timeScale);
        }

        [UnityTest]
        public IEnumerator Graphics_SixConvexVerticesPistonAndFourOutletsKeepBeamRules()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var p=Find("PlayerCharacterController");p.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);p.transform.position=new Vector2(-2,-3);
            var h=NodesOfType("ArrowNode").First();h.gameObject.SetActive(true);
            foreach(float r in new[]{.1f,.8f,3f}){
                var graphic=Get(h,"flat");Call(graphic,"Draw",true,r,.18f,1f,0f,true,1f,0f,Color.yellow,true);
                Assert.Greater((float)Get(graphic,"QuadExtent"),r*1.35f,"Artwork bounds must contain the ring at every allowed legacy radius.");
                Assert.That((float)Get(graphic,"CoreRadius"),Is.Zero.Within(.0001f),"D-019 removes the independent marker; the outline still remains at zero energy.");
            }
            ConfigureHexPreparation(h,.05f);Set(h,"warningDuration",.12f);Set(h,"attackDuration",.30f);
            var geometry=TypeFor("NodeFlatGraphic").GetMethod("Hexagon");
            for(int k=0;k<=100;k++){
                var points=(Vector2[])geometry.Invoke(null,new object[]{.8f,Mathf.Lerp(1,.18f,k/100f)});
                Assert.AreEqual(6,points.Length);
                for(int i=0;i<6;i++){
                    Vector2 a=points[(i+1)%6]-points[i],b=points[(i+2)%6]-points[(i+1)%6];
                    Assert.Greater(a.magnitude,.001f);Assert.Greater(a.x*b.y-a.y*b.x,0,"Strictly convex throughout morph");Assert.That(points[i].magnitude,Is.EqualTo(.8f).Within(.00001f));
                }
            }
            var stage=Find("StageManager");Vector2 nexus=(Vector2)Get(stage,"NexusPosition");
            foreach(var direction in new[]{Vector2.up,Vector2.left,Vector2.down,Vector2.right}){
                h.transform.position=nexus+direction*2;Call(h,"Execute");yield return WaitCirclePhase(h,"Fired");
                var points=(Vector3[])Get(h,"TriangleWorldPoints");Vector2 outlet=(points[0]+points[1])*.5f;
                Assert.That(Vector2.Dot((outlet-(Vector2)h.transform.position).normalized,-direction),Is.GreaterThan(.9999f));
                Assert.That(Vector2.Distance((Vector2)Get(h,"origin"),h.transform.position),Is.LessThan(.0001f),"Logical origin unchanged");
                Assert.That(Vector2.Distance((Vector2)Get(h,"BeamOutlet"),outlet),Is.LessThan(.0001f));
                Assert.IsTrue((bool)Get(Get(h,"flat"),"Visible"));
                float began=Time.time;yield return WaitGameplay(.12f);
                Assert.That((float)Get(h,"EnergyRemaining"),Is.InRange(.35f,.75f));
                Assert.That((float)Get(Get(h,"flat"),"CoreRadius"),Is.Zero.Within(.0001f));
                yield return WaitCirclePhase(h,"Inactive");Assert.AreEqual(1,Get(h,"ImpactsThisExecution"));
                yield return WaitRealtime(.4f);
            }
        }

        [UnityTest]
        public IEnumerator Graphics_DirectionalSingleContactAndCleanupKeepUserPause()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var ui=Find("CursorDashRuntimeUI");Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"visualPreset\",\"name\":\"strong\"}");Call(ui,"Close");
            var stage=Find("StageManager");var n=NodesOfType("NormalNode").First();n.gameObject.SetActive(true);
            var p=Find("PlayerCharacterController");p.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);p.transform.position=new Vector2(-2,-3);
            int expected=0;int graphicCount=Resources.FindObjectsOfTypeAll(TypeFor("NodeFlatGraphic")).Length;
            foreach(var direction in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down}){
                n.transform.position=new Vector2(0,2);Call(n,"Execute");
                Call(n,"ContactAt",(Vector2)n.transform.position-direction*.27f,direction);expected++;
                Call(n,"ShowEffect",(Vector2)n.transform.position-direction);
                Assert.AreEqual(expected,Get(stage,"DestroyedNodes"));
                var feedback=Get(stage,"Feedback");var evt=((IEnumerable)Get(feedback,"Recent")).Cast<object>().Last();
                Assert.AreEqual(direction,Get(evt,"incoming"));
                // FindObjectsByType deliberately excludes DontSave objects; owned effects use that flag.
                var burst=Resources.FindObjectsOfTypeAll(TypeFor("CircularBreakVfx")).Cast<Component>().Single(c=>c.gameObject.scene.IsValid()&&c.gameObject.activeInHierarchy);
                Assert.Greater(Vector2.Dot((Vector3)Get(burst,"MeanVelocity"),direction),0,"Fragments follow incoming direction, not a radial flower.");
                Assert.AreEqual(10,Get(burst,"PieceCount"));
                Assert.IsFalse(n.transform.Find("BreakShell").gameObject.activeSelf);Assert.IsFalse(((GameObject)Get(n,"effectObject")).activeSelf);
                Call(stage,"SetPaused",true);yield return WaitRealtime(.65f);Assert.AreEqual(0,Time.timeScale,"D-019 .50s world response expires without unpausing user pause");
                Assert.AreEqual(0,Get(Get(feedback,"Distortion"),"ActiveCount"));
                Assert.That(Camera.main.transform.localPosition.x,Is.EqualTo(0).Within(.0001f));
                Call(stage,"SetPaused",false);yield return WaitGameplay(.6f);Assert.AreEqual("Inactive",Get(n,"Phase").ToString());
            }
            Assert.AreEqual(graphicCount,Resources.FindObjectsOfTypeAll(TypeFor("NodeFlatGraphic")).Length);
            Assert.AreEqual(0,Resources.FindObjectsOfTypeAll(TypeFor("CircularBreakVfx")).Cast<Component>().Count(c=>c.gameObject.scene.IsValid()&&c.gameObject.activeInHierarchy));
        }

        [UnityTest]
        public IEnumerator Graphics_Legacy141FullImportAndNewPresetRoundtripAreAtomic()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"Open",true);var tuning=TypeFor("CursorDashTuning");
            string old=File.ReadAllText(Path.GetFullPath("../docs/CursorDash/examples/cp14-full.json"));object[] args={old,null};
            Assert.IsTrue((bool)tuning.GetMethod("Import").Invoke(null,args),args[1]?.ToString());
            string restored=(string)tuning.GetMethod("Export").Invoke(null,new object[]{"legacy migrated",false});
            Assert.That(Newtonsoft.Json.Linq.JObject.Parse(restored)["values"].Count(),Is.EqualTo(161));
            Assert.That((float)Newtonsoft.Json.Linq.JObject.Parse(restored)["values"]["hex.radius"],Is.EqualTo(.8f));
            var incomplete=Newtonsoft.Json.Linq.JObject.Parse(old);((Newtonsoft.Json.Linq.JObject)incomplete["values"]).Remove("hex.radius");args=new object[]{incomplete.ToString(),null};
            Assert.IsFalse((bool)tuning.GetMethod("Import").Invoke(null,args),"Arbitrary missing old keys are not migration.");
            Assert.IsTrue(Newtonsoft.Json.Linq.JToken.DeepEquals(Newtonsoft.Json.Linq.JObject.Parse(restored)["values"],Newtonsoft.Json.Linq.JObject.Parse((string)tuning.GetMethod("Export").Invoke(null,new object[]{"legacy migrated",false}))["values"]),"Rejected input is atomic");
            Call(ui,"BrowserCommand","{\"cmd\":\"visualPreset\",\"name\":\"strong\"}");
            string legacyStrong=(string)tuning.GetMethod("Export").Invoke(null,new object[]{"legacy visual only",false});
            var oldValues=Newtonsoft.Json.Linq.JObject.Parse(old)["values"];
            var preserved=Newtonsoft.Json.Linq.JObject.Parse(legacyStrong)["values"];
            foreach(var field in ((Newtonsoft.Json.Linq.JObject)oldValues).Properties().Where(f=>!f.Name.StartsWith("fx.circleRemove.")&&!f.Name.StartsWith("fx.triangleBreak.")))
                Assert.IsTrue((bool)tuning.GetMethod("Equivalent").Invoke(null,new object[]{field.Name,field.Value,preserved[field.Name]}),"Visual preset must preserve imported combat/user value: "+field.Name);
            // The original CP14 JSON intentionally contains a .45s experiment. A visual preset must preserve it.
            // Start a fresh default training session explicitly before exporting the new .20s comparison examples.
            Call(ui,"BrowserCommand","{\"cmd\":\"resetAll\"}");
            Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            Call(Find("StageManager"),"DamageNexus",1f);yield return WaitRealtime(.4f);
            Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"spawnCircle\"}");
            Call(ui,"BrowserCommand","{\"cmd\":\"visualPreset\",\"name\":\"strong\"}");
            string strong=(string)tuning.GetMethod("Export").Invoke(null,new object[]{"강렬한 노드 시안",false});
            var vals=Newtonsoft.Json.Linq.JObject.Parse(strong)["values"];
            Assert.AreEqual(.15f,(float)vals["circle.morph"]);Assert.AreEqual(.20f,(float)vals["circle.dash"]);Assert.AreEqual(.60f,(float)vals["circle.scale"]);
            Assert.AreEqual(.085f,(float)vals["fx.triangleBreak.stop"]);Assert.AreEqual(.08f,(float)vals["fx.nexusDamage.stop"]);Assert.AreEqual(0,(float)vals["fx.hexBlock.stop"]);
            args=new object[]{strong,null};Assert.IsTrue((bool)tuning.GetMethod("Import").Invoke(null,args));
            string outDir=Path.Combine(GraphicsEvidenceRoot,"Presets");Directory.CreateDirectory(outDir);
            File.WriteAllText(Path.Combine(outDir,"strong-full.json"),strong);
            File.WriteAllText(Path.Combine(outDir,"strong-changes.json"),(string)tuning.GetMethod("Export").Invoke(null,new object[]{"강렬한 노드 시안",true}));
            File.WriteAllText(Path.Combine(outDir,"strong-report-ko.txt"),(string)tuning.GetMethod("ChangedReport").Invoke(null,null));
            Call(ui,"BrowserCommand","{\"cmd\":\"visualPreset\",\"name\":\"baseline\"}");
            File.WriteAllText(Path.Combine(outDir,"baseline-full.json"),(string)tuning.GetMethod("Export").Invoke(null,new object[]{"CP14 타격감 비교 기준",false}));yield return null;
        }
    }
}
