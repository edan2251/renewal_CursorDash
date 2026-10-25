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
   struct v2f { float4 pos:SV_POSITION;float4 color:COLOR;float coverage:TEXCOORD0; };
   float4 _Color;
   v2f vert(appdata v) {
    v2f o;o.pos=UnityObjectToClipPos(v.vertex);
    float4 along=UnityObjectToClipPos(v.vertex+float4(v.miter,0,0));
    float2 pixels=(along.xy/along.w-o.pos.xy/o.pos.w)*_ScreenParams.xy;
    float lengthPixels=length(pixels);
    if(lengthPixels>.00001)o.pos.xy+=pixels/lengthPixels*v.edge.y*length(v.miter)*2/_ScreenParams.xy*o.pos.w;
    o.color=v.color*_Color;o.coverage=v.edge.x;return o;
   }
   fixed4 frag(v2f i):SV_Target {return float4(i.color.rgb,i.color.a*saturate(i.coverage));}
   ENDCG
  }
 }
 Fallback Off
}
