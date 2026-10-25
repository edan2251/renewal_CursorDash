using UnityEngine;
public static class CursorDashSession
{
    public enum Mode { Normal, Tutorial, Training }
    public static Mode CurrentMode=Mode.Normal;
    public static bool IsPractice => CurrentMode!=Mode.Normal;
    public static string LastSaveError {get;private set;}="";
    public static bool FirstGuideNeeded => CursorDashStorage.Get(CursorDashTuning.TutorialKey,"new")=="new";
    public static bool SetTutorialStatus(string status)
    {
        if(status!="completed"&&status!="skipped"&&status!="incomplete")return false;
        bool ok=CursorDashStorage.Save(CursorDashTuning.TutorialKey,status,out string error);LastSaveError=error;return ok;
    }
}
