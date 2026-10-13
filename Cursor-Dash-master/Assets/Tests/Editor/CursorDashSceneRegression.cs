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
    public class CursorDashSceneRegression
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const string Prefix = "CursorDash.Regression.";
        private static readonly string[] PreferenceKeys = { "CursorDash_Best_Cyberpunk", "BGM", "SFX" };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
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
            EditorSceneManager.OpenScene("Assets/Scenes/01.StageSelectScene.unity", OpenSceneMode.Single);
            yield return new EnterPlayMode();
            yield return null;
            yield return null;
            MuteEditorSources();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
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
        }

        [UnityTest]
        public IEnumerator RoundLifecycle_PauseEndRetryAndHomeRestoreState()
        {
            Assert.IsNotNull(Find("MainSceneController"));
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
            yield return WaitGameplay(0.65f);
            Assert.AreEqual(0, (int)Get(trail, "ActiveSegmentCount"));

            Set(score, "Hp", 0f);
            yield return WaitRealtime(0.6f);
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
            Assert.IsNull(Find("StageManager"));
            Assert.IsNotNull(Find("MainSceneController"));
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
            Set(normal, "lineDuration", 0.2f);
            Set(hex, "preparationDuration", 0.06f);
            Set(hex, "warningDuration", 0.05f);
            Set(hex, "attackDuration", 0.35f);

            // Real Physics2D body overlap goes through PlayerCharacterController.OnTriggerEnter2D.
            normal.transform.position = player.transform.position;
            Call(normal, "Execute");
            yield return WaitPhysicsStep();
            yield return WaitPhysicsStep();
            Assert.AreEqual(1, (int)Get(stage, "DestroyedNodes"));
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            yield return WaitRealtime(0.35f);
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
            Assert.AreEqual(2f, (float)Get(score, "Hp"), "A live trail cannot stop a circular-node beam.");
            Assert.AreEqual(0, (int)Get(stage, "BlockedBeams"));

            Call(trail, "ClearTrail");
            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            Call(hex, "Interaction");
            Assert.AreEqual("Preparing", Get(hex, "Phase").ToString(), "Touching a hexagonal body must not destroy it.");
            yield return WaitGameplay(0.3f);
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            yield return WaitRealtime(0.3f);

            Call(trail, "ClearTrail");
            Set(hex, "preparationDuration", 0.18f);
            Set(hex, "attackDuration", 0.2f);
            Call(trail, "RecordMovement", new Vector2(-1f, 1f), new Vector2(1f, 1f), Time.time - 0.48f);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.1f);
            Assert.AreEqual("Preparing", Get(hex, "Phase").ToString());
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));
            yield return WaitGameplay(0.4f);
            Assert.AreEqual(1f, (float)Get(score, "Hp"), "A trail expired before firing must not reserve defense.");
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));

            Call(trail, "ClearTrail");
            Set(hex, "preparationDuration", 0.05f);
            Set(hex, "attackDuration", 0.4f);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.12f);
            Assert.AreEqual("Fired", Get(hex, "Phase").ToString());
            Call(trail, "RecordMovement", new Vector2(-1f, 1.8f), new Vector2(1f, 1.8f), Time.time);
            yield return WaitGameplay(0.08f);
            Assert.AreEqual(2, (int)Get(stage, "BlockedBeams"), "A new live trail intersecting the visible fired beam must block it.");
            Assert.AreEqual(1f, (float)Get(score, "Hp"));
        }

        [UnityTest]
        public IEnumerator CircularNode_WarnsUntilFire_CanBeDestroyedAndResetsForReuse()
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
            Vector3 originalScale = normal.transform.localScale;
            Vector3 originalRingScale = ring.localScale;
            float originalLineWidth = beam.widthMultiplier;
            Assert.That((float)Get(trail, "Lifetime"), Is.EqualTo(0.6f).Within(0.00001f));
            Assert.That(trail.GetComponent<TrailRenderer>().time, Is.EqualTo(0.6f).Within(0.00001f));
            Assert.IsFalse(((AudioSource)Get((Component)Get(Find("GameManager"), "soundManager"), "BGMSource")).isPlaying);
            Assert.IsNotNull((AudioSource)Get((Component)Get(Find("GameManager"), "soundManager"), "SFXSource"));
            Assert.That((float)Get(normal, "preparationDuration"), Is.EqualTo(2.5f).Within(0.00001f));
            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            yield return WaitGameplay(2.56f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            ScreenCapture.CaptureScreenshot("Captures/node-alignment-runtime.png");
            yield return null;
            Call(normal, "ResetObject");
            Set(normal, "preparationDuration", 0.1f);
            Set(normal, "warningDuration", 0.3f);
            Set(normal, "lineDuration", 0.12f);

            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            Assert.AreEqual("Preparing", Get(normal, "Phase").ToString());
            yield return WaitGameplay(0.15f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            Assert.IsTrue(body.enabled, "The circular node remains destructible during warning.");
            Assert.IsFalse(beam.enabled);
            Assert.Less(normal.transform.localScale.x, originalScale.x);
            Assert.Less(ring.localScale.x, originalRingScale.x);
            ScreenCapture.CaptureScreenshot("Captures/node-warning-runtime.png");
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

            normal.transform.position = new Vector2(0f, 2f);
            Call(normal, "Execute");
            yield return WaitGameplay(0.15f);
            Assert.AreEqual("Warning", Get(normal, "Phase").ToString());
            yield return WaitGameplay(0.32f);
            Assert.AreEqual("Fired", Get(normal, "Phase").ToString());
            Assert.IsFalse(body.enabled);
            Assert.IsTrue(beam.enabled);
            Assert.That(beam.widthMultiplier, Is.EqualTo(originalLineWidth * 1.6f).Within(0.0001f));
            yield return WaitGameplay(0.18f);
            Assert.AreEqual(2f, (float)Get(score, "Hp"));
            Assert.AreEqual("Inactive", Get(normal, "Phase").ToString());
            Assert.AreEqual(originalScale, normal.transform.localScale);
            Assert.AreEqual(originalRingScale, ring.localScale);
            Assert.AreEqual(originalLineWidth, beam.widthMultiplier);
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
            LineRenderer beam = hex.GetComponent<LineRenderer>();
            Transform body = hex.transform.GetChild(0);
            Transform outsideOne = body.Find("Outside 1");
            Transform outsideTwo = body.Find("Outside 2");
            Vector3 bodyScale = body.localScale;
            Vector3 outsideOneScale = outsideOne.localScale;
            Vector3 outsideTwoScale = outsideTwo.localScale;
            float baseWidth = beam.widthMultiplier;
            float baseBlockRadius = beam.startWidth * 0.5f;
            Set(hex, "preparationDuration", 0.1f);
            Set(hex, "warningDuration", 0.3f);
            Set(hex, "attackDuration", 0.25f);
            hex.transform.position = new Vector2(0f, 2f);
            Call(hex, "Execute");
            yield return WaitGameplay(0.15f);
            Assert.AreEqual("Warning", Get(hex, "Phase").ToString());
            Assert.IsFalse(beam.enabled);
            Assert.IsFalse(hex.GetComponent<CircleCollider2D>().enabled);
            Assert.Less(body.localScale.x, bodyScale.x);
            Assert.Less(outsideOne.localScale.x, outsideOneScale.x);
            Assert.Less(outsideTwo.localScale.x, outsideTwoScale.x);
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
            yield return WaitGameplay(0.31f);
            Assert.AreEqual("Fired", Get(hex, "Phase").ToString());
            Assert.IsTrue(beam.enabled);
            Assert.That(beam.widthMultiplier, Is.EqualTo(baseWidth * 1.6f).Within(0.0001f));
            Assert.That((float)Get(hex, "beamDefenseRadius"), Is.EqualTo(baseBlockRadius).Within(0.0001f),
                "The brighter beam must keep its previous blocking radius.");
            Call(trail, "RecordMovement", new Vector2(-1f, 1.8f), new Vector2(1f, 1.8f), Time.time);
            yield return WaitGameplay(0.06f);
            Assert.AreEqual(1, (int)Get(stage, "BlockedBeams"));
            Assert.AreEqual(3f, (float)Get(score, "Hp"));
            yield return WaitRealtime(0.3f);
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
            yield return WaitGameplay(0.08f);
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
            float deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != name)
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Scene transition timed out.");
                yield return null;
            }
            yield return null;
            yield return null;
            MuteEditorSources();
        }
        private static void Click(string path)
        {
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
            PropertyInfo property = item.GetType().GetProperty(name, Flags);
            return property != null ? property.GetValue(item) : item.GetType().GetField(name, Flags).GetValue(item);
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
