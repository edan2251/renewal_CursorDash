using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace CursorDashTests
{
    public partial class CursorDashSceneRegression
    {
        private static string GraphicsEvidenceRoot=>Path.GetFullPath(SessionState.GetString("NodeGraphics.OutputRoot","Logs/node-graphics-20261024-192424"));
        // Real gameplay clock, default attack timings. Calls are internal test inputs, not physical mouse input.
        [UnityTest]
        public IEnumerator Graphics_ComparisonSequence()
        {
            string label=SessionState.GetString("NodeGraphics.Compare","after");
            string dir=Path.Combine(GraphicsEvidenceRoot,label);
            Directory.CreateDirectory(dir);
            Call(Find("CursorDashRuntimeUI"),"BrowserCommand","{\"cmd\":\"training\"}");
            yield return WaitForScene("Cyberpunk");yield return null;DisablePatterns();
            var stage=Find("StageManager");var ui=Find("CursorDashRuntimeUI");
            if(label!="before") {Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"visualPreset\",\"name\":\"strong\"}");Call(ui,"Close");}
            var p=Find("PlayerCharacterController");((Behaviour)Find("VirtualJoyStick")).enabled=false;
            var body=p.GetComponent<Rigidbody2D>();body.position=new Vector2(-2,-3);p.transform.position=body.position;
            var circle=NodesOfType("NormalNode").First();var hex=NodesOfType("ArrowNode").First();
            circle.gameObject.SetActive(true);hex.gameObject.SetActive(true);
            var lines=new List<string>{"frame,segment,realTime,gameTime,timeScale,phase,hp,cameraX,cameraY"};int frame=0;
            foreach(string segment in new[]{"circle-attack","circle-contact","hex-return"})
            {
                Call(circle,"ResetObject");Call(hex,"ResetObject");Call(Find("PlayerCharacterAfterImage"),"ClearTrail");
                body.position=segment=="circle-contact"?new Vector2(-2,2):new Vector2(-2,-3);p.transform.position=body.position;
                Set(p,"MoveVector",Vector2.zero);Call(p,"MoveStop");
                Component n=segment=="hex-return"?hex:circle;
                n.transform.position=new Vector2(0,2);Call(n,"Execute");
                float began=Time.time,until=Time.realtimeSinceStartup+9;bool moving=false;
                while(Time.realtimeSinceStartup<until)
                {
                    if(segment=="circle-contact"&&!moving&&Time.time-began>1.2f){Set(p,"MoveVector",Vector2.right);moving=true;}
                    if(segment=="hex-return")Call(Find("PlayerCharacterAfterImage"),"RecordMovement",new Vector2(-1,1),new Vector2(1,1),Time.time);
                    string phase=Get(n,"Phase").ToString();
                    Vector3 cam=Camera.main.transform.position;
                    string file=frame.ToString("D5")+".png";
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir,file));
                    lines.Add(string.Format(System.Globalization.CultureInfo.InvariantCulture,"{0},{1},{2:R},{3:R},{4:R},{5},{6:R},{7:R},{8:R}",frame++,segment,Time.realtimeSinceStartup,Time.time,Time.timeScale,phase,Get(Get(stage,"scoreManager"),"Hp"),cam.x,cam.y));
                    yield return null;
                    if(phase=="Inactive")break;
                }
                Assert.AreEqual("Inactive",Get(n,"Phase").ToString(),segment);
                Set(p,"MoveVector",Vector2.zero);Call(p,"MoveStop");
            }
            File.WriteAllLines(Path.Combine(dir,"frames.csv"),lines);
            Assert.AreEqual(1,Get(stage,"DestroyedNodes"));Assert.AreEqual(1,Get(stage,"HexDestroyed"));
        }
    }
    internal static class CursorDashGraphicsRun
    {
        [MenuItem("Tools/CursorDash Graphics/8 Feedback Revision Full")]
        static void RevisionFull(){SessionState.SetString("NodeGraphics.OutputRoot",Path.GetFullPath("Logs/hitfeel2-20261024-231337"));Full();}
        [MenuItem("Tools/CursorDash Graphics/9 Feedback Revision Focus")]
        static void RevisionFocus()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Editor must be idle");
            SessionState.SetString("NodeGraphics.OutputRoot",Path.GetFullPath("Logs/hitfeel2-20261024-231337"));
            var api=ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashSceneRegression.Feedback2_DefaultAndLegacyCircleSizesAreBounded",
                "CursorDashTests.CursorDashSceneRegression.Feedback2_YellowStagesNoCoreAnd157Compatibility",
                "CursorDashTests.CursorDashSceneRegression.Feedback2_RealBackgroundRipplesBrightensAndRestoresWithStableUI"}}));
        }
        [MenuItem("Tools/CursorDash Graphics/1 Capture Before %#F8")]
        static void Before()=>Run("before");
        [MenuItem("Tools/CursorDash Graphics/2 Capture After %#F9")]
        static void After()=>Run("after");
        [MenuItem("Tools/CursorDash Graphics/3 Full Regression %#F10")]
        static void Full(){SessionState.SetString("NodeGraphics.Compare","after-regression");EditorApplication.ExecuteMenuItem("Tools/CursorDash Remake/Run Scene Regression");}
        [MenuItem("Tools/CursorDash Graphics/4 Graphics Regression")]
        static void Graphics()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Editor must be idle");
            var api=ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{
                "CursorDashTests.CursorDashSceneRegression.Graphics_DirectionalSingleContactAndCleanupKeepUserPause",
                "CursorDashTests.CursorDashSceneRegression.Graphics_Legacy141FullImportAndNewPresetRoundtripAreAtomic",
                "CursorDashTests.CursorDashSceneRegression.Graphics_SixConvexVerticesPistonAndFourOutletsKeepBeamRules"}}));
        }
        [MenuItem("Tools/CursorDash Graphics/7 Shader Regression")]
        static void ShaderRegression()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Editor must be idle");
            var api=ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{"CursorDashTests.CursorDashSceneRegression.Graphics_BroadShaderBendsRealPixelsAndEdgesRecoverWithoutMovingUI","CursorDashTests.CursorDashSceneRegression.Graphics_SixConvexVerticesPistonAndFourOutletsKeepBeamRules"}}));
        }
        static void Run(string label)
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Editor must be idle");
            SessionState.SetString("NodeGraphics.Compare",label);
            var api=ScriptableObject.CreateInstance<TestRunnerApi>();
            api.Execute(new ExecutionSettings(new Filter{testMode=UnityEditor.TestTools.TestRunner.Api.TestMode.EditMode,testNames=new[]{"CursorDashTests.CursorDashSceneRegression.Graphics_ComparisonSequence"}}));
        }
    }
}
