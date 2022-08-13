using HarmonyLib;
using System;
using static TheOtherRoles.TheOtherRoles;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using TheOtherRoles.Players;
using TheOtherRoles.Utilities;

namespace TheOtherRoles.Patches {
    [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.OnDestroy))]
    class IntroCutsceneOnDestroyPatch
    {
        public static PoolablePlayer playerPrefab;
        public static void Prefix(IntroCutscene __instance) {
            // Generate and initialize player icons
            int playerCounter = 0;
            if (CachedPlayer.LocalPlayer != null && FastDestroyableSingleton<HudManager>.Instance != null) {
                Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z);
                foreach (PlayerControl p in CachedPlayer.AllPlayers) {
                    GameData.PlayerInfo data = p.Data;
                    PoolablePlayer player = UnityEngine.Object.Instantiate<PoolablePlayer>(__instance.PlayerPrefab, FastDestroyableSingleton<HudManager>.Instance.transform);
                    playerPrefab = __instance.PlayerPrefab;
                    p.SetPlayerMaterialColors(player.cosmetics.currentBodySprite.BodySprite);
                    player.SetSkin(data.DefaultOutfit.SkinId, data.DefaultOutfit.ColorId);
                    player.cosmetics.SetHat(data.DefaultOutfit.HatId, data.DefaultOutfit.ColorId);
                   // PlayerControl.SetPetImage(data.DefaultOutfit.PetId, data.DefaultOutfit.ColorId, player.PetSlot);
                    player.cosmetics.nameText.text = data.PlayerName;
                    player.SetFlipX(true);
                    MapOptions.playerIcons[p.PlayerId] = player;

                    if (CachedPlayer.LocalPlayer.PlayerControl == Arsonist.arsonist && p != Arsonist.arsonist) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, -0.25f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(true);
                        player.gameObject.SetActive(true);
                    } else if (CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.bountyHunter) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(false);
                    } else if (CachedPlayer.LocalPlayer.PlayerControl == Kataomoi.kataomoi && p == Kataomoi.target) {
                        player.transform.localPosition = bottomLeft + new Vector3(-0.25f, 0f, 0);
                        player.transform.localScale = Vector3.one * 0.4f;
                        player.gameObject.SetActive(true);
                    } else if (TaskRacer.isTaskRacer(CachedPlayer.LocalPlayer.PlayerControl)) { // Task Vs Mode
                        var position = bottomLeft + new Vector3(-0.55f, -0.45f, 0) + Vector3.right * playerCounter++ * 0.35f;
                        TaskRacer.rankUIPositions.Add(position);
                        player.transform.localPosition = position;
                        player.transform.localScale = Vector3.one * 0.2f;
                        player.setSemiTransparent(false);
                        player.gameObject.SetActive(true);

                        int index = playerCounter - 1;
                        var taskFinishedMark = new GameObject("TaskFinishedMark_" + (index + 1));

                        var rend = taskFinishedMark.AddComponent<SpriteRenderer>();
                        rend.sprite = TaskRacer.getTaskFinishedSprites();
                        rend.color = new Color(1, 1, 1, 1);
                        taskFinishedMark.transform.parent = FastDestroyableSingleton<HudManager>.Instance.transform;
                        taskFinishedMark.transform.localPosition = position;
                        taskFinishedMark.transform.localScale = Vector3.one * 0.8f;
                        taskFinishedMark.SetActive(false);
                        TaskRacer.taskFinishedMarkTable.Add(p.PlayerId, taskFinishedMark);

                        if (playerCounter >= 1 && playerCounter <= 3) {
                            TaskRacer.rankMarkObjects[index] = new GameObject("RankMarkObject_" + (index + 1));
                            rend = TaskRacer.rankMarkObjects[index].AddComponent<SpriteRenderer>();
                            rend.sprite = TaskRacer.getRankGameSprites(playerCounter);
                            rend.color = new Color(1, 1, 1, 1);
                            TaskRacer.rankMarkObjects[index].transform.parent = FastDestroyableSingleton<HudManager>.Instance.transform;
                            TaskRacer.rankMarkObjects[index].transform.localPosition = position + new Vector3(0f, 0.39f, -8f);
                            TaskRacer.rankMarkObjects[index].transform.localScale = Vector3.one * 0.8f;
                        }
                    } else {
                        player.gameObject.SetActive(false);
                    }
                }
            }

            // Force Bounty Hunter to load a new Bounty when the Intro is over
            if (BountyHunter.bounty != null && CachedPlayer.LocalPlayer.PlayerControl == BountyHunter.bountyHunter) {
                BountyHunter.bountyUpdateTimer = 0f;
                if (FastDestroyableSingleton<HudManager>.Instance != null) {
                    Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    BountyHunter.cooldownText = UnityEngine.Object.Instantiate<TMPro.TextMeshPro>(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    BountyHunter.cooldownText.alignment = TMPro.TextAlignmentOptions.Center;
                    BountyHunter.cooldownText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    BountyHunter.cooldownText.gameObject.SetActive(true);
                }
            }

            // Force Reload of SoundEffectHolder
            SoundEffectsManager.Load();

            // First kill
            if (AmongUsClient.Instance.AmHost && MapOptions.shieldFirstKill && MapOptions.firstKillName != "") {
                PlayerControl target = PlayerControl.AllPlayerControls.ToArray().ToList().FirstOrDefault(x => x.Data.PlayerName.Equals(MapOptions.firstKillName));
                if (target != null) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(CachedPlayer.LocalPlayer.PlayerControl.NetId, (byte)CustomRPC.SetFirstKill, Hazel.SendOption.Reliable, -1);
                    writer.Write(target.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.setFirstKill(target.PlayerId);
                }
            }

            MapOptions.firstKillName = "";
            if (Kataomoi.kataomoi != null && CachedPlayer.LocalPlayer.PlayerControl == Kataomoi.kataomoi) {
                if (FastDestroyableSingleton<HudManager>.Instance != null) {
                    Vector3 bottomLeft = new Vector3(-FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.x, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.y, FastDestroyableSingleton<HudManager>.Instance.UseButton.transform.localPosition.z) + new Vector3(-0.25f, 1f, 0);
                    Kataomoi.stareText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                    Kataomoi.stareText.alignment = TMPro.TextAlignmentOptions.Center;
                    Kataomoi.stareText.transform.localPosition = bottomLeft + new Vector3(0f, -1f, -1f);
                    Kataomoi.stareText.gameObject.SetActive(true);

                    Kataomoi.gaugeRenderer[0] = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.graphic, FastDestroyableSingleton<HudManager>.Instance.transform);
                    var killButton = Kataomoi.gaugeRenderer[0].GetComponent<KillButton>();
                    killButton.SetCoolDown(0.00000001f, 0.00000001f);
                    killButton.SetFillUp(0.00000001f, 0.00000001f);
                    killButton.SetDisabled();
                    Helpers.hideGameObjects(Kataomoi.gaugeRenderer[0].gameObject);
                    var components = killButton.GetComponents<Component>();
                    foreach (var c in components) {
                        if ((c as KillButton) == null && (c as SpriteRenderer) == null)
                            GameObject.Destroy(c);
                    }

                    Kataomoi.gaugeRenderer[0].sprite = Kataomoi.getLoveGaugeSprite(0);
                    Kataomoi.gaugeRenderer[0].color = new Color32(175, 175, 176, 255);
                    Kataomoi.gaugeRenderer[0].size = new Vector2(300f, 64f);
                    Kataomoi.gaugeRenderer[0].gameObject.SetActive(true);
                    Kataomoi.gaugeRenderer[0].transform.localPosition = new Vector3(-3.354069f, -2.429999f, -8f);
                    Kataomoi.gaugeRenderer[0].transform.localScale = Vector3.one;

                    Kataomoi.gaugeRenderer[1] = UnityEngine.Object.Instantiate(Kataomoi.gaugeRenderer[0], FastDestroyableSingleton<HudManager>.Instance.transform);
                    Kataomoi.gaugeRenderer[1].sprite = Kataomoi.getLoveGaugeSprite(1);
                    Kataomoi.gaugeRenderer[1].size = new Vector2(261f, 7f);
                    Kataomoi.gaugeRenderer[1].color = Kataomoi.color;
                    Kataomoi.gaugeRenderer[1].transform.localPosition = new Vector3(-3.482069f, -2.626999f, -8.1f);
                    Kataomoi.gaugeRenderer[1].transform.localScale = Vector3.one;

                    Kataomoi.gaugeRenderer[2] = UnityEngine.Object.Instantiate(Kataomoi.gaugeRenderer[0], FastDestroyableSingleton<HudManager>.Instance.transform);
                    Kataomoi.gaugeRenderer[2].sprite = Kataomoi.getLoveGaugeSprite(2);
                    Kataomoi.gaugeRenderer[2].color = Kataomoi.gaugeRenderer[0].color;
                    Kataomoi.gaugeRenderer[2].size = new Vector2(300f, 64f);
                    Kataomoi.gaugeRenderer[2].transform.localPosition = new Vector3(-3.354069f, -2.429999f, -8.2f);
                    Kataomoi.gaugeRenderer[2].transform.localScale = Vector3.one;

                    Kataomoi.gaugeTimer = 1.0f;
                }
            }

            // Task Vs Mode
            if (TaskRacer.isValid()) {
                TaskRacer.startText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                TaskRacer.startText.rectTransform.sizeDelta = new Vector2(600, TaskRacer.startText.rectTransform.sizeDelta.y);
                TaskRacer.startText.name = "TaskVsMode_Start";

                TaskRacer.timerText = UnityEngine.Object.Instantiate(FastDestroyableSingleton<HudManager>.Instance.KillButton.cooldownTimerText, FastDestroyableSingleton<HudManager>.Instance.transform);
                TaskRacer.timerText.rectTransform.sizeDelta = new Vector2(600, TaskRacer.startText.rectTransform.sizeDelta.y * 2);
                TaskRacer.timerText.transform.localPosition = new Vector3(0.89f, 2.76f, TaskRacer.timerText.transform.localPosition.z);
                TaskRacer.timerText.transform.localScale *= 0.4f;
                TaskRacer.timerText.name = "TaskVsMode_Timer";
                TaskRacer.timerText.gameObject.SetActive(false);

                if (PlayerControl.GameOptions.MapId != (byte)MapId.Airship) {
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        CachedPlayer.LocalPlayer.PlayerControl.NetId,
                        (byte)CustomRPC.TaskVsMode_Ready,
                        Hazel.SendOption.Reliable,
                        -1);
                    writer.Write(CachedPlayer.LocalPlayer.PlayerControl.PlayerId);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                    RPCProcedure.taskVsModeReady(CachedPlayer.LocalPlayer.PlayerControl.PlayerId);
                }

                // Task Vs Mode
                IntroPatch.IntroPatchHelper.CheckTaskRacer();
            }

            if (PlayerControl.GameOptions.MapId == (byte)MapId.Airship && CustomOptionHolder.airshipWallCheckOnTasks.getBool()) {
                var objList = GameObject.FindObjectsOfType<Console>().ToList();
                objList.Find(x => x.name == "task_garbage1").checkWalls = true;
                objList.Find(x => x.name == "task_garbage2").checkWalls = true;
                objList.Find(x => x.name == "task_garbage3").checkWalls = true;
                objList.Find(x => x.name == "task_garbage4").checkWalls = true;
                objList.Find(x => x.name == "task_garbage5").checkWalls = true;
                objList.Find(x => x.name == "task_shower").checkWalls = true;
                objList.Find(x => x.name == "task_developphotos").checkWalls = true;
                objList.Find(x => x.name == "DivertRecieve" && x.Room == SystemTypes.Armory).checkWalls = true;
                objList.Find(x => x.name == "DivertRecieve" && x.Room == SystemTypes.MainHall).checkWalls = true;
            }
        }
    }

    [HarmonyPatch]
    class IntroPatch {
        public static void setupIntroTeamIcons(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            // Intro solo teams
            if (CachedPlayer.LocalPlayer.PlayerControl == Jester.jester || CachedPlayer.LocalPlayer.PlayerControl == Jackal.jackal || CachedPlayer.LocalPlayer.PlayerControl == Arsonist.arsonist || CachedPlayer.LocalPlayer.PlayerControl == Vulture.vulture || CachedPlayer.LocalPlayer.PlayerControl == Lawyer.lawyer || CachedPlayer.LocalPlayer.PlayerControl == Kataomoi.kataomoi) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
                yourTeam = soloTeam;
            }

            /*
             * Madmate is a solo team as well
             * This code is redundant, but this part should be decoupled from the original code
             * to merge future changes
             */
            if (CachedPlayer.LocalPlayer.PlayerControl == Madmate.madmate) {
                var soloTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                soloTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
                yourTeam = soloTeam;
            }

            // Add the Spy to the Impostor team (for the Impostors)
            if (Spy.spy != null && CachedPlayer.LocalPlayer.Data.Role.IsImpostor) {
                List<PlayerControl> players = PlayerControl.AllPlayerControls.ToArray().ToList().OrderBy(x => Guid.NewGuid()).ToList();
                var fakeImpostorTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>(); // The local player always has to be the first one in the list (to be displayed in the center)
                fakeImpostorTeam.Add(CachedPlayer.LocalPlayer.PlayerControl);
                foreach (PlayerControl p in players) {
                    if (CachedPlayer.LocalPlayer.PlayerControl != p && (p == Spy.spy || p.Data.Role.IsImpostor))
                        fakeImpostorTeam.Add(p);
                }
                yourTeam = fakeImpostorTeam;
            }
        }

        public static void setupIntroTeam(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
            List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl);
            RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
            if (roleInfo == null) return;
            if (roleInfo.roleId == RoleId.TaskRacer) {
                __instance.BackgroundBar.material.color = roleInfo.color;
                __instance.TeamTitle.text = "Task Vs Mode";
                __instance.TeamTitle.color = roleInfo.color;
                __instance.ImpostorText.gameObject.SetActive(false);
            } else if (roleInfo.isNeutral) {
                var neutralColor = new Color32(76, 84, 78, 255);
                __instance.BackgroundBar.material.color = neutralColor;
                __instance.TeamTitle.text = "Neutral";
                __instance.TeamTitle.color = neutralColor;
            } else if (roleInfo.roleId == RoleId.Madmate) {
                __instance.BackgroundBar.material.color = roleInfo.color;
                __instance.TeamTitle.text = roleInfo.name;
                __instance.TeamTitle.color = roleInfo.color;
                __instance.ImpostorText.gameObject.SetActive(true);
                __instance.ImpostorText.text = "Team Impostor";
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.ShowRole))]
        class ShowRolePatch
        {
            public static void Postfix(IntroCutscene __instance) {
                if (!CustomOptionHolder.activateRoles.getBool()) return; // Don't override the intro of the vanilla roles
                if (IntroCutsceneShowRoleUpdatePatch.introCutscene != null)
                    IntroCutsceneShowRoleUpdatePatch.introCutscene = null;
                else
                    IntroCutsceneShowRoleUpdatePatch.introCutscene = __instance;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.CreatePlayer))]
        class CreatePlayerPatch {
            public static void Postfix(IntroCutscene __instance, bool impostorPositioning, ref PoolablePlayer __result) {
                if (impostorPositioning) __result.SetNameColor(Palette.ImpostorRed);
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class IntroCutsceneShowRoleUpdatePatch
        {
            public static IntroCutscene introCutscene;

            public static void Postfix(HudManager __instance) {
                UpdateRoleText();
            }

            static void UpdateRoleText() {
                if (introCutscene == null) return;

                List<RoleInfo> infos = RoleInfo.getRoleInfoForPlayer(CachedPlayer.LocalPlayer.PlayerControl);
                RoleInfo roleInfo = infos.Where(info => !info.isModifier).FirstOrDefault();
                RoleInfo modifierInfo = infos.Where(info => info.isModifier).FirstOrDefault();
                introCutscene.RoleBlurbText.text = "";
                if (roleInfo.roleId == RoleId.TaskMaster && TaskMaster.becomeATaskMasterWhenCompleteAllTasks)
                    roleInfo = RoleInfo.crewmate;

                if (roleInfo != null) {
                    introCutscene.RoleText.text = roleInfo.name;
                    introCutscene.RoleText.color = roleInfo.color;
                    introCutscene.RoleBlurbText.text = roleInfo.introDescription;
                    introCutscene.RoleBlurbText.color = roleInfo.color;
                }
                if (modifierInfo != null) {
                    if (modifierInfo.roleId != RoleId.Lover)
                        introCutscene.RoleBlurbText.text += Helpers.cs(modifierInfo.color, $"\n{modifierInfo.introDescription}");
                    else {
                        PlayerControl otherLover = CachedPlayer.LocalPlayer.PlayerControl == Lovers.lover1 ? Lovers.lover2 : Lovers.lover1;
                        introCutscene.RoleBlurbText.text += Helpers.cs(Lovers.color, $"\n♥ You are in love with {otherLover?.Data?.PlayerName ?? ""} ♥");
                    }
                }
                if (infos.Any(info => info.roleId == RoleId.Kataomoi)) {
                    introCutscene.RoleBlurbText.text += Helpers.cs(Lovers.color, $"\n♥ Your unrequited love target is {Kataomoi.target?.Data?.PlayerName ?? ""} ♥");
                }
                if (Deputy.knowsSheriff && Deputy.deputy != null && Sheriff.sheriff != null) {
                    if (infos.Any(info => info.roleId == RoleId.Sheriff))
                        introCutscene.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Deputy is {Deputy.deputy?.Data?.PlayerName ?? ""}");
                    else if (infos.Any(info => info.roleId == RoleId.Deputy))
                        introCutscene.RoleBlurbText.text += Helpers.cs(Sheriff.color, $"\nYour Sheriff is {Sheriff.sheriff?.Data?.PlayerName ?? ""}");
                }
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        class BeginCrewmatePatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                if (TaskRacer.isValid())
                    teamToDisplay = PlayerControl.AllPlayerControls;
                else
                    setupIntroTeamIcons(__instance, ref teamToDisplay);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> teamToDisplay) {
                if (TaskRacer.isValid())
                    teamToDisplay = PlayerControl.AllPlayerControls;

                 setupIntroTeam(__instance, ref teamToDisplay);

                /*
                 * Workaround
                 * reset and re-assign tasks
                 * This should be done before a game starting and after tasks assinged
                 * If you have an idea, please send me a pull request!
                 */
                if (Madmate.madmate != null && CachedPlayer.LocalPlayer.PlayerControl == Madmate.madmate
                    && Madmate.noticeImpostors) {
                    MadmateTaskHelper.SetMadmateTasks();
                }
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        class BeginImpostorPatch {
            public static void Prefix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                if (TaskRacer.isValid())
                    yourTeam = PlayerControl.AllPlayerControls;
                else
                    setupIntroTeamIcons(__instance, ref yourTeam);
            }

            public static void Postfix(IntroCutscene __instance, ref  Il2CppSystem.Collections.Generic.List<PlayerControl> yourTeam) {
                if (TaskRacer.isValid())
                    yourTeam = PlayerControl.AllPlayerControls;

                setupIntroTeam(__instance, ref yourTeam);
            }
        }

        public static class IntroPatchHelper
        {
            public static void CheckTaskRacer() {
                // Task Vs Mode
                if (!TaskRacer.isValid() || !CustomOptionHolder.taskVsModeEnabledMakeItTheSameTaskAsTheHost.getBool())
                    return;

                if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost) {

                    // Init host's tasks.
                    List<byte> taskTypeIdList = new List<byte>();
                    for (int i = 0; i < CachedPlayer.LocalPlayer.PlayerControl.Data.Tasks.Count; ++i)
                        taskTypeIdList.Add(CachedPlayer.LocalPlayer.PlayerControl.Data.Tasks[i].TypeId);

                    var taskIdDataTable = new Dictionary<uint, byte[]>();
                    var playerData = CachedPlayer.LocalPlayer.PlayerControl.Data;
                    playerData.Object.clearAllTasks();
                    playerData.Tasks = new Il2CppSystem.Collections.Generic.List<GameData.TaskInfo>(taskTypeIdList.Count);
                    for (int j = 0; j < taskTypeIdList.Count; j++) {
                        playerData.Tasks.Add(new GameData.TaskInfo(taskTypeIdList[j], (uint)j));
                        playerData.Tasks[j].Id = (uint)j;
                    }
                    for (int j = 0; j < playerData.Tasks.Count; j++) {
                        GameData.TaskInfo taskInfo = playerData.Tasks[j];
                        NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(ShipStatus.Instance.GetTaskById(taskInfo.TypeId), playerData.Object.transform);
                        normalPlayerTask.Id = taskInfo.Id;
                        normalPlayerTask.Owner = playerData.Object;
                        normalPlayerTask.Initialize();
                        if (normalPlayerTask.Data != null && normalPlayerTask.Data.Length > 0)
                            taskIdDataTable.Add(normalPlayerTask.Id, normalPlayerTask.Data);
                        playerData.Object.myTasks.Add(normalPlayerTask);
                    }
                    foreach (var pair in taskIdDataTable) {
                        MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                            CachedPlayer.LocalPlayer.PlayerControl.NetId,
                            (byte)CustomRPC.TaskVsMode_MakeItTheSameTaskAsTheHostDetail,
                            Hazel.SendOption.Reliable,
                            -1);
                        writer.Write(pair.Key);
                        writer.Write(pair.Value);
                        AmongUsClient.Instance.FinishRpcImmediately(writer);
                        TaskRacer.setHostTaskDetail(pair.Key, pair.Value);
                    }

                    MessageWriter writer2 = AmongUsClient.Instance.StartRpcImmediately(
                        CachedPlayer.LocalPlayer.PlayerControl.NetId,
                        (byte)CustomRPC.TaskVsMode_MakeItTheSameTaskAsTheHost,
                        Hazel.SendOption.Reliable,
                        -1);
                    byte[] taskTypeIds = taskTypeIdList.ToArray();
                    if (taskTypeIdList.Count > 0)
                        writer2.Write(taskTypeIds);
                    AmongUsClient.Instance.FinishRpcImmediately(writer2);
                    TaskRacer.setHostTasks(taskTypeIds);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Constants), nameof(Constants.ShouldHorseAround))]
    public static class ShouldAlwaysHorseAround {
        public static bool isHorseMode;
        public static bool Prefix(ref bool __result) {
            if (isHorseMode != MapOptions.enableHorseMode && LobbyBehaviour.Instance != null) __result = isHorseMode;
            else {
                __result = MapOptions.enableHorseMode;
                isHorseMode = MapOptions.enableHorseMode;
            }
            return false;
        }
    }
}

