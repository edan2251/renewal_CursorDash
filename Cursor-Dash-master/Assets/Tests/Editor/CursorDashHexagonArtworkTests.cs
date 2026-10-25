using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CursorDashTests
{
    public partial class CursorDashSceneRegression
    {
        [UnityTest]
        public IEnumerator HexagonA_TwoInterruptedRingsCounterRotatePauseAndResetWithoutCopies()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var hex=NodesOfType("ArrowNode").First();hex.gameObject.SetActive(true);hex.transform.position=new Vector2(0,2);
            Set(hex,"preparationDuration",.8f);Call(hex,"Execute");yield return null;
            var art=(Component)Get(hex,"originalRings");Assert.AreEqual(6,Get(art,"PiecesPerRing"));Assert.AreEqual(2,art.transform.childCount);
            var first=art.transform.GetChild(0);var second=art.transform.GetChild(1);
            float a=first.localEulerAngles.z,b=second.localEulerAngles.z;
            yield return WaitGameplay(.12f);
            Assert.Less(Mathf.DeltaAngle(a,first.localEulerAngles.z),-1,"First original Rotator moves clockwise.");
            Assert.Greater(Mathf.DeltaAngle(b,second.localEulerAngles.z),1,"Second original Rotator moves counterclockwise.");
            foreach(Transform ring in art.transform){
                var renderer=ring.GetComponent<MeshRenderer>();Assert.IsTrue(renderer.enabled);Assert.Greater(ring.GetComponent<MeshFilter>().sharedMesh.triangles.Length,0);
                var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);Color c=block.GetColor("_Color");
                Color baseline=(Color)Get(hex,"bodyColor");Color expected=ring.GetSiblingIndex()==0?baseline:new Color(baseline.r*.82f,baseline.g*.82f,baseline.b*.82f,baseline.a);
                Assert.That(c.r,Is.EqualTo(expected.r).Within(.001f));Assert.That(c.g,Is.EqualTo(expected.g).Within(.001f));Assert.That(c.b,Is.EqualTo(expected.b).Within(.001f));
            }
            var body=(Component)Get(hex,"flat");var material=body.GetComponent<MeshRenderer>().sharedMaterial;
            Assert.That(material.GetFloat("_Stroke"),Is.EqualTo(.071768f).Within(.00001f));Assert.That(material.GetFloat("_FillScale"),Is.EqualTo(1.014927f).Within(.00001f));
            var stage=Find("StageManager");Call(stage,"SetPaused",true);a=first.localEulerAngles.z;b=second.localEulerAngles.z;
            yield return WaitRealtime(.12f);Assert.That(Mathf.DeltaAngle(a,first.localEulerAngles.z),Is.Zero.Within(.001f));Assert.That(Mathf.DeltaAngle(b,second.localEulerAngles.z),Is.Zero.Within(.001f));Call(stage,"SetPaused",false);
            Call(hex,"ResetObject");Assert.IsFalse((bool)Get(art,"Visible"));Assert.IsTrue(art.GetComponentsInChildren<MeshRenderer>(true).All(r=>!r.enabled));
            hex.gameObject.SetActive(true);hex.transform.position=new Vector2(1,2);Call(hex,"Execute");yield return null;
            Assert.AreSame(art,Get(hex,"originalRings"));Assert.AreEqual(2,art.transform.childCount);
            var one=(Transform)Get(hex,"outsideOne");var two=(Transform)Get(hex,"outsideTwo");
            Assert.Less(first.localScale.magnitude,one.localScale.magnitude,"D-031 rings unfold inside-out before matching their source transforms.");
            yield return WaitGameplay(.22f);
            Assert.That(Quaternion.Angle(one.localRotation,first.localRotation),Is.LessThan(.001f));Assert.That(Quaternion.Angle(two.localRotation,second.localRotation),Is.LessThan(.001f));
            Call(hex,"ResetObject");
        }
    }
}
