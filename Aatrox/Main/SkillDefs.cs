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

namespace Aatrox
{
    class BorisSkillDefs
    {
        internal static void BorisPassiveSetup()
        {
            SkillLocator component = AatroxPlugin.borisPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("BORIS_PASSIVE_NAME", "Deathbringer");
            LanguageAPI.Add("BORIS_PASSIVE_DESCRIPTION", "<style=cIsHealing>Heal a portion of your max health</style> every time you <style=cIsDamage>deal damage</style>.");

            component.passiveSkill.enabled = false;
            component.passiveSkill.skillNameToken = "BORIS_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "BORIS_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.bIconP;
        }
        internal static void BorisPrimarySetup()
        {
            SkillLocator component = AatroxPlugin.borisPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("BORIS_PRIMARY_SLASH_NAME", "The Darkin Blade");
            LanguageAPI.Add("BORIS_PRIMARY_SLASH_DESCRIPTION", "Swing your massive sword, dealing <style=cIsDamage>750% damage</style>. <style=cIsDamage>Damage increases with attack speed</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BorisGroundLight));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.bIcon1;
            mySkillDef.skillDescriptionToken = "BORIS_PRIMARY_SLASH_DESCRIPTION";
            mySkillDef.skillName = "BORIS_PRIMARY_SLASH_NAME";
            mySkillDef.skillNameToken = "BORIS_PRIMARY_SLASH_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            component.primary = AatroxPlugin.borisPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }
        internal static void BorisSecondarySetup()
        {
            SkillLocator component = AatroxPlugin.borisPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("BORIS_SECONDARY_BLAST_NAME", "Eldritch Bolts");
            //LanguageAPI.Add("BORIS_SECONDARY_BLAST_DESCRIPTION", "Shoot a ball of demonic energy that explodes for <style=cIsDamage>600% damage</style>, <style=cIsUtility>zapping nearby enemies</style> along the way.");
            LanguageAPI.Add("BORIS_SECONDARY_BLAST_DESCRIPTION", "Continuously fire tracking bolts for <style=cIsDamage>200% damage</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BorisLuce));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.canceledFromSprinting = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.bIcon2;
            mySkillDef.skillDescriptionToken = "BORIS_SECONDARY_BLAST_DESCRIPTION";
            mySkillDef.skillName = "BORIS_SECONDARY_BLAST_NAME";
            mySkillDef.skillNameToken = "BORIS_SECONDARY_BLAST_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            component.secondary = AatroxPlugin.borisPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }
        internal static void BorisUtilitySetup()
        {
            SkillLocator component = AatroxPlugin.borisPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("BORIS_UTILITY_BLINK_NAME", "Umbral Dash");
            LanguageAPI.Add("BORIS_UTILITY_BLINK_DESCRIPTION", "<style=cIsUtility>Instantly blink</style> to a nearby location.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BorisBlink));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.bIcon3;
            mySkillDef.skillDescriptionToken = "BORIS_UTILITY_BLINK_DESCRIPTION";
            mySkillDef.skillName = "BORIS_UTILITY_BLINK_NAME";
            mySkillDef.skillNameToken = "BORIS_UTILITY_BLINK_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            component.utility = AatroxPlugin.borisPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }
        internal static void BorisSpecialSetup()
        {
            SkillLocator component = AatroxPlugin.borisPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("BORIS_SPECIAL_JUDGEMENT_NAME", "Judgement");
            LanguageAPI.Add("BORIS_SPECIAL_JUDGEMENT_DESCRIPTION", "Unleash all of your power at once with a devastating combo, dealing <style=cIsDamage>3x500% damage</style> and ending with an explosion that deals <style=cIsDamage>1500% damage</style>. <style=cIsHealth>Ends the transformation</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(BorisFinisher));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 150;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.rechargeStock = 0;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 0;
            mySkillDef.icon = Assets.bIcon4;
            mySkillDef.skillDescriptionToken = "BORIS_SPECIAL_JUDGEMENT_DESCRIPTION";
            mySkillDef.skillName = "BORIS_SPECIAL_JUDGEMENT_NAME";
            mySkillDef.skillNameToken = "BORIS_SPECIAL_JUDGEMENT_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            component.special = AatroxPlugin.borisPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.special.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
        }
    }
    class AatroxSkillDefs
    {
        internal static void PassiveSetup()
        {
            SkillLocator component = AatroxPlugin.aatroxPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("AATROX_PASSIVE_NAME", "Hellbent");
            //LanguageAPI.Add("AATROX_PASSIVE_DESCRIPTION", "Convert all <style=cIsUtility>shields</style> into <style=cIsHealth>bonus health</style>.");
            LanguageAPI.Add("AATROX_PASSIVE_DESCRIPTION", "Gain increased <style=cIsDamage>attack speed</style> the <style=cIsUtility>lower your health is</style>.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = "AATROX_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = "AATROX_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.iconP;
        }
        internal static void PrimarySetup()
        {
            SkillLocator component = AatroxPlugin.aatroxPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("AATROX_PRIMARY_FAMILY_NAME", "AatroxPrimaryFamily");

            LanguageAPI.Add("AATROX_PRIMARY_SLASH_NAME", "Blood Thirst");
            LanguageAPI.Add("AATROX_PRIMARY_SLASH_DESCRIPTION", "<style=cIsUtility>Agile.</style> Swing your sword for <style=cIsDamage>" + StaticValues.bloodThirstComboDamage * 100f + "% damage</style>. Every <style=cIsUtility>third hit</style> deals <style=cIsDamage>" + StaticValues.bloodThirstFinisherDamage * 100f + "% damage</style> <style=cStack>(bonus damage to low health enemies)</style>, and <style=cIsHealing>heals you for " + AatroxPlugin.baseHealAmount * 100f + "% max health, increasing the lower your health is</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AatroxGroundLight));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1;
            mySkillDef.skillDescriptionToken = "AATROX_PRIMARY_SLASH_DESCRIPTION";
            mySkillDef.skillName = "AATROX_PRIMARY_SLASH_NAME";
            mySkillDef.skillNameToken = "AATROX_PRIMARY_SLASH_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_AGILE"
            };

            ContentAddition.AddSkillDef(mySkillDef);

            component.primary = AatroxPlugin.aatroxPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = "AATROX_PRIMARY_FAMILY_NAME";
            newFamily.variants = new SkillFamily.Variant[1];
            component.primary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.primary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);

            LanguageAPI.Add("AATROX_PRIMARY_GUN_NAME", StaticValues.gunName);
            LanguageAPI.Add("AATROX_PRIMARY_GUN_DESCRIPTION", "<style=cIsUtility>Agile.</style> Fire bolts of demonic power for <style=cIsDamage>2x" + StaticValues.gunDamage * 100f + "% damage</style>.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Aatrox.Gun.AatroxFireGuns));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1c;
            mySkillDef.skillDescriptionToken = "AATROX_PRIMARY_GUN_DESCRIPTION";
            mySkillDef.skillName = "AATROX_PRIMARY_GUN_NAME";
            mySkillDef.skillNameToken = "AATROX_PRIMARY_GUN_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_AGILE"
            };

            ContentAddition.AddSkillDef(mySkillDef);

            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            /*
            LanguageAPI.Add("AATROX_PRIMARY_SCYTHE_NAME", "Rhaast");
            LanguageAPI.Add("AATROX_PRIMARY_SCYTHE_DESCRIPTION", "Swing a massive scythe, dealing <style=cIsDamage>250% damage</style> and <style=cIsHealing>healing you for 5% max health</style>.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(EntityStates.Aatrox.Rhaast.RhaastGroundLight));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1b;
            mySkillDef.skillDescriptionToken = "AATROX_PRIMARY_SCYTHE_DESCRIPTION";
            mySkillDef.skillName = "AATROX_PRIMARY_SCYTHE_NAME";
            mySkillDef.skillNameToken = "AATROX_PRIMARY_SCYTHE_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };*/
        }
        internal static void SecondarySetup()
        {
            SkillLocator component = AatroxPlugin.aatroxPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("AATROX_SECONDARY_FAMILY_NAME", "AatroxSecondaryFamily");

            LanguageAPI.Add("KEYWORD_LUNGE", "<style=cKeywordName>Stinger</style><style=cSub>A <style=cIsUtility>forward lunge</style> that stops upon hitting an enemy.\n<style=cIsDamage>" + AatroxLunge.damageCoefficient * 100f + "% damage. Deals bonus damage to low health enemies.</style></style>");
            LanguageAPI.Add("KEYWORD_KNOCKUP", "<style=cKeywordName>High Time</style><style=cSub>An <style=cIsUtility>uppercut</style> that <style=cIsDamage>stuns</style> and knocks enemies airborne.\n<style=cIsDamage>" + AatroxUppercut.baseDamageCoefficient * 100f + "% damage</style></style>");
            LanguageAPI.Add("KEYWORD_KNOCKDOWN", "<style=cKeywordName>Helm Breaker</style><style=cSub>A powerful <style=cIsDamage>overhead slam</style> that stops upon hitting the ground.\n<style=cIsDamage>" + AatroxDownslash.baseDamageCoefficient * 100f + "% damage</style></style>");

            LanguageAPI.Add("AATROX_SECONDARY_COMBO_NAME", "Blood Price");
            LanguageAPI.Add("AATROX_SECONDARY_COMBO_DESCRIPTION", "<style=cIsHealth>" + StaticValues.skillHealthCost * 100 + "% HP</style>. Hold to <style=cIsDamage>empower</style> your next <style=cIsDamage>primary attack</style>. Hold <style=cIsUtility>forward</style> to use <style=cIsDamage>Stinger</style> and <style=cIsUtility>back</style> to use <style=cIsDamage>High Time</style> or <style=cIsDamage>Helm Breaker</style>. <style=cIsHealing>Cooldown resets on hit</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AatroxSwordStance));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 5f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 0;
            mySkillDef.icon = Assets.icon2b;
            mySkillDef.skillDescriptionToken = "AATROX_SECONDARY_COMBO_DESCRIPTION";
            mySkillDef.skillName = "AATROX_SECONDARY_COMBO_NAME";
            mySkillDef.skillNameToken = "AATROX_SECONDARY_COMBO_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_PERCENT_HP",
                "KEYWORD_LUNGE",
                "KEYWORD_KNOCKUP",
                "KEYWORD_KNOCKDOWN"
            };

            ContentAddition.AddSkillDef(mySkillDef);

            component.secondary = AatroxPlugin.aatroxPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = "AATROX_SECONDARY_FAMILY_NAME";
            newFamily.variants = new SkillFamily.Variant[1];
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.secondary.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);

            LanguageAPI.Add("AATROX_SECONDARY_SLASH_NAME", "Blades of Torment");
            LanguageAPI.Add("AATROX_SECONDARY_SLASH_DESCRIPTION", "<style=cIsHealth>" + StaticValues.skillHealthCost * 100 + "% HP</style>. Fire two converging blades of energy, dealing <style=cIsDamage>2x350% damage</style>.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AatroxFireProjectile));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 3f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon2;
            mySkillDef.skillDescriptionToken = "AATROX_SECONDARY_SLASH_DESCRIPTION";
            mySkillDef.skillName = "AATROX_SECONDARY_SLASH_NAME";
            mySkillDef.skillNameToken = "AATROX_SECONDARY_SLASH_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_PERCENT_HP"
            };

            ContentAddition.AddSkillDef(mySkillDef);

            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }
        internal static void UtilitySetup()
        {
            SkillLocator component = AatroxPlugin.aatroxPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("AATROX_UTILITY_FAMILY_NAME", "AatroxUtilityFamily");

            LanguageAPI.Add("AATROX_UTILITY_FLIGHT_NAME", "Dark Flight");
            LanguageAPI.Add("AATROX_UTILITY_FLIGHT_DESCRIPTION", "<style=cIsDamage>Stunning</style>. Rise up into the air, then <style=cIsUtility>dive back down</style>. Upon impact, deal <style=cIsDamage>" + AatroxDive.impactDamageCoefficient * 100f + "% damage</style>.");

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AatroxBeginFlight));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 14f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon3;
            mySkillDef.skillDescriptionToken = "AATROX_UTILITY_FLIGHT_DESCRIPTION";
            mySkillDef.skillName = "AATROX_UTILITY_FLIGHT_NAME";
            mySkillDef.skillNameToken = "AATROX_UTILITY_FLIGHT_NAME";
            mySkillDef.keywordTokens = new string[] {
                "KEYWORD_STUNNING"
            };

            ContentAddition.AddSkillDef(mySkillDef);

            component.utility = AatroxPlugin.aatroxPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = "AATROX_UTILITY_FAMILY_NAME";
            newFamily.variants = new SkillFamily.Variant[1];
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);

            LanguageAPI.Add("AATROX_UTILITY_SHADOW_NAME", "Shadow Step");
            LanguageAPI.Add("AATROX_UTILITY_SHADOW_DESCRIPTION", "<style=cIsUtility>Blink</style> in a direction. Hold up to 3 charges.");

            mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AatroxShadowStep));
            mySkillDef.activationStateMachineName = "Dash";
            mySkillDef.baseMaxStock = StaticValues.shadowStepStock;
            mySkillDef.baseRechargeInterval = 4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon3b;
            mySkillDef.skillDescriptionToken = "AATROX_UTILITY_SHADOW_DESCRIPTION";
            mySkillDef.skillName = "AATROX_UTILITY_SHADOW_NAME";
            mySkillDef.skillNameToken = "AATROX_UTILITY_SHADOW_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }
        internal static void SpecialSetup()
        {
            SkillLocator component = AatroxPlugin.aatroxPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add("AATROX_SPECIAL_FAMILY_NAME", "AatroxSpecialFamily");

            string skillName = "";
            string skillDesc = "";

            if (AatroxPlugin.devilTrigger.Value)
            {
                skillName = "Devil Trigger";
                skillDesc = "Toggle on to <style=cIsHealth>drain health</style> in exchange for bonus <style=cIsDamage>damage</style> and <style=cIsHealing>armor</style>. <style=cIsHealth>Health drain</style> and <style=cIsHealing>stat bonuses</style> increase <style=cIsUtility>the longer Devil Trigger is active</style>.";
            }
            else
            {
                skillName = "Massacre";
                skillDesc = "Toggle on to <style=cIsHealth>drain health</style> in exchange for bonus <style=cIsDamage>damage</style> and <style=cIsHealing>armor</style>. <style=cIsHealth>Health drain</style> and <style=cIsHealing>stat bonuses</style> increase <style=cIsUtility>the longer Massacre is active</style>.";
            }

            LanguageAPI.Add("AATROX_SPECIAL_MASSACRE_NAME", skillName);
            LanguageAPI.Add("AATROX_SPECIAL_MASSACRE_DESCRIPTION", skillDesc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(AatroxMassacre));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 1f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.cancelSprintingOnActivation = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon4;
            mySkillDef.skillDescriptionToken = "AATROX_SPECIAL_MASSACRE_DESCRIPTION";
            mySkillDef.skillName = "AATROX_SPECIAL_MASSACRE_NAME";
            mySkillDef.skillNameToken = "AATROX_SPECIAL_MASSACRE_NAME";

            ContentAddition.AddSkillDef(mySkillDef);

            component.special = AatroxPlugin.aatroxPrefab.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            (newFamily as ScriptableObject).name = "AATROX_SPECIAL_FAMILY_NAME";
            newFamily.variants = new SkillFamily.Variant[1];
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.special.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);

            AatroxPlugin.massacreDef = mySkillDef;

            if (AatroxPlugin.devilTrigger.Value)
            {
                skillName = "Devil Trigger";
                skillDesc = "Cancel Devil Trigger.";
            }
            else
            {
                skillName = "Massacre";
                skillDesc = "Cancel Massacre.";
            }

            LanguageAPI.Add("AATROX_SPECIAL_MASSACRECANCEL_NAME", skillName);
            LanguageAPI.Add("AATROX_SPECIAL_MASSACRECANCEL_DESCRIPTION", skillDesc);

            SkillDef newSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            newSkillDef.activationState = new SerializableEntityStateType(typeof(AatroxMassacre));
            newSkillDef.activationStateMachineName = "Body";
            newSkillDef.baseMaxStock = 1;
            newSkillDef.baseRechargeInterval = 1f;
            newSkillDef.beginSkillCooldownOnSkillEnd = false;
            newSkillDef.canceledFromSprinting = false;
            newSkillDef.fullRestockOnAssign = true;
            newSkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            newSkillDef.isCombatSkill = true;
            newSkillDef.mustKeyPress = false;
            newSkillDef.cancelSprintingOnActivation = false;
            newSkillDef.rechargeStock = 1;
            newSkillDef.requiredStock = 1;
            newSkillDef.stockToConsume = 1;
            newSkillDef.icon = Assets.icon4b;
            newSkillDef.skillDescriptionToken = "AATROX_SPECIAL_MASSACRECANCEL_DESCRIPTION";
            newSkillDef.skillName = "AATROX_SPECIAL_MASSACRECANCEL_NAME";
            newSkillDef.skillNameToken = "AATROX_SPECIAL_MASSACRECANCEL_NAME";

            ContentAddition.AddSkillDef(newSkillDef);

            AatroxPlugin.massacreCancelDef = newSkillDef;


            if (AatroxPlugin.devilTrigger.Value)
            {
                skillName = "Sin Devil Trigger";
                skillDesc = "Reveal your <style=cIsUtility>true demonic form</style> and gain <style=cIsHealth>massive power</style>.";
            }
            else
            {
                skillName = "World Ender";
                skillDesc = "Reveal your <style=cIsUtility>true demonic form</style> and gain <style=cIsHealth>massive power</style>.";
            }

            LanguageAPI.Add("AATROX_SPECIAL_WORLDENDER_NAME", skillName);
            LanguageAPI.Add("AATROX_SPECIAL_WORLDENDER_DESCRIPTION", skillDesc);

            SkillDef wSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            wSkillDef.activationState = new SerializableEntityStateType(typeof(AatroxTransformation));
            wSkillDef.activationStateMachineName = "Body";
            wSkillDef.baseMaxStock = 150;
            wSkillDef.baseRechargeInterval = 0f;
            wSkillDef.beginSkillCooldownOnSkillEnd = false;
            wSkillDef.canceledFromSprinting = false;
            wSkillDef.fullRestockOnAssign = false;
            wSkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            wSkillDef.isCombatSkill = true;
            wSkillDef.mustKeyPress = false;
            wSkillDef.cancelSprintingOnActivation = true;
            wSkillDef.rechargeStock = 0;
            wSkillDef.requiredStock = 150;
            wSkillDef.stockToConsume = 150;
            wSkillDef.icon = Assets.icon4c;
            wSkillDef.skillDescriptionToken = "AATROX_SPECIAL_WORLDENDER_DESCRIPTION";
            wSkillDef.skillName = "AATROX_SPECIAL_WORLDENDER_NAME";
            wSkillDef.skillNameToken = "AATROX_SPECIAL_WORLDENDER_NAME";

            ContentAddition.AddSkillDef(wSkillDef);

            AatroxPlugin.worldEnderDef = wSkillDef;


            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = wSkillDef,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(wSkillDef.skillNameToken, false, null)
            };

            if (AatroxPlugin.devilTrigger.Value)
            {
                skillName = "Sin Devil Trigger";
                skillDesc = "<style=cIsUtility>Charge up</style> this form by <style=cIsHealth>spending health</style>, <style=cIsDamage>dealing damage</style> and <style=cIsHealing>healing</style>.";
            }
            else
            {
                skillName = "World Ender";
                skillDesc = "<style=cIsUtility>Charge up</style> this form by <style=cIsHealth>spending health</style>, <style=cIsDamage>dealing damage</style> and <style=cIsHealing>healing</style>.";
            }

            LanguageAPI.Add("AATROX_SPECIAL_WORLDENDERINACTIVE_NAME", skillName);
            LanguageAPI.Add("AATROX_SPECIAL_WORLDENDERINACTIVE_DESCRIPTION", skillDesc);

            SkillDef wiSkillDef = ScriptableObject.CreateInstance<SkillDef>();
            wiSkillDef.activationState = new SerializableEntityStateType(typeof(AatroxTransformation));
            wiSkillDef.activationStateMachineName = "Body";
            wiSkillDef.baseMaxStock = 150;
            wiSkillDef.baseRechargeInterval = 0f;
            wiSkillDef.beginSkillCooldownOnSkillEnd = false;
            wiSkillDef.canceledFromSprinting = false;
            wiSkillDef.fullRestockOnAssign = false;
            wiSkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            wiSkillDef.isCombatSkill = true;
            wiSkillDef.mustKeyPress = false;
            wiSkillDef.cancelSprintingOnActivation = true;
            wiSkillDef.rechargeStock = 0;
            wiSkillDef.requiredStock = 150;
            wiSkillDef.stockToConsume = 150;
            wiSkillDef.icon = Assets.icon4d;
            wiSkillDef.skillDescriptionToken = "AATROX_SPECIAL_WORLDENDERINACTIVE_DESCRIPTION";
            wiSkillDef.skillName = "AATROX_SPECIAL_WORLDENDERINACTIVE_NAME";
            wiSkillDef.skillNameToken = "AATROX_SPECIAL_WORLDENDERINACTIVE_NAME";

            ContentAddition.AddSkillDef(wiSkillDef);

            AatroxPlugin.worldEnderInactiveDef = wiSkillDef;
        }
    }
}
