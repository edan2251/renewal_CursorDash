using System;

/// <summary>Single-settle presentation curve. Does not own spawn, preparation or attack time.</summary>
public static class NodeEntranceMath
{
    private static double Clamp(double t)=>Math.Max(0,Math.Min(1,t));
    private static double Back(double t,double overshoot){t=Clamp(t)-1;return 1+(overshoot+1)*t*t*t+overshoot*t*t;}
    public static double Body(double progress)=>Back(progress,.85);
    public static double Ring(double progress)=>Back((Clamp(progress)-.14)/.86,1.05);
    public static double RingTurn(double progress)=>-48*Math.Pow(1-Clamp((progress-.14)/.86),2);
    public static double Collider(double progress)=>Clamp(Body(progress));
}
