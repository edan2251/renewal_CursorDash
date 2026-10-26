Shader "CursorDash/CircuitScoreGlyphMask"
{
 Properties {_MainTex("Actual score atlas",2D)="black"{} _Coverage("Presentation handoff only",Float)=1}
 SubShader {Cull Off ZWrite Off ZTest Always Blend One One BlendOp Max
  Pass {CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _FontWeight,_GlyphAdjust;float _IsSDF,_Coverage;
   struct V {float4 vertex:POSITION;float4 uv:TEXCOORD0;};
   struct F {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float threshold:TEXCOORD1;};
   F vert(V i){F o;i.vertex.xy+=_GlyphAdjust.zw;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv.xy;
    // Match the shipped TMP_SDF face contour. Display vertex/face alpha is deliberately not a mask input.
    float weight=(lerp(_FontWeight.x,_FontWeight.y,step(i.uv.w,0))/4+_FontWeight.z)*_FontWeight.w*.5;
    o.threshold=.5-weight;return o;
   }
   fixed4 frag(F i):SV_Target{
    float sdf=tex2D(_MainTex,i.uv).a;float pixels=(sdf-(_IsSDF>.5?i.threshold:.5))/max(fwidth(sdf),.00001);
    float edge=max(.25,_GlyphAdjust.y),coverage=smoothstep(-edge*.5,edge*.5,pixels+_GlyphAdjust.x);
    coverage*=saturate(_Coverage);return fixed4(coverage,coverage,coverage,coverage);
   }
  ENDCG}
 } Fallback Off
}
