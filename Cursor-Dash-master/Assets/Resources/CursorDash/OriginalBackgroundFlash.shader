Shader "CursorDash/OriginalBackgroundFlash"
{
 Properties { _MainTex("World",2D)="black"{} _FlashTex("Existing Background_Flash",2D)="black"{} }
 SubShader {
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   #include "CircleLens.cginc"
   sampler2D _MainTex,_FlashTex;
   float4 _ImageRect,_SpriteRect,_ImageColor,_Aspect,_Flashes[8],_LensHits[8],_LensShapes[8];
   float _FlashGain,_OffsetCap,_RestContrast;
   fixed4 frag(v2f_img i):SV_Target {
    float2 offset=0;
    for(int n=0;n<8;n++)if(_LensShapes[n].x>1.5)offset+=CDCircleLens(i.uv,_LensHits[n],_LensShapes[n].y,_LensShapes[n].z,_Aspect.xy);
    float2 bent=i.uv+CDCircleBorder(i.uv,offset,_OffsetCap);
    float2 local=(bent-_ImageRect.xy)/max(_ImageRect.zw,.0001);
    float inside=step(0,local.x)*step(local.x,1)*step(0,local.y)*step(local.y,1);
    fixed4 layer=tex2D(_FlashTex,_SpriteRect.xy+saturate(local)*_SpriteRect.zw)*_ImageColor;
    float glow=0;
    for(int k=0;k<8;k++){
     float4 f=_Flashes[k];float age=f.z;
     float envelope=smoothstep(0,.12,age)*pow(1-age,1.4)*f.w;
     float radius=lerp(.14,1.5,age);
     float distanceFromHit=length((bent-f.xy)*_Aspect.xy);
     glow=max(glow,envelope*exp(-pow(distanceFromHit/max(.001,radius),2)));
    }
    // Source color/alpha stay untouched. Lower only the resting decoration; keep the same transient hit increment.
    layer.a=saturate(layer.a*(_RestContrast+2*glow*_FlashGain))*inside;
    fixed4 world=tex2D(_MainTex,i.uv);
    return float4(lerp(world.rgb,layer.rgb,layer.a),world.a);
   }
   ENDCG
  }
 }
 Fallback Off
}
