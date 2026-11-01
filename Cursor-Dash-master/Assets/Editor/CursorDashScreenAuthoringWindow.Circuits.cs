using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using P=CircuitPathGraph.Point;

// Static path/coverage preview uses the live graph, stroke arithmetic, mesh buffer, union and glyph shaders.
// Temporal hit propagation, fever withdrawal and postprocessing remain Play-only.
public sealed partial class CursorDashScreenAuthoringWindow
{
    private void AddCircuitComposition(TMP_Text score)
    {
        var profile=Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");
        if(profile==null||!profile.dynamicCircuitsEnabled)return;
        float aspect=previewCamera.aspect,unit=2*previewCamera.orthographicSize*Mathf.Min(1,aspect);
        var graph=profile.circuitLayout==null?new CircuitPathGraph(Math.Max(1,aspect),Math.Max(1,1/aspect),profile.circuitDensity):profile.circuitLayout.CreateGraph(Math.Max(1,aspect),Math.Max(1,1/aspect),profile.circuitDensity);
        var geometry=new CircuitStrokeBuffer();
        Color tint=profile.circuitNormalTint;tint.a=Mathf.Min(.48f,profile.circuitNormalContrast*.8f);
        Vector3 World(P point)=>new Vector3(previewCamera.transform.position.x+(float)point.X*unit,previewCamera.transform.position.y+(float)point.Y*unit,0);
        float Half(int index)=>unit*.5f*(graph.Edges[index].Thin?profile.circuitSecondaryWidth:profile.circuitPrimaryWidth);
        Vector3 Side(int index,bool end)
        {
            var edge=graph.Edges[index];int at=end?edge.B:edge.A;P current=graph.Points[at],before=graph.Points[edge.A],after=graph.Points[edge.B];
            if(end){before=graph.Points[edge.A];after=current+(current-before);}
            else {after=graph.Points[edge.B];before=current-(after-current);}
            if(graph.Degree[at]==2){int other=graph.Incident[at][0]==index?graph.Incident[at][1]:graph.Incident[at][0];var link=graph.Edges[other];P next=graph.Points[link.A==at?link.B:link.A];if(end)after=next;else before=next;}
            P side=CircuitStrokeMath.Side(before,current,after,Half(index));return new Vector3((float)side.X,(float)side.Y,0);
        }
        for(int i=0;i<graph.Edges.Length;i++)
        {
            var edge=graph.Edges[i];Vector3 a=World(graph.Points[edge.A]),b=World(graph.Points[edge.B]);
            Vector3 first=Side(i,false),last=Side(i,true);geometry.Quad(a-first,a+first,b+last,b-last,tint,0,previewCamera);
            if(graph.Degree[edge.B]==1){Vector3 axis=(b-a).normalized*Half(i),normal=new Vector3(-axis.y,axis.x);geometry.Quad(b-axis-normal,b-axis+normal,b+axis+normal,b+axis-normal,tint,1,previewCamera);}
            if(graph.Degree[edge.A]>2){float half=Half(i);foreach(int other in graph.Incident[edge.A])half=Mathf.Max(half,Half(other));Vector3 x=Vector3.right*half,y=Vector3.up*half;geometry.Quad(a-x-y,a-x+y,a+x+y,a+x-y,tint,2,previewCamera);}
        }
        var mesh=new Mesh{hideFlags=HideFlags.HideAndDontSave};geometry.Upload(mesh);boardResources.Add(mesh);
        var paths=new Material(Resources.Load<Shader>("CursorDash/CircuitPath")){hideFlags=HideFlags.HideAndDontSave};boardResources.Add(paths);
        var composite=new Material(Resources.Load<Shader>("CursorDash/CircuitComposite")){hideFlags=HideFlags.HideAndDontSave};boardResources.Add(composite);
        if(borrowsScene)composite.SetFloat("_LocalUV",1); // Scene view can show the board inside a larger editing viewport.
        var mask=new RenderTexture(target.width,target.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){hideFlags=HideFlags.HideAndDontSave};mask.Create();boardResources.Add(mask);
        var glyph=new RenderTexture(target.width,target.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear){hideFlags=HideFlags.HideAndDontSave};glyph.Create();boardResources.Add(glyph);
        var glyphMaterial=new Material(Resources.Load<Shader>("CursorDash/CircuitScoreGlyphMask")){hideFlags=HideFlags.HideAndDontSave};boardResources.Add(glyphMaterial);
        var buffer=new CommandBuffer{name="CursorDash authoring circuit sample"};var previous=RenderTexture.active;
        try
        {
            buffer.SetRenderTarget(glyph);buffer.SetViewport(new Rect(0,0,target.width,target.height));buffer.ClearRenderTarget(false,true,Color.clear);buffer.SetViewProjectionMatrices(previewCamera.worldToCameraMatrix,previewCamera.projectionMatrix);
            if(score.GetComponent<MeshRenderer>().enabled&&profile.circuitScoreMaskEnabled)
                foreach(var filter in score.GetComponentsInChildren<MeshFilter>()){
                    var renderer=filter.GetComponent<MeshRenderer>();if(renderer==null||filter.sharedMesh==null||renderer.sharedMaterial==null)continue;
                    var material=renderer.sharedMaterial;float Read(string name,float fallback)=>material.HasProperty(name)?material.GetFloat(name):fallback;
                    var properties=new MaterialPropertyBlock();properties.SetTexture("_MainTex",material.mainTexture);
                    properties.SetVector("_FontWeight",new Vector4(Read("_WeightNormal",0),Read("_WeightBold",.5f),Read("_FaceDilate",0),Read("_ScaleRatioA",1)));
                    properties.SetVector("_GlyphAdjust",new Vector4(profile.circuitScoreMaskPadding,profile.circuitScoreMaskSoftness,Read("_VertexOffsetX",0),Read("_VertexOffsetY",0)));
                    properties.SetFloat("_IsSDF",material.HasProperty("_GradientScale")?1:0);properties.SetFloat("_Coverage",1);
                    buffer.DrawMesh(filter.sharedMesh,renderer.localToWorldMatrix,glyphMaterial,0,0,properties);
                }
            Graphics.ExecuteCommandBuffer(buffer);buffer.Clear();
            paths.SetTexture("_CircuitScoreMask",glyph);paths.SetFloat("_ScoreHide",profile.circuitScoreMaskEnabled?profile.circuitScoreMaskStrength:0);
            buffer.SetRenderTarget(mask);buffer.SetViewport(new Rect(0,0,target.width,target.height));buffer.ClearRenderTarget(false,true,Color.clear);buffer.SetViewProjectionMatrices(previewCamera.worldToCameraMatrix,previewCamera.projectionMatrix);buffer.DrawMesh(mesh,Matrix4x4.identity,paths);Graphics.ExecuteCommandBuffer(buffer);
        }
        finally{buffer.Release();RenderTexture.active=previous;}
        composite.SetTexture("_MainTex",mask);
        var quad=new Mesh{hideFlags=HideFlags.HideAndDontSave};float h=previewCamera.orthographicSize,w=h*aspect;Vector3 center=new Vector3(previewCamera.transform.position.x,previewCamera.transform.position.y,0);
        quad.vertices=new[]{center+new Vector3(-w,-h),center+new Vector3(-w,h),center+new Vector3(w,h),center+new Vector3(w,-h)};quad.triangles=new[]{0,1,2,0,2,3};boardResources.Add(quad);
        quad.uv=new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right};
        var item=BoardObject("Circuit union with real score stroke mask");item.AddComponent<MeshFilter>().sharedMesh=quad;var view=item.AddComponent<MeshRenderer>();view.sharedMaterial=composite;view.sortingOrder=-950;
    }
}
