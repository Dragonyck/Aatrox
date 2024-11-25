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

namespace Aatrox
{
    class Hooks
    {
        internal static void RegisterHooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            RoR2.ContentManagement.ContentManager.onContentPacksAssigned += LateSetup;
        }
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (self && self.HasBuff(AatroxPlugin.massacreBuff))
            {
                AatroxController aatroxController = self.gameObject.GetComponent<AatroxController>();

                if (aatroxController)
                {
                    float damageValue = aatroxController.GetDamage();
                    float armorValue = aatroxController.GetArmor();

                    args.damageMultAdd += damageValue;
                    args.armorAdd += armorValue;
                    args.moveSpeedMultAdd += 0.15f;
                }
            }

            if (self && self.HasBuff(AatroxPlugin.worldEnderBuff))
            {
                args.damageMultAdd += 0.75f;
                args.armorAdd += 250;
            }

            if (self && self.baseNameToken == "AATROX_NAME")
            {
                AatroxController aatroxController = self.gameObject.GetComponent<AatroxController>();

                if (aatroxController)
                {
                    float attackSpeedValue = aatroxController.GetAttackSpeed();
                    args.attackSpeedMultAdd += attackSpeedValue;
                }
            }
        }

        private static void LateSetup(HG.ReadOnlyArray<RoR2.ContentManagement.ReadOnlyContentPack> obj)
        {
            ItemDisplays.SetIDRS();
        }
        private static void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport report)
        {
            if (self != null && report != null)
            {
                if (report.attacker)
                {
                    CharacterBody body = report.attacker.GetComponent<CharacterBody>();
                    if (body)
                    {
                        if (body.baseNameToken == "AATROX_NAME")
                        {
                            AatroxController styleObject = report.attacker.GetComponent<AatroxController>();
                            if (styleObject)
                            {
                                styleObject.AddStyle(0.35f);
                            }
                        }
                        else
                        {
                            GenericStyleController styleObject = report.attacker.GetComponent<GenericStyleController>();
                            if (styleObject != null)
                            {
                                styleObject.AddStyle(1.5f);
                            }
                        }
                    }
                }
            }

            orig(self, report);
        }

        private static void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo info, GameObject victim)
        {
            if (self != null && info != null && victim != null)
            {
                if (info.attacker && NetworkServer.active)
                {
                    CharacterBody body = info.attacker.GetComponent<CharacterBody>();
                    if (body)
                    {
                        if (body.baseNameToken == "AATROX_NAME")
                        {
                            AatroxController styleObject = info.attacker.GetComponent<AatroxController>();
                            if (styleObject != null)
                            {
                                styleObject.AddStyle(0f);
                            }
                            if ((info.damageType & DamageType.BypassBlock) != DamageType.Generic)
                            {
                                CharacterMotor charMotor = victim.GetComponent<CharacterMotor>();

                                if (charMotor && !charMotor.isGrounded)
                                {
                                    charMotor.velocity = new Vector3(0, 0.75f, 0);
                                }
                                info.damageType = DamageType.Generic;

                                HealthComponent healthComponent = info.attacker.GetComponent<HealthComponent>();

                                float healMod = 1f + (healthComponent.fullCombinedHealth / healthComponent.combinedHealth);

                                float healAmount = healthComponent.fullHealth * 0.05f * healMod;

                                healthComponent.Heal(healAmount, default(ProcChainMask), true);
                            }
                            if ((info.damageType & DamageType.BypassOneShotProtection) != DamageType.Generic)
                            {
                                info.damageType = DamageType.BonusToLowHealth;

                                //heal!!

                                HealthComponent healthComponent = info.attacker.GetComponent<HealthComponent>();

                                float healMod = 1f + (healthComponent.fullCombinedHealth / healthComponent.combinedHealth);

                                float healAmount = healthComponent.fullHealth * AatroxPlugin.baseHealAmount * healMod;

                                healthComponent.Heal(healAmount, default(ProcChainMask), true);

                                HealOrb healOrb = new HealOrb();
                                healOrb.origin = body.corePosition;
                                healOrb.target = healthComponent.body.mainHurtBox;
                                healOrb.healValue = 0f;
                                healOrb.overrideDuration = UnityEngine.Random.Range(0.3f, 0.6f);
                                OrbManager.instance.AddOrb(healOrb);
                            }

                            if ((info.damageType & DamageType.BypassArmor) != DamageType.Generic)
                            {
                                info.damageType = DamageType.Generic;

                                //air juggle

                                CharacterMotor charMotor = victim.GetComponent<CharacterMotor>();

                                if (charMotor && !charMotor.isGrounded)
                                {
                                    charMotor.velocity = new Vector3(0, 0.75f, 0);
                                }
                            }
                        }
                        else
                        {
                            if (body.teamComponent && body.teamComponent.teamIndex == TeamIndex.Player)
                            {
                                GenericStyleController styleObject = info.attacker.GetComponent<GenericStyleController>();
                                if (styleObject != null)
                                {
                                    styleObject.AddStyle(info.procCoefficient);
                                }
                            }
                        }
                    }
                }
            }

            orig(self, info, victim);
        }

        internal static void SettingsPanelControllerStart(On.RoR2.UI.SettingsPanelController.orig_Start orig, RoR2.UI.SettingsPanelController self)
        {
            orig(self);

            if (self.name == "SettingsSubPanel, Controls (M&KB)" || self.name == "SettingsSubPanel, Controls (Gamepad)")
            {
                var jumpBindingTransform = self.transform.Find("Scroll View").Find("Viewport").Find("VerticalLayout").Find("SettingsEntryButton, Binding (Jump)");

                AddActionBindingToSettings("AATROX_SKILL_SWAP_NAME", jumpBindingTransform);
                AddActionBindingToSettings("AATROX_SKILL_DANCE_NAME", jumpBindingTransform);
            }
        }
        internal static void AddActionBindingToSettings(string actionName, Transform buttonToCopy)
        {
            var inputBindingObject = GameObject.Instantiate(buttonToCopy, buttonToCopy.parent);
            var inputBindingControl = inputBindingObject.GetComponent<InputBindingControl>();
            inputBindingControl.actionName = actionName;
            //Usualy calling awake is bad as it's supposed to be called only by unity.
            //But in this case it is necessary to apply "actionName" change.
            inputBindingControl.Awake();
        }
    }
}
