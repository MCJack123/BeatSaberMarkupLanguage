﻿using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using BS_Utils.Utilities;
using IPA;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace BeatSaberMarkupLanguage
{
    public class Plugin : IBeatSaberPlugin
    {
        public static Config config;
        public void Init(IPALogger logger)
        {
            Logger.log = logger;
            BSEvents.menuSceneLoadedFresh += MenuLoadFresh;
            config = new Config("BSML");
        }
        public void MenuLoadFresh()
        {
            //BSMLSettings.instance.AddSettingsMenu("Test", "BeatSaberMarkupLanguage.Views.settings-test.bsml", SettingsTest.instance);
            //Resources.FindObjectsOfTypeAll<GameScenesManager>().FirstOrDefault().StartCoroutine(PresentTest());
            BSMLSettings.instance.Setup();
            MenuButtons.MenuButtons.instance.Setup();
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode) { }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "MenuViewControllers" && prevScene.name == "EmptyTransition")
                BSMLParser.instance.MenuSceneLoaded();
        }

        public void OnApplicationStart() { }
        public void OnApplicationQuit() { }
        public void OnSceneUnloaded(Scene scene) { }
        public void OnUpdate() { }
        public void OnFixedUpdate() { }

        //It's just for testing so don't yell at me
        private IEnumerator PresentTest()
        {
            yield return new WaitForSeconds(1);
            TestViewController testViewController = BeatSaberUI.CreateViewController<TestViewController>();
            Resources.FindObjectsOfTypeAll<MainFlowCoordinator>().First().InvokeMethod("PresentViewController", new object[] { testViewController, null, false });
        }
    }
}
