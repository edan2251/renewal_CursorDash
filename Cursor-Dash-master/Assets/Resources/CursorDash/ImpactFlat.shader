Shader "CursorDash/ImpactFlat"
{
 Properties { _MainTex("Unused flat texture",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent"}
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   struct ParticleInput {float4 vertex:POSITION;float4 color:COLOR;float4 edges:TEXCOORD1;};
   struct ParticleOutput {float4 position:SV_POSITION;float4 color:COLOR;float4 edges:TEXCOORD0;};
   float4 _Color;
   ParticleOutput vert(ParticleInput v){ParticleOutput o;o.position=UnityObjectToClipPos(v.vertex);o.color=v.color*_Color;o.edges=v.edges;return o;}
   fixed4 frag(ParticleOutput i):SV_Target {
    // Distances to the polygon's outer edges, not to the triangle fan's internal diagonals.
    float d=min(min(i.edges.x,i.edges.y),min(i.edges.z,i.edges.w));
    float coverage=saturate(d/max(.00001,fwidth(d)));
    return fixed4(i.color.rgb,i.color.a*coverage);
   }
   ENDCG
  }
 }
 Fallback Off
}
