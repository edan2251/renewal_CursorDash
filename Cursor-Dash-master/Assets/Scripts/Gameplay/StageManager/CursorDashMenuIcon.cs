using UnityEngine;
using UnityEngine.UI;

/// <summary>Authored Image sprites from Google Material Symbols and flag-icons SVG originals.</summary>
[RequireComponent(typeof(CanvasRenderer))]
public sealed class CursorDashMenuIcon : Image
{
    public enum Symbol { Trophy, Settings, Reset, Globe, Korea, English, Japan, China, Down }
    [SerializeField] private Symbol symbol;
    public Symbol Kind
    {
        get=>symbol;
        set{if(symbol==value&&sprite!=null)return;symbol=value;AssignDefaultArtwork();}
    }
    // Explicit authoring upgrade only; enabling a View never overwrites its Image.
    public void AssignDefaultArtwork()
    {
        string name=symbol==Symbol.Trophy?"leaderboard":symbol==Symbol.Settings?"settings":symbol==Symbol.Reset?"restart_alt":symbol==Symbol.Globe?"language":symbol==Symbol.Korea?"kr":symbol==Symbol.English?"us":symbol==Symbol.Japan?"jp":symbol==Symbol.China?"cn":"keyboard_arrow_down";
        sprite=Resources.Load<Sprite>("CursorDash/MenuIcons/"+name);preserveAspect=true;type=Type.Simple;
        if(sprite==null)Debug.LogError("CursorDash menu sprite is missing: "+name,this);
    }
}
