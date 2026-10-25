Shader "CursorDash/CursorForeground"
{
 Properties { _MainTex("Board with nexus",2D)="black"{} _CursorTex("Actual cursor render",2D)="black"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   #include "WorldImpact.cginc"
   #include "FeverFlow.cginc"
   sampler2D _MainTex,_CursorTex;
   float4 _FeverCenterAspect,_FeverFlow;
   float4 _CursorTex_TexelSize;float _FeverCoreRadius;
   fixed4 frag(v2f_img i):SV_Target {
    float2 headUV=clamp(i.uv+CDFContactOffset(i.uv,_FeverCenterAspect,_FeverFlow),.001,.999);
    float3 response=CDWorldResponse(headUV);
    fixed4 cursor=tex2D(_CursorTex,clamp(headUV+response.xy,.001,.999));
    // Sprite/Default already renders premultiplied RGB into the transparent RT.
    if(cursor.a>1e-6)cursor.rgb=CDWorldLight(cursor.rgb/cursor.a,response.z)*cursor.a;
    float fever=CDFMask(i.uv,_FeverCenterAspect,_FeverFlow);
    cursor.rgb=lerp(cursor.rgb,float3(.008,.01,.014)*cursor.a,fever);
    fixed4 board=tex2D(_MainTex,i.uv);
    // Actual cursor stays readable through ink shards and the nexus. The fine white edge is invisible
    // on the white background; it never changes the cursor silhouette, movement or normal mode.
    if(fever>.01){
     float2 uv=clamp(headUV+response.xy,.001,.999),px=abs(_CursorTex_TexelSize.xy)*1.25;
     float around=max(max(tex2D(_CursorTex,uv+float2(px.x,0)).a,tex2D(_CursorTex,uv-float2(px.x,0)).a),max(tex2D(_CursorTex,uv+float2(0,px.y)).a,tex2D(_CursorTex,uv-float2(0,px.y)).a));
     float edge=saturate(around-cursor.a)*fever;board.rgb=lerp(board.rgb,float3(1,1,1),edge);
    }
    return fixed4(cursor.rgb+board.rgb*(1-cursor.a),cursor.a+board.a*(1-cursor.a));
   }
   ENDCG
  }
 }
 Fallback Off
}
