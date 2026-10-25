#ifndef CURSORDASH_WORLD_IMPACT_INCLUDED
#define CURSORDASH_WORLD_IMPACT_INCLUDED
#include "CircleLens.cginc"
float4 _Hits[8],_Shapes[8],_Responses[8],_Aspect;
float _Clock,_OffsetCap,_SuccessActive;
float3 CDWorldResponse(float2 uv)
{
 float2 offset=0;float light=0;
 for(int n=0;n<8;n++){
  float4 h=_Hits[n];float2 d=(uv-h.xy)*_Aspect.xy;
  float r=length(d);float fall=saturate(1-r/max(.0001,h.z));
  float4 s=_Shapes[n];
  if(s.x>1.5){offset+=CDCircleLens(uv,h,s.y,s.z,_Aspect.xy);continue;}
  float wave=sin(r*65-_Clock*24)*fall*fall;
  if(s.x>.5){
   float4 response=_Responses[n];float age=saturate(s.y);
   float kick=pow(1-sqrt(saturate(1-(1-age)*(1-age))),max(.5,s.z)*.5);
   float recoil=-.16*exp(-pow((age-.45)/.16,2));
   float front=age*s.w*response.y;
   float width=max(.05,response.z),ring=exp(-pow((r-front)/width,2));
   float ripple=sin((r-front)*6.2831853/width)*ring*pow(1-age,.7)*response.x;
   wave=(kick+recoil)*fall*fall*(3-2*fall)+ripple*(.35+.65*fall);
   light=max(light,response.w*(.5*pow(1-age,1.3)+.7*ring*fall*pow(1-age,.7)));
  }
  offset+=d/max(.0001,r)/_Aspect.xy*(wave*h.w);
 }
 offset=clamp(offset,-_OffsetCap,_OffsetCap);
 if(_SuccessActive>.5){float edge=min(min(uv.x,1-uv.x),min(uv.y,1-uv.y));offset*=smoothstep(0,.09,edge);}
 return float3(offset,light);
}
float3 CDWorldLight(float3 color,float light)
{
 float lineLevel=smoothstep(.01,.18,dot(color,float3(.2126,.7152,.0722)));
 light=min(.8,light);
 return color*(1+light*1.2)+light*(.035+lineLevel*.15)*float3(.55,.8,1);
}
#endif
