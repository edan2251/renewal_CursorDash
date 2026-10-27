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
        [Test] public void FeverDrainsCompletelyBeforeExitRecallAndExitPauseOrAbortCannotAward()
        {
            var m=New();Call(m,"FillForReview");Call(m,"Tick",.22d);Call(m,"Tick",1.05d);Call(m,"Tick",7.999d);
            Assert.AreEqual("Fever",Get(m,"Current").ToString());Assert.Greater((double)Get(m,"Gauge"),0);
            Assert.That(Flow("OpticsWeight",7.999d,.001d),Is.EqualTo(1));Call(m,"Tick",.001d);
            Assert.AreEqual("Exiting",Get(m,"Current").ToString());Assert.AreEqual(0d,Get(m,"Gauge"));Assert.AreEqual(1d,Flow("ExitRemaining",0d));
            m.GetType().GetProperty("Paused").SetValue(m,true);double held=(double)Get(m,"Elapsed");Call(m,"Tick",10d);Assert.AreEqual(held,Get(m,"Elapsed"));Assert.AreEqual(0,Get(m,"CompletedFevers"));
            Call(m,"Abort");Call(m,"Tick",10d);Assert.AreEqual("Stopped",Get(m,"Current").ToString());Assert.AreEqual(0,Get(m,"CompletedFevers"));
            Call(m,"ResetRun");Assert.AreEqual("Normal",Get(m,"Current").ToString());Assert.AreEqual(1,Get(m,"Multiplier"));
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
            Assert.AreEqual(0d,Flow("DotScale",true,0d));Assert.Greater(Flow("DotScale",true,.20d),.9);Assert.That(Flow("DotScale",true,1d),Is.EqualTo(1d).Within(1e-9));
            double previous=1;for(int i=0;i<=100;i++){double next=Flow("ExitRemaining",i/100d);Assert.LessOrEqual(next,previous);previous=next;}
            Assert.AreEqual(0d,previous);
        }
        [Test] public void ExitShowsEmptyGaugeThenAcceleratesItsInwardTravelWithoutExtendingFever()
        {
            Assert.AreEqual(1,Flow("ExitRemaining",.06d));Assert.AreEqual(0,Flow("ExitRemaining",.90d));
            double early=Flow("ExitRemaining",.18d)-Flow("ExitRemaining",.28d),late=Flow("ExitRemaining",.60d)-Flow("ExitRemaining",.70d);
            Assert.Greater(late,early*3,"Final convergence must accelerate, not ease out like the previous mask.");
            foreach(double aspect in new[]{.4,1d,2.5}){
                double ax=Math.Max(1,aspect),ay=Math.Max(1,1/aspect),extent=Math.Sqrt(ax*ax+ay*ay)*.7;
                for(int i=0;i<12;i++){double a=i*Math.PI/6;Assert.Less(Flow("ExitSignedField",Math.Cos(a)*extent,Math.Sin(a)*extent,.06d,extent),0);}
                Assert.Greater(Flow("ExitSignedField",0d,0d,1d,extent),0,"No central residual mask at completion.");
            }
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
        private static object ExitMotion()=>Activator.CreateInstance(Runtime("FeverExitMotion"),new object[]{.20d,.51d,.24d,3.2d,1260d,.10d,.12d,.28d,.90d,.006d,.0035d,.012d});
        [Test] public void ExitRingLeadsSuctionAndSharesOneFinalPressureAndRecoilPeak()
        {
            var motion=ExitMotion();double impact=(double)Get(motion,"Impact"),total=(double)Get(motion,"Total");
            Assert.That(total,Is.EqualTo(.95).Within(1e-8));Assert.AreEqual(0d,Call(motion,"Suction",.20d));
            Assert.AreEqual(0d,Call(motion,"RingAlpha",0d));Assert.AreEqual(180d,Call(motion,"Speed",.20d));Assert.AreEqual(0d,Call(motion,"Pressure",.20d));Assert.AreEqual(1d,Call(motion,"RingAlpha",.20d));
            foreach(double t in new[]{.40,.50,.6,impact})Assert.AreEqual(1260d,Call(motion,"Speed",t));
            Assert.AreEqual(1d,Call(motion,"Pressure",impact));Assert.AreEqual(.88d,Call(motion,"RingScale",impact));
            Assert.AreEqual(0d,Call(motion,"Pressure",total));Assert.AreEqual(1d,Call(motion,"RingScale",total));Assert.AreEqual(24d,Call(motion,"Speed",total));
        }
        [Test] public void ExitRecallStartsAtTheNodeAndCurvesOnceToTheActualRing()
        {
            var motion=ExitMotion();var method=motion.GetType().GetMethod("Position");
            for(int i=0;i<14;i++){
                double a=i*Math.PI*2/14,sx=1+Math.Cos(a)*3,sy=-2+Math.Sin(a)*3;
                object[] args={sx,sy,1d,-2d,.7d,0d,i,0d,0d};method.Invoke(motion,args);Assert.That((double)args[7],Is.EqualTo(sx).Within(1e-8));Assert.That((double)args[8],Is.EqualTo(sy).Within(1e-8));
                args[5]=.6d;method.Invoke(motion,args);double cross=((double)args[7]-sx)*(-2-sy)-((double)args[8]-sy)*(1-sx);Assert.Greater(Math.Abs(cross),.01,"Each path bends away from a straight radial line.");
                args[5]=1d;method.Invoke(motion,args);double dx=(double)args[7]-1,dy=(double)args[8]+2;Assert.That(Math.Sqrt(dx*dx+dy*dy),Is.EqualTo(.7).Within(1e-8));
            }
            var scale=Runtime("FeverExitMotion").GetMethod("NodeScale");Assert.AreEqual(1d,scale.Invoke(null,new object[]{.68d}));Assert.AreEqual(0d,scale.Invoke(null,new object[]{1d}));
        }
        [Test] public void ExitMaskAndArrivalFinishAtImpactEvenForEmptyOrFullRecall()
        {
            var motion=ExitMotion();var type=motion.GetType();double maximum=4;
            foreach(int count in new[]{0,1,14})for(int i=0;i<count;i++){
                double at=(double)type.GetMethod("Arrival").Invoke(null,new object[]{1d+i*.2,maximum,i,i==count-1});Assert.That(at,Is.InRange(.84d,1d));if(i==count-1)Assert.AreEqual(1d,at);
            }
            double early=(double)type.GetMethod("Travel").Invoke(null,new object[]{.2d,2.6d})-(double)type.GetMethod("Travel").Invoke(null,new object[]{.1d,2.6d});
            double late=(double)type.GetMethod("Travel").Invoke(null,new object[]{1d,2.6d})-(double)type.GetMethod("Travel").Invoke(null,new object[]{.9d,2.6d});Assert.Greater(late,early*5);
            Assert.Greater((double)Runtime("FeverFlowMath").GetMethod("RecallField").Invoke(null,new object[]{0d,0d,1d,2d,2.6d,.90d}),0,"No residual center mask at final absorption, regardless of node count.");
        }

        [Test] public void ExitAppearanceHasNoSuctionAndDistanceShrinkWaitsForTheRing()
        {
            var motion=ExitMotion();double appear=(double)motion.GetType().GetField("Appear").GetValue(motion),impact=(double)Get(motion,"Impact");
            foreach(double t in new[]{0d,.05,.10,.15,appear}){Assert.AreEqual(0d,Call(motion,"Suction",t));Assert.AreEqual(0d,Call(motion,"Pressure",t));Assert.AreEqual(180d,Call(motion,"Speed",t));}
            Assert.AreEqual(1d,Call(motion,"SizeAtDistance",.50d));Assert.AreEqual(1d,Call(motion,"SizeAtDistance",.22d));Assert.AreEqual(0d,Call(motion,"SizeAtDistance",0d));
            Assert.Greater((double)Call(motion,"StretchAt",.5d),1.5);Assert.AreEqual(1d,Call(motion,"StretchAt",0d));
            Assert.AreEqual(.88d,Call(motion,"RingScale",impact+.05d),"Recoil remains visible after the white mask reaches zero.");
            Assert.AreEqual(1d,Call(motion,"RingScale",(double)Get(motion,"Total")));
        }
        [Test] public void ScoreAwardNotificationIsExactAndDuplicateOrResetNeverPulses()
        {
            var model=New();int events=0;System.Numerics.BigInteger delta=0,total=0;
            Action<System.Numerics.BigInteger,System.Numerics.BigInteger> observe=(d,t)=>{events++;delta=d;total=t;};
            model.GetType().GetEvent("PointsAwarded").AddEventHandler(model,observe);
            Assert.IsTrue((bool)Call(model,"Kill",100,1ul));Assert.AreEqual(1,events);Assert.AreEqual(System.Numerics.BigInteger.One,delta);
            Assert.IsFalse((bool)Call(model,"Kill",100,1ul));Assert.AreEqual(1,events);Cycle(model);Assert.IsTrue((bool)Call(model,"Kill",101,1ul));
            Assert.AreEqual(new System.Numerics.BigInteger(2),delta);Assert.AreEqual(Get(model,"PointsText"),total.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Call(model,"NexusHit");Call(model,"ResetRun");Assert.AreEqual(2,events);
        }
        [Test] public void BackgroundScoreUsesOneBoundedSpringAndFitsDigitsAtMaximumPulse()
        {
            object Make()=>Activator.CreateInstance(Runtime("ScoreProjectionPulse"));var small=Make();var large=Make();
            Call(small,"Award",System.Numerics.BigInteger.One);Call(large,"Award",new System.Numerics.BigInteger(4));
            double first=(double)Get(small,"Value"),big=(double)Get(large,"Value");Assert.Greater(big,first);
            // D-055: the requested small scoring undershoot has a .955 floor; all upper/opacity/pause/fit assertions remain.
            for(int i=0;i<600;i++){if(i%7==0)Call(large,"Award",new System.Numerics.BigInteger(4));Call(large,"Advance",1d/60,.32d);Assert.That((double)Call(large,"Scale",.22d,1.22d),Is.InRange(.955d,1.22d));Assert.LessOrEqual((double)Call(large,"Opacity",.105d,.14d,.24d),.24d);}
            double held=(double)Get(large,"Value");Call(large,"Advance",0d,.32d);Assert.AreEqual(held,Get(large,"Value"));Call(large,"Advance",2d,.32d);Assert.That((double)Get(large,"Value"),Is.EqualTo(0).Within(.001));Call(large,"Clear");Assert.AreEqual(0d,Get(large,"Value"));
            foreach(double width in new[]{1d,8d,100d}){double fit=(double)Runtime("ScoreProjectionPulse").GetMethod("Fit").Invoke(null,new object[]{width,2d,3d,5d,4d,1.22d});Assert.LessOrEqual(width*fit*1.22,5d+1e-8);Assert.LessOrEqual(2*fit*1.22,4d+1e-8);}
        }
    }
}
