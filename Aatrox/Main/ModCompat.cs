using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Aatrox
{
    public static class ModCompat
    {
        private static bool? emoteEnabled;
        private static bool? skillsEnabled;

        public static bool EmoteAPIEnabled
        {
            get
            {
                if (emoteEnabled == null)
                {
                    emoteEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.weliveinasociety.CustomEmotesAPI");
                }
                return (bool)emoteEnabled;
            }
        }
        public static bool SkillsPlusPlusEnabled
        {
            get
            {
                if (skillsEnabled == null)
                {
                    skillsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.cwmlolzlz.skills");
                }
                return (bool)skillsEnabled;
            }
        }
        public static void AddSkillsSupport()
        {
            //RegisterSkillModifiers();
        }
        public static void AddEmoteSupport()
        {
            EmotesAPI.CustomEmotesAPI.ImportArmature(AatroxPlugin.aatroxPrefab, Assets.mainAssetBundle.LoadAsset<GameObject>("AatroxHumanoidEmoteSetup"), false);
            EmotesAPI.CustomEmotesAPI.ImportArmature(AatroxPlugin.borisPrefab, Assets.secondaryAssetBundle.LoadAsset<GameObject>("BorisHumanoidEmoteSetup"), false);
        }
    }
}
