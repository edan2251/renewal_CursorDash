using System;
using System.Collections.Generic;

/// <summary>Authored 1080x1920 diagonal circuit family. No Unity/physics/reward state.</summary>
public sealed class CircuitPathGraph
{
    public struct Point
    {
        public double X,Y;
        public Point(double x,double y){X=x;Y=y;}
        public double Length=>Math.Sqrt(X*X+Y*Y);
        public static Point operator +(Point a,Point b)=>new Point(a.X+b.X,a.Y+b.Y);
        public static Point operator -(Point a,Point b)=>new Point(a.X-b.X,a.Y-b.Y);
        public static Point operator *(Point a,double b)=>new Point(a.X*b,a.Y*b);
    }
    public struct Edge {public int A,B,Root;public double Length,Depth,WidthPixels;public bool Thin;}
    public readonly Point[] Points;
    public readonly Edge[] Edges;
    public readonly int[] Roots,Degree,Contacts,RootSides;
    public readonly int[][] Incident;
    public readonly string[] Names;
    public readonly double[] Depths,RootLengths;
    public readonly double MaxDepth;
    public CircuitPathGraph(double aspectX,double aspectY,int density=12)
    {
        aspectX=Math.Max(1,aspectX);aspectY=Math.Max(1,aspectY);density=Math.Max(4,Math.Min(12,density));
        var p=new List<Point>();var e=new List<Edge>();var roots=new List<int>();var depths=new List<double>();var contacts=new List<int>();var names=new List<string>();var sides=new List<int>();double maximum=0;int current=-1;
        Point Px(double x,double y)=>new Point(x,y);
        Point Map(Point px)=>new Point((px.X/1080-.5)*aspectX,(.5-px.Y/1920)*aspectY);
        int AddPoint(Point v,double depth){p.Add(v);depths.Add(depth);contacts.Add(0);maximum=Math.Max(maximum,depth);return p.Count-1;}
        int Root(string name,int side,double x,double y){current=roots.Count;int n=AddPoint(Map(Px(x,y)),0);roots.Add(n);names.Add(name);sides.Add(side);return n;}
        int Trail(int from,double width,params Point[] points){foreach(var px in points){Point to=Map(px);double length=(to-p[from]).Length;if(length<.000001)continue;int b=AddPoint(to,depths[from]+length);e.Add(new Edge{A=from,B=b,Root=current,Depth=depths[from],Length=length,WidthPixels=width,Thin=width<=18});from=b;}return from;}
        // A consistent diagonal family: uninterrupted trunks, 45-degree changes of direction and Y branches.
        // Every turn/junction is authored without perpendicular legs; no target rings or decorative pads.
        // Long parallel companions and staggered inward reaches add density without a grid or new ornaments.
        {
            int a=Root("Upper left / long primary with inward Y",0,232,-32);
            int j=Trail(a,23,Px(232,160),Px(196,196),Px(196,488));
            Trail(j,23,Px(196,592),Px(132,656),Px(132,780),Px(76,836),Px(76,1100));
            Trail(j,12,Px(300,592),Px(300,740),Px(344,784),Px(344,844));
        }
        {
            int a=Root("Left lower / rising primary and short Y",3,-32,1416);
            int j=Trail(a,23,Px(116,1268),Px(116,1168));
            Trail(j,23,Px(116,1056),Px(172,1000),Px(172,904));
            Trail(j,12,Px(244,1040),Px(244,948));
        }
        {
            int a=Root("Lower right / inward primary and outer Y",2,836,1952);
            int j=Trail(a,23,Px(836,1792),Px(780,1736),Px(780,1480));
            Trail(j,23,Px(780,1344),Px(732,1296),Px(732,1132));
            Trail(j,12,Px(908,1352),Px(908,1216),Px(956,1168),Px(956,1048));
        }
        {
            int a=Root("Right middle / descending primary with inward Y",1,1112,424);
            int j=Trail(a,23,Px(1000,536),Px(1000,716));
            Trail(j,23,Px(1000,904),Px(1048,952),Px(1048,1108));
            Trail(j,12,Px(872,844),Px(872,940),Px(816,996),Px(816,1064));
        }
        if(density>=5){
            int a=Root("Upper right / quiet inner companion",0,708,-32);
            Trail(a,12,Px(708,196),Px(748,236),Px(748,420),Px(804,476),Px(804,740),Px(760,784),Px(760,872));
        }
        if(density>=6){
            int a=Root("Lower left / quiet inner companion",2,324,1952);
            Trail(a,12,Px(324,1764),Px(276,1716),Px(276,1508),Px(332,1452),Px(332,1200),Px(288,1156),Px(288,1100));
        }
        if(density>=7){
            int a=Root("Upper left / extended outer companion",0,108,-32);
            Trail(a,12,Px(108,240),Px(148,280),Px(148,520),Px(84,584),Px(84,760));
        }
        if(density>=8){
            int a=Root("Lower right / outer diagonal companion",2,1024,1952);
            Trail(a,12,Px(1024,1728),Px(972,1676),Px(972,1480),Px(1012,1440),Px(1012,1160));
        }
        if(density>=9){
            int a=Root("Upper inner left / long parallel feeder",0,400,-32);
            Trail(a,12,Px(400,300),Px(360,340),Px(360,560),Px(392,592),Px(392,660));
        }
        if(density>=10){
            int a=Root("Lower left / long parallel feeder",2,164,1952);
            Trail(a,12,Px(164,1716),Px(208,1672),Px(208,1472),Px(164,1428),Px(164,1352));
        }
        if(density>=11){
            int a=Root("Upper right / long parallel feeder",0,1028,-32);
            Trail(a,12,Px(1028,220),Px(952,296),Px(952,472),Px(908,516),Px(908,644));
        }
        if(density>=12){
            int a=Root("Lower inner right / long parallel feeder",2,616,1952);
            Trail(a,12,Px(616,1744),Px(660,1700),Px(660,1496),Px(616,1452),Px(616,1328));
        }
        Points=p.ToArray();Edges=e.ToArray();Roots=roots.ToArray();Depths=depths.ToArray();Contacts=contacts.ToArray();Names=names.ToArray();RootSides=sides.ToArray();MaxDepth=maximum;
        Degree=new int[Points.Length];RootLengths=new double[Roots.Length];foreach(var edge in Edges){Degree[edge.A]++;Degree[edge.B]++;RootLengths[edge.Root]=Math.Max(RootLengths[edge.Root],Depths[edge.B]);}
        Incident=new int[Points.Length][];for(int i=0;i<Points.Length;i++)Incident[i]=new int[Degree[i]];var used=new int[Points.Length];
        for(int i=0;i<Edges.Length;i++){var edge=Edges[i];Incident[edge.A][used[edge.A]++]=i;Incident[edge.B][used[edge.B]++]=i;}
    }
    public static double Clamp(double x)=>Math.Max(0,Math.Min(1,x));
    public static double Smooth(double x){x=Clamp(x);return x*x*(3-2*x);}
    public double Nearest(Point p,out int edgeIndex,out double fraction)
    {
        double best=double.PositiveInfinity;edgeIndex=-1;fraction=0;
        for(int n=0;n<Edges.Length;n++){
            var edge=Edges[n];Point a=Points[edge.A],d=Points[edge.B]-a;double t=Clamp(((p.X-a.X)*d.X+(p.Y-a.Y)*d.Y)/(edge.Length*edge.Length));
            double distance=(p-(a+d*t)).Length;if(distance<best){best=distance;edgeIndex=n;fraction=t;}
        }return best;
    }
    public static double VisibleLength(Edge edge,double front)=>Math.Max(0,Math.Min(edge.Length,front-edge.Depth));
    public static Point Recall(Point point,Point center,double travel,double twist,double bend)
    {
        Point d=point-center;travel=Clamp(travel);double angle=-twist*travel*(.35+.65*Math.Min(1,d.Length))*Clamp(bend),c=Math.Cos(angle),s=Math.Sin(angle);
        return center+new Point(d.X*c-d.Y*s,d.X*s+d.Y*c)*(1-travel);
    }
    // A connected tip retreats from the deepest ends toward the outside root. It never fades the whole route.
    public double DeathFront(double age,double speed,double stagger,int root)
    {
        double delay=Math.Min(.16,Math.Max(0,stagger)*((root*5)%Roots.Length));
        double length=RootLengths[root],seconds=Math.Min(.44,length/Math.Max(.25,speed));
        return length*(1-Smooth((age-delay)/seconds));
    }
}

