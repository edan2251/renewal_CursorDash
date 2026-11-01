using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Rendering-only samples. Never clone a controller, collider, game manager or event system.
public sealed partial class CursorDashScreenAuthoringWindow
{
    private readonly List<Object> boardResources = new List<Object>();

    private void DrawBoardSourceButtons()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("넥서스 본체 원본")) Select(source.NexusBodySource.gameObject);
            if(GUILayout.Button("넥서스 전체 배치")&&source.NexusView!=null&&source.NexusView.PlacementRoot!=null)Select(source.NexusView.PlacementRoot.gameObject);
            if (GUILayout.Button("넥서스 장식 원본")) Select(source.NexusFrameSource.gameObject);
            if (GUILayout.Button("공유 연출 설정")) Selection.activeObject = Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype");
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if(GUILayout.Button("노드 Mesh 외형"))Selection.activeObject=CursorDashNodeAppearance.Default;
            if(GUILayout.Button("회로 경로 원본"))Selection.activeObject=Resources.Load<CursorDashVisualProfile>("CursorDash/VisualPrototype").circuitLayout;
            if(GUILayout.Button("커서 표시 원본")){var player=source.gameObject.scene.GetRootGameObjects();foreach(var root in player){var controller=root.GetComponent<PlayerCharacterController>();if(controller!=null)Select(root.transform.Find("CursorVisual").gameObject);}}
            if(GUILayout.Button("튜토리얼 안내 원본")&&source.GuidanceView!=null){CursorDashUIAuthoringPreview.Show(source.GuidanceView,screen==Screen.FeverNotice);Select(source.GuidanceView.gameObject);}
        }
    }

    private GameObject BoardObject(string name)
    {
        var item = new GameObject("PREVIEW ONLY - " + name) { hideFlags = HideFlags.HideAndDontSave };
        SceneManager.MoveGameObjectToScene(item, previewScene);
        clones.Add(item);
        return item;
    }

    private void AddBoardComposition()
    {
        var original = source.NexusBodySource;
        var root = BoardObject("Nexus source images");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = previewCamera;
        canvas.planeDistance = 1; canvas.sortingOrder = original.canvas.sortingOrder;
        var scaler = original.canvas.GetComponent<CanvasScaler>();
        canvas.scaleFactor = scaler == null ? 1 : Mathf.Pow(2, Mathf.Lerp(
            Mathf.Log(target.width / scaler.referenceResolution.x, 2),
            Mathf.Log(target.height / scaler.referenceResolution.y, 2), scaler.matchWidthOrHeight));
        Transform imageParent=root.transform;
        if(source.NexusView!=null&&source.NexusView.PlacementRoot!=null){
            var group=new GameObject("NexusPlacement",typeof(RectTransform));group.transform.SetParent(root.transform,false);
            CopyRect(source.NexusView.PlacementRoot,group.GetComponent<RectTransform>());imageParent=group.transform;
        }
        foreach (var image in original.transform.parent.GetComponentsInChildren<Image>(true))
        {
            if (image != original && image != source.NexusFrameSource && image.name != "HpCentral") continue;
            if (image == source.NexusFrameSource) continue; // Replaced by the same charged-only gauge shader below.
            var item = new GameObject(image.name, typeof(RectTransform), typeof(CanvasRenderer));
            item.transform.SetParent(imageParent, false);
            var rect = item.GetComponent<RectTransform>();
            CopyRect(image.rectTransform,rect);
            var copy = item.AddComponent<Image>();
            copy.sprite = image.sprite; copy.color = image.color; copy.raycastTarget = false;
            if (image == original)
            {
                var shader=Resources.Load<Shader>("CursorDash/NexusEnergy");
                var material=image.material!=null&&image.material.shader==shader?new Material(image.material):new Material(shader);material.hideFlags=HideFlags.HideAndDontSave;
                boardResources.Add(material);
                material.SetFloat("_SolidFraction", source.NexusSolidFraction);
                material.SetFloat("_Level", sampleHealth); material.SetFloat("_ImpactPress", 0);
                material.SetColor("_Color", source.NexusView==null?new Color(.04f,.7f,.9f):source.NexusView.EnergyTint); copy.material = material;
            }
        }
        Canvas.ForceUpdateCanvases();
        var body = imageParent.Find(original.name) as RectTransform;
        Vector3 center = body.TransformPoint(body.rect.center);
        float radius = Mathf.Min(body.rect.width * Mathf.Abs(body.lossyScale.x), body.rect.height * Mathf.Abs(body.lossyScale.y)) * .5f * source.NexusSolidFraction;
        // A world-space sample uses the same screen projection as the saved UI source.
        Vector3 viewport = previewCamera.WorldToViewportPoint(center);
        Vector3 world = previewCamera.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, -previewCamera.transform.position.z));
        AddGauge(world,radius*(source.NexusView==null?1.86f:source.NexusView.GaugeRadiusRatio),radius);
    }
    private static void CopyRect(RectTransform from,RectTransform to)
    {
        to.anchorMin=from.anchorMin;to.anchorMax=from.anchorMax;to.pivot=from.pivot;
        to.sizeDelta=from.sizeDelta;to.anchoredPosition3D=from.anchoredPosition3D;to.localScale=from.localScale;to.localRotation=from.localRotation;
    }

    private void AddGauge(Vector3 center,float radius,float bodyRadius)
    {
        var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
        mesh.vertices = new[] { new Vector3(-1.3f,-1.3f),new Vector3(-1.3f,1.3f),new Vector3(1.3f,1.3f),new Vector3(1.3f,-1.3f) };
        mesh.triangles = new[] { 0,1,2,0,2,3 }; boardResources.Add(mesh);
        var material = new Material(Resources.Load<Shader>("CursorDash/FeverGauge")) { hideFlags = HideFlags.HideAndDontSave };
        boardResources.Add(material); material.SetColor("_Color", source.NexusFrameSource.color);
        material.SetFloat("_Head", sampleCharge); material.SetFloat("_Tail", 0); material.SetFloat("_Arrow", sampleCharge > 0 ? 1 : 0);
        material.SetFloat("_Start", 7f/12); material.SetFloat("_Thickness", source.NexusView==null?.14f:source.NexusView.GaugeThickness); material.SetFloat("_GapDegrees", source.NexusView==null?4:source.NexusView.GaugeGapDegrees);
        material.SetVector("_NexusWorld", new Vector4(center.x, center.y, bodyRadius, 0));
        for (int i=0; i<5; i++)
        {
            var item = BoardObject("Gauge arc " + (i+1)); item.transform.position = center; item.transform.localScale = Vector3.one * radius;
            item.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = item.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = 34;
            var properties = new MaterialPropertyBlock(); properties.SetFloat("_Segment", i); renderer.SetPropertyBlock(properties);
        }
    }

    private void AddGuidance()
    {
        if(source.GuidanceView==null||screen!=Screen.Tutorial&&screen!=Screen.FeverNotice)return;
        // The saved view uses its existing HUD's scaler and sorting; no standalone gameplay UI is invented.
        var root=BoardObject("Guidance canvas");var canvas=root.AddComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=previewCamera;canvas.planeDistance=.7f;
        var original=source.GuidanceView.GetComponentInParent<Canvas>();canvas.sortingOrder=original.sortingOrder;
        var scaler=original.GetComponent<CanvasScaler>();canvas.scaleFactor=scaler==null?1:Mathf.Pow(2,Mathf.Lerp(Mathf.Log(target.width/scaler.referenceResolution.x,2),Mathf.Log(target.height/scaler.referenceResolution.y,2),scaler.matchWidthOrHeight));
        var view=Clone(source.GuidanceView);view.transform.SetParent(root.transform,false);
        var rect=(RectTransform)view.transform;rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
        BindFonts(view.gameObject);view.PresentHint(view.Checklist(0,0),screen==Screen.Tutorial);view.PresentNotice(screen==Screen.FeverNotice?1:0);
    }

    private SpriteRenderer ReferenceSprite(Transform parent,SpriteRenderer original)
    {
        var root=new GameObject("Artwork reference");root.transform.SetParent(parent,false);
        root.transform.localPosition=original.transform.localPosition;root.transform.localRotation=original.transform.localRotation;root.transform.localScale=original.transform.localScale;
        var renderer=root.AddComponent<SpriteRenderer>();renderer.enabled=false;renderer.sortingLayerID=original.sortingLayerID;renderer.sortingOrder=original.sortingOrder;return renderer;
    }

    private void AddNodeAndCursorSamples(bool includeCursor=true)
    {
        var creator=source.GetComponent<NodeCreator>();var pool=creator.AuthoredPool;
        if(pool!=null)
        {
            var circle=pool.GetComponentInChildren<NormalNode>(true);
            if(circle!=null){
                var root=BoardObject("Circular mesh sample");root.transform.position=previewCamera.ViewportToWorldPoint(new Vector3(.27f,.68f,-previewCamera.transform.position.z));root.transform.localScale=circle.transform.lossyScale;
                var reference=ReferenceSprite(root.transform,circle.GetComponent<SpriteRenderer>());
                var art=CircularNodeArtwork.Create(root.transform,reference);art.DrawSample(sampleNodeCharge,1,30,true);
            }
            var hex=pool.GetComponentInChildren<ArrowNode>(true);
            if(hex!=null){
                var root=BoardObject("Hexagonal mesh sample");root.transform.position=previewCamera.ViewportToWorldPoint(new Vector3(.73f,.68f,-previewCamera.transform.position.z));root.transform.localScale=hex.transform.lossyScale;
                var original=hex.transform.Find("Node");var node=new GameObject("Node");node.transform.SetParent(root.transform,false);node.transform.localScale=original.localScale;
                var first=ReferenceSprite(node.transform,original.Find("Outside 1").GetComponent<SpriteRenderer>());
                var second=ReferenceSprite(node.transform,original.Find("Outside 2").GetComponent<SpriteRenderer>());
                var reference=ReferenceSprite(node.transform,original.Find("Inside").GetComponent<SpriteRenderer>());
                var data=new SerializedObject(hex);Color color=data.FindProperty("triangleColor").colorValue;
                var flat=NodeFlatGraphic.Create(node.transform,reference);
                flat.DrawHexagonSample(Mathf.Lerp(HexagonalNodeRings.BodyRadius,data.FindProperty("triangleRadius").floatValue,sampleHexMorph),Mathf.Lerp(1,.18f,sampleHexMorph),sampleNodeCharge,1,false,color,sampleHexMorph);
                var rings=HexagonalNodeRings.Create(node.transform,first,second);rings.Draw(color,1-sampleHexMorph);
            }
        }
        if(!includeCursor)return;
        foreach(var original in source.gameObject.scene.GetRootGameObjects())
        {
            var controller=original.GetComponent<PlayerCharacterController>();if(controller==null)continue;
            var sprite=original.transform.Find("CursorVisual").GetComponent<SpriteRenderer>();
            var root=BoardObject("Cursor source sprite");root.transform.position=previewCamera.ViewportToWorldPoint(new Vector3(.64f,.35f,-previewCamera.transform.position.z));root.transform.localScale=sprite.transform.lossyScale;root.transform.rotation=sprite.transform.rotation;
            var renderer=root.AddComponent<SpriteRenderer>();renderer.sprite=sprite.sprite;renderer.color=sprite.color;renderer.sharedMaterial=sprite.sharedMaterial;renderer.sortingOrder=50;
        }
    }

    private void ReleaseBoardResources()
    {
        foreach (var item in boardResources) if (item != null)
        {
            var texture=item as RenderTexture;
            if(texture!=null)texture.Release();
            Object.DestroyImmediate(item);
        }
        boardResources.Clear();
    }
}
