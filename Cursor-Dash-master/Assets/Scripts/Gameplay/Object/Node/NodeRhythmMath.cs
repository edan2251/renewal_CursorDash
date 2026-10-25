using System;

/// <summary>Node presentation and rotation-phase arithmetic. No native tween, damage or reward ownership.</summary>
public static class NodeRhythmMath
{
    public const double MorphStartTurn=5.5;
    public const int FireTurn=6;
    private static double Clamp(double t)=>Math.Max(0,Math.Min(1,t));
    private static double Safe(double value,double fallback,double minimum)=>double.IsNaN(value)||double.IsInfinity(value)?fallback:Math.Max(minimum,value);
    private static double Smooth(double t){t=Clamp(t);return t*t*(3-2*t);}
    public static double Departure(double elapsed,double seconds)=>1-Smooth(Math.Max(0,elapsed)/Safe(seconds,.18,.001));
    public static double BonusScale(double age,double appear,double remaining,double retire)
    {
        double t=Clamp(Math.Max(0,age)/Safe(appear,.12,.001))-1;
        double born=.70+.30*(1+2.7*t*t*t+1.7*t*t);
        // The same scale drives the visible circle and its collider; expiry time is not extended.
        return born*Smooth(Math.Max(0,remaining)/Safe(retire,.55,.001));
    }
    public static double RotationTurns(double elapsed,double movement,double pause)
    {
        if(double.IsNaN(elapsed)||double.IsInfinity(elapsed)||double.IsNaN(movement)||double.IsInfinity(movement)||movement<=0)return 0;
        elapsed=Math.Max(0,elapsed);pause=Safe(pause,0,0);double cycle=movement+pause,loops=Math.Floor(elapsed/cycle);
        return loops+Clamp((elapsed-loops*cycle)/movement);
    }
    public static int RotationChargeStage(double turns)=>turns>=5-.000001?3:turns>=3-.000001?2:turns>=1-.000001?1:0;
    public static bool RotationMorphReady(double turns)=>turns>=MorphStartTurn-.000001;
    public static bool RotationReady(double turns)=>turns>=FireTurn-.000001;
    public static double RotationMorphSeconds(double movement,double requestedMorph,double requestedHold)
    {
        // Fit the existing morph/aiming pose inside the second half of the firing movement.
        double window=Safe(movement,.4,.00001)*.5;
        double hold=Math.Min(window*.5,Safe(requestedHold,.025,0));
        return Math.Min(window-hold,Safe(requestedMorph,.25,.000001));
    }
    public static double RotationChargePulse(double age,double settle,int stage)
    {
        if(stage<1||age<0||double.IsNaN(age)||double.IsInfinity(age))return 0;
        return Math.Sin(Math.PI*Clamp(age/Safe(settle,.14,.001)))*(stage==3?1.35:1);
    }
    public static double RotationChargeFill(double turns,double kickAge,double settle,double elasticity)
    {
        // Movements 1, 3 and 5 grow the charge; each third settles at that movement's END.
        double p=Math.Max(0,turns);double fill=p<1?p/3:p<2?1d/3:p<3?(1+p-2)/3:p<4?2d/3:p<5?(2+p-4)/3:1;
        int stage=RotationChargeStage(turns);double kick=RotationChargePulse(kickAge,settle,stage)*Safe(elasticity,.06,0)/3;
        return Clamp(fill+(stage==3?-kick:kick));
    }
}
