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

        public static GameObject FreeMovePlayerPrefab =>
            Resources.Load<GameObject>("prefabs\\gbcinterior\\FreeMovePlayer");

        public static GameObject TryCreateGBCEntranceScene(string name = "SCENE")
        {
            var go = new GameObject(name);
            GameObject.Instantiate(GBCCamerasPrefab).transform.parent = go.transform;
            GameObject.Instantiate(FreeMovePlayerPrefab).transform.parent = go.transform;
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
            foreach (var gameObject in GameObject.FindObjectsOfType<GameObject>().ToList().FindAll(g => g.activeInHierarchy))
            {
                if(gameObject.name!="CursorClone"&& gameObject.name!="CustomCoroutineRunner"&&gameObject.name!=GBCSingletonsPrefab.name&& gameObject.scene.name==("GBC_WorldMap") && gameObject.hideFlags==HideFlags.None) 
                    gameObject.SetActive(false);
            }

            scene.SetActive(true);
        }

        public static void AddNavigationZoneEntrance(GameObject navigationZoneObject,
            GameObject EntranceScene, LookDirection dir)
        {
            var zone = navigationZoneObject.GetComponent<NavigationZone2D>();
            if (zone.navigationEvents == null) zone.navigationEvents = new List<NavigationZone2D.NavigationEvent>();
            zone.navigationEvents.Add(new NavigationZone2D.NavigationEvent()
                { direction = dir, action = delegate { SwitchToEntranceScene(EntranceScene); } });
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