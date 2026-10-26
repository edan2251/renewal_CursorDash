using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    public sealed class CursorDashFeverPolishMathTests
    {
        private static Type Runtime(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static double MathCall(string type,string method,params object[] args)=>(double)Runtime(type).GetMethod(method).Invoke(null,args);
        private static bool Sweep(double x0,double y0,double x1,double y1,double hx,double hy,double r,out double hit){object[] args={x0,y0,x1,y1,hx,hy,r,0d};bool ok=(bool)Runtime("FeverContactMath").GetMethod("Sweep").Invoke(null,args);hit=(double)args[7];return ok;}
        [Test] public void PlayerTurnDefaultIs1800DegreesPerSecondAndStillTurnsAlongMotion()
        {
            var type=Runtime("PlayerCharacterController");float rate=(float)type.GetField("DefaultTurnDegreesPerSecond").GetRawConstantValue();Assert.AreEqual(1800f,rate);
            object[] args={Vector2.right,5.75f,Vector2.up,5.75f,60f,18f,rate,.02f};
            Vector2 displacement=(Vector2)type.GetMethod("AdvanceMotion").Invoke(null,args),direction=(Vector2)args[0];
            Assert.That(Vector2.Angle(Vector2.right,direction),Is.EqualTo(36f).Within(.001));Assert.Greater(displacement.x,0);Assert.Greater(displacement.y,0);
        }
        private static object Plan(double fire=3.15,double morph=2.8,double length=.3125)=>Activator.CreateInstance(Runtime("HexRotationTimeline"),new object[]{.1d,.95d,.6d,.2d,1.95d,1d,.2d,morph,length,fire});
        private static double PlanFill(object plan,double beat){object[] args={beat,.06d,0d};return (double)plan.GetType().GetMethod("Fill").Invoke(plan,args);}
        [Test] public void HexTimelineUserDefaultsQuickStagesAndDecimalLandmarks()
        {
            var plan=Plan();Assert.AreEqual("",plan.GetType().GetProperty("Warning").GetValue(plan));
            Assert.That(PlanFill(plan,0),Is.EqualTo(.1d).Within(1e-8));Assert.That(PlanFill(plan,.95),Is.EqualTo(.1d).Within(1e-8));
            Assert.Greater(PlanFill(plan,1.05),.5);Assert.That(PlanFill(plan,1.15),Is.EqualTo(.6d).Within(1e-8));Assert.That(PlanFill(plan,1.9),Is.EqualTo(.6d).Within(1e-8));
            Assert.That(PlanFill(plan,2.15),Is.EqualTo(1d).Within(1e-8));
            Assert.That((double)plan.GetType().GetMethod("Seconds").Invoke(plan,new object[]{2.8d,.4d,.4d}),Is.EqualTo(2.24d).Within(1e-8));
            Assert.That((double)plan.GetType().GetMethod("Seconds").Invoke(plan,new object[]{3.15d,.4d,.4d}),Is.EqualTo(2.52d).Within(1e-8));
        }
        [Test] public void HexTimelineWholeBeatSpeedScalesMorphAndFireButNotFlight()
        {
            var plan=Plan();foreach(double speed in new[]{.25,.5,1,2,4}){
                double period=.8/speed,morphStart=2.8*period,morphEnd=(2.8+.3125)*period,fire=3.15*period;
                Assert.Less(morphStart,morphEnd);Assert.Less(morphEnd,fire);Assert.That(fire+ .1-fire,Is.EqualTo(.1).Within(1e-8));
                Assert.That((double)plan.GetType().GetMethod("Seconds").Invoke(plan,new object[]{3.15d,.4/speed,.4/speed}),Is.EqualTo(fire).Within(1e-8));
            }
        }
        [Test] public void HexTimelineCrossedEventsAreMonotoneAndIdempotentWithSkippedFrames()
        {
            var plan=Plan(4,3.5,.275);int seen=0,handled=0;var due=plan.GetType().GetMethod("DueEvents");
            foreach(double beat in new[]{0d,.1,1.7,3.8,4.05,4.05,8}){
                int crossed=(int)due.Invoke(plan,new object[]{beat})&~seen;seen|=crossed;
                for(int bit=1;bit<=16;bit<<=1)if((crossed&bit)!=0)handled++;
            }
            Assert.AreEqual(31,seen);Assert.AreEqual(5,handled,"Every threshold is observed once even when several fall in one skipped frame.");
        }
        [Test] public void HexTimelineConflictsWarnAndDoNotMoveTheRequestedFireTime()
        {
            var plan=Plan(3.6,3.5,.2);Assert.IsNotEmpty((string)plan.GetType().GetProperty("Warning").GetValue(plan));
            Assert.AreEqual(3.6,(double)plan.GetType().GetField("FireAt").GetValue(plan));Assert.IsTrue(plan.GetType().GetField("FireAt").IsInitOnly,"Captured nodes cannot be retimed by editing the tool.");
        }
        [Test] public void RotationProgressUsesRealMoveAndRestTimesForDifferentSpeeds()
        {
            foreach(double speed in new[]{.25,.5,1,2,4}){
                double move=.4/speed,pause=.4;
                for(int turn=1;turn<=7;turn++){
                    double end=turn*move+(turn-1)*pause;
                    Assert.That(MathCall("NodeRhythmMath","RotationTurns",end,move,pause),Is.EqualTo(turn).Within(1e-7));
                    Assert.That(MathCall("NodeRhythmMath","RotationTurns",end+pause*.5,move,pause),Is.EqualTo(turn).Within(1e-7));
                }
            }
            Assert.AreEqual(0,MathCall("NodeRhythmMath","RotationTurns",5d,0d,.4d));
        }
        [Test] public void FeverBonusHasOneBirthSettleAndMonotoneVisibleDeparture()
        {
            double Scale(double age,double remaining)=>MathCall("NodeRhythmMath","BonusScale",age,.12d,remaining,.55d);
            Assert.That(Scale(0,2.8),Is.EqualTo(.7).Within(1e-8));Assert.AreEqual(1,Scale(.12,2.68));
            double peak=0;int turns=0;bool falling=false;
            for(int i=0;i<=1000;i++){double value=Scale(.12*i/1000,2.8);Assert.That(value,Is.InRange(.699999999d,1.04d));
                if(value<peak-1e-9&&!falling){falling=true;turns++;}peak=Math.Max(peak,value);
            }
            Assert.Greater(peak,1);Assert.AreEqual(1,turns);
            double last=1;for(int i=0;i<=1000;i++){double value=Scale(2.25+.55*i/1000,.55*(1-i/1000d));Assert.LessOrEqual(value,last+1e-9);last=value;}
            Assert.AreEqual(0,last);Assert.AreEqual(0,MathCall("NodeRhythmMath","Departure",.18d,.18d));Assert.AreEqual(1,MathCall("NodeRhythmMath","Departure",0d,.18d));
        }
        [Test] public void FeverOpticsKeepFullWeightUntilGaugeZeroAndNeverAccumulate()
        {
            Assert.AreEqual(0,MathCall("FeverFlowMath","OpticsWeight",0d,8d));Assert.AreEqual(0,MathCall("FeverFlowMath","OpticsWeight",8d,0d));
            Assert.AreEqual(1,MathCall("FeverFlowMath","OpticsWeight",1d,7d));Assert.AreEqual(0,MathCall("FeverFlowMath","OpticsWeight",double.NaN,7d));
            Assert.AreEqual(1,MathCall("FeverFlowMath","OpticsWeight",7.999d,.001d),"No premature monochrome/optical ending while gauge remains.");
            for(int i=0;i<=1000;i++){double elapsed=8*i/1000d;Assert.That(MathCall("FeverFlowMath","OpticsWeight",elapsed,8-elapsed),Is.InRange(0d,1d));}
        }
        [Test] public void FeverBurstShellKeepsAllEdgesWithoutVectorOverflowOrInternalFading()
        {
            var build=Runtime("FeverBonusBurst").GetMethod("AppendPolygon",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(build);
            foreach(int count in new[]{3,4,12})foreach(int winding in new[]{-1,1})for(int turn=0;turn<16;turn++){
                var shape=new Vector3[count];const float radius=.205f;
                for(int i=0;i<count;i++){float a=turn*.37f+winding*i*Mathf.PI*2/count;shape[i]=new Vector3(.75f+Mathf.Cos(a)*radius,-1+Mathf.Sin(a)*radius);}
                var vertices=new System.Collections.Generic.List<Vector3>();var indices=new System.Collections.Generic.List<int>();
                var owners=new System.Collections.Generic.List<int>();var edges=new System.Collections.Generic.List<Vector4>();
                Assert.DoesNotThrow(()=>build.Invoke(null,new object[]{shape,-2,vertices,indices,owners,edges}));
                Assert.AreEqual(count>4?count*3:count,vertices.Count);Assert.AreEqual(vertices.Count,edges.Count);Assert.AreEqual(vertices.Count,owners.Count);
                Assert.AreEqual(count>4?count*3:(count-2)*3,indices.Count);Assert.IsTrue(owners.All(o=>o==-2));
                double area=0;for(int i=0;i<indices.Count;i+=3){
                    int a=indices[i],b=indices[i+1],c=indices[i+2];Assert.That(a,Is.InRange(0,vertices.Count-1));Assert.That(b,Is.InRange(0,vertices.Count-1));Assert.That(c,Is.InRange(0,vertices.Count-1));
                    Vector3 u=vertices[b]-vertices[a],v=vertices[c]-vertices[a];area+=Math.Abs(u.x*v.y-u.y*v.x)*.5;
                    var middle=(edges[a]+edges[b]+edges[c])/3;Assert.Greater(Mathf.Min(Mathf.Min(middle.x,middle.y),Mathf.Min(middle.z,middle.w)),0,"Triangle interiors must remain visible, including shell fan spokes.");
                }
                Assert.That(area,Is.EqualTo(count*Math.Sin(2*Math.PI/count)*radius*radius*.5).Within(1e-5),"Do not fix overflow by dropping shell edges.");
                if(count>4)for(int i=0;i<count;i++){
                    Assert.Greater(edges[i*3].x,radius*.5f);Assert.That(edges[i*3+1].x,Is.EqualTo(0).Within(1e-5));Assert.That(edges[i*3+2].x,Is.EqualTo(0).Within(1e-5));
                    Assert.AreEqual(1000,edges[i*3].y);Assert.AreEqual(1000,edges[i*3].z);Assert.AreEqual(1000,edges[i*3].w);
                }
            }
        }
        [Test] public void TimedSupplyDoesNotNeedKillsOrMovementAndDoesNotBankBursts()
        {
            var type=Runtime("FeverSupplyClock");var supply=Activator.CreateInstance(type);var advance=type.GetMethod("Advance");
            bool Tick(double dt,int active)=>(bool)advance.Invoke(supply,new object[]{dt,.12d,active,14});
            int spawned=0;for(int i=0;i<8000;i++)if(Tick(.001,0))spawned++;
            Assert.AreEqual(67,spawned);Assert.IsFalse(Tick(0,0));Assert.IsFalse(Tick(double.NaN,0));
            for(int i=0;i<2000;i++)Assert.IsFalse(Tick(.01,14));
            Assert.IsTrue(Tick(.001,13));Assert.IsFalse(Tick(.001,13));
            type.GetMethod("Reset").Invoke(supply,null);Assert.IsTrue(Tick(50,0));Assert.IsFalse(Tick(.001,0));
        }
        [Test] public void IdleSupplyWaitsOnlyAtDensityCapAndResumesAfterVisibleLifetime()
        {
            var type=Runtime("FeverSupplyClock");var supply=Activator.CreateInstance(type);var advance=type.GetMethod("Advance");
            var ends=new System.Collections.Generic.List<double>();int spawned=0,peak=0;
            for(int i=0;i<8000;i++){double clock=i*.001;ends.RemoveAll(t=>t<=clock);
                if((bool)advance.Invoke(supply,new object[]{.001d,.12d,ends.Count,14})){spawned++;ends.Add(clock+2.8);}
                peak=Math.Max(peak,ends.Count);
            }
            Assert.AreEqual(14,peak);Assert.AreEqual(42,spawned,"No kills or movement. Density limits supply; no live target is removed to make room.");
        }
        [Test] public void FragmentChannelsAreRepeatableAndNotOneCorrelatedCurvedSequence()
        {
            double x=0,y=0,xx=0,yy=0,xy=0;int left=0,right=0;
            for(int i=0;i<4000;i++){
                double a=MathCall("FeverScatter","Unit",17,i,2),b=MathCall("FeverScatter","Unit",17,i,10);
                Assert.That(a,Is.InRange(0d,.999999999));Assert.That(b,Is.InRange(0d,.999999999));Assert.AreEqual(a,MathCall("FeverScatter","Unit",17,i,2));
                x+=a;y+=b;xx+=a*a;yy+=b*b;xy+=a*b;if(a<.5)left++;else right++;
            }
            double correlation=(xy/4000-x*y/16000000)/Math.Sqrt((xx/4000-x*x/16000000)*(yy/4000-y*y/16000000));
            Assert.Less(Math.Abs(correlation),.08);Assert.Greater(left,1500);Assert.Greater(right,1500);
        }
        [Test] public void GatherKeepsRingLeadThenReleasesVisualCompressionRapidly()
        {
            var phase=Enum.Parse(Runtime("FeverState").GetNestedType("Phase"),"Entering");
            Assert.Less(MathCall("FeverFlowMath","OuterScale",0,false,.15d),1);Assert.AreEqual(1,MathCall("FeverFlowMath","DotScale",false,.15d));
            Assert.AreEqual(1,MathCall("FeverFlowMath","GatherLight",phase,.58d));Assert.AreEqual(0,MathCall("FeverFlowMath","GatherLight",phase,.67d));
            Assert.That(MathCall("FeverFlowMath","BodyScale",phase,.58d,.115d),Is.EqualTo(.885).Within(1e-8));
            Assert.AreEqual(1,MathCall("FeverFlowMath","BodyScale",phase,1d,.115d));
        }
        [Test] public void FastSweepFindsCrossingWithoutInflatingRoundedCorners()
        {
            Assert.IsFalse(Sweep(-5,0,-5,0,.1,.15,.2,out _));Assert.IsFalse(Sweep(5,0,5,0,.1,.15,.2,out _));
            Assert.IsTrue(Sweep(-5,0,5,0,.1,.15,.2,out double hit));Assert.That(hit,Is.EqualTo(.47).Within(1e-8));
            Assert.IsFalse(Sweep(.29,.29,.29,.29,.1,.1,.2,out _),"Expanded AABB corner alone is not a circular contact.");
            Assert.IsTrue(Sweep(-2,-2,2,2,.1,.1,.2,out hit));Assert.That(hit,Is.EqualTo((2-.1-.2/Math.Sqrt(2))/4).Within(1e-8));
            Assert.IsTrue(Sweep(0,0,0,0,.1,.1,.2,out hit));Assert.AreEqual(0,hit);
        }
        [Test] public void ContactFractionMatchesBoxDistanceForDifferentDirections()
        {
            for(int i=0;i<720;i++){
                double a=i*Math.PI/360,x=2*Math.Cos(a),y=2*Math.Sin(a),offset=(i%7-3)*.055;
                if(!Sweep(x,y,-x+offset,-y-offset,.12,.21,.205,out double hit))continue;
                double px=x+(-2*x+offset)*hit,py=y+(-2*y-offset)*hit;
                double dx=Math.Max(0,Math.Abs(px)-.12),dy=Math.Max(0,Math.Abs(py)-.21);
                Assert.That(Math.Sqrt(dx*dx+dy*dy),Is.EqualTo(.205).Within(1e-7));Assert.That(hit,Is.InRange(0d,1d));
            }
        }
        [Test] public void NodeEntranceHasOneSettleAndColliderScaleNeverExceedsBodyScale()
        {
            double peak=0,ringPeak=0;int bodyFalls=0,ringFalls=0;bool falling=false,ringFalling=false;
            for(int i=0;i<=1000;i++){
                double t=i/1000d,b=MathCall("NodeEntranceMath","Body",t),r=MathCall("NodeEntranceMath","Ring",t),c=MathCall("NodeEntranceMath","Collider",t);
                Assert.That(c,Is.InRange(0d,1d));Assert.LessOrEqual(c,b+1e-9);Assert.LessOrEqual(b,1.05);Assert.LessOrEqual(r,1.07);
                if(b<peak-1e-9&&!falling){falling=true;bodyFalls++;}if(r<ringPeak-1e-9&&!ringFalling){ringFalling=true;ringFalls++;}
                peak=Math.Max(peak,b);ringPeak=Math.Max(ringPeak,r);
            }
            Assert.AreEqual(1,bodyFalls);Assert.AreEqual(1,ringFalls);
            Assert.That(MathCall("NodeEntranceMath","Body",1d),Is.EqualTo(1));Assert.That(MathCall("NodeEntranceMath","Ring",1d),Is.EqualTo(1));Assert.AreEqual(0,MathCall("NodeEntranceMath","Ring",.1d));
        }
        [Test] public void ScoreAndSmallerMultiplierFitPortraitLandscapeAndLongScores()
        {
            foreach(double width in new[]{160d,300d,700d,1400d})foreach(double score in new[]{40d,200d,1200d,9000d}){
                double fit=MathCall("FeverFlowMath","ScoreFit",width,score,70d,18d);Assert.That(fit,Is.InRange(0d,1d));Assert.LessOrEqual((score+70+18)*fit,width+1e-7);
            }
        }
        [Test] public void WhiteFieldCoversAllCornersForDifferentRatiosAndNexusCenters()
        {
            foreach(double aspect in new[]{.35,.5625,.75,1,1.7778,2.8})foreach(double center in new[]{.3,.5,.7}){
                double ax=Math.Max(1,aspect),ay=Math.Max(1,1/aspect),extent=Math.Sqrt(Math.Pow(Math.Max(center,1-center)*ax,2)+Math.Pow(Math.Max(center,1-center)*ay,2));
                for(int x=0;x<=10;x++)for(int y=0;y<=10;y++)Assert.Less(MathCall("FeverFlowMath","SignedField",(x/10d-center)*ax,(y/10d-center)*ay,1d,extent),0);
            }
        }
        [Test] public void VisibleFadingBonusKeepsMatchingCircleUntilExpiry()
        {
            Type type=Runtime("FeverBonusNode");var root=new GameObject("Fever fade test");root.SetActive(false);Mesh mesh=null;Material material=null;
            try{
                root.AddComponent<SpriteRenderer>();var collider=root.AddComponent<CircleCollider2D>();collider.isTrigger=true;var node=root.AddComponent(type);
                type.GetMethod("Awake",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.DeclaredOnly).Invoke(node,null);
                mesh=new Mesh();material=new Material(Shader.Find("Sprites/Default"));type.GetMethod("Bind").Invoke(node,new object[]{null,mesh,material});
                type.GetMethod("Spawn").Invoke(node,new object[]{Vector2.zero,.205f,2.8f,.3f});type.GetMethod("Advance").Invoke(node,new object[]{2.70f});
                Assert.IsTrue(collider.enabled&&root.activeSelf);Assert.AreEqual(1,collider.radius);
                Assert.That((float)type.GetProperty("WorldRadius").GetValue(node),Is.EqualTo(Mathf.Abs(root.transform.lossyScale.x)).Within(1e-6));
                type.GetMethod("Advance").Invoke(node,new object[]{.11f});Assert.IsFalse(root.activeSelf,"Expiry removes both visual and collider, without reward.");
            }finally{Object.DestroyImmediate(root);if(mesh!=null)Object.DestroyImmediate(mesh);if(material!=null)Object.DestroyImmediate(material);}
        }
    }
    // Authored only: inherits the existing preference-protecting Play fixture. Never auto-run by this task.
    public partial class CursorDashSceneRegression
    {
        [UnityTest] public IEnumerator HexUserTimelinePausesCancelsAndFiresAtConfiguredBeatAfterMorph()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var player=Find("PlayerCharacterController");player.GetComponent<Rigidbody2D>().position=new Vector2(-2,-3);player.transform.position=new Vector2(-2,-3);
            var hex=NodesOfType("ArrowNode").First();var artwork=hex.transform.GetChild(0);
            foreach(string name in new[]{"Outside 1","Outside 2"}){var r=artwork.Find(name).GetComponent(TypeFor("Rotator"));Set(r,"duration",.2f);Set(r,"interval",.04f);}
            hex.transform.position=new Vector2(0,2);Call(hex,"Execute");
            float deadline=Time.realtimeSinceStartup+3;
            while((int)Get(hex,"ChargeStepsCompleted")<3){Assert.Less(Time.realtimeSinceStartup,deadline);yield return null;}
            Assert.IsTrue((bool)Get(hex,"ChargeFollowsRotation"));Assert.AreEqual("Preparing",Get(hex,"Phase").ToString());Assert.Less((float)Get(hex,"ChargeRotationTurns"),3);
            Call(stage,"SetPaused",true);float held=(float)Get(hex,"ChargeRotationTurns");yield return WaitRealtime(.15f);Assert.AreEqual(held,Get(hex,"ChargeRotationTurns"));Call(stage,"SetPaused",false);
            yield return WaitCirclePhase(hex,"Warning");Assert.GreaterOrEqual((float)Get(hex,"ChargeRotationTurns"),2f-.00001f);Assert.IsFalse(hex.GetComponent<LineRenderer>().enabled);Call(hex,"ResetObject");yield return WaitGameplay(.4f);
            Assert.IsFalse(hex.GetComponent<LineRenderer>().enabled);Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));
            hex.transform.position=new Vector2(1,2);float began=Time.time;Call(hex,"Execute");Assert.AreEqual(1,Get(hex,"ChargeStepsCompleted"));Assert.AreEqual(0f,Get(hex,"ChargeRotationTurns"));
            yield return WaitCirclePhase(hex,"Fired");Assert.GreaterOrEqual((float)Get(hex,"ChargeRotationTurns"),3-.00001f);
            Assert.That((float)Get(hex,"FiredAt")-began,Is.EqualTo(3.15f*(.2f+.04f)).Within(Mathf.Max(.06f,2*Time.deltaTime)),"User defaults start morph at 2.8 beats and fire at 3.15, without an extra timer.");
            Assert.Greater((float)Get(hex,"FiredAt"),(float)Get(hex,"WarningCompletedAt"));Call(hex,"ResetObject");
        }
        [UnityTest] public IEnumerator FeverFinalOpticsRunAfterCursorAndStopOutsideFeverOrInContactDiagnostics()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");var world=(Component)Get(fever,"World");
            Assert.IsFalse((bool)Get(world,"ShouldRenderOptics"));Call(fever,"Review","fill");Call(fever,"Review","step");Call(fever,"Review","step");yield return WaitGameplay(.25f);yield return null;
            Assert.IsTrue((bool)Get(world,"ShouldRenderOptics"));Assert.Greater((int)Get(world,"OpticsFrames"),0,"The final pass must actually be invoked, not just listed.");
            Set(fever,"ContactDiagnostics",true);yield return null;Assert.IsFalse((bool)Get(world,"ShouldRenderOptics"));
            Set(fever,"ContactDiagnostics",false);Call(fever,"Review","abort");yield return null;Assert.IsFalse((bool)Get(world,"ShouldRenderOptics"));
        }
        [UnityTest] public IEnumerator FeverEntryRetiresArtworkWithoutKeepingThreatsOrPausedMotion()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var creator=(Component)Get(stage,"NodeCreator");
            Call(creator,"SpawnPreviewAt",0,new Vector2(-1,2));Call(creator,"SpawnPreviewAt",1,new Vector2(1,2));yield return WaitGameplay(.25f);
            var fever=(Component)Get(stage,"Fever");Call(fever,"Review","fill");Call(fever,"Review","step");
            Assert.AreEqual(0,Get(creator,"ActiveCount"));Assert.AreEqual(2,Get(creator,"DepartingArtworkCount"));
            var copies=creator.GetComponentsInChildren<Transform>().Where(t=>t.name=="Fever departing node artwork").ToArray();Assert.AreEqual(2,copies.Length);
            foreach(var copy in copies){Assert.AreEqual(0,copy.GetComponentsInChildren<Collider2D>(true).Length);Assert.AreEqual(0,copy.GetComponentsInChildren(TypeFor("Node"),true).Length);}
            Call(stage,"SetPaused",true);var scales=copies.Select(t=>t.localScale).ToArray();yield return WaitRealtime(.15f);
            for(int i=0;i<copies.Length;i++)Assert.AreEqual(scales[i],copies[i].localScale);
            Call(stage,"SetPaused",false);yield return WaitGameplay(.23f);Assert.AreEqual(0,Get(creator,"DepartingArtworkCount"));
            Assert.AreEqual("0",Get(Get(fever,"Model"),"PointsText"));Call(fever,"Review","abort");Assert.AreEqual(0,Get(creator,"ActiveCount"));
        }
        [UnityTest] public IEnumerator FeverSupplyPausesAndHudMultiplierUsesItsOwnFontAtlas()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");
            Call(fever,"Review","fill");Call(fever,"Review","step");Call(fever,"Review","step");yield return WaitGameplay(.36f);
            int supplied=(int)Get(fever,"SpawnedBonuses");Assert.GreaterOrEqual(supplied,3);Assert.LessOrEqual((int)Get(fever,"ActiveBonuses"),14);
            Call(stage,"SetPaused",true);yield return WaitRealtime(.20f);Assert.AreEqual(supplied,Get(fever,"SpawnedBonuses"));Call(stage,"SetPaused",false);
            Call(fever,"Review","step");Call(fever,"Review","step");yield return null;yield return null;
            var label=Resources.FindObjectsOfTypeAll(TypeFor("TMPro.TextMeshProUGUI")).Cast<Component>().Single(t=>t.name=="Score multiplier x2-x4");
            Assert.IsTrue(label.gameObject.activeSelf);Assert.AreEqual("×2",Get(label,"text"));
            var font=Get(label,"font");var material=(Material)Get(label,"fontSharedMaterial");
            Assert.AreSame(Get(font,"atlasTexture"),material.mainTexture,"Glyph UVs must sample that font's atlas, not another score font material.");
            Call(fever,"Review","hit");yield return null;Assert.IsFalse(label.gameObject.activeSelf);
        }
        [UnityTest] public IEnumerator FeverBodySweepAndTriggerCannotAwardOrBreakTwice()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");var model=Get(fever,"Model");
            Call(fever,"Review","fill");Call(fever,"Review","step");Call(fever,"Review","step");
            var nodes=Resources.FindObjectsOfTypeAll(TypeFor("FeverBonusNode")).Cast<Component>().Where(n=>n.gameObject.scene.IsValid()).ToArray();Assert.Greater(nodes.Length,0);
            var node=nodes[0];foreach(var other in nodes)Call(other,"ResetObject");
            Assert.AreEqual(HideFlags.None,node.gameObject.hideFlags);Assert.AreEqual(HideFlags.None,node.transform.parent.gameObject.hideFlags,"The physics roots stay in the real scene.");
            var player=Find("PlayerCharacterController");var body=player.GetComponent<Rigidbody2D>();body.position=new Vector2(-1,-2);player.transform.position=body.position;player.transform.rotation=Quaternion.identity;
            Call(node,"Spawn",new Vector2(0,-2),.205f,2.8f,.3f);Call(fever,"SweepBonuses",player,new Vector2(-1,-2),new Vector2(1,-2));
            string points=Get(model,"PointsText").ToString();Assert.AreEqual("1",points);Assert.IsFalse(node.gameObject.activeSelf);Assert.AreEqual(1,Get(fever,"BonusCollections"));
            var bursts=((IEnumerable)Get(fever,"BonusBursts")).Cast<Component>().ToArray();Assert.AreEqual(1,bursts.Length);
            Assert.IsNotNull(Get(bursts[0],"View"),"White-world mask reads the owned DontSave burst directly.");
            Call(fever,"SweepBonuses",player,new Vector2(1,-2),new Vector2(-1,-2));Assert.IsFalse((bool)Call(node,"TryCollect",Vector2.zero,Vector2.right,false));
            Assert.AreEqual(points,Get(model,"PointsText"));Assert.AreEqual(1,Get(fever,"BonusCollections"));Assert.AreEqual(1,Get(fever,"BonusSweepCollections"));
        }
        [UnityTest] public IEnumerator FeverExitCopiesRealPositionsWithoutCollidersOrRewardsAndCancelsCleanly()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");var model=Get(fever,"Model");
            Call(fever,"Review","fill");Call(fever,"Review","step");Call(fever,"Review","step");
            var node=Resources.FindObjectsOfTypeAll(TypeFor("FeverBonusNode")).Cast<Component>().First(n=>n.gameObject.scene.IsValid());
            Call(stage,"SetPaused",true);Call(node,"Spawn",new Vector2(1.4f,2.1f),.205f,2.8f,0f);
            Vector3 origin=((Renderer)Get(node,"RecallSource")).transform.position;string points=Get(model,"PointsText").ToString();
            Call(fever,"Review","step");Assert.AreEqual("Exiting",Get(model,"Current").ToString());Assert.AreEqual(0d,Get(model,"Gauge"));Assert.AreEqual(0,Get(fever,"ActiveBonuses"));
            var copies=((IEnumerable)Get(fever,"RecallViews")).Cast<MeshRenderer>().ToArray();Assert.Greater(copies.Length,0);
            Assert.IsTrue(copies.Any(c=>Vector3.Distance(c.transform.position,origin)<.0001f));
            foreach(var copy in copies){Assert.AreEqual(0,copy.GetComponents<Collider2D>().Length);Assert.AreEqual(0,copy.GetComponents(TypeFor("Node")).Length);}
            Assert.IsFalse((bool)Call(node,"TryCollect",(Vector2)origin,Vector2.right,false));Assert.AreEqual(points,Get(model,"PointsText"));
            var held=copies.Select(c=>c.transform.position).ToArray();yield return WaitRealtime(.12f);
            for(int i=0;i<copies.Length;i++)Assert.AreEqual(held[i],copies[i].transform.position);
            Call(fever,"Review","abort");Assert.AreEqual(0,Get(fever,"RecallingBonuses"));Assert.AreEqual("Normal",Get(model,"Current").ToString());
            Assert.AreEqual(0,Get(model,"CompletedFevers"));Assert.AreEqual(points,Get(model,"PointsText"));
            foreach(var copy in copies)Assert.IsTrue(copy==null||!copy.enabled,"Cancelled copies must stop rendering before deferred destruction.");
            Call(stage,"SetPaused",false);
        }
        [UnityTest] public IEnumerator ProjectedScoreUsesTheRealTotalAtlasAndWorldLayerWithTopTotalHidden()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var fever=(Component)Get(stage,"Fever");Call(fever,"Review","kill");
            Assert.AreEqual(Get(Get(fever,"Model"),"PointsText"),Get(fever,"BackgroundScoreText"));Assert.IsTrue((bool)Get(fever,"BackgroundScoreAtlasMatches"));
            var top=(Component)Get(stage,"timeScoreLabel");object font=Get(top,"font"),hudMaterial=Get(top,"fontSharedMaterial");
            var renderer=(MeshRenderer)Get(fever,"BackgroundScoreRenderer");Assert.IsNotNull(renderer);Assert.AreEqual(-900,renderer.sortingOrder);Assert.IsNull(renderer.GetComponent<Canvas>());
            Set(fever,"BackgroundScoreVisible",false);yield return null;Assert.IsFalse(renderer.enabled);Set(fever,"BackgroundScoreVisible",true);yield return null;Assert.IsTrue(renderer.enabled);Assert.IsTrue(top.gameObject.activeInHierarchy);Assert.IsFalse(((Behaviour)top).enabled,"The old top total is hidden while its source/font is preserved.");Assert.AreSame(font,Get(top,"font"));Assert.AreSame(hudMaterial,Get(top,"fontSharedMaterial"));Assert.AreEqual(Get(Get(fever,"Model"),"PointsText"),Get(top,"text"));
            Call(stage,"SetPaused",true);Call(fever,"Review","kill");double held=(double)Get(fever,"BackgroundScorePulse");yield return WaitRealtime(.12f);Assert.AreEqual(held,Get(fever,"BackgroundScorePulse"));
            Call(fever,"Abort");Assert.AreEqual(0d,Get(fever,"BackgroundScorePulse"));Assert.IsFalse(renderer.enabled);Call(stage,"SetPaused",false);
        }
        [UnityTest] public IEnumerator OrdinaryNodeEntranceReusesPoolWithoutRestartingAttackClock()
        {
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var creator=(Component)Get(stage,"NodeCreator");
            var node=(Component)Call(creator,"SpawnPreviewAt",0,new Vector2(-1,2));var collider=node.GetComponent<CircleCollider2D>();
            Assert.IsFalse(collider.enabled);yield return WaitGameplay(.25f);Assert.IsTrue(collider.enabled);Assert.AreEqual("Preparing",Get(node,"Phase").ToString());
            float full=collider.radius;Call(node,"ResetObject");var reused=(Component)Call(creator,"SpawnPreviewAt",0,new Vector2(1,2));
            Assert.AreSame(node,reused);Assert.IsFalse(collider.enabled);yield return WaitGameplay(.25f);Assert.That(collider.radius,Is.EqualTo(full).Within(1e-6));
        }
    }
}
