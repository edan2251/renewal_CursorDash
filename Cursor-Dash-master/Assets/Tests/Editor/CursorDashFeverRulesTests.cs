using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace CursorDashTests
{
    // No scene setup/EnterPlayMode/PlayerPrefs writes. Pure rules and decimal parsing only.
    public class CursorDashFeverRulesTests
    {
        private static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object New()=>Activator.CreateInstance(Runtime("FeverState"),new object[]{null});
        private static object Call(object m,string method,params object[] args)=>m.GetType().GetMethod(method).Invoke(m,args);
        private static object Get(object m,string name)=>m.GetType().GetProperty(name).GetValue(m);
        private static void Cycle(object m){Call(m,"FillForReview");Call(m,"Tick",100d);}
        [Test] public void KillTokenAwardsOncePerExecutionAndNoMoveBasedCharge()
        {
            var m=New();Assert.IsTrue((bool)Call(m,"Kill",42,1ul));Assert.IsFalse((bool)Call(m,"Kill",42,1ul));
            Assert.IsTrue((bool)Call(m,"Kill",42,2ul));Assert.IsFalse((bool)Call(m,"Kill",42,1ul));
            Assert.AreEqual("2",Get(m,"PointsText"));Assert.AreEqual(.25d,Get(m,"Gauge"));Call(m,"Tick",2d);Assert.AreEqual(.25d,Get(m,"Gauge"));
        }
        [Test] public void ExitCompletionChangesMultiplierOnceAndCapsAtFour()
        {
            var m=New();Call(m,"FillForReview");Call(m,"Tick",.22d);Assert.AreEqual("Entering",Get(m,"Current").ToString());
            Call(m,"Tick",(double)Get(m,"PhaseSeconds"));Assert.AreEqual("Fever",Get(m,"Current").ToString());Call(m,"Kill",1,1ul);Assert.AreEqual("1",Get(m,"PointsText"));
            Call(m,"Tick",8d);Assert.AreEqual("Exiting",Get(m,"Current").ToString());Assert.AreEqual(1,Get(m,"Multiplier"));
            Call(m,"Kill",2,1ul);Assert.AreEqual("2",Get(m,"PointsText"));Call(m,"Tick",(double)Get(m,"PhaseSeconds"));Assert.AreEqual(2,Get(m,"Multiplier"));
            Call(m,"Kill",3,1ul);Assert.AreEqual("4",Get(m,"PointsText"));
            for(int i=0;i<5;i++)Cycle(m);Assert.AreEqual(4,Get(m,"Multiplier"));Assert.AreEqual(6,Get(m,"CompletedFevers"));
            Call(m,"Tick",100d);Assert.AreEqual(6,Get(m,"CompletedFevers"));
        }
        [Test] public void NexusHitPreservesPointsAndDedupWhileClearingMultiplierAndGauge()
        {
            var m=New();Cycle(m);Call(m,"Kill",7,1ul);Call(m,"NexusHit");Assert.AreEqual("2",Get(m,"PointsText"));
            Assert.AreEqual(1,Get(m,"Multiplier"));Assert.AreEqual(0d,Get(m,"Gauge"));Assert.IsFalse((bool)Call(m,"Kill",7,1ul));
            Assert.AreEqual("NexusHit",Get(m,"LastReset"));
        }
        [Test] public void NaturalDecayToZeroRetainsMultiplier()
        {
            var m=New();Cycle(m);Call(m,"Kill",8,1ul);Call(m,"Tick",4d);Assert.AreEqual(.125d,Get(m,"Gauge"));
            Call(m,"Tick",10d);Assert.AreEqual(0d,Get(m,"Gauge"));Assert.AreEqual(2,Get(m,"Multiplier"));
        }
        [Test] public void PauseAndCancelOrRoundEndNeverCompleteFever()
        {
            var m=New();Call(m,"FillForReview");m.GetType().GetProperty("Paused").SetValue(m,true);Call(m,"Tick",100d);
            Assert.AreEqual("Closing",Get(m,"Current").ToString());m.GetType().GetProperty("Paused").SetValue(m,false);
            Call(m,"Tick",2d);Call(m,"CancelFever");Call(m,"Tick",100d);Assert.AreEqual(0,Get(m,"CompletedFevers"));
            Call(m,"FillForReview");Call(m,"Tick",8.7d);Call(m,"Abort");Call(m,"Tick",100d);Assert.AreEqual(1,Get(m,"Multiplier"));
            Assert.IsFalse((bool)Call(m,"Kill",3,1ul));Call(m,"ResetRun");Assert.AreEqual("Normal",Get(m,"Current").ToString());Assert.AreEqual("0",Get(m,"PointsText"));
        }
        [Test] public void FeverKillsNeverRefillAndBoundaryReturnsClockwiseToStart()
        {
            var m=New();Call(m,"FillForReview");Call(m,"Tick",4.57d);double prior=(double)Get(m,"Gauge");Call(m,"Kill",2,1ul);Assert.AreEqual(prior,Get(m,"Gauge"));
            var method=Runtime("FeverState").GetMethod("DrainHead");double previous=0;
            for(int i=0;i<=100;i++){double head=(double)method.Invoke(null,new object[]{i/100d,60d,8d,.12d});Assert.GreaterOrEqual(head,previous);previous=head;}
            Assert.AreEqual(1d,previous);Assert.That(210d+previous*360,Is.EqualTo(570d));
            Assert.That((double)Runtime("FeverState").GetMethod("CloseHead").Invoke(null,new object[]{1d,60d}),Is.EqualTo(1d));
        }
        [Test] public void IntegerPointsParsingDoesNotReinterpretSeconds()
        {
            var parse=Runtime("CursorDashPointsRecord").GetMethod("Parse",BindingFlags.Static|BindingFlags.Public);
            Assert.AreEqual("0",parse.Invoke(null,new object[]{"11.53"}).ToString());Assert.AreEqual("0",parse.Invoke(null,new object[]{"-1"}).ToString());
            string huge="123456789012345678901234567890";Assert.AreEqual(huge,parse.Invoke(null,new object[]{huge}).ToString());
        }
        [Test] public void NormalCombatClockHoldsAcrossTransitionsAndResumesWithRemainder()
        {
            var m=New();Call(m,"Tick",2d);Call(m,"FillForReview");Call(m,"Tick",(double)Get(m,"PhaseSeconds"));
            double clock=(double)Get(m,"NormalCombatSeconds");Assert.IsTrue((bool)Get(m,"SuppressesThreats"));
            foreach(string expected in new[]{"Fever","Exiting","Normal"}){
                Call(m,"Tick",(double)Get(m,"PhaseSeconds"));Assert.AreEqual(expected,Get(m,"Current").ToString());
                Assert.That((double)Get(m,"NormalCombatSeconds"),Is.EqualTo(clock).Within(1e-8));
            }
            Assert.IsFalse((bool)Get(m,"SuppressesThreats"));Call(m,"Tick",.3d);
            Assert.That((double)Get(m,"NormalCombatSeconds"),Is.EqualTo(clock+.3).Within(1e-8));
        }
        [Test] public void CancelAndHitInvalidatePresentationWithoutCompletingOrLosingPoints()
        {
            var m=New();Call(m,"Kill",1,1ul);uint version=(uint)Get(m,"ResetVersion");Call(m,"FillForReview");
            Call(m,"Tick",2d);Call(m,"CancelFever");Assert.Greater((uint)Get(m,"ResetVersion"),version);
            Assert.AreEqual("1",Get(m,"PointsText"));Assert.AreEqual(0,Get(m,"CompletedFevers"));Assert.IsFalse((bool)Get(m,"SuppressesThreats"));
            Call(m,"NexusHit");Call(m,"FillForReview");Assert.AreEqual("",Get(m,"LastReset"));CycleAfterClosing(m);
            Assert.AreEqual(2,Get(m,"Multiplier"));Assert.AreEqual(1,Get(m,"CompletedFevers"));
        }
        private static void CycleAfterClosing(object m)=>Call(m,"Tick",100d);
        [Test] public void BonusFeedbackRequestsAreBoundedForSimultaneousAndContinuousKills()
        {
            var gate=Activator.CreateInstance(Runtime("FeverFeedbackGate"));int stops=0,screens=0;
            for(int i=0;i<1000;i++){double time=i/1000d;
                if((bool)Call(gate,"Stop",time,.18d))stops++;
                if((bool)Call(gate,"Screen",time,.10d))screens++;
            }
            Assert.That(stops,Is.InRange(5,6));Assert.That(screens,Is.InRange(9,10));
            Call(gate,"Reset");Assert.IsTrue((bool)Call(gate,"Stop",0d,.18d));Assert.IsFalse((bool)Call(gate,"Stop",0d,.18d));
        }
        private static double Flow(string name,params object[] values)=>(double)Runtime("FeverFlowMath").GetMethod(name).Invoke(null,values);
        [Test] public void CurvedFieldIsAsymmetricAndCoversViewportAtCompletion()
        {
            double extent=Math.Sqrt(.5*.5+1d);
            Assert.Greater(Math.Abs(Flow("SignedField",.4d,.2d,.4d,extent)-Flow("SignedField",-.4d,-.2d,.4d,extent)),.005);
            for(int x=0;x<=10;x++)for(int y=0;y<=20;y++)
                Assert.Less(Flow("SignedField",x/10d-.5,y/10d-1,1d,extent),0);
        }
        [Test] public void AbsorptionAndExitCurvesRestoreExactlyAndHaveSingleRelease()
        {
            Assert.AreEqual(0d,Flow("EntrySpread",.52d));Assert.AreEqual(1d,Flow("EntrySpread",1d));
            Assert.AreEqual(1d,Flow("DotScale",false,0d));Assert.AreEqual(0d,Flow("DotScale",false,.62d));
            Assert.AreEqual(0d,Flow("DotScale",true,.56d));Assert.That(Flow("DotScale",true,1d),Is.EqualTo(1d).Within(1e-9));
            double previous=1;for(int i=0;i<=100;i++){double next=Flow("ExitRemaining",i/100d);Assert.LessOrEqual(next,previous);previous=next;}
            Assert.AreEqual(0d,previous);
        }
        [Test] public void FiveSegmentsFillContinuouslyWithoutChangingCharge()
        {
            double previous=0;
            for(int n=0;n<=1000;n++){
                double charge=n/1000d,sum=0;for(int i=0;i<5;i++)sum+=Flow("SegmentFill",charge,i);
                Assert.That(sum/5,Is.EqualTo(charge).Within(1e-9));Assert.GreaterOrEqual(sum,previous);previous=sum;
            }
        }
        [Test] public void OuterArcsLeadDottedAbsorptionAndAllRestore()
        {
            Assert.Less(Flow("OuterScale",0,false,.15d),1);Assert.AreEqual(1d,Flow("DotScale",false,.15d));
            for(int i=0;i<5;i++){Assert.AreEqual(0d,Flow("OuterScale",i,false,.66d));Assert.That(Flow("OuterScale",i,true,1d),Is.EqualTo(1d).Within(1e-9));}
        }
        [Test] public void DottedRepeatedEventsAreBoundedAndSettleWithoutDirectionReversal()
        {
            var spin=Activator.CreateInstance(Runtime("FeverRingSpin"));var normal=Enum.Parse(Runtime("FeverState").GetNestedType("Phase"),"Normal");
            for(int i=0;i<1000;i++)Call(spin,"Pulse",true);
            Assert.That((double)Get(spin,"Boost"),Is.EqualTo(780));Assert.AreEqual(0d,Call(spin,"Advance",0d,normal,0d));
            for(int i=0;i<1000;i++)Assert.Greater((double)Call(spin,"Advance",.01d,normal,0d),0);
            Assert.That((double)Get(spin,"Speed"),Is.EqualTo(24d).Within(.001));
        }
    }
}
