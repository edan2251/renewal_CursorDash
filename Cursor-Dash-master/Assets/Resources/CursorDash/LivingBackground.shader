Shader "CursorDash/LivingBackground"
{
 SubShader {Tags {"Queue"="Background" "RenderType"="Opaque"} Cull Off ZWrite Off ZTest Always
  Pass { CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct V{float4 vertex:POSITION;};struct F{float4 vertex:SV_POSITION;float2 p:TEXCOORD0;};float _Clock,_FlowSpeed,_Mix,_Bend;float4 _HitDrift;
   F vert(V i){F o;o.vertex=UnityObjectToClipPos(i.vertex);o.p=i.vertex.xy;return o;}
   fixed4 frag(F i):SV_Target{
    float t=_Clock*_FlowSpeed;float2 p=i.p+_HitDrift.xy;
    float2 q=p+_Bend*float2(sin(p.y*.95+t*.39),cos(p.x*1.05-t*.27));
    float a=sin(q.x*1.35+q.y*.65+t*.73)+.42*sin(q.y*1.7-q.x*.58-t*.43);
    float b=cos(q.y*1.12-q.x*.76-t*.52)+.30*sin(q.x*2.0+q.y*1.15+t*.31);
    float3 navy=float3(.013,.027,.085),teal=float3(.014,.17,.205),violet=float3(.13,.032,.205);
    float3 c=lerp(navy,teal,smoothstep(-.85,1.20,a)*_Mix);c=lerp(c,violet,smoothstep(-.65,1.15,b)*_Mix*.88);
    c*=.82+.18*saturate(1-dot(p*.45,p*.45));return fixed4(c,1);
   }
  ENDCG }
 } Fallback Off
}