/// <summary>Shared bounded miter arithmetic for the two stroke meshes. No blur or unbounded corner spikes.</summary>
public static class CircuitStrokeMath
{
    public static CircuitPathGraph.Point Side(CircuitPathGraph.Point before,CircuitPathGraph.Point at,CircuitPathGraph.Point after,double halfWidth)
    {
        var a=at-before;var b=after-at;double la=a.Length,lb=b.Length;
        if(la<1e-9)a=b;else a=a*(1/la);if(lb<1e-9)b=a;else b=b*(1/lb);
        var n=new CircuitPathGraph.Point(-b.Y,b.X);var m=new CircuitPathGraph.Point(-a.Y-b.Y,a.X+b.X);double lm=m.Length;
        if(lm<1e-9)return n*halfWidth;m=m*(1/lm);double dot=m.X*n.X+m.Y*n.Y;
        return dot<=.01?n*halfWidth:m*Math.Min(halfWidth*1.6,halfWidth/dot);
    }
}

/// <summary>Fever-gauge-driven retreat of connected prefixes. One captured plan per fever, never wall time.</summary>
public sealed class CircuitFeverRetreat
{
    private readonly CircuitPathGraph graph;
    private readonly double[] starts,finishes,powers,built,last;
    private double hold;
    public CircuitFeverRetreat(CircuitPathGraph graph){this.graph=graph;int n=graph.Roots.Length;starts=new double[n];finishes=new double[n];powers=new double[n];built=new double[n];last=new double[n];}
    private static double Hash(uint x){x^=x>>16;x*=0x7feb352d;x^=x>>15;x*=0x846ca68b;x^=x>>16;return x/(double)uint.MaxValue;}
    public void Capture(uint cycle,double start=.18,double spread=.30,double power=1.35,double readHold=.06)
    {
        start=Math.Max(.08,Math.Min(.4,start));spread=Math.Max(0,Math.Min(.45,spread));power=Math.Max(.55,Math.Min(2.4,power));hold=Math.Max(.02,Math.Min(.15,readHold));
        for(int root=0;root<starts.Length;root++){
            uint seed=(uint)(root+1)*0x9e3779b9u+cycle*0x85ebca6bu;
            int order=0;double key=Hash(seed^0xa511e9b3u);for(int other=0;other<starts.Length;other++){uint otherSeed=(uint)(other+1)*0x9e3779b9u+cycle*0x85ebca6bu;double otherKey=Hash(otherSeed^0xa511e9b3u);if(otherKey<key||otherKey==key&&other<root)order++;}double rank=order/Math.Max(1d,starts.Length-1);
            finishes[root]=.58+.42*rank;starts[root]=Math.Min(finishes[root]-.15,start+spread*Hash(seed));
            powers[root]=power*(.70+.65*Hash(seed^0x68bc21ebu));built[root]=-1;last[root]=graph.RootLengths[root];
        }
    }
    public void CopyCycle(CircuitFeverRetreat previous)
    {
        for(int n=0;n<Math.Min(last.Length,previous.last.Length);n++){starts[n]=previous.starts[n];finishes[n]=previous.finishes[n];powers[n]=previous.powers[n];built[n]=previous.built[n];last[n]=graph.RootLengths[n]*(previous.last[n]/Math.Max(.000001,previous.graph.RootLengths[n]));}
        hold=previous.hold;
    }
    public void Built(int root,double progress){if(built[root]<0)built[root]=CircuitPathGraph.Clamp(progress);}
    public double Start(int root)=>starts[root];
    public double Finish(int root)=>finishes[root];
    public double Front(int root,double progress)
    {
        progress=CircuitPathGraph.Clamp(progress);if(progress>=1)return last[root]=0;
        if(built[root]<0)return last[root];
        double start=Math.Min(.999,Math.Max(starts[root],built[root]+hold)),finish=Math.Min(1,Math.Max(finishes[root],start+.12));
        double u=CircuitPathGraph.Clamp((progress-start)/Math.Max(.001,finish-start));
        return last[root]=Math.Min(last[root],graph.RootLengths[root]*(1-Math.Pow(u,powers[root])));
    }
}

