using UnityEditor;
using UnityEngine;

// Uses the same mesh/glyph-mask builders as the comparison render, in the actual edit scene.
public sealed partial class CursorDashScreenAuthoringWindow
{
    internal static CursorDashScreenAuthoringWindow SceneArtwork(StageManager stage,Screen state,Camera reference,int width,int height,TMPro.TMP_Text score,Vector3 nexus,float bodyRadius,float charge,float nodeCharge,float morph,bool nodes)
    {
        creatingSceneArtwork=true;
        CursorDashScreenAuthoringWindow drawer;
        try{drawer=CreateInstance<CursorDashScreenAuthoringWindow>();}finally{creatingSceneArtwork=false;}
        drawer.borrowsScene=true;drawer.hideFlags=HideFlags.HideAndDontSave;
        drawer.source=stage;drawer.screen=state;drawer.previewScene=stage.gameObject.scene;drawer.previewCamera=reference;
        drawer.sampleCharge=charge;drawer.sampleNodeCharge=nodeCharge;drawer.sampleHexMorph=morph;
        drawer.target=new RenderTexture(width,height,0,RenderTextureFormat.ARGB32){hideFlags=HideFlags.HideAndDontSave};drawer.target.Create();
        try{
            if(state!=Screen.FeverNotice)drawer.AddCircuitComposition(score);
            if(state==Screen.InGame||state==Screen.InGamePause||state==Screen.Tutorial){
                drawer.AddGauge(nexus,bodyRadius*stage.NexusView.GaugeRadiusRatio,bodyRadius);
                if(nodes)drawer.AddNodeAndCursorSamples(false);
            }
            foreach(var item in drawer.clones)item.hideFlags|=HideFlags.NotEditable;
            return drawer;
        }catch{drawer.Cleanup();DestroyImmediate(drawer);throw;}
    }
}
