using System;

/// <summary>One captured presentation clock. No score, enemy, input or fever rule clock.</summary>
public sealed class CursorDashStartTimeline
{
    public readonly double Seconds,RetreatEnd,RebuildEnd,NexusAt,CursorAt,RingLead,BodyOvershoot,CursorAcceleration;
    public double Age {get;private set;}
    public double Progress=>Clamp(Age/Seconds);
    public bool Complete=>Age>=Seconds;
    public CursorDashStartTimeline(double seconds=1.35,double retreat=.28,double rebuild=.82,double nexus=.42,double cursor=.64)
        :this(seconds,retreat,rebuild,nexus,cursor,.10,.045,.18){}
    public CursorDashStartTimeline(double seconds,double retreat,double rebuild,double nexus,double cursor,double ringLead,double overshoot,double acceleration)
    {
        Seconds=Safe(seconds,1.35,.65,3);RetreatEnd=Safe(retreat,.28,.12,.4);
        RebuildEnd=Safe(rebuild,.82,RetreatEnd+.15,.95);NexusAt=Safe(nexus,.42,.25,.65);CursorAt=Safe(cursor,.64,NexusAt+.1,.82);
        RingLead=Safe(ringLead,.10,.03,.18);BodyOvershoot=Safe(overshoot,.045,0,.08);CursorAcceleration=Safe(acceleration,.18,.08,.4);
    }
    public void Advance(double dt){if(!double.IsNaN(dt)&&!double.IsInfinity(dt)&&dt>0)Age=Math.Min(Seconds,Age+dt);}
    public static double Clamp(double x)=>Math.Max(0,Math.Min(1,x));
    public static double Smooth(double x){x=Clamp(x);return x*x*(3-2*x);}
    private static double Safe(double x,double fallback,double lo,double hi)=>double.IsNaN(x)||double.IsInfinity(x)?fallback:Math.Max(lo,Math.Min(hi,x));
    public double Retreat=>Smooth(Progress/RetreatEnd);
    public double Rebuild=>Smooth((Progress-RetreatEnd)/(RebuildEnd-RetreatEnd));
    public double LogoAlpha=>1-Smooth(Progress/.22);
    public double LogoTravel=>Math.Pow(Clamp(Progress/.22),2);
    public double PromptAlpha=>1-Smooth(Progress/.16);
    public double BestTravel=>Smooth((Progress-.10)/.72);
    public double BestAlpha=>1-Smooth((Progress-.30)/.52);
    public double CurrentScoreAlpha=>Smooth((Progress-.50)/.36);
    public double Ring=>Smooth((Progress-NexusAt+RingLead)/RingLead);
    public double Nexus=>Body((Progress-NexusAt)/.28,BodyOvershoot);
    public static double Body(double t,double overshoot){t=Clamp(t);return t<.72?(1+overshoot)*(1-Math.Pow(1-t/.72,3)):1+overshoot*(1-Smooth((t-.72)/.28));}
    public static double Launch(double t,double acceleration=.18){t=Clamp(t);return t<acceleration?t*t/acceleration:1-(1-t)*(1-t)/(1-acceleration);}
    public double Cursor=>Launch((Progress-CursorAt)/Math.Min(.28,1-CursorAt),CursorAcceleration);
    public double ScoreTravel=>Smooth((Progress-.10)/.72);
    public double ScoreCountProgress=>Smooth((Progress-.08)/.70);
}

/// <summary>Fixed-duration display only. No calls into score, multiplier, rewards or persistence.</summary>
public static class CursorDashScoreCountdown
{
    public static System.Numerics.BigInteger At(System.Numerics.BigInteger best,double progress)
    {
        if(best.Sign<=0)return System.Numerics.BigInteger.Zero;
        if(double.IsNaN(progress))progress=0;
        double p=CursorDashStartTimeline.Clamp(progress);if(p>=1)return System.Numerics.BigInteger.Zero;
        // Reduce significant digits too, so a huge best does not stay twenty digits until the final frame.
        double places=p*(best.ToString(System.Globalization.CultureInfo.InvariantCulture).Length+1);
        int power=(int)Math.Floor(places);long fraction=1000000-(long)Math.Floor((places-power)*900000);
        return best*fraction/(System.Numerics.BigInteger.Pow(10,power)*1000000);
    }
}

/// <summary>Presentation-only loss. The live target is read on every tick, never a stale completion callback.</summary>
public sealed class NexusGaugeRewind
{
    private double from,age,seconds;
    public bool Active=>age<seconds;
    public void Begin(double displayed,double duration){from=CursorDashStartTimeline.Clamp(displayed);age=0;seconds=Math.Max(.04,Math.Min(.4,duration));}
    public void Clear(){from=age=seconds=0;}
    public double Tick(double actual,double delta)
    {
        actual=CursorDashStartTimeline.Clamp(actual);if(!Active)return actual;
        if(delta>0&&!double.IsNaN(delta)&&!double.IsInfinity(delta))age=Math.Min(seconds,age+delta);
        double remainder=Math.Pow(1-CursorDashStartTimeline.Clamp(age/seconds),3);
        return actual+Math.Max(0,from-actual)*remainder;
    }
}
