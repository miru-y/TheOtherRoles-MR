using HarmonyLib;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;
using System.Reflection;
using TheOtherRoles.Players;

namespace TheOtherRoles.Patches 
{
    [HarmonyPatch]
    public static class ClientOptionsPatch
    {
        static SelectionBehaviour[] AllOptions = {
            new SelectionBehaviour("Ghosts See Remaining Tasks", () => MapOptions.ghostsSeeTasks = TheOtherRolesPlugin.GhostsSeeTasks.Value = !TheOtherRolesPlugin.GhostsSeeTasks.Value, TheOtherRolesPlugin.GhostsSeeTasks.Value),
            new SelectionBehaviour("Ghosts Can See Votes", () => MapOptions.ghostsSeeVotes = TheOtherRolesPlugin.GhostsSeeVotes.Value = !TheOtherRolesPlugin.GhostsSeeVotes.Value, TheOtherRolesPlugin.GhostsSeeVotes.Value),
            new SelectionBehaviour("Ghosts Can See Roles", () => MapOptions.ghostsSeeRoles = TheOtherRolesPlugin.GhostsSeeRoles.Value = !TheOtherRolesPlugin.GhostsSeeRoles.Value, TheOtherRolesPlugin.GhostsSeeRoles.Value),
            new SelectionBehaviour("Ghosts Can Additionally See Modifier", () => MapOptions.ghostsSeeModifier = TheOtherRolesPlugin.GhostsSeeModifier.Value = !TheOtherRolesPlugin.GhostsSeeModifier.Value, TheOtherRolesPlugin.GhostsSeeModifier.Value),
            new SelectionBehaviour("Show Role Summary", () => MapOptions.showRoleSummary = TheOtherRolesPlugin.ShowRoleSummary.Value = !TheOtherRolesPlugin.ShowRoleSummary.Value, TheOtherRolesPlugin.ShowRoleSummary.Value),
            new SelectionBehaviour("Show Lighter / Darker", () => MapOptions.showLighterDarker = TheOtherRolesPlugin.ShowLighterDarker.Value = !TheOtherRolesPlugin.ShowLighterDarker.Value, TheOtherRolesPlugin.ShowLighterDarker.Value),
            new SelectionBehaviour("Enable Sound Effects", () => MapOptions.enableSoundEffects = TheOtherRolesPlugin.EnableSoundEffects.Value = !TheOtherRolesPlugin.EnableSoundEffects.Value, TheOtherRolesPlugin.EnableSoundEffects.Value),
        };

        public static bool isOpenPreset = false;
        public static GameObject popUp;
        static TextMeshPro titleText;
        static ToggleButtonBehaviour buttonPrefab;
        static Vector3? _origin;


        class PresetInfo
        {
            public string section { get; set; }
            public string presetName { get; set; }
            public long registTime { get; set; }

            public PresetInfo(string presetName) {
                SetPresetName(presetName);
            }

            public void SetRegistTime(long time) {
                registTime = time;
            }

            public string GetDescription() {
                string description = "";
                int roleCount = 0;
                foreach (var option in CustomOption.options) {
                    if (option.id == 0) continue;
                    var configDefinition = new BepInEx.Configuration.ConfigDefinition(section, option.id.ToString());
                    int v = option.defaultSelection;
                    if (orphanedEntries.TryGetValue(configDefinition, out string value))
                        int.TryParse(value, out v);
                    if (option.isRoleHeader() && v > 0) {
                        ++roleCount;
                        if (roleCount <= 8) {
                            if (roleCount > 1) description += "/";
                            description += option.name;
                        }
                    }
                }
                if (roleCount > 8)
                    description += string.Format(" +{0}Roles", roleCount - 8);

                return description;
            }

            public void Initialize() {
                var configDefinition = new BepInEx.Configuration.ConfigDefinition(section, "0");
                if (orphanedEntries.TryGetValue(configDefinition, out string value) && long.TryParse(value, out long time))
                    SetRegistTime(time);
            }

