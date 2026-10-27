using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

/// <summary>Pure fever rules/configuration. No Unity API, storage, damage or spawning.</summary>
[Serializable]
public sealed class FeverSettings
{
    public double chargePerKill=.125,decayGrace=4,decayPerSecond=.03;
    public double closeSeconds=.22,enterSeconds=1.05,feverSeconds=8,exitSeconds=.95,gapRevealSeconds=.12;
    public int bonusInitial=18,bonusPerWave=6,bonusCapacity=36;
    public double bonusWaveSeconds=.85,bonusLife=2.8,bonusRadius=.205;
    public double gaugeSmoothSeconds=.06,gaugeThickness=.14,gaugeFontSize=32,gaugeGapDegrees=4;
    public double bonusStop=.012,bonusStopGap=.18,bonusScreenGap=.10,bonusShake=.06;
    public static double Safe(double value,double fallback,double min) => double.IsNaN(value)||double.IsInfinity(value)?fallback:Math.Max(min,value);
}

public sealed class FeverState
{
    public enum Phase { Normal, Closing, Entering, Fever, Exiting, Stopped }
    public const int MaxMultiplier=4;
    public readonly FeverSettings Settings;
    private readonly Dictionary<int,ulong> kills=new Dictionary<int,ulong>();
    private double idle,elapsed;
    public Phase Current {get;private set;}=Phase.Normal;
    public BigInteger Points {get;private set;}=BigInteger.Zero;
    public int Multiplier {get;private set;}=1;
    public double Gauge {get;private set;}
    public bool Paused {get;set;}
    public bool PracticeOnly {get;set;}
    public bool PresentationHold {get;set;}
    public int CompletedFevers {get;private set;}
    public string LastReset {get;private set;}="";
    public uint ResetVersion {get;private set;}
    public double NormalCombatSeconds {get;private set;}
    public bool SuppressesThreats=>Current==Phase.Entering||Current==Phase.Fever||Current==Phase.Exiting;
    public event Action<Phase,Phase> Transitioned;
    public event Action<BigInteger,BigInteger> PointsAwarded; // Presentation notification: actual awarded delta, exact new total.
    public double Elapsed=>elapsed;
    public string PointsText=>Points.ToString(CultureInfo.InvariantCulture);
    public string MultiplierText=>"x"+Multiplier.ToString(CultureInfo.InvariantCulture);
    public double PhaseSeconds=>Current==Phase.Closing?FeverSettings.Safe(Settings.closeSeconds,.22,.001):
        Current==Phase.Entering?FeverSettings.Safe(Settings.enterSeconds,1.05,.001):
        Current==Phase.Fever?FeverSettings.Safe(Settings.feverSeconds,8,.01):
        Current==Phase.Exiting?FeverSettings.Safe(Settings.exitSeconds,.95,.001):0;
    public double Progress=>PhaseSeconds>0?Math.Min(1,elapsed/PhaseSeconds):0;
    public FeverState(FeverSettings settings=null){Settings=settings??new FeverSettings();}
    public void ResetRun(){kills.Clear();Points=0;Multiplier=1;Gauge=idle=elapsed=0;NormalCombatSeconds=0;Current=Phase.Normal;CompletedFevers=0;Paused=PresentationHold=PracticeOnly=false;LastReset="";ResetVersion++;}
    public bool Kill(int source,ulong execution)
    {
        if(Current==Phase.Stopped||PresentationHold||execution==0||(kills.TryGetValue(source,out var previous)&&execution<=previous))return false;
        kills[source]=execution;BigInteger awarded=Multiplier;Points+=awarded;
        if(Current==Phase.Normal){idle=0;Gauge=Math.Min(PracticeOnly ? .875 : 1,Gauge+FeverSettings.Safe(Settings.chargePerKill,.125,0));if(Gauge>=1&&!PracticeOnly)Transition(Phase.Closing);}
        // Confirmed policy: no recharge from kills during fever or its transitions.
        PointsAwarded?.Invoke(awarded,Points);
        return true;
    }
    public void NexusHit(){if(Current==Phase.Stopped)return;PresentationHold=false;Gauge=idle=0;Multiplier=1;LastReset="NexusHit";ResetVersion++;Transition(Phase.Normal);}
    public void CancelFever(){if(Current==Phase.Stopped)return;PresentationHold=false;Gauge=idle=0;LastReset="Cancelled";ResetVersion++;Transition(Phase.Normal);}
    public void Abort(){Gauge=0;Paused=PresentationHold=false;LastReset="RoundEnd";ResetVersion++;Transition(Phase.Stopped);}
    public void FillForReview(){if(Current==Phase.Normal&&!PracticeOnly){Gauge=1;Transition(Phase.Closing);}}
    private void Transition(Phase next){var previous=Current;Current=next;elapsed=0;if(next==Phase.Closing)LastReset="";Transitioned?.Invoke(previous,next);}
    public void Tick(double delta)
    {
        if(Paused||PresentationHold||Current==Phase.Stopped||delta<=0||double.IsNaN(delta)||double.IsInfinity(delta))return;
        while(delta>0&&!PresentationHold){
            if(Current==Phase.Normal){
                double grace=FeverSettings.Safe(Settings.decayGrace,4,0);
                double decayTime=Math.Max(0,idle+delta-grace)-Math.Max(0,idle-grace);
                idle+=delta;if(!PracticeOnly)NormalCombatSeconds+=delta;Gauge=Math.Max(0,Gauge-decayTime*FeverSettings.Safe(Settings.decayPerSecond,.03,0));return;
            }
            double span=PhaseSeconds,step=Math.Min(delta,Math.Max(0,span-elapsed));elapsed+=step;delta-=step;
            if(Current==Phase.Closing)NormalCombatSeconds+=step;
            if(Current==Phase.Fever)Gauge=Math.Max(0,1-elapsed/span);
            if(elapsed<span-1e-10)return;
            if(Current==Phase.Closing)Transition(Phase.Entering);
            else if(Current==Phase.Entering)Transition(Phase.Fever);
            else if(Current==Phase.Fever){Gauge=0;Transition(Phase.Exiting);}
            else if(Current==Phase.Exiting){Multiplier=Math.Min(MaxMultiplier,Multiplier+1);CompletedFevers++;Gauge=idle=0;Transition(Phase.Normal);}
        }
    }
    public static double FillHead(double gauge,double gapDegrees)=>Math.Max(0,Math.Min(1,gauge))*(1-gapDegrees/360);
    public static double CloseHead(double progress,double gapDegrees)=>1-gapDegrees/360+Math.Max(0,Math.Min(1,progress))*gapDegrees/360;
    // One clockwise lap, with a short continuous return across the lower text gap at the end.
    public static double DrainHead(double progress,double gapDegrees,double seconds,double returnSeconds)
    {
        double tail=Math.Min(.2,Math.Max(.001,returnSeconds/Math.Max(.01,seconds))),p=Math.Max(0,Math.Min(1,progress)),main=1-gapDegrees/360;
        if(p>=1)return 1; // Exact endpoint; floating division can otherwise produce 1.0000000000000002.
        return Math.Max(0,Math.Min(1,p<=1-tail?main*p/(1-tail):main+(1-main)*(p-(1-tail))/tail));
    }
}
