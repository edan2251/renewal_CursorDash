using System.Globalization;
using System.Numerics;
using UnityEngine;

/// <summary>New integer-point record. Never reads/writes the legacy survival-time key or map.highScore.</summary>
public static class CursorDashPointsRecord
{
    public static string LastError {get;private set;}="";
    public static string Key(string map)=>"CursorDash_BestPoints_v1_"+map;
    public static BigInteger Parse(string text)=>BigInteger.TryParse(text,NumberStyles.None,CultureInfo.InvariantCulture,out var n)&&n>=0?n:BigInteger.Zero;
    public static BigInteger Read(string map)=>Parse(PlayerPrefs.GetString(Key(map),"0"));
    public static void Save(string map,BigInteger points,bool eligible)
    {
        if(!eligible||points<=Read(map))return;
        try{PlayerPrefs.SetString(Key(map),points.ToString(CultureInfo.InvariantCulture));PlayerPrefs.Save();LastError="";}
        catch(System.Exception e){LastError="최고 점수 저장 실패: "+e.Message;Debug.LogWarning(LastError);}
    }
}
