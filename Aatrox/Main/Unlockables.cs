using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Achievements;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Aatrox
{
    public static class Unlockables
    {
        internal static UnlockableDef masteryUnlock;
        internal static UnlockableDef goldUnlock;
        internal static UnlockableDef superUnlock;
        public static void RegisterUnlockables()
        {
            masteryUnlock = NewUnlockable<Achievements.MasteryUnlockable>("MASTERYUNLOCKABLE", AatroxPlugin.justicarSkinIcon, "Aatrox: Mastery", "As Aatrox, beat the game or obliterate on Monsoon.");
            goldUnlock = NewUnlockable<Achievements.GoldUnlockable>("GOLDUNLOCKABLE", AatroxPlugin.goldSkinIcon, "Aatrox: SSStylish!", "As Aatrox, reach a stylish rank of SSS.");
            superUnlock = NewUnlockable<Achievements.SuperUnlockable>("SUPERUNLOCKABLE", AatroxPlugin.superSkinIcon, "Aatrox: Jackpot!", "As Aatrox, kill a Twisted Scavenger on Monsoon with no allies.");
        }
        static UnlockableDef NewUnlockable<T>(string AchievementIdentifier, Sprite Icon, string Title, string Description) where T : BaseAchievement
        {
            string IDKey = "ACHIEVEMENT_ROB_AATROX_";
            var unlock = ScriptableObject.CreateInstance<UnlockableDef>();
            string langName = IDKey + AchievementIdentifier + "_NAME";
            string langDesc = IDKey + AchievementIdentifier + "_DESCRIPTION";
            LanguageAPI.Add(langName, Title);
            LanguageAPI.Add(langDesc, Description);
            var s = new Func<string>(() => Language.GetStringFormatted("UNLOCKED_FORMAT", new object[]
            {
                Language.GetString(langName),
                Language.GetString(langDesc)
            }));
            Type type = typeof(T);

            unlock.cachedName = IDKey + AchievementIdentifier + "_UNLOCKABLE_ID";
            unlock.getHowToUnlockString = s;
            unlock.getUnlockedString = s;
            unlock.achievementIcon = Icon;
            unlock.sortScore = 200;
            unlock.hidden = false;
            ContentAddition.AddUnlockableDef(unlock);
            return unlock;
        }
    }
}

namespace Aatrox.Achievements
{
    [RegisterAchievement("ROB_AATROX_MASTERYUNLOCKABLE", "ACHIEVEMENT_ROB_AATROX_MASTERYUNLOCKABLE_UNLOCKABLE_ID", null, null)]
    public class MasteryUnlockable : BasePerSurvivorClearGameMonsoonAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("AatroxBody");
        }
    }
    [RegisterAchievement("ROB_AATROX_GOLDUNLOCKABLE", "ACHIEVEMENT_ROB_AATROX_GOLDUNLOCKABLE_UNLOCKABLE_ID", null, null)]
    public class GoldUnlockable : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("AatroxBody");
        }

        public void StyleCheck(float styleScore)
        {
            if (styleScore >= 95 && base.meetsBodyRequirement)
            {
                base.Grant();
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();

            StyleMeter.OnStyleChanged += this.StyleCheck;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();

            StyleMeter.OnStyleChanged -= this.StyleCheck;
        }
    }
    [RegisterAchievement("ROB_AATROX_SUPERUNLOCKABLE", "ACHIEVEMENT_ROB_AATROX_SUPERUNLOCKABLE_UNLOCKABLE_ID", null, null)]
    public class SuperUnlockable : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("AatroxBody");
        }

        public void CheckIfDeathIsAScav(DamageReport report)
        {
            if (report is null) return;
            if (report.victimBody is null) return;
            if (report.attackerBody is null) return;
            if (report.victimBody.name.Contains("ScavLunar"))
            {
                int count = 0;
                bool flag2 = true;
                ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
                TeamIndex objectTeam = TeamComponent.GetObjectTeam(report.attackerBody.gameObject);
                for (int j = 0; j < readOnlyInstancesList.Count; j++)
                {
                    CharacterMaster characterMaster = readOnlyInstancesList[j];
                    bool flag7 = characterMaster.teamIndex == objectTeam;
                    if (flag7)
                    {
                        count++;
                    }
                }

                if (count > 1) flag2 = false;

                if (flag2)
                {
                    if (base.meetsBodyRequirement)
                    {
                        DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.ruleBook.FindDifficulty());

                        if (difficultyDef != null && difficultyDef.countsAsHardMode)
                        {
                            base.Grant();
                        }
                    }
                }
            }
        }

        public override void OnInstall()
        {
            base.OnInstall();

            GlobalEventManager.onCharacterDeathGlobal += this.CheckIfDeathIsAScav;
        }

        public override void OnUninstall()
        {
            base.OnUninstall();

            GlobalEventManager.onCharacterDeathGlobal -= this.CheckIfDeathIsAScav;
        }
    }
}