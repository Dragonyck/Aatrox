using R2API;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;

namespace Aatrox
{
    public static class Skins
    {
        public static void RegisterSkins()
        {
            GameObject model = AatroxPlugin.aatroxPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            AatroxPlugin.mainSkinIcon = LoadoutAPI.CreateSkinIcon(new Color(0.36f, 0.16f, 0.27f), new Color(0.56f, 0.23f, 0.19f), new Color(0.14f, 0.14f, 0.19f), new Color(0.17f, 0.24f, 0.34f));
            AatroxPlugin.justicarSkinIcon = LoadoutAPI.CreateSkinIcon(new Color(0.09f, 0.09f, 0.37f), new Color(0.48f, 0.23f, 0.57f), new Color(0.12f, 0.12f, 0.12f), new Color(0.19f, 0.21f, 0.36f));
            AatroxPlugin.goldSkinIcon = LoadoutAPI.CreateSkinIcon(new Color(0.36f, 0.16f, 0.27f), new Color(1f, 0.37f, 0f), new Color(0.27f, 0.23f, 0.12f), new Color(0.99f, 0.99f, 0.44f));
            AatroxPlugin.superSkinIcon = LoadoutAPI.CreateSkinIcon(new Color(0.43f, 0.15f, 0.25f), new Color(0.95f, 0.17f, 0.08f), new Color(0.17f, 0.23f, 0.34f), new Color(1f, 0.47f, 0.21f));

            LanguageAPI.Add("AATROXBODY_DEFAULT_SKIN_NAME", "Default");
            LanguageAPI.Add("AATROXBODY_MASTERY_SKIN_NAME", "Voidborn");
            LanguageAPI.Add("AATROXBODY_GOLD_SKIN_NAME", "Gilded");
            LanguageAPI.Add("AATROXBODY_SUPER_SKIN_NAME", "Ultimate");

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            skinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailL").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailR").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ChestLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ArmLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("HandLightning").gameObject,
                    shouldActivate = false
                },
            };

            skinDefInfo.Icon = AatroxPlugin.mainSkinIcon;
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            skinDefInfo.Name = "AATROXBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "AATROXBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.UnlockableDef = null;

            CharacterModel.RendererInfo[] rendererInfos = skinDefInfo.RendererInfos;
            CharacterModel.RendererInfo[] array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);

            Material material = array[0].defaultMaterial;

            /*if (material)
            {
                material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
                material.SetColor("_Color", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetColor("_Color"));
                material.SetTexture("_MainTex", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetTexture("_MainTex"));
                material.SetColor("_EmColor", Color.white);
                material.SetFloat("_EmPower", 2);
                material.SetTexture("_EmTex", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetTexture("_EmissionMap"));
                material.SetFloat("_NormalStrength", 0);

                array[0].defaultMaterial = material;
            }

            material = array[1].defaultMaterial;

            if (material)
            {
                material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
                material.SetColor("_Color", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetColor("_Color"));
                material.SetTexture("_MainTex", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetTexture("_MainTex"));
                material.SetColor("_EmColor", Color.white);
                material.SetFloat("_EmPower", 2);
                material.SetTexture("_EmTex", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetTexture("_EmissionMap"));
                material.SetFloat("_NormalStrength", 0);

                array[1].defaultMaterial = material;
            }

            material = array[2].defaultMaterial;

            if (material)
            {
                material = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
                material.SetColor("_Color", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetColor("_Color"));
                material.SetTexture("_MainTex", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetTexture("_MainTex"));
                material.SetColor("_EmColor", Color.white);
                material.SetFloat("_EmPower", 1);
                material.SetTexture("_EmTex", Assets.MainAssetBundle.LoadAsset<Material>("matAatrox").GetTexture("_EmissionMap"));
                material.SetFloat("_NormalStrength", 0);

                array[2].defaultMaterial = material;
            }

            skinDefInfo.RendererInfos = array;*/

            SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            //LoadoutAPI.AddSkinToCharacter(aatroxPrefab, defaultSkin);

            LoadoutAPI.SkinDefInfo justicarSkinDefInfo = default(LoadoutAPI.SkinDefInfo);
            justicarSkinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            justicarSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            justicarSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            justicarSkinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailL").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailR").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ChestLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ArmLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("HandLightning").gameObject,
                    shouldActivate = false
                },
            };

            justicarSkinDefInfo.Icon = AatroxPlugin.justicarSkinIcon;
            justicarSkinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            justicarSkinDefInfo.Name = "AATROXBODY_MASTERY_SKIN_NAME";
            justicarSkinDefInfo.NameToken = "AATROXBODY_MASTERY_SKIN_NAME";
            justicarSkinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            justicarSkinDefInfo.RootObject = model;
            justicarSkinDefInfo.UnlockableDef = Unlockables.masteryUnlock;

            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);

            material = array[0].defaultMaterial;

            if (material)
            {
                material = UnityEngine.Object.Instantiate<Material>(material);
                material.SetTexture("_MainTex", Assets.justicarMat.GetTexture("_MainTex"));
                material.SetTexture("_EmTex", Assets.justicarMat.GetTexture("_EmissionMap"));
                material.SetFloat("_EmPower", 5);

                array[0].defaultMaterial = material;
            }

            justicarSkinDefInfo.RendererInfos = array;

            SkinDef justicarSkin = LoadoutAPI.CreateNewSkinDef(justicarSkinDefInfo);

            LoadoutAPI.SkinDefInfo goldSkinDefInfo = default(LoadoutAPI.SkinDefInfo);
            goldSkinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            goldSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            goldSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            goldSkinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailL").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailR").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ChestLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ArmLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("HandLightning").gameObject,
                    shouldActivate = false
                },
            };

            goldSkinDefInfo.Icon = AatroxPlugin.goldSkinIcon;
            goldSkinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            goldSkinDefInfo.Name = "AATROXBODY_GOLD_SKIN_NAME";
            goldSkinDefInfo.NameToken = "AATROXBODY_GOLD_SKIN_NAME";
            goldSkinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            goldSkinDefInfo.RootObject = model;
            goldSkinDefInfo.UnlockableDef = Unlockables.goldUnlock;

            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);

            material = array[0].defaultMaterial;

            if (material)
            {
                material = UnityEngine.Object.Instantiate<Material>(material);
                material.SetTexture("_MainTex", Assets.gildedMat.GetTexture("_MainTex"));
                material.SetTexture("_EmTex", Assets.gildedMat.GetTexture("_EmissionMap"));
                material.SetFloat("_EmPower", 20);

                array[0].defaultMaterial = material;
            }

            goldSkinDefInfo.RendererInfos = array;

            SkinDef goldSkin = LoadoutAPI.CreateNewSkinDef(goldSkinDefInfo);

            LoadoutAPI.SkinDefInfo superSkinDefInfo = default(LoadoutAPI.SkinDefInfo);
            superSkinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            superSkinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            superSkinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];

            superSkinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[]
            {
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailL").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("EyeTrailR").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ChestLightning").gameObject,
                    shouldActivate = false
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("ArmLightning").gameObject,
                    shouldActivate = true
                },
                new SkinDef.GameObjectActivation
                {
                    gameObject = childLocator.FindChild("HandLightning").gameObject,
                    shouldActivate = true
                },
            };

            superSkinDefInfo.Icon = AatroxPlugin.superSkinIcon;
            superSkinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[]
            {
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };
            superSkinDefInfo.Name = "AATROXBODY_SUPER_SKIN_NAME";
            superSkinDefInfo.NameToken = "AATROXBODY_SUPER_SKIN_NAME";
            superSkinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            superSkinDefInfo.RootObject = model;
            superSkinDefInfo.UnlockableDef = Unlockables.superUnlock;

            rendererInfos = skinDefInfo.RendererInfos;
            array = new CharacterModel.RendererInfo[rendererInfos.Length];
            rendererInfos.CopyTo(array, 0);

            material = array[0].defaultMaterial;

            if (material)
            {
                material = UnityEngine.Object.Instantiate<Material>(material);
                material.SetTexture("_MainTex", Assets.superMat.GetTexture("_MainTex"));
                material.SetTexture("_EmTex", Assets.superMat.GetTexture("_EmissionMap"));
                material.SetFloat("_EmPower", 10);

                array[0].defaultMaterial = material;
            }

            superSkinDefInfo.RendererInfos = array;

            SkinDef superSkin = LoadoutAPI.CreateNewSkinDef(superSkinDefInfo);

            skinController.skins = new SkinDef[]
            {
                defaultSkin,
                justicarSkin,
                goldSkin,
                superSkin
            };
        }
    }
}
