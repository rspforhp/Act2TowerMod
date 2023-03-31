using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using DiskCardGame;
using GBC;
using HarmonyLib;
using InscryptionAPI.Helpers;
using UnityEngine;
using UnityEngine.EventSystems;
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


        public static GameObject TowerOnMap;
        public static GameObject TowerOnGrid;
        public const string TowerScene="TowerScene";

    

        private void Awake()
        {
            Harmony.CreateAndPatchAll(this.GetType().Assembly);
            ModInstance = this;
            Logger.LogMessage($"Mod {nameof(Act2Tower)} loaded!");
            AssetBundleHelper.TryGet(Path.Combine(Paths.PluginPath, "towerprefabs"), "TOWER", out TowerOnMap);
            AssetBundleHelper.TryGet(Path.Combine(Paths.PluginPath, "towerprefabs"), "TowerGrid", out TowerOnGrid);
            AssetBundle.LoadFromFile(Path.Combine(Paths.PluginPath, "towerscene"));
            SceneManager.sceneLoaded+= OnSceneManagerOnsceneLoaded;
        }

      

        private void OnSceneManagerOnsceneLoaded(Scene arg0, LoadSceneMode mode)
        {
            if (arg0.name.Contains("GBC")|| arg0.name.Equals(TowerScene, StringComparison.InvariantCultureIgnoreCase))
            {
                if (GBCEncounterManager.Instance == null|| GBCEncounterManager.Instance is null)
                {
                    Logger.LogMessage("Tried to create gbcencountermanager");
                    SceneLoader.Load("GBC_Mycologist_Hut");
                    SceneLoader.Load(arg0.name);
                }
            }
            if (arg0.name == "GBC_WorldMap")
            {

              
                
                Instantiate(TowerOnMap);
                var g=Instantiate(TowerOnGrid);
                var navGrid = NavigationGrid.instance;
                for (int i = 0; i < navGrid.zones.GetLength(0); i++)
                {
                    for (int j = 0; j < navGrid.zones.GetLength(1); j++)
                    {
                        var zone = navGrid.zones[i, j];
                        _Logger.LogInfo($"[{i} {j}]={zone?.gameObject?.name}");
                    }
                }

                navGrid.zones[2, 3] = g.GetComponentInChildren<NavigationZone>();
                ((NavigationZone2D)navGrid.zones[2, 3]).blockedDirections.Add(LookDirection.North);
            }
        }
    }
}