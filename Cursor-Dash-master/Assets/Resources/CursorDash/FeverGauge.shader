Shader "CursorDash/FeverGauge"
{
 Properties {_Color("Energy tint",Color)=(.08,.94,.86,1)}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   struct V{float4 vertex:POSITION;};struct F{float4 vertex:SV_POSITION;float2 p:TEXCOORD0;float2 world:TEXCOORD1;};
   float4 _Color,_NexusWorld;float _Segment,_Start,_Head,_Tail,_Arrow,_Drain,_Thickness,_GapDegrees,_ChargePulse;
   F vert(V i){F o;o.vertex=UnityObjectToClipPos(i.vertex);o.p=i.vertex.xy;o.world=mul(unity_ObjectToWorld,i.vertex).xy;return o;}
   float cover(float d){return saturate(.5-d/max(.00001,fwidth(d)));}
   fixed4 frag(F i):SV_Target{
    float2 p=i.p;float turn=frac(atan2(p.x,p.y)/6.283185307-_Start+1),gap=_GapDegrees/360;
    float begin=_Segment/5+gap*.5,end=(_Segment+1)/5-gap*.5;
    float angularAA=max(.00001,fwidth(turn));float arc=smoothstep(begin-angularAA,begin+angularAA,turn)*(1-smoothstep(end-angularAA,end+angularAA,turn)),band=cover(abs(length(p)-1)-_Thickness*.5)*arc;
    float filled=smoothstep(_Tail-angularAA,_Tail+angularAA,turn)*(1-smoothstep(_Head-angularAA,_Head+angularAA,turn))*step(.00001,_Head);
    float alpha=band*filled;float3 tint=_Color.rgb*(1+.30*_ChargePulse);
    float head=_Drain>.5?_Tail:_Head,sector=min(4,floor(min(.99999,head)*5));
    float angle=(_Start+clamp(head,begin,end))*6.283185307;float2 radial=float2(sin(angle),cos(angle)),tangent=float2(cos(angle),-sin(angle));
    float2 d=p-radial;float along=dot(d,tangent),across=dot(d,radial);
    float arrow=cover(max(max(-.09-along,along-.10),abs(across)-(.10-along)/.19*.105))*_Arrow*step(abs(_Segment-sector),.01);
    float distanceToBody=length(i.world-_NexusWorld.xy)-_NexusWorld.z;
    float outside=smoothstep(-fwidth(distanceToBody),fwidth(distanceToBody),distanceToBody);
    return fixed4(tint,_Color.a*max(alpha,arrow)*outside*step(.00001,_Head-_Tail));
   }
  ENDCG }
 } Fallback Off
}
