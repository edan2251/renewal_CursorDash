using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>Private render copies of authored fonts. Preview and Play never populate the saved atlas.</summary>
public sealed class CursorDashViewFonts : IDisposable
{
    private readonly Dictionary<TMP_FontAsset, TMP_FontAsset> copies = new Dictionary<TMP_FontAsset, TMP_FontAsset>();
    private readonly List<Material> presets = new List<Material>();

    public void Bind(TMP_Text text)
    {
        var source = text.font;
        if (source == null || copies.ContainsValue(source)) return;
        var originalMaterial = text.fontSharedMaterial;
        TMP_FontAsset copy;
        if (!copies.TryGetValue(source, out copy))
        {
            copy = UnityEngine.Object.Instantiate(source);
            copy.name = source.name + " (view render copy)";
            copy.hideFlags = HideFlags.HideAndDontSave;
            var textures = source.atlasTextures;
            var owned = new Texture2D[textures.Length];
            for (int n = 0; n < textures.Length; n++)
                if (textures[n] != null) { owned[n] = UnityEngine.Object.Instantiate(textures[n]); owned[n].hideFlags = HideFlags.HideAndDontSave; }
            copy.atlasTextures = owned;
            copy.material = new Material(source.material) { hideFlags = HideFlags.HideAndDontSave };
            copy.material.mainTexture = owned.Length == 0 ? null : owned[0];
            copy.ReadFontAssetDefinition();
            copies.Add(source, copy);
        }
        text.font = copy;
        if(originalMaterial!=null&&originalMaterial!=source.material)
        {
            var material=new Material(originalMaterial){hideFlags=HideFlags.HideAndDontSave};material.mainTexture=copy.atlasTexture;
            presets.Add(material);text.fontSharedMaterial=material;
        }
        else text.fontSharedMaterial = copy.material;
    }

    public void Dispose()
    {
        foreach(var material in presets)Release(material);presets.Clear();
        foreach (var font in copies.Values)
        {
            if (font == null) continue;
            foreach (var texture in font.atlasTextures) Release(texture);
            Release(font.material); Release(font);
        }
        copies.Clear();
    }

    private static void Release(UnityEngine.Object item)
    {
        if (item == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(item); else UnityEngine.Object.DestroyImmediate(item);
    }
}
