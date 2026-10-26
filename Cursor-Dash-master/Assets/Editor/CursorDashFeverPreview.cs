using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Explicit EditMode GPU diagnostic of the actual C shader on an old capture, not live gameplay.</summary>
public static class CursorDashFeverPreview
{
    [Serializable] private class Frame {public string name;public int mode;public float progress,spread;}
    [Serializable] private class Report {
        public string method="Isolated transition FIELD GPU diagnostic on an old capture. No element mask/central text/live C gameplay. Full fever is white by design in this mask-only diagnostic.";
        public string device,beforeScenes,afterScenes;public bool supported,playingBefore,playingAfter,sceneStateUnchanged;
        public string[] shaderMessages;public Frame[] frames;
    }
    private static string Scenes()=>string.Join(";",Enumerable.Range(0,SceneManager.sceneCount).Select(i=>{var s=SceneManager.GetSceneAt(i);return s.path+"|dirty="+s.isDirty;}));
    [MenuItem("Tools/CursorDash Graphics/9 Fever C GPU Preview (No Play)")]
    public static void Capture()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||
            !string.Equals(Path.GetFullPath(Application.dataPath),@"D:\renewal_CursorDash\Cursor-Dash-master\Assets",StringComparison.OrdinalIgnoreCase)){
            Debug.LogWarning("Fever C GPU preview not run: requires idle EditMode in the intended project.");return;
        }
        string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Logs/fever-contact-polish-20261026/GPU"));Directory.CreateDirectory(folder);
        var report=new Report{device=SystemInfo.graphicsDeviceType.ToString(),playingBefore=EditorApplication.isPlaying,beforeScenes=Scenes()};
        Shader shader=Resources.Load<Shader>("CursorDash/FeverWorld");report.supported=shader!=null&&shader.isSupported;
        report.shaderMessages=shader==null?new[]{"Missing shader"}:ShaderUtil.GetShaderMessages(shader).Select(m=>m.severity+": "+m.message+" ("+m.line+")").ToArray();
        var frames=new[]{
            new Frame{name="00-source",mode=1,progress=0},
            new Frame{name="01-gather",mode=1,progress=.58f},new Frame{name="02-release-start",mode=1,progress=.66f},
            new Frame{name="03-release-early",mode=1,progress=.68f},new Frame{name="04-release-mid",mode=1,progress=.72f},
            new Frame{name="05-release-late",mode=1,progress=.83f},new Frame{name="06-fever",mode=2,progress=.4f},
            new Frame{name="07-recall-early",mode=3,progress=.18f},new Frame{name="08-recall-mid",mode=3,progress=.36f},
            new Frame{name="09-recall-late",mode=3,progress=.52f},new Frame{name="10-restored",mode=3,progress=1}
        };report.frames=frames;
        Texture2D source=null,read=null;RenderTexture target=null;Material material=null;var previous=RenderTexture.active;
        try{
            if(!report.supported)throw new InvalidOperationException("FeverWorld shader missing or unsupported");
            source=new Texture2D(2,2,TextureFormat.RGBA32,false);source.hideFlags=HideFlags.HideAndDontSave;
            if(!source.LoadImage(File.ReadAllBytes(Path.Combine(Application.dataPath,"../Captures/node-alignment-runtime.png"))))throw new InvalidDataException("Old source capture unavailable");
            material=new Material(shader){hideFlags=HideFlags.HideAndDontSave};
            var exit=new FeverExitMotion();material.SetVector("_RecallSettings",new Vector4((float)exit.Power,(float)exit.Twist,(float)exit.SpacePull,0));
            float ax=1,ay=16f/9f;material.SetVector("_Center",new Vector4(.5f,.5f,0,0));material.SetVector("_Aspect",new Vector4(ax,ay,0,0));
            material.SetFloat("_Extent",FeverWorldTransition.Extent(new Vector2(.5f,.5f),ax,ay));material.SetFloat("_CoreRadius",.058f);material.SetFloat("_Light",1);material.SetFloat("_Warp",1);material.SetVectorArray("_Pulses",new Vector4[6]);
            material.SetVector("_FeverCenterAspect",new Vector4(.5f,.5f,ax,ay));material.SetVector("_Aspect",new Vector4(ax,ay,0,0));
            material.SetFloat("_HasElements",0);material.SetFloat("_CoreVisible",0);material.SetFloat("_VisualBodyScale",1);material.SetFloat("_BonusGain",2.5f);
            target=new RenderTexture(450,800,0,RenderTextureFormat.ARGB32){hideFlags=HideFlags.HideAndDontSave};target.Create();
            read=new Texture2D(450,800,TextureFormat.RGBA32,false){hideFlags=HideFlags.HideAndDontSave};
            foreach(var frame in frames){
                frame.spread=(float)(frame.mode==1?FeverFlowMath.EntrySpread(frame.progress):frame.mode==2?1:FeverFlowMath.ExitRemaining(frame.progress));
                material.SetFloat("_Mode",frame.mode);material.SetFloat("_Progress",frame.progress);material.SetFloat("_Spread",frame.spread);
                material.SetVector("_FeverFlow",new Vector4(frame.mode,frame.spread,FeverWorldTransition.Extent(new Vector2(.5f,.5f),ax,ay),frame.mode==3?(float)exit.Suction(frame.progress*exit.Total):frame.progress));
                Graphics.Blit(source,target,material);RenderTexture.active=target;read.ReadPixels(new Rect(0,0,450,800),0,0);read.Apply();
                File.WriteAllBytes(Path.Combine(folder,frame.name+".png"),read.EncodeToPNG());
            }
        }finally{
            RenderTexture.active=previous;if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
            if(source!=null)UnityEngine.Object.DestroyImmediate(source);if(read!=null)UnityEngine.Object.DestroyImmediate(read);if(material!=null)UnityEngine.Object.DestroyImmediate(material);
            report.afterScenes=Scenes();report.playingAfter=EditorApplication.isPlaying;report.sceneStateUnchanged=report.beforeScenes==report.afterScenes;
            File.WriteAllText(Path.Combine(folder,"gpu-report.json"),JsonUtility.ToJson(report,true));
        }
        Debug.Log("Fever C GPU preview complete: 11 isolated frames; no Play/scene save. "+folder);
    }
}
