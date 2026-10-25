using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>Only v2 setting/preset keys. Web uses synchronous localStorage with explicit failure reporting.</summary>
public static class CursorDashStorage
{
    public static string LastError {get;private set;}="";
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern IntPtr CDStoreGet(string key);
    [DllImport("__Internal")] private static extern int CDStoreSave(string key,string value);
    [DllImport("__Internal")] private static extern IntPtr CDStoreError();
    [DllImport("__Internal")] private static extern void CDStoreFree(IntPtr ptr);
    private static string Take(IntPtr ptr){if(ptr==IntPtr.Zero)return null;string s=Marshal.PtrToStringUTF8(ptr);CDStoreFree(ptr);return s;}
#endif
    public static string Get(string key,string fallback="")
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string value=Take(CDStoreGet(key));LastError=Take(CDStoreError())??"";return value??fallback;
#else
        return PlayerPrefs.GetString(key,fallback);
#endif
    }
    public static bool Save(string key,string value,out string error)
    {
        if(key!=CursorDashTuning.UserKey&&key!=CursorDashTuning.PresetKey&&key!=CursorDashTuning.TutorialKey){error="지원하지 않는 저장 key";return false;}
#if UNITY_WEBGL && !UNITY_EDITOR
        if(CDStoreSave(key,value)==1){error="";return true;}error=Take(CDStoreError())??"브라우저 저장 실패";return false;
#else
        string before=PlayerPrefs.GetString(key,null);
        try{PlayerPrefs.SetString(key,value);PlayerPrefs.Save();error="";return true;}
        catch(Exception e){if(before==null)PlayerPrefs.DeleteKey(key);else PlayerPrefs.SetString(key,before);error="저장 실패: "+e.Message;return false;}
#endif
    }
}
