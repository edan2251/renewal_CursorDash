Shader "CursorDash/CircuitPath"
{
 // Coverage/energy union in a transparent circuit-only target. Overlapping strips never add opacity.
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off ZTest Always
  Blend One One
  BlendOp Max
  Pass {CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   #include "FeverFlow.cginc"
   struct V {float4 vertex:POSITION;float4 color:COLOR;float3 edge:TEXCOORD0;float2 viewport:TEXCOORD1;};
   struct F {float4 vertex:SV_POSITION;float4 color:COLOR;float3 edge:TEXCOORD0;float2 viewport:TEXCOORD1;};
   float4 _FeverCenterAspect,_FeverFlow;float _FeverCoreRadius,_CircuitFever,_ScoreHide;sampler2D _CircuitScoreMask;
   F vert(V i){F o;o.vertex=UnityObjectToClipPos(i.vertex);o.color=i.color;o.edge=i.edge;o.viewport=i.viewport;return o;}
   fixed4 frag(F i):SV_Target{
    float d=1-abs(i.edge.y),r=length(i.edge.xy);
    if(i.edge.z>.5&&i.edge.z<1.5)d=min(1-r,i.edge.x); // Outward semicircle: no alpha overlap on the existing stroke.
    else if(i.edge.z>=1.5)d=1-r; // Flush stroke-sized junction only; no target rings/pads.
    float coverage=saturate(d/max(.00001,fwidth(d))),white=CDFMask(i.viewport,_FeverCenterAspect,_FeverFlow);
    float alpha=i.color.a*coverage*(_CircuitFever>.5?white:1-white);
    if(_CircuitFever>.5)alpha*=smoothstep(_FeverCoreRadius*.90,_FeverCoreRadius*1.04,length((i.viewport-_FeverCenterAspect.xy)*_FeverCenterAspect.zw));
    alpha*=1-saturate(tex2D(_CircuitScoreMask,i.viewport).a)*_ScoreHide;
    return fixed4(i.color.rgb*alpha,alpha);
   }
  ENDCG}
 } Fallback Off
}