/// <summary>Quiet, staggered graph pulses. Own clock uses the same paused game delta; never calls game RNG.</summary>
public sealed class CircuitAmbientFlow
{
    private readonly CircuitPathGraph graph;private readonly CircuitPulseField pulses;private uint random=0x91e10da5;private double wait=.65;private int previous=-1,launched;private bool resting;
    private double speed=.24,tail=.20,head=.025;public int Count=>pulses.Count;
    public CircuitAmbientFlow(CircuitPathGraph graph){this.graph=graph;pulses=new CircuitPulseField(graph);}
    private double Next(){random^=random<<13;random^=random>>17;random^=random<<5;return random/(double)uint.MaxValue;}
    public void Clear(){pulses.Clear();wait=.65;previous=-1;launched=0;resting=false;}
    public void Advance(double delta,bool running,double velocity,double interval,int limit)
        =>AdvanceShaped(delta,running,velocity,interval,limit,.20,.025);
    public void AdvanceShaped(double delta,bool running,double velocity,double interval,int limit,double tailLength,double headWidth)
    {
        limit=Math.Max(0,Math.Min(3,limit));if(!running||limit==0){Clear();return;}speed=Math.Max(.08,Math.Min(.6,velocity));
        tail=Math.Max(.06,Math.Min(.35,tailLength));head=Math.Max(.008,Math.Min(.06,headWidth));
        pulses.AdvanceFlow(Math.Max(0,delta),speed,tail,limit);if(delta<=0)return;
        if(resting){if(pulses.Count>0)return;resting=false;launched=0;wait=Math.Max(1.5,interval)*(.25+Next()*.40);return;}
        wait-=delta;if(wait>0||pulses.Count>=limit)return;
        int root=(int)(Next()*graph.Roots.Length)%graph.Roots.Length;if(root==previous)root=(root+1)%graph.Roots.Length;previous=root;
        int source=-1;for(int n=0;n<graph.Edges.Length;n++)if(graph.Edges[n].Root==root){source=n;break;}
        if(source>=0){var edge=graph.Edges[source];var point=graph.Points[edge.A]+(graph.Points[edge.B]-graph.Points[edge.A])*.35;pulses.HitFlow(point,.00001,limit,.80+Next()*.40);}
        wait=Math.Max(1.5,interval)*(.75+Next()*.60);
        if(++launched>=limit)resting=true; // Let the small independent group finish; leave genuinely quiet time.
    }
    public double Level(int edge,double t)=>pulses.FlowLevel(edge,t,speed,tail,head);
}

