#ifndef CURSORDASH_FEVER_FLOW
#define CURSORDASH_FEVER_FLOW
float CDFMerge(float a,float b,float k){float h=saturate(.5+.5*(b-a)/k);return lerp(b,a,h)-k*h*(1-h);}
float CDFFlow(float2 p,float t,float e){
 e=max(.001,e);t=saturate(t);float d=length(p)-e*(.015+1.12*pow(t,2.2));
 [unroll]for(int j=0;j<5;j++){
  float a=.38+j*6.283185307/5;float2 axis=float2(cos(a),sin(a)),side=float2(-axis.y,axis.x);
  float smoothT=t*t*(3-2*t),travel=e*.49*smoothT,bend=e*.11*sin(t*3.141592654+j*1.1)*smoothT;
  float2 delta=p-axis*travel-side*bend;float2 q=float2(dot(delta,axis)*.90,dot(delta,side)*1.06);
  float radius=e*(.02+.64*pow(saturate(t*(.90+.18*sin(j*1.7+.6))),.78));d=CDFMerge(d,length(q)-radius,e*.10);
 }return d;
}
float4 _RecallSettings;
float CDFRecallProgress(float phase){return saturate(phase);}
float CDFRecallTravel(float u){return .04*u+.20*u*u+.76*pow(u,_RecallSettings.x>0?_RecallSettings.x:3.2);}
float CDFExitFlow(float2 p,float phase,float extent){
 float u=CDFRecallProgress(phase),remaining=1-CDFRecallTravel(u);if(remaining<=.00001)return max(.001,extent);
 float a=_RecallSettings.y*CDFRecallTravel(u)*(.35+.65*saturate(length(p)/max(.001,extent))),c=cos(a),s=sin(a);float2 q=float2(p.x*c-p.y*s,p.x*s+p.y*c)/remaining;
 return CDFFlow(q,.94,extent*1.025)*remaining;
}
// Inverse sampling moves the existing circuit details INWARD with the same progress as nodes/boundary.
float2 CDFRecallCircuitUV(float2 uv,float4 centerAspect,float4 flow){
 if(flow.x!=3||flow.w<=0)return uv;
 float2 p=(uv-centerAspect.xy)*centerAspect.zw;float travel=CDFRecallTravel(saturate(flow.w));
 float a=_RecallSettings.y*travel*(.35+.65*saturate(length(p)/max(.001,flow.z))),c=cos(a),s=sin(a);
 float2 q=float2(p.x*c-p.y*s,p.x*s+p.y*c)/max(.15,1-saturate(_RecallSettings.z)*travel);
 return centerAspect.xy+q/max(.001,centerAspect.zw);
}
float CDFMask(float2 uv,float4 centerAspect,float4 flow){
 if(flow.x<.5||flow.y<=.00001)return 0;if(flow.x==2||flow.y>=.99999)return 1;
 float2 p=(uv-centerAspect.xy)*centerAspect.zw;float field=flow.x==3?CDFExitFlow(p,flow.w,flow.z):CDFFlow(p,flow.y,flow.z);
 return 1-smoothstep(-.006,.006,field);
}
// Shared by the board/mask and the foreground head: no bonus-only coordinate displacement.
float4 _ContactPulses[6];float _ContactWarp;
float2 CDFContactOffset(float2 uv,float4 centerAspect,float4 flow){
 if(flow.x!=2||_ContactWarp<=0)return 0;float2 bend=0;
 [unroll]for(int n=0;n<6;n++)if(_ContactPulses[n].w>0){
  float2 d=(uv-_ContactPulses[n].xy)*centerAspect.zw;float r=length(d),age=_ContactPulses[n].z;
  float pressure=exp(-r*r/.00055)*pow(1-age,3);bend+=d/max(.005,r)*pressure*.00065;
 }
 return clamp(bend*_ContactWarp,-.002,.002)/max(.001,centerAspect.zw);
}
#endif
