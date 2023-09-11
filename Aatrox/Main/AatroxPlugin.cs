using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using R2API;
using R2API.Utils;
using EntityStates;
using EntityStates.Aatrox;
using EntityStates.Boris;
using RoR2;
using RoR2.Orbs;
using RoR2.Skills;
using RoR2.Projectile;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using MonoMod.Cil;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;
using EmotesAPI;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace Aatrox
{
    [BepInDependency("com.bepis.r2api", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(MODUID, "Aatrox", "4.3.4")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SoundAPI), nameof(LanguageAPI), nameof(LoadoutAPI), nameof(DamageAPI), nameof(RecalculateStatsAPI))]

    public class AatroxPlugin : BaseUnityPlugin
    {
        internal static AatroxPlugin instance;

        public const string MODUID = "com.rob.Aatrox";

        public static int devilTriggerActive = 0;

        public static float baseHealAmount = 0.05f;

        public static int borisPrimaryCost = 10;
        public static int borisSecondaryCost = 4;//30
        public static int borisUtilityCost = 10;

        public static GameObject aatroxPrefab;
        public static GameObject characterDisplay;
        public GameObject doppelganger;

        public static GameObject borisPrefab;
        public static GameObject infiniteBorisPrefab;
        public GameObject borisDisplay;
        public GameObject borisDoppelganger;

        public static GameObject gunCrosshair;
        public static GameObject needlerCrosshair;

        public static SkillDef massacreDef;
        public static SkillDef massacreCancelDef;

        public static SkillDef worldEnderDef;
        public static SkillDef worldEnderInactiveDef;

        public static GameObject bladeProjectile;
        public static GameObject blastProjectile;
        public static GameObject luceProjectile;

        public static BuffDef massacreBuff;
        public static BuffDef worldEnderBuff;

        //public static CustomSpriteProvider masteryUnlockProvider;
        //public static CustomSpriteProvider superUnlockProvider;

        public static string aatroxLore = "\n''Some fight for honor, some fight for glory. It matters only that you fight.''\n\nAatrox is a legendary warrior, one of only five that remain of an ancient race known as the Darkin. He wields his massive blade with grace and poise, slicing through legions in a style that is hypnotic to behold. With each foe felled, Aatrox's seemingly living blade drinks in their blood, empowering him and fueling his brutal, elegant campaign of slaughter.";
        public static string aatroxEnding = "..and so he left, war still raging within.";

        public static string borisLore = "\n''I must destroy even hope...''\n\nOnce honored defenders against the Void, Aatrox and his brethren would eventually become an even greater threat to the world, and were defeated only by cunning mortal sorcery. But after centuries of imprisonment, Aatrox was the first to find freedom once more, corrupting and transforming those foolish enough to try and wield the magical weapon that contained his essence. Now, with stolen flesh, he walks the land in a brutal approximation of his previous form, seeking an apocalyptic and long overdue vengeance .";
        public static string borisEnding = "..and so he left, leaving nothing but carnage in his wake.";

        public static readonly Color aatroxColor = new Color(0.86f, 0.07f, 0.23f);

        public static Sprite mainSkinIcon { get; set; }
        public static Sprite justicarSkinIcon { get; set; }
        public static Sprite goldSkinIcon { get; set; }
        public static Sprite superSkinIcon { get; set; }

        //public static Aatrox.ServerChangeScene origServerChangeScene;
        //public static Aatrox.ClientChangeScene origClientChangeScene;
        //public static Hook OnServerChangeSceneHook;
        //public static Hook OnClientChangeSceneHook;

        private static Dictionary<string, GameObject> itemDisplayPrefabs = new Dictionary<string, GameObject>();

        public static ConfigEntry<bool> buryTheLight;
        public static ConfigEntry<bool> devilTrigger;
        public static ConfigEntry<bool> boris;
        public static ConfigEntry<bool> wideAatrox;
        public static ConfigEntry<bool> bigHead;
        public static ConfigEntry<bool> styleUI;
        public static ConfigEntry<bool> enableItemDisplays;
        public static ConfigEntry<bool> classicSkins;
        public static ConfigEntry<bool> enableEmoteAPI;

        public static ConfigEntry<float> styleMultiplier;

        public static ConfigEntry<KeyCode> danceKeybind;
        public static ConfigEntry<KeyCode> swapKeybind;
        public static ConfigEntry<KeyCode> swordmasterBackKeybind;
        public static ConfigEntry<KeyCode> swordmasterForwardKeybind;

        private void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };

            instance = this;

            ReadConfig();
            Assets.PopulateAssets();
            RegisterStates();
            Unlockables.RegisterUnlockables();
            CreateDisplayPrefab();
            CreateAatroxPrefab();
            CreateBorisPrefab();
            RegisterBuff();
            RegisterAatrox();
            RegisterBoris();

            Skins.RegisterSkins();
            BorisSkins.RegisterSkins();

            CreateCrosshair();

            if (enableItemDisplays.Value)
            {
                ItemDisplays.RegisterDisplays();
                ItemDisplays.PopulateDisplays();
            }


            //AkSoundEngine.SetRTPCValue("Devil_Volume", 100);
            AkSoundEngine.SetState("Ranking", "F");

            Hooks.RegisterHooks();

            if (ModCompat.EmoteAPIEnabled && enableEmoteAPI.Value)
            {
                ModCompat.AddEmoteSupport();
            }
            if (ModCompat.SkillsPlusPlusEnabled)
            {
                ModCompat.AddSkillsSupport();
            }
        }

        private void ReadConfig()
        {
            buryTheLight = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Bury The Light"), true, new ConfigDescription("Play Bury The Light when Massacre is active", null, Array.Empty<object>()));
            devilTrigger = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Devil Trigger"), false, new ConfigDescription("Changes Massacre into Devil Trigger", null, Array.Empty<object>()));
            boris = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "World Ender"), false, new ConfigDescription("Makes World Ender selectable as a standalone character", null, Array.Empty<object>()));
            wideAatrox = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Wide"), false, new ConfigDescription("Wide Aatrox", null, Array.Empty<object>()));
            bigHead = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Big Head"), false, new ConfigDescription("Makes Aatrox's head really big", null, Array.Empty<object>()));
            styleUI = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Style Rank"), true, new ConfigDescription("Enables the DMC style ranking system", null, Array.Empty<object>()));
            enableItemDisplays = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Enable Item Displays"), true, new ConfigDescription("Enables item displays for Aatrox", null, Array.Empty<object>()));
            classicSkins = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Classic Skins"), false, new ConfigDescription("Reverts the skins to their original, more detailed textures", null, Array.Empty<object>()));
            enableEmoteAPI = base.Config.Bind<bool>(new ConfigDefinition("01 - General Settings", "Emote API Support"), false, new ConfigDescription("Enables CustomEmotesAPI Support, but it does break the animations.", null, Array.Empty<object>()));

            styleMultiplier = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Style Multiplier"), 1f, new ConfigDescription("Multiplies the amount of style you get from everything", null, Array.Empty<object>()));

            danceKeybind = base.Config.Bind<KeyCode>(new ConfigDefinition("02 - Keybinds", "Dance"), KeyCode.Z, new ConfigDescription("Dance key", null, Array.Empty<object>()));
            swapKeybind = base.Config.Bind<KeyCode>(new ConfigDefinition("02 - Keybinds", "Weapon Swap"), KeyCode.F, new ConfigDescription("Weapon swap key", null, Array.Empty<object>()));
            swordmasterForwardKeybind = base.Config.Bind<KeyCode>(new ConfigDefinition("02 - Keybinds", "Forward"), KeyCode.W, new ConfigDescription("Whatever key you use to move forward(for Blood Price directional inputs)", null, Array.Empty<object>()));
            swordmasterBackKeybind = base.Config.Bind<KeyCode>(new ConfigDefinition("02 - Keybinds", "Back"), KeyCode.S, new ConfigDescription("Whatever key you use to move back(for Blood Price directional inputs)", null, Array.Empty<object>()));


            if (devilTrigger.Value)
            {
                IL.RoR2.MusicController.LateUpdate += il =>
                {
                    var cursor = new ILCursor(il);

                    cursor.GotoNext(i => i.MatchStloc(out _));
                    cursor.EmitDelegate<Func<bool, bool>>(b =>
                    {
                        if (b)
                            return true;

                        return devilTriggerActive != 0;
                    });
                };
            }
        }

        private void RegisterSkillModifiers()
        {
            //SkillsPlusPlus.SkillModifierManager.LoadSkillModifiers();
        }

        private void CreateCrosshair()
        {
            gunCrosshair = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/LoaderCrosshair"), "AatroxGunCrosshair", true);
            gunCrosshair.AddComponent<NetworkIdentity>();
            Destroy(gunCrosshair.GetComponent<LoaderHookCrosshairController>());

            var control = gunCrosshair.GetComponent<CrosshairController>();

            control.maxSpreadAlpha = 0;
            control.maxSpreadAngle = 3;
            control.minSpreadAlpha = 0;
            control.spriteSpreadPositions = new CrosshairController.SpritePosition[]
            {
                new CrosshairController.SpritePosition
                {
                    target = gunCrosshair.transform.GetChild(2).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(-14f, 0, 0),
                    onePosition = new Vector3(-48f, 0, 0)
                },
                new CrosshairController.SpritePosition
                {
                    target = gunCrosshair.transform.GetChild(3).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(14f, 0, 0),
                    onePosition = new Vector3(48f, 0, 0)
                }
            };

            Destroy(gunCrosshair.transform.GetChild(0).gameObject);
            Destroy(gunCrosshair.transform.GetChild(1).gameObject);

            needlerCrosshair = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/LoaderCrosshair"), "AatroxNeedlerCrosshair", true);
            needlerCrosshair.AddComponent<NetworkIdentity>();
            Destroy(needlerCrosshair.GetComponent<LoaderHookCrosshairController>());

            needlerCrosshair.GetComponent<RawImage>().enabled = false;

            control = needlerCrosshair.GetComponent<CrosshairController>();

            control.maxSpreadAlpha = 0;
            control.maxSpreadAngle = 3;
            control.minSpreadAlpha = 0;
            control.spriteSpreadPositions = new CrosshairController.SpritePosition[]
            {
                new CrosshairController.SpritePosition
                {
                    target = needlerCrosshair.transform.GetChild(2).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(-20f, 0, 0),
                    onePosition = new Vector3(-48f, 0, 0)
                },
                new CrosshairController.SpritePosition
                {
                    target = needlerCrosshair.transform.GetChild(3).GetComponent<RectTransform>(),
                    zeroPosition = new Vector3(20f, 0, 0),
                    onePosition = new Vector3(48f, 0, 0)
                }
            };

            Destroy(needlerCrosshair.transform.GetChild(0).gameObject);
            Destroy(needlerCrosshair.transform.GetChild(1).gameObject);
        }

        private void RegisterBuff()
        {
            massacreBuff = ScriptableObject.CreateInstance<BuffDef>();
            {
                massacreBuff.buffColor = aatroxColor;
                massacreBuff.canStack = true;
                massacreBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texAttackIcon.png").WaitForCompletion();
                massacreBuff.isDebuff = false;
                massacreBuff.name = "AatroxMassacreBuff";
            };
            ContentAddition.AddBuffDef(massacreBuff);

            worldEnderBuff = ScriptableObject.CreateInstance<BuffDef>();
            {
                worldEnderBuff.buffColor = Color.red;
                worldEnderBuff.canStack = false;
                worldEnderBuff.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texAttackIcon.png").WaitForCompletion();
                worldEnderBuff.isDebuff = false;
                worldEnderBuff.name = "AatroxWorldEnderBuff";
            };
            ContentAddition.AddBuffDef(worldEnderBuff);
        }
        
        private static GameObject CreateModel(GameObject main, int modelIndex)
        {
            Destroy(main.transform.Find("ModelBase").gameObject);
            Destroy(main.transform.Find("CameraPivot").gameObject);
            Destroy(main.transform.Find("AimOrigin").gameObject);

            GameObject model = null;

            if (modelIndex == 0) model = Assets.mainAssetBundle.LoadAsset<GameObject>("mdlAatrox");
            else if (modelIndex == 1) model = Assets.secondaryAssetBundle.LoadAsset<GameObject>("mdlBoris");
            else if (modelIndex == 2) model = Assets.mainAssetBundle.LoadAsset<GameObject>("AatroxDisplay");

            return model;
        }

        internal static void CreateDisplayPrefab()
        {
            GameObject tempDisplay = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody"), "AatroxDisplay");

            GameObject model = CreateModel(tempDisplay, 2);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = tempDisplay.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;

            ModelLocator modelLocator = tempDisplay.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = null;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            characterModel.SetFieldValue("mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());

            var ragdoll = tempDisplay.GetComponentInChildren<RagdollController>();
            if (ragdoll) DestroyImmediate(ragdoll);

            foreach (CharacterJoint i in tempDisplay.GetComponentsInChildren<CharacterJoint>())
            {
                if (i) Destroy(i);
            }

            foreach (Collider i in tempDisplay.GetComponentsInChildren<Collider>())
            {
                if (i)
                {
                    i.enabled = false;
                    Destroy(i);
                }
            }

            foreach (Rigidbody i in tempDisplay.GetComponentsInChildren<Rigidbody>())
            {
                if (i)
                {
                    i.isKinematic = true;
                    i.useGravity = false;
                    i.freezeRotation = true;
                    Destroy(i);
                }
            }

            characterDisplay = PrefabAPI.InstantiateClone(tempDisplay.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "AatroxDisplay", true);
        }

        internal static void CreateAatroxPrefab()
        {
            aatroxPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody"), "AatroxBody", true);

            aatroxPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            GameObject model = CreateModel(aatroxPrefab, 0);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = aatroxPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = aatroxPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = aatroxPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = "AatroxBody";
            bodyComponent.baseNameToken = "AATROX_NAME";
            bodyComponent.subtitleNameToken = "AATROX_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 100;
            bodyComponent.levelMaxHealth = 30;
            bodyComponent.baseRegen = 0.25f;
            bodyComponent.levelRegen = 0.75f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 80;
            bodyComponent.baseJumpPower = 15;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 11;
            bodyComponent.levelDamage = 2.2f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20;
            bodyComponent.levelArmor = 0;
            bodyComponent.baseJumpCount = 2;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.spreadBloomDecayTime = 1;
            bodyComponent.spreadBloomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            bodyComponent._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.charPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CharacterBody>().preferredPodPrefab;
            bodyComponent.preferredInitialStateType = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CharacterBody>().preferredInitialStateType;
            bodyComponent.skinIndex = 0U;
            bodyComponent.bodyColor = aatroxColor;

            CharacterMotor characterMotor = aatroxPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 100f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            InputBankTest inputBankTest = aatroxPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = aatroxPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = aatroxPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            ChildLocator childLocator = model.GetComponentInChildren<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().material,
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GauntletL").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GauntletL").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = childLocator.FindChild("GauntletR").GetComponentInChildren<MeshRenderer>().material,
                    renderer = childLocator.FindChild("GauntletR").GetComponentInChildren<MeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            Material material = null;
            for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
            {
                material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
                material.SetColor("_Color", Color.white);
                material.SetTexture("_MainTex", Assets.mainMat.GetTexture("_MainTex"));
                material.SetColor("_EmColor", Color.white);
                material.SetFloat("_EmPower", 5);
                material.SetTexture("_EmTex", Assets.mainMat.GetTexture("_EmissionMap"));
                material.SetFloat("_NormalStrength", 0.5f);
                material.SetTexture("_NormalTex", Assets.mainMat.GetTexture("_BumpMap"));

                characterModel.baseRendererInfos[i].defaultMaterial = material;
            }

            characterModel.SetFieldValue("mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());

            TeamComponent teamComponent = null;
            if (aatroxPrefab.GetComponent<TeamComponent>() != null) teamComponent = aatroxPrefab.GetComponent<TeamComponent>();
            else teamComponent = aatroxPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            HealthComponent healthComponent = aatroxPrefab.GetComponent<HealthComponent>();
            healthComponent.health = 120f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            aatroxPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            aatroxPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            CharacterDeathBehavior characterDeathBehavior = aatroxPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = aatroxPrefab.GetComponent<EntityStateMachine>();
            //characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            EntityStateMachine entityStateMachine = aatroxPrefab.GetComponent<EntityStateMachine>();
            entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(AatroxMain));
            entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(AatroxSpawnState));

            SfxLocator sfxLocator = aatroxPrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_ui_player_death";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_char_land";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = aatroxPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = aatroxPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = aatroxPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.center = new Vector3(0, 0, 0);
            capsuleCollider.material = null;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox componentInChildren = model.transform.Find("TempHurtbox").GetComponent<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = healthComponent;
            componentInChildren.isBullseye = true;
            componentInChildren.damageModifier = HurtBox.DamageModifier.Normal;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                componentInChildren
            };

            hurtBoxGroup.mainHurtBox = componentInChildren;
            hurtBoxGroup.bullseyeCount = 1;

            HitBox hitBox1 = childLocator.FindChild("SwordHitbox").gameObject.AddComponent<HitBox>();
            hitBox1.gameObject.layer = LayerIndex.projectile.intVal;
            hitBox1.transform.localScale = Vector3.one * 600f;

            HitBox hitBox2 = childLocator.FindChild("SwordBigHitbox").gameObject.AddComponent<HitBox>();
            hitBox2.gameObject.layer = LayerIndex.projectile.intVal;
            hitBox2.transform.localScale = Vector3.one * 900f;

            HitBox hitBox3 = childLocator.FindChild("ScytheHitboxBase").gameObject.AddComponent<HitBox>();
            hitBox3.gameObject.layer = LayerIndex.projectile.intVal;
            hitBox3.transform.localScale = Vector3.one * 800f;

            HitBox hitBox4 = childLocator.FindChild("ScytheHitboxBlade").gameObject.AddComponent<HitBox>();
            hitBox4.gameObject.layer = LayerIndex.projectile.intVal;
            hitBox4.transform.localScale = Vector3.one * 500f;

            HitBoxGroup hitBoxGroup1 = model.AddComponent<HitBoxGroup>();
            hitBoxGroup1.hitBoxes = new HitBox[]
            {
                hitBox1
            };
            hitBoxGroup1.groupName = "Sword";

            HitBoxGroup hitBoxGroup2 = model.AddComponent<HitBoxGroup>();
            hitBoxGroup2.hitBoxes = new HitBox[]
            {
                hitBox2
            };
            hitBoxGroup2.groupName = "SwordBig";

            HitBoxGroup hitBoxGroup3 = model.AddComponent<HitBoxGroup>();
            hitBoxGroup3.hitBoxes = new HitBox[]
            {
                hitBox3,
                hitBox4
            };
            hitBoxGroup3.groupName = "Scythe";

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");

            RagdollController ragdollController = model.GetComponent<RagdollController>();

            PhysicMaterial physicMat = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<RagdollController>().bones[1].GetComponent<Collider>().material;

            foreach (Transform i in ragdollController.bones)
            {
                if (i)
                {
                    i.gameObject.layer = LayerIndex.ragdoll.intVal;
                    Collider j = i.GetComponent<Collider>();
                    if (j)
                    {
                        j.material = physicMat;
                        j.sharedMaterial = physicMat;
                    }
                }
            }

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -80f;
            aimAnimator.yawRangeMax = 80f;
            aimAnimator.pitchGiveupRange = 40f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 1f;

            var newStateMachine = aatroxPrefab.AddComponent<EntityStateMachine>();
            newStateMachine.customName = "Dash";
            newStateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));
            newStateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.Idle));

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            var list = networkStateMachine.stateMachines.ToList();
            list.Add(newStateMachine);
            networkStateMachine.stateMachines = list.ToArray();
        }

        internal static void CreateBorisPrefab()
        {
            borisPrefab = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody"), "BorisBody", true);

            borisPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            GameObject model = CreateModel(borisPrefab, 1);

            GameObject gameObject = new GameObject("ModelBase");
            gameObject.transform.parent = borisPrefab.transform;
            gameObject.transform.localPosition = new Vector3(0f, -0.81f, 0f);
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject2 = new GameObject("CameraPivot");
            gameObject2.transform.parent = gameObject.transform;
            gameObject2.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            gameObject2.transform.localRotation = Quaternion.identity;
            gameObject2.transform.localScale = Vector3.one;

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = gameObject.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = gameObject.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = borisPrefab.GetComponent<CharacterDirection>();
            characterDirection.moveVector = Vector3.zero;
            characterDirection.targetTransform = gameObject.transform;
            characterDirection.overrideAnimatorForwardTransform = null;
            characterDirection.rootMotionAccumulator = null;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.driveFromRootRotation = false;
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = borisPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = "BorisBody";
            bodyComponent.baseNameToken = "BORIS_NAME";
            bodyComponent.subtitleNameToken = "BORIS_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 100;
            bodyComponent.levelMaxHealth = 30;
            bodyComponent.baseRegen = 5;
            bodyComponent.levelRegen = 1.5f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 80;
            bodyComponent.baseJumpPower = 20;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 12;
            bodyComponent.levelDamage = 2.4f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 20;
            bodyComponent.levelArmor = 0;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.55f;
            bodyComponent.wasLucky = false;
            bodyComponent.spreadBloomDecayTime = 1;
            bodyComponent.spreadBloomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            bodyComponent._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
            bodyComponent.hideCrosshair = true;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.borisPortrait;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CharacterBody>().preferredPodPrefab;
            bodyComponent.preferredInitialStateType = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/MercBody").GetComponent<CharacterBody>().preferredInitialStateType;
            bodyComponent.skinIndex = 0U;

            CharacterMotor characterMotor = borisPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 100f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            InputBankTest inputBankTest = borisPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = borisPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = borisPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = gameObject.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            CharacterModel characterModel = model.AddComponent<CharacterModel>();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[0];
            /*{
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().materials[0],
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().materials[1],
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                },
                new CharacterModel.RendererInfo
                {
                    defaultMaterial = model.GetComponentInChildren<SkinnedMeshRenderer>().materials[2],
                    renderer = model.GetComponentInChildren<SkinnedMeshRenderer>(),
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };*/

            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();

            /*Material material = null;
            for (int i = 0; i < characterModel.baseRendererInfos.Length; i++)
            {
                string matString = "matBorisBody";
                if (i == 1) matString = "matBorisSword";
                if (i == 2) matString = "matBorisWings";

                material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
                material.SetColor("_Color", Color.white);
                material.SetTexture("_MainTex", Assets.secondaryAssetBundle.LoadAsset<Material>(matString).GetTexture("_MainTex"));
                material.SetColor("_EmColor", Color.white);
                material.SetFloat("_EmPower", 0);
                material.SetTexture("_EmTex", Assets.secondaryAssetBundle.LoadAsset<Material>(matString).GetTexture("_EmissionMap"));
                material.SetFloat("_NormalStrength", 0);

                characterModel.baseRendererInfos[i].defaultMaterial = material;
            }

            characterModel.SetFieldValue("mainSkinnedMeshRenderer", characterModel.baseRendererInfos[0].renderer.gameObject.GetComponent<SkinnedMeshRenderer>());*/

            TeamComponent teamComponent = null;
            if (borisPrefab.GetComponent<TeamComponent>() != null) teamComponent = borisPrefab.GetComponent<TeamComponent>();
            else teamComponent = borisPrefab.GetComponent<TeamComponent>();
            teamComponent.hideAllyCardDisplay = false;
            teamComponent.teamIndex = TeamIndex.None;

            HealthComponent healthComponent = borisPrefab.GetComponent<HealthComponent>();
            healthComponent.health = 120f;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;
            healthComponent.magnetiCharge = 0f;
            healthComponent.body = null;
            healthComponent.dontShowHealthbar = false;
            healthComponent.globalDeathEventChanceCoefficient = 1f;

            borisPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            borisPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            CharacterDeathBehavior characterDeathBehavior = borisPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = borisPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(GenericCharacterDeath));

            EntityStateMachine entityStateMachine = borisPrefab.GetComponent<EntityStateMachine>();
            entityStateMachine.mainStateType = new SerializableEntityStateType(typeof(BorisMain));
            entityStateMachine.initialStateType = new SerializableEntityStateType(typeof(BorisSpawnState));

            SfxLocator sfxLocator = borisPrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_ui_player_death";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "";
            sfxLocator.fallDamageSound = "Play_char_land_fall_damage";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            Rigidbody rigidbody = borisPrefab.GetComponent<Rigidbody>();
            rigidbody.mass = 100f;
            rigidbody.drag = 0f;
            rigidbody.angularDrag = 0f;
            rigidbody.useGravity = false;
            rigidbody.isKinematic = true;
            rigidbody.interpolation = RigidbodyInterpolation.None;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rigidbody.constraints = RigidbodyConstraints.None;

            CapsuleCollider capsuleCollider = borisPrefab.GetComponent<CapsuleCollider>();
            capsuleCollider.isTrigger = false;
            capsuleCollider.material = null;
            capsuleCollider.center = new Vector3(0f, 0f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 1.82f;
            capsuleCollider.direction = 1;

            KinematicCharacterMotor kinematicCharacterMotor = borisPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;
            kinematicCharacterMotor.Capsule = capsuleCollider;
            kinematicCharacterMotor.Rigidbody = rigidbody;

            kinematicCharacterMotor.DetectDiscreteCollisions = false;
            kinematicCharacterMotor.GroundDetectionExtraDistance = 0f;
            kinematicCharacterMotor.MaxStepHeight = 0.2f;
            kinematicCharacterMotor.MinRequiredStepDepth = 0.1f;
            kinematicCharacterMotor.MaxStableSlopeAngle = 55f;
            kinematicCharacterMotor.MaxStableDistanceFromLedge = 0.5f;
            kinematicCharacterMotor.PreventSnappingOnLedges = false;
            kinematicCharacterMotor.MaxStableDenivelationAngle = 55f;
            kinematicCharacterMotor.RigidbodyInteractionType = RigidbodyInteractionType.None;
            kinematicCharacterMotor.PreserveAttachedRigidbodyMomentum = true;
            kinematicCharacterMotor.HasPlanarConstraint = false;
            kinematicCharacterMotor.PlanarConstraintAxis = Vector3.up;
            kinematicCharacterMotor.StepHandling = StepHandlingMethod.None;
            kinematicCharacterMotor.LedgeHandling = true;
            kinematicCharacterMotor.InteractiveRigidbodyHandling = true;
            kinematicCharacterMotor.SafeMovement = false;

            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();

            HurtBox componentInChildren = model.GetComponentInChildren<CapsuleCollider>().gameObject.AddComponent<HurtBox>();
            componentInChildren.gameObject.layer = LayerIndex.entityPrecise.intVal;
            componentInChildren.healthComponent = healthComponent;
            componentInChildren.isBullseye = true;
            componentInChildren.damageModifier = HurtBox.DamageModifier.Normal;
            componentInChildren.hurtBoxGroup = hurtBoxGroup;
            componentInChildren.indexInGroup = 0;

            hurtBoxGroup.hurtBoxes = new HurtBox[]
            {
                componentInChildren
            };

            hurtBoxGroup.mainHurtBox = componentInChildren;
            hurtBoxGroup.bullseyeCount = 1;

            HitBoxGroup hitBoxGroup = model.AddComponent<HitBoxGroup>();

            GameObject hitboxObj = new GameObject("Sword");
            hitboxObj.transform.parent = model.GetComponent<ChildLocator>().FindChild("Weapon_Tip");
            hitboxObj.transform.localPosition = Vector3.zero;
            hitboxObj.transform.rotation = Quaternion.Euler(Vector3.zero);
            hitboxObj.transform.localScale *= 12f;

            HitBox hitBox = hitboxObj.AddComponent<HitBox>();
            hitBox.gameObject.layer = LayerIndex.projectile.intVal;

            hitBoxGroup.hitBoxes = new HitBox[]
            {
                hitBox
            };

            hitBoxGroup.groupName = "Sword";

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_player_footstep";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");

            RagdollController ragdollController = model.AddComponent<RagdollController>();
            ragdollController.bones = null;
            ragdollController.componentsToDisableOnRagdoll = null;

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMax = 55f;
            aimAnimator.pitchRangeMin = -50f;
            aimAnimator.yawRangeMin = -44f;
            aimAnimator.yawRangeMax = 44f;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 8f;
        }

        private void FindComponents(GameObject obj)
        {
            //for finding all components on an object
            if (obj)
            {
                Debug.Log("Listing components on " + obj.name);
                foreach (Component i in obj.GetComponentsInChildren<Component>())
                {
                    if (i) Debug.Log(i.gameObject.name + " has component " + i.GetType().Name);
                }
            }
        }

        private void RegisterBoris()
        {
            borisPrefab.AddComponent<BorisController>();

            borisDisplay = PrefabAPI.InstantiateClone(borisPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "BorisDisplay", true);

            borisDisplay.AddComponent<NetworkIdentity>();
            borisDisplay.AddComponent<BorisMenuAnim>();

            blastProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/BeamSphere"), "BorisBlastProjectile", true);

            blastProjectile.GetComponent<ProjectileSimple>().desiredForwardSpeed = 120f;
            blastProjectile.GetComponent<ProjectileController>().procCoefficient = 1f;
            blastProjectile.GetComponent<ProjectileDamage>().damage = 1f;
            blastProjectile.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 1f;
            var beamC = blastProjectile.GetComponent<ProjectileProximityBeamController>();
            beamC.attackRange = 12f;
            beamC.damageCoefficient = 0.2f;
            beamC.lightningType = LightningOrb.LightningType.RazorWire;
            beamC.procCoefficient = 0.2f;

            luceProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/ArtifactShellSeekingSolarFlare"), "BorisLuceProjectile", true);

            luceProjectile.GetComponent<ProjectileController>().procCoefficient = 0.8f;
            luceProjectile.GetComponent<ProjectileController>().startSound = Sounds.BorisFireBlast;

            luceProjectile.GetComponent<ProjectileDirectionalTargetFinder>().lookCone = 360f;
            luceProjectile.GetComponent<ProjectileSteerTowardTarget>().rotationSpeed = 125f;
            luceProjectile.GetComponent<SphereCollider>().radius = 1.5f;

            var explosion = luceProjectile.GetComponent<ProjectileSingleTargetImpact>();
            if (explosion)
            {
                explosion.enemyHitSoundString = Sounds.BorisBlastImpact;
                explosion.hitSoundString = Sounds.BorisBlastImpact;
            }
            /*var impactExplosion = luceProjectile.GetComponent<ProjectileImpactExplosion>()
            if (luceProjectile.GetComponent<ProjectileImpactExplosion>()) luceProjectile.GetComponent<ProjectileImpactExplosion>().blastRadius *= 3;*/
            //luceProjectile.GetComponent<ProjectileController>().startSound = "";


            /*GameObject blastGhostPrefab = PrefabAPI.InstantiateClone(blastProjectile.GetComponent<ProjectileController>().ghostPrefab, "AatroxBlastGhost");

            foreach(ParticleSystem i in blastGhostPrefab.GetComponentsInChildren<ParticleSystem>())
            {
                if (i) i.startColor = Color.red;
            }

            foreach(Light i in blastGhostPrefab.GetComponentsInChildren<Light>())
            {
                if (i) i.color = Color.red;
            }*/

            GameObject blastImpactPrefab = PrefabAPI.InstantiateClone(blastProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect, "AatroxBlastImpact");

            foreach (ParticleSystem i in blastImpactPrefab.GetComponentsInChildren<ParticleSystem>())
            {
                if (i) i.startColor = Color.red;
                if (i && i.gameObject.name == "Ring") Destroy(i);
            }

            foreach (Light i in blastImpactPrefab.GetComponentsInChildren<Light>())
            {
                if (i) i.color = Color.red;
            }

            /*var pullComponent = blastImpactPrefab.AddComponent<PullNearby>();
            pullComponent.maximumPullCount = 64;
            pullComponent.pullDuration = 0.2f;
            pullComponent.pullOnStart = true;
            pullComponent.pullRadius = 32f;
            pullComponent.pullStrengthCurve = AnimationCurve.Linear(0f, 500f, 0.2f, 0f);*/

            //FindComponents(blastImpactPrefab);

            GameObject blastModel = Assets.blastModel.InstantiateClone("AatroxBlastModel", true);
            blastModel.AddComponent<NetworkIdentity>();
            blastModel.AddComponent<ProjectileGhostController>();

            blastModel.transform.localScale *= 1.25f;

            blastProjectile.GetComponent<ProjectileController>().ghostPrefab = blastModel;
            blastProjectile.GetComponent<ProjectileImpactExplosion>().impactEffect = blastImpactPrefab;
            blastProjectile.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.None;

            luceProjectile.GetComponent<ProjectileController>().ghostPrefab = blastModel;

            blastImpactPrefab.AddComponent<NetworkIdentity>();

            if (borisPrefab) PrefabAPI.RegisterNetworkPrefab(borisPrefab);
            if (borisDisplay) PrefabAPI.RegisterNetworkPrefab(borisDisplay);
            if (blastProjectile) PrefabAPI.RegisterNetworkPrefab(blastProjectile);
            if (luceProjectile) PrefabAPI.RegisterNetworkPrefab(luceProjectile);

            ContentAddition.AddProjectile(blastProjectile);
            ContentAddition.AddProjectile(luceProjectile);

            ContentAddition.AddEffect(blastImpactPrefab);

            BorisSkillSetup();

            ContentAddition.AddBody(borisPrefab);

            CreateBorisMaster();

            if (boris.Value)
            {
                infiniteBorisPrefab = PrefabAPI.InstantiateClone(borisPrefab, "InfiniteBorisBody", true);
                infiniteBorisPrefab.GetComponent<BorisController>().infiniteSDT = true;

                string desc = "The World Ender, Aatrox's final form, is a powerful juggernaut who uses his massive sword to cleave through his foes.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
                desc = desc + "< ! > The Darkin Blade hits harder the more attack speed you have." + Environment.NewLine + Environment.NewLine;
                desc = desc + "< ! > Eldritch Bolts are great for taking care of swarms." + Environment.NewLine + Environment.NewLine;
                desc = desc + "< ! > Umbral Dash is a versatile skill that can be used both offensively and defensively." + Environment.NewLine + Environment.NewLine;
                desc = desc + "< ! > Try and save Judgement for when your transformation is about to end to maximize damage output.</color>" + Environment.NewLine + Environment.NewLine;

                LanguageAPI.Add("BORIS_NAME", "World Ender");
                LanguageAPI.Add("BORIS_DESCRIPTION", desc);
                LanguageAPI.Add("BORIS_SUBTITLE", "Frenzied Berserker");
                LanguageAPI.Add("BORIS_LORE", borisLore);
                LanguageAPI.Add("BORIS_OUTRO_FLAVOR", borisEnding);

                SurvivorDef survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
                {
                    survivorDef.cachedName = "BORIS_NAME";
                    survivorDef.unlockableDef = null;
                    survivorDef.descriptionToken = "BORIS_DESCRIPTION";
                    survivorDef.primaryColor = aatroxColor;
                    survivorDef.bodyPrefab = borisPrefab;
                    survivorDef.displayPrefab = borisDisplay;
                    survivorDef.outroFlavorToken = "BORIS_OUTRO_FLAVOR";
                };

                ContentAddition.AddSurvivorDef(survivorDef);
            }
        }

        private void RegisterAatrox()
        {
            characterDisplay.AddComponent<NetworkIdentity>();
            characterDisplay.AddComponent<MenuAnim>();

            aatroxPrefab.AddComponent<AatroxController>();

            bladeProjectile = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/FMJ"), "Prefabs/Projectiles/AatroxBladeProjectile", true);

            GameObject bladeModel = Assets.projectileModel.InstantiateClone("BladeProjectileModel", true);
            bladeModel.AddComponent<NetworkIdentity>();
            bladeModel.AddComponent<ProjectileGhostController>();

            bladeProjectile.transform.localScale *= 2;
            bladeModel.transform.GetChild(0).localScale *= 1.75f;

            bladeProjectile.GetComponent<ProjectileController>().ghostPrefab = bladeModel;

            if (aatroxPrefab) PrefabAPI.RegisterNetworkPrefab(aatroxPrefab);
            if (characterDisplay) PrefabAPI.RegisterNetworkPrefab(characterDisplay);
            if (bladeProjectile) PrefabAPI.RegisterNetworkPrefab(bladeProjectile);
            if (bladeModel) PrefabAPI.RegisterNetworkPrefab(bladeModel);

            ContentAddition.AddProjectile(bladeProjectile);


            string desc = "Aatrox is a risky melee survivor who spends his health to inflict heavy damage.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Using Blood Thirst on weak enemies is a good way to stay healthy without putting yourself at risk." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Use Blood Price and Blood Thirst in conjunction to dish out heavy damage while staying healthy." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Blood Price performs different attacks if you're moving either forward or backward." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Massacre gets stronger the longer it's active, however the risk increases as well.</color>" + Environment.NewLine + Environment.NewLine;

            LanguageAPI.Add("AATROX_NAME", "Aatrox");
            LanguageAPI.Add("AATROX_DESCRIPTION", desc);
            LanguageAPI.Add("AATROX_SUBTITLE", "The Darkin Blade");
            LanguageAPI.Add("AATROX_LORE", aatroxLore);
            LanguageAPI.Add("AATROX_OUTRO_FLAVOR", aatroxEnding);

            SurvivorDef survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = "AATROX_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = "AATROX_DESCRIPTION";
                survivorDef.primaryColor = aatroxColor;
                survivorDef.bodyPrefab = aatroxPrefab;
                survivorDef.displayPrefab = characterDisplay;
                survivorDef.outroFlavorToken = "AATROX_OUTRO_FLAVOR";
            };

            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();

            ContentAddition.AddBody(aatroxPrefab);

            CreateMaster();

            aatroxPrefab.tag = "Player";
        }
        void BorisSkillSetup()
        {
            foreach (GenericSkill obj in borisPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            BorisSkillDefs.BorisPassiveSetup();
            BorisSkillDefs.BorisPrimarySetup();
            BorisSkillDefs.BorisSecondarySetup();
            BorisSkillDefs.BorisUtilitySetup();
            BorisSkillDefs.BorisSpecialSetup();
        }
        void SkillSetup()
        {
            foreach (GenericSkill obj in aatroxPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }

            AatroxSkillDefs.PassiveSetup();
            AatroxSkillDefs.PrimarySetup();
            AatroxSkillDefs.SecondarySetup();
            AatroxSkillDefs.UtilitySetup();
            AatroxSkillDefs.SpecialSetup();
        }
        void RegisterStates()
        {
            bool flag;

            ContentAddition.AddEntityState<BaseEmote>(out flag);
            ContentAddition.AddEntityState<Dance>(out flag);

            ContentAddition.AddEntityState<AatroxMain>(out flag);
            ContentAddition.AddEntityState<AatroxSpawnState>(out flag);
            ContentAddition.AddEntityState<AatroxMassacre>(out flag);
            ContentAddition.AddEntityState<AatroxTransformation>(out flag);
            ContentAddition.AddEntityState<AatroxSwordStance>(out flag);
            ContentAddition.AddEntityState<AatroxLunge>(out flag);
            ContentAddition.AddEntityState<AatroxLungeFollowup>(out flag);
            ContentAddition.AddEntityState<AatroxUppercut>(out flag);
            ContentAddition.AddEntityState<AatroxDownslash>(out flag);
            ContentAddition.AddEntityState<AatroxGroundLight>(out flag);
            ContentAddition.AddEntityState<AatroxBeginFlight>(out flag);
            ContentAddition.AddEntityState<AatroxAimDive>(out flag);
            ContentAddition.AddEntityState<AatroxDive>(out flag);
            ContentAddition.AddEntityState<AatroxFireProjectile>(out flag);
            ContentAddition.AddEntityState<AatroxShadowStep>(out flag);

            ContentAddition.AddEntityState<EntityStates.Aatrox.Rhaast.RhaastGroundLight>(out flag);
            ContentAddition.AddEntityState<EntityStates.Aatrox.Rhaast.RhaastUppercut>(out flag);

            ContentAddition.AddEntityState<EntityStates.Aatrox.Gun.AatroxFireGuns>(out flag);
            ContentAddition.AddEntityState<EntityStates.Aatrox.Gun.Rainstorm>(out flag);
            ContentAddition.AddEntityState<EntityStates.Aatrox.Gun.Flurry>(out flag);
            ContentAddition.AddEntityState<EntityStates.Aatrox.Gun.GunStinger>(out flag);

            ContentAddition.AddEntityState<BorisMain>(out flag);
            ContentAddition.AddEntityState<BorisSpawnState>(out flag);
            ContentAddition.AddEntityState<BorisBlink>(out flag);
            ContentAddition.AddEntityState<BorisEndTransformation>(out flag);
            ContentAddition.AddEntityState<BorisGroundLight>(out flag);
            ContentAddition.AddEntityState<BorisSpawnState>(out flag);
            ContentAddition.AddEntityState<BorisFireProjectile>(out flag);
            ContentAddition.AddEntityState<BorisLuce>(out flag);
            ContentAddition.AddEntityState<BorisFinisher>(out flag);

        }
        private void CreateMaster()
        {
            doppelganger = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "AatroxMonsterMaster", true);
            doppelganger.GetComponent<CharacterMaster>().bodyPrefab = aatroxPrefab;

            ContentAddition.AddMaster(doppelganger);
        }
        private void CreateBorisMaster()
        {
            GameObject borisDoppelganger = PrefabAPI.InstantiateClone(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterMasters/LoaderMonsterMaster"), "BorisMonsterMaster", true);
            borisDoppelganger.GetComponent<CharacterMaster>().bodyPrefab = borisPrefab;

            ContentAddition.AddMaster(borisDoppelganger);
        }
    }
}