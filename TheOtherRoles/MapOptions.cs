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
        public static bool ghostsSeeTasks = true;
        public static bool ghostsSeeVotes = true;
        public static bool showRoleSummary = true;
        public static bool allowParallelMedBayScans = false;

        // Updating values
        public static int meetingsCount = 0;
        public static List<SurvCamera> camerasToAdd = new List<SurvCamera>();
        public static List<Vent> ventsToSeal = new List<Vent>();
        public static Dictionary<byte, PoolablePlayer> playerIcons = new Dictionary<byte, PoolablePlayer>();
        public static float AdminTimer = 0f;
        public static float VitalsTimer = 0f;
        public static TMPro.TextMeshPro AdminTimerText = null;
        public static TMPro.TextMeshPro VitalsTimerText = null;

public static void clearAndReloadMapOptions() {
            meetingsCount = 0;
            camerasToAdd = new List<SurvCamera>();
            ventsToSeal = new List<Vent>();
            playerIcons = new Dictionary<byte, PoolablePlayer>(); ;

            AdminTimer = CustomOptionHolder.adminTimer.getFloat();
            VitalsTimer = CustomOptionHolder.vitalsTimer.getFloat();

            UpdateTimer();

            maxNumberOfMeetings = Mathf.RoundToInt(CustomOptionHolder.maxNumberOfMeetings.getSelection());
            blockSkippingInEmergencyMeetings = CustomOptionHolder.blockSkippingInEmergencyMeetings.getBool();
            noVoteIsSelfVote = CustomOptionHolder.noVoteIsSelfVote.getBool();
            hidePlayerNames = CustomOptionHolder.hidePlayerNames.getBool();
            allowParallelMedBayScans = CustomOptionHolder.allowParallelMedBayScans.getBool();
            ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value;
            ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value;
            ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value;
            showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value;
        }

        public static void MeetingEndedUpdate()
        {
            UpdateTimer();
        }

        public static void UpdateAdminTimerText()
        {
            if (!CustomOptionHolder.enabledAdminTimer.getBool())
                return;
            if (HudManager.Instance == null)
                return;
            AdminTimerText = UnityEngine.Object.Instantiate(HudManager.Instance.TaskText, HudManager.Instance.transform);
            AdminTimerText.transform.localPosition = new Vector3(-3.5f, -4.0f, 0);
            if (AdminTimer > 0)
                AdminTimerText.text = $"Admin: {AdminTimer.ToString("#0.0")} sec remaining";
            else
                AdminTimerText.text = "Admin: ran out of time";
            AdminTimerText.gameObject.SetActive(true);
        }

        private static void ClearAdminTimerText()
        {
            if (AdminTimerText == null)
                return;
            UnityEngine.Object.Destroy(AdminTimerText);
            AdminTimerText = null;
        }

        public static void UpdateVitalsTimerText() {
            if (!CustomOptionHolder.enabledVitalsTimer.getBool())
                return;
            if (HudManager.Instance == null)
                return;
            VitalsTimerText = UnityEngine.Object.Instantiate(HudManager.Instance.TaskText, HudManager.Instance.transform);
            VitalsTimerText.color = Color.green;
            VitalsTimerText.transform.localPosition = new Vector3(-1.0f, -4.0f, 0);
            if (VitalsTimer > 0)
                VitalsTimerText.text = $"Vitals: {VitalsTimer.ToString("#0.0")} sec remaining";
            else
                VitalsTimerText.text = "Vitals: ran out of time";
            VitalsTimerText.gameObject.SetActive(true);
        }

        private static void ClearVitalsTimerText() {
            if (VitalsTimerText == null)
                return;
            UnityEngine.Object.Destroy(VitalsTimerText);
            VitalsTimerText = null;
        }

        private static void UpdateTimer() {
            ClearAdminTimerText();
            UpdateAdminTimerText();

            if (Helpers.GetVitals() != null) {
                ClearVitalsTimerText();
                UpdateVitalsTimerText();
            }
        }
    }
} 