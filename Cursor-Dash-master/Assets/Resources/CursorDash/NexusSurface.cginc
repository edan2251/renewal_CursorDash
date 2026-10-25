#ifndef CURSORDASH_NEXUS_SURFACE_INCLUDED
#define CURSORDASH_NEXUS_SURFACE_INCLUDED
fixed4 CDNexusSurface(float2 uv,fixed4 color,float radius,float level,float press)
{
 if(level<=0)return fixed4(0,0,0,0);
 float2 p=(uv-.5)*2;
 float disk=1-smoothstep(radius-.006,radius+.006,length(p));
 // One short downward compression, with no oscillator/clock and no residual idle movement.
 float height=lerp(-radius,radius,level)-press*saturate(4*level*(1-level));
 float liquid=level>=1?1:1-smoothstep(height-.004,height+.004,p.y);
 color.rgb*=.72+.28*saturate((p.y+radius)/(2*radius));
 color.rgb+=exp(-abs(p.y-height)*100)*.12;color.a*=disk*liquid;
 return color;
}
#endif
