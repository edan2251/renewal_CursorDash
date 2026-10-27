using System.Globalization;
using System.Numerics;
using UnityEngine;

/// <summary>New integer-point record. Never reads/writes the legacy survival-time key or map.highScore.</summary>
public static class CursorDashPointsRecord
{
    public static string LastError {get;private set;}="";
    public static string LastSaveStatus {get;private set;}="아직 라운드 정산 없음";
    public static string Key(string map)=>"CursorDash_BestPoints_v1_"+map;
    public static BigInteger Parse(string text)=>BigInteger.TryParse(text,NumberStyles.None,CultureInfo.InvariantCulture,out var n)&&n>=0?n:BigInteger.Zero;
    public static BigInteger Read(string map)=>Parse(PlayerPrefs.GetString(Key(map),"0"));
    public static void Save(string map,BigInteger points,bool eligible)
    {
        LastError=""; // A previous failed write must not label a later legitimate settlement as failed.
        if(!eligible){LastSaveStatus="기록 제외 라운드 — 기존 최고점 유지";return;}
        if(points<=Read(map)){LastSaveStatus="정상 정산 — 기존 최고점 유지";return;}
        try{
            PlayerPrefs.SetString(Key(map),points.ToString(CultureInfo.InvariantCulture));PlayerPrefs.Save();
            if(Read(map)!=points)throw new System.IO.IOException("저장 직후 읽은 최고점이 일치하지 않습니다.");
            LastSaveStatus="최고점 저장·즉시 읽기 확인: "+points.ToString(CultureInfo.InvariantCulture);
        }
        catch(System.Exception e){LastError="최고 점수 저장 실패: "+e.Message;LastSaveStatus=LastError;Debug.LogWarning(LastError);}
    }
}
