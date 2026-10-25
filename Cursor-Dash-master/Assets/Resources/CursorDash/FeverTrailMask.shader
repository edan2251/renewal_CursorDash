Shader "CursorDash/FeverTrailMask"
{
 SubShader {Tags {"Queue"="Transparent"} Cull Off ZWrite Off ZTest Always Blend One OneMinusSrcAlpha
  Pass {CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct Input{float4 vertex:POSITION;fixed4 color:COLOR;};
   struct Output{float4 vertex:SV_POSITION;fixed4 color:COLOR;};
   Output vert(Input i){Output o;o.vertex=UnityObjectToClipPos(i.vertex);o.color=i.color;return o;}
   fixed4 frag(Output i):SV_Target{float a=saturate(i.color.a);return fixed4(a,a,a,a);}
  ENDCG}
 } Fallback Off
}
