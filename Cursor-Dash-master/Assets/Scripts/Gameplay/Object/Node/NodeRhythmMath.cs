using System;

/// <summary>Node presentation and rotation-phase arithmetic. No native tween, damage or reward ownership.</summary>
public static class NodeRhythmMath
{
    public const int MorphEndTurn=3,FireMovement=4;
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
    public static int RotationChargeStage(double turns)=>turns>=2-.000001?3:turns>=1-.000001?2:1;
    public static double RotationEndSeconds(int turn,double movement,double pause)=>Math.Max(1,turn)*Safe(movement,.4,.00001)+(Math.Max(1,turn)-1)*Safe(pause,0,0);
    public static double RotationFireSeconds(double movement,double pause)=>(FireMovement-1)*(Safe(movement,.4,.00001)+Safe(pause,0,0));
    public static double RotationMorphSeconds(double movement,double requestedMorph)=>Math.Min(Safe(movement,.4,.00001),Safe(requestedMorph,.25,.000001));
    public static double RotationMorphStartSeconds(double movement,double pause,double requestedMorph)=>RotationEndSeconds(MorphEndTurn,movement,pause)-RotationMorphSeconds(movement,requestedMorph);
    public static bool RotationMorphReady(double elapsed,double movement,double pause,double requestedMorph)=>elapsed>=RotationMorphStartSeconds(movement,pause,requestedMorph)-.000001;
    // Turns stay at 3 throughout the rest. The native tween elapsed clock distinguishes END 3 from START 4.
    public static bool RotationReady(double elapsed,double movement,double pause)=>elapsed>=RotationFireSeconds(movement,pause)-.000001;
    public static double RotationChargePulse(double age,double settle,int stage)
    {
        if(stage<1||age<0||double.IsNaN(age)||double.IsInfinity(age))return 0;
        return Math.Sin(Math.PI*Clamp(age/Safe(settle,.14,.001)))*(stage==3?1.35:1);
    }
    public static double RotationChargeFill(double turns,double kickAge,double settle,double elasticity)
    {
        // Born at 1/3. First and second movements add one third with a quick rise, then hold through the rest.
        double p=Math.Max(0,turns);double t=Clamp(p<1?p:p-1),rise=1-Math.Pow(1-t,3);
        double fill=p<1?(1+rise)/3:p<2?(2+rise)/3:1;
        int stage=RotationChargeStage(turns);double kick=RotationChargePulse(kickAge,settle,stage)*Safe(elasticity,.06,0)/3;
        return Clamp(fill+(stage==3?-kick:kick));
    }
}

/// <summary>Immutable per-spawn timeline. One beat is a native movement plus its rest; zero is spawn.</summary>
public sealed class HexRotationTimeline
{
    public const double DefaultMorphLength=.3125;
    public const double DefaultBorn=.1,DefaultFirstAt=.95,DefaultFirstTarget=.6,DefaultChargeLength=.2,DefaultSecondAt=1.95,DefaultMorphAt=2.8,DefaultFireAt=3.15,DefaultFlightSeconds=.1;
    public readonly double Born,FirstAt,FirstTarget,FirstLength,SecondAt,SecondTarget,SecondLength,MorphAt,MorphLength,FireAt;
    public HexRotationTimeline(double born=DefaultBorn,double firstAt=DefaultFirstAt,double firstTarget=DefaultFirstTarget,double firstLength=DefaultChargeLength,
        double secondAt=DefaultSecondAt,double secondTarget=1,double secondLength=DefaultChargeLength,double morphAt=DefaultMorphAt,double morphLength=DefaultMorphLength,double fireAt=DefaultFireAt)
    {Born=born;FirstAt=firstAt;FirstTarget=firstTarget;FirstLength=firstLength;SecondAt=secondAt;SecondTarget=secondTarget;SecondLength=secondLength;MorphAt=morphAt;MorphLength=morphLength;FireAt=fireAt;}
    private static double Clamp(double n)=>Math.Max(0,Math.Min(1,n));
    public string Warning
    {
        get {
            foreach(double n in new[]{Born,FirstAt,FirstTarget,FirstLength,SecondAt,SecondTarget,SecondLength,MorphAt,MorphLength,FireAt})if(double.IsNaN(n)||double.IsInfinity(n))return "유한한 수만 입력하세요.";
            if(Born<0||FirstTarget<Born||SecondTarget<FirstTarget||SecondTarget>1)return "충전 목표는 생성량 이상으로 누적되어야 하며 100%를 넘을 수 없습니다.";
            if(FirstAt<0||FirstLength<=0||SecondLength<=0||MorphLength<=0||FireAt<=0)return "시점은 0 이상, 소요 길이와 발사 시점은 0보다 커야 합니다.";
            if(FirstAt+FirstLength>SecondAt+1e-8)return "첫 충전이 끝나기 전에 두 번째 충전이 시작됩니다.";
            if(SecondAt+SecondLength>MorphAt+1e-8)return "두 번째 충전이 끝나기 전에 변형이 시작됩니다.";
            if(MorphAt+MorphLength>FireAt+1e-8)return "변형 완료보다 발사가 빠릅니다. 발사 시점은 자동으로 미루지 않습니다.";
            return "";
        }
    }
    public double Seconds(double beat,double movement,double pause)=>beat*(movement+pause);
    public static double Beat(double elapsed,double movement,double pause)=>Math.Max(0,elapsed)/Math.Max(.000001,movement+pause);
    public int DueEvents(double beat)=>(beat>=FirstAt-1e-8?1:0)|(beat>=SecondAt-1e-8?2:0)|(beat>=MorphAt-1e-8?4:0)|(beat>=MorphAt+MorphLength-1e-8?8:0)|(beat>=FireAt-1e-8?16:0);
    public int CompletedCharges(double beat)=>1+(beat>=FirstAt+FirstLength-1e-8?1:0)+(beat>=SecondAt+SecondLength-1e-8?1:0);
    public double Fill(double beat,double elasticity,out double pulse)
    {
        pulse=0;if(beat<FirstAt)return Born;
        double from=beat<SecondAt?Born:FirstTarget,to=beat<SecondAt?FirstTarget:SecondTarget;
        double t=Clamp((beat-(beat<SecondAt?FirstAt:SecondAt))/(beat<SecondAt?FirstLength:SecondLength));
        pulse=t>=1?0:Math.Sin(t*Math.PI)*(to>=1?1.35:1);
        double fill=from+(to-from)*(1-Math.Pow(1-t,3));
        return Clamp(fill+(to>=1?-1:1)*pulse*Math.Max(0,elasticity)*(to-from));
    }
    public double NextBeat(double beat)
    {
        foreach(double at in new[]{FirstAt,FirstAt+FirstLength,SecondAt,SecondAt+SecondLength,MorphAt,MorphAt+MorphLength,FireAt})if(at>beat+1e-8)return at;
        return beat;
    }
    public string NextEvent(double beat)
    {
        double[] at={FirstAt,FirstAt+FirstLength,SecondAt,SecondAt+SecondLength,MorphAt,MorphAt+MorphLength,FireAt};
        string[] names={"충전 1 시작","충전 1 완료","충전 2 시작","충전 2 완료","삼각 변형 시작","삼각 변형 완료","레이저 발사"};
        for(int i=0;i<at.Length;i++)if(at[i]>beat+1e-8)return names[i];return "타임라인 완료";
    }
}
