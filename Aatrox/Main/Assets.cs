using R2API;
using RoR2;
using RoR2.Projectile;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Aatrox
{
    public static class Assets
    {
        public static AssetBundle mainAssetBundle = null;

        public static AssetBundle secondaryAssetBundle = null;

        public static Texture charPortrait;
        public static Texture borisPortrait;

        public static Material mainMat;
        public static Material justicarMat;
        public static Material gildedMat;
        public static Material superMat;

        public static Sprite swapIcon;
        public static Sprite iconP;
        public static Sprite icon1;
        public static Sprite icon1b;
        public static Sprite icon1c;
        public static Sprite icon2;
        public static Sprite icon2b;
        public static Sprite icon3;
        public static Sprite icon3b;
        public static Sprite icon4;
        public static Sprite icon4b;
        public static Sprite icon4c;
        public static Sprite icon4d;

        public static Sprite bIconP;
        public static Sprite bIcon1;
        public static Sprite bIcon2;
        public static Sprite bIcon3;
        public static Sprite bIcon4;

        public static GameObject styleMeter;

        public static Sprite styleD;
        public static Sprite styleC;
        public static Sprite styleB;
        public static Sprite styleA;
        public static Sprite styleS;
        public static Sprite styleSS;
        public static Sprite styleSSS;

        public static Sprite styleTextD;
        public static Sprite styleTextC;
        public static Sprite styleTextB;
        public static Sprite styleTextA;
        public static Sprite styleTextS;
        public static Sprite styleTextSS;
        public static Sprite styleTextSSS;

        public static GameObject hitFX { get; set; }
        public static GameObject healFX { get; set; }
        public static GameObject critFX { get; set; }

        public static GameObject combo1FX { get; set; }
        public static GameObject combo2FX;
        public static GameObject combo3FX;

        public static GameObject downslashFX;
        public static GameObject uppercutFX;
        public static GameObject lungeFX;

        public static GameObject rhaastUppercutFX;

        public static GameObject flightFX;

        public static GameObject massacreFX;

        public static GameObject projectileModel;
        public static GameObject blastModel;

        public static GameObject borisSlash1FX;
        public static GameObject borisSlash2FX;
        public static GameObject borisSlash3FX;

        public static GameObject borisCombo1FX;
        public static GameObject borisCombo2FX;

        public static GameObject borisSpawnFX;
        public static GameObject borisDeathFX;
        public static GameObject borisJudgementFX;
        public static GameObject borisJudgementStartFX;
        public static GameObject borisExplosionFX;

        public static GameObject borisHitFX;

        public static void PopulateAssets()
        {
            if (mainAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.aatrox"))
                {
                    mainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }

            if (secondaryAssetBundle == null)
            {
                using (var assetStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.boris"))
                {
                    secondaryAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }

            using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.aatrox.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }
            using (Stream manifestResourceStream4 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.boris.bnk"))
            {
                byte[] array = new byte[manifestResourceStream4.Length];
                manifestResourceStream4.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }

            using (Stream manifestResourceStream4 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.Aatrox_Triggered.bnk"))
            {
                byte[] array = new byte[manifestResourceStream4.Length];
                manifestResourceStream4.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }

            /*

            using (Stream manifestResourceStream3 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.DevilTrigger.bnk"))
            {
                byte[] array = new byte[manifestResourceStream3.Length];
                manifestResourceStream3.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }

            using (Stream manifestResourceStream5 = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aatrox.AatroxInit.bnk"))
            {
                byte[] array = new byte[manifestResourceStream5.Length];
                manifestResourceStream5.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }*/

            if (AatroxPlugin.styleUI.Value)
            {
                Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HUDSimple.prefab").WaitForCompletion().AddComponent<Aatrox.Main.HUDAdder>();
                AkSoundEngine.SetRTPCValue("Style_Rank", 100);
            }

            AatroxPlugin.mainSkinIcon = R2API.Skins.CreateSkinIcon(new Color(0.36f, 0.16f, 0.27f), new Color(0.56f, 0.23f, 0.19f), new Color(0.14f, 0.14f, 0.19f), new Color(0.17f, 0.24f, 0.34f));
            AatroxPlugin.justicarSkinIcon = R2API.Skins.CreateSkinIcon(new Color(0.09f, 0.09f, 0.37f), new Color(0.48f, 0.23f, 0.57f), new Color(0.12f, 0.12f, 0.12f), new Color(0.19f, 0.21f, 0.36f));
            AatroxPlugin.goldSkinIcon = R2API.Skins.CreateSkinIcon(new Color(0.36f, 0.16f, 0.27f), new Color(1f, 0.37f, 0f), new Color(0.27f, 0.23f, 0.12f), new Color(0.99f, 0.99f, 0.44f));
            AatroxPlugin.superSkinIcon = R2API.Skins.CreateSkinIcon(new Color(0.43f, 0.15f, 0.25f), new Color(0.95f, 0.17f, 0.08f), new Color(0.17f, 0.23f, 0.34f), new Color(1f, 0.47f, 0.21f));

            charPortrait = mainAssetBundle.LoadAsset<Sprite>("texAatroxBody").texture;
            borisPortrait = secondaryAssetBundle.LoadAsset<Texture>("BorisBody");

            if (AatroxPlugin.classicSkins.Value)
            {
                mainMat = mainAssetBundle.LoadAsset<Material>("matAatrox");
                justicarMat = mainAssetBundle.LoadAsset<Material>("matAatroxMonsoon");
                gildedMat = mainAssetBundle.LoadAsset<Material>("matAatroxGilded");
            }
            else
            {
                mainMat = mainAssetBundle.LoadAsset<Material>("matAatroxSimple");
                justicarMat = mainAssetBundle.LoadAsset<Material>("matAatroxVergil");
                gildedMat = mainAssetBundle.LoadAsset<Material>("matAatroxPrestige");
            }
            superMat = mainAssetBundle.LoadAsset<Material>("matAatroxUltimate");

            swapIcon = mainAssetBundle.LoadAsset<Sprite>("SwapIcon");
            iconP = mainAssetBundle.LoadAsset<Sprite>("BloodWellIcon");
            icon1 = mainAssetBundle.LoadAsset<Sprite>("ThirstIcon");
            icon1b = mainAssetBundle.LoadAsset<Sprite>("RhaastIcon");
            icon1c = mainAssetBundle.LoadAsset<Sprite>("GunslingerIcon");
            icon2 = mainAssetBundle.LoadAsset<Sprite>("BladesIcon");
            icon2b = mainAssetBundle.LoadAsset<Sprite>("PriceIcon");
            icon3 = mainAssetBundle.LoadAsset<Sprite>("DarkFlightIcon");
            icon3b = mainAssetBundle.LoadAsset<Sprite>("ShadowStepIcon");
            icon4 = mainAssetBundle.LoadAsset<Sprite>("MassacreIcon");
            icon4b = mainAssetBundle.LoadAsset<Sprite>("MassacreActiveIcon");
            icon4c = secondaryAssetBundle.LoadAsset<Sprite>("WorldEnderIcon");
            icon4d = secondaryAssetBundle.LoadAsset<Sprite>("WorldEnderInactiveIcon");

            styleMeter = mainAssetBundle.LoadAsset<GameObject>("StylePanel");

            styleD = mainAssetBundle.LoadAsset<Sprite>("StyleD");
            styleC = mainAssetBundle.LoadAsset<Sprite>("StyleC");
            styleB = mainAssetBundle.LoadAsset<Sprite>("StyleB");
            styleA = mainAssetBundle.LoadAsset<Sprite>("StyleA");
            styleS = mainAssetBundle.LoadAsset<Sprite>("StyleS");
            styleSS = mainAssetBundle.LoadAsset<Sprite>("StyleSS");
            styleSSS = mainAssetBundle.LoadAsset<Sprite>("StyleSSS");

            styleTextD = mainAssetBundle.LoadAsset<Sprite>("StyleDismal");
            styleTextC = mainAssetBundle.LoadAsset<Sprite>("StyleCrazy");
            styleTextB = mainAssetBundle.LoadAsset<Sprite>("StyleBadass");
            styleTextA = mainAssetBundle.LoadAsset<Sprite>("StyleApocalyptic");
            styleTextS = mainAssetBundle.LoadAsset<Sprite>("StyleSavage");
            styleTextSS = mainAssetBundle.LoadAsset<Sprite>("StyleSickSkills");
            styleTextSSS = mainAssetBundle.LoadAsset<Sprite>("StyleSmokinSexyStyle");

            bIconP = secondaryAssetBundle.LoadAsset<Sprite>("DeathbringerIcon");
            bIcon1 = secondaryAssetBundle.LoadAsset<Sprite>("DarkinBladeIcon");
            bIcon2 = secondaryAssetBundle.LoadAsset<Sprite>("ChainIcon");
            bIcon3 = secondaryAssetBundle.LoadAsset<Sprite>("UmbralDashIcon");
            bIcon4 = secondaryAssetBundle.LoadAsset<Sprite>("JudgementIcon");

            hitFX = mainAssetBundle.LoadAsset<GameObject>("AatroxHitFX");
            Utils.RegisterEffect(hitFX, 1);
            healFX = mainAssetBundle.LoadAsset<GameObject>("AatroxHealFX");
            Utils.RegisterEffect(healFX, 2);
            critFX = mainAssetBundle.LoadAsset<GameObject>("AatroxCritFX");
            Utils.RegisterEffect(critFX, 1);

            combo1FX = mainAssetBundle.LoadAsset<GameObject>("AatroxSlash1");
            combo2FX = mainAssetBundle.LoadAsset<GameObject>("AatroxSlash2");
            combo3FX = mainAssetBundle.LoadAsset<GameObject>("AatroxSlash3");

            downslashFX = mainAssetBundle.LoadAsset<GameObject>("AatroxDownslash");
            uppercutFX = mainAssetBundle.LoadAsset<GameObject>("AatroxUppercut");
            lungeFX = mainAssetBundle.LoadAsset<GameObject>("AatroxLungeSlash");

            rhaastUppercutFX = mainAssetBundle.LoadAsset<GameObject>("RhaastUppercutFX");

            flightFX = mainAssetBundle.LoadAsset<GameObject>("FlightFX");

            massacreFX = mainAssetBundle.LoadAsset<GameObject>("AatroxUltimate");

            projectileModel = mainAssetBundle.LoadAsset<GameObject>("BladeProjectile");
            blastModel = secondaryAssetBundle.LoadAsset<GameObject>("LuceModel");

            borisSlash1FX = secondaryAssetBundle.LoadAsset<GameObject>("BorisSlash1");
            borisSlash2FX = secondaryAssetBundle.LoadAsset<GameObject>("BorisSlash2");
            borisSlash3FX = secondaryAssetBundle.LoadAsset<GameObject>("BorisThrust");

            borisCombo1FX = secondaryAssetBundle.LoadAsset<GameObject>("BorisCombo1FX");
            borisCombo2FX = secondaryAssetBundle.LoadAsset<GameObject>("BorisCombo2FX");

            borisSpawnFX = secondaryAssetBundle.LoadAsset<GameObject>("BorisSpawnEffect");
            borisDeathFX = secondaryAssetBundle.LoadAsset<GameObject>("BorisDeathEffect");
            borisJudgementFX = secondaryAssetBundle.LoadAsset<GameObject>("BorisJudgementFX");
            borisJudgementStartFX = secondaryAssetBundle.LoadAsset<GameObject>("JudgementStartFX");
            borisExplosionFX = secondaryAssetBundle.LoadAsset<GameObject>("BorisFinisher");

            borisHitFX = secondaryAssetBundle.LoadAsset<GameObject>("BorisHitFX");
            Utils.RegisterEffect(borisHitFX, 1);

            //luce shader
            var hgShader = Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion();
            var matRhaast = mainAssetBundle.LoadAsset<Material>("matRhaast");
            matRhaast.shader = hgShader;

            Material material = null;
            material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            material.SetTexture("_MainTex", null);
            material.SetTexture("_EmTex", null);
            material.SetFloat("_EmPower", 30);
            material.SetColor("_EmColor", Color.red);
            material.SetFloat("_NormalStrength", 0);

            blastModel.GetComponentInChildren<MeshRenderer>().material = material;

            //add components to effects here

            /*combo1FX.AddComponent<EffectComponent>().soundName = Sounds.AatroxSwordSwing;
            combo1FX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            combo1FX.AddComponent<NetworkIdentity>();

            combo2FX.AddComponent<EffectComponent>().soundName = Sounds.AatroxSwordSwing;
            combo2FX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            combo2FX.AddComponent<NetworkIdentity>();

            combo3FX.AddComponent<EffectComponent>().soundName = Sounds.AatroxSwordSwing;
            combo3FX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            combo3FX.AddComponent<NetworkIdentity>();

            downslashFX.AddComponent<EffectComponent>().soundName = Sounds.AatroxDownslash;
            downslashFX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            downslashFX.AddComponent<NetworkIdentity>();

            uppercutFX.AddComponent<EffectComponent>().soundName = Sounds.AatroxUppercut;
            uppercutFX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            uppercutFX.AddComponent<NetworkIdentity>();

            hitFX.AddComponent<EffectComponent>();
            hitFX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            hitFX.AddComponent<NetworkIdentity>();

            healFX.AddComponent<EffectComponent>();
            healFX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            healFX.AddComponent<NetworkIdentity>();

            critFX.AddComponent<EffectComponent>();
            critFX.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            critFX.AddComponent<NetworkIdentity>();*/

            ShakeEmitter component = borisSpawnFX.AddComponent<ShakeEmitter>();

            ShakeEmitter component2 = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/BeamSphere").GetComponent<ProjectileImpactExplosion>().impactEffect.GetComponentInChildren<ShakeEmitter>();

            //LightIntensityCurve component4 = LegacyResourcesAPI.Load<GameObject>("Prefabs/Projectiles/BeamSphere").GetComponent<ProjectileImpactExplosion>().impactEffect.GetComponentInChildren<LightIntensityCurve>();

            component.amplitudeTimeDecay = component2.amplitudeTimeDecay;
            component.duration = component2.duration;
            component.radius = component2.radius;
            component.scaleShakeRadiusWithLocalScale = component2.scaleShakeRadiusWithLocalScale;
            component.shakeOnStart = true;
            component.wave = component2.wave;

            foreach (Light x in borisSpawnFX.GetComponentsInChildren<Light>())
            {
                if (x)
                {
                    x.enabled = false;
                    /*LightIntensityCurve component3 = x.gameObject.AddComponent<LightIntensityCurve>();
                    component3.curve = component4.curve;
                    component3.loop = false;
                    component3.randomStart = false;
                    component3.timeMax = component4.timeMax;*/
                }
            }

            foreach (Light x in borisDeathFX.GetComponentsInChildren<Light>())
            {
                if (x)
                {
                    x.enabled = false;
                }
            }

            foreach (Light x in borisJudgementFX.GetComponentsInChildren<Light>())
            {
                if (x)
                {
                    x.enabled = false;
                }
            }

            /*EffectComponent component2 = borisSpawnFX.AddComponent<EffectComponent>();
            component2.applyScale = false;
            component2.disregardZScale = true;
            component2.effectData = new EffectData();
            component2.effectIndex = EffectIndex.Invalid;
            component2.parentToReferencedTransform = false;
            component2.positionAtReferencedTransform = false;
            component2.soundName = Sounds.AatroxMassacreCast;*/

            //register network prefabs

            /*PrefabAPI.RegisterNetworkPrefab(hitFX);
            PrefabAPI.RegisterNetworkPrefab(healFX);
            PrefabAPI.RegisterNetworkPrefab(critFX);
            PrefabAPI.RegisterNetworkPrefab(combo1FX);
            PrefabAPI.RegisterNetworkPrefab(combo2FX);
            PrefabAPI.RegisterNetworkPrefab(combo3FX);
            PrefabAPI.RegisterNetworkPrefab(downslashFX);
            PrefabAPI.RegisterNetworkPrefab(uppercutFX);
            PrefabAPI.RegisterNetworkPrefab(flightFX);
            PrefabAPI.RegisterNetworkPrefab(massacreFX);*/

            // populate SOUNDS
            /*using (var bankStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PlayableSephiroth.SephirothBank.bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundBanks.Add(bytes);
            }*/

            // gather assets
            //SephIcon = MainAssetBundle.LoadAsset<Sprite>("SephirothIcon");
            //SephHitFx = MainAssetBundle.LoadAsset<GameObject>("SephHitFx");
        }
    }

}
