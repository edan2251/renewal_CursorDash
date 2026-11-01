#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace CursorDashTests
{
    public sealed class CursorDashHierarchyTests
    {
        private static Type T(string name)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(name)).First(t=>t!=null);
        private static object Get(object o,string name)=>o.GetType().GetProperty(name).GetValue(o);
        [SetUp]public void SavedTargetOnly()
        {
            Assert.IsFalse(EditorApplication.isPlayingOrWillChangePlaymode);Assert.IsNull(PrefabStageUtility.GetCurrentPrefabStage());
            Assert.IsFalse(SceneManager.GetActiveScene().isDirty);
            if(SceneManager.GetActiveScene().path!="Assets/Scenes/Cyberpunk.unity")EditorSceneManager.OpenScene("Assets/Scenes/Cyberpunk.unity");
        }
        [Test]public void RootDividersAreEmptyAndDoNotReparentSceneObjects()
        {
            var roots=SceneManager.GetActiveScene().GetRootGameObjects();
            CollectionAssert.AreEqual(new[]{"----[Manager]----","GameManager","StageManager","----[Camera & Input]----","Main Camera","EventSystem","VirtualJoyStick","----[Background]----","BoardView","----[Gameplay]----","PlayerCharacter","----[Pool & Spawn]----","Nodes","AfterImageCollision","GeneratePositions","----[UI]----","UI","Flash","RemakeHUD","MainLogo","MainMenuView","ProjectedScoreView"},roots.Select(g=>g.name));
            foreach(var root in roots.Where(g=>g.name.StartsWith("----["))){Assert.AreEqual(1,root.GetComponents<Component>().Length);Assert.AreEqual(0,root.transform.childCount);Assert.AreEqual(Vector3.zero,root.transform.position);Assert.AreEqual(Vector3.one,root.transform.localScale);}
            foreach(var name in new[]{"GameObject","DeathUI","Pause UI","Hold Achivement","EnemyArrows","Cyberpunk_Background"})Assert.IsFalse(roots.Any(g=>g.name==name));
        }
        [Test]public void RealPoolsSpawnPositionsAndFallbackControllersRemainExplicitlyConnected()
        {
            var stage=(Component)Object.FindFirstObjectByType(T("StageManager"));var data=new SerializedObject(stage);
            foreach(string field in new[]{"hpBarImage","hpBarFrame","timeScoreLabel","mainLogoView","mainMenuView","settingsView","projectedScoreView","nexusView","guidanceView","boardView","coreDeathAnimation"})Assert.IsNotNull(data.FindProperty(field).objectReferenceValue,field);
            var death=(Component)data.FindProperty("deathUIController").objectReferenceValue;var pause=(Component)data.FindProperty("pauseHandler").objectReferenceValue;
            Assert.AreEqual("RemakeHUD/ResultPanel",AnimationUtility.CalculateTransformPath(death.transform,null));Assert.AreEqual("RemakeHUD/PausePanel",AnimationUtility.CalculateTransformPath(pause.transform,null));
            var creator=stage.GetComponent(T("NodeCreator"));var creatorData=new SerializedObject(creator);Assert.AreSame(GameObject.Find("Nodes"),creatorData.FindProperty("nodesObject").objectReferenceValue);
            var positions=creatorData.FindProperty("generatePositions");Assert.Greater(positions.arraySize,0);for(int i=0;i<positions.arraySize;i++)Assert.IsTrue(((Transform)positions.GetArrayElementAtIndex(i).objectReferenceValue).IsChildOf(GameObject.Find("GeneratePositions").transform));
            var trail=Object.FindFirstObjectByType(T("PlayerCharacterAfterImage"));Assert.AreSame(GameObject.Find("AfterImageCollision"),new SerializedObject(trail).FindProperty("trailCollisions").objectReferenceValue);
            Assert.IsTrue((bool)Get(Get(stage,"BoardView"),"IsConfigured"));
            foreach(var root in SceneManager.GetActiveScene().GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Transform>(true))Assert.AreEqual(0,GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));
        }
    }
}
#endif
