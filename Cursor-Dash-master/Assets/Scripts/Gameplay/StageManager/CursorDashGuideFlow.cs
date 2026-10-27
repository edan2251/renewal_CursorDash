using System;
using System.Collections.Generic;

/// <summary>Practice-only progress and presentation clocks. No Unity, scoring, spawning or storage.</summary>
public sealed class CursorDashGuideFlow
{
    public enum Phase { Practicing, Settling, Fading, Reset, Done, Cancelled }
    public Phase Current {get;private set;}=Phase.Practicing;
    public int CircleWins {get;private set;}
    public int HexWins {get;private set;}
    public double ResetAge {get;private set;}
    public double FinishAge {get;private set;}
    private readonly Dictionary<int,ulong> accepted=new Dictionary<int,ulong>();
    public bool Accept(int kind,int source,ulong execution)
    {
        if(execution==0||Current!=Phase.Practicing||kind<0||kind>1)return false;
        if(accepted.TryGetValue(source,out var previous)&&execution<=previous)return false;
        accepted[source]=execution;
        if(kind==0){if(CircleWins==2)return false;CircleWins++;}
        else {if(HexWins==2)return false;HexWins++;}
        if(CircleWins==2&&HexWins==2)Current=Phase.Settling;
        return true;
    }
    public void AdvanceFinish(double delta,bool paused,bool particlesDone,double settle=.35,double fade=.4)
    {
        if(paused||!FinitePositive(delta)||(Current!=Phase.Settling&&Current!=Phase.Fading))return;
        FinishAge+=delta;
        if(Current==Phase.Settling&&particlesDone&&FinishAge>=Safe(settle,.35,.1)){Current=Phase.Fading;FinishAge=0;}
        else if(Current==Phase.Fading&&FinishAge>=Safe(fade,.4,.1)){Current=Phase.Reset;FinishAge=0;}
    }
    public void AdvanceReset(double delta,bool paused,double duration)
    {
        if(Current!=Phase.Reset||paused||!FinitePositive(delta))return;
        ResetAge+=delta;if(ResetAge>=Safe(duration,1.1,.4))Current=Phase.Done;
    }
    public void Cancel(){if(Current!=Phase.Done)Current=Phase.Cancelled;}
    public static bool FinitePositive(double n)=>n>0&&!double.IsNaN(n)&&!double.IsInfinity(n);
    public static double Safe(double n,double fallback,double minimum)=>double.IsNaN(n)||double.IsInfinity(n)?fallback:Math.Max(minimum,n);
    public static double Smooth(double t){t=Math.Max(0,Math.Min(1,t));return t*t*(3-2*t);}
    public static double ResetRing(double p)=>1; // Power-on changes color/spin, never nexus size.
    public static double ResetCircuit(double p)=>p<.43?1-Smooth(p/.43):Smooth((p-.52)/.48);
    public static double ResetBody(double p)=>1;
    public static double ResetScore(double p)=>Smooth((p-.68)/.30);
    public static double ResetPower(double p)=>Smooth((p-.56)/.32);
}

/// <summary>Shared guide-only supply/spacing arithmetic, independent of Unity and game rewards.</summary>
public static class CursorDashGuideRules
{
    public static bool NeedsRecovery(bool active,bool hex,bool resolving,bool returnPending,bool nexusArrived)
        =>!active||(hex?nexusArrived||resolving&&!returnPending:resolving);
    public static bool Separated(double x,double y,double radius,double otherX,double otherY,double otherRadius,double gap)
    {double dx=x-otherX,dy=y-otherY,r=radius+otherRadius+gap;return dx*dx+dy*dy>=r*r;}
    public static bool SupplyDue(double now,double next,int count,int cap)=>now>=next&&count<Math.Max(1,cap);
    public static int NextKind(int planned,int circleWins,int hexWins)=>circleWins>=2?1:hexWins>=2?0:planned;
}

public sealed class FeverFirstNotice
{
    private double age,hold,fadeOut;
    public bool Active {get;private set;}
    public double Total=>hold+fadeOut;
    public double Elapsed=>age;
    public double ReleaseProgress=>CursorDashGuideFlow.Smooth((age-hold)/fadeOut);
    public double PreviewAge=>Math.Max(0,age-hold);
    public double Alpha=>Active?1-ReleaseProgress:0;
    public double TimeScale=>Active?0:1;
    // Signature retained for existing callers. The retired fade-in/recovery/slow parameters are no longer used.
    public void Begin(double fade=0,double read=2,double exit=2,double settle=0,double minimumScale=0)
    {
        hold=CursorDashGuideFlow.Safe(read,2,.3);fadeOut=CursorDashGuideFlow.Safe(exit,2,.08);age=0;Active=true;
    }
    public bool Advance(double delta,bool paused)
    {
        if(!Active||paused||!CursorDashGuideFlow.FinitePositive(delta))return false;
        age=Math.Min(Total,age+delta);if(age<Total)return false;Active=false;return true;
    }
    public void Cancel(){age=0;Active=false;}
}
