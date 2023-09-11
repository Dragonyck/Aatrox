using R2API;
using R2API.Utils;
using RoR2;
using System;
using UnityEngine;

namespace Aatrox
{
    public static class BorisSkins
    {
        public static void RegisterSkins()
        {
            GameObject model = AatroxPlugin.borisPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            LanguageAPI.Add("BORISBODY_DEFAULT_SKIN_NAME", "Default");

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[0];

            skinDefInfo.Icon = AatroxPlugin.mainSkinIcon;
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[0];
            /*{
                new SkinDef.MeshReplacement
                {
                    renderer = mainRenderer,
                    mesh = mainRenderer.sharedMesh
                }
            };*/
            skinDefInfo.Name = "BORISBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "BORISBODY_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.UnlockableDef = null;

            SkinDef defaultSkin = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            skinController.skins = new SkinDef[]
            {
                defaultSkin
            };

            if (!AatroxPlugin.boris.Value) return;

            model = AatroxPlugin.infiniteBorisPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            characterModel = model.GetComponent<CharacterModel>();

            skinController = model.AddComponent<ModelSkinController>();

            skinController.skins = new SkinDef[]
            {
                defaultSkin
            };
        }
    }
}
