using System;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CursorDashTests
{
    public partial class CursorDashSceneRegression
    {
        [UnityTest]
        public IEnumerator CircleA_ArtworkSharesItsCenterAndSurvivesPoolReuse()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var node=NodesOfType("NormalNode").First();node.gameObject.SetActive(true);node.transform.position=new Vector2(0,2);Call(node,"Execute");yield return null;
            var art=(Component)Get(node,"flat");Assert.AreEqual("CircularNodeArtwork",art.GetType().Name);
            Assert.AreEqual(5,Get(art,"RingPieceCount"),"All five original arrow-tipped arcs must be present.");
            Assert.AreEqual(Vector3.zero,art.transform.localPosition);
            foreach(Transform child in art.transform){
                Assert.AreEqual(Vector3.zero,child.localPosition,"Charge, body and rotating arcs share one origin.");
                var mesh=child.GetComponent<MeshFilter>().sharedMesh;
                Assert.Greater(mesh.triangles.Length,0);Assert.IsTrue(mesh.vertices.All(v=>!float.IsNaN(v.x)&&!float.IsNaN(v.y)));
            }
            var rim=art.transform.Find("Body outline");var charge=art.transform.Find("Charge");
            Assert.That(rim.GetComponent<MeshFilter>().sharedMesh.bounds.center.sqrMagnitude,Is.LessThan(.000001f));
            var block=new MaterialPropertyBlock();charge.GetComponent<MeshRenderer>().GetPropertyBlock(block);
            Assert.That(block.GetColor("_Color").r,Is.EqualTo(203f/255).Within(.001f));
            Assert.That(block.GetColor("_Color").g,Is.EqualTo(73f/255).Within(.001f));
            Call(node,"ResetObject");Assert.IsTrue(art.GetComponentsInChildren<MeshRenderer>(true).All(r=>!r.enabled));
            node.gameObject.SetActive(true);node.transform.position=new Vector2(1,2);Call(node,"Execute");yield return null;
            Assert.AreSame(art,Get(node,"flat"),"Pool reuse owns the same artwork rather than appending another copy.");
            Assert.AreEqual(Vector3.zero,art.transform.localPosition);Assert.AreEqual(3,art.transform.childCount);
            Assert.IsTrue(rim.GetComponent<MeshRenderer>().enabled);Call(node,"ResetObject");
        }

        [UnityTest]
        public IEnumerator Feedback2_DefaultAndLegacyCircleSizesAreBounded()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var n=NodesOfType("NormalNode").First();n.gameObject.SetActive(true);
            foreach(float request in new[]{.05f,1f}){
                if(request==1){Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"fx.circleRemove.size\",\"value\":1}");Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"user.flash\",\"value\":0}");Call(ui,"Close");}
                n.gameObject.SetActive(true);n.transform.position=new Vector2(0,2);Call(n,"Execute");Call(n,"ContactAt",new Vector2(-.1f,2),Vector2.right);
                var burst=Resources.FindObjectsOfTypeAll(TypeFor("CircularBreakVfx")).Cast<Component>().Single(c=>c.gameObject.scene.IsValid()&&c.gameObject.activeInHierarchy);
                var basis=(Vector3[])Get(burst,"basis");var owners=(int[])Get(burst,"owners");float largest=0;
                foreach(int piece in owners.Where(i=>i>=0).Distinct()){
                    Vector3[] vertices=basis.Where((v,i)=>owners[i]==piece).ToArray();
                    foreach(var a in vertices)foreach(var b in vertices)largest=Mathf.Max(largest,Vector3.Distance(a,b));
                }
                // World-space 1 fallback used to produce pieces ~2 units wide, larger than the whole node.
                Assert.Less(largest,.28f,"Circular pieces remain small even for a preserved legacy size=1 preset.");
                Assert.Greater(largest,.025f,"Pieces remain visible rather than suppressing the effect.");
                Assert.AreEqual("CursorDash/ImpactFlat",burst.GetComponent<MeshRenderer>().sharedMaterial.shader.name);
                if(request==1){var vertexColors=burst.GetComponent<MeshFilter>().sharedMesh.colors;Assert.AreEqual(0,vertexColors[vertexColors.Length-1].a,"Flash0 also suppresses the first frame, not only Update.");}
                Assert.Greater(Vector2.Dot((Vector3)Get(burst,"MeanVelocity"),Vector2.right),0);
                if(request<1)Assert.That((float)(JToken)TypeFor("CursorDashTuning").GetMethod("Effective").Invoke(null,new object[]{"fx.circleRemove.size"}),Is.EqualTo(.05f));
                yield return WaitRealtime(.65f);
            }
        }

        [UnityTest]
        public IEnumerator Feedback2_YellowStagesNoCoreAnd157Compatibility()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var hex=NodesOfType("ArrowNode").First();hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2);
            ConfigureHexPreparation(hex,2.8f);Set(hex,"warningDuration",.12f);Set(hex,"attackDuration",.3f);Call(hex,"Execute");
            foreach(string phase in new[]{"Preparing","Warning","Fired"}){
                if(phase!="Preparing")yield return WaitCirclePhase(hex,phase);
                yield return null;
                var art=(Component)Get(hex,"flat");var mat=art.GetComponent<MeshRenderer>().sharedMaterial;Color c=mat.GetColor("_Color");
                Assert.That(c.r,Is.GreaterThan(.99));Assert.That(c.g,Is.InRange(.82f,.85f));Assert.That(c.b,Is.InRange(.39f,.41f));
                Assert.That(mat.GetFloat("_Core"),Is.Zero);
            }
            Color beam=hex.GetComponent<LineRenderer>().startColor;Assert.That(beam.r,Is.GreaterThan(.99));Assert.That(beam.g,Is.InRange(.82f,.85f));
            Call(hex,"ResetObject");
            Call(ui,"Open",true);string previous=File.ReadAllText(Path.GetFullPath("Logs/node-graphics-20261024-192424/Web/strong-full.json"));
            var tuning=TypeFor("CursorDashTuning");object[] args={previous,null};Assert.IsTrue((bool)tuning.GetMethod("Import").Invoke(null,args),args[1]?.ToString());
            var migrated=JObject.Parse((string)tuning.GetMethod("Export").Invoke(null,new object[]{"157 migrated",false}));
            Assert.AreEqual(161,((JObject)migrated["values"]).Count);
            foreach(var p in ((JObject)JObject.Parse(previous)["values"]).Properties())Assert.IsTrue((bool)tuning.GetMethod("Equivalent").Invoke(null,new object[]{p.Name,p.Value,migrated["values"][p.Name]}),p.Name);
            var broken=JObject.Parse(previous);((JObject)broken["values"]).Remove("hex.radius");args=new object[]{broken.ToString(),null};Assert.IsFalse((bool)tuning.GetMethod("Import").Invoke(null,args));
            var unchanged=JObject.Parse((string)tuning.GetMethod("Export").Invoke(null,new object[]{"157 migrated",false}));Assert.IsTrue(JToken.DeepEquals(migrated["values"],unchanged["values"]));
        }

        private static Color[] FeedbackCameraPixels(Camera camera,RenderTexture target,Texture2D texture)
        {
            var previousTarget=camera.targetTexture;var previousActive=RenderTexture.active;
            try{camera.targetTexture=target;RenderTexture.active=target;GL.Clear(true,true,Color.clear);camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();return texture.GetPixels();}
            finally{camera.targetTexture=previousTarget;RenderTexture.active=previousActive;}
        }

        [UnityTest]
        public IEnumerator Feedback2_OriginalBackgroundLensBrightensAndRestoresWithStableUI()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");Call(stage,"SetPaused",true);
            var feedback=Get(stage,"Feedback");var effect=Get(feedback,"Distortion");var camera=Camera.main;var overlay=(Camera)Get(effect,"UiCamera");
            Vector3 stable=overlay.transform.position;var target=new RenderTexture(256,448,0,RenderTextureFormat.ARGB32);var texture=new Texture2D(256,448,TextureFormat.RGBA32,false);
            string dir=Path.Combine(GraphicsEvidenceRoot,"Background");Directory.CreateDirectory(dir);
            bool hadFlash=PlayerPrefs.HasKey("OnFlash");string savedFlash=PlayerPrefs.GetString("OnFlash","");
            try{
                PlayerPrefs.SetString("OnFlash","true");
                Call(effect,"Clear");var before=FeedbackCameraPixels(camera,target,texture);File.WriteAllBytes(Path.Combine(dir,"before.png"),texture.EncodeToPNG());
                Assert.IsNull(camera.GetComponent(TypeFor("WorldCircuitBackdrop")),"D-021 removes the generated circuit mesh.");
                var original=(Component)Get(effect,"OriginalBackground");var image=(Component)Get(original,"SourceImage");
                Assert.IsNotNull(image);Assert.AreEqual("Background_Flash",image.name);
                Assert.IsFalse(((Behaviour)image).enabled,"Only the camera compositor displays this same image during play.");
                Assert.AreEqual("Assets/Art/Backgrounds/Cyberpunk/Original/Cyberpunk_Background_Flash.png",UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object)Get(image,"sprite")));
                float savedAlpha=((Color)Get(image,"color")).a;Assert.That(savedAlpha,Is.EqualTo(87f/255).Within(.001f));
                // Isolate the requested local image flash from pixel movement; previous global exposure is intentionally absent.
                Vector3 hit=camera.ViewportToWorldPoint(new Vector3(.28f,.72f,10));Call(effect,"AddCircleSuccess",hit,0f,2.4f,.5f,1.4f);
                yield return WaitRealtime(.08f);var during=FeedbackCameraPixels(camera,target,texture);File.WriteAllBytes(Path.Combine(dir,"during.png"),texture.EncodeToPNG());
                int brighter=0;float beforeSum=0,duringSum=0;
                for(int i=0;i<before.Length;i++){beforeSum+=before[i].grayscale;duringSum+=during[i].grayscale;if(during[i].grayscale>before[i].grayscale+.01f)brighter++;}
                Assert.Greater(brighter,2000,"The actual Cyberpunk background, not a synthetic ramp, must brighten across the board.");
                Assert.Greater(duringSum,beforeSum,"The original masked lines brighten without a blanket world exposure.");Assert.AreEqual(stable,overlay.transform.position);Assert.AreEqual(0,Time.timeScale);
                Assert.AreEqual(savedAlpha,((Color)Get(image,"color")).a,"Runtime flash must not overwrite the user's saved alpha.");
                yield return WaitRealtime(.60f);var after=FeedbackCameraPixels(camera,target,texture);File.WriteAllBytes(Path.Combine(dir,"after.png"),texture.EncodeToPNG());
                Assert.AreEqual(0,Get(effect,"ActiveCount"));Assert.AreEqual(0,Time.timeScale,"An expired shock must not resume user pause");
                float error=0;for(int i=0;i<before.Length;i++)error+=Mathf.Abs(before[i].grayscale-after[i].grayscale);
                Assert.Less(error/before.Length,.003f,"After the pulse the actual board returns to its original pixels.");
                File.WriteAllText(Path.Combine(dir,"metrics.json"),new JObject{["brighterPixels"]=brighter,["meanBrightnessRatio"]=duringSum/beforeSum,["restoredMeanError"]=error/before.Length}.ToString());
            }finally{if(hadFlash)PlayerPrefs.SetString("OnFlash",savedFlash);else PlayerPrefs.DeleteKey("OnFlash");target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);Call(stage,"SetPaused",false);}
        }
    }
}
