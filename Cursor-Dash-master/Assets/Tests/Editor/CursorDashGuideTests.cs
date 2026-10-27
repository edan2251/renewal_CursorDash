#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NUnit.Framework;

namespace CursorDashTests
{
    // Uses the existing runtime-reflection boundary: no Assembly-CSharp reference or user storage access.
    public sealed class CursorDashGuideTests
    {
        private static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Make(string name)=>Activator.CreateInstance(Runtime(name));
        private static object Get(object owner,string name)=>owner.GetType().GetProperty(name).GetValue(owner);
        private static object Call(object owner,string name,params object[] args)=>owner.GetType().GetMethod(name).Invoke(owner,args);
        private static bool Win(object flow,int kind,int source,ulong execution)=>(bool)Call(flow,"Accept",kind,source,execution);
        [Test] public void BothKindsCanCompleteInEitherOrderWithoutExceedingTwo()
        {
            foreach(bool hexFirst in new[]{false,true}){
                var flow=Make("CursorDashGuideFlow");int first=hexFirst?1:0,second=1-first;
                Assert.IsTrue(Win(flow,first,1,1));Assert.IsTrue(Win(flow,first,1,2));Assert.IsFalse(Win(flow,first,1,3));Assert.AreEqual("Practicing",Get(flow,"Current").ToString());
                Assert.IsTrue(Win(flow,second,2,1));Assert.IsTrue(Win(flow,second,2,2));Assert.AreEqual("Settling",Get(flow,"Current").ToString());Assert.AreEqual(2,Get(flow,"CircleWins"));Assert.AreEqual(2,Get(flow,"HexWins"));
            }
        }
        [Test] public void DuplicateOldExecutionAndLatePreviousPhaseCannotSkip()
        {
            var flow=Make("CursorDashGuideFlow");Assert.IsFalse(Win(flow,0,1,0));Assert.IsTrue(Win(flow,0,1,1));Assert.IsFalse(Win(flow,0,1,1));Assert.IsTrue(Win(flow,0,1,2));
            Assert.IsFalse(Win(flow,0,1,99));Assert.AreEqual(0,Get(flow,"HexWins"));Assert.AreEqual(2,Get(flow,"CircleWins"));
        }
        [Test] public void AlternatingSuccessAndLastCallbackCompleteOnlyOnce()
        {
            var flow=Make("CursorDashGuideFlow");Assert.IsTrue(Win(flow,1,7,1));Assert.IsTrue(Win(flow,0,3,1));Assert.IsTrue(Win(flow,1,7,2));Assert.IsTrue(Win(flow,0,3,2));
            Assert.IsFalse(Win(flow,0,3,2));Assert.IsFalse(Win(flow,1,7,3));Assert.AreEqual("Settling",Get(flow,"Current").ToString());
        }
        [Test] public void ReflectedResolvingNodeSurvivesUntilActualImpact()
        {
            var rules=Runtime("CursorDashGuideRules");var method=rules.GetMethod("NeedsRecovery");
            Assert.IsFalse((bool)method.Invoke(null,new object[]{true,true,true,true,false}),"Resolving is a live reflected return, not a failed practice target.");
            Assert.IsTrue((bool)method.Invoke(null,new object[]{true,true,false,false,true}));Assert.IsTrue((bool)method.Invoke(null,new object[]{false,true,false,false,false}));
        }
        [Test] public void ResetWaitsForItsClockAndPausePreservesIt()
        {
            var flow=Make("CursorDashGuideFlow");Win(flow,0,1,1);Win(flow,0,1,2);Win(flow,1,2,1);Win(flow,1,2,2);
            Call(flow,"AdvanceFinish",10d,true,true,.35d,.4d);Assert.AreEqual("Settling",Get(flow,"Current").ToString());
            Call(flow,"AdvanceFinish",.4d,false,false,.35d,.4d);Assert.AreEqual("Settling",Get(flow,"Current").ToString());
            Call(flow,"AdvanceFinish",.01d,false,true,.35d,.4d);Assert.AreEqual("Fading",Get(flow,"Current").ToString());
            Call(flow,"AdvanceFinish",.4d,false,true,.35d,.4d);Assert.AreEqual("Reset",Get(flow,"Current").ToString());
            Call(flow,"AdvanceReset",9d,true,1.1d);Assert.AreEqual(0d,Get(flow,"ResetAge"));Call(flow,"AdvanceReset",.6d,false,1.1d);Assert.AreEqual("Reset",Get(flow,"Current").ToString());
            Call(flow,"AdvanceReset",.6d,false,1.1d);Assert.AreEqual("Done",Get(flow,"Current").ToString());
        }
        [Test] public void InterruptedTutorialNeverCompletes()
        {
            var flow=Make("CursorDashGuideFlow");Win(flow,0,1,1);Call(flow,"Cancel");Assert.IsFalse(Win(flow,0,1,2));Call(flow,"AdvanceReset",10d,false,1d);Assert.AreEqual("Cancelled",Get(flow,"Current").ToString());
        }
        private static object Notice(){var n=Make("FeverFirstNotice");Call(n,"Begin",.10d,.80d,.32d,.24d,.06d);return n;}
        [Test] public void BuiltWorldNoticeHoldsTwoThenFadesTwoWithoutSpendingFever()
        {
            var n=Make("FeverFirstNotice");Call(n,"Begin",0d,2d,2d,0d,0d);
            Assert.AreEqual(1d,Get(n,"Alpha"));Assert.AreEqual(0d,Get(n,"TimeScale"));Assert.AreEqual(4d,Get(n,"Total"));
            Call(n,"Advance",2d,false);Assert.AreEqual(1d,Get(n,"Alpha"));Assert.AreEqual(0d,Get(n,"PreviewAge"));
            Call(n,"Advance",1d,false);Assert.AreEqual(.5d,Get(n,"Alpha"));Assert.AreEqual(1d,Get(n,"PreviewAge"));Assert.AreEqual(0d,Get(n,"TimeScale"));
            Assert.IsTrue((bool)Call(n,"Advance",1d,false));Assert.AreEqual(0d,Get(n,"Alpha"));Assert.AreEqual(1d,Get(n,"TimeScale"));Assert.IsFalse((bool)Get(n,"Active"));
        }
        [Test] public void AlternatingSupplyAndResetVisualsKeepCountersAndBodyBoundarySeparate()
        {
            var rules=Runtime("CursorDashGuideRules");var next=rules.GetMethod("NextKind");Assert.AreEqual(0,next.Invoke(null,new object[]{0,0,0}));Assert.AreEqual(1,next.Invoke(null,new object[]{1,1,0}));Assert.AreEqual(1,next.Invoke(null,new object[]{0,2,0}));
            var flow=Runtime("CursorDashGuideFlow");Assert.That((double)flow.GetMethod("ResetBody").Invoke(null,new object[]{.4d}),Is.EqualTo(1d).Within(.00001));
            Assert.AreEqual(0d,flow.GetMethod("ResetScore").Invoke(null,new object[]{.5d}));Assert.AreEqual(1d,flow.GetMethod("ResetScore").Invoke(null,new object[]{1d}));
        }
        [Test] public void FirstFeverReadPhaseIsOpaqueAndNotButtonDriven()
        {
            var n=Notice();Assert.AreEqual(0d,Get(n,"TimeScale"));Call(n,"Advance",.20d,false);Assert.AreEqual(1d,Get(n,"Alpha"));Assert.IsTrue((bool)Get(n,"Active"));
        }
        [Test] public void NoticePauseCancelAndCompletionAreBounded()
        {
            var n=Notice();Call(n,"Advance",.2d,false);Call(n,"Advance",100d,true);Assert.AreEqual(.2d,Get(n,"Elapsed"));int finished=0;
            for(int i=0;i<100;i++){if((bool)Call(n,"Advance",.02d,false))finished++;Assert.That((double)Get(n,"Alpha"),Is.InRange(0d,1d));Assert.That((double)Get(n,"TimeScale"),Is.InRange(0d,1d));}
            Assert.AreEqual(1,finished);Assert.AreEqual(0d,Get(n,"Alpha"));Assert.AreEqual(1d,Get(n,"TimeScale"));
            n=Notice();Call(n,"Cancel");Assert.IsFalse((bool)Call(n,"Advance",10d,false));Assert.IsFalse((bool)Get(n,"Active"));
        }
        [Test] public void GuidedPracticeRetainsAwardsWithoutFeverOrOfficialClock()
        {
            var type=Runtime("FeverState");var settings=Make("FeverSettings");var model=Activator.CreateInstance(type,new[]{settings});type.GetProperty("PracticeOnly").SetValue(model,true);
            for(int i=0;i<32;i++)Assert.IsTrue((bool)Call(model,"Kill",i,1UL));Call(model,"Tick",10d);
            Assert.AreEqual("Normal",Get(model,"Current").ToString());Assert.AreEqual("32",Get(model,"PointsText"));Assert.AreEqual(0d,Get(model,"NormalCombatSeconds"));Assert.AreEqual(1,Get(model,"Multiplier"));
        }
        [Test] public void PresentationHoldDoesNotSpendGaugeAndRejectsBonusAward()
        {
            var type=Runtime("FeverState");var model=Activator.CreateInstance(type,new[]{Make("FeverSettings")});Call(model,"FillForReview");Call(model,"Tick",1.27d);
            type.GetProperty("PresentationHold").SetValue(model,true);double gauge=(double)Get(model,"Gauge"),age=(double)Get(model,"Elapsed");Call(model,"Tick",100d);
            Assert.AreEqual(gauge,Get(model,"Gauge"));Assert.AreEqual(age,Get(model,"Elapsed"));Assert.IsFalse((bool)Call(model,"Kill",7,1UL));
            type.GetProperty("PresentationHold").SetValue(model,false);Call(model,"Tick",8d);Assert.AreEqual(0d,Get(model,"Gauge"));Assert.AreEqual("Exiting",Get(model,"Current").ToString());
        }
    }
}
#endif
