using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Owns only its own event effects. Game counts/damage are committed by StageManager first.</summary>
public class HitFeedback : MonoBehaviour
{
    public struct EventInfo { public int sequence; public string kind,cause; public Vector3 point; public Vector2 incoming; public float gameTime,realTime;public bool confirmedDamage,protectedContact; }
    private readonly List<EventInfo> recent=new List<EventInfo>();
    private readonly List<MonoBehaviour> fragments=new List<MonoBehaviour>();
    private StageManager stage;
    private CameraShake shake;
    private CameraFlash flash;
    private LocalImpactDistortion distortion;
    private Material sourceMaterial;
    private int sequence;
    public event Action<EventInfo> Signaled;
    public IReadOnlyList<EventInfo> Recent=>recent;
    public LocalImpactDistortion Distortion=>distortion;
    public int FragmentBursts=>fragments.Count;
    public bool HasLiveFragments {get{foreach(var effect in fragments)if(effect!=null&&effect.gameObject.activeInHierarchy)return true;return false;}}
    private void Awake()
    {
        stage=GetComponent<StageManager>();shake=GetComponent<CameraShake>();flash=GetComponent<CameraFlash>();
        if(Camera.main!=null)distortion=Camera.main.gameObject.AddComponent<LocalImpactDistortion>();
        var player=UnityEngine.Object.FindFirstObjectByType<PlayerCharacterController>();
        var sr=player==null?null:player.GetComponentInChildren<SpriteRenderer>();
        if(sr!=null)sourceMaterial=sr.sharedMaterial;
    }
    public void Emit(string kind,Vector3 point,Vector3[] triangle=null,Color? triangleTint=null,Vector2 incoming=default)
    { EmitCore(kind,point,triangle,triangleTint,incoming,null,false,null); }
    public void EmitCircle(Vector3 point,Vector3[] shape,Vector2 incoming,GameObject sheet)
    { EmitCore("circleRemove",point,shape,null,incoming,sheet,false,"A_cursorCircle"); }
    public void EmitNexus(NexusImpactVfx.Cause cause,Vector2 point,Vector2 incoming,Color tint,float circleLife)
    {EmitNexusCommitted(cause,point,incoming,tint,circleLife,false);}
    public void EmitNexusCommitted(NexusImpactVfx.Cause cause,Vector2 point,Vector2 incoming,Color tint,float circleLife,bool confirmedDamage,bool protectedContact=false)
    {
        bool typed=cause!=NexusImpactVfx.Cause.Manual;
        EmitCore("nexusDamage",point,null,null,incoming,null,typed,cause==NexusImpactVfx.Cause.Circle?"C_circleNexus":cause==NexusImpactVfx.Cause.Laser?"D_laserNexus":"manual",confirmedDamage,protectedContact);
        if(!typed||!CursorDashTuning.Toggle("fx.enabled",true)||!CursorDashTuning.Toggle("fx.particles",true))return;
        fragments.RemoveAll(f=>f==null);if(fragments.Count>=CursorDashTuning.Integer("fx.burstCap",16))return;
        var burst=new GameObject("Nexus contact "+cause){hideFlags=HideFlags.DontSave}.AddComponent<NexusImpactVfx>();
        burst.InitializeContact(cause,point,incoming,tint,circleLife);fragments.Add(burst);
    }
    public void EmitFinal(Vector2 point,Vector2 incoming)=>EmitFinalCommitted(point,incoming,false);
    public void EmitFinalCommitted(Vector2 point,Vector2 incoming,bool confirmedDamage)
    {
        var evt=new EventInfo{sequence=++sequence,kind="nexusFinal",cause="final",point=point,incoming=incoming,gameTime=Time.time,realTime=Time.unscaledTime,confirmedDamage=confirmedDamage};
        recent.Add(evt);if(recent.Count>32)recent.RemoveAt(0);Signaled?.Invoke(evt);
        if(!CursorDashTuning.Toggle("fx.enabled",true))return;
        if(confirmedDamage){
            var profile=stage.Fever?.Visuals;
            shake?.Viberate(CursorDashTuning.FeedbackNumber("fx.nexusDamage.shake",.10f)*CursorDashTuning.Number("user.shake",1),profile==null?.48f:Mathf.Clamp(profile.nexusDamageSeconds,.1f,.5f),CursorDashTuning.FeedbackNumber("fx.nexusDamage.shakeDecay",1.8f));
            DamageSignal(point);stage.Fever?.Circuits?.NexusDamaged(true);
        }
        else {shake?.Kick(incoming,.16f*CursorDashTuning.Number("user.shake",1),.24f,1.8f,26,.35f);flash?.Flash(Color.white,.28f*CursorDashTuning.Number("user.flash",1),.18f);distortion?.Add(point,.018f,.24f,.30f);}
        GameManager.instance.soundManager.PlaySFX(ClipTag.coreDamaged,CursorDashTuning.Number("fx.nexusDamage.volume",.8f),CursorDashTuning.Number("fx.nexusDamage.pitch",1));
    }
    private void DamageSignal(Vector2? contact=null)
    {
        // Every confirmed impact, including lethal HP loss, uses this one profile and spatial response.
        var profile=stage.Fever?.Visuals;float duration=profile==null?.48f:Mathf.Clamp(profile.nexusDamageSeconds,.1f,.5f);
        flash?.ConfirmedDamage(profile==null?new Color(1,.04f,.08f):profile.nexusDamageTint,(profile==null?0:profile.nexusDamageFlash)*Mathf.Clamp01(CursorDashTuning.Number("user.flash",1)),profile==null?.10f:Mathf.Clamp(profile.nexusDamageFlashSeconds,.04f,.2f));
        stage.Fever?.World?.NexusDamaged(duration,true,contact);
    }
    public void ConfirmedNexusDamage(Vector2 point){if(CursorDashTuning.Toggle("fx.enabled",true))DamageSignal(point);}
    private void EmitCore(string kind,Vector3 point,Vector3[] triangle,Color? triangleTint,Vector2 incoming,GameObject sheet,bool skipParticles,string cause,bool confirmedDamage=false,bool protectedContact=false)
    {
        bool success=kind=="circleRemove"||kind=="triangleBreak";
        if(success&&incoming.sqrMagnitude<.0001f)incoming=Vector2.up;
        var evt=new EventInfo{sequence=++sequence,kind=kind,cause=cause??(kind=="triangleBreak"?"B_hexReturn":kind),point=point,incoming=incoming,gameTime=Time.time,realTime=Time.unscaledTime,confirmedDamage=confirmedDamage,protectedContact=protectedContact};
        recent.Add(evt);if(recent.Count>32)recent.RemoveAt(0);Signaled?.Invoke(evt);
        if(!CursorDashTuning.Toggle("fx.enabled",true))return;
        string prefix="fx."+kind+".";
        // Success effects have authoritative numeric defaults in the tool metadata. A circle now also
        // supplies a silhouette; its existence must never change the world-size default from .05 to 1.
        float Read(string suffix,float fallback) {
            var definition=CursorDashTuning.Find(prefix+suffix);
            return CursorDashTuning.FeedbackNumber(prefix+suffix,success&&definition!=null?(float)definition.defaultValue:fallback);
        }
        float stop=Read("stop",kind=="hexBlock"?0:kind=="triangleBreak"?.05f:kind=="circleRemove"?.04f:.08f);
        stage.RequestFeedbackStop(stop);
        float amount=Read("shake",kind=="hexBlock"?.018f:kind=="nexusDamage"?.10f:kind=="triangleBreak"?.065f:.055f);
        float shakeAmount=amount*CursorDashTuning.Number("user.shake",1),shakeDuration=Read("shakeDuration",.14f),shakeDecay=Read("shakeDecay",1.8f);
        if(kind=="nexusDamage"&&confirmedDamage)shakeDuration=stage.Fever?.Visuals==null?.24f:Mathf.Clamp(stage.Fever.Visuals.nexusDamageSeconds,.1f,.5f);
        if(success)shake?.Kick(incoming,shakeAmount,shakeDuration,shakeDecay,
            Read("frequency",26),Read("rebound",.35f));
        else shake?.Viberate(shakeAmount,shakeDuration,shakeDecay);
        float strength=Read("distortion",kind=="hexBlock"?.002f:kind=="triangleBreak"?.01f:.008f);
        if(kind=="circleRemove")distortion?.AddSuccess(point,strength,Read("radius",.14f),Read("duration",.18f),Read("recovery",2));
        else if(success)distortion?.AddSuccess(point,strength,Read("radius",.14f),Read("duration",.18f),Read("recovery",2));
        else if(kind!="nexusDamage"||!(confirmedDamage||protectedContact))distortion?.Add(point,strength,CursorDashTuning.Number(prefix+"radius",.14f),CursorDashTuning.Number(prefix+"duration",.18f));
        float opacity=Read("flash",0)*CursorDashTuning.Number("user.flash",1);
        // Red/optics came exactly once from the authoritative HP-decrease notification.
        if(!(kind=="nexusDamage"&&(confirmedDamage||protectedContact))&&opacity>0)flash?.Flash(kind=="hexBlock"?Color.yellow:Color.white,opacity,CursorDashTuning.Number(prefix+"duration",.18f));
        ClipTag clip=kind=="hexBlock"?ClipTag.arrowDefence:kind=="nexusDamage"?ClipTag.coreDamaged:ClipTag.nodeExplosion;
        GameManager.instance.soundManager.PlaySFX(clip,CursorDashTuning.Number(prefix+"volume",.8f),CursorDashTuning.Number(prefix+"pitch",1));
        if(skipParticles)return;
        fragments.RemoveAll(f=>f==null);
        int cap=CursorDashTuning.Integer("fx.burstCap",16);
        if(!CursorDashTuning.Toggle("fx.particles",true)||fragments.Count>=cap||sourceMaterial==null)return;
        Color color=CursorDashTuning.FeedbackColorValue(prefix+"color",triangleTint??(kind=="hexBlock"?new Color(1,.85f,.2f):kind=="nexusDamage"?new Color(.1f,.8f,1):new Color(.75f,.3f,1)));
        int count=Mathf.RoundToInt(Read("count",kind=="hexBlock"?4:6));
        float size=Read("size",kind=="circleRemove"?.05f:triangle==null?.05f:1);
        var go=new GameObject("Impact "+kind){hideFlags=HideFlags.DontSave};
        if(kind=="circleRemove"){
            var circle=go.AddComponent<CircularBreakVfx>();
            circle.Initialize(point,triangle,incoming,count,size,Read("speed",1.5f),Read("life",.3f),color,sourceMaterial,Read("spread",65),Read("spin",180),sheet);
            fragments.Add(circle);return;
        }
        var burst=go.AddComponent<ImpactFragments>();
        if(success)burst.InitializeDirectional(point,triangle,incoming,count,size,Read("speed",1.5f),
            Read("life",.3f),color,sourceMaterial,Read("spread",65),Read("spin",180),kind=="circleRemove");
        else burst.Initialize(point,triangle,count,size,CursorDashTuning.Number(prefix+"speed",1.5f),CursorDashTuning.Number(prefix+"life",.3f),color,sourceMaterial);
        fragments.Add(burst);
    }
    public void Clear()
    {
        distortion?.Clear();shake?.Reset();flash?.ResetFlash();
        foreach(var f in fragments)if(f!=null)Destroy(f.gameObject);fragments.Clear();
    }
    private void OnDestroy()=>Clear();
}
