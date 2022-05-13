using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using static TheOtherRoles.TheOtherRoles;

namespace TheOtherRoles{
    static class MapOptions {
        // Set values
        public static int maxNumberOfMeetings = 10;
        public static bool blockSkippingInEmergencyMeetings = false;
        public static bool noVoteIsSelfVote = false;
        public static bool hidePlayerNames = false;
        public static bool ghostsSeeRoles = true;
        public static bool ghostsSeeModifier = true;
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool allowParallelMedBayScans = false;
        public static bool showLighterDarker = true;
        public static bool enableHorseMode = false;
        public static bool shieldFirstKill = false;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();
        public static float AdminTimer = 0f;
        public static float VitalsTimer = 0f;
        public static float SecurityCameraTimer = 0f;
        public static TMPro.TextMeshPro AdminTimerText = null;
        public static string firstKillName;
        public static PlayerControl firstKillPlayer;
        public static TMPro.TextMeshPro VitalsTimerText = null;
        public static TMPro.TextMeshPro SecurityCameraTimerText = null;

        const float TimerUIBaseX = -3.5f;
        const float TimerUIMoveX = 2.5f;

        public static void clearAndReloadMapOptions() {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>(); ;

            AdminTimer = CustomOptionHolder.adminTimer.getFloat();
            VitalsTimer = CustomOptionHolder.vitalsTimer.getFloat();
            SecurityCameraTimer = CustomOptionHolder.securityCameraTimer.getFloat();

            UpdateTimer();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            shieldFirstKill = CustomOptionHolder.shieldFirstKill.getBool();
            firstKillPlayer = null;
        }

        public static void reloadPluginOptions() {
            ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value;
            ghostsSeeModifier = TheOtherRolesPlugin.GhostsSeeModifier.Value;
            ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value;
            ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value;
            showLighterDarker = TheOtherRolesPlugin.ShowLighterDarker.Value;
            enableHorseMode = TheOtherRolesPlugin.EnableHorseMode.Value;
            Patches.ShouldAlwaysHorseAround.isHorseMode = TheOtherRolesPlugin.EnableHorseMode.Value;
        }

        public static void MeetingEndedUpdate()
        {
            UpdateTimer();
        }

        public static int UpdateAdminTimerText(int viewIndex)
        {
            if (!CustomOptionHolder.enabledAdminTimer.getBool() || CustomOptionHolder.enabledTaskVsMode.getBool())
                return viewIndex;
            if (HudManager.Instance == null)
                return viewIndex;
            AdminTimerText = UnityEngine.Object.Instantiate(HudManager.Instance.TaskText, HudManager.Instance.transform);
            AdminTimerText.transform.localPosition = new Vector3(TimerUIBaseX + TimerUIMoveX * viewIndex, -4.0f, 0);
            if (AdminTimer > 0)
                AdminTimerText.text = $"Admin: {AdminTimer.ToString("#0.0")} sec remaining";
            else
                AdminTimerText.text = "Admin: ran out of time";
            AdminTimerText.gameObject.SetActive(true);

            return viewIndex + 1;
        }

        private static void ClearAdminTimerText()
        {
            if (AdminTimerText == null)
                return;
            UnityEngine.Object.Destroy(AdminTimerText);
            AdminTimerText = null;
        }

        public static int UpdateVitalsTimerText(int viewIndex) {
            if (!CustomOptionHolder.enabledVitalsTimer.getBool() || CustomOptionHolder.enabledTaskVsMode.getBool())
                return viewIndex;
            if (HudManager.Instance == null)
                return viewIndex;
            VitalsTimerText = UnityEngine.Object.Instantiate(HudManager.Instance.TaskText, HudManager.Instance.transform);
            VitalsTimerText.color = Color.green;
            VitalsTimerText.transform.localPosition = new Vector3(TimerUIBaseX + TimerUIMoveX * viewIndex, -4.0f, 0);
            if (VitalsTimer > 0)
                VitalsTimerText.text = $"Vitals: {VitalsTimer.ToString("#0.0")} sec remaining";
            else
                VitalsTimerText.text = "Vitals: ran out of time";
            VitalsTimerText.gameObject.SetActive(true);

            return viewIndex + 1;
        }

        private static void ClearVitalsTimerText() {
            if (VitalsTimerText == null)
                return;
            UnityEngine.Object.Destroy(VitalsTimerText);
            VitalsTimerText = null;
        }


        public static int UpdateSecurityCameraTimerText(int viewIndex) {
            if (!CustomOptionHolder.enabledSecurityCameraTimer.getBool() || CustomOptionHolder.enabledTaskVsMode.getBool())
                return viewIndex;
            if (HudManager.Instance == null)
                return viewIndex;
            SecurityCameraTimerText = UnityEngine.Object.Instantiate(HudManager.Instance.TaskText, HudManager.Instance.transform);
            SecurityCameraTimerText.color = Color.red;
            SecurityCameraTimerText.transform.localPosition = new Vector3(TimerUIBaseX + TimerUIMoveX * viewIndex, -4.0f, 0);
            if (SecurityCameraTimer > 0)
                SecurityCameraTimerText.text = $"Camera: {SecurityCameraTimer.ToString("#0.0")} sec remaining";
            else
                SecurityCameraTimerText.text = "Camera: ran out of time";
            SecurityCameraTimerText.gameObject.SetActive(true);

            return viewIndex + 1;
        }

        private static void ClearSecurityCameraTimerText() {
            if (SecurityCameraTimerText == null)
                return;
            UnityEngine.Object.Destroy(SecurityCameraTimerText);
            SecurityCameraTimerText = null;
        }


        private static void UpdateTimer() {

            int viewIndex = 0;
            ClearAdminTimerText();
            viewIndex = UpdateAdminTimerText(viewIndex);

            if (Helpers.existVitals()) {
                ClearVitalsTimerText();
                viewIndex = UpdateVitalsTimerText(viewIndex);
            }

            if (Helpers.existSecurityCamera()) {
                ClearSecurityCameraTimerText();
                viewIndex = UpdateSecurityCameraTimerText(viewIndex);
            }
        }
    }
}