            public void OnLoad() {
                BasicOptions.Load(section, orphanedEntries);
                foreach (CustomOption option in CustomOption.options) {
                    if (option.id == 0) continue;

                    int v = option.defaultSelection;
                    var configDefinition = new BepInEx.Configuration.ConfigDefinition(section, option.id.ToString());
                    if (orphanedEntries.TryGetValue(configDefinition, out string value))
                        int.TryParse(value, out v);
                    option.updateSelection(v, false);
                }
                TheOtherRolesPlugin.Instance.Config.Save();
                CustomOption.ShareOptionSelections();
                CachedPlayer.LocalPlayer.PlayerControl.RpcSyncSettings(GameOptionsData.hostOptionsData);
            }

            public void OnRename(string newPresetName) {
                var optionTable = new Dictionary<int, string>();
                foreach (var option in CustomOption.options) {
                    if (option.id == 0) continue;
                    var definition = new BepInEx.Configuration.ConfigDefinition(section, option.id.ToString());
                    if (orphanedEntries.TryGetValue(definition, out string value))
                        optionTable.Add(option.id, value);
                }
                OnRemove(false);
                if (string.IsNullOrEmpty(newPresetName))
                    newPresetName = GetUniquePresetName();
                SetPresetName(newPresetName);
                var configDefinition = new BepInEx.Configuration.ConfigDefinition(section, "0");
                orphanedEntries.Add(configDefinition, registTime.ToString());
                foreach (var option in CustomOption.options) {
                    if (option.id == 0) continue;
                    var definition = new BepInEx.Configuration.ConfigDefinition(section, option.id.ToString());
                    orphanedEntries.Add(definition, optionTable[option.id]);
                }
                TheOtherRolesPlugin.Instance.Config.Save();
            }

            public void OnRemove(bool isSave = true) {
                BasicOptions.Remove(section, orphanedEntries);
                var configDefinition = new BepInEx.Configuration.ConfigDefinition(section, "0");
                if (!TheOtherRolesPlugin.Instance.Config.Remove(configDefinition))
                    orphanedEntries.Remove(configDefinition);
                foreach (var option in CustomOption.options) {
                    configDefinition = new BepInEx.Configuration.ConfigDefinition(section, option.id.ToString());
                    if (!TheOtherRolesPlugin.Instance.Config.Remove(configDefinition))
                        orphanedEntries.Remove(configDefinition);
                }
                if (isSave)
                    TheOtherRolesPlugin.Instance.Config.Save();
            }

            void SetPresetName(string presetName) {
                this.presetName = presetName;
                section = $"CustomPreset_{presetName}";
            }
        }

