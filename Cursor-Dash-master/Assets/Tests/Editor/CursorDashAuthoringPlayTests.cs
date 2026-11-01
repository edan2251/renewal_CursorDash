#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    public partial class CursorDashSceneRegression
    {
        [UnityTest]
        public IEnumerator AuthoredPresentationDefaultsDoNotDisqualifyButExplicitDevStillDoes()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            var stage=Find("StageManager");Assert.IsTrue((bool)Get(stage,"RecordEligible"));
            var type=TypeFor("CursorDashNodeAppearance");var source=Resources.Load("CursorDash/NodeAppearance",type);
            var temporary=Object.Instantiate(source);var cache=type.GetField("cached",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            var previous=cache.GetValue(null);var nexus=Get(stage,"NexusView");var tint=(Color)Get(nexus,"EnergyTint");
            var player=Find("PlayerCharacterController");var cursor=(SpriteRenderer)Get(player,"cursorVisual");
            float originalScale=(float)Get(player,"sourceVisualScale");Vector3 originalPose=(Vector3)Get(player,"sourceVisualPose");
            Vector2 colliderSize=player.GetComponent<BoxCollider2D>().size;
            var tuning=TypeFor("CursorDashTuning");var signal=tuning.GetField("RuleSessionChanged");int changes=0;Action watch=()=>changes++;
            signal.SetValue(null,(Action)signal.GetValue(null)+watch);
            try
            {
                type.GetField("outline").SetValue(temporary,.063f);cache.SetValue(null,temporary);
                Set(player,"sourceVisualScale",.27f);Set(player,"sourceVisualPose",new Vector3(.27f,.19f,1));Call(player,"ApplyRoundTuning");
                Assert.AreEqual(new Vector3(.27f,.19f,1),cursor.transform.localScale);Assert.AreEqual(colliderSize,player.GetComponent<BoxCollider2D>().size);
                Set(nexus,"energyTint",new Color(.2f,.3f,.4f));Call(Get(stage,"Energy"),"SetTarget",1f);
                Call(Get(stage,"NodeCreator"),"CreateNode",0,0);yield return null;
                var node=NodesOfType("NormalNode").First(n=>n.gameObject.activeSelf);var artwork=Get(node,"flat");
                Assert.That((float)Get(artwork,"stroke"),Is.EqualTo(.063f).Within(.00001f));
                Call(node,"ContactAt",(Vector2)node.transform.position,Vector2.up);yield return WaitRealtime(.12f);
                Assert.AreEqual(0,changes,"Reading authored presentation values is not a rule override.");
                Assert.IsTrue((bool)Get(stage,"RecordEligible"));Assert.AreEqual("1",Get(Get(Get(stage,"Fever"),"Model"),"PointsText"));
                var enableDev=tuning.GetMethod("EnableDev");Assert.IsNotNull(enableDev);
                enableDev.Invoke(null,new object[]{true});
                Assert.Greater(changes,0);Assert.IsFalse((bool)Get(stage,"RecordEligible"),"Existing explicit developer record exclusion remains unchanged.");
            }
            finally
            {
                cache.SetValue(null,previous);Set(nexus,"energyTint",tint);Set(player,"sourceVisualScale",originalScale);Set(player,"sourceVisualPose",originalPose);cursor.transform.localScale=originalPose;
                signal.SetValue(null,(Action)signal.GetValue(null)-watch);Object.Destroy(temporary);
            }
        }
    }
}
#endif
