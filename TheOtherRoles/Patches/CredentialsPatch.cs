using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.UI.Button;

namespace TheOtherRoles.Patches {
    [HarmonyPatch]
    public static class CredentialsPatch {
        public static string fullCredentials = 
$@"<size=130%><color=#ff351f>TheOtherRoles MR</color></size> v{TheOtherRolesPlugin.Version.ToString()}
<size=60%>Modded by <color=#FCCE03FF>Eisbison</color>, <color=#FCCE03FF>EndOfFile</color>
<color=#FCCE03FF>Thunderstorm584</color> & <color=#FCCE03FF>Mallöris</color>
Button design by <color=#FCCE03FF>Bavari</color>
このMODについて: https://git.io/AUMod</size>";

    public static string mainMenuCredentials = 
$@"Modded by <color=#FCCE03FF>Eisbison</color>, <color=#FCCE03FF>Thunderstorm584</color>, <color=#FCCE03FF>EndOfFile</color> & <color=#FCCE03FF>Mallöris</color>
Design by <color=#FCCE03FF>Bavari</color>
このMODについて: https://git.io/AUMod";


        public static string contributorsCredentials = "<size=80%>GitHub Contributors: Alex2911, amsyarasyiq, gendelo3</size>";

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        private static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo == null) return;

                var credentials = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(__instance.text);
                credentials.transform.position = new Vector3(0, 0, 0);
                credentials.SetText($"MRエディション v{TheOtherRolesPlugin.Version.ToString()}\n<size=30f%>\n</size>{mainMenuCredentials}\n<size=30%>\n</size>{contributorsCredentials}");
                credentials.alignment = TMPro.TextAlignmentOptions.Center;
                credentials.fontSize *= 0.75f;

                credentials.transform.SetParent(amongUsLogo.transform);
            }
        }

        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingTrackerPatch
        {
            private static GameObject modStamp;
            public static GameObject customPreset;
            static void Prefix(PingTracker __instance) {
                if (modStamp == null) {
                    modStamp = new GameObject("ModStamp");
                    var rend = modStamp.AddComponent<SpriteRenderer>();
                    rend.sprite = TheOtherRolesPlugin.GetModStamp();
                    rend.color = new Color(1, 1, 1, 0.5f);
                    modStamp.transform.parent = __instance.transform.parent;
                    modStamp.transform.localScale *= 0.6f;
                }
                if (customPreset == null) {
                    var buttonBehaviour = UnityEngine.Object.Instantiate(HudManager.Instance.GameMenu.CensorChatButton);
                    buttonBehaviour.Text.text = "";
                    buttonBehaviour.Background.sprite = TheOtherRolesPlugin.GetCustomPreset();
                    buttonBehaviour.Background.color = new Color(1, 1, 1, 1);
                    customPreset = buttonBehaviour.gameObject;
                    customPreset.name = "CustomPreset";
                    customPreset.transform.parent = __instance.transform.parent;
                    customPreset.transform.localScale = new Vector3(0.2f, 1.2f, 1.2f) * 1.2f;
                    customPreset.SetActive(true);
                    var button = buttonBehaviour.GetComponent<PassiveButton>();
                    button.ClickSound = null;
                    button.OnMouseOver = new UnityEngine.Events.UnityEvent();
                    button.OnMouseOut = new UnityEngine.Events.UnityEvent();
                    button.OnClick = new ButtonClickedEvent();
                    button.OnClick.AddListener((Action)(() => {
                        ClientOptionsPatch.isOpenPreset = true;
                        HudManager.Instance.GameMenu.Open();
                    }));
                }
                float offset = (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) ? 0.75f : 0f;
                modStamp.transform.position = HudManager.Instance.MapButton.transform.position + Vector3.down * offset;
                if (customPreset) {
                    customPreset.transform.position = modStamp.transform.position + Vector3.down * 0.75f;
                    if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started && customPreset.gameObject.activeSelf)
                        customPreset.gameObject.SetActive(false);
                }

            }

            static void Postfix(PingTracker __instance){
                __instance.text.alignment = TMPro.TextAlignmentOptions.TopRight;
                if (AmongUsClient.Instance.GameState == InnerNet.InnerNetClient.GameStates.Started) {
                    __instance.text.text = $"<size=130%><color=#ff351f>TheOtherRoles MR</color></size> v{TheOtherRolesPlugin.Version.ToString()}\n" + __instance.text.text;
                    if (PlayerControl.LocalPlayer.Data.IsDead || (!(PlayerControl.LocalPlayer == null) && (PlayerControl.LocalPlayer == Lovers.lover1 || PlayerControl.LocalPlayer == Lovers.lover2))) {
                        __instance.transform.localPosition = new Vector3(3.45f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    } else {
                        __instance.transform.localPosition = new Vector3(4.2f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                    }
                } else {
                    __instance.text.text = $"{fullCredentials}\n{__instance.text.text}";
                    __instance.transform.localPosition = new Vector3(3.5f, __instance.transform.localPosition.y, __instance.transform.localPosition.z);
                }
            }
        }

        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        private static class LogoPatch
        {
            static void Postfix(PingTracker __instance) {
                var amongUsLogo = GameObject.Find("bannerLogo_AmongUs");
                if (amongUsLogo != null) {
                    amongUsLogo.transform.localScale *= 0.6f;
                    amongUsLogo.transform.position += Vector3.up * 0.25f;
                }

                var torLogo = new GameObject("bannerLogo_TOR");
                torLogo.transform.position = Vector3.up;
                var renderer = torLogo.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);

                // Task Vs Mode
                TaskRacer.clearAndReload();
            }
        }
    }
}