        const int PresetInfoOnePageViewMax = 4;
        static OptionsMenuBehaviour _instance = null;
        static List<PresetInfo> presetInfoList = new List<PresetInfo>();
        static List<GameObject> presetInfoObjList = new List<GameObject>();
        static int presetInfoPageNow = 0;
        static int presetInfoPageMax = 0;
        static TextMeshPro presetTitle = null;
        static GameObject presetRoot = null;
        static SelectionBehaviour prevPresetPageInfo = null; 
        static SelectionBehaviour nextPresetPageInfo = null;
        static SelectionBehaviour createNewPresetInfo = null;
        static GameObject createNewPresetPopUp = null;
        static EditName createNewPresetEditName = null;
        static GameObject renamePresetPopUp = null;
        static EditName renamePresetEditName = null;
        static Dictionary<BepInEx.Configuration.ConfigDefinition, string> orphanedEntries = null;
        static SelectionBehaviourObservable tabObservable = new SelectionBehaviourObservable();
        static SelectionBehaviour optionTabInfo = null;
        static SelectionBehaviour presetTabInfo = null;


        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static void MainMenuManager_StartPostfix(MainMenuManager __instance) {
            // Prefab for the title
            var tmp = __instance.Announcement.transform.Find("Title_Text").gameObject.GetComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.transform.localPosition += Vector3.left * 0.2f;
            titleText = Object.Instantiate(tmp);
            Object.Destroy(titleText.GetComponent<TextTranslatorTMP>());
            titleText.gameObject.SetActive(false);
            Object.DontDestroyOnLoad(titleText);
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Start))]
        public static void OptionsMenuBehaviour_StartPostfix(OptionsMenuBehaviour __instance) {
            _instance = __instance;
            if (!__instance.CensorChatButton) return;

            if (!popUp) {
                CreateCustom(__instance);
            }

            if (!buttonPrefab) {
                buttonPrefab = Object.Instantiate(__instance.CensorChatButton);
                Object.DontDestroyOnLoad(buttonPrefab);
                buttonPrefab.name = "CensorChatPrefab";
                buttonPrefab.gameObject.SetActive(false);
            }

            SetUpOptions();
            InitializeMoreButton(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OptionsMenuBehaviour), nameof(OptionsMenuBehaviour.Open))]
        public static void OptionsMenuBehaviour_OpenPostfix(OptionsMenuBehaviour __instance) {
            if (isOpenPreset) {
                __instance.StartCoroutine(Effects.Lerp(0.01f, new Action<float>((p) => {
                    OnMoreButton(__instance);
                })));
            }
        }

        static void CreateCustom(OptionsMenuBehaviour prefab) {
            popUp = Object.Instantiate(prefab.gameObject);
            Object.DontDestroyOnLoad(popUp);
            var transform = popUp.transform;
            var pos = transform.localPosition;
            pos.z = -810f;
            transform.localPosition = pos;

            Object.Destroy(popUp.GetComponent<OptionsMenuBehaviour>());
            foreach (var gObj in popUp.gameObject.GetAllChilds()) {
                switch (gObj.name) {
                    case "Background":
                    case "CloseButton": 
                        {
                        }
                        break;
                    default: 
                        {
                            Object.Destroy(gObj);
                        }
                        break;
                }
            }

            popUp.SetActive(false);
        }

        static void InitializeMoreButton(OptionsMenuBehaviour __instance) {
            var moreOptions = Object.Instantiate(buttonPrefab, __instance.CensorChatButton.transform.parent);
            var transform = __instance.CensorChatButton.transform;
            __instance.CensorChatButton.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
            _origin ??= transform.localPosition;

            transform.localPosition = _origin.Value + Vector3.left * 0.45f;
            transform.localScale = new Vector3(0.66f, 1, 1);
            __instance.EnableFriendInvitesButton.transform.localScale = new Vector3(0.66f, 1, 1);
            __instance.EnableFriendInvitesButton.transform.localPosition += Vector3.right * 0.5f;
            __instance.EnableFriendInvitesButton.Text.transform.localScale = new Vector3(1.2f, 1, 1);

            moreOptions.transform.localPosition = _origin.Value + Vector3.right * 4f / 3f;
            moreOptions.transform.localScale = new Vector3(0.66f, 1, 1);

            moreOptions.gameObject.SetActive(true);
            moreOptions.Text.text = "Mod Options...";
            moreOptions.Text.transform.localScale = new Vector3(1 / 0.66f, 1, 1);
            var moreOptionsButton = moreOptions.GetComponent<PassiveButton>();
            moreOptionsButton.OnClick = new ButtonClickedEvent();
            moreOptionsButton.OnClick.AddListener((Action)(() => {
                OnMoreButton(__instance);
            }));
        }

        static void RefreshOpen() {
            popUp.gameObject.SetActive(false);
            popUp.gameObject.SetActive(true);
            SetUpOptions();
        }

        static void OnMoreButton(OptionsMenuBehaviour __instance) {
            if (!popUp) return;

            if (__instance.transform.parent && __instance.transform.parent == FastDestroyableSingleton<HudManager>.Instance.transform) {
                popUp.transform.SetParent(FastDestroyableSingleton<HudManager>.Instance.transform);
                popUp.transform.localPosition = new Vector3(0, 0, -800f);
            } else {
                popUp.transform.SetParent(null);
                Object.DontDestroyOnLoad(popUp);
            }

            RefreshOpen();
        }

        static void SetUpOptions() {
            if (popUp.transform.GetComponentInChildren<ToggleButtonBehaviour>()) {
                if (isOpenPreset && presetTabInfo != null) {
                    isOpenPreset = false;
                    presetTabInfo.Select();
                }
                if (createNewPresetInfo != null) {
                    createNewPresetInfo.SetActive(AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started);
                }
                UpdatePresetInfo();
                return;
            }
            tabObservable.Clear();

            // Tab
            SelectionBehaviour.InitDesc tabDesc = new SelectionBehaviour.InitDesc();
            tabDesc.buttonPrefab = buttonPrefab;
            tabDesc.parent = popUp.transform;
            tabDesc.observable = tabObservable;
            tabDesc.font = titleText.font;
            tabDesc.selectColor = Color.white;
            tabDesc.unselectColor = new Color32(85, 85, 85, byte.MaxValue);
            tabDesc.mouseOverColor = Color.green;
            tabDesc.buttonSize = new Vector2(1f, .5f);
            tabDesc.colliderButtonSize = new Vector2(1f, .5f);

            // Option Tab
            var optionRoot = new GameObject("OptionRoot");
            optionRoot.transform.SetParent(popUp.transform);
            optionRoot.transform.localPosition = Vector3.zero;
            optionRoot.transform.localScale = Vector3.one;
            optionTabInfo = new SelectionBehaviour("Option", () => { return true; }, true, optionRoot);
            tabDesc.pos = new Vector3(-.7f, 2.35f, -.5f);
            optionTabInfo.Initialize(tabDesc);

            // Option Title
            var optionTitle = Object.Instantiate(titleText, optionRoot.transform);
            optionTitle.GetComponent<RectTransform>().localPosition = new Vector3(0, 1.8f, -.5f);
            optionTitle.gameObject.SetActive(true);
            optionTitle.text = "More Options...";
            optionTitle.name = "OptionTitleText";

            // Option Contents
            SelectionBehaviour.InitDesc optionButtonDesc = new SelectionBehaviour.InitDesc();
            optionButtonDesc.buttonPrefab = buttonPrefab;
            optionButtonDesc.parent = optionRoot.transform;
            optionButtonDesc.font = titleText.font;
            for (var i = 0; i < AllOptions.Length; i++) {
                optionButtonDesc.pos = new Vector3(i % 2 == 0 ? -1.17f : 1.17f, 0.8f - i / 2 * 0.8f, -.5f);
                AllOptions[i].Initialize(optionButtonDesc);
            }

            // Preset Tab
            presetRoot = new GameObject("PresetRoot");
            presetRoot.transform.SetParent(popUp.transform);
            presetRoot.transform.localPosition = Vector3.zero;
            presetRoot.transform.localScale = Vector3.one;
            presetTabInfo = new SelectionBehaviour("Preset", () => { return true; }, false, presetRoot);
            tabDesc.pos = new Vector3(.7f, 2.35f, -.5f);
            presetTabInfo.Initialize(tabDesc);
            presetTabInfo.ChangeButtonState(false);

            // Preset Title
            presetTitle = Object.Instantiate(titleText, presetRoot.transform);
            presetTitle.GetComponent<RectTransform>().localPosition = new Vector3(0, 1.8f, -.5f);
            presetTitle.gameObject.SetActive(true);
            presetTitle.name = "PresetTitleText";

            // Preset Contents
            // List
            presetInfoList.Clear();
            string customPresetPrefix = "CustomPreset_";
            var property = typeof(BepInEx.Configuration.ConfigFile).GetProperty("OrphanedEntries", BindingFlags.NonPublic | BindingFlags.Instance);
            var getter = property.GetGetMethod(true);
            if (getter != null) {
                orphanedEntries = getter.Invoke(TheOtherRolesPlugin.Instance.Config, new object[0]) as Dictionary<BepInEx.Configuration.ConfigDefinition, string>;
                for (int i = 0; i < orphanedEntries.Count; ++i) {
                    string section = orphanedEntries.ElementAt(i).Key.Section;
                    if (section.Contains(customPresetPrefix)) {
                        string name = section.Substring(section.IndexOf(customPresetPrefix) + customPresetPrefix.Length);
                        if (presetInfoList.Find((info) => info.presetName == name) == null)
                            presetInfoList.Add(new PresetInfo(name));
                    }
                }
            }
            for (int i = 0; i < TheOtherRolesPlugin.Instance.Config.Count; ++i) {
                string section = TheOtherRolesPlugin.Instance.Config.ElementAt(i).Key.Section;
                if (section.Contains(customPresetPrefix)) {
                    string name = section.Substring(section.IndexOf(customPresetPrefix) + customPresetPrefix.Length);
                    if (presetInfoList.Find((info) => info.presetName == name) == null)
                        presetInfoList.Add(new PresetInfo(name));
                }
            }
            for (int i = 0; i < presetInfoList.Count; ++i)
                presetInfoList[i].Initialize();
            presetInfoList.Sort((l, r) => {
                long c = l.registTime - r.registTime;
                if (c > 0) return 1;
                if (c < 0) return -1;
                return 0;
            });

            // Buttons
            SelectionBehaviour.InitDesc presetButtonDesc = new SelectionBehaviour.InitDesc();
            presetButtonDesc.buttonPrefab = buttonPrefab;
            presetButtonDesc.parent = presetRoot.transform;
            presetButtonDesc.font = titleText.font;
            presetButtonDesc.selectColor = Color.white;
            presetButtonDesc.unselectColor = Color.white;
            presetButtonDesc.pos = new Vector3(0f, -2.3f, -.5f);
            presetButtonDesc.buttonSize = new Vector2(2f, .5f);
            presetButtonDesc.colliderButtonSize = new Vector2(2f, .5f);
            createNewPresetInfo = new SelectionBehaviour("Create New Preset", () => { return OnCreateNewPreset(); });
            createNewPresetInfo.Initialize(presetButtonDesc);
            createNewPresetInfo.SetActive(AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started);

            prevPresetPageInfo = new SelectionBehaviour("Prev", () => { return OnPrevPresetPage(); });
            presetButtonDesc.buttonSize = new Vector2(1f, .5f);
            presetButtonDesc.colliderButtonSize = new Vector2(1f, .5f);
            presetButtonDesc.pos = new Vector3(-.7f, -1.7f, -.5f);
            prevPresetPageInfo.Initialize(presetButtonDesc);

            nextPresetPageInfo = new SelectionBehaviour("Next", () => { return OnNextPresetPage(); });
            presetButtonDesc.pos = new Vector3(.7f, -1.7f, -.5f);
            nextPresetPageInfo.Initialize(presetButtonDesc);

            presetInfoPageNow = 1;
            presetInfoPageMax = ((presetInfoList.Count - 1) / PresetInfoOnePageViewMax) + 1;

            UpdatePresetInfo();

            if (isOpenPreset) {
                isOpenPreset = false;
                presetTabInfo.Select();
            }
        }

        static bool OnCreateNewPreset() {
            if (createNewPresetPopUp) {
                var button = createNewPresetPopUp.transform.FindChild("SubmitButton").GetComponentInChildren<PassiveButton>();
                _instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                    createNewPresetEditName.nameText.nameSource.SetText("");
                    createNewPresetPopUp.gameObject.SetLayerRecursively(button.gameObject.layer);
                })));
                createNewPresetPopUp.SetActive(true);
                return false;
            }

            createNewPresetPopUp = Object.Instantiate(AccountManager.Instance.accountTab.editNameScreen.gameObject, popUp.transform);
            Object.DontDestroyOnLoad(createNewPresetPopUp);

            var pos = createNewPresetPopUp.transform.localPosition;
            pos.z = -50f;
            createNewPresetPopUp.transform.localPosition = pos;
            createNewPresetPopUp.SetActive(true);

            _instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => { 
                createNewPresetEditName = createNewPresetPopUp.GetComponentInChildren<EditName>();
                createNewPresetEditName.nameText.nameSource.SetText("");
                var titleText = createNewPresetPopUp.transform.FindChild("TitleText_TMP").GetComponent<TextMeshPro>();
                titleText.SetText("Create a preset with the current settings.");
                var nameText = createNewPresetPopUp.transform.FindChild("ChangeNameTitle_TMP").GetComponent<TextMeshPro>();
                nameText.SetText("Preset Name");
                var submitText = createNewPresetPopUp.transform.FindChild("SubmitButton").GetComponentInChildren<TextMeshPro>();
                submitText.SetText("Create");
                var backText = createNewPresetPopUp.transform.FindChild("BackButton").GetComponentInChildren<TextMeshPro>();
                backText.SetText("Back");
                var submitButton = createNewPresetPopUp.transform.FindChild("SubmitButton").GetComponentInChildren<PassiveButton>();
                submitButton.OnClick = new ButtonClickedEvent();
                submitButton.OnClick.AddListener((Action)(() => {
                    if (!createNewPresetPopUp) return;
                    if (presetInfoList.Find((info) => info.presetName == createNewPresetEditName.nameText.nameSource.text) == null)
                        CreateNewPreset(createNewPresetEditName.nameText.nameSource.text);
                    createNewPresetEditName.Close();
                }));
                createNewPresetPopUp.transform.FindChild("RandomizeName").gameObject.SetActive(false);
                createNewPresetPopUp.gameObject.SetLayerRecursively(submitButton.gameObject.layer);
            })));

            return false;
        }

        static bool OnRenamePreset(PresetInfo info) {
            if (renamePresetPopUp) {
                var button = renamePresetPopUp.transform.FindChild("SubmitButton").GetComponentInChildren<PassiveButton>();
                _instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                    renamePresetEditName.nameText.nameSource.SetText(info.presetName);
                    renamePresetPopUp.gameObject.SetLayerRecursively(button.gameObject.layer);
                })));
                renamePresetPopUp.SetActive(true);
                return false;
            }

            renamePresetPopUp = Object.Instantiate(AccountManager.Instance.accountTab.editNameScreen.gameObject, popUp.transform);
            Object.DontDestroyOnLoad(renamePresetPopUp);

            var pos = renamePresetPopUp.transform.localPosition;
            pos.z = -50f;
            renamePresetPopUp.transform.localPosition = pos;
            renamePresetPopUp.SetActive(true);

            _instance.StartCoroutine(Effects.Lerp(0.1f, new Action<float>((p) => {
                renamePresetEditName = renamePresetPopUp.GetComponentInChildren<EditName>();
                renamePresetEditName.nameText.nameSource.SetText(info.presetName);
                var titleText = renamePresetPopUp.transform.FindChild("TitleText_TMP").GetComponent<TextMeshPro>();
                titleText.SetText("Rename a preset.");
                var nameText = renamePresetPopUp.transform.FindChild("ChangeNameTitle_TMP").GetComponent<TextMeshPro>();
                nameText.SetText("Preset Name");
                var submitText = renamePresetPopUp.transform.FindChild("SubmitButton").GetComponentInChildren<TextMeshPro>();
                submitText.SetText("Rename");
                var backText = renamePresetPopUp.transform.FindChild("BackButton").GetComponentInChildren<TextMeshPro>();
                backText.SetText("Back");
                var submitButton = renamePresetPopUp.transform.FindChild("SubmitButton").GetComponentInChildren<PassiveButton>();
                submitButton.OnClick = new ButtonClickedEvent();
                submitButton.OnClick.AddListener((Action)(() => {
                    if (!renamePresetPopUp) return;
                    if (presetInfoList.Find((info) => info.presetName == renamePresetEditName.nameText.nameSource.text) == null) {
                        info.OnRename(renamePresetEditName.nameText.nameSource.text);
                        UpdatePresetInfo();
                    }
                    renamePresetEditName.Close();
                }));
                renamePresetPopUp.transform.FindChild("RandomizeName").gameObject.SetActive(false);
                renamePresetPopUp.gameObject.SetLayerRecursively(submitButton.gameObject.layer);
            })));

            return false;
        }

        static bool OnPrevPresetPage() {
            if (presetInfoPageMax > 0) {
                if (--presetInfoPageNow <= 0)
                    presetInfoPageNow = presetInfoPageMax;
                UpdatePresetInfo();
            }
            return false;
        }

        static bool OnNextPresetPage() {
            if (presetInfoPageMax > 0) {
                if (++presetInfoPageNow > presetInfoPageMax)
                    presetInfoPageNow = 1;
                UpdatePresetInfo();
            }
            return false;
        }

        static string GetUniquePresetName() {
            int id = 1;
            while (true) {
                var definition = new BepInEx.Configuration.ConfigDefinition($"CustomPreset_NewPreset{id}", "0");
                if (!orphanedEntries.TryGetValue(definition, out string value))
                    return $"NewPreset{id}";
                ++id;
            }
        }

        static void CreateNewPreset(string name) {
            long registTime = DateTime.Now.Ticks;
            if (string.IsNullOrEmpty(name))
                name = GetUniquePresetName();
            var presetInfo = new PresetInfo(name);
            presetInfoList.Add(presetInfo);
            presetInfoPageNow = presetInfoPageMax = ((presetInfoList.Count - 1) / PresetInfoOnePageViewMax) + 1;
            var configDefinition = new BepInEx.Configuration.ConfigDefinition(presetInfo.section, "0");
            if (!orphanedEntries.ContainsKey(configDefinition))
                orphanedEntries.Add(configDefinition, registTime.ToString());
            else
                orphanedEntries[configDefinition] = registTime.ToString();

            foreach (var option in CustomOption.options) {
                if (option.id == 0) continue;
                configDefinition = new BepInEx.Configuration.ConfigDefinition(presetInfo.section, option.id.ToString());
                if (!orphanedEntries.ContainsKey(configDefinition))
                    orphanedEntries.Add(configDefinition, option.selection.ToString());
                else
                    orphanedEntries[configDefinition] = option.selection.ToString();
            }
            presetInfo.SetRegistTime(registTime);
            BasicOptions.Save(presetInfo.section, orphanedEntries);
            TheOtherRolesPlugin.Instance.Config.Save();
            UpdatePresetInfo();
        }

        static void UpdatePresetInfo() {
            if (buttonPrefab == null || presetRoot == null) return;

            for (int i = 0; i < presetInfoObjList.Count; ++i) {
                if (presetInfoObjList[i] != null)
                    GameObject.Destroy(presetInfoObjList[i]);
            }
            presetInfoObjList.Clear();

            SelectionBehaviour.InitDesc desc = new SelectionBehaviour.InitDesc();
            desc.buttonPrefab = buttonPrefab;
            desc.parent = presetRoot.transform;
            desc.font = titleText.font;
            desc.selectColor = Color.white;
            desc.unselectColor = Color.white;
            desc.mouseOverColor = Color.white;
            desc.buttonSize = new Vector2(5f, .6f);
            desc.colliderButtonSize = new Vector2(5f, .6f);
            desc.textAlignment = TextAlignmentOptions.MidlineLeft;
            desc.textOffset = new Vector2(.1f, -.05f);

            SelectionBehaviour.InitDesc subButtonDesc = new SelectionBehaviour.InitDesc();
            subButtonDesc.buttonPrefab = buttonPrefab;
            subButtonDesc.font = titleText.font;
            subButtonDesc.mouseOverColor = Color.green;
            subButtonDesc.buttonSize = new Vector2(.63f, .3f);
            subButtonDesc.colliderButtonSize = new Vector2(.63f, .3f);

            for (int i = 0; i < PresetInfoOnePageViewMax; ++i) {
                int idx = (presetInfoPageNow - 1) * PresetInfoOnePageViewMax + i;
                if (idx >= presetInfoList.Count)
                    break;
                var info = presetInfoList[idx];
                var presetInfo = new SelectionBehaviour(info.presetName, () => { return false; });
                desc.pos = new Vector3(0f, 1.18f - i * .75f, -.5f);
                desc.buttonName = string.Format("{0}\n<size=70%>{1}", info.presetName, info.GetDescription());
                presetInfo.Initialize(desc);
                presetInfoObjList.Add(presetInfo._transform.gameObject);

                if (AmongUsClient.Instance.AmHost && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    subButtonDesc.parent = presetInfo._transform;
                    subButtonDesc.pos = new Vector3(.6f, .1f, -5f);
                    subButtonDesc.selectColor = Color.yellow;
                    subButtonDesc.unselectColor = subButtonDesc.selectColor;
                    var loadButtonInfo = new SelectionBehaviour("Load", () => {
                        info.OnLoad();
                        return false;
                    });
                    loadButtonInfo.Initialize(subButtonDesc);
                }

                if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) {
                    subButtonDesc.parent = presetInfo._transform;
                    subButtonDesc.pos = new Vector3(1.3f, .1f, -5f);
                    subButtonDesc.selectColor = Color.cyan;
                    subButtonDesc.unselectColor = subButtonDesc.selectColor;
                    var renameButtonInfo = new SelectionBehaviour("Rename", () => {
                        OnRenamePreset(info);
                        return false;
                    });
                    renameButtonInfo.Initialize(subButtonDesc);
                }

                subButtonDesc.parent = presetInfo._transform;
                subButtonDesc.pos = new Vector3(2.0f, .1f, -5f);
                subButtonDesc.selectColor = new Color32(235, 76, 70, 0xff);
                subButtonDesc.unselectColor = subButtonDesc.selectColor;
                var deleteButtonInfo = new SelectionBehaviour("Delete", () => {
                    info.OnRemove();
                    presetInfoList.Remove(info);
                    presetInfoPageMax = ((presetInfoList.Count - 1) / PresetInfoOnePageViewMax) + 1;
                    presetInfoPageNow = Mathf.Min(presetInfoPageNow, presetInfoPageMax);
                    UpdatePresetInfo();
                    return false;
                });
                deleteButtonInfo.Initialize(subButtonDesc);
            }

            prevPresetPageInfo._transform.gameObject.SetActive(presetInfoPageMax > 1);
            nextPresetPageInfo._transform.gameObject.SetActive(presetInfoPageMax > 1);
            presetTitle.text = String.Format("Preset ({0}/{1})", presetInfoPageNow, presetInfoPageMax);
        }

        static IEnumerable<GameObject> GetAllChilds(this GameObject Go) {
            for (var i = 0; i < Go.transform.childCount; i++) {
                yield return Go.transform.GetChild(i).gameObject;
            }
        }

        static void SetLayerRecursively(this GameObject self, int layer) {
            self.layer = layer;
            for (int i = 0; i < self.transform.childCount; ++i)
                self.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
        }
    }
}
