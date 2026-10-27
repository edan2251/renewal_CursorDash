Shader "CursorDash/GuideInk"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white" {} _Color("Guide ink",Color)=(.76,.79,.82,1) _GuideSketch("Fixed ink variation",Range(0,1))=.15 }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "CanUseSpriteAtlas"="True" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;float4 color:COLOR;};
   struct output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;float alpha:COLOR;float2 local:TEXCOORD1;};
   sampler2D _MainTex;float4 _Color,_MainTex_TexelSize;float _GuideSketch;
   output vert(input v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.alpha=v.color.a;o.local=v.vertex.xy;return o;}
   fixed4 frag(output i):SV_Target{
    float2 uv=i.uv+float2(sin(i.local.y*79+i.local.x*17),sin(i.local.x*61-i.local.y*23))*_MainTex_TexelSize.xy*_GuideSketch*.8;
    float alpha=tex2D(_MainTex,uv).a*i.alpha*_Color.a;
    float edge=saturate(fwidth(alpha)*5+.12);
    float variation=.5+.5*sin(i.local.x*47+i.local.y*31)*sin(i.local.y*79-i.local.x*19);
    return float4(_Color.rgb*(1-_GuideSketch*edge*variation),alpha);
   }
   ENDCG
  }
 }
 Fallback Off
}
