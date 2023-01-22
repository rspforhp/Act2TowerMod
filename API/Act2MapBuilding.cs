using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using DiskCardGame;
using GBC;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Act2Tower.API
{
    public static class Act2MapBuilding
    {
        public static GameObject CreateGBCGameObject(string name, [CanBeNull] GameObject MapBase,
            [CanBeNull] Vector3 pos, [CanBeNull] Sprite sprite)
        {
            var go = new GameObject(name, new[] { typeof(SpriteRenderer) });
            if (MapBase != null) go.transform.parent = MapBase.transform;
            go.GetComponent<SpriteRenderer>().sprite = sprite;
            go.layer = LayerMask.NameToLayer("GBCPixel");
            go.transform.position = pos;
            return go;
        }

        public static GameObject GBCCamerasPrefab => Resources.Load<GameObject>("prefabs\\gbcoverworld\\GBCCameras");

        public static GameObject GBCSingletonsPrefab =>
            Resources.Load<GameObject>("prefabs\\gbcoverworld\\GBCSingletons");

        private static GameObject _ReturnToMapVolumePrefab;

        public static GameObject ReturnToMapVolumePrefab
        {
            get
            {
                if (_ReturnToMapVolumePrefab == null)
                {
                    _ReturnToMapVolumePrefab = new GameObject("ReturnToMapVolume");
                    _ReturnToMapVolumePrefab.transform.position = new Vector3(0, -1.2f, 0);
                    _ReturnToMapVolumePrefab.SetActive(false);
                    _ReturnToMapVolumePrefab.layer = LayerMask.NameToLayer("GBCPixel");
                    var boxCollider2D = _ReturnToMapVolumePrefab.AddComponent<BoxCollider2D>();
                    boxCollider2D.isTrigger = true;
                    boxCollider2D.size=new Vector2(0.5f ,0.3f);
                    var marker=new GameObject("PlayerPositionMarker");
                    marker.transform.parent = _ReturnToMapVolumePrefab.transform;
                    marker.transform.localPosition = new Vector3(0 ,0.3f, 0);
                    var volume=_ReturnToMapVolumePrefab.AddComponent<SceneTransitionVolume>();
                    volume.enterPositionMarker = marker.transform;
                    volume.sceneId = "GBC_WorldMap";
                    _ReturnToMapVolumePrefab.AddComponent<SpriteRenderer>().sprite =
                        Resources.Load<Sprite>("art\\gbc\\temples\\door_light");
                    GameObject.DontDestroyOnLoad(_ReturnToMapVolumePrefab);
                }
                return _ReturnToMapVolumePrefab;
            }
        }

        public static GameObject FreeMovePlayerPrefab =>
            Resources.Load<GameObject>("prefabs\\gbcinterior\\FreeMovePlayer");

        public static GameObject TryCreateGBCEntranceScene(string name = "SCENE", bool addExitToMap = true)
        {
            var go = new GameObject(name);
            GameObject.Instantiate(GBCCamerasPrefab).transform.parent = go.transform;
            GameObject.Instantiate(FreeMovePlayerPrefab).transform.parent = go.transform;
            if (addExitToMap)
            {
                var gV=GameObject.Instantiate(ReturnToMapVolumePrefab);
                gV .transform.parent = go.transform;
                gV.SetActive(true);
            }
            go.SetActive(false);
            go.hideFlags = HideFlags.HideAndDontSave;
            return go;
        }

        public static ZoneEntrance AddNavigationZoneEntrance(GameObject navigationZoneObject,
            string nameOfSceneToSwitch, LookDirection dir)
        {
            var e = navigationZoneObject.AddComponent<ZoneEntrance>();
            var zone = navigationZoneObject.GetComponent<NavigationZone2D>();
            if (zone.navigationEvents == null) zone.navigationEvents = new List<NavigationZone2D.NavigationEvent>();
            zone.navigationEvents.Add(new NavigationZone2D.NavigationEvent()
                { direction = dir, action = delegate { e.EnterZone(); } });
            e.zoneSceneId = nameOfSceneToSwitch;
            return e;
        }

        private static void SwitchToEntranceScene(GameObject scene)
        {
            foreach (var gameObject in GameObject.FindObjectsOfType<GameObject>().ToList()
                         .FindAll(g => g.activeInHierarchy))
            {
                if (gameObject.name != "CursorClone" && gameObject.name != "CustomCoroutineRunner" &&
                    gameObject.name != GBCSingletonsPrefab.name && gameObject.scene.name == ("GBC_WorldMap") &&
                    gameObject.hideFlags == HideFlags.None)
                {
                    gameObject.SetActive(false);
                }
            }

            scene.SetActive(true);
        }

        public static event Action<GameObject> OnGBCZoneSceneLoaded;

        public static void AddNavigationZoneEntrance(GameObject navigationZoneObject,
            GameObject EntranceScene, LookDirection dir)
        {
            var zone = navigationZoneObject.GetComponent<NavigationZone2D>();
            if (zone.navigationEvents == null) zone.navigationEvents = new List<NavigationZone2D.NavigationEvent>();
            zone.navigationEvents.Add(new NavigationZone2D.NavigationEvent()
            {
                direction = dir, action = delegate
                {
                    SwitchToEntranceScene(EntranceScene);
                    OnGBCZoneSceneLoaded?.Invoke(EntranceScene);
                }
            });
        }

        public static GameObject CreateGBCNaviationZone(string name, GameObject MapBase, Vector3 pos,
            List<LookDirection> blockedDirections, int x, int y)
        {
            var go = new GameObject(name, new[] { typeof(NavigationZone2D) });
            go.transform.parent = MapBase.transform.parent.GetChild(0);
            var navZone = go.GetComponent<NavigationZone2D>();
            navZone.blockedDirections = blockedDirections;
            go.transform.position = pos;
            var navigationGrid = NavigationGrid.instance;
            if (navigationGrid.zones[x, y] != null)
                Act2Tower._Logger.LogError("Tried to add a nav zone into not an empty space");
            else if (navigationGrid.zones.GetLength(0) < x || navigationGrid.zones.GetLength(1) < y)
                Act2Tower._Logger.LogError("Tried to add a nav zone outside of bounds");
            else
                navigationGrid.zones[x, y] = navZone;
            return go;
        }
    }
}