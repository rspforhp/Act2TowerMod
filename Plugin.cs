using System;
using System.Collections.Generic;
using Act2Tower.API;
using BepInEx;
using BepInEx.Logging;
using GBC;
using InscryptionAPI.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Act2Tower
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Act2Tower : BaseUnityPlugin
    {
        public const string PluginGuid = "inscryption.kopie.act2tower";
        public const string PluginName = "Act2Tower";
        public const string PluginVersion = "0.0.1";
        internal static Act2Tower ModInstance;
        internal static ManualLogSource _Logger => ModInstance.Logger;

        public static event Action<Scene,GameObject> OnGBCMapLoaded;


        public void GBCMapLoaded_Handling(Scene s, GameObject MapBase)
        {
            Vector3 pos = new Vector3(0.03f, -0.3f, 0);
            string name = "Test of bridge";
            Sprite bridgeSpr = TextureHelper.GetImageAsTexture("Artwork\\Bridge.png").ConvertTexture();
            var testBridge = Act2MapBuilding.CreateGBCGameObject(name, MapBase, pos, bridgeSpr);
            var testBridgeNavZone=Act2MapBuilding.CreateGBCNaviationZone(name, MapBase, pos, new List<LookDirection>(), 3, 3);
            var testBridgeInside = Act2MapBuilding.TryCreateGBCEntranceScene("BridgeInsides");
            Act2MapBuilding.AddNavigationZoneEntrance(testBridgeNavZone, testBridgeInside,LookDirection.South);
        }

        private void Awake()
        {
            ModInstance = this;
            Logger.LogMessage($"Mod {nameof(Act2Tower)} loaded!");
            SceneManager.sceneLoaded+= delegate(Scene argScene, LoadSceneMode mode)
            {
                if (argScene.name == "GBC_WorldMap")
                {
                   if(OnGBCMapLoaded!=null)OnGBCMapLoaded(argScene, GameObject.Find("Map").transform.GetChild(1).gameObject);
                }
            };
            OnGBCMapLoaded += GBCMapLoaded_Handling;
            
            




        }
    }
}