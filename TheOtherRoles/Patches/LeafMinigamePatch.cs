using HarmonyLib;
using UnityEngine;
using TheOtherRoles.Players;
using System;
using BepInEx.IL2CPP.Utils.Collections;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(LeafMinigame), nameof(LeafMinigame.Begin))]
    class LeafMinigameBeginPatch
    {
        public static Vector3? savePos = null;
        public static void Prefix(LeafMinigame __instance)
        {
            if (MapOptions.skeldPreventPlayerFromMovingDuringCleanO2FilterTask)
			{
                CachedPlayer.LocalPlayer.PlayerPhysics.body.velocity = Vector2.zero;
                savePos = CachedPlayer.LocalPlayer.transform.position;
            }
        }

        public static void reset()
		{
            savePos = null;
        }
    }

    [HarmonyPatch(typeof(Minigame), nameof(Minigame.Close), new Type[] {})]
    class MinigameClosePatch
    {
        public static void Prefix(Minigame __instance)
        {
            if (MapOptions.skeldPreventPlayerFromMovingDuringCleanO2FilterTask)
			{
                if (__instance.TaskType == TaskTypes.CleanO2Filter)
                    DestroyableSingleton<HudManager>.Instance.StartCoroutine(WaitClose().WrapToIl2Cpp());
            }
        }

        static System.Collections.IEnumerator WaitClose()
		{
			while (LeafMinigame.Instance != null) 
			{
                if (MeetingHud.Instance != null)
                    LeafMinigameBeginPatch.savePos = CachedPlayer.LocalPlayer.transform.position;
                yield return null;
            }
			LeafMinigameBeginPatch.savePos = null;
        }
    }
}