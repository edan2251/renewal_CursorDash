#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CursorDashTests
{
    public class CursorDashCircuitTests
    {
        // This asmdef intentionally cannot reference Assembly-CSharp. Match the existing regression tests.
        private static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Field(object value,string name)=>value.GetType().GetField(name).GetValue(value);
        private static object Property(object value,string name)=>value.GetType().GetProperty(name).GetValue(value);
        private static object Call(object value,string name,params object[] args)=>value.GetType().GetMethod(name).Invoke(value,args);
        private static object Point(double x,double y)=>Activator.CreateInstance(Runtime("CircuitPathGraph+Point"),new object[]{x,y});
        private static double X(object point)=>(double)Field(point,"X");
        private static double Y(object point)=>(double)Field(point,"Y");
        private sealed class Graph
        {
            public readonly object Value;public readonly Array Points,Edges,Roots;public readonly double MaxDepth;
            public Graph(double x,double y,int density=12){Value=Activator.CreateInstance(Runtime("CircuitPathGraph"),new object[]{x,y,density});Points=(Array)Field(Value,"Points");Edges=(Array)Field(Value,"Edges");Roots=(Array)Field(Value,"Roots");MaxDepth=(double)Field(Value,"MaxDepth");}
            public object Midpoint(int edge){var e=Edges.GetValue(edge);var a=Points.GetValue((int)Field(e,"A"));var b=Points.GetValue((int)Field(e,"B"));return Point((X(a)+X(b))*.5,(Y(a)+Y(b))*.5);}
            public double DeathFront(double age,double speed,double stagger,int root)=>(double)Call(Value,"DeathFront",age,speed,stagger,root);
        }
        private sealed class Pulses
        {
            private readonly object value;
            public Pulses(Graph graph){value=Activator.CreateInstance(Runtime("CircuitPulseField"),new[]{graph.Value});}
            public int Count=>(int)Property(value,"Count");
            public bool Hit(object point,double radius,int cap)=>(bool)Call(value,"Hit",point,radius,cap);
            public void Advance(double delta,double speed,double seconds,double decay,int cap)=>Call(value,"Advance",delta,speed,seconds,decay,cap);
            public double Level(int edge,double t,double speed,double seconds,double decay)=>(double)Call(value,"Level",edge,t,speed,seconds,decay);
            public void Clear()=>Call(value,"Clear");
        }
        [TestCase(1,1.7777778)] [TestCase(1,1)] [TestCase(2.16,1)]
        public void DiagonalRoutesAreStableAndLeaveNexusSpaceUnderViewportStretch(double x,double y)
        {
            var graph=new Graph(x,y);var again=new Graph(x,y);
            Assert.AreEqual(12,graph.Roots.Length);Assert.AreEqual(82,graph.Edges.Length);
            for(int i=0;i<graph.Points.Length;i++){
                var point=graph.Points.GetValue(i);var other=again.Points.GetValue(i);
                Assert.AreEqual(X(point),X(other));Assert.AreEqual(Y(point),Y(other));
                Assert.Greater((double)Property(point,"Length"),.19,"Do not converge every route into the Nexus.");
            }
            // Preserve viewport mapping; diagonal turns are authored in 1080x1920 design space.
            foreach(var edge in graph.Edges){var a=graph.Points.GetValue((int)Field(edge,"A"));var b=graph.Points.GetValue((int)Field(edge,"B"));double dx=(X(b)-X(a))/x*1080,dy=(Y(b)-Y(a))/y*1920;Assert.IsTrue(Math.Abs(dx)<1e-8||Math.Abs(dy)<1e-8||Math.Abs(Math.Abs(dx)-Math.Abs(dy))<1e-8);Assert.Greater((double)Field(edge,"Length"),0);}
        }
        [Test] public void DistantHitDoesNotJumpToTheOppositeCircuit()
        {
            var graph=new Graph(1,1.8);var pulse=new Pulses(graph);
            Assert.IsFalse(pulse.Hit(Point(0,0),.13,4));Assert.AreEqual(0,pulse.Count);
        }
        [Test] public void ConnectedBranchReceivesADelayedWeakerPulseButOtherTreesStayQuiet()
        {
            var graph=new Graph(1,1.8);var pulse=new Pulses(graph);var midpoint=graph.Midpoint(2);
            Assert.IsTrue(pulse.Hit(midpoint,.001,4));double seedPeak=0,branchPeak=0;
            for(int i=0;i<150;i++){
                pulse.Advance(.01,1.65,.24,.26,4);seedPeak=Math.Max(seedPeak,pulse.Level(2,.5,1.65,.24,.26));branchPeak=Math.Max(branchPeak,pulse.Level(8,.5,1.65,.24,.26));
                // Edge 12 is a separate tree, not merely a farther segment of the left bus.
                Assert.AreEqual(0,pulse.Level(12,.5,1.65,.24,.26));
            }
            Assert.Greater(seedPeak,.5);Assert.Greater(branchPeak,.01);Assert.Less(branchPeak,seedPeak);Assert.AreEqual(0,pulse.Count);
        }
        [Test] public void RepeatedHitsAreBoundedAndPauseDoesNotAdvanceTheirAge()
        {
            var graph=new Graph(1,1.8);var pulse=new Pulses(graph);var p=graph.Midpoint(1);
            for(int i=0;i<40;i++){Assert.IsTrue(pulse.Hit(p,.001,4));Assert.LessOrEqual(pulse.Count,4);}
            pulse.Advance(.04,1.65,.24,.26,4);double before=pulse.Level(1,.5,1.65,.24,.26);
            pulse.Advance(0,1.65,.24,.26,4);Assert.AreEqual(before,pulse.Level(1,.5,1.65,.24,.26));
            pulse.Advance(0,1.65,.24,.26,1);Assert.AreEqual(1,pulse.Count);pulse.Clear();Assert.AreEqual(0,pulse.Count);
        }
        [Test] public void TipsTravelAlongEdgesAndDeathRetractsTowardTheOutsideBeforeResults()
        {
            var graph=new Graph(1,1.8,100);Assert.AreEqual(12,graph.Roots.Length);
            var edge=graph.Edges.GetValue(2);double depth=(double)Field(edge,"Depth"),length=(double)Field(edge,"Length");
            double Visible(double front)=>(double)Runtime("CircuitPathGraph").GetMethod("VisibleLength").Invoke(null,new[]{edge,(object)front});
            Assert.AreEqual(0,Visible(depth-.01));Assert.AreEqual(length*.5,Visible(depth+length*.5),1e-8);
            for(int root=0;root<graph.Roots.Length;root++){
                double last=graph.MaxDepth;for(int i=0;i<100;i++){double front=graph.DeathFront(i*.01,.4,.04,root);Assert.LessOrEqual(front,last+1e-8);last=front;}
                Assert.AreEqual(0,graph.DeathFront(.601,.4,.04,root));
            }
        }
        [Test] public void ResetCircuitDefaultsLeavesApprovedBackgroundFeverAndScoreValuesAlone()
        {
            var profile=ScriptableObject.CreateInstance(Runtime("CursorDashVisualProfile"));
            try{
                void Set(string name,object value)=>profile.GetType().GetField(name).SetValue(profile,value);
                Set("backgroundSpeed",.273f);Set("feverBreakStrength",2.5f);Set("backgroundScoreY",.5f);Set("backgroundScoreReaction",.22f);
                Set("backgroundScoreOpacity",.017f);
                Set("circuitDensity",4);Set("circuitLineWidth",.006f);Set("dynamicCircuitsEnabled",false);Call(profile,"ResetCircuitDefaults");
                Assert.AreEqual(12,Field(profile,"circuitDensity"));Assert.AreEqual(.025925926f,Field(profile,"circuitLineWidth"));Assert.IsTrue((bool)Field(profile,"dynamicCircuitsEnabled"));
                Assert.AreEqual(.009259259f,Field(profile,"circuitBranchWidth"));Assert.AreEqual(.02962963f,Field(profile,"circuitContactRadius"));Assert.AreEqual(.006481481f,Field(profile,"circuitContactStroke"));
                Assert.AreEqual(.043055556f,Field(profile,"circuitMainWidth"));Assert.AreEqual(.075f,Field(profile,"circuitNormalContrast"));Assert.AreEqual(.065f,Field(profile,"circuitFeverContrast"));
                Assert.AreEqual(.018518519f,Field(profile,"circuitPrimaryWidth"));Assert.AreEqual(.013888889f,Field(profile,"circuitSecondaryWidth"));Assert.AreEqual(1f,Field(profile,"circuitScoreMaskStrength"));
                Assert.AreEqual(.273f,Field(profile,"backgroundSpeed"));Assert.AreEqual(2.5f,Field(profile,"feverBreakStrength"));Assert.AreEqual(.5f,Field(profile,"backgroundScoreY"));Assert.AreEqual(.22f,Field(profile,"backgroundScoreReaction"));
                Assert.AreEqual(.017f,Field(profile,"backgroundScoreOpacity"));
            }finally{UnityEngine.Object.DestroyImmediate(profile);}
        }
        [TestCase(4)] [TestCase(8)] [TestCase(12)]
        public void DiagonalYJunctionsHaveNoRightAngleLegsOrCircularOrnaments(int density)
        {
            var graph=new Graph(1,1920d/1080,density);var sides=(int[])Field(graph.Value,"RootSides");var degree=(int[])Field(graph.Value,"Degree");var incident=(int[][])Field(graph.Value,"Incident");var contacts=(int[])Field(graph.Value,"Contacts");
            CollectionAssert.AreEquivalent(new[]{0,1,2,3},sides.Distinct());Assert.IsTrue(contacts.All(v=>v==0));
            // These two authored style tags select the profile's independently editable primary/secondary widths.
            CollectionAssert.AreEquivalent(new[]{23d,12d},graph.Edges.Cast<object>().Select(e=>(double)Field(e,"WidthPixels")).Distinct());
            for(int n=0;n<degree.Length;n++){
                Assert.AreEqual(degree[n],incident[n].Length);Assert.LessOrEqual(degree[n],3);
                var center=graph.Points.GetValue(n);
                double[] Ray(int index){var e=graph.Edges.GetValue(index);int other=(int)Field(e,"A")==n?(int)Field(e,"B"):(int)Field(e,"A");var point=graph.Points.GetValue(other);return new[]{X(point)-X(center),Y(point)-Y(center)};}
                foreach(int e in incident[n]){var edge=graph.Edges.GetValue(e);Assert.IsTrue((int)Field(edge,"A")==n||(int)Field(edge,"B")==n);}
                for(int a=0;a<incident[n].Length;a++)for(int b=a+1;b<incident[n].Length;b++){
                    var u=Ray(incident[n][a]);var v=Ray(incident[n][b]);double dot=u[0]*v[0]+u[1]*v[1];
                    Assert.Greater(Math.Abs(dot),1e-10,"No perpendicular legs at an elbow or a Y junction.");
                    if(degree[n]==2)Assert.Less(dot,0,"No acute return or doubled-back elbow.");
                }
            }
        }
        [Test] public void StrokeMitersAreFiniteAndBoundedAtStraightAndBentJoins()
        {
            foreach(double angle in new[]{0d,45,90,135,179,180}){
                double radians=angle*Math.PI/180;var result=Runtime("CircuitStrokeMath").GetMethod("Side").Invoke(null,new[]{Point(-1,0),Point(0,0),Point(Math.Cos(radians),Math.Sin(radians)),(object).02});
                double length=(double)Property(result,"Length");Assert.IsFalse(double.IsNaN(length));Assert.LessOrEqual(length,.032000001);Assert.Greater(length,0);
            }
        }
        [Test] public void FeverRetreatWaitsForBuildAndDrainsEveryPathByGaugeZero()
        {
            var graph=new Graph(1,1.8,10);var retreat=Activator.CreateInstance(Runtime("CircuitFeverRetreat"),new[]{graph.Value});Call(retreat,"Capture",1u,.18d,.30d,1.35d,.06d);var lengths=(double[])Field(graph.Value,"RootLengths");
            for(int root=0;root<graph.Roots.Length;root++){
                Assert.AreEqual(lengths[root],Call(retreat,"Front",root,.10d));Call(retreat,"Built",root,.10d);Assert.AreEqual(lengths[root],Call(retreat,"Front",root,.15d));
                double last=lengths[root];for(int frame=0;frame<=100;frame++){double front=(double)Call(retreat,"Front",root,frame/100d);Assert.LessOrEqual(front,last+1e-8);last=front;}Assert.AreEqual(0,last);
            }
        }
        [Test] public void RetreatPlanIsStableAndKeepsConnectedPrefixesAcrossViewportChanges()
        {
            var graph=new Graph(1,1.8,10);var nextGraph=new Graph(1.8,1,10);var type=Runtime("CircuitFeverRetreat");var first=Activator.CreateInstance(type,new[]{graph.Value});var next=Activator.CreateInstance(type,new[]{nextGraph.Value});Call(first,"Capture",2u,.18d,.30d,1.35d,.06d);Call(next,"Capture",2u,.18d,.30d,1.35d,.06d);
            var lengths=(double[])Field(graph.Value,"RootLengths");var nextLengths=(double[])Field(nextGraph.Value,"RootLengths");var ends=new double[graph.Roots.Length];
            for(int root=0;root<ends.Length;root++){Call(first,"Built",root,0d);ends[root]=(double)Call(first,"Finish",root);Call(first,"Front",root,.7d);}Assert.AreEqual(ends.Length,ends.Distinct().Count());Call(next,"CopyCycle",first);
            for(int root=0;root<ends.Length;root++){
                double before=(double)Call(first,"Front",root,.7d),after=(double)Call(next,"Front",root,.7d);Assert.AreEqual(before/lengths[root],after/nextLengths[root],1e-8);
                foreach(var edge in graph.Edges){if((int)Field(edge,"Root")!=root)continue;double depth=(double)Field(edge,"Depth");double visible=(double)Runtime("CircuitPathGraph").GetMethod("VisibleLength").Invoke(null,new[]{edge,(object)before});Assert.IsTrue(visible==0||before>depth);}
            }
        }
        [Test] public void QuietFlowHasAHardCapIdlePeriodsAndPauseAndClear()
        {
            var graph=new Graph(1,1.8,10);var flow=Activator.CreateInstance(Runtime("CircuitAmbientFlow"),new[]{graph.Value});bool sawPulse=false,sawGap=false;
            for(int frame=0;frame<6000;frame++){
                Call(flow,"Advance",.02d,true,.24d,4.5d,2);int active=(int)Property(flow,"Count");Assert.LessOrEqual(active,2);sawPulse|=active>0;if(sawPulse&&active==0)sawGap=true;
            }
            Assert.IsTrue(sawPulse&&sawGap);var before=Call(flow,"Level",1,.5d);Call(flow,"Advance",0d,true,.24d,4.5d,2);Assert.AreEqual(before,Call(flow,"Level",1,.5d));Call(flow,"Clear");Assert.AreEqual(0,Property(flow,"Count"));
        }
    }
}
#endif
