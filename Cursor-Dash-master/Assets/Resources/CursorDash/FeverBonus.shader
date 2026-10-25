Shader "CursorDash/FeverBonus"
{
 Properties { _Invert("Dark body",Float)=0 _Opacity("Opacity",Float)=1 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   struct Input {float4 vertex:POSITION;};struct Output {float4 vertex:SV_POSITION;float2 p:TEXCOORD0;};
   float _Invert,_Opacity;
   Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.p=v.vertex.xy;return o;}
   fixed4 frag(Output i):SV_Target {float r=length(i.p),aa=max(fwidth(r),.001);float alpha=saturate((1-r)/aa);float rim=smoothstep(.77,.81,r);float body=lerp(.014,.032,_Invert),edge=.006;return fixed4(lerp(body,edge,rim).xxx,alpha*_Opacity);}
   ENDCG
  }
 }
 Fallback Off
}
