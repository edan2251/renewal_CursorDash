#ifndef CURSORDASH_CIRCLE_LENS_INCLUDED
#define CURSORDASH_CIRCLE_LENS_INCLUDED
float2 CDCircleLens(float2 uv,float4 hit,float age,float recovery,float2 aspect)
{
 float2 d=(uv-hit.xy)*aspect;float r=length(d);
 float fall=saturate(1-r/max(.0001,hit.z));
 float t=saturate(age);float kick=pow(1-sqrt(saturate(1-(1-t)*(1-t))),max(.5,recovery)*.5);
 // Displacement approaches zero at the impact center, avoiding a pinched/tearing radial singularity.
 return d/aspect*(4/max(.0001,hit.z)*kick*fall*fall*(3-2*fall)*hit.w);
}
float2 CDCircleBorder(float2 uv,float2 offset,float cap)
{
 float edge=min(min(uv.x,1-uv.x),min(uv.y,1-uv.y));
 return clamp(offset,-cap,cap)*smoothstep(0,.09,edge);
}
#endif
