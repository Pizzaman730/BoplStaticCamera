using BepInEx;
using BepInEx.Logging;
using BoplFixedMath;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace StaticCamera
{

    [BepInPlugin("com.PizzaMan730.StaticCamera", "StaticCamera", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {

        public static ManualLogSource Log;

        private void Awake()
        {
            Logger.LogInfo("StaticCamera has loaded!");

            Harmony harmony = new Harmony("com.PizzaMan730.StaticCamera");


            harmony.PatchAll(typeof(Patch));
        }
    }

    [HarmonyPatch]
    public class Patch
    {
        
        [HarmonyPatch(typeof(PlayerAverageCamera), "UpdateCamera")]
        [HarmonyPrefix]
        public static bool UpdateCamera(ref bool ___useMinMaxForCameraX, ref float ___maxDeltaTime, ref float ___weight, 
                                        ref PlayerAverageCamera __instance, ref Camera ___camera, ref float ___outsideLevelX, 
                                        ref float ___MinHeightAboveFloor, ref bool ___UpdateX, ref bool ___UpdateY, 
                                        ref float ___extraZoomRoom, ref float ___MAX_ZOOM, ref float ___MIN_ZOOM, 
                                        ref float ___zoomWeight)
        {
            List<Player> list = PlayerHandler.Get().PlayerList();
            if (GameLobby.isOnlineGame)
            {
                //This line is neccesary for it to work for some reason? Not sure why
                Plugin.Log.LogInfo("Online game");
                for (int playerNum = 0; playerNum < list.Count; playerNum++)
                {
                    if (list[playerNum].IsLocalPlayer)
                    {
                        list.RemoveAt(playerNum);
                    }
                }
            }
            else
            {
                //This line is neccesary for it to work for some reason? Not sure why
                Plugin.Log.LogInfo("Offline Game");
                for (int playerNum = 0; playerNum < list.Count; playerNum++)
                {
                    if (!list[playerNum].IsLocalPlayer)
                    {
                        list.RemoveAt(playerNum);
                    }
                }
            }
	        Vector2 vector = default(Vector2);
	        float d = (float)list.Count;
	        float num = 0f;
	        float num2 = 0f;
	        for (int i = 0; i < list.Count; i++)
	        {
	        	if (list[i].IsAlive)
	        	{
	        		Vector2 vector2 = (Vector2)list[i].Position;
	        		vector += vector2;
	        		num = Mathf.Max(num, vector2.x);
	        		num2 = Mathf.Min(num2, vector2.x);
	        	}
	        }
	        Vec2[] playerSpawns_readonly = GameSessionHandler.playerSpawns_readonly;
	        vector /= d;
	        if (___useMinMaxForCameraX)
	        {
	        	vector = new Vector2((num2 + num) * 0.5f, vector.y);
	        }
	        float num3 = Mathf.Min(___maxDeltaTime, Time.unscaledDeltaTime);
	        Vector2 vector3 = (1f - ___weight * num3) * __instance.transform.localPosition + ___weight * num3 * (Vector3)vector;
	        float num4 = ___camera.orthographicSize * ___camera.aspect + ___outsideLevelX;
	        vector3.x = Mathf.Max((float)SceneBounds.Camera_XMin + num4, vector3.x);
	        vector3.x = Mathf.Min((float)SceneBounds.Camera_XMax - num4, vector3.x);
	        vector3.x = __instance.RoundToNearestPixel(vector3.x);
	        vector3.y = Mathf.Max((float)SceneBounds.WaterHeight + ___MinHeightAboveFloor, vector3.y);
	        vector3.y = Mathf.Min((float)SceneBounds.Camera_YMax, vector3.y);
	        vector3.y = __instance.RoundToNearestPixel(vector3.y);
	        if (!___UpdateX)
	        {
	        	vector3.x = __instance.transform.position.x;
	        }
	        if (!___UpdateY)
	        {
	        	vector3.y = __instance.transform.position.y;
	        }
	        Vector3 position = new Vector3(vector3.x, vector3.y, __instance.transform.position.z);
	        __instance.transform.position = position;
	        float num5 = 0f;
	        float num6 = 0f;
	        for (int j = 0; j < list.Count; j++)
	        {
	        	Vector2 vector4 = (Vector2)list[j].Position;
	        	float b = Mathf.Abs(vector4.x - vector.x);
	        	num5 = Mathf.Max(num5, b);
	        	b = Mathf.Abs(vector4.y - vector.y);
	        	num6 = Mathf.Max(num6, b);
	        }
	        float num7 = (float)(Screen.width / Screen.height);
	        num5 *= num7;
	        float num8 = Mathf.Max(num5, num6);
	        num8 += ___extraZoomRoom;
	        if (num8 > ___MAX_ZOOM)
	        {
	        	num8 = ___MAX_ZOOM;
	        }
	        if (num8 < ___MIN_ZOOM)
	        {
	        	num8 = ___MIN_ZOOM;
	        }
	        float num9 = Mathf.Clamp((1f - ___zoomWeight * num3) * ___camera.orthographicSize + ___zoomWeight * num3 * num8, ___MIN_ZOOM, ___MAX_ZOOM);
	        if (___camera.orthographicSize != num9)
	        {
	        	___camera.orthographicSize = num9;
	        }
            return false;
        }
    }
}