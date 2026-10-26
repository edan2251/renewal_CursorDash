using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CursorDashTests
{
    // Reflection keeps tests in an Editor-only assembly without moving the existing game scripts
    // out of Assembly-CSharp or including test hooks in the shipped player.
    public partial class CursorDashSceneRegression
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string Prefix = "CursorDash.Regression.";
        private static readonly string[] PreferenceKeys = { "CursorDash_Best_Cyberpunk", "BGM", "SFX" };
        private static readonly string[] V2Keys={"CursorDash.v2.user","CursorDash.v2.presets","CursorDash.v2.tutorial","CursorDash_BestPoints_v1_Cyberpunk","CursorDash_BestPoints_v1_Flame"};

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            SessionState.SetBool(Prefix+"CapturedThisRun",false);
            for(int n=0;n<SceneManager.sceneCount;n++)Assert.IsFalse(SceneManager.GetSceneAt(n).isDirty,"검수 전에 사용자 미저장 씬을 먼저 보존하세요. 테스트는 저장·폐기하지 않습니다.");
            foreach(string key in V2Keys){SessionState.SetBool(Prefix+key+".exists",PlayerPrefs.HasKey(key));SessionState.SetString(Prefix+key,PlayerPrefs.GetString(key,""));}
            foreach (string key in PreferenceKeys)
            {
                SessionState.SetBool(Prefix + key + ".exists", PlayerPrefs.HasKey(key));
                SessionState.SetFloat(Prefix + key, PlayerPrefs.GetFloat(key));
            }
            foreach (string mapName in new[] { "Cyberpunk", "Flame" })
            {
                Object map = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/Maps/" + mapName + ".asset");
                SessionState.SetFloat(Prefix + mapName + ".score", (float)Get(map, "highScore"));
                SessionState.SetBool(Prefix + mapName + ".unlock", (bool)Get(map, "isUnlock"));
            }
            SessionState.SetBool(Prefix+"CapturedThisRun",true);
            PlayerPrefs.SetString("CursorDash.v2.user","{}");PlayerPrefs.SetString("CursorDash.v2.presets","{}");PlayerPrefs.SetString("CursorDash.v2.tutorial","skipped");
            EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;
            yield return null;
            MuteEditorSources();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if(!SessionState.GetBool(Prefix+"CapturedThisRun",false))yield break;
            Time.timeScale = 1f;
            foreach (string key in PreferenceKeys)
            {
                if (SessionState.GetBool(Prefix + key + ".exists", false))
                    PlayerPrefs.SetFloat(key, SessionState.GetFloat(Prefix + key, 0f));
                else PlayerPrefs.DeleteKey(key);
            }
            foreach (string mapName in new[] { "Cyberpunk", "Flame" })
            {
                Object map = AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/Maps/" + mapName + ".asset");
                Set(map, "highScore", SessionState.GetFloat(Prefix + mapName + ".score", 0f));
                Set(map, "isUnlock", SessionState.GetBool(Prefix + mapName + ".unlock", false));
            }
            if (EditorApplication.isPlaying) yield return new ExitPlayMode();
            // ExitPlayMode reloads serialized resources; restore the user's original memory values afterwards too.
            foreach(string mapName in new[]{"Cyberpunk","Flame"}){Object map=AssetDatabase.LoadAssetAtPath<Object>("Assets/Resources/Maps/"+mapName+".asset");Set(map,"highScore",SessionState.GetFloat(Prefix+mapName+".score",0));Set(map,"isUnlock",SessionState.GetBool(Prefix+mapName+".unlock",false));}
            foreach(string key in V2Keys){if(SessionState.GetBool(Prefix+key+".exists",false))PlayerPrefs.SetString(key,SessionState.GetString(Prefix+key,""));else PlayerPrefs.DeleteKey(key);}
            PlayerPrefs.Save();
            SessionState.SetBool(Prefix+"CapturedThisRun",false);
        }

        [UnityTest]
        public IEnumerator CP3_TutorialUsesRealMovementContactAndReturnThenStaysInTraining()
        {
            PlayerPrefs.SetString("CursorDash.v2.tutorial","new");Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");yield return null;
            var practice=Find("CursorDashPractice");Assert.IsNotNull(practice);Assert.AreEqual(0,Get(practice,"Step"));
            var player=Find("PlayerCharacterController");((Behaviour)Find("VirtualJoyStick")).enabled=false;Set(player,"MoveVector",Vector2.right);yield return WaitGameplay(.2f);Set(player,"MoveVector",Vector2.zero);yield return WaitGameplay(.1f);Assert.AreEqual(1,Get(practice,"Step"));
            var circle=(Component)Call(practice,"Spawn",0);Assert.IsNotNull(circle);var body=player.GetComponent<Rigidbody2D>();body.position=circle.transform.position;Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();yield return WaitGameplay(.15f);
            Assert.AreEqual(2,Get(practice,"Step"));var hex=(Component)Call(practice,"Spawn",1);Assert.IsNotNull(hex);Set(hex,"preparationDuration",.05f); // current generation was already snapped: wait for its genuine charge.
            var stage=Find("StageManager");body.position=new Vector2(-1,-2);var trail=Find("PlayerCharacterAfterImage");float deadline=Time.realtimeSinceStartup+7;
            while((int)Get(stage,"HexDestroyed")==0&&Time.realtimeSinceStartup<deadline){Call(trail,"RecordMovement",new Vector2(-1,1),new Vector2(1,1),Time.time);yield return null;}
            Assert.AreEqual(1,Get(stage,"BlockedBeams"));Assert.AreEqual(1,Get(stage,"HexDestroyed"));yield return null;Assert.AreEqual(3,Get(practice,"Step"));Assert.AreEqual("completed",PlayerPrefs.GetString("CursorDash.v2.tutorial"));Assert.IsFalse((bool)Get(stage,"RecordEligible"));Assert.AreEqual("Cyberpunk",SceneManager.GetActiveScene().name);
        }
        [UnityTest]
        public IEnumerator CP3_PanelOwnsInputAndPreservesIndependentPause()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();var ui=Find("CursorDashRuntimeUI");var stage=Find("StageManager");var player=Find("PlayerCharacterController");
            Call(ui,"Open",false);Assert.AreEqual(0,Time.timeScale);Set(player,"MoveVector",Vector2.right);Vector3 pos=player.transform.position;yield return WaitRealtime(.15f);Assert.AreEqual(pos,player.transform.position);
            var stick=Find("VirtualJoyStick");Assert.IsFalse((bool)Call(stick,"TryBeginPointer",71,new Vector2(Screen.width/2,Screen.height/2),false));
            Call(stage,"SetPaused",true);Call(ui,"Close");Assert.AreEqual(0,Time.timeScale);Assert.AreEqual(Vector2.zero,Get(player,"MoveVector"));Call(stage,"SetPaused",false);Assert.AreEqual(1,Time.timeScale);
        }
        [UnityTest]
        public IEnumerator CP3_TrainingAndDevNeverRecoverCurrentRoundRecordEligibility()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;var stage=Find("StageManager");Assert.IsFalse((bool)Get(stage,"RecordEligible"));var score=(Component)Get(stage,"scoreManager");float best=PlayerPrefs.GetFloat("CursorDash_Best_Cyberpunk");Set(score,"TimeScore",best+100);Call(stage,"Death");Assert.AreEqual(best,PlayerPrefs.GetFloat("CursorDash_Best_Cyberpunk"));
            Call(ui,"BrowserCommand","{\"cmd\":\"normal\"}");yield return WaitForScene("Cyberpunk");stage=Find("StageManager");DisablePatterns();Assert.IsTrue((bool)Get(stage,"RecordEligible"),"Fresh default round qualifies.");
            Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"devOff\"}");Call(ui,"Close");Assert.IsFalse((bool)Get(stage,"RecordEligible"));
        }
        [UnityTest]
        public IEnumerator CP4_JsonValidationIsAtomicAndPresetsRoundTripWithoutOldPreferenceWrites()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"Open",true);var tuning=TypeFor("CursorDashTuning");
            Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"circle.dash\",\"value\":0.37}");string json=(string)tuning.GetMethod("Export").Invoke(null,new object[]{"test cp4",false});Assert.IsTrue(json.Contains("0.37"));
            object[] invalid={json.Replace("\"circle.dash\": 0.37","\"circle.dash\": -3"),null};Assert.IsFalse((bool)tuning.GetMethod("Import").Invoke(null,invalid));Assert.IsTrue(((string)tuning.GetMethod("Export").Invoke(null,new object[]{"test cp4",false})).Contains("0.37"));
            foreach(string bad in new[]{json.Replace("\"circle.dash\"","\"unknown.attack\""),json.Replace("\"schemaVersion\": 1","\"schemaVersion\": 99"),json.Replace("\"circle.dash\": 0.37","\"circle.dash\": \"NaN\""),new string('x',65537)}){object[] args={bad,null};Assert.IsFalse((bool)tuning.GetMethod("Import").Invoke(null,args));}
            Call(ui,"BrowserCommand","{\"cmd\":\"savePreset\",\"name\":\"CP4-test\"}");Call(ui,"BrowserCommand","{\"cmd\":\"duplicatePreset\",\"name\":\"CP4-test\",\"target\":\"CP4-copy\"}");Assert.AreEqual(2,((string[])tuning.GetMethod("PresetNames").Invoke(null,null)).Length);
            Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"circle.dash\",\"value\":0.9}");Call(ui,"BrowserCommand","{\"cmd\":\"loadPreset\",\"name\":\"CP4-copy\"}");Assert.IsTrue(((string)tuning.GetMethod("Export").Invoke(null,new object[]{"test cp4",false})).Contains("0.37"),Get(ui,"Message").ToString());
            foreach(string key in PreferenceKeys)Assert.AreEqual(SessionState.GetFloat(Prefix+key,0),PlayerPrefs.GetFloat(key));yield return null;
        }
        [UnityTest]
        public IEnumerator CP4_SafeRebuildExpandsTrailPoolAndNextRoundValuesWait()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;Call(ui,"Open",true);
            var trail=Find("PlayerCharacterAfterImage");float original=(float)Get(trail,"Lifetime");var player=Find("PlayerCharacterController");var box=player.GetComponent<BoxCollider2D>();Vector2 before=box.size;
            Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"trail.lifetime\",\"value\":4}");Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"trail.sample\",\"value\":0.005}");Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"player.colliderX\",\"value\":1.2}");Assert.AreEqual(original,Get(trail,"Lifetime"));Assert.AreEqual(before,box.size);
            Call(ui,"SafeTrial");Assert.AreEqual(4f,Get(trail,"Lifetime"));Assert.GreaterOrEqual((int)Get(trail,"Capacity"),804);Assert.AreEqual(before,box.size,"Next-round collider must not mutate mid-round.");
            Call(ui,"BrowserCommand","{\"cmd\":\"restart\"}");yield return WaitRealtime(.3f);Assert.That(Find("PlayerCharacterController").GetComponent<BoxCollider2D>().size.x,Is.EqualTo(1.2f).Within(.001f));
        }
        [UnityTest]
        public IEnumerator CP4_DeveloperReproductionUsesRealBeamGeometryAndStableBoundary()
        {
            var ui=Find("CursorDashRuntimeUI");Call(ui,"BrowserCommand","{\"cmd\":\"training\"}");yield return WaitForScene("Cyberpunk");yield return null;Call(ui,"Open",true);Call(ui,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"debug.boundary\",\"value\":true}");yield return null;
            var view=Find("CursorDashBoundaryView");var line=(LineRenderer)Get(view,"line");Assert.IsTrue(line.enabled);var stage=Find("StageManager");Assert.That(Vector2.Distance(line.GetPosition(0),(Vector2)Get(stage,"NexusPosition")),Is.EqualTo((float)Get(stage,"NexusRadius")).Within(.00001f));
            Call(ui,"BrowserCommand","{\"cmd\":\"reflectTrial\"}");float deadline=Time.realtimeSinceStartup+7;while((int)Get(stage,"HexDestroyed")==0&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual(1,Get(stage,"BlockedBeams"));Assert.AreEqual(1,Get(stage,"HexDestroyed"));Assert.AreEqual(3f,Get(Get(stage,"scoreManager"),"Hp"));Assert.IsFalse((bool)Get(stage,"RecordEligible"));
        }

        [UnityTest]
        public IEnumerator RoundLifecycle_PauseEndRetryAndHomeRestoreState()
        {
            Assert.IsNotNull(Find("CursorDashSpaceFlow"));
            Assert.AreEqual("Main",Get(Find("StageManager"),"State").ToString());
            ScreenCapture.CaptureScreenshot("Captures/main-runtime.png");
            yield return WaitRealtime(0.15f);
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            Component stage = Find("StageManager");
            Component score = (Component)Get(stage, "scoreManager");
            Component trail = Find("PlayerCharacterAfterImage");
            Assert.AreEqual("Playing", Get(stage, "State").ToString());
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            Assert.AreEqual(1f, Time.timeScale);
            ScreenCapture.CaptureScreenshot("Captures/play-runtime.png");
            yield return WaitRealtime(0.15f);

            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time);
            Click("RemakeHUD/PauseButton");
            Assert.AreEqual("Paused", Get(stage, "State").ToString());
            float pausedScore = (float)Get(score, "TimeScore");
            int pausedNodeCount = AllNodes().Count(node => node.gameObject.activeSelf);
            yield return WaitRealtime(2.2f);
            Assert.AreEqual(0f, Time.timeScale);
            Assert.AreEqual(pausedScore, (float)Get(score, "TimeScore"));
            Assert.AreEqual(1, (int)Get(trail, "ActiveSegmentCount"));
            Assert.AreEqual(pausedNodeCount, AllNodes().Count(node => node.gameObject.activeSelf));
            Assert.IsFalse(AllNodes().Any(node => Get(node, "Phase").ToString() == "Fired"));
            Assert.IsTrue(GameObject.Find("RemakeHUD/PausePanel").activeInHierarchy);
            ScreenCapture.CaptureScreenshot("Captures/pause-runtime.png");
            yield return WaitRealtime(0.15f);
            Click("RemakeHUD/PausePanel/Resume");
            Assert.AreEqual(1f, Time.timeScale);
            Call(stage, "DamageNexus", 1f);
            Call(stage, "SetPaused", true);
            yield return WaitRealtime(0.16f);
            Assert.AreEqual(0f, Time.timeScale, "A completing hit-stop must not unpause the round.");
            Call(stage, "SetPaused", false);
            yield return WaitGameplay(0.75f);
            Assert.AreEqual(0, (int)Get(trail, "ActiveSegmentCount"));

            Set(score, "Hp", 0f);
            // D-025 exclusive finale lasts .64s; keep the result assertion after that boundary.
            yield return WaitRealtime(0.75f);
            Assert.AreEqual("Ended", Get(stage, "State").ToString());
            Assert.IsTrue(GameObject.Find("RemakeHUD/ResultPanel").activeInHierarchy);
            Assert.AreEqual(0, (int)Get(trail, "ActiveSegmentCount"));
            float endedScore = (float)Get(score, "TimeScore");
            yield return WaitRealtime(0.1f);
            Assert.AreEqual(endedScore, (float)Get(score, "TimeScore"));
            Assert.IsTrue(AllNodes().All(node => !node.gameObject.activeSelf));
            ScreenCapture.CaptureScreenshot("Captures/result-runtime.png");
            yield return WaitRealtime(0.15f);
            Click("RemakeHUD/ResultPanel/Retry");
            yield return WaitForScene("Cyberpunk");
            stage = Find("StageManager");
            score = (Component)Get(stage, "scoreManager");
            Assert.AreEqual("Playing", Get(stage, "State").ToString());
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            Assert.Less((float)Get(score, "TimeScore"), 0.5f);
            Assert.AreEqual(0, (int)Get(stage, "DestroyedNodes"));
            Assert.AreEqual(0, (int)Get(stage, "BlockedBeams"));
            Assert.AreEqual(Vector2.zero, (Vector2)Get(Find("PlayerCharacterController"), "MoveVector"));
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(AllNodes().Any(node => Get(node, "Phase").ToString() == "Fired"));
            Call(stage, "SetPaused", true);
            Click("RemakeHUD/PausePanel/Home");
            yield return WaitForScene("01.StageSelectScene");
            Assert.AreSame(stage,Find("StageManager"));
            Assert.AreEqual("Main",Get(stage,"State").ToString());
            Assert.AreEqual("Cyberpunk",SceneManager.GetActiveScene().name);
            Assert.AreEqual(1f, Time.timeScale);
        }

        [UnityTest]
        public IEnumerator NodeRoles_OnlyFiredHexBeamCanUseLiveTrail()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component stage = Find("StageManager");
            Component score = (Component)Get(stage, "scoreManager");
            Component trail = Find("PlayerCharacterAfterImage");
            Component normal = NodesOfType("NormalNode").First();
            Component hex = NodesOfType("ArrowNode").First();
            Component player = Find("PlayerCharacterController");
            Set(normal, "preparationDuration", 0.1f);
            Set(normal, "warningDuration", 0.06f);
            Set(normal, "dashDuration", 0.2f);
            ConfigureHexPreparation(hex,0.06f);
            Set(hex, "warningDuration", 0.05f);
            ConfigureHexFlight(hex,0.35f);

            // Real Physics2D body overlap goes through PlayerCharacterController.OnTriggerEnter2D.
            normal.transform.position = player.transform.position;
            Call(normal, "Execute");
            yield return WaitPhysicsStep();
            yield return WaitPhysicsStep();
            Assert.AreEqual(1, (int)Get(stage, "DestroyedNodes"));
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            yield return WaitRealtime(0.5f);
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.position = new Vector2(1.8f, -2f);
            player.transform.position = playerBody.position;
            yield return null;

            Call(trail, "ClearTrail");
            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time);
            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            yield return WaitGameplay(0.12f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            yield return WaitGameplay(0.1f);
            Assert.AreEqual("Fired", Get(normal, "Phase").ToString());
            Call(normal, "Interaction");
            Assert.AreEqual(1, (int)Get(stage, "DestroyedNodes"));
            yield return WaitGameplay(0.3f);
            Assert.AreEqual(2f, (float)Get(score, "Hp"), "A live trail cannot stop the circular body dash.");
            Assert.AreEqual(0, (int)Get(stage, "BlockedBeams"));

            Call(trail, "ClearTrail");
            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            Call(hex, "Interaction");
            Assert.AreEqual("Preparing", Get(hex, "Phase").ToString(), "Touching a hexagonal body must not destroy it.");
            // D-016 ends the beam at the core edge, changing its world distance at a fixed duration.
            // Observe the real block within the attack window instead of the old center-target timestamp.
            float blockDeadline=Time.realtimeSinceStartup+1f;
            while((int)Get(stage,"BlockedBeams")<1){Assert.Less(Time.realtimeSinceStartup,blockDeadline);yield return null;}
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            yield return WaitGameplay(1.7f);

            Call(trail, "ClearTrail");
            ConfigureHexPreparation(hex,0.18f);
            ConfigureHexFlight(hex,0.2f);
            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time - 0.48f);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.1f);
            Assert.AreEqual("Preparing", Get(hex, "Phase").ToString());
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));
            yield return WaitGameplay(0.4f+(float)Get(hex,"PreFireSeconds"));
            Assert.AreEqual(1f, (float)Get(score, "Hp"), "A trail expired before firing must not reserve defense.");
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));

            Call(trail, "ClearTrail");
            ConfigureHexPreparation(hex,0.05f);
            ConfigureHexFlight(hex,0.4f);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.12f+(float)Get(hex,"PreFireSeconds"));
            Assert.AreEqual("Fired", Get(hex, "Phase").ToString());
            Call(trail, "RecordMovement", new Vector2(-1f, 1.8f), new Vector2(1f, 1.8f), Time.time);
            yield return WaitGameplay(0.08f);
            Assert.AreEqual(2, (int)Get(stage, "BlockedBeams"), "A new live trail intersecting the visible fired beam must block it.");
            Assert.AreEqual(1f, (float)Get(score, "Hp"));
        }

        [UnityTest]
        public IEnumerator CircularNode_MorphsUntilDash_CanBeDestroyedAndResetsForReuse()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component stage = Find("StageManager");
            Component score = (Component)Get(stage, "scoreManager");
            Component normal = NodesOfType("NormalNode").First();
            Component player = Find("PlayerCharacterController");
            Component trail = Find("PlayerCharacterAfterImage");
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.position = new Vector2(2f, -3f);
            player.transform.position = playerBody.position;
            yield return null;
            LineRenderer beam = normal.GetComponent<LineRenderer>();
            CircleCollider2D body = normal.GetComponent<CircleCollider2D>();
            Transform ring = normal.transform.Find("Outside");
            Transform inside = normal.transform.Find("Inside");
            Transform broken = normal.transform.Find("BreakShell");
            Assert.IsNotNull(broken.GetComponent<SpriteRenderer>().sprite);
            Vector3 originalScale = normal.transform.localScale;
            Vector3 originalRingScale = ring.localScale;
            float originalLineWidth = beam.widthMultiplier;
            Assert.That((float)Get(trail, "Lifetime"), Is.EqualTo(0.6782609f).Within(0.00001f));
            Assert.That(trail.GetComponent<TrailRenderer>().time, Is.EqualTo(0.6782609f).Within(0.00001f));
            Assert.That((float)Get(player, "MoveSpeed"), Is.EqualTo(5.75f).Within(0.00001f));
            Assert.That(trail.GetComponent<TrailRenderer>().startWidth, Is.EqualTo(0.0675f).Within(0.00001f));
            Assert.That(5.75f * (float)Get(trail, "Lifetime"), Is.EqualTo(3.9f).Within(0.00001f));
            Assert.AreEqual(Vector3.one * 0.5f, player.transform.Find("CursorVisual").localScale);
            Assert.AreEqual(new Vector2(0.75f, 0.75f), player.GetComponent<BoxCollider2D>().size,
                "The cursor visual must shrink without changing its existing collision body.");
            Assert.IsFalse(((AudioSource)Get((Component)Get(Find("GameManager"), "soundManager"), "BGMSource")).isPlaying);
            Assert.IsNotNull((AudioSource)Get((Component)Get(Find("GameManager"), "soundManager"), "SFXSource"));
            Assert.That((float)Get(normal, "preparationDuration"), Is.EqualTo(2.5f).Within(0.00001f));
            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            yield return WaitGameplay(2.56f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            Assert.That(inside.localScale.x, Is.EqualTo(1f).Within(0.001f));
            ScreenCapture.CaptureScreenshot("Captures/node-alignment-runtime.png");
            yield return null;
            yield return WaitGameplay(0.12f);
            ScreenCapture.CaptureScreenshot("Captures/node-warning-runtime.png");
            yield return null;
            yield return null;
            Call(normal, "ResetObject");
            Set(normal, "preparationDuration", 0.1f);
            Set(normal, "warningDuration", 0.3f);
            Set(normal, "dashDuration", 0.12f);

            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            Assert.AreEqual("Preparing", Get(normal, "Phase").ToString());
            yield return WaitGameplay(0.15f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            Assert.IsTrue(body.enabled, "The circular node remains destructible during warning.");
            Assert.IsFalse(beam.enabled);
            Assert.Less(normal.transform.localScale.x, originalScale.x);
            Assert.Less(ring.localScale.x, originalRingScale.x);
            Assert.Greater(Mathf.Abs(ring.localEulerAngles.z), 0f);
            ScreenCapture.CaptureScreenshot("Captures/node-warning-short-runtime.png");
            yield return null;
            yield return null;
            Call(stage, "SetPaused", true);
            Vector3 pausedScale = normal.transform.localScale;
            yield return WaitRealtime(0.4f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            Assert.Less(Vector3.Distance(pausedScale, normal.transform.localScale), 0.001f);
            Assert.IsFalse(beam.enabled);
            Call(stage, "SetPaused", false);

            normal.transform.position = player.transform.position;
            yield return WaitPhysicsStep();
            yield return WaitPhysicsStep();
            Assert.AreEqual(1, (int)Get(stage, "DestroyedNodes"));
            Assert.AreEqual("Resolving", Get(normal, "Phase").ToString());
            Assert.IsFalse(broken.gameObject.activeSelf,"D-018 removes the duplicate legacy shell, not the contact impact.");
            Assert.AreEqual(1,(int)Get(Get(stage,"Feedback"),"FragmentBursts"),"Exactly one owned directional burst replaces the legacy layered effects.");
            ScreenCapture.CaptureScreenshot("Captures/node-break-runtime.png");
            yield return null;
            yield return null;
            Assert.IsFalse(beam.enabled);
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            yield return WaitGameplay(0.55f);
            Assert.AreEqual("Inactive", Get(normal, "Phase").ToString());
            Assert.IsFalse(beam.enabled, "A destroyed warning must never fire later.");
            Assert.IsFalse(((GameObject)Get(normal, "effectObject")).activeSelf,
                "A destroyed warning must not revive its delayed effect.");
            Assert.AreEqual(1, (int)Get(stage, "DestroyedNodes"));
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            Assert.AreEqual(originalScale, normal.transform.localScale);
            Assert.AreEqual(originalRingScale, ring.localScale);
            Assert.AreEqual(originalLineWidth, beam.widthMultiplier);
            Assert.IsFalse(broken.gameObject.activeSelf);

            // DamageNexus shakes the camera/ScreenSpaceCamera canvas after contact.
            // Compare with the undisturbed body boundary, not its post-impact shaken transform.
            Vector2 impactCenter = (Vector2)Get(stage, "NormalDashNexusPosition");
            float impactRadius = (float)Get(stage, "NormalDashNexusRadius");
            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            yield return WaitGameplay(0.15f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            yield return WaitGameplay(0.32f);
            Assert.AreEqual("Fired", Get(normal, "Phase").ToString());
            Assert.IsFalse(body.enabled);
            // C-01 draft intentionally replaces the former beam with the moving body.
            Assert.IsFalse(beam.enabled, "Circular dash never enables the legacy beam.");
            Assert.IsTrue(normal.transform.Find("CursorMorph").GetComponent<MeshRenderer>().enabled);
            Assert.IsFalse(ring.GetComponent<SpriteRenderer>().enabled);
            Assert.That(ring.localScale.sqrMagnitude, Is.LessThan(0.000001f));
            yield return WaitGameplay(0.18f);
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            Assert.AreEqual(1, (int)Get(normal, "ImpactsThisExecution"));
            Vector2 impact = (Vector2)Get(normal, "LastImpactPoint");
            Assert.That(Vector2.Distance(impact, impactCenter),
                Is.EqualTo(impactRadius).Within(0.0001f));
            yield return WaitGameplay(0.5f);
            Assert.AreEqual("Inactive", Get(normal, "Phase").ToString());
            Assert.AreEqual(originalScale, normal.transform.localScale);
            Assert.AreEqual(originalRingScale, ring.localScale);
            Assert.AreEqual(originalLineWidth, beam.widthMultiplier);
        }


        [UnityTest]
        public IEnumerator CP1_RecoilScaleTimingAndCancellationAreIndependent()
        {
            Click("RemakeMenu/Start"); yield return WaitForScene("Cyberpunk"); DisablePatterns();
            Component n=NodesOfType("NormalNode").First(), stage=Find("StageManager"), p=Find("PlayerCharacterController");
            p.GetComponent<Rigidbody2D>().position=new Vector2(2,-3);p.transform.position=new Vector2(2,-3);
            Assert.That((float)Get(n,"cursorScale"),Is.EqualTo(0.6f).Within(0.00001f));
            Assert.That((float)Get(n,"recoilDuration"),Is.EqualTo(0.05f).Within(0.00001f));
            Assert.That((float)Get(n,"warningDuration"),Is.EqualTo(0.15f).Within(0.00001f));
            Assert.That((float)Get(n,"dashDuration"),Is.EqualTo(0.20f).Within(0.00001f));
            Set(n,"preparationDuration",0.05f);
            Vector3 origin=new Vector3(-2,2,0);
            n.transform.position=origin;Call(n,"Execute");
            Assert.AreEqual(Vector3.one*0.5f,n.transform.localScale,"Charging circle is not scaled to .6.");
            float until=Time.realtimeSinceStartup+5f;
            while(!(bool)Get(n,"IsRecoiling")){Assert.Less(Time.realtimeSinceStartup,until);yield return null;}
            Assert.AreEqual("Warning",Get(n,"Phase").ToString());
            Assert.IsTrue(n.GetComponent<CircleCollider2D>().enabled);
            Assert.That((float)Get(n,"CursorWorldLength"),Is.EqualTo(1.6f*0.6f*0.5f*0.92f).Within(0.00001f));
            Assert.That((float)Get(n,"RecoilWorldDistance"),Is.EqualTo(0.035328f).Within(0.00001f));
            Assert.That(Vector2.Distance(n.transform.position,(Vector2)Get(n,"LeadingTip")),
                Is.EqualTo(1.6f*0.68f*0.6f*0.5f*0.92f).Within(0.00001f),"Apply .6 exactly once.");
            Call(stage,"SetPaused",true);Vector3 paused=n.transform.position;
            yield return WaitRealtime(0.15f);Assert.AreEqual(paused,n.transform.position);
            Call(stage,"SetPaused",false);Call(n,"Interaction");
            yield return WaitGameplay(0.6f);
            Assert.AreEqual("Inactive",Get(n,"Phase").ToString());
            Assert.AreEqual(3f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            n.transform.position=origin;Call(n,"Execute");yield return WaitCirclePhase(n,"Fired");
            Assert.That(Vector3.Distance(origin,n.transform.position),Is.EqualTo(0.035328f).Within(0.001f));
            float began=Time.time;yield return WaitCirclePhase(n,"Resolving");
            Assert.That(Time.time-began,Is.EqualTo(0.20f).Within(0.04f),"Recoil is outside the forward duration.");
            Assert.AreEqual(1,(int)Get(n,"ImpactsThisExecution"));
            Assert.AreEqual(2f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            yield return WaitGameplay(0.6f);
            n.transform.position=origin;Call(n,"Execute");
            Assert.AreEqual(Vector3.one*0.5f,n.transform.localScale);
            Assert.IsFalse((bool)Get(n,"IsRecoiling"));
            Assert.IsFalse(n.transform.Find("CursorMorph").GetComponent<MeshRenderer>().enabled);
        }

        [UnityTest]
        public IEnumerator CP1_RequestedValuesWaitForTheNextGeneration()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            Component n=NodesOfType("NormalNode").First();
            Type tuning=TypeFor("CursorDashTuning");
            tuning.GetMethod("EnableDev").Invoke(null,new object[]{true});
            object[] args={"circle.dash","0.4",null,false};
            Assert.IsTrue((bool)tuning.GetMethod("TrySetText").Invoke(null,args));
            Set(n,"preparationDuration",0.05f);
            Find("PlayerCharacterController").transform.position=new Vector2(2,-3);
            Find("PlayerCharacterController").GetComponent<Rigidbody2D>().position=new Vector2(2,-3);
            n.transform.position=new Vector2(-2,2);Call(n,"Execute");yield return WaitCirclePhase(n,"Fired");
            Assert.That((float)Get(n,"DashSeconds"),Is.EqualTo(0.4f).Within(0.0001f));
            args=new object[]{"circle.dash","0.1",null,false};
            Assert.IsTrue((bool)tuning.GetMethod("TrySetText").Invoke(null,args));
            Assert.That((float)Get(n,"DashSeconds"),Is.EqualTo(0.4f).Within(0.0001f),"No in-flight mutation.");
            Call(n,"ResetObject");n.transform.position=new Vector2(-2,2);Call(n,"Execute");
            Assert.That((float)Get(n,"DashSeconds"),Is.EqualTo(0.1f).Within(0.0001f));
            tuning.GetMethod("EnableDev").Invoke(null,new object[]{false});
            Call(n,"ResetObject");n.transform.position=new Vector2(-2,2);Call(n,"Execute");
            Assert.That((float)Get(n,"DashSeconds"),Is.EqualTo(0.2f).Within(0.0001f),"Dev off uses deployment defaults.");
        }

        // D-016 updates only the first draft's timing/scale contract; the circular laser remains disabled.
        [UnityTest]
        public IEnumerator CircularDash_DefaultPhasesAndVisibleNoseImpact()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component stage = Find("StageManager"), normal = NodesOfType("NormalNode").First();
            Component player = Find("PlayerCharacterController");
            player.GetComponent<Rigidbody2D>().position = new Vector2(2f, -3f);
            player.transform.position = new Vector2(2f, -3f);
            Assert.That((float)Get(normal, "preparationDuration"), Is.EqualTo(2.5f).Within(0.0001f));
            // D-016 replaces the first visual draft's .25/.5 timing with the user's .15/.20.
            Assert.That((float)Get(normal, "warningDuration"), Is.EqualTo(0.15f).Within(0.0001f));
            Assert.That((float)Get(normal, "dashDuration"), Is.EqualTo(0.20f).Within(0.0001f));
            Assert.That((float)Get(stage, "NormalDashNexusRadius"), Is.EqualTo(0.3255208f).Within(0.0001f));
            Assert.AreEqual(Vector2.zero, (Vector2)Get(stage, "NexusPosition"), "Hex targeting stays unchanged.");
            Vector2 impactCenter = (Vector2)Get(stage, "NormalDashNexusPosition");
            float impactRadius = (float)Get(stage, "NormalDashNexusRadius");
            normal.transform.position = new Vector2(-1.6f, 2.2f);
            Vector2 origin = normal.transform.position;
            Call(normal, "Execute");
            yield return WaitGameplay(1.25f);
            Assert.AreEqual("Preparing", Get(normal, "Phase").ToString());
            yield return CaptureCircleDraft("01-charge");
            yield return WaitCirclePhase(normal, "Warning");
            yield return WaitGameplay(0.1f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            Assert.Greater((float)Get(normal, "MorphProgress"), 0f);
            Assert.Less((float)Get(normal, "MorphProgress"), 1f);
            yield return CaptureCircleDraft("02-absorb-morph");
            yield return WaitCirclePhase(normal, "Fired");
            Assert.AreEqual(1f, (float)Get(normal, "MorphProgress"));
            Assert.IsFalse(normal.transform.Find("Outside").GetComponent<SpriteRenderer>().enabled);
            Assert.IsFalse(normal.GetComponent<LineRenderer>().enabled);
            Assert.IsFalse(normal.GetComponent<CircleCollider2D>().enabled);
            yield return WaitGameplay(0.06f);
            Assert.Greater(Vector2.Distance(origin, normal.transform.position), 0.1f, "The node itself must move.");
            Call(normal, "Interaction");
            Assert.AreEqual("Fired", Get(normal, "Phase").ToString(), "Dash cannot be contact-destroyed.");
            Component trail = Find("PlayerCharacterAfterImage");
            Call(trail, "RecordMovement", new Vector2(-2f, 1f), new Vector2(1f, 1f), Time.time);
            yield return CaptureCircleDraft("03-cursor-dash");
            yield return WaitCirclePhase(normal, "Resolving");
            Assert.AreEqual(2f, (float)Get((Component)Get(stage, "scoreManager"), "Hp"));
            Assert.AreEqual(0, (int)Get(stage, "BlockedBeams"), "Dash never asks the trail for defense.");
            Assert.AreEqual(1, (int)Get(normal, "ImpactsThisExecution"));
            Vector2 hit = (Vector2)Get(normal, "LastImpactPoint");
            Assert.That(Vector2.Distance(hit, impactCenter),
                Is.EqualTo(impactRadius).Within(0.0001f));
            Assert.Less(Vector2.Distance(hit, (Vector2)Get(normal, "LeadingTip")), 0.0001f);
            yield return CaptureCircleDraft("04-nose-impact");
            yield return WaitGameplay(0.6f);
            Assert.AreEqual("Inactive", Get(normal, "Phase").ToString());
            Assert.AreEqual(2f, (float)Get((Component)Get(stage, "scoreManager"), "Hp"), "No delayed duplicate hit.");
        }

        [UnityTest]
        public IEnumerator CircularDash_PreparingContactPauseDisableAndRepeatedPoolReuse()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component normal = NodesOfType("NormalNode").First(), stage = Find("StageManager");
            Component player = Find("PlayerCharacterController"), score = (Component)Get(stage, "scoreManager");
            Vector3 scale = normal.transform.localScale;
            normal.transform.position = player.transform.position;
            Call(normal, "Execute");
            yield return WaitPhysicsStep();
            yield return WaitPhysicsStep();
            Assert.AreEqual("Resolving", Get(normal, "Phase").ToString());
            Assert.AreEqual(1, (int)Get(stage, "DestroyedNodes"), "Preparing contact still explodes.");
            yield return WaitGameplay(0.6f);
            player.GetComponent<Rigidbody2D>().position = new Vector2(2f, -3f);
            player.transform.position = new Vector2(2f, -3f);
            Set(normal, "preparationDuration", 0.05f);
            Set(normal, "warningDuration", 0.1f);
            Set(normal, "dashDuration", 0.4f);
            for (int cycle = 0; cycle < 4; cycle++)
            {
                normal.transform.position = new Vector2(-2f, 2f);
                Call(normal, "Execute");
                Assert.AreEqual("Preparing", Get(normal, "Phase").ToString());
                Assert.AreEqual(0f, (float)Get(normal, "MorphProgress"));
                Assert.AreEqual(scale, normal.transform.localScale);
                Assert.IsFalse(normal.GetComponent<CircleCollider2D>().enabled,"D-031: invisible first entrance frame must not have a full collision body.");
                Assert.IsTrue(normal.transform.Find("Outside").GetComponent<SpriteRenderer>().enabled);
                yield return WaitCirclePhase(normal,"Warning");
                Assert.IsTrue(normal.GetComponent<CircleCollider2D>().enabled,"Contact removal remains available after entrance and during warning.");
                yield return WaitCirclePhase(normal, "Fired");
                Call(stage, "SetPaused", true);
                Vector3 paused = normal.transform.position;
                yield return WaitRealtime(0.2f);
                Assert.AreEqual(paused, normal.transform.position);
                Assert.AreEqual("Fired", Get(normal, "Phase").ToString());
                Call(stage, "SetPaused", false);
                normal.gameObject.SetActive(false);
                yield return WaitGameplay(0.55f);
                Assert.AreEqual("Inactive", Get(normal, "Phase").ToString());
                Assert.AreEqual(Vector3.zero, normal.transform.localPosition);
                Assert.AreEqual(scale, normal.transform.localScale);
                Assert.IsTrue(normal.GetComponent<CircleCollider2D>().enabled);
                Assert.AreEqual(3f, (float)Get(score, "Hp"), "Disabled dash must never complete later.");
            }
            normal.transform.position = Vector2.up * 2f;
            Call(normal, "Execute");
            yield return WaitCirclePhase(normal, "Fired");
            Call(stage, "SetPaused", true);
            yield return WaitRealtime(0.15f);
            Call(stage, "SetPaused", false);
            yield return WaitGameplay(0.9f);
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            Assert.AreEqual(1, (int)Get(normal, "ImpactsThisExecution"));
        }

        [UnityTest]
        public IEnumerator CircularDash_LethalHitRetryHomeRetireOldVisuals()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component stage = Find("StageManager"), normal = NodesOfType("NormalNode").First();
            // The scene's player starts at (0, 2.32); keep the dash origin clear of touch removal.
            Component player = Find("PlayerCharacterController");
            player.GetComponent<Rigidbody2D>().position = new Vector2(2f, -3f);
            player.transform.position = new Vector2(2f, -3f);
            Set((Component)Get(stage, "scoreManager"), "Hp", 1f);
            Set(normal, "preparationDuration", 0.05f);
            Set(normal, "warningDuration", 0.05f);
            Set(normal, "dashDuration", 0.01f);
            normal.transform.position = Vector2.up * 2f;
            Call(normal, "Execute");
            yield return WaitCirclePhase(normal, "Inactive");
            Assert.AreEqual("Ended", Get(stage, "State").ToString());
            Assert.AreEqual(0f, (float)Get((Component)Get(stage, "scoreManager"), "Hp"));
            Assert.AreEqual(1, (int)Get(normal, "ImpactsThisExecution"));
            // D-025 final break now precedes result display for .64s.
            yield return WaitRealtime(0.75f);
            Click("RemakeHUD/ResultPanel/Retry");
            yield return WaitForScene("Cyberpunk");
            stage = Find("StageManager"); normal = NodesOfType("NormalNode").First();
            Assert.AreEqual(3f, (float)Get((Component)Get(stage, "scoreManager"), "Hp"));
            Call(stage, "SetPaused", true);
            Click("RemakeHUD/PausePanel/Home");
            yield return WaitForScene("01.StageSelectScene");
            Assert.AreEqual(1f, Time.timeScale);
            Assert.IsFalse(Resources.FindObjectsOfTypeAll<MeshRenderer>().Any(r =>
                r.gameObject.scene.IsValid() && r.gameObject.name == "CircleDashImpact"));
        }

        [UnityTest]
        public IEnumerator Rotator_DisableAndReuseOwnsRotation_HexAndHpRemainAnimated()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            var go = new GameObject("Rotation lifecycle regression");
            var rotator = (Behaviour)go.AddComponent(TypeFor("Rotator"));
            for (int i = 0; i < 3; i++)
            {
                rotator.enabled = true;
                Quaternion start = go.transform.localRotation;
                yield return WaitGameplay(0.1f);
                Assert.Greater(Quaternion.Angle(start, go.transform.localRotation), 10f);
                rotator.enabled = false;
                Quaternion stopped = go.transform.localRotation;
                yield return WaitGameplay(0.1f);
                Assert.Less(Quaternion.Angle(stopped, go.transform.localRotation), 0.01f);
            }
            Object.Destroy(go);
            Component hex = NodesOfType("ArrowNode").First();
            hex.transform.position = Vector2.up * 2f;
            Call(hex, "Execute");
            Transform ring = hex.transform.Find("Node/Outside 1");
            Transform hp = GameObject.Find("UI/HpCentral").transform;
            Quaternion hexBefore = ring.localRotation, hpBefore = hp.localRotation;
            yield return WaitGameplay(0.2f);
            Assert.AreEqual("Preparing", Get(hex, "Phase").ToString());
            Assert.Greater(Quaternion.Angle(hexBefore, ring.localRotation), 1f);
            Assert.Greater(Quaternion.Angle(hpBefore, hp.localRotation), 1f);
        }

        private static IEnumerator WaitCirclePhase(Component normal, string phase)
        {
            float until = Time.realtimeSinceStartup + 10f;
            while (Get(normal, "Phase").ToString() != phase)
            {
                Assert.Less(Time.realtimeSinceStartup, until, "Circular phase timed out: " + phase);
                yield return null;
            }
        }
        private static IEnumerator CaptureCircleDraft(string name)
        {
            string folder = SessionState.GetString("CursorDash.CirclePrototype.EvidenceDirectory", "Logs/CirclePrototype");
            System.IO.Directory.CreateDirectory(folder);
            ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(folder, name + ".png"));
            // Do not restore a sampled timeScale=0 after the game's hit-stop has completed.
            // Capture the current rendered frame without taking ownership of the gameplay clock.
            yield return null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator CP2_ContinuousReturnArrivesBeforeTriangleShardsAndCancelsDamage()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            Component stage=Find("StageManager"),hex=NodesOfType("ArrowNode").First(),trail=Find("PlayerCharacterAfterImage"),p=Find("PlayerCharacterController");
            p.GetComponent<Rigidbody2D>().position=new Vector2(2,-3);p.transform.position=new Vector2(2,-3);
            ConfigureHexPreparation(hex,.05f);Set(hex,"warningDuration",.05f);ConfigureHexFlight(hex,.4f);
            hex.transform.position=Vector2.up*2;Call(hex,"Execute");yield return WaitCirclePhase(hex,"Fired");
            Assert.IsTrue((bool)Get(hex,"TriangleVisible"));
            Assert.IsTrue(hex.transform.GetChild(0).GetComponentsInChildren<SpriteRenderer>(true).All(sr=>!sr.enabled),"Every original hex sprite is absorbed; no hidden central outline remains.");
            Vector3[] corners=(Vector3[])Get(hex,"TriangleWorldPoints");
            // D-018: six convex vertices with three nonzero cut edges, not the superseded equilateral triangle.
            Assert.AreEqual(6,corners.Length);
            Assert.Greater(Vector3.Distance(corners[0],corners[1]),.001f);
            Assert.That(Vector3.Distance(corners[0],corners[1])/Vector3.Distance(corners[1],corners[2]),Is.EqualTo(.18f).Within(.0001f));
            Call(trail,"RecordMovement",new Vector2(-1,1.8f),new Vector2(1,1.8f),Time.time);
            yield return WaitCirclePhase(hex,"Resolving");
            float turned=Time.time;
            var beam=hex.GetComponent<LineRenderer>();
            Assert.Less(Vector2.Distance(beam.GetPosition(0),(Vector2)Get(hex,"BeamOutlet")),.0001f,"Visible return endpoint is the short-edge outlet; collision still starts at the unchanged origin.");
            Assert.Greater(Vector3.Distance(beam.GetPosition(0),beam.GetPosition(1)),.05f,"No new zero-length line at the contact.");
            Assert.AreEqual(1,(int)Get(stage,"BlockedBeams"));Assert.AreEqual(0,(int)Get(stage,"HexDestroyed"));
            Assert.IsTrue((bool)Get(hex,"TriangleVisible"));Assert.AreEqual(3f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            float until=Time.realtimeSinceStartup+3;
            while((int)Get(stage,"HexDestroyed")==0){Assert.Less(Time.realtimeSinceStartup,until);yield return null;}
            Assert.That(Time.time-turned,Is.EqualTo(.06f).Within(.04f));
            Assert.IsFalse((bool)Get(hex,"TriangleVisible"));Assert.IsFalse(beam.enabled);
            Assert.AreEqual(1,(int)Get(hex,"BlocksThisExecution"));Assert.AreEqual(1,(int)Get(hex,"ImpactsThisExecution"));
            Assert.AreEqual(3f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            yield return WaitGameplay(1.5f);
            Assert.AreEqual(1,(int)Get(stage,"HexDestroyed"));
            Call(trail,"ClearTrail");
            Type tuning=TypeFor("CursorDashTuning");tuning.GetMethod("EnableDev").Invoke(null,new object[]{true});
            object[] arg={"fx.enabled","false",null,false};Assert.IsTrue((bool)tuning.GetMethod("TrySetText").Invoke(null,arg));
            ConfigureHexFlight(hex,.01f);hex.transform.position=new Vector2(2,2);Call(hex,"Execute");
            yield return WaitCirclePhase(hex,"Inactive");
            Assert.AreEqual(2f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            Vector2 edge=(Vector2)Get(hex,"target");
            Assert.That(Vector2.Distance(edge,(Vector2)Get(stage,"NexusPosition")),Is.EqualTo((float)Get(stage,"NexusRadius")).Within(.0001f));
            yield return WaitGameplay(.3f);Assert.AreEqual(2f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            tuning.GetMethod("EnableDev").Invoke(null,new object[]{false});
        }

        [UnityTest]
        public IEnumerator CP2_StableBoundaryAndLatestEnergyTargetSurviveHitsAndPause()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            Component stage=Find("StageManager");object energy=Get(stage,"Energy");
            Vector2 center=(Vector2)Get(stage,"NexusPosition");float radius=(float)Get(stage,"NexusRadius");
            var bodyImage=(UnityEngine.UI.Image)Get(stage,"hpBarImage");var bodyRect=bodyImage.rectTransform;
            float settled=Mathf.Min(Mathf.Abs(bodyRect.rect.width*bodyRect.lossyScale.x),Mathf.Abs(bodyRect.rect.height*bodyRect.lossyScale.y))*.5f*(float)Get(stage,"normalNexusSolidFraction");
            Assert.That(radius,Is.EqualTo(settled).Within(.00001f),"Capture gameplay boundary after CanvasScaler has settled.");
            Call(stage,"DamageNexus",1f);
            Assert.AreEqual(2f,(float)Get((Component)Get(stage,"scoreManager"),"Hp"));
            Assert.That((float)Get(energy,"TargetLevel"),Is.EqualTo(2f/3).Within(.0001f));
            yield return WaitRealtime(.05f);float shown=(float)Get(energy,"DisplayedLevel");
            Assert.That(shown,Is.EqualTo(2f/3).Within(.0001f),"D-023 displays the actual remaining health without the former slow drain.");
            Assert.AreEqual(center,(Vector2)Get(stage,"NexusPosition"));Assert.AreEqual(radius,(float)Get(stage,"NexusRadius"));
            Call(stage,"DamageNexus",1f);Assert.That((float)Get(energy,"TargetLevel"),Is.EqualTo(1f/3).Within(.0001f));
            Assert.That((float)Get(energy,"DisplayedLevel"),Is.EqualTo(1f/3).Within(.0001f),"A second hit replaces the displayed level immediately.");
            Call(stage,"SetPaused",true);float paused=(float)Get(energy,"DisplayedLevel");
            yield return WaitRealtime(.2f);Assert.AreEqual(paused,(float)Get(energy,"DisplayedLevel"));Assert.AreEqual(0f,Time.timeScale);
            var runtime=Find("CursorDashRuntimeUI");Call(runtime,"Open",true);Call(runtime,"BrowserCommand","{\"cmd\":\"set\",\"key\":\"nexus.color\",\"value\":\"#29FFCC\"}");yield return null;
            var energyMaterial=(Material)Get(energy,"material");Assert.That(energyMaterial.GetColor("_Color").g,Is.GreaterThan(.99f),"Visual color previews while gameplay is paused.");Call(runtime,"Close");TypeFor("CursorDashTuning").GetMethod("EnableDev").Invoke(null,new object[]{false});
            Call(stage,"SetPaused",false);yield return WaitGameplay(.7f);
            Assert.That((float)Get(energy,"DisplayedLevel"),Is.EqualTo(1f/3).Within(.0001f));
            Assert.AreEqual(center,(Vector2)Get(stage,"NexusPosition"));
            object distortion=Get(Get(stage,"Feedback"),"Distortion");
            var uiCamera=(Camera)Get(distortion,"UiCamera");
            Assert.AreNotEqual(Camera.main,uiCamera);
            Assert.AreEqual(0,Camera.main.cullingMask&(1<<5));Assert.AreNotEqual(0,uiCamera.cullingMask&(1<<5));
        }

        [UnityTest]
        public IEnumerator CP2_RepeatedStopsAreBoundedAndDevPauseNeverReleasesUserPause()
        {
            Click("RemakeMenu/Start");yield return WaitForScene("Cyberpunk");DisablePatterns();
            Component stage=Find("StageManager");float start=Time.time,until=Time.realtimeSinceStartup+.35f;
            while(Time.realtimeSinceStartup<until){Call(stage,"RequestFeedbackStop",.08f);yield return null;}
            Assert.Greater(Time.time-start,.02f,"Repeated impacts cannot freeze the game indefinitely.");
            for(int i=0;i<12;i++)Call(stage,"RegisterCircleDestroyed",new Vector2(i%2==0?-1:1,1));
            object feedback=Get(stage,"Feedback"),distortion=Get(feedback,"Distortion");
            Assert.LessOrEqual((int)Get(distortion,"ActiveCount"),4);
            Assert.LessOrEqual((int)Get(feedback,"FragmentBursts"),16);
            Call(stage,"SetDevPaused",true);Assert.AreEqual(0f,Time.timeScale);
            Call(stage,"SetPaused",true);Call(stage,"SetDevPaused",false);
            yield return WaitRealtime(.2f);Assert.AreEqual(0f,Time.timeScale);
            Call(stage,"SetPaused",false);yield return WaitGameplay(.15f);Assert.AreEqual(1f,Time.timeScale);
        }

        [UnityTest]
        public IEnumerator HexNode_WarnsWithoutBodyDestruction_AndKeepsOriginalBlockRadius()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component stage = Find("StageManager");
            Component score = (Component)Get(stage, "scoreManager");
            Component trail = Find("PlayerCharacterAfterImage");
            Component hex = NodesOfType("ArrowNode").First();
            Component player = Find("PlayerCharacterController");
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.position = new Vector2(2f, -3f);
            player.transform.position = playerBody.position;
            yield return null;
            LineRenderer beam = hex.GetComponent<LineRenderer>();
            Transform body = hex.transform.GetChild(0);
            Transform outsideOne = body.Find("Outside 1");
            Transform outsideTwo = body.Find("Outside 2");
            Vector3 bodyScale = body.localScale;
            Vector3 outsideOneScale = outsideOne.localScale;
            Vector3 outsideTwoScale = outsideTwo.localScale;
            float baseWidth = beam.widthMultiplier;
            float baseBlockRadius = beam.startWidth * 0.5f;
            Assert.That((float)Get(hex,"reflectionDuration"),Is.EqualTo(0.06f).Within(0.00001f),"D-016 fast-return default.");
            // Slow this instance only so intermediate return continuity remains observable.
            Set(hex,"reflectionDuration",0.3f);
            ConfigureHexPreparation(hex,2.8f); // Give the 5.5→6 morph a visible test window.
            Set(hex, "warningDuration", 0.3f);
            ConfigureHexFlight(hex,0.25f);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitCirclePhase(hex,"Warning");yield return WaitGameplay(.04f);
            Assert.AreEqual("Warning", Get(hex, "Phase").ToString());
            Assert.IsFalse(beam.enabled);
            Assert.IsFalse(hex.GetComponent<CircleCollider2D>().enabled);
            Assert.Less(body.localScale.x, bodyScale.x);
            Assert.Less(outsideOne.localScale.x, outsideOneScale.x);
            Assert.Less(outsideTwo.localScale.x, outsideTwoScale.x);
            Assert.Greater(Mathf.Abs(outsideOne.localEulerAngles.z), 0f);
            Call(hex, "Interaction");
            Assert.AreEqual("Warning", Get(hex, "Phase").ToString(),
                "Touching a hexagonal body must not destroy it.");
            Assert.AreEqual(0, (int)Get(stage, "DestroyedNodes"));
            Call(stage, "SetPaused", true);
            Vector3 pausedScale = body.localScale;
            yield return WaitRealtime(0.4f);
            Assert.AreEqual("Warning", Get(hex, "Phase").ToString());
            Assert.Less(Vector3.Distance(pausedScale, body.localScale), 0.001f);
            Assert.IsFalse(beam.enabled);
            Call(stage, "SetPaused", false);
            yield return WaitCirclePhase(hex,"Fired");
            Assert.AreEqual("Fired", Get(hex, "Phase").ToString());
            Assert.IsTrue(beam.enabled);
            Assert.IsTrue(body.gameObject.activeSelf, "The target remains visible while the beam travels out.");
            Assert.IsFalse(hex.GetComponent<Animator>().enabled, "Destroy art must not play at fire time.");
            Assert.That(beam.widthMultiplier, Is.EqualTo(baseWidth * 0.65f).Within(0.0001f));
            Assert.That((float)Get(hex, "beamDefenseRadius"), Is.EqualTo(baseBlockRadius).Within(0.0001f),
                "The thinner beam must keep its previous blocking radius.");
            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time);
            yield return WaitGameplay(0.14f);
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            Assert.AreEqual("Resolving", Get(hex, "Phase").ToString());
            Assert.IsTrue(body.gameObject.activeSelf);
            Assert.IsTrue(beam.enabled);
            Assert.IsFalse(hex.GetComponent<Animator>().enabled);
            Vector3 returnStart = beam.GetPosition(1);
            yield return WaitGameplay(0.12f);
            Assert.Less(Vector3.Distance(beam.GetPosition(1), hex.transform.position),
                Vector3.Distance(returnStart, hex.transform.position), "The reflected beam must travel toward the node.");
            ScreenCapture.CaptureScreenshot("Captures/hex-return-runtime.png");
            yield return WaitGameplay(0.24f);
            Assert.AreEqual("Resolving", Get(hex, "Phase").ToString());
            Assert.IsFalse(body.gameObject.activeSelf);
            Assert.IsFalse(beam.enabled);
            Assert.IsFalse(hex.GetComponent<Animator>().enabled,"Triangle shards replace the old hex destruction animation.");
            Assert.IsFalse((bool)Get(hex,"TriangleVisible"));
            Assert.AreEqual(1,(int)Get(stage,"HexDestroyed"));
            ScreenCapture.CaptureScreenshot("Captures/hex-impact-runtime.png");
            yield return WaitGameplay(1.4f);
            Assert.AreEqual("Inactive", Get(hex, "Phase").ToString());
            Assert.AreEqual(bodyScale, body.localScale);
            Assert.AreEqual(outsideOneScale, outsideOne.localScale);
            Assert.AreEqual(outsideTwoScale, outsideTwo.localScale);
            Assert.AreEqual(baseWidth, beam.widthMultiplier);

            Call(trail, "ClearTrail");
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.75f);
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            Assert.AreEqual("Inactive", Get(hex, "Phase").ToString());
            Assert.AreEqual(baseWidth, beam.widthMultiplier);

            // A round interruption during reflection cannot leave an impact callback on this pooled node.
            ConfigureHexPreparation(hex,0.05f);
            Set(hex, "warningDuration", 0.05f);
            ConfigureHexFlight(hex,0.4f);
            Call(trail, "ClearTrail");
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.13f);
            Call(trail, "RecordMovement", new Vector2(-1f, 1.8f), new Vector2(1f, 1.8f), Time.time);
            yield return WaitGameplay(0.06f);
            Assert.AreEqual("Resolving", Get(hex, "Phase").ToString());
            Call(Find("NodeCreator"), "StopNodes");
            Assert.AreEqual("Inactive", Get(hex, "Phase").ToString());
            Assert.IsFalse(beam.enabled);
            yield return WaitGameplay(0.4f);
            Assert.IsFalse(hex.GetComponent<Animator>().enabled);
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            Call(trail, "ClearTrail");
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.7f);
            Assert.AreEqual("Inactive", Get(hex, "Phase").ToString());
            Assert.AreEqual(1f, (float)Get(score, "Hp"));
        }

        [Test]
        public void OutsideRing_PivotMatchesItsMeasuredCircularCenter()
        {
            Sprite ring = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Art/Nodes/Cyberpunk/Types/Cyberpunk_NormalNode/Cyberpunk_NormalNode_Outside.png");
            Assert.IsNotNull(ring);
            Assert.That(ring.pivot.x, Is.EqualTo(100f).Within(0.1f));
            Assert.That(ring.pivot.y, Is.EqualTo(91.5f).Within(0.1f));
        }

        [UnityTest]
        public IEnumerator InputOwnership_UiPauseFocusReleaseAndDiagonalMotion()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component stage = Find("StageManager");
            Component stick = Find("VirtualJoyStick");
            Component player = Find("PlayerCharacterController");
            // Both visible sound controls must affect the sources used by gameplay.
            Component sound = (Component)Get(Find("GameManager"), "soundManager");
            Set(sound, "SFXVolume", 0f);
            foreach (string sourceName in new[] { "sfxSource", "arrowDefence", "nodeExplosion", "coreDamaged", "laser" })
                Assert.AreEqual(0f, ((AudioSource)Get(sound, sourceName)).volume, sourceName);
            Set(sound, "SFXVolume", 0.5f);
            Canvas.ForceUpdateCanvases();
            Vector2 uiPoint = RectTransformUtility.WorldToScreenPoint(null, GameObject.Find("RemakeHUD/PauseButton").transform.position);
            bool overButton = (bool)Call(stick, "IsOverButton", uiPoint);
            Assert.IsTrue(overButton);
            Assert.IsFalse((bool)Call(stick, "TryBeginPointer", 11, uiPoint, overButton));
            Assert.IsTrue((bool)Call(stick, "TryBeginPointer", 11, Vector2.one * 100f, false));
            Assert.IsFalse((bool)Call(stick, "TryBeginPointer", 12, Vector2.one * 200f, false));
            Call(stick, "ReleasePointer", 12);
            Assert.IsTrue((bool)Get(stick, "HasGesture"));
            Call(stick, "SetMovement", Vector2.right);
            Set(player, "travelSpeed", 2f);
            Call(stage, "SetPaused", true);
            Assert.IsFalse((bool)Get(stick, "HasGesture"));
            Assert.AreEqual(Vector2.zero, (Vector2)Get(player, "MoveVector"));
            Assert.AreEqual(0f, (float)Get(player, "travelSpeed"));
            Call(stage, "SetPaused", false);
            Assert.IsFalse((bool)Get(stick, "keyboardReady"), "Held keyboard input must be released after pause.");
            Assert.IsTrue((bool)Call(stick, "TryBeginPointer", 12, Vector2.one * 200f, false));
            Set(player, "travelSpeed", 2f);
            Call(stick, "OnApplicationFocus", false);
            Assert.IsFalse((bool)Get(stick, "HasGesture"));
            Assert.AreEqual(0f, (float)Get(player, "travelSpeed"));
            Assert.IsTrue((bool)Call(stick, "TryBeginPointer", 13, Vector2.one * 200f, false));
            Set(player, "travelSpeed", 2f);
            Call(stick, "ReleasePointer", 13);
            Assert.IsFalse((bool)Get(stick, "HasGesture"));
            Assert.AreEqual(Vector2.zero, (Vector2)Get(player, "MoveVector"));
            Assert.AreEqual(2f, (float)Get(player, "travelSpeed"), "Normal release should keep momentum.");
            Assert.IsTrue((bool)Call(stick, "TryBeginPointer", 14, Vector2.one * 200f, false));
            Call(stick, "CancelPointer", 13);
            Assert.IsTrue((bool)Get(stick, "HasGesture"), "A different finger cannot cancel the owner.");
            Call(stick, "CancelPointer", 14);
            Assert.IsFalse((bool)Get(stick, "HasGesture"));
            Assert.AreEqual(0f, (float)Get(player, "travelSpeed"), "Canceled touch must stop immediately.");
            ((Behaviour)stick).enabled = false;
            Vector2 start = player.transform.position;
            Set(player, "MoveVector", Vector2.one);
            Assert.That(((Vector2)Get(player, "MoveVector")).magnitude, Is.EqualTo(1f).Within(0.001f));
            yield return WaitGameplay(0.2f);
            Vector2 driven = player.transform.position;
            Assert.That(Vector2.Distance(start, driven), Is.InRange(0.65f, 1.2f),
                "Acceleration should remain responsive without an instant speed jump.");
            Set(player, "MoveVector", Vector2.zero);
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            Vector2 releasePosition = body.position;
            yield return WaitGameplay(0.08f);
            Assert.Greater(Vector2.Distance(releasePosition, body.position), 0.05f,
                "A normal release should coast before stopping.");
            Call(player, "MoveStop");
            Vector2 stopped = player.transform.position;
            Vector2 stoppedPhysics = body.position;
            yield return WaitGameplay(0.12f);
            Assert.Less(Vector2.Distance(stoppedPhysics, body.position), 0.01f, "Release must not leave physical Rigidbody drift.");
            float interpolationStep = (float)Get(player, "MoveSpeed") * Time.fixedDeltaTime + 0.01f;
            Assert.Less(Vector2.Distance(stopped, player.transform.position), interpolationStep,
                "The visual transform may settle by one interpolation step; ongoing movement is invalid.");
            Call(stage, "Retry");
            yield return WaitForScene("Cyberpunk");
            Assert.IsFalse((bool)Get(Find("VirtualJoyStick"), "HasGesture"));
            Assert.AreEqual(Vector2.zero, (Vector2)Get(Find("PlayerCharacterController"), "MoveVector"));
            Assert.AreEqual(0f, (float)Get(Find("PlayerCharacterController"), "travelSpeed"));
        }

        [UnityTest]
        public IEnumerator OppositeInput_ChangesThePhysicalPathGradually()
        {
            Click("RemakeMenu/Start");
            yield return WaitForScene("Cyberpunk");
            DisablePatterns();
            Component player = Find("PlayerCharacterController");
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            Set(player, "MoveVector", Vector2.right);
            yield return WaitGameplay(0.12f);
            Vector2 reversalStart = body.position;
            Set(player, "MoveVector", Vector2.left);
            yield return WaitGameplay(0.04f); // New 1800 deg/s default: inspect the first two physics steps, before the 0.10 s reversal completes.
            Vector2 earlyTurn = body.position;
            Assert.Greater(earlyTurn.x, reversalStart.x,
                "Reversing at speed should continue forward briefly while turning.");
            Assert.Greater(Mathf.Abs(earlyTurn.y - reversalStart.y), 0.04f,
                "The actual Rigidbody path must curve, not just the sprite.");
            yield return WaitGameplay(0.32f);
            Assert.Less(((Vector2)Get(player, "travelDirection")).x, -0.9f);
            Assert.Less(body.position.x, earlyTurn.x,
                "After the curve, physical movement should follow the new direction.");
            Call(player, "MoveStop");
        }

        // EditMode keeps driving the enumerator after EnterPlayMode; plain engine
        // yield instructions are not a reliable clock here. Wait for observed
        // clocks/physics ticks, preserving the original timing assertions.
        private static void ConfigureHexPreparation(Component hex,float seconds)
        {
            Set(hex,"preparationDuration",seconds); // Retained fallback, same total test preparation target.
            Transform artwork=hex.transform.GetChild(0);
            foreach(string name in new[]{"Outside 1","Outside 2"}){
                var ring=artwork.Find(name);Assert.IsNotNull(ring);var rotation=ring.GetComponent(TypeFor("Rotator"));Assert.IsNotNull(rotation);
                float parts=2*(float)(double)TypeFor("HexRotationTimeline").GetField("DefaultFireAt").GetRawConstantValue();Set(rotation,"duration",seconds/parts);Set(rotation,"interval",seconds/parts); // Keep the test target time under the new default firing beat.
            }
        }
        private static void ConfigureHexFlight(Component hex,float seconds)
        {
            var tuning=TypeFor("CursorDashTuning");tuning.GetMethod("EnableDev").Invoke(null,new object[]{true});
            object[] args={"hex.attack",new Newtonsoft.Json.Linq.JValue((double)(decimal)seconds),null,false};Assert.IsTrue((bool)tuning.GetMethod("TrySet").Invoke(null,args),args[2]?.ToString());
        }
        private static IEnumerator WaitGameplay(float seconds)
        {
            float target = Time.time + seconds;
            float deadline = Time.realtimeSinceStartup + seconds + 5f;
            while (Time.time < target)
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Gameplay clock timed out.");
                yield return null;
            }
        }
        private static IEnumerator WaitRealtime(float seconds)
        {
            float target = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < target) yield return null;
        }
        private static IEnumerator WaitPhysicsStep()
        {
            float previous = Time.fixedTime;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (Time.fixedTime <= previous)
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Physics clock timed out.");
                yield return null;
            }
        }

        private static void DisablePatterns()
        {
            ((Behaviour)Find("PatternReader")).enabled = false;
            Call(Find("NodeCreator"), "StopNodes");
            Call(Find("PlayerCharacterAfterImage"), "ClearTrail");
            ((Behaviour)Find("VirtualJoyStick")).enabled = false;
            MuteEditorSources();
        }
        private static IEnumerator WaitForScene(string name)
        {
            // Deferred tool commands get one update before inspecting the in-scene transition state.
            yield return null;
            float deadline = Time.realtimeSinceStartup + 10f;
            bool Ready(){
                var stage=Find("StageManager");var space=Find("CursorDashSpaceFlow");
                if(space!=null&&name=="01.StageSelectScene")return SceneManager.GetActiveScene().name=="Cyberpunk"&&(bool)Get(space,"IsMenu");
                if(space!=null&&name=="Cyberpunk")return SceneManager.GetActiveScene().name==name&&stage!=null&&Get(stage,"State").ToString()=="Playing";
                return SceneManager.GetActiveScene().name==name;
            }
            while (!Ready())
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Scene transition timed out: actual="+SceneManager.GetActiveScene().name+" target="+name+" Stage="+(Find("StageManager")!=null));
                yield return null;
            }
            yield return null;
            yield return null;
            MuteEditorSources();
        }
        private static void Click(string path)
        {
            if(path=="RemakeMenu/Start"&&Find("CursorDashSpaceFlow")!=null){
                var space=Find("CursorDashSpaceFlow");Assert.IsTrue((bool)Get(space,"IsMenu"));
                Assert.IsTrue((bool)Call(space,"RequestStart",new Vector2(Screen.width*.5f,Screen.height*.5f)));return;
            }
            GameObject item = GameObject.Find(path);
            Assert.IsNotNull(item, path);
            Component button = item.GetComponent(TypeFor("UnityEngine.UI.Button"));
            Assert.IsNotNull(button, path + " button");
            UnityEvent click = (UnityEvent)Get(button, "onClick");
            Assert.Greater(click.GetPersistentEventCount(), 0, path + " callback");
            click.Invoke();
        }
        private static void MuteEditorSources()
        {
            foreach (AudioSource source in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None)) source.mute = true;
        }
        private static Type TypeFor(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(name);
                if (type != null) return type;
            }
            throw new Exception("Missing type " + name);
        }
        private static Component Find(string name)
        {
            return Resources.FindObjectsOfTypeAll(TypeFor(name)).OfType<Component>()
                .FirstOrDefault(component => component.gameObject.scene.IsValid() && component.gameObject.activeInHierarchy);
        }
        private static Component[] AllNodes() => GameObject.Find("Nodes").GetComponentsInChildren(TypeFor("Node"), true);
        private static Component[] NodesOfType(string name) => GameObject.Find("Nodes").GetComponentsInChildren(TypeFor(name), true);
        private static object Get(object item, string name)
        {
            Assert.IsNotNull(item, "Missing reflection target for " + name);
            PropertyInfo property = item.GetType().GetProperty(name, Flags);
            if (property != null) return property.GetValue(item);
            FieldInfo field = item.GetType().GetField(name, Flags);
            Assert.IsNotNull(field, item.GetType().FullName + " has no field/property " + name);
            return field.GetValue(item);
        }
        private static void Set(object item, string name, object value)
        {
            PropertyInfo property = item.GetType().GetProperty(name, Flags);
            if (property != null) property.SetValue(item, value);
            else item.GetType().GetField(name, Flags).SetValue(item, value);
        }
        private static object Call(object item, string name, params object[] arguments)
        {
            MethodInfo method = item.GetType().GetMethods(Flags).First(m => m.Name == name && m.GetParameters().Length == arguments.Length);
            return method.Invoke(item, arguments);
        }
    }

    public class CursorDashControlFeelTests
    {
        private static MethodInfo Method(string typeName, string methodName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(typeName))
                .FirstOrDefault(candidate => candidate != null);
            Assert.IsNotNull(type, typeName);
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, methodName);
            return method;
        }

        private static Vector2 Step(ref Vector2 direction, ref float speed, Vector2 input, float dt)
        {
            object[] args = { direction, speed, input, 5f, 60f, 18f, 540f, dt };
            Vector2 displacement = (Vector2)Method("PlayerCharacterController", "AdvanceMotion").Invoke(null, args);
            direction = (Vector2)args[0];
            speed = (float)args[1];
            return displacement;
        }


        [Test]
        public void CircularDash_SweptNoseStopsAtFirstEdgeEvenAcrossTheWholeCore()
        {
            MethodInfo hitTest = Method("NormalNode", "TryFirstSegmentCircleHit");
            object[] args = { new Vector2(0f, 2f), new Vector2(0f, -2f), Vector2.zero, 0.3255208f, Vector2.zero };
            Assert.IsTrue((bool)hitTest.Invoke(null, args));
            Assert.That(((Vector2)args[4]).y, Is.EqualTo(0.3255208f).Within(0.00001f));
            args = new object[] { new Vector2(2f, 2f), new Vector2(2f, -2f), Vector2.zero, 0.3255208f, Vector2.zero };
            Assert.IsFalse((bool)hitTest.Invoke(null, args), "A parallel miss must not count as damage.");
            args = new object[] { Vector2.zero, Vector2.zero, Vector2.zero, 0.3255208f, Vector2.zero };
            Assert.IsTrue((bool)hitTest.Invoke(null, args));
            args = new object[] { Vector2.one, Vector2.one, Vector2.zero, 0.3255208f, Vector2.zero };
            Assert.IsFalse((bool)hitTest.Invoke(null, args));
        }

        [Test]
        public void FloatingOrigin_FollowsOnlyBeyondTheStickRadius()
        {
            MethodInfo follow = Method("VirtualJoyStick", "FollowOrigin");
            Vector2 origin = Vector2.zero;
            Assert.AreEqual(origin, (Vector2)follow.Invoke(null, new object[] { origin, new Vector2(100f, 0f), 100f }));
            origin = (Vector2)follow.Invoke(null, new object[] { origin, new Vector2(150f, 0f), 100f });
            Assert.AreEqual(new Vector2(50f, 0f), origin);
            Vector2 input = (Vector2)Method("VirtualJoyStick", "CalculateInput")
                .Invoke(null, new object[] { new Vector2(150f, 0f) - origin, 100f, 0.12f });
            Assert.That(input.x, Is.EqualTo(1f).Within(0.001f));
            Assert.AreEqual(Vector2.zero, (Vector2)Method("VirtualJoyStick", "CalculateInput")
                .Invoke(null, new object[] { new Vector2(5f, 0f), 100f, 0.12f }));
        }

        [Test]
        public void ReleaseCoastsThenStops_AndSmallInputSetsAProportionalSpeed()
        {
            Vector2 direction = Vector2.right;
            float speed = 5f;
            Vector2 first = Step(ref direction, ref speed, Vector2.zero, 0.02f);
            Assert.Greater(first.x, 0f);
            Assert.That(speed, Is.EqualTo(4.64f).Within(0.001f));
            for (int i = 0; i < 16; i++) Step(ref direction, ref speed, Vector2.zero, 0.02f);
            Assert.AreEqual(0f, speed);
            Assert.AreEqual(Vector2.zero, Step(ref direction, ref speed, Vector2.zero, 0.02f));
            for (int i = 0; i < 20; i++) Step(ref direction, ref speed, Vector2.right * 0.25f, 0.02f);
            Assert.That(speed, Is.EqualTo(1.25f).Within(0.001f));
        }

        [Test]
        public void ReverseAtSpeedCurves_ButNearStopRespondsPromptly()
        {
            Vector2 direction = Vector2.right;
            float speed = 5f;
            Vector2 first = Step(ref direction, ref speed, Vector2.left, 0.02f);
            Assert.Greater(first.x, 0f);
            Assert.Greater(Mathf.Abs(first.y), 0.001f);
            Vector2 distance = first;
            for (int i = 0; i < 19; i++) distance += Step(ref direction, ref speed, Vector2.left, 0.02f);
            Assert.Less(direction.x, -0.9f);
            Assert.Greater(Mathf.Abs(distance.y), 0.4f);
            direction = Vector2.right;
            speed = 0.25f;
            Vector2 restart = Step(ref direction, ref speed, Vector2.left, 0.02f);
            Assert.Less(restart.x, 0f);
        }

        [Test]
        public void CurveAndCoastStayCloseAcrossPhysicsStepSizes()
        {
            Vector2 coarse = SimulateTurn(0.02f);
            Vector2 fine = SimulateTurn(0.01f);
            Assert.Less(Vector2.Distance(coarse, fine), 0.12f);
            Assert.That(SimulateCoast(0.02f), Is.EqualTo(SimulateCoast(0.01f)).Within(0.07f));
        }

        private static Vector2 SimulateTurn(float dt)
        {
            Vector2 direction = Vector2.right;
            float speed = 5f;
            Vector2 position = Vector2.zero;
            for (int i = 0; i < Mathf.RoundToInt(0.4f / dt); i++)
                position += Step(ref direction, ref speed, Vector2.left, dt);
            Assert.Less(direction.x, -0.9f);
            return position;
        }

        private static float SimulateCoast(float dt)
        {
            Vector2 direction = Vector2.right;
            float speed = 5f;
            float distance = 0f;
            for (int i = 0; i < Mathf.RoundToInt(0.4f / dt); i++)
                distance += Step(ref direction, ref speed, Vector2.zero, dt).magnitude;
            Assert.AreEqual(0f, speed);
            return distance;
        }
    }
}
