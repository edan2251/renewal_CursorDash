Shader "CursorDash/CircularArtwork"
{
 Properties { _Color("Original A layer",Color)=(1,1,1,1) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   struct appdata { float4 vertex:POSITION;float4 color:COLOR;float2 miter:TEXCOORD0;float2 edge:TEXCOORD1; };
   struct v2f { float4 pos:SV_POSITION;float4 color:COLOR;float coverage:TEXCOORD0;float2 local:TEXCOORD1; };
   float4 _Color;float _GuideSketch;
   v2f vert(appdata v) {
    v2f o;o.pos=UnityObjectToClipPos(v.vertex);
    float4 along=UnityObjectToClipPos(v.vertex+float4(v.miter,0,0));
    float2 pixels=(along.xy/along.w-o.pos.xy/o.pos.w)*_ScreenParams.xy;
    float lengthPixels=length(pixels);
    if(lengthPixels>.00001)o.pos.xy+=pixels/lengthPixels*v.edge.y*length(v.miter)*2/_ScreenParams.xy*o.pos.w;
    if(lengthPixels>.00001){float ink=sin(v.vertex.x*47+v.vertex.y*31)*sin(v.vertex.y*79-v.vertex.x*19);o.pos.xy+=pixels/lengthPixels*ink*_GuideSketch*1.4/_ScreenParams.xy*o.pos.w;}
    o.color=v.color*_Color;o.coverage=v.edge.x;o.local=v.vertex.xy;return o;
   }
   fixed4 frag(v2f i):SV_Target {float ink=.5+.5*sin(i.local.x*47+i.local.y*31);return float4(i.color.rgb*(1-_GuideSketch*.12*ink),i.color.a*saturate(i.coverage));}
   ENDCG
  }
 }
 Fallback Off
}
