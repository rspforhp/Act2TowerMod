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
        public Sprite TowerSprite;
        public Sprite TowerFloorSprite;
        public void GBCMapLoaded_Handling(Scene s, GameObject MapBase)
        {
            Vector3 pos = new Vector3(0.03f, -0.3f, 0);
            string name = "Test of bridge";
            Sprite bridgeSpr = TextureHelper.GetImageAsTexture("Artwork\\Bridge.png").ConvertTexture();
            var testBridge = Act2MapBuilding.CreateGBCGameObject(name, MapBase, pos, bridgeSpr);
            Act2MapBuilding.CreateGBCGameObject("InfinitTowerSprite", MapBase, pos+new Vector3(-0.25f,0.12f,0), TowerSprite);
            var testBridgeNavZone=Act2MapBuilding.CreateGBCNaviationZone(name, MapBase, pos, new List<LookDirection>(), 3, 3);
            var testBridgeInside = Act2MapBuilding.TryCreateGBCEntranceScene("BridgeInsides",true);
            Act2MapBuilding.AddNavigationZoneEntrance(testBridgeNavZone, testBridgeInside,LookDirection.West);
            Act2MapBuilding.OnGBCZoneSceneLoaded+= delegate(GameObject o)
            {
                if (o == testBridgeInside)
                {
                    Logger.LogMessage($"A custom zone entered;");
                    GBC.PlayerMovementController.Instance.transform.position = new Vector3(0, -0.9f, 0);
                    var floor = new GameObject("Floor");
                    floor.transform.position = new Vector3(0, 0, 0);
                    floor.transform.localScale = new Vector3(1.1f, 1, 1);
                    floor.layer = LayerMask.NameToLayer("GBCPixel");
                    var collidersStor = new GameObject("Colliders");
                    collidersStor.transform.parent = floor.transform;
                    collidersStor.transform.localPosition = new Vector3(-0.3325f, -0.0531f, -1.8481f);
                    for (int i = 0; i < 4; i++)
                    {
                        var colliderG = new GameObject("Collider " + i);
                        var collider = colliderG.AddComponent<BoxCollider2D>();
                        Vector3 localPos=Vector3.zero;
                        Vector2 collSize=Vector2.zero;
                        switch (i)
                        {
                            case 0:
                            {
                                localPos = new Vector3(-1.164f, 0.878f ,1.8481f);
                                collSize = new Vector2( 0.5f,4);
                                break;
                            }
                            case 1:
                            {
                                localPos = new Vector3(-1.164f, 1.146f, 1.8481f);
                                collSize = new Vector2(4,0.5f );
                                break;
                            }
                            case 2:
                            {
                                localPos = new Vector3(2.0315f, 0.878f ,1.8481f);
                                collSize = new Vector2(0.5f,4);
                                break;
                            }
                            case 3:
                            {
                                localPos = new Vector3(-1.3315f, -1.346f ,1.8481f);
                                collSize = new Vector2(4,0.5f);
                                break;
                            }
                           
                            default:
                                break;
                        }

                        collider.size = collSize;
                        colliderG.transform.parent = collidersStor.transform;
                        colliderG.transform.localPosition = localPos;

                    }
                    
                    
                    
                    floor.AddComponent<SpriteRenderer>().sprite = TowerFloorSprite;
                }
            };
        }

        private void Awake()
        {
            ModInstance = this;
            TowerSprite = TextureHelper.GetImageAsTexture("Artwork\\Tower.png").ConvertTexture();
            TowerFloorSprite =Resources.Load<Sprite>("art\\gbc\\temples\\wizard\\wizard_temple_floor");
            Logger.LogMessage($"Mod {nameof(Act2Tower)} loaded!");
            SceneManager.sceneLoaded+= delegate(Scene argScene, LoadSceneMode mode)
            {
                if (argScene.name == "GBC_WorldMap")
                {
                    OnGBCMapLoaded?.Invoke(argScene, GameObject.Find("Map").transform.GetChild(1).gameObject);
                }
            };
            OnGBCMapLoaded += GBCMapLoaded_Handling;
            
            




        }
    }
}