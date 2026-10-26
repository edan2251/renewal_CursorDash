Shader "CursorDash/FeverWorld"
{
 Properties {_MainTex("Composed board",2D)="black"{} _ElementsTex("Actual element alpha",2D)="black"{} _CircuitTex("Preserved circuit",2D)="black"{} _DotTex("Existing dotted ring",2D)="black"{}}
 SubShader {Cull Off ZWrite Off ZTest Always
  Pass {CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   #include "FeverFlow.cginc"
   #include "WorldImpact.cginc"
   sampler2D _MainTex,_ElementsTex,_CircuitTex,_DotTex;
   float4 _FeverCenterAspect,_FeverFlow,_Pulses[6],_CircuitImage,_CircuitSprite,_DotU,_DotV,_DotRect,_CoreTint;
   float _CoreRadius,_CoreVisible,_Level,_Press,_HasElements,_CircuitVisible,_DotVisible,_Light,_Warp,_FlowClock,_VisualBodyScale,_BonusGain;
   float circuit(float2 uv){
    float2 local=(uv-_CircuitImage.xy)/max(.0001,_CircuitImage.zw);float inside=step(0,local.x)*step(local.x,1)*step(0,local.y)*step(local.y,1);
    float4 c=tex2D(_CircuitTex,_CircuitSprite.xy+saturate(local)*_CircuitSprite.zw);return saturate(max(max(c.r,c.g),c.b)*c.a*2*_CircuitVisible*inside);
   }
   float dotRing(float2 uv){
    float2 d=uv-_DotU.xy,u=_DotU.zw,v=_DotV.xy;float det=u.x*v.y-u.y*v.x;if(abs(det)<1e-10)return 0;
    float2 local=float2(d.x*v.y-d.y*v.x,u.x*d.y-u.y*d.x)/det;float inside=step(0,local.x)*step(local.x,1)*step(0,local.y)*step(local.y,1);
    float bodyDistance=length((uv-_FeverCenterAspect.xy)*_FeverCenterAspect.zw)-_CoreRadius;
    return tex2D(_DotTex,_DotRect.xy+saturate(local)*_DotRect.zw).a*inside*_DotVisible*smoothstep(-.001,.001,bodyDistance);
   }
   fixed4 frag(v2f_img i):SV_Target{
    float2 sampleUV=clamp(i.uv+CDFContactOffset(i.uv,_FeverCenterAspect,_FeverFlow),.001,.999);
    float3 impact=CDWorldResponse(sampleUV);float2 boardUV=clamp(sampleUV+impact.xy,.001,.999);
    float2 aspect=_FeverCenterAspect.zw,p=(boardUV-_FeverCenterAspect.xy)*aspect;float mono=CDFMask(i.uv,_FeverCenterAspect,_FeverFlow);
    float inkPulse=0;
    [unroll]for(int j=0;j<6;j++)if(_Pulses[j].w>0){float2 d=(i.uv-_Pulses[j].xy)*aspect;float r=length(d),age=_Pulses[j].z;float pressure=exp(-r*r/.00028)*pow(1-age,3);inkPulse=max(inkPulse,pressure*.10);}
    fixed4 source=tex2D(_MainTex,sampleUV); // Same bounded local displacement as the actual foreground head.
    float radius=max(.001,_CoreRadius*_VisualBodyScale),r=length(p),aa=max(fwidth(r),.0001);float3 color=source.rgb;
    if(_FeverFlow.x>.5){
    float element=tex2D(_ElementsTex,boardUV).a*_HasElements;
    float3 whiteWorld=lerp(float3(1,1,1),float3(.80,.82,.84),circuit(CDFRecallCircuitUV(i.uv,_FeverCenterAspect,_FeverFlow)));
    whiteWorld=lerp(whiteWorld,float3(.008,.010,.014),element);
    float body=(1-smoothstep(radius-aa,radius+aa,r))*_CoreVisible;
    float water=lerp(-radius,radius,_Level)-_Press*radius*2*saturate(4*_Level*(1-_Level));
    float filled=_Level>=1?1:_Level<=0?0:1-smoothstep(water-aa,water+aa,p.y);
    float3 core=lerp(float3(.12,.13,.14),float3(.006,.008,.011),filled);
    float waterline=(_Level>.001&&_Level<.999)?exp(-abs(p.y-water)/max(.0001,radius*.022)):0;core=lerp(core,float3(.85,.86,.87),waterline*.70);
    whiteWorld=lerp(whiteWorld,core,body);whiteWorld=lerp(whiteWorld,float3(.008,.010,.014),dotRing(i.uv));
    whiteWorld*=1-min(.24,inkPulse*_BonusGain*_Light); // Bounded ink pressure, no white-on-white flashes.
    color=lerp(source.rgb,whiteWorld,mono);
    }
    float outline=(1-smoothstep(radius*.025-aa,radius*.025+aa,abs(r-radius*.975)))*_CoreVisible;
    color=lerp(color,lerp(CDWorldLight(_CoreTint.rgb,impact.z),float3(.006,.008,.011),mono),outline);
    if(_FeverFlow.x>.5&&_FeverFlow.x!=2){
     float field=_FeverFlow.x==3?CDFExitFlow(p,_FeverFlow.w,_FeverFlow.z):CDFFlow(p,_FeverFlow.y,_FeverFlow.z),edge=exp(-abs(field)/max(.006,_FeverFlow.z*.018));
     float gather=_FeverFlow.x==1?(smoothstep(.10,.52,_FeverFlow.w)*(1-smoothstep(.61,.67,_FeverFlow.w))):sin(saturate(_FeverFlow.w)*3.141592654);
     float compressed=exp(-dot(p,p)/max(.00001,radius*radius*.36))*gather;
     color=lerp(color,lerp(float3(1,1,1),float3(.02,.025,.03),mono),saturate((edge*.34+compressed*.52)*_Light));
     // Visible inward curved streaks on exit, distinct from just shrinking a mask.
     if(_FeverFlow.x==3){
      [unroll]for(int k=0;k<7;k++){
       float u=saturate((CDFRecallProgress(_FeverFlow.w)-k*.012)/(1-k*.012)),distanceIn=_FeverFlow.z*1.12*(1-CDFRecallTravel(u));
       float angle=k*2.399963+1.8*u+.18*sin(u*3.141592654+k),size=_FeverFlow.z*(.018+.055*sin(u*3.141592654));
       float2 axis=float2(cos(angle),sin(angle)),side=float2(-axis.y,axis.x),q=p-axis*distanceIn;
       float2 curved=float2(dot(q,axis)*.85,dot(q,side)*1.2)/max(.003,size);
       float packet=exp(-dot(curved,curved)*2)*sin(u*3.141592654);
       color=lerp(color,float3(.98,.99,1),packet*.75*_Light);
      }
     }
    }
    return fixed4(color,lerp(source.a,1,mono)); // The completed white world is opaque through every viewport corner.
   }
  ENDCG}
 } Fallback Off
}
