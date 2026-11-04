#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CursorDashTests
{
    // Only serialized data / pure rules. Does not enter Play, save scenes or write player preferences.
    public sealed class CursorDashDifficultyTests
    {
        private static Type T(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
        private static object Get(object value, string name) => value.GetType().GetProperty(name).GetValue(value);
        private static object Field(object value, string name) => value.GetType().GetField(name).GetValue(value);
        private static object Call(object value, string name, params object[] args) => value.GetType().GetMethod(name).Invoke(value, args);
        private static object Rule(string name, params object[] args) => T("CursorDashEncounterRules").GetMethod(name).Invoke(null, args);
        private static UnityEngine.Object Profile() => AssetDatabase.LoadMainAssetAtPath("Assets/Resources/CursorDash/MultiplierDifficulty.asset");

        [Test] public void ProfileHasFourMonotonicTiersAndNoChangesToAttackTimings()
        {
            var profile = Profile(); Assert.IsNotNull(profile); Assert.IsTrue((bool)Get(profile, "IsValid"));
            Assert.IsTrue((bool)Field(profile, "useMultiplierEncounters"));
            var tiers = (Array)Field(profile, "tiers"); Assert.AreEqual(4, tiers.Length);
            float lastSpacing = 2, lastSpread = -1; int lastLimit = 0;
            foreach (var tier in tiers)
            {
                float spacing = (float)Field(tier, "spacingScale"), spread = (float)Field(tier, "angularSpread");
                int limit = (int)Field(tier, "activeLimit");
                Assert.Less(spacing, lastSpacing); Assert.GreaterOrEqual(spread, lastSpread); Assert.GreaterOrEqual(limit, lastLimit);
                Assert.LessOrEqual((int)Field(tier, "hexLimit"), 3); Assert.LessOrEqual(limit, 6);
                lastSpacing = spacing; lastSpread = spread; lastLimit = limit;
                Assert.IsNull(tier.GetType().GetField("attackSpeed"));
            }
        }
        [Test] public void EveryTierSuppliesBothKindsAndRecoveryWithoutImmediatePhraseRepeats()
        {
            var patterns = (Array)Field(Profile(), "patterns"); var random = new System.Random(7331);
            for (int multiplier = 1; multiplier <= 4; multiplier++)
            {
                int previous = -1, circle = 0, hex = 0;
                for (int phrase = 0; phrase < 180; phrase++)
                {
                    bool recovery = phrase % 3 == 2;
                    int index = (int)Rule("Choose", patterns, multiplier, recovery, previous, random.NextDouble());
                    Assert.That(index, Is.InRange(0, patterns.Length - 1)); Assert.AreNotEqual(previous, index);
                    var chosen = patterns.GetValue(index); Assert.AreEqual(recovery, Field(chosen, "recovery"));
                    Assert.Greater((int)Rule("Weight", chosen, multiplier), 0);
                    foreach (var step in (Array)Field(chosen, "steps")) if ((int)Field(step, "kind") == 0) circle++; else hex++;
                    previous = index;
                }
                Assert.Greater(circle, 0); Assert.Greater(hex, 0);
            }
        }
        [Test] public void RotationsAndMirrorsPreserveOppositeAndDistinctLanes()
        {
            for (int rotation = 0; rotation < 4; rotation++) foreach (bool mirror in new[] { false, true })
            {
                var lanes = Enumerable.Range(0, 4).Select(i => (int)Rule("Lane", i, rotation, mirror)).ToArray();
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3 }, lanes);
                Assert.AreEqual(3, lanes[0] + lanes[2]); Assert.AreEqual(3, lanes[1] + lanes[3]);
            }
        }
        [Test] public void FeverAndDamageInvalidatePendingEventsButPauseDoesNot()
        {
            var pacer = Activator.CreateInstance(T("CursorDashEncounterPacer"));
            void Observe(int tier, int completed, uint reset, double time, bool suppressed) => Call(pacer, "Observe", tier, completed, reset, time, suppressed, .8d, .8d);
            Observe(1, 0, 1, 2, false); int initial = (int)Get(pacer, "Revision");
            for (int i = 0; i < 100; i++) Observe(1, 0, 1, 2, false);
            Assert.AreEqual(initial, Get(pacer, "Revision"));
            Observe(1, 0, 1, 2, true); Assert.Greater((int)Get(pacer, "Revision"), initial); Assert.IsFalse((bool)Call(pacer, "Ready", 200d));
            Observe(2, 1, 1, 2, false); Assert.AreEqual(2, Get(pacer, "Multiplier"));
            Assert.IsFalse((bool)Call(pacer, "Ready", 2.79d)); Assert.IsTrue((bool)Call(pacer, "Ready", 2.8d));
            Observe(4, 8, 1, 5, false); Observe(1, 8, 2, 6, false);
            Assert.AreEqual(1, Get(pacer, "Multiplier")); Assert.AreEqual(6.8d, Get(pacer, "ReadyAt"));
            Observe(1, 8, 3, 6.1, false); Assert.That((double)Get(pacer, "ReadyAt"), Is.EqualTo(6.9d).Within(1e-8));
        }
        [Test] public void RealFeverModelKeepsRewardsAndTimingWhileDrivingTier()
        {
            var model = Activator.CreateInstance(T("FeverState"), new object[] { null });
            var pacer = Activator.CreateInstance(T("CursorDashEncounterPacer"));
            void Observe() => Call(pacer, "Observe", Get(model, "Multiplier"), Get(model, "CompletedFevers"), Get(model, "ResetVersion"), Get(model, "NormalCombatSeconds"), Get(model, "SuppressesThreats"), .8d, .8d);
            Call(model, "ResetRun"); Observe();
            for (int cycle = 0; cycle < 6; cycle++)
            {
                int expected = Math.Min(4, cycle + 1);
                for (int i = 0; i < 8; i++) Call(model, "Kill", cycle * 8 + i, 1ul);
                Assert.AreEqual("Closing", Get(model, "Current").ToString()); Observe();
                for (int phase = 0; phase < 4; phase++)
                {
                    if (Get(model, "Current").ToString() == "Fever") Assert.AreEqual(8d, Get(model, "PhaseSeconds"));
                    Call(model, "Tick", Get(model, "PhaseSeconds")); Observe();
                    if (phase < 3) Assert.AreEqual(expected, Get(pacer, "Multiplier"));
                }
                Assert.AreEqual(Math.Min(4, expected + 1), Get(pacer, "Multiplier"));
            }
            string score = Get(model, "PointsText").ToString(); Call(model, "NexusHit"); Observe();
            Assert.AreEqual(1, Get(pacer, "Multiplier")); Assert.AreEqual(score, Get(model, "PointsText"));
            Call(model, "ResetRun"); Observe(); Assert.AreEqual(1, Get(pacer, "Multiplier"));
        }
        [Test] public void FootprintSpacingIncludesBothNodesAndRequestedGap()
        {
            Assert.IsFalse((bool)Rule("Separated", 0d, 0d, .4d, .9d, 0d, .4d, .18d));
            Assert.IsTrue((bool)Rule("Separated", 0d, 0d, .4d, 1d, 0d, .4d, .18d));
        }
    }
}
#endif
