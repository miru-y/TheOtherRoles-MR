using HarmonyLib;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;
using UnityEngine;


namespace TheOtherRoles.Patches {

	[HarmonyPatch(typeof(MapBehaviour))]
	class MapBehaviourPatch {

		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
		static bool Prefix(MapBehaviour __instance) {
			if (!MeetingHud.Instance) return true;  // Only run in meetings, and then set the Position of the HerePoint to the Position before the Meeting!
			if (!MapUtilities.CachedShipStatus) {
				return false;
			}
			Vector3 vector = AntiTeleport.position != null ? AntiTeleport.position : CachedPlayer.LocalPlayer.transform.position;
			vector /= MapUtilities.CachedShipStatus.MapScale;
			vector.x *= Mathf.Sign(MapUtilities.CachedShipStatus.transform.localScale.x);
			vector.z = -1f;
			__instance.HerePoint.transform.localPosition = vector;
			CachedPlayer.LocalPlayer.PlayerControl.SetPlayerMaterialColors(__instance.HerePoint);

			return false;
		}
	}

	[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.Close))]
	class MapBehaviourClosePatch
	{
		static void Postfix(MapBehaviour __instance)
		{
			if (EvilHacker.evilHacker != null && EvilHacker.evilHacker == CachedPlayer.LocalPlayer.PlayerControl)
				EvilHacker.isMobile = false;
		}
	}

	[HarmonyPatch(typeof(MapBehaviour), "get_IsOpenStopped")]
	class MapBehaviorGetIsOpenStoppedPatch
	{
		static bool Prefix(MapBehaviour __instance, ref bool __result)
		{
			if (EvilHacker.evilHacker == CachedPlayer.LocalPlayer.PlayerControl && CustomOptionHolder.evilHackerCanMoveEvenIfUsesAdmin.getBool())
			{
				__result = false;
				return false;
			}
			return true;
		}
	}
}
