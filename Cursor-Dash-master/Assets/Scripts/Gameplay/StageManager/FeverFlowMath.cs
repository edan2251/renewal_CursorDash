using System;

/// <summary>Presentation-only curves and grouping gates. Kept independent of Unity for numerical checks.</summary>
public static class FeverFlowMath
{
    public static double Clamp(double t)=>Math.Max(0,Math.Min(1,t));
    public static double Smooth(double t){t=Clamp(t);return t*t*(3-2*t);}
    public static double FinalScoreBreath(double age,double amount,double seconds,double fade)
    {
        if(age<=0||double.IsNaN(age)||double.IsInfinity(age))return 1;
        amount=Math.Max(0,Math.Min(.08,amount));seconds=Math.Max(1,Math.Min(6,seconds));
        return 1+amount*Math.Sin(age*Math.PI*2/seconds)*Smooth(age/.25)*Clamp(fade);
    }
    public static double Out(double t){t=Clamp(t);return t>=1-1e-12?1:1-Math.Pow(2,-9*t);}
    public static double OpticsWeight(double elapsed,double remaining)
    {
        if(double.IsNaN(elapsed)||double.IsNaN(remaining)||remaining<=0)return 0;
        return Smooth(elapsed/.12); // Keep fever optics until the real gauge reaches zero, never fade early.
    }
    public static double EntrySpread(double phase)=>Out((phase-.66)/.34);
    // Default preview wrappers. Runtime passes the captured exit's absorb progress/curve to the same field.
    public static double ExitRecallProgress(double phase)=>new FeverExitMotion().Suction(phase*.95);
    public static double ExitRemaining(double phase)=>1-FeverExitMotion.Travel(ExitRecallProgress(phase),3.2);
    public static double DotScale(bool exit,double phase)
    {
        if(!exit)return 1-Smooth((phase-.20)/.42);
        var motion=new FeverExitMotion();double elapsed=phase*motion.Total;return motion.RingScale(elapsed)*motion.RingAlpha(elapsed);
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
    // The existing broad merged fronts contract and twist as a single inward flow; mirrors CDFExitFlow.
    public static double ExitSignedField(double x,double y,double phase,double extent)
    {
        return RecallField(x,y,ExitRecallProgress(phase),extent,3.2,.90);
    }
    public static double RecallField(double x,double y,double progress,double extent,double power,double twist)
    {
        double remaining=1-FeverExitMotion.Travel(progress,power);if(remaining<=.00001)return Math.Max(.001,extent);
        double angle=twist*FeverExitMotion.Travel(progress,power)*(.35+.65*Math.Min(1,Math.Sqrt(x*x+y*y)/Math.Max(.001,extent))),c=Math.Cos(angle),s=Math.Sin(angle);
        return SignedField((x*c-y*s)/remaining,(x*s+y*c)/remaining,.94,extent*1.025)*remaining;
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
    public void EntryPulse(double strength,double seconds){Boost=Math.Min(950,Math.Max(0,strength));decay=Math.Max(.08,Math.Min(.5,seconds));}
    public double Advance(double delta,FeverState.Phase phase,double progress)
    {
        if(delta<=0||double.IsNaN(delta)||double.IsInfinity(delta))return 0;
        double transition=phase==FeverState.Phase.Entering?900*Math.Sin(FeverFlowMath.Clamp(progress/.66)*Math.PI):phase==FeverState.Phase.Exiting?700*Math.Sin(FeverFlowMath.Clamp(progress)*Math.PI):0;
        Speed=24+Math.Max(Boost,transition);double angle=Speed*delta;Boost*=Math.Exp(-delta/Math.Max(.08,decay));return angle;
    }
    public void Clear(){Boost=0;Speed=24;}
    public double AdvanceExit(double delta,double elapsed,FeverExitMotion exit){Boost=0;Speed=exit.Speed(elapsed);return delta>0?Speed*delta:0;}
}

/// <summary>One captured presentation clock for ring, reward copies, white world and final optics. No combat ownership.</summary>
public sealed class FeverExitMotion
{
    public readonly double Appear,Absorb,Settle,Power,MaxSpin,SpinDecay,Squeeze,Bend,Twist,Chromatic,Shake,Lens;
    public readonly double AppearSpin,NodeStretch,NodeShrinkStart,SpacePull,RecoilHold;
    public double Impact=>Appear+Absorb;
    public double Total=>Impact+Settle;
    private static double Safe(double x,double fallback,double low,double high)=>double.IsNaN(x)||double.IsInfinity(x)?fallback:Math.Max(low,Math.Min(high,x));
    // Retain the original constructor for existing callers; new profile values are captured by the full overload.
    public FeverExitMotion(double appear=.20,double absorb=.51,double settle=.24,double power=3.2,double maxSpin=1260,double spinDecay=.10,double squeeze=.12,double bend=.28,double twist=.90,double chromatic=.006,double shake=.0035,double lens=.012)
        :this(appear,absorb,settle,power,maxSpin,spinDecay,squeeze,bend,twist,chromatic,shake,lens,180,.65,.22,.65,.06){}
    public FeverExitMotion(double appear,double absorb,double settle,double power,double maxSpin,double spinDecay,double squeeze,double bend,double twist,double chromatic,double shake,double lens,double appearSpin,double nodeStretch,double nodeShrinkStart,double spacePull,double recoilHold)
    {
        Appear=Safe(appear,.20,.08,.6);Absorb=Safe(absorb,.51,.25,1.2);Settle=Safe(settle,.24,.1,.6);Power=Safe(power,3.2,1.2,4);MaxSpin=Safe(maxSpin,1260,180,1800);SpinDecay=Safe(spinDecay,.10,.04,.4);
        Squeeze=Safe(squeeze,.12,0,.2);Bend=Safe(bend,.28,0,.5);Twist=Safe(twist,.90,0,1.8);Chromatic=Safe(chromatic,.006,0,.006);Shake=Safe(shake,.0035,0,.006);Lens=Safe(lens,.012,0,.018);
        AppearSpin=Math.Min(MaxSpin,Safe(appearSpin,180,24,480));NodeStretch=Safe(nodeStretch,.65,0,1);NodeShrinkStart=Safe(nodeShrinkStart,.22,.05,.4);SpacePull=Safe(spacePull,.65,0,.85);RecoilHold=Math.Min(Settle*.5,Safe(recoilHold,.06,0,.12));
    }
    public static double Travel(double u,double power){u=FeverFlowMath.Clamp(u);return .04*u+.20*u*u+.76*Math.Pow(u,power);}
    public double Suction(double elapsed)=>FeverFlowMath.Clamp((elapsed-Appear)/Absorb);
    public double Pressure(double elapsed)
    {
        if(elapsed<=Appear||elapsed>=Total)return 0; // Appearance is ring-only: no pull, shrink, chromatic offset or shake.
        if(elapsed<=Impact)return Math.Pow(Suction(elapsed),1.4);
        return 1-FeverFlowMath.Smooth((elapsed-Impact)/Math.Min(.14,Settle));
    }
    public double Speed(double elapsed)
    {
        if(elapsed<Appear)return AppearSpin;
        if(elapsed<=Impact)return AppearSpin+(MaxSpin-AppearSpin)*FeverFlowMath.Smooth(Suction(elapsed)/.32);
        double t=Math.Max(0,elapsed-Impact),end=Math.Exp(-Settle/SpinDecay);
        return 24+(MaxSpin-24)*FeverFlowMath.Clamp((Math.Exp(-t/SpinDecay)-end)/(1-end));
    }
    public double RingAlpha(double elapsed)=>FeverFlowMath.Smooth(elapsed/(Appear*.7));
    public double RingScale(double elapsed)
    {
        if(elapsed<Appear)return .96+.04*FeverFlowMath.Smooth(elapsed/Appear);
        double press=elapsed<=Impact?FeverFlowMath.Smooth((elapsed-(Impact-.025))/.025):1-FeverFlowMath.Smooth((elapsed-Impact-RecoilHold)/Math.Max(.001,Settle-RecoilHold));
        return 1-Squeeze*press; // Hold once after the white world is gone, so the recoil is readable.
    }
    public static double Arrival(double distance,double maximum,int index,bool last)=>last?1:Math.Min(.98,.84+.12*Math.Max(0,distance)/Math.Max(.001,maximum)+.02*FeverScatter.Unit(41,index,3));
    public double SizeAtDistance(double remaining)=>FeverFlowMath.Smooth(FeverFlowMath.Clamp(remaining)/NodeShrinkStart);
    public double StretchAt(double travelled)=>1+NodeStretch*Math.Sin(Math.PI*FeverFlowMath.Clamp(travelled));
    public static double NodeScale(double progress)=>new FeverExitMotion().SizeAtDistance(1-Travel(progress,3.2));
    public void Position(double sx,double sy,double cx,double cy,double radius,double progress,int index,out double x,out double y)
    {
        double dx=sx-cx,dy=sy-cy,d=Math.Sqrt(dx*dx+dy*dy),nx=d>.000001?dx/d:1,ny=d>.000001?dy/d:0;
        double r=Math.Min(Math.Max(0,radius),d*.8),ex=cx+nx*r,ey=cy+ny*r;
        double sign=(index&1)==0?1:-1,bend=d*Bend*(.75+.25*FeverScatter.Unit(17,index,4))*sign;
        double bx=(sx+ex)*.5-ny*bend,by=(sy+ey)*.5+nx*bend,t=Travel(progress,Power),a=1-t;
        x=a*a*sx+2*a*t*bx+t*t*ex;y=a*a*sy+2*a*t*by+t*t*ey;
    }
}

/// <summary>One bounded spring for score presentation, driven only by actual awarded delta. Never changes points.</summary>
public sealed class ScoreProjectionPulse
{
    public double Value {get;private set;}
    private double velocity;
    public void Clear(){Value=velocity=0;}
    public void Award(System.Numerics.BigInteger delta)
    {
        if(delta<=0)return;
        double gain=delta>=4?1:Math.Log(1+(double)delta)/Math.Log(5); // Design choice: per-award gain, not accumulated total.
        Value=Math.Min(1,Math.Max(Value,.30*gain));velocity=Math.Min(22,Math.Max(velocity,22*gain));
    }
    public void Advance(double delta,double seconds)
    {
        if(delta<=0||double.IsNaN(delta)||double.IsInfinity(delta))return;
        seconds=double.IsNaN(seconds)||double.IsInfinity(seconds)?.32:Math.Max(.15,Math.Min(.65,seconds));
        double w=6.5/seconds,damping=.65*w,damped=w*Math.Sqrt(1-.65*.65),decay=Math.Exp(-damping*delta),a=Value,b=(velocity+damping*a)/damped;
        double c=Math.Cos(damped*delta),s=Math.Sin(damped*delta);Value=decay*(a*c+b*s);velocity=decay*((-damping*a+damped*b)*c+(-damping*b-damped*a)*s);
        if(Value>=1){Value=1;velocity=Math.Min(0,velocity);}else if(Value<=-.06){Value=-.06;velocity=Math.Max(0,velocity);}
        if(Math.Abs(Value)<.0001&&Math.Abs(velocity)<.001)Clear();
    }
    public static double Fit(double width,double height,double designHeight,double safeWidth,double safeHeight,double maximum)=>Math.Max(0,Math.Min(designHeight/Math.Max(.0001,height),Math.Min(safeWidth/Math.Max(.0001,width)/Math.Max(1,maximum),safeHeight/Math.Max(.0001,height)/Math.Max(1,maximum))));
    public double Scale(double strength,double maximum)=>Math.Min(Math.Max(1,maximum),Math.Max(.98,1+Math.Max(0,Math.Min(.4,strength))*Value));
    public double Opacity(double rest,double boost,double maximum)=>Math.Max(0,Math.Min(Math.Min(.35,maximum),rest+Math.Max(0,Value)*Math.Max(0,boost)));
}

/// <summary>Finite visual impulses sampled from one clock; no transform, HP, gauge or collision ownership.</summary>
public sealed class NexusMagnetism
{
    private double charge=-100,damage=-100;private bool directional;private double angle;
    public void Clear(){charge=damage=-100;directional=false;angle=0;}
    public void Charge(double clock)=>charge=clock;
    public void Hit(double clock,bool known,double radians){damage=clock;directional=known;angle=radians;}
    private static double Pulse(double age,double seconds){double t=FeverFlowMath.Clamp(age/Math.Max(.08,seconds));return age<0?0:Math.Sin(Math.PI*t)*(1-t)/.57923;}
    public double Body(double clock,double breath,double gain,double press,double seconds)=>1+breath*Math.Sin(clock*Math.PI*2/3.2)+gain*Pulse(clock-charge,seconds)-press*Pulse(clock-damage,seconds);
    public double Light(double clock,double breath,double seconds)=>breath*(.5+.5*Math.Sin(clock*Math.PI*2/3.2))+ .55*Pulse(clock-charge,seconds);
    public double Ring(double clock,double gain,double press,double seconds)=>1+gain*Pulse(clock-charge-.025,seconds)-press*Pulse(clock-damage-.018,seconds);
    public double Arc(double clock,double radians,double gauge,double idle,double gain,double press,double gap,double seconds)
        =>ArcShaped(clock,radians,gauge,idle,gain,press,gap,seconds,.8,.12);
    public double ArcShaped(double clock,double radians,double gauge,double idle,double gain,double press,double gap,double seconds,double motion,double stagger)
    {
        double weight=directional?.25+.75*Math.Pow((1+Math.Cos(radians-angle))*.5,2):1;
        // Fixed per-angle phase/strength: an irregular ripple, never a fresh random pose each frame.
        double phase=.5+.5*Math.Sin(radians*2.73+1.17),variation=1+.08*Math.Sin(radians*4.19+.61);
        double delay=.035+Math.Max(0,Math.Min(.24,stagger))*phase;
        double displacement=idle*Math.Sin(clock*.85+radians*1.7)+gap*FeverFlowMath.Clamp(gauge)
            +gain*variation*Pulse(clock-charge-delay,seconds)-press*weight*Pulse(clock-damage-.025*(1-weight),seconds);
        return 1+Math.Max(0,Math.Min(1,motion))*displacement;
    }
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
