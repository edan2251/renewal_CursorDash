Shader "CursorDash/NodeFlat"
{
 Properties { _Color("Flat body",Color)=(1,0.5,1,1) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   struct v2f { float4 pos:SV_POSITION;float2 p:TEXCOORD0; };
   float4 _Color,_Polygon[6];
   float _Hex,_Radius,_Charge,_Energy,_Piston,_Body,_Ring,_Angle,_Stroke,_Core,_Glow,_FillScale,_ChargePulse;
   v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.p=v.vertex.xy;return o;}
   float polygonDistance(float2 p){
    float dist=100,side=1;
    for(int j=0;j<6;j++){
     float2 a=_Polygon[j].xy,b=_Polygon[j==5?0:j+1].xy,e=b-a,w=p-a;
     float2 closest=w-e*saturate(dot(w,e)/max(.000001,dot(e,e)));
     dist=min(dist,dot(closest,closest));side=min(side,e.x*w.y-e.y*w.x);
    }
    return sqrt(dist)*(side>=0?-1:1);
   }
   float sdf(float2 p){return _Hex>.5?polygonDistance(p):length(p)-_Radius;}
   fixed4 frag(v2f i):SV_Target {
    float2 p=i.p;float aa=max(fwidth(p.x),fwidth(p.y))*1.1;
    float d=sdf(p),outer=1-smoothstep(-aa,aa,abs(d)-_Stroke*.5);
    float fill=(1-smoothstep(-aa,aa,sdf(p/max(.0001,_Charge*_FillScale))))*step(.0001,_Charge);
    if(_Piston>.5){
     float back=min(_Polygon[3].x,_Polygon[4].x),front=(_Polygon[0].x+_Polygon[1].x)*.5;
     float cut=lerp(front+aa,back-aa,_Energy);
     fill=(1-smoothstep(-aa,aa,d))*smoothstep(-aa,aa,p.x-cut)*step(.0001,_Energy);
    }
    float core=step(.0001,_Core)*(1-smoothstep(-aa,aa,length(p)-_Core));
    float body=max(outer,max(fill,core))*_Body;
    float r=length(p),angle=atan2(p.y,p.x)+_Angle;
    float ring=(1-smoothstep(-aa,aa,abs(r-(_Radius+.19)*_Ring)-_Stroke*.55))*step(.001,_Ring);
    ring*=smoothstep(-.55,-.25,sin(angle*3));
    if(_Hex>.5){
     float turn=_Angle*_Ring;float2 rp=float2(p.x*cos(turn)-p.y*sin(turn),p.x*sin(turn)+p.y*cos(turn));
     float scale=1+.35*_Ring;
     float rd=polygonDistance(rp/scale)*scale;
     ring=(1-smoothstep(-aa,aa,abs(rd)-_Stroke*.55))*saturate(_Ring*6);
    }
    float glow=exp(-abs(d)*35)*_Glow*_Body;
    float alpha=saturate(max(body,ring)+glow);
    // Core is off by default. An explicitly imported legacy visual.core value remains adjustable.
    float3 rgb=lerp(_Color.rgb,1,core*_Body*.85);
    // A brief response belongs only to the charged interior, never the outline or rotating rings.
    float chargeLight=fill*(1-outer)*step(.5,_Hex)*(1-step(.5,_Piston))*_ChargePulse*.12;
    rgb=lerp(rgb,float3(1,.98,.76),saturate(chargeLight));
    return float4(rgb,alpha*_Color.a);
   }
   ENDCG
  }
 }
 Fallback Off
}
