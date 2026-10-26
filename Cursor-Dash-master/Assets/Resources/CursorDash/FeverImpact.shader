Shader "CursorDash/FeverImpact"
{
 Properties { _MainTex("Completed gameboard including cursor",2D)="white"{} }
 SubShader {Cull Off ZWrite Off ZTest Always
  Pass {CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _LensAspect,_Optics,_OpticalPulses[6],_ExitOptics,_ExitCenter;float _LensRadius;
   fixed4 frag(v2f_img i):SV_Target {
    float2 aspect=max(.001,_LensAspect.xy),bend=0;float strongest=0,inkSplit=0;
    [unroll]for(int n=0;n<6;n++)if(_OpticalPulses[n].w>0){
     float2 d=(i.uv-_OpticalPulses[n].xy)*aspect;float r=length(d),age=saturate(_OpticalPulses[n].z),envelope=(1-age)*(1-age);
     float core=exp(-dot(d,d)/.035),front=exp(-pow((r-(.05+age*_LensRadius))/.065,2))*sin(age*3.141592654);
     // Broad expanding lens pressure and a short LOCAL horizontal slip, never whole-screen random jitter.
     float band=exp(-pow(d.y/.035,2)-d.x*d.x/.16)*sin(age*24+_OpticalPulses[n].y*11);
     float2 candidate=(d/max(.006,r)*(-core*.65+front*.90)+float2(band*.22,0))*envelope*_Optics.x;
     float power=dot(candidate,candidate);if(power>strongest){strongest=power;bend=candidate;}
     inkSplit=max(inkSplit,saturate(max(core,front)+abs(band)*.25)*envelope);
    }
    // Strongest local field wins. Repeated contacts cannot add unbounded displacement or RGB splitting.
    bend*=_Optics.w;bend*=min(1,.018/max(.000001,length(bend)));
    float2 uv=clamp(i.uv+bend/aspect,.0002,.9998);
    float split=min(.006,(_Optics.z+_Optics.y*inkSplit)*_Optics.w);
    float2 chroma=float2(split,0)/aspect;
    if(_ExitOptics.w>0){
     float2 delta=(i.uv-_ExitCenter.xy)*aspect;float r=length(delta),pressure=_ExitOptics.w;
     float2 axis=delta/max(.006,r),side=float2(-axis.y,axis.x);
     // Same exit pressure as the ring recoil. Scaled exit time freezes under Pause; stable HUD is composed later.
     float2 shake=float2(cos(_ExitCenter.z*113),cos(_ExitCenter.z*151))*_ExitOptics.y;
     float2 pull=(axis*(-.45-.55*exp(-r*r/.12))+side*.18)*_ExitOptics.z;
     float2 offset=(pull+shake)*pressure;offset*=min(1,_ExitCenter.w/max(.000001,length(offset)));
     uv=clamp(i.uv+offset/aspect,.0002,.9998);chroma=(axis*.85+float2(.15,0))*min(.006,_ExitOptics.x*pressure)/aspect;
    }
    fixed4 center=tex2D(_MainTex,uv);
    float red=tex2D(_MainTex,clamp(uv+chroma,.0002,.9998)).r;
    float blue=tex2D(_MainTex,clamp(uv-chroma,.0002,.9998)).b;
    // Actual channel sampling: uniform white stays white and black interiors stay black; edges split in color.
    return fixed4(red,center.g,blue,center.a);
   }
  ENDCG}
 } Fallback Off
}
