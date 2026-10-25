Shader "CursorDash/LocalImpact"
{
 Properties { _MainTex("Source",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   #include "WorldImpact.cginc"
   sampler2D _MainTex;
   fixed4 frag(v2f_img i):SV_Target {
    float3 response=CDWorldResponse(i.uv);
    fixed4 color=tex2D(_MainTex,clamp(i.uv+response.xy,.001,.999));
    color.rgb=CDWorldLight(color.rgb,response.z);
    return color;
   }
   ENDCG
  }
 }
 Fallback Off
}
