using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Original A charge layers, sharing one geometric center. Owns runtime artwork only.</summary>
public sealed class CircularNodeArtwork : MonoBehaviour
{
    public const float BodyRadius = .518279f;
    public static readonly Color FillColor = new Color32(203, 73, 255, 255);
    private static readonly Color BorderColor = new Color32(232, 124, 255, 255);
    private readonly List<Mesh> meshes = new List<Mesh>();
    private Material material;
    private MeshRenderer fill, border, ring;
    private MaterialPropertyBlock properties;
    private float stroke = -1;
    private bool visible;
    public int RingPieceCount => CircularNodeOutlineA.Paths.Length;
    public Vector3 Center => transform.position;
    public bool Visible
    {
        get => visible;
        set { visible = value; if (!value) SetVisibility(false, false, false); }
    }

    public static CircularNodeArtwork Create(Transform parent, SpriteRenderer reference)
    {
        var item = new GameObject("Original A circular artwork") { hideFlags = HideFlags.DontSave, layer = parent.gameObject.layer };
        item.transform.SetParent(parent, false);
        var art = item.AddComponent<CircularNodeArtwork>();
        var shader = Resources.Load<Shader>("CursorDash/CircularArtwork");
        if (shader == null || !shader.isSupported)
            throw new InvalidOperationException("CursorDash circular artwork shader is unavailable.");
        art.material = new Material(shader) { hideFlags = HideFlags.DontSave };
        art.properties = new MaterialPropertyBlock();
        var disk = new Shape(); disk.Solid(Circle(BodyRadius));
        art.fill = art.NewLayer("Charge", disk.ToMesh(), reference, 1);
        art.border = art.NewLayer("Body outline", new Mesh(), reference, 2);
        var outside = new Shape();
        foreach (var path in CircularNodeOutlineA.Paths) outside.Solid(path);
        art.ring = art.NewLayer("Five original arrow arcs", outside.ToMesh(), reference, 3);
        art.Visible = false;
        return art;
    }

    public void Draw(float charge, float absorption, float angle, bool charging)
    {
        float requestedStroke = Mathf.Clamp(CursorDashTuning.Number("visual.outline", .035f), .01f, .12f);
        if (Mathf.Abs(stroke - requestedStroke) > .00001f)
        {
            stroke = requestedStroke;
            var shape = new Shape(); shape.Annulus(BodyRadius, BodyRadius - stroke);
            shape.Apply(border.GetComponent<MeshFilter>().sharedMesh);
        }
        float remaining = Mathf.Clamp01(absorption);
        fill.transform.localScale = new Vector3(Mathf.Clamp01(charge), Mathf.Clamp01(charge), 1);
        ring.transform.localScale = new Vector3(remaining, remaining, 1);
        ring.transform.localRotation = Quaternion.Euler(0, 0, angle);
        Tint(fill, FillColor);
        Color rim = BorderColor; rim.a = charging ? 1 : remaining;
        Tint(border, rim);
        Color arcs = FillColor; arcs.a = remaining;
        Tint(ring, arcs);
        visible = true;
        SetVisibility(charging && charge > .001f, charging || remaining > .001f, remaining > .001f);
    }

