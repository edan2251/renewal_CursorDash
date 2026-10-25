using System;

/// <summary>Presentation-only curves and grouping gates. Kept independent of Unity for numerical checks.</summary>
public static class FeverFlowMath
{
    public static double Clamp(double t)=>Math.Max(0,Math.Min(1,t));
    public static double Smooth(double t){t=Clamp(t);return t*t*(3-2*t);}
    public static double Out(double t){t=Clamp(t);return t>=1?1:1-Math.Pow(2,-9*t);}
    public static double OpticsWeight(double elapsed,double remaining)
    {
        if(double.IsNaN(elapsed)||double.IsNaN(remaining))return 0;
        return Math.Min(Smooth(elapsed/.12),Smooth(remaining/.18));
    }
    public static double EntrySpread(double phase)=>Out((phase-.66)/.34);
    public static double ExitRemaining(double phase)=>1-Smooth(phase/.72);
    public static double DotScale(bool exit,double phase)
    {
        if(!exit)return 1-Smooth((phase-.20)/.42);
        double t=Clamp((phase-.56)/.44);return Smooth(t)+Math.Sin(t*Math.PI)*.025;
    }
    private static double Merge(double a,double b,double k){double h=Clamp(.5+.5*(b-a)/k);return b+(a-b)*h-k*h*(1-h);}
    public static double OuterScale(int segment,bool exit,double phase)
    {
        double t=exit?Clamp((phase-.58-segment*.014)/.36):Clamp((phase-segment*.018)/.50);
        return exit?Smooth(t):1-Smooth(t);
    }
    public static double SegmentFill(double gauge,int segment)=>Clamp(gauge*5-segment);
    public static double ScoreFit(double available,double score,double multiplier,double gap)=>Math.Min(1,Math.Max(1,available)/Math.Max(1,score+multiplier+gap));
    public static double BodyScale(FeverState.Phase phase,double progress,double compression=.115)=>1-Math.Max(0,Math.Min(.16,compression))*GatherLight(phase,progress);
    // Build pressure while rings arrive, hold it briefly, then release in .06 phase (63 ms at 1.05 s).
    public static double GatherEnvelope(double progress)=>Smooth((progress-.10)/.42)*(1-Smooth((progress-.61)/.06));
    public static double GatherLight(FeverState.Phase phase,double progress)=>phase==FeverState.Phase.Entering?GatherEnvelope(progress):0;
    // Overlapping broad rounded fronts, without narrow capsules/arms. Mirrored in FeverFlow.cginc.
    public static double SignedField(double x,double y,double progress,double extent)
    {
        progress=Clamp(progress);extent=Math.Max(.001,extent);
        double d=Math.Sqrt(x*x+y*y)-extent*(.015+1.12*Math.Pow(progress,2.2));
        for(int i=0;i<5;i++){
            double a=.38+i*Math.PI*2/5,s=Math.Sin(a),c=Math.Cos(a),speed=.90+.18*Math.Sin(i*1.7+.6);
            double travel=extent*.49*Smooth(progress),bend=extent*.11*Math.Sin(progress*Math.PI+i*1.1)*Smooth(progress);
            double dx=x-(c*travel-s*bend),dy=y-(s*travel+c*bend);
            double along=(dx*c+dy*s)*.90,across=(-dx*s+dy*c)*1.06;
            double radius=extent*(.02+.64*Math.Pow(Clamp(progress*speed),.78));
            d=Merge(d,Math.Sqrt(along*along+across*across)-radius,extent*.10);
        }
        return d;
    }
}
public sealed class FeverFeedbackGate
{
    private double nextStop,nextScreen;
    public void Reset(){nextStop=nextScreen=0;}
    public bool Stop(double clock,double gap){if(clock<nextStop)return false;nextStop=clock+Math.Max(.08,gap);return true;}
    public bool Screen(double clock,double gap){if(clock<nextScreen)return false;nextScreen=clock+Math.Max(.06,gap);return true;}
}
public sealed class FeverRingSpin
{
    public double Boost {get;private set;}
    public double Speed {get;private set;}=24;
    private double decay=.22;
    public void Pulse(bool damage){Boost=Math.Min(950,Math.Max(Boost,damage?780:420));decay=damage ? .30 : .20;}
    public void Begin(){Boost=Math.Max(Boost,800);decay=.30;}
    public double Advance(double delta,FeverState.Phase phase,double progress)
    {
        if(delta<=0||double.IsNaN(delta)||double.IsInfinity(delta))return 0;
        double transition=phase==FeverState.Phase.Entering?900*Math.Sin(FeverFlowMath.Clamp(progress/.66)*Math.PI):phase==FeverState.Phase.Exiting?700*Math.Sin(FeverFlowMath.Clamp(progress)*Math.PI):0;
        Speed=24+Math.Max(Boost,transition);double angle=Speed*delta;Boost*=Math.Exp(-delta/Math.Max(.08,decay));return angle;
    }
    public void Clear(){Boost=0;Speed=24;}
}

/// <summary>Independent timed supply, with no kill dependency, burst catch-up or full-pool debt.</summary>
public sealed class FeverSupplyClock
{
    private double wait;
    public void Reset()=>wait=0;
    public bool Advance(double delta,double interval,int active,int limit)
    {
        if(delta<=0||double.IsNaN(delta)||double.IsInfinity(delta))return false;
        interval=FeverSettings.Safe(interval,.12,.06);limit=Math.Max(1,limit);
        wait-=delta;
        if(active>=limit){wait=Math.Max(0,wait);return false;}
        if(wait>1e-9)return false;
        // Preserve the sub-frame remainder, but a long stall never banks a burst.
        wait=wait<=-interval?interval:wait+interval;return true;
    }
}
/// <summary>Independent fragment channels; never touches Unity's shared random state.</summary>
public static class FeverScatter
{
    public static double Unit(int seed,int index,int channel)
    {
        unchecked {uint x=(uint)seed^((uint)index*0x9E3779B9u)^((uint)(channel+1)*0x85EBCA6Bu);
        x^=x>>16;x*=0x7FEB352Du;x^=x>>15;x*=0x846CA68Bu;x^=x>>16;return (x&0xFFFFFFu)/16777216.0;}
    }
}
