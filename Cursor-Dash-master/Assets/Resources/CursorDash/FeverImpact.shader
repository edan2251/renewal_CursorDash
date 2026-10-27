Shader "CursorDash/FeverImpact"
{
 Properties { _MainTex("Completed gameboard including cursor",2D)="white"{} _DamageTint("Damage wave tint",Color)=(1,.04,.08,1) _ScoreChromaticGain("Fever score RGB gain",Float)=1 }
 SubShader {Cull Off ZWrite Off ZTest Always
  Pass {CGPROGRAM
   #pragma vertex vert_img
   #pragma fragment frag
   #pragma target 3.0
   #include "UnityCG.cginc"
   sampler2D _MainTex,_DamageTextMask;float4 _LensAspect,_Optics,_OpticalPulses[6],_ExitOptics,_ExitCenter,_NexusDamage,_DamageField,_DamageCore,_DamageShape,_DamageNexusCenter,_DamageTint;float _LensRadius,_DamageLightUser,_ScoreChromaticGain;
   fixed4 frag(v2f_img i):SV_Target {
    float2 aspect=max(.001,_LensAspect.xy),bend=0;float strongest=0,inkSplit=0,damageLight=0;
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
    // Fever RGB also samples the score glyphs. The damage-only text guard below does not affect it.
    float split=min(.012,(_Optics.z+_Optics.y*inkSplit)*_Optics.w);
    // Reuse the real, current glyph mask; stronger score fringes do not require stronger combat/HUD distortion.
    split=min(.024,split*lerp(1,max(1,_ScoreChromaticGain),saturate(tex2D(_DamageTextMask,i.uv).a)));
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
    if(_NexusDamage.w>0){
     float2 delta=(i.uv-_NexusDamage.xy)*aspect;float radius=length(delta),age=saturate(_NexusDamage.z);
     float2 axis=delta/max(.006,radius);
     float front=_DamageCore.x+min(_DamageField.w,age*_DamageCore.z*_DamageField.z);
     float width=max(.03,_DamageField.y),x=(radius-front)/width;
     float envelope=pow(1-age,_DamageShape.y),band=exp(-x*x)*envelope;
     // One compression then one outward release. No colored line/ring or repeated sinusoidal vibration.
     float core=exp(-pow(radius/max(.08,_DamageCore.x+width*2),2))*exp(-age*15)*_DamageShape.x;
     float release=-x*exp(-x*x)*envelope;
     // Protect only the solid core/actual score strokes. The former broad core guard hid most initial pressure.
     float coreDistance=length((i.uv-_DamageNexusCenter.xy)*aspect);
     float coreGuard=_DamageNexusCenter.z>.5?smoothstep(_DamageCore.x*.70,_DamageCore.x*1.10,coreDistance):smoothstep(_DamageCore.x*.88,_DamageCore.x*1.65,radius);
     float protect=coreGuard*(1-saturate(tex2D(_DamageTextMask,i.uv).a));
     float kick=exp(-age*10)*exp(-radius*radius/.16);
     float2 wave=axis*(-core-kick*.55+release*2.1)*_DamageField.x*_NexusDamage.w*protect;float damageOffsetCap=_DamageNexusCenter.z>.5?.035:.018;wave*=min(1,damageOffsetCap/max(.000001,length(wave)));
     // Strongest field wins. Texture displacement affects the actual background/circuit content, never draws geometry.
     if(dot(wave,wave)>dot((uv-i.uv)*aspect,(uv-i.uv)*aspect))uv=clamp(i.uv+wave/aspect,.0002,.9998);
     float splitDamage=min(_DamageNexusCenter.z>.5?.024:.006,_DamageCore.y*(core*.25+band*.75)*(1-age)*_NexusDamage.w*protect);
     if(splitDamage>length(chroma*aspect))chroma=axis*splitDamage/aspect;
     // Broad red light follows the SAME moving pressure field, not a drawn red ring or screen-wide wash.
     float lightLocality=1-smoothstep(1.25,2,abs(x));
     damageLight=min(.85,_DamageShape.z*(core*.15+band+kick*.12)*protect*_DamageLightUser)*lightLocality;
    }
    fixed4 center=tex2D(_MainTex,uv);
    float red=tex2D(_MainTex,clamp(uv+chroma,.0002,.9998)).r;
    float blue=tex2D(_MainTex,clamp(uv-chroma,.0002,.9998)).b;
    // Actual channel sampling: uniform white stays white and black interiors stay black; edges split in color.
    float lineLevel=smoothstep(.01,.18,dot(float3(red,center.g,blue),float3(.2126,.7152,.0722)));
    float3 color=float3(red,center.g,blue);
    color+=damageLight*(.08+lineLevel*.30)*_DamageTint.rgb;
    return fixed4(color,center.a);
   }
  ENDCG}
 } Fallback Off
}