    private void Tint(MeshRenderer renderer, Color color)
    {
        properties.SetColor("_Color", color); renderer.SetPropertyBlock(properties);
    }
    public void ApplyEntrance(float body,float outside,float turn)
    {
        border.transform.localScale=Vector3.one*body;
        fill.transform.localScale*=body;
        ring.transform.localScale*=outside;
        ring.transform.localRotation*=Quaternion.Euler(0,0,turn);
    }
    private void SetVisibility(bool f, bool b, bool r)
    {
        if (fill != null) fill.enabled = f;
        if (border != null) border.enabled = b;
        if (ring != null) ring.enabled = r;
    }
    private MeshRenderer NewLayer(string label, Mesh mesh, SpriteRenderer reference, int order)
    {
        mesh.name = "Original A " + label; mesh.hideFlags = HideFlags.DontSave; meshes.Add(mesh);
        var item = new GameObject(label) { hideFlags = HideFlags.DontSave, layer = gameObject.layer };
        item.transform.SetParent(transform, false);
        item.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = item.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.sortingLayerID = reference.sortingLayerID; renderer.sortingOrder = reference.sortingOrder + order;
        return renderer;
    }
    private static Vector2[] Circle(float radius)
    {
        var points = new Vector2[64];
        for (int i = 0; i < points.Length; i++)
        { float a = i * Mathf.PI * 2 / points.Length; points[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius; }
        return points;
    }
    private void OnDisable() { Visible = false; }
    private void OnDestroy()
    {
        foreach (var mesh in meshes) if (mesh != null) Destroy(mesh);
        if (material != null) Destroy(material);
    }

    // Filled contours plus a one-screen-pixel AA fringe. No soft source-texture halo or internal fan edges.
    private sealed class Shape
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Vector2> miters = new List<Vector2>(), edges = new List<Vector2>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<int> triangles = new List<int>();
        private int[] Boundary(Vector2[] p, float sign = 1)
        {
            var core = new int[p.Length];
            for (int i = 0; i < p.Length; i++)
            {
                Vector2 a = (p[i] - p[(i + p.Length - 1) % p.Length]).normalized;
                Vector2 b = (p[(i + 1) % p.Length] - p[i]).normalized;
                Vector2 n1 = new Vector2(a.y, -a.x), n2 = new Vector2(b.y, -b.x);
                Vector2 normal = (n1 + n2).normalized;
                normal *= sign * Mathf.Min(3, 1 / Mathf.Max(.05f, Vector2.Dot(normal, n2)));
                core[i] = vertices.Count;
                for (int j = 0; j < 2; j++)
                { vertices.Add(p[i]); miters.Add(normal); edges.Add(new Vector2(1 - j, j - .5f)); colors.Add(Color.white); }
            }
            for (int i = 0; i < p.Length; i++)
            {
                int a = core[i], b = core[(i + 1) % p.Length];
                triangles.AddRange(new[] { a, b, b + 1, a, b + 1, a + 1 });
            }
            return core;
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
        public void Solid(Vector2[] p)
        {
            int[] ids = Boundary(p); var remaining = new List<int>();
            for (int i = 0; i < p.Length; i++) remaining.Add(i);
            while (remaining.Count > 3)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int a = remaining[(i + remaining.Count - 1) % remaining.Count], b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                    if (Cross(p[b] - p[a], p[c] - p[b]) <= .00000001f) continue;
                    bool occupied = false;
                    foreach (int k in remaining)
                        if (k != a && k != b && k != c && Cross(p[b] - p[a], p[k] - p[a]) >= -.00000001f &&
                            Cross(p[c] - p[b], p[k] - p[b]) >= -.00000001f && Cross(p[a] - p[c], p[k] - p[c]) >= -.00000001f)
                        { occupied = true; break; }
                    if (occupied) continue;
                    triangles.AddRange(new[] { ids[a], ids[b], ids[c] }); remaining.RemoveAt(i); clipped = true; break;
                }
                if (!clipped) throw new InvalidOperationException("Original A contour could not be triangulated.");
            }
            triangles.AddRange(new[] { ids[remaining[0]], ids[remaining[1]], ids[remaining[2]] });
        }
        public void Annulus(float outer, float inner)
        {
            int[] a = Boundary(Circle(outer)), b = Boundary(Circle(inner), -1);
            for (int i = 0; i < a.Length; i++)
            { int j = (i + 1) % a.Length; triangles.AddRange(new[] { a[i], a[j], b[j], a[i], b[j], b[i] }); }
        }
        public Mesh ToMesh() { var mesh = new Mesh(); Apply(mesh); return mesh; }
        public void Apply(Mesh mesh)
        {
            mesh.Clear(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.SetUVs(0, miters); mesh.SetUVs(1, edges);
            mesh.SetColors(colors); mesh.RecalculateBounds();
            Bounds bounds = mesh.bounds; bounds.Expand(.04f); mesh.bounds = bounds;
        }
    }
}
