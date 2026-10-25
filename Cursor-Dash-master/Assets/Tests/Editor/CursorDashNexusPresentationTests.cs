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
        public IEnumerator Nexus_ImmediateHealthCutSingleCompressionZeroAndRetry()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var energy=Get(stage,"Energy");var score=Get(stage,"scoreManager");float max=(float)Get(score,"MaxHp");
            Set(stage,"PracticeInvulnerable",false);Set(score,"Hp",max*.5f);
            Assert.That((float)Get(energy,"DisplayedLevel"),Is.EqualTo(.5f).Within(.00001f));Assert.Greater((float)Get(energy,"SurfacePress"),0);
            yield return WaitRealtime(.18f);Assert.That((float)Get(energy,"SurfacePress"),Is.Zero);Assert.That((float)Get(energy,"DisplayedLevel"),Is.EqualTo(.5f).Within(.00001f));
            Set(score,"Hp",max);Assert.That((float)Get(energy,"SurfacePress"),Is.Zero);
            for(int i=1;i<=3;i++){
                Call(stage,"DamageNexus",max*.1f);
                Assert.That((float)Get(energy,"DisplayedLevel"),Is.EqualTo(1-i*.1f).Within(.00001f),"Each hit displays actual HP immediately, even in the same frame.");
            }
            yield return WaitRealtime(.18f);Assert.That((float)Get(energy,"SurfacePress"),Is.Zero);Assert.That((float)Get(energy,"DisplayedLevel"),Is.EqualTo(.7f).Within(.00001f));
            Set(score,"Hp",0f);Assert.IsTrue((bool)Get(stage,"IsDeath"));Assert.That((float)Get(energy,"DisplayedLevel"),Is.Zero);Assert.That((float)Get(energy,"SurfacePress"),Is.Zero);
            Call(stage,"Retry");yield return WaitForScene("Cyberpunk");yield return null;yield return null;DisablePatterns();
            var next=Find("StageManager");Assert.AreNotSame(stage,next);Assert.That((float)Get(Get(next,"Energy"),"DisplayedLevel"),Is.EqualTo(1));Assert.That((float)Get(Get(next,"Energy"),"SurfacePress"),Is.Zero);
        }

        [UnityTest]
        public IEnumerator Nexus_OnlyThreeOriginalImagesJoinBoardAndPreserveMeasuredBoundary()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var presentation=(Component)Get(stage,"NexusPresentation");
            Assert.IsTrue((bool)Get(presentation,"CanComposite"),"The actual nexus shader and supported original images must bind.");
            var sources=((IEnumerable)Get(presentation,"SourceImages")).Cast<Component>().ToArray();
            CollectionAssert.AreEquivalent(new[]{"HpBar","HpFrame","HpCentral"},sources.Select(i=>i.name).ToArray());
            Assert.IsTrue(sources.All(i=>!((Behaviour)i).enabled));
            Assert.That(Vector2.Distance((Vector2)Get(presentation,"WorldCenter"),(Vector2)Get(stage,"NexusPosition")),Is.LessThan(.0001f));
            Assert.That((float)Get(presentation,"BodyRadius"),Is.EqualTo((float)Get(stage,"NexusRadius")).Within(.0001f));
            var distortion=Get(Get(stage,"Feedback"),"Distortion");var uiCamera=(Camera)Get(distortion,"UiCamera");Vector3 stable=uiCamera.transform.position;
            Vector3[] positions=sources.Select(s=>s.transform.position).ToArray();Call(stage,"CameraShake",.10f,.14f);yield return null;
            Assert.That(Vector3.Distance(Camera.main.transform.position,uiCamera.transform.position),Is.GreaterThan(.0001f));Assert.AreEqual(stable,uiCamera.transform.position);
            for(int i=0;i<sources.Length;i++)Assert.AreEqual(positions[i],sources[i].transform.position,"Original layout/boundary objects never move with camera shake.");
            Call(presentation,"SetDriverActive",false);Assert.IsTrue(sources.All(i=>((Behaviour)i).enabled));Call(presentation,"SetDriverActive",true);Assert.IsTrue(sources.All(i=>!((Behaviour)i).enabled));
        }
    }
}
