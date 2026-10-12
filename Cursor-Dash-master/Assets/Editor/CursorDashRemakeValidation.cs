using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

public static class CursorDashRemakeValidation
{
    [MenuItem("Tools/CursorDash/Validate Input Math")]
    public static void ValidateInput()
    {
        Assert.AreEqual(Vector2.zero, VirtualJoyStick.CalculateInput(Vector2.zero, 100f, 0.12f));
        Assert.AreEqual(Vector2.zero, VirtualJoyStick.CalculateInput(new Vector2(10f, 0f), 100f, 0.12f));
        Assert.AreApproximatelyEqual((0.5f - 0.12f) / 0.88f,
            VirtualJoyStick.CalculateInput(new Vector2(50f, 0f), 100f, 0.12f).x, 0.0001f);
        Assert.AreApproximatelyEqual(1f,
            VirtualJoyStick.CalculateInput(new Vector2(100f, 100f), 100f, 0.12f).magnitude, 0.0001f);
        Assert.AreEqual(Vector2.zero, VirtualJoyStick.CalculateInput(Vector2.one, 0f, 0.12f));
        Assert.AreEqual(new Vector2(2f, -2f),
            PlayerCharacterController.ClampPosition(new Vector2(8f, -8f), new Vector2(-2f, -2f), new Vector2(2f, 2f)));
        GameObject item = new GameObject("Input validation") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            VirtualJoyStick stick = item.AddComponent<VirtualJoyStick>();
            Assert.IsFalse(stick.TryBeginPointer(1, Vector2.zero, true));
            Assert.IsTrue(stick.TryBeginPointer(1, Vector2.zero, false));
            Assert.IsFalse(stick.TryBeginPointer(2, Vector2.one, false));
            stick.ReleasePointer(2);
            Assert.IsTrue(stick.HasGesture);
            stick.ReleasePointer(1);
            Assert.IsFalse(stick.HasGesture);
            Assert.IsTrue(stick.TryBeginPointer(2, Vector2.one, false));
            stick.ResetInput();
            Assert.IsFalse(stick.HasGesture);
        }
        finally { Object.DestroyImmediate(item); }
        Debug.Log("CURSORDASH_INPUT_VALIDATION_PASS: 13 math/ownership assertions. Native device input remains a separate check.");
    }

    [MenuItem("Tools/CursorDash/Validate Trail And Patterns")]
    public static void ValidateTrailAndPatterns()
    {
        Assert.IsTrue(PlayerCharacterAfterImage.IsSegmentAlive(true, 1f, 1.49f, 0.5f));
        Assert.IsFalse(PlayerCharacterAfterImage.IsSegmentAlive(true, 1f, 1.5f, 0.5f));
        Assert.IsFalse(PlayerCharacterAfterImage.IsSegmentAlive(false, 1f, 1.1f, 0.5f));
        Assert.IsFalse(PlayerCharacterAfterImage.IsSegmentAlive(true, 2f, 1f, 0.5f));
        float along;
        Assert.AreApproximatelyEqual(0f, PlayerCharacterAfterImage.DistanceBetweenSegments(
            new Vector2(-1f, 0f), new Vector2(1f, 0f), new Vector2(0f, -1f), new Vector2(0f, 1f), out along), 0.0001f);
        Assert.AreApproximatelyEqual(0.5f, along, 0.0001f);
        Assert.AreApproximatelyEqual(1f, PlayerCharacterAfterImage.DistanceBetweenSegments(
            Vector2.zero, Vector2.right, Vector2.up, Vector2.one, out along), 0.0001f);
        Assert.AreApproximatelyEqual(1f, PlayerCharacterAfterImage.DistanceBetweenSegments(
            Vector2.zero, Vector2.zero, Vector2.right, Vector2.right * 2f, out along), 0.0001f);

        var patterns = PatternReader.ParsePatterns("// test\r\n0/0\r\n#0.5\r\n1/1,\n2/0");
        Assert.AreEqual(2, patterns.Count);
        Assert.AreEqual(3, patterns[0].PatternSteps.Count);
        Assert.AreApproximatelyEqual(0.5f, patterns[0].PatternSteps[1].WaitingTime, 0.0001f);
        Assert.AreEqual(1, patterns[0].PatternSteps[2].NodeStyle);
        MapInformation map = AssetDatabase.LoadAssetAtPath<MapInformation>("Assets/Resources/Maps/Cyberpunk.asset");
        Assert.IsNotNull(map.mainPatternFile);
        Assert.IsNotNull(map.subPatternFile);
        Assert.AreNotEqual(map.mainPatternFile, map.subPatternFile);
        Assert.IsTrue(PatternReader.ParsePatterns(map.mainPatternFile.text).Count > 0);
        Assert.IsTrue(PatternReader.ParsePatterns(map.subPatternFile.text).Count > 0);
        Debug.Log("CURSORDASH_TRAIL_PATTERN_VALIDATION_PASS: 17 expiry/geometry/parser/asset assertions.");
    }
}
