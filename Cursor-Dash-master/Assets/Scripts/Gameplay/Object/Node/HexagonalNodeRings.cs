using System.Collections.Generic;
using UnityEngine;

/// <summary>Original A ring artwork follows the two existing Rotator transforms. No attack or feedback ownership.</summary>
public sealed class HexagonalNodeRings : MonoBehaviour
{
    public const float BodyRadius = .555349f;
    public const float BodyStroke = .071768f;
    public const float FillRatio = 1.014927f;
    private readonly Transform[] sources = new Transform[2];
    private readonly MeshRenderer[] views = new MeshRenderer[2];
    private readonly Mesh[] meshes = new Mesh[2];
    private readonly float[] directions=new float[2];
    private Material material;
    private MaterialPropertyBlock properties;
    public int PiecesPerRing => 6;
    public bool Visible
    {
        get => views[0] != null && views[0].enabled;
        set { foreach (var view in views) if (view != null) view.enabled = value; }
    }

    public static HexagonalNodeRings Create(Transform parent, SpriteRenderer first, SpriteRenderer second)
    {
        var item = new GameObject("Original A hexagonal rings") { hideFlags = HideFlags.DontSave, layer = parent.gameObject.layer };
        item.transform.SetParent(parent, false);
        var art = item.AddComponent<HexagonalNodeRings>();
        // Reuse the approved crisp-edge shader without changing circular artwork or its material.
        art.material = new Material(Resources.Load<Shader>("CursorDash/CircularArtwork")) { hideFlags = HideFlags.DontSave };
        art.properties = new MaterialPropertyBlock();
        SpriteRenderer[] references = { first, second };
        for (int i = 0; i < references.Length; i++)
        {
            art.sources[i] = references[i].transform;
            var rotator=references[i].GetComponent<Rotator>();art.directions[i]=rotator==null?(i==0?-1:1):rotator.DirectionSign;
            var layer = new GameObject("Interrupted ring " + (i + 1)) { hideFlags = HideFlags.DontSave, layer = item.layer };
            layer.transform.SetParent(item.transform, false);
            art.meshes[i] = CreateMesh(i * 6);
            layer.AddComponent<MeshFilter>().sharedMesh = art.meshes[i];
            var view = layer.AddComponent<MeshRenderer>();
            view.sharedMaterial = art.material; view.sortingLayerID = references[i].sortingLayerID; view.sortingOrder = references[i].sortingOrder;
            art.views[i] = view;
        }
        art.Visible = false;
        return art;
    }

    public void Draw(Color color, float absorption, bool demonstration=false)
    {
        color.a *= Mathf.Clamp01(absorption);
        for (int i = 0; i < views.Length; i++)
        {
            // One original yellow, one readable deeper yellow; geometry and rotation stay identical.
            Color tint = i == 0||demonstration ? color : new Color(color.r * .82f, color.g * .82f, color.b * .82f, color.a);
            properties.SetColor("_Color", tint);
            Transform target = views[i].transform, source = sources[i];
            target.localPosition = source.localPosition; target.localRotation = source.localRotation; target.localScale = source.localScale;
            views[i].SetPropertyBlock(properties);
            views[i].enabled = absorption > .001f;
        }
    }

    private static Mesh CreateMesh(int offset)
    {
        var vertices = new List<Vector3>(); var miters = new List<Vector2>(); var edges = new List<Vector2>();
        var colors = new List<Color>(); var triangles = new List<int>();
        for (int piece = offset; piece < offset + 6; piece++)
        {
            Vector2[] p = HexagonalNodeOutlineA.Paths[piece]; int start = vertices.Count;
            for (int i = 0; i < p.Length; i++)
            {
                Vector2 a = (p[i] - p[(i + p.Length - 1) % p.Length]).normalized, b = (p[(i + 1) % p.Length] - p[i]).normalized;
                Vector2 n1 = new Vector2(a.y, -a.x), n2 = new Vector2(b.y, -b.x), normal = (n1 + n2).normalized;
                normal *= Mathf.Min(3, 1 / Mathf.Max(.05f, Vector2.Dot(normal, n2)));
                for (int j = 0; j < 2; j++) { vertices.Add(p[i]); miters.Add(normal); edges.Add(new Vector2(1 - j, j - .5f)); colors.Add(Color.white); }
                int current = start + i * 2, next = start + ((i + 1) % p.Length) * 2;
                triangles.AddRange(new[] { current, next, next + 1, current, next + 1, current + 1 });
            }
            foreach (int index in HexagonalNodeOutlineA.Triangles[piece]) triangles.Add(start + index * 2);
        }
        var mesh = new Mesh { name = "Original A six interrupted corners", hideFlags = HideFlags.DontSave };
        mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, miters); mesh.SetUVs(1, edges); mesh.SetColors(colors); mesh.RecalculateBounds();
        Bounds bounds = mesh.bounds; bounds.Expand(.04f); mesh.bounds = bounds;
        return mesh;
    }
    public void ApplyEntrance(float scale,float turn)
    {
        for(int i=0;i<views.Length;i++){
            views[i].transform.localScale*=scale;
            views[i].transform.localRotation*=Quaternion.Euler(0,0,turn*directions[i]);
        }
    }
    private void OnDisable() { Visible = false; }
    private void OnDestroy()
    {
        foreach (var mesh in meshes) if (mesh != null) Destroy(mesh);
        if (material != null) Destroy(material);
    }
}
