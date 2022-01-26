using HarmonyLib;
using static TheOtherRoles.TheOtherRoles;
using System.Collections;
using System.Collections.Generic;
using System;
using Hazel;
using TheOtherRoles.Patches;

namespace TheOtherRoles {
    [HarmonyPatch]
    public static class TasksHandler {

        public static Tuple<int, int> taskInfo(GameData.PlayerInfo playerInfo, bool madmateCount = false, bool isResult = false) {
            int TotalTasks = 0;
            int CompletedTasks = 0;
            if (!playerInfo.Disconnected && playerInfo.Tasks != null &&
                playerInfo.Object &&
                (PlayerControl.GameOptions.GhostsDoTasks || !playerInfo.IsDead) &&
                playerInfo.Role && playerInfo.Role.TasksCountTowardProgress &&
                (playerInfo.Object != Madmate.madmate || (madmateCount && PlayerControl.LocalPlayer == Madmate.madmate)) &&
                !playerInfo.Object.hasFakeTasks()
                ) {
                bool isOldTaskMasterEx = TaskMaster.taskMaster && TaskMaster.oldTaskMasterPlayerId == playerInfo.PlayerId;
                bool isTaskMasterEx = TaskMaster.taskMaster && TaskMaster.taskMaster == playerInfo.Object && TaskMaster.isTaskComplete;
                if (isOldTaskMasterEx || (!isResult && isTaskMasterEx)) {
                    TotalTasks = CompletedTasks = PlayerControl.GameOptions.NumCommonTasks + PlayerControl.GameOptions.NumLongTasks + PlayerControl.GameOptions.NumShortTasks;
                } else {
                    for (int j = 0; j < playerInfo.Tasks.Count; j++) {
                        TotalTasks++;
                        if (playerInfo.Tasks[j].Complete) {
                            CompletedTasks++;
                        }
                    }
                }
            }
            return Tuple.Create(CompletedTasks, TotalTasks);
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.RecomputeTaskCounts))]
        private static class GameDataRecomputeTaskCountsPatch {
            private static bool Prefix(GameData __instance) {
                __instance.TotalTasks = 0;
                __instance.CompletedTasks = 0;
                for (int i = 0; i < __instance.AllPlayers.Count; i++) {
                    GameData.PlayerInfo playerInfo = __instance.AllPlayers[i];
                    if (playerInfo.Object
                    && playerInfo.Object.hasAliveKillingLover() // Tasks do not count if a Crewmate has an alive killing Lover
                    || playerInfo.PlayerId == Lawyer.lawyer?.PlayerId // Tasks of the Lawyer do not count
                    || (playerInfo.PlayerId == Pursuer.pursuer?.PlayerId && Pursuer.pursuer.Data.IsDead) // Tasks of the Pursuer only count, if he's alive
                    )
                        continue;
                    var (playerCompleted, playerTotal) = taskInfo(playerInfo);
                    __instance.TotalTasks += playerTotal;
                    __instance.CompletedTasks += playerCompleted;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(GameData), nameof(GameData.CompleteTask))]
        private static class GameDataCompleteTaskPatch {
            private static void Postfix(GameData __instance, [HarmonyArgument(0)] PlayerControl pc, [HarmonyArgument(1)] uint taskId) {

                if (AmongUsClient.Instance.AmHost && !pc.Data.IsDead && TaskMaster.isTaskMaster(pc.PlayerId)) {
                    byte clearTasks = 0;
                    for (int i = 0; i < pc.Data.Tasks.Count; ++i) {
                        if (pc.Data.Tasks[i].Complete)
                            ++clearTasks;
                    }
                    bool allTasksCompleted = clearTasks == pc.Data.Tasks.Count;
                    Action action = () => {
                        if (TaskMaster.isTaskComplete) {
                            byte clearTasks = 0;
                            for (int i = 0; i < pc.Data.Tasks.Count; ++i) {
                                if (pc.Data.Tasks[i].Complete)
                                    ++clearTasks;
                            }
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TaskMasterUpdateExTasks, Hazel.SendOption.Reliable, -1);
                            writer.Write(clearTasks);
                            writer.Write((byte)pc.Data.Tasks.Count);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.taskMasterUpdateExTasks(clearTasks, (byte)pc.Data.Tasks.Count);
                        }
                    };

                    if (allTasksCompleted) {
                        if (!TaskMaster.isTaskComplete) {
                            byte[] taskTypeIds = TaskMasterTaskHelper.GetTaskMasterTasks(pc);
                            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId, (byte)CustomRPC.TaskMasterSetExTasks, Hazel.SendOption.Reliable, -1);
                            writer.Write(pc.PlayerId);
                            writer.Write(byte.MaxValue);
                            writer.Write(taskTypeIds);
                            AmongUsClient.Instance.FinishRpcImmediately(writer);
                            RPCProcedure.taskMasterSetExTasks(pc.PlayerId, byte.MaxValue, taskTypeIds);
                            action();
                        } else {
                            action();
                            ShipStatus.Instance.enabled = false;
                            ShipStatus.RpcEndGame((GameOverReason)CustomGameOverReason.TaskMasterWin, false);
                        }
                    } else {
                        action();
                    }
                }
            }
        }
    }
}
