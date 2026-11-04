using UnityEngine;

public partial class NodeCreator
{
    // Only the multiplier reader uses this path. Tutorial/ghost/manual spawn paths remain unchanged.
    public bool TryCreateEncounterNode(int kind, int lane, CursorDashEncounterTier tier, CursorDashDifficultyProfile profile)
    {
        var stage = StageManager.instance; var camera = Camera.main;
        if (stage == null || !stage.IsPlaying || stage.NormalThreatsSuppressed || CursorDashSession.IsPractice ||
            camera == null || generatePositions == null || generatePositions.Length < 4 || kind < 0 || kind > 1 ||
            ActiveCount >= Mathf.Min(activeLimit, tier.activeLimit)) return false;
        if (kind == 1)
        {
            int activeHexes = 0;
            foreach (var hex in arrowNodes) if (hex.gameObject.activeSelf) activeHexes++;
            if (activeHexes >= tier.hexLimit) return false;
        }
        var node = GetAvailable(kind == 0 ? normalNodes : arrowNodes);
        if (node == null || node is LaserNode) return false;
        float radius = EncounterFootprint(node);
        Vector2 low = (Vector2)camera.ViewportToWorldPoint(new Vector3(.12f, .12f)) + Vector2.one * radius;
        Vector2 high = (Vector2)camera.ViewportToWorldPoint(new Vector3(.88f, .88f)) - Vector2.one * radius;
        if (high.x <= low.x || high.y <= low.y) return false;
        Vector2 center = stage.NexusPosition;
        Vector2 anchor = PlayerCharacterController.ClampPosition(generatePositions[Mathf.Clamp(lane, 0, 3)].position, low, high);
        Vector2 offset = anchor - center;
        float baseAngle = Mathf.Atan2(offset.y, offset.x), distance = offset.magnitude;
        var player = PlayerCharacterController.instance;
        float headRadius = .1f;
        if (player != null && player.TryGetComponent<BoxCollider2D>(out var head)) headRadius = head.bounds.extents.magnitude;
        for (int attempt = 0; attempt < 24; attempt++)
        {
            // A stable sector per event, with increasing angular variation, rather than ever-closer threats.
            float angle = baseAngle + (SampleEncounter() * 2 - 1) * tier.angularSpread * Mathf.Deg2Rad;
            float r = Mathf.Max(.01f, distance + (SampleEncounter() * 2 - 1) * tier.radialSpread);
            Vector2 point = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
            if (point.x < low.x || point.x > high.x || point.y < low.y || point.y > high.y) continue;
            if (!CursorDashEncounterRules.Separated(point.x, point.y, radius, center.x, center.y, stage.NexusRadius, profile.nexusClearance)) continue;
            if (player != null && !CursorDashEncounterRules.Separated(point.x, point.y, radius, player.transform.position.x, player.transform.position.y, headRadius, profile.cursorClearance)) continue;
            if (stage.Fever != null && stage.Fever.OverlapsHud(point, radius, camera)) continue;
            bool free = true;
            foreach (var other in allNodes)
            {
                if (!other.gameObject.activeSelf) continue;
                if (!CursorDashEncounterRules.Separated(point.x, point.y, radius, other.transform.position.x, other.transform.position.y, EncounterFootprint(other), profile.nodeGap)) { free = false; break; }
            }
            if (!free) continue;
            node.transform.position = point; node.SetDemonstration(null); node.Execute();
            return node.gameObject.activeSelf;
        }
        return false;
    }
    private float SampleEncounter() => seededRandom == null ? Random.value : (float)seededRandom.NextDouble();
    private static float EncounterFootprint(Node node)
    {
        if (node is NormalNode circle) return Mathf.Max(.1f, circle.TutorialFootprint);
        if (node is ArrowNode hex) return Mathf.Max(.1f, hex.TutorialFootprint);
        return Mathf.Max(.1f, node.RestColliderRadius * Mathf.Abs(node.transform.lossyScale.x));
    }
}
