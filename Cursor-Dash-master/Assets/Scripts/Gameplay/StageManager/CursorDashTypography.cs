using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

/// <summary>Official, unmodified embedded fonts. Runtime-owned atlases; never edits existing TMP assets.</summary>
public sealed class CursorDashTypography : MonoBehaviour
{
    private static CursorDashTypography instance;
    private Font boldSource,mediumSource;private TMP_FontAsset bold,medium;
    private readonly HashSet<int> assigned=new HashSet<int>();
    public static TMP_FontAsset Bold=>Ensure().bold;
    public static TMP_FontAsset Medium=>Ensure().medium;
    public static Font MediumFont=>Ensure().mediumSource;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics(){instance=null;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] private static void Boot(){Ensure();}
    private static CursorDashTypography Ensure()
    {
        if(instance!=null)return instance;
        var go=new GameObject("CursorDash official typography"){hideFlags=HideFlags.DontSave};DontDestroyOnLoad(go);
        instance=go.AddComponent<CursorDashTypography>();instance.Load();return instance;
    }
    private void Load()
    {
        boldSource=Resources.Load<Font>("CursorDash/Fonts/esamanru Bold");mediumSource=Resources.Load<Font>("CursorDash/Fonts/esamanru Medium");
        TMP_FontAsset Create(Font source){
            if(source==null)return null;
            var asset=TMP_FontAsset.CreateFontAsset(source,64,6,GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
            if(asset==null){Debug.LogWarning("CursorDash: runtime font atlas could not be created for "+source.name);return null;}
            asset.hideFlags=HideFlags.DontSave;asset.name=source.name+" runtime SDF";
            asset.TryAddCharacters("0123456789×x+-./%: SCOREBESTPAUSERETRYHOME점수최고이번생존처치방어훈련피버일시정지재개설정메인시작",out string missing);return asset;
        }
        bold=Create(boldSource);medium=Create(mediumSource);
        if(bold==null||medium==null)Debug.LogWarning("CursorDash: official Isamanru font unavailable; existing font retained.");
        SceneManager.sceneLoaded+=OnScene;Scan();
    }
    private void OnScene(Scene scene,LoadSceneMode mode){assigned.Clear();Scan();}
    private static bool IsNumber(string text){if(string.IsNullOrWhiteSpace(text))return false;foreach(char c in text)if(!char.IsDigit(c)&&" ×x.,+-/%".IndexOf(c)<0)return false;return true;}
    private void Scan()
    {
        foreach(var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
            if(!assigned.Add(text.GetInstanceID()))continue;
            string name=text.name.ToLowerInvariant();bool heavy=IsNumber(text.text)||name.Contains("score")||name.Contains("multiplier")||name.Contains("best");
            var font=heavy?bold:medium;if(font==null)continue;
            // Preserve font size/alignment, including existing shadow objects and title logo Images.
            text.font=font;text.fontStyle=FontStyles.Normal;
        }
        foreach(var text in Object.FindObjectsByType<Text>(FindObjectsInactive.Include,FindObjectsSortMode.None)){
            if(!assigned.Add(text.GetInstanceID()))continue;var font=IsNumber(text.text)||text.name.ToLowerInvariant().Contains("score")?boldSource:mediumSource;
            if(font!=null){text.font=font;text.fontStyle=FontStyle.Normal;}
        }
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded-=OnScene;
        void Release(TMP_FontAsset asset){if(asset==null)return;foreach(var atlas in asset.atlasTextures)if(atlas!=null)Destroy(atlas);if(asset.material!=null)Destroy(asset.material);Destroy(asset);}
        Release(bold);Release(medium);if(instance==this)instance=null;
    }
}
