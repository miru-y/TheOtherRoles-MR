using System.Collections.Generic;
using BepInEx.Configuration;
using System;


namespace TheOtherRoles
{
    public static class BasicOptions
    {
        public static void Save(string section, Dictionary<ConfigDefinition, string> orphanedEntries, bool isSave = false) {

            var optionData = AmongUsClient.Instance.AmHost ? SaveManager.hostOptionsData : SaveManager.GameHostOptions;

            // Generic options
            mapId.Save(section, orphanedEntries, optionData.MapId);
            playerSpeedMod.Save(section, orphanedEntries, optionData.PlayerSpeedMod);
            crewLightMod.Save(section, orphanedEntries, optionData.CrewLightMod);
            impostorLightMod.Save(section, orphanedEntries, optionData.ImpostorLightMod);
            killCooldown.Save(section, orphanedEntries, optionData.KillCooldown);
            numCommonTasks.Save(section, orphanedEntries, optionData.NumCommonTasks);
            numLongTasks.Save(section, orphanedEntries, optionData.NumLongTasks);
            numShortTasks.Save(section, orphanedEntries, optionData.NumShortTasks);
            numEmergencyMeetings.Save(section, orphanedEntries, optionData.NumEmergencyMeetings);
            emergencyCooldown.Save(section, orphanedEntries, optionData.EmergencyCooldown);
            numImpostors.Save(section, orphanedEntries, optionData.NumImpostors);
            ghostsDoTasks.Save(section, orphanedEntries, optionData.GhostsDoTasks);
            killDistance.Save(section, orphanedEntries, optionData.KillDistance);
            discussionTime.Save(section, orphanedEntries, optionData.DiscussionTime);
            votingTime.Save(section, orphanedEntries, optionData.VotingTime);
            confirmImpostor.Save(section, orphanedEntries, optionData.ConfirmImpostor);
            visualTasks.Save(section, orphanedEntries, optionData.VisualTasks);
            anonymousVotes.Save(section, orphanedEntries, optionData.AnonymousVotes);
            taskBarMode.Save(section, orphanedEntries, optionData.TaskBarMode);
            isDefaults.Save(section, orphanedEntries, optionData.isDefaults);

            // Role options
            shapeshifterLeaveSkin.Save(section, orphanedEntries, optionData.RoleOptions.ShapeshifterLeaveSkin);
            shapeshifterCooldown.Save(section, orphanedEntries, optionData.RoleOptions.ShapeshifterCooldown);
            shapeshifterDuration.Save(section, orphanedEntries, optionData.RoleOptions.ShapeshifterDuration);
            scientistCooldown.Save(section, orphanedEntries, optionData.RoleOptions.ScientistCooldown);
            scientistBatteryCharge.Save(section, orphanedEntries, optionData.RoleOptions.ScientistBatteryCharge);
            guardianAngelCooldown.Save(section, orphanedEntries, optionData.RoleOptions.GuardianAngelCooldown);
            impostorsCanSeeProtect.Save(section, orphanedEntries, optionData.RoleOptions.ImpostorsCanSeeProtect);
            protectionDurationSeconds.Save(section, orphanedEntries, optionData.RoleOptions.ProtectionDurationSeconds);
            engineerCooldown.Save(section, orphanedEntries, optionData.RoleOptions.EngineerCooldown);
            engineerInVentMaxTime.Save(section, orphanedEntries, optionData.RoleOptions.EngineerInVentMaxTime);

            if (optionData.RoleOptions.roleRates.ContainsKey(RoleTypes.Scientist)) {
                var roleRate = optionData.RoleOptions.roleRates[RoleTypes.Scientist];
                scientistMaxCount.Save(section, orphanedEntries, roleRate.MaxCount);
                scientistChance.Save(section, orphanedEntries, roleRate.Chance);
            }
            if (optionData.RoleOptions.roleRates.ContainsKey(RoleTypes.Engineer)) {
                var roleRate = optionData.RoleOptions.roleRates[RoleTypes.Engineer];
                engineerMaxCount.Save(section, orphanedEntries, roleRate.MaxCount);
                engineerChance.Save(section, orphanedEntries, roleRate.Chance);
            }
            if (optionData.RoleOptions.roleRates.ContainsKey(RoleTypes.GuardianAngel)) {
                var roleRate = optionData.RoleOptions.roleRates[RoleTypes.GuardianAngel];
                guardianAngelMaxCount.Save(section, orphanedEntries, roleRate.MaxCount);
                guardianAngelChance.Save(section, orphanedEntries, roleRate.Chance);
            }
            if (optionData.RoleOptions.roleRates.ContainsKey(RoleTypes.Shapeshifter)) {
                var roleRate = optionData.RoleOptions.roleRates[RoleTypes.Shapeshifter];
                shapeshifterMaxCount.Save(section, orphanedEntries, roleRate.MaxCount);
                shapeshifterChance.Save(section, orphanedEntries, roleRate.Chance);
            }

            if (isSave)
                TheOtherRolesPlugin.Instance.Config.Save();
        }