/// <summary>Fixed pulse slots and reusable graph-distance scratch space. Disconnected circuits never receive a hit.</summary>
public sealed class CircuitPulseField
{
    // A spatial HP-loss wave, distinct from graph-connected success pulses. Never creates graph edges.
    public static double RadialLevel(double distance,double age,double speed,double width,double seconds)
    {
        if(distance<0||age<0||age>=seconds||double.IsNaN(distance)||double.IsInfinity(distance)||double.IsNaN(age)||double.IsInfinity(age))return 0;
        speed=Math.Max(.1,speed);width=Math.Max(.03,width);seconds=Math.Max(.05,seconds);
        double behind=age*speed-distance;if(behind<0||behind>width)return 0;
        double front=CircuitPathGraph.Smooth(behind/(width*.12)),tail=1-CircuitPathGraph.Smooth(behind/width);
        return Math.Max(0,Math.Min(1,front*tail*Math.Pow(1-age/seconds,.65)));
    }
    public static double DamagePriority(double level)=>CircuitPathGraph.Smooth(level/.08);
    private sealed class Pulse {public readonly double[] Distance;public readonly int[] Branches;public bool Active,Radial;public CircuitPathGraph.Point Origin;public double Age,T,Rate=1,Reach,Life;public int Edge;public Pulse(int nodes){Distance=new double[nodes];Branches=new int[nodes];}}
    private readonly CircuitPathGraph graph;private readonly Pulse[] pulses=new Pulse[8];private readonly bool[] visited;
    public int Count {get{int n=0;foreach(var p in pulses)if(p.Active)n++;return n;}}
    public CircuitPulseField(CircuitPathGraph graph){this.graph=graph;visited=new bool[graph.Points.Length];for(int i=0;i<pulses.Length;i++)pulses[i]=new Pulse(graph.Points.Length);}
    public bool Hit(CircuitPathGraph.Point point,double radius,int cap)
        =>Seed(point,radius,cap,1);
    public bool HitFlow(CircuitPathGraph.Point point,double radius,int cap,double rate)=>Seed(point,radius,cap,rate);
    private Pulse Acquire(int cap)
    {
        cap=Math.Max(1,Math.Min(8,cap));int slot=0;double oldest=-1;
        for(int i=0;i<cap;i++){if(!pulses[i].Active){slot=i;break;}if(pulses[i].Age>oldest){oldest=pulses[i].Age;slot=i;}}
        for(int i=cap;i<pulses.Length;i++)pulses[i].Active=false;return pulses[slot];
    }
    public bool HitRadial(CircuitPathGraph.Point point,int cap,double speed,double width)
    {
        if(double.IsNaN(point.X)||double.IsNaN(point.Y)||double.IsInfinity(point.X)||double.IsInfinity(point.Y)||double.IsNaN(speed)||double.IsInfinity(speed)||double.IsNaN(width)||double.IsInfinity(width))return false;
        var p=Acquire(cap);p.Active=p.Radial=true;p.Age=0;p.Origin=point;p.Rate=Math.Max(.1,speed);p.T=Math.Max(.03,width);p.Reach=0;
        foreach(var node in graph.Points)p.Reach=Math.Max(p.Reach,(node-point).Length);
        p.Life=(p.Reach+p.T)/p.Rate+.01;return true;
    }
    private bool Seed(CircuitPathGraph.Point point,double radius,int cap,double rate)
    {
        double distance=graph.Nearest(point,out int edge,out double t);if(distance>Math.Max(0,radius)||edge<0)return false;
        var p=Acquire(cap);p.Active=true;p.Radial=false;p.Age=0;p.Edge=edge;p.T=t;p.Rate=rate;p.Reach=0;Array.Clear(visited,0,visited.Length);
        for(int i=0;i<p.Distance.Length;i++){p.Distance[i]=double.PositiveInfinity;p.Branches[i]=0;}
        var seed=graph.Edges[edge];p.Distance[seed.A]=seed.Length*t;p.Distance[seed.B]=seed.Length*(1-t);
        for(int step=0;step<graph.Points.Length;step++){
            int node=-1;double best=double.PositiveInfinity;
            for(int n=0;n<visited.Length;n++)if(!visited[n]&&p.Distance[n]<best){best=p.Distance[n];node=n;}
            if(node<0)break;visited[node]=true;
            foreach(var link in graph.Edges){int next=link.A==node?link.B:link.B==node?link.A:-1;if(next<0)continue;
                double candidate=best+link.Length;if(candidate<p.Distance[next]){p.Distance[next]=candidate;p.Branches[next]=p.Branches[node]+(graph.Degree[node]>2?1:0);}}
        }
        foreach(double value in p.Distance)if(!double.IsInfinity(value))p.Reach=Math.Max(p.Reach,value);
        return true;
    }
    public void Advance(double delta,double speed,double seconds,double decay,int cap)
    {
        cap=Math.Max(1,Math.Min(8,cap));double lifetime=Math.Max(.05,seconds)+Math.Max(.05,decay)*3/Math.Max(.1,speed);
        for(int i=0;i<pulses.Length;i++){var p=pulses[i];if(i>=cap){p.Active=false;continue;}if(!p.Active)continue;p.Age+=Math.Max(0,delta);if(p.Age>=(p.Radial?p.Life:lifetime))p.Active=false;}
    }
    public double Level(int edgeIndex,double t,double speed,double seconds,double decay)
    {
        double level=0;var edge=graph.Edges[edgeIndex];speed=Math.Max(.1,speed);seconds=Math.Max(.05,seconds);decay=Math.Max(.05,decay);
        foreach(var pulse in pulses){if(!pulse.Active)continue;
            if(pulse.Radial){
                var point=graph.Points[edge.A]+(graph.Points[edge.B]-graph.Points[edge.A])*t;
                // A spatial front reaches independent routes too; the geometry/mask still clips hidden sections.
                level=Math.Max(level,RadialLevel((point-pulse.Origin).Length,pulse.Age,pulse.Rate,pulse.T,pulse.Life*3));continue;
            }
            double a=pulse.Distance[edge.A]+edge.Length*t,b=pulse.Distance[edge.B]+edge.Length*(1-t);int branches=a<=b?pulse.Branches[edge.A]:pulse.Branches[edge.B];double distance=Math.Min(a,b);
            if(pulse.Edge==edgeIndex){distance=Math.Min(distance,Math.Abs(t-pulse.T)*edge.Length);branches=0;}
            double age=pulse.Age-distance/speed;if(age<0||age>=seconds||distance>decay*3)continue;
            double u=age/seconds,envelope=CircuitPathGraph.Smooth(u/.12)*Math.Pow(1-u,1.6);
            level=Math.Max(level,envelope*Math.Exp(-distance/decay)*Math.Pow(.68,branches));
        }return ClampLevel(level);
    }
    public void AdvanceFlow(double delta,double speed,double tail,int cap)
    {
        cap=Math.Max(0,Math.Min(3,cap));
        for(int i=0;i<pulses.Length;i++){var p=pulses[i];if(i>=cap){p.Active=false;continue;}if(!p.Active)continue;p.Age+=Math.Max(0,delta);if(p.Age*Math.Max(.08,speed)*p.Rate>p.Reach+tail)p.Active=false;}
    }
    public double FlowLevel(int index,double t,double speed,double tail,double head)
    {
        double level=0;var edge=graph.Edges[index];
        foreach(var p in pulses){if(!p.Active)continue;
            double a=p.Distance[edge.A]+edge.Length*t,b=p.Distance[edge.B]+edge.Length*(1-t),distance=Math.Min(a,b);int branches=a<=b?p.Branches[edge.A]:p.Branches[edge.B];
            if(p.Edge==index){distance=Math.Min(distance,Math.Abs(t-p.T)*edge.Length);branches=0;}
            if(double.IsInfinity(distance))continue;
            double behind=p.Age*speed*p.Rate-distance;if(behind < -head||behind>tail)continue;
            double shape=behind<0?CircuitPathGraph.Smooth(1+behind/head):Math.Exp(-3*behind/tail)*(1-CircuitPathGraph.Smooth((behind/tail-.70)/.30));
            level=Math.Max(level,shape*Math.Exp(-.38*distance/Math.Max(.1,p.Reach))*Math.Pow(.70,branches));
        }return ClampLevel(level);
    }
    private static double ClampLevel(double x)=>Math.Max(0,Math.Min(1,x));
    public void Clear(){foreach(var pulse in pulses)pulse.Active=false;}
}
