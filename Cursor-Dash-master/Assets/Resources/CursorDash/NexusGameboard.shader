Shader "CursorDash/NexusGameboard"
{
 Properties { _MainTex("Board",2D)="black"{} _Layer0Tex("Original nexus 0",2D)="white"{} _Layer1Tex("Original nexus 1",2D)="white"{} _Layer2Tex("Original nexus 2",2D)="white"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   #include "WorldImpact.cginc"
   #include "NexusSurface.cginc"
   sampler2D _MainTex,_Layer0Tex,_Layer1Tex,_Layer2Tex;
   float4 _RectsU[3],_RectsV[3],_Tints[3],_TextureRects[3],_EnergyTint,_Bounds,_CoreCircle;
   float _Level,_SolidFraction,_ImpactPress,_VisualBodyScale,_GatherLight,_Absorbing;int _EnergyIndex;
   fixed4 frag(v2f_img i):SV_Target {
    if(i.uv.x<_Bounds.x||i.uv.y<_Bounds.y||i.uv.x>_Bounds.z||i.uv.y>_Bounds.w)return tex2D(_MainTex,i.uv);
    float3 response=CDWorldResponse(i.uv);float2 boardUV=clamp(i.uv+response.xy,.001,.999);
    float4 nexus=0;
    for(int n=0;n<3;n++){
     if(_Tints[n].a<=0)continue;
     float2 a=_RectsU[n].xy,u=_RectsU[n].zw,v=_RectsV[n].xy,d=boardUV-a;
     float det=u.x*v.y-u.y*v.x;
     if(abs(det)<1e-12)continue;
     float2 local=float2(d.x*v.y-d.y*v.x,u.x*d.y-u.y*d.x)/det;
     float inside=step(0,local.x)*step(local.x,1)*step(0,local.y)*step(local.y,1);
     if(inside<=0)continue;
     float2 texUV=_TextureRects[n].xy+saturate(local)*_TextureRects[n].zw;
     fixed4 sampleColor=n==0?tex2D(_Layer0Tex,texUV):n==1?tex2D(_Layer1Tex,texUV):tex2D(_Layer2Tex,texUV);
     float2 bodyLocal=.5+(local-.5)/max(.01,_VisualBodyScale);
     fixed4 layer=n==_EnergyIndex?CDNexusSurface(bodyLocal,_Tints[n]*_EnergyTint,_SolidFraction,_Level,_ImpactPress):sampleColor*_Tints[n];
     if(n==_EnergyIndex)layer.rgb*=1+_GatherLight*.50;
     else if(_Absorbing>.5){
      // The entire solid surface occludes absorbed decorations, even above a low HP waterline.
      float distanceToBody=length((boardUV-_CoreCircle.xy)*_Aspect.xy)-_CoreCircle.z;
      layer.a*=smoothstep(-.001,.001,distanceToBody);
     }
     layer.a*=inside;
     nexus.rgb=layer.rgb*layer.a+nexus.rgb*(1-layer.a);nexus.a=layer.a+nexus.a*(1-layer.a);
    }
    if(nexus.a>1e-6)nexus.rgb=CDWorldLight(nexus.rgb/nexus.a,response.z)*nexus.a;
    fixed4 board=tex2D(_MainTex,i.uv);
    return fixed4(nexus.rgb+board.rgb*(1-nexus.a),nexus.a+board.a*(1-nexus.a));
   }
   ENDCG
  }
 }
 Fallback Off
}