        public static void Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries) {
            // Generic options
            if (mapId.Load(section, orphanedEntries, out byte byteValue))
                SaveManager.hostOptionsData.MapId = byteValue;
            if (playerSpeedMod.Load(section, orphanedEntries, out float floatValue))
                SaveManager.hostOptionsData.PlayerSpeedMod = floatValue;
            if (crewLightMod.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.CrewLightMod = floatValue;
            if (impostorLightMod.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.ImpostorLightMod = floatValue;
            if (killCooldown.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.KillCooldown = floatValue;
            if (numCommonTasks.Load(section, orphanedEntries, out int intValue))
                SaveManager.hostOptionsData.NumCommonTasks = intValue;
            if (numLongTasks.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.NumLongTasks = intValue;
            if (numShortTasks.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.NumShortTasks = intValue;
            if (numEmergencyMeetings.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.NumEmergencyMeetings = intValue;
            if (emergencyCooldown.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.EmergencyCooldown = intValue;
            if (numImpostors.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.NumImpostors = intValue;
            if (ghostsDoTasks.Load(section, orphanedEntries, out bool boolValue))
                SaveManager.hostOptionsData.GhostsDoTasks = boolValue;
            if (killDistance.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.KillDistance = intValue;
            if (discussionTime.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.DiscussionTime = intValue;
            if (votingTime.Load(section, orphanedEntries, out intValue))
                SaveManager.hostOptionsData.VotingTime = intValue;
            if (confirmImpostor.Load(section, orphanedEntries, out boolValue))
                SaveManager.hostOptionsData.ConfirmImpostor = boolValue;
            if (visualTasks.Load(section, orphanedEntries, out boolValue))
                SaveManager.hostOptionsData.VisualTasks = boolValue;
            if (anonymousVotes.Load(section, orphanedEntries, out boolValue))
                SaveManager.hostOptionsData.AnonymousVotes = boolValue;
            if (taskBarMode.Load(section, orphanedEntries, out TaskBarMode taskBarModeValue))
                SaveManager.hostOptionsData.TaskBarMode = taskBarModeValue;
            if (isDefaults.Load(section, orphanedEntries, out boolValue))
                SaveManager.hostOptionsData.isDefaults = boolValue;

            // Role options
            if (shapeshifterLeaveSkin.Load(section, orphanedEntries, out boolValue))
                SaveManager.hostOptionsData.RoleOptions.ShapeshifterLeaveSkin = boolValue;
            if (shapeshifterCooldown.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.ShapeshifterCooldown = floatValue;
            if (shapeshifterDuration.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.ShapeshifterDuration = floatValue;
            if (scientistCooldown.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.ScientistCooldown = floatValue;
            if (scientistBatteryCharge.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.ScientistBatteryCharge = floatValue;
            if (guardianAngelCooldown.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.GuardianAngelCooldown = floatValue;
            if (impostorsCanSeeProtect.Load(section, orphanedEntries, out boolValue))
                SaveManager.hostOptionsData.RoleOptions.ImpostorsCanSeeProtect = boolValue;
            if (protectionDurationSeconds.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.ProtectionDurationSeconds = floatValue;
            if (engineerCooldown.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.EngineerCooldown = floatValue;
            if (engineerInVentMaxTime.Load(section, orphanedEntries, out floatValue))
                SaveManager.hostOptionsData.RoleOptions.EngineerInVentMaxTime = floatValue;

            if (scientistMaxCount.Load(section, orphanedEntries, out intValue) && scientistChance.Load(section, orphanedEntries, out int intValue2))
                SaveManager.hostOptionsData.RoleOptions.SetRoleRate(RoleTypes.Scientist, intValue, intValue2);
            if (engineerMaxCount.Load(section, orphanedEntries, out intValue) && engineerChance.Load(section, orphanedEntries, out intValue2))
                SaveManager.hostOptionsData.RoleOptions.SetRoleRate(RoleTypes.Engineer, intValue, intValue2);
            if (guardianAngelMaxCount.Load(section, orphanedEntries, out intValue) && guardianAngelChance.Load(section, orphanedEntries, out intValue2))
                SaveManager.hostOptionsData.RoleOptions.SetRoleRate(RoleTypes.GuardianAngel, intValue, intValue2);
            if (shapeshifterMaxCount.Load(section, orphanedEntries, out intValue) && shapeshifterChance.Load(section, orphanedEntries, out intValue2))
                SaveManager.hostOptionsData.RoleOptions.SetRoleRate(RoleTypes.Shapeshifter, intValue, intValue2);
        }

        public static void Remove(string section, Dictionary<ConfigDefinition, string> orphanedEntries, bool isSave = false) {
            // Generic options
            mapId.Remove(section, orphanedEntries);
            playerSpeedMod.Remove(section, orphanedEntries);
            crewLightMod.Remove(section, orphanedEntries);
            impostorLightMod.Remove(section, orphanedEntries);
            killCooldown.Remove(section, orphanedEntries);
            numCommonTasks.Remove(section, orphanedEntries);
            numLongTasks.Remove(section, orphanedEntries);
            numShortTasks.Remove(section, orphanedEntries);
            numEmergencyMeetings.Remove(section, orphanedEntries);
            emergencyCooldown.Remove(section, orphanedEntries);
            numImpostors.Remove(section, orphanedEntries);
            ghostsDoTasks.Remove(section, orphanedEntries);
            killDistance.Remove(section, orphanedEntries);
            discussionTime.Remove(section, orphanedEntries);
            votingTime.Remove(section, orphanedEntries);
            confirmImpostor.Remove(section, orphanedEntries);
            visualTasks.Remove(section, orphanedEntries);
            anonymousVotes.Remove(section, orphanedEntries);
            taskBarMode.Remove(section, orphanedEntries);
            isDefaults.Remove(section, orphanedEntries);


            // Role options
            shapeshifterLeaveSkin.Remove(section, orphanedEntries);
            shapeshifterCooldown.Remove(section, orphanedEntries);
            shapeshifterDuration.Remove(section, orphanedEntries);
            scientistCooldown.Remove(section, orphanedEntries);
            scientistBatteryCharge.Remove(section, orphanedEntries);
            guardianAngelCooldown.Remove(section, orphanedEntries);
            impostorsCanSeeProtect.Remove(section, orphanedEntries);
            protectionDurationSeconds.Remove(section, orphanedEntries);
            engineerCooldown.Remove(section, orphanedEntries);
            engineerInVentMaxTime.Remove(section, orphanedEntries);


            scientistMaxCount.Remove(section, orphanedEntries);
            scientistChance.Remove(section, orphanedEntries);
            engineerMaxCount.Remove(section, orphanedEntries);
            engineerChance.Remove(section, orphanedEntries);
            guardianAngelMaxCount.Remove(section, orphanedEntries);
            guardianAngelChance.Remove(section, orphanedEntries);
            shapeshifterMaxCount.Remove(section, orphanedEntries);
            shapeshifterChance.Remove(section, orphanedEntries);


            if (isSave)
                TheOtherRolesPlugin.Instance.Config.Save();
        }


        class Option<T>
        {
            public int id { get; private set; }
            public T defaultData { get; private set; }

            public Option(int id, T defaultData) {
                this.id = id;
                this.defaultData = defaultData;
            }

            public void Save(string section, Dictionary<ConfigDefinition, string> orphanedEntries, T value) {
                var configDefinition = new ConfigDefinition(section, id.ToString());
                if (!orphanedEntries.ContainsKey(configDefinition))
                    orphanedEntries.Add(configDefinition, value.ToString());
                else
                    orphanedEntries[configDefinition] = value.ToString();
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out int outValue) {
                outValue = 0;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && int.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out float outValue) {
                outValue = 0;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && float.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out byte outValue) {
                outValue = 0;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && byte.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out bool outValue) {
                outValue = false;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && bool.TryParse(value, out outValue);
            }

            public bool Load(string section, Dictionary<ConfigDefinition, string> orphanedEntries, out TaskBarMode outValue) {
                outValue = TaskBarMode.Normal;
                var configDefinition = new ConfigDefinition(section, id.ToString());
                return orphanedEntries.TryGetValue(configDefinition, out string value) && TaskBarMode.TryParse(value, out outValue);
            }

            public bool Remove(string section, Dictionary<ConfigDefinition, string> orphanedEntries) {
                var configDefinition = new ConfigDefinition(section, id.ToString());
                if (orphanedEntries.ContainsKey(configDefinition)) {
                    orphanedEntries.Remove(configDefinition);
                    return true;
                }
                return false;
            }
        }

        // Generic options
        //static Option<InnerNet.GameKeywords> keywords;
        //static Option<int> maxPlayers;
        static Option<byte> mapId;
        static Option<float> playerSpeedMod;
        static Option<float> crewLightMod;
        static Option<float> impostorLightMod;
        static Option<float> killCooldown;
        static Option<int> numCommonTasks;
        static Option<int> numLongTasks;
        static Option<int> numShortTasks;
        static Option<int> numEmergencyMeetings;
        static Option<int> emergencyCooldown;
        static Option<int> numImpostors;
        static Option<bool> ghostsDoTasks;
        static Option<int> killDistance;
        static Option<int> discussionTime;
        static Option<int> votingTime;
        static Option<bool> confirmImpostor;
        static Option<bool> visualTasks;
        static Option<bool> anonymousVotes;
        static Option<TaskBarMode> taskBarMode;
        static Option<bool> isDefaults;

        // Role options
        static Option<bool> shapeshifterLeaveSkin;
        static Option<float> shapeshifterCooldown;
        static Option<float> shapeshifterDuration;
        static Option<float> scientistCooldown;
        static Option<float> scientistBatteryCharge;
        static Option<float> guardianAngelCooldown;
        static Option<bool> impostorsCanSeeProtect;
        static Option<float> protectionDurationSeconds;
        static Option<float> engineerCooldown;
        static Option<float> engineerInVentMaxTime;

        static Option<int> scientistMaxCount;
        static Option<int> scientistChance;

        static Option<int> engineerMaxCount;
        static Option<int> engineerChance;

        static Option<int> guardianAngelMaxCount;
        static Option<int> guardianAngelChance;

        static Option<int> shapeshifterMaxCount;
        static Option<int> shapeshifterChance;

        static GameOptionsData defaultData = null;

        static BasicOptions() {
            defaultData = new GameOptionsData();

            // Generic options : 890000000-
            //keywords = new Option<InnerNet.GameKeywords>(890000000, defaultData.Keywords);
            //maxPlayers = new Option<int>(890000001, defaultData.MaxPlayers);
            mapId = new Option<byte>(890000002, defaultData.MapId);
            playerSpeedMod = new Option<float>(890000003, defaultData.PlayerSpeedMod);
            crewLightMod = new Option<float>(890000004, defaultData.CrewLightMod);
            impostorLightMod = new Option<float>(890000005, defaultData.ImpostorLightMod);
            killCooldown = new Option<float>(890000006, defaultData.KillCooldown);
            numCommonTasks = new Option<int>(890000007, defaultData.NumCommonTasks);
            numLongTasks = new Option<int>(890000008, defaultData.NumLongTasks);
            numShortTasks = new Option<int>(890000009, defaultData.NumShortTasks);
            numEmergencyMeetings = new Option<int>(890000010, defaultData.NumEmergencyMeetings);
            emergencyCooldown = new Option<int>(890000011, defaultData.EmergencyCooldown);
            numImpostors = new Option<int>(890000012, defaultData.NumImpostors);
            ghostsDoTasks = new Option<bool>(890000013, defaultData.GhostsDoTasks);
            killDistance = new Option<int>(890000014, defaultData.KillDistance);
            discussionTime = new Option<int>(890000015, defaultData.DiscussionTime);
            votingTime = new Option<int>(890000016, defaultData.VotingTime);
            confirmImpostor = new Option<bool>(890000017, defaultData.ConfirmImpostor);
            visualTasks = new Option<bool>(890000018, defaultData.VisualTasks);
            anonymousVotes = new Option<bool>(890000019, defaultData.AnonymousVotes);
            taskBarMode = new Option<TaskBarMode>(890000020, defaultData.TaskBarMode);
            isDefaults = new Option<bool>(890000021, defaultData.isDefaults);

            // Role options : 891000000-
            shapeshifterLeaveSkin = new Option<bool>(891000000, defaultData.RoleOptions.ShapeshifterLeaveSkin);
            shapeshifterCooldown = new Option<float>(891000001, defaultData.RoleOptions.ShapeshifterCooldown);
            shapeshifterDuration = new Option<float>(891000002, defaultData.RoleOptions.ShapeshifterDuration);
            scientistCooldown = new Option<float>(891000003, defaultData.RoleOptions.ScientistCooldown);
            scientistBatteryCharge = new Option<float>(891000004, defaultData.RoleOptions.ScientistBatteryCharge);
            guardianAngelCooldown = new Option<float>(891000005, defaultData.RoleOptions.GuardianAngelCooldown);
            impostorsCanSeeProtect = new Option<bool>(891000006, defaultData.RoleOptions.ImpostorsCanSeeProtect);
            protectionDurationSeconds = new Option<float>(891000007, defaultData.RoleOptions.ProtectionDurationSeconds);
            engineerCooldown = new Option<float>(891000008, defaultData.RoleOptions.EngineerCooldown);
            engineerInVentMaxTime = new Option<float>(891000009, defaultData.RoleOptions.EngineerInVentMaxTime);

            // Role generic options : 892000000-
            scientistMaxCount = new Option<int>(892000000, 0);
            scientistChance = new Option<int>(892000001, 0);

            engineerMaxCount = new Option<int>(892000100, 0);
            engineerChance = new Option<int>(892000101, 0);

            guardianAngelMaxCount = new Option<int>(892000200, 0);
            guardianAngelChance = new Option<int>(892000201, 0);

            shapeshifterMaxCount = new Option<int>(892000300, 0);
            shapeshifterChance = new Option<int>(892000301, 0);
        }
    }
}