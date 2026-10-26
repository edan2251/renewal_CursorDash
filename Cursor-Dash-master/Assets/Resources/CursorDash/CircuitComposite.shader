Shader "CursorDash/CircuitComposite"
{
 Properties {_MainTex("Circuit-only premultiplied union",2D)="black"{}}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off ZTest LEqual
  Blend One OneMinusSrcAlpha
  Pass {CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_TexelSize;
   struct F {float4 vertex:SV_POSITION;float4 screen:TEXCOORD0;};
   F vert(appdata_base v){F o;o.vertex=UnityObjectToClipPos(v.vertex);o.screen=ComputeScreenPos(o.vertex);return o;}
   fixed4 frag(F i):SV_Target{float2 uv=i.screen.xy/i.screen.w;
    #if UNITY_UV_STARTS_AT_TOP
    if(_MainTex_TexelSize.y<0)uv.y=1-uv.y;
    #endif
    return tex2D(_MainTex,uv);
   }
  ENDCG}
 } Fallback Off
}
