using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Aatrox
{
    public class AatroxController : MonoBehaviour
    {
        public int skillUseCount = 0;

        public bool massacreFull;

        public float attackSpeedMod;
        public float damageBonus;
        public float armorBonus;

        private float minAttackSpeed;
        private float maxAttackSpeed;

        private int maxStacks;
        private int currentStacks;

        private float stackDamage;
        private float stackArmor;

        private float minHealthCost;
        private float maxHealthCost;

        private float maxMassacreBonusTimer;
        private float currentMassacreTimer;

        private float bladeSoundTimer;

        private CharacterBody charBody;
        private CharacterMotor charMotor;
        private HealthComponent charHealth;
        private InputBankTest inputBank;
        private ChildLocator childLocator;
        private Animator animator;

        private ParticleSystem priceFX;
        private ParticleSystem thirstFX;
        private ParticleSystem massacreFX;
        private ParticleSystem massacreCancelFX;
        private ParticleSystem darkFlightFX;
        private ParticleSystem healFX;

        private Transform chest;

        public StyleMeter styleMeter;

        private bool bladeStance;
        public bool massacre;
        public bool super;

        private bool transformationReady;

        public uint massacrePlayID;
        public uint buryTheLightSystemID;

        private void Awake()
        {
            massacrePlayID = 0;
            massacre = false;
            massacreFull = false;

            minAttackSpeed = 0f;
            maxAttackSpeed = 0.75f;
            maxStacks = 20;

            stackDamage = 0.05f;
            stackArmor = 4;

            minHealthCost = 0.06f;
            maxHealthCost = 6f;

            maxMassacreBonusTimer = 200;

            if (AatroxPlugin.buryTheLight.Value)
            {
                AkSoundEngine.SetState("Ranking", "F");
                buryTheLightSystemID = Util.PlaySound(Sounds.BuryTheLight, this.gameObject);
            }
        }

        private void Start()
        {
            foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
            {
                if (i.transform.parent.name == "PriceEffect") priceFX = i;
                if (i.transform.parent.name == "ThirstEffect") thirstFX = i;
                if (i.transform.parent.name == "MassacreEffect")
                {
                    massacreFX = i;
                    chest = i.transform.parent.parent;
                }
                if (i.transform.parent.name == "MassacreCancelEffect") massacreCancelFX = i;
                if (i.transform.parent.name == "DarkFlightEffect") darkFlightFX = i;
                if (i.transform.parent.name == "HealEffect") healFX = i;
            }

            charBody = base.GetComponent<CharacterBody>();
            if (charBody)
            {
                charHealth = charBody.healthComponent;
            }
            charMotor = GetComponentInChildren<CharacterMotor>();
            inputBank = GetComponentInChildren<InputBankTest>();
            childLocator = charBody.modelLocator.modelTransform.GetComponent<ChildLocator>();
            animator = childLocator.GetComponent<Animator>();

            Invoke("WorldEnderCheck", 0.1f);

            Invoke("EnableThirst", 0.5f);

            InvokeRepeating("Fuck", 0.5f, 0.1f);
            InvokeRepeating("MassacreCheck", 1f, 1f);

            super = false;
            if (charBody)
            {
                if (charBody.skinIndex == 3)
                {
                    super = true;
                    ChargeUltimate(1);
                }
            }

            if (AatroxPlugin.wideAatrox.Value && charBody)
            {
                if (charBody.modelLocator)
                {
                    Vector3 tempScale = charBody.modelLocator.modelBaseTransform.localScale;
                    tempScale.x *= 2f;
                    charBody.modelLocator.modelBaseTransform.localScale = tempScale;
                }
            }

            if (AatroxPlugin.bigHead.Value && childLocator)
            {
                childLocator.FindChild("Head").transform.localScale *= 2.5f;
            }

            HideWeapon();
        }

        private void HideWeapon()
        {
            var weapon = childLocator.FindChild("Weapon");
            if (weapon)
            {
                weapon.transform.localScale = Vector3.one * 0.01f;
            }
            weapon = childLocator.FindChild("Rhaast");
            if (weapon)
            {
                weapon.gameObject.SetActive(false);
            }
            weapon = childLocator.FindChild("GauntletL");
            if (weapon)
            {
                weapon.gameObject.SetActive(false);
            }
            weapon = childLocator.FindChild("GauntletR");
            if (weapon)
            {
                weapon.gameObject.SetActive(false);
            }
        }

        public void ResetWeapon()
        {
            int weapon = GetWeapon();
            EquipWeapon(weapon);
            SetCrosshair(weapon);
        }

        private int GetWeapon()
        {
            if (charBody && charBody.skillLocator)
            {
                if (charBody.skillLocator.primary.skillDef.skillNameToken == "AATROX_PRIMARY_SLASH_NAME")
                {
                    return 0;
                }
                else if (charBody.skillLocator.primary.skillDef.skillNameToken == "AATROX_PRIMARY_SCYTHE_NAME")
                {
                    return 1;
                }
                else if (charBody.skillLocator.primary.skillDef.skillNameToken == "AATROX_PRIMARY_GUN_NAME")
                {
                    return 2;
                }
                else if (charBody.skillLocator.primary.skillDef.skillNameToken == "SKILL_LUNAR_PRIMARY_REPLACEMENT_NAME")
                {
                    return 3;
                }
            }
            return -1;
        }

        private void SetCrosshair(int weaponIndex)
        {
            if (!charBody) return;

            switch (weaponIndex)
            {
                case 0:
                    charBody._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
                    break;
                case 1:
                    charBody._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
                    break;
                case 2:
                    charBody._defaultCrosshairPrefab = AatroxPlugin.gunCrosshair;
                    break;
                case 3:
                    charBody._defaultCrosshairPrefab = AatroxPlugin.needlerCrosshair;
                    break;
            }
        }

        public void EquipWeapon(int weaponIndex)
        {
            HideWeapon();

            Transform weapon = null;

            switch (weaponIndex)
            {
                case 0:
                    weapon = childLocator.FindChild("Weapon");
                    if (weapon)
                    {
                        weapon.transform.localScale = Vector3.one;
                    }
                    break;
                case 1:
                    weapon = childLocator.FindChild("Rhaast");
                    if (weapon)
                    {
                        weapon.gameObject.SetActive(true);
                    }
                    break;
                case 2:
                    weapon = childLocator.FindChild("GauntletL");
                    if (weapon)
                    {
                        weapon.gameObject.SetActive(true);
                    }
                    weapon = childLocator.FindChild("GauntletR");
                    if (weapon)
                    {
                        weapon.gameObject.SetActive(true);
                    }
                    break;
                //case 4:
                    //childLocator.FindChild("Needler").gameObject.SetActive(true);
                    //break;
            }
        }

        /*private void FixedUpdate()
        {
            if (hasScythe)
            {
                childLocator.FindChild("Weapon").transform.localScale = Vector3.one * 0.01f;
            }
        }*/

        private void CheckIcon()
        {
            if (charBody.skillLocator.special.stock >= 150)
            {
                if (!transformationReady)
                {
                    transformationReady = true;

                    childLocator.FindChild("ChestLightning").gameObject.SetActive(true);
                    Util.PlaySound(Sounds.BorisUltReady, base.gameObject);
                }

                if (charBody.skillLocator)
                {
                    if (charBody.skillLocator.special.skillDef.skillNameToken == "AATROX_SPECIAL_WORLDENDERINACTIVE_NAME")
                    {
                        charBody.skillLocator.special.SetBaseSkill(AatroxPlugin.worldEnderDef);
                    }
                }
            }
        }

        public void ChargeUltimate(int toCharge)
        {
            if (charBody.skillLocator.special.stock >= 150)
            {
                CheckIcon();
                return;
            }

            for (int i = 0; i < toCharge; i++)
            {
                if (charBody.skillLocator.special.stock >= 150)
                {
                    CheckIcon();
                    return;
                }

                charBody.skillLocator.special.AddOneStock();
                if (styleMeter && styleMeter.styleScore >= 95f) charBody.skillLocator.special.AddOneStock();
            }
        }

        private void WorldEnderCheck()
        {
            if (super) return;

            if (charBody.skillLocator)
            {
                if (charBody.skillLocator.special.skillDef.skillNameToken == "AATROX_SPECIAL_WORLDENDER_NAME")
                {
                    charBody.skillLocator.special.SetBaseSkill(AatroxPlugin.worldEnderInactiveDef);
                    charBody.skillLocator.special.RemoveAllStocks();
                }
            }
        }

        private void Fuck()
        {
            if (inputBank)
            {
                if (charBody.skillLocator)
                {
                    if (charBody.skillLocator.secondary.skillDef.skillNameToken == "AATROX_SECONDARY_COMBO_NAME")
                    {
                        bool oldStance = bladeStance;

                        if (inputBank.skill2.down)
                        {
                            bladeStance = true;
                        }
                        else
                        {
                            bladeStance = false;
                        }

                        if (bladeStance != oldStance)
                        {
                            if (bladeStance)
                            {
                                EnablePrice();
                            }
                            else
                            {
                                EnableThirst();
                            }
                        }
                    }
                }
            }
        }

        public void AddStyle(float coefficient)
        {
            if (styleMeter) styleMeter.AddStyle(coefficient);
        }

        private void MassacreCheck()
        {
            if (massacre && NetworkServer.active)
            {
                if (charHealth)
                {
                    if (!super)
                    {
                        float currentCost = GetHealthCost();
                        currentCost = charHealth.combinedHealth * currentCost;

                        currentCost /=  0.9f + (0.1f * charBody.level);

                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = currentCost;
                        damageInfo.position = charBody.corePosition;
                        damageInfo.force = Vector3.zero;
                        damageInfo.damageColorIndex = DamageColorIndex.Bleed;
                        damageInfo.crit = false;
                        damageInfo.attacker = null;
                        damageInfo.inflictor = null;
                        damageInfo.damageType = DamageType.NonLethal;
                        damageInfo.procCoefficient = 0f;
                        damageInfo.procChainMask = default(ProcChainMask);

                        bool flag = true;
                        if (styleMeter && styleMeter.styleScore >= 95) flag = false;

                        if (flag) charHealth.TakeDamage(damageInfo);
                    }

                    AddMassacreStack();
                }
            }
        }

        public void AddMassacreStack()
        {
            if (!massacre) return;

            currentStacks = Mathf.Clamp(currentStacks + 1, 0, maxStacks + 1);

            if (currentStacks >= (1 + maxStacks) && !massacreFull)
            {
                massacreFull = true;
                //this is where the effect would happen, for range increase
                Util.PlayAttackSpeedSound(Sounds.AatroxMassacreCast, base.gameObject, 4f);

                childLocator.FindChild("SwordLightning").gameObject.SetActive(true);
                animator.SetFloat("ultActive", 1f);
            }

            if (currentStacks < (1 + maxStacks))
            {
                if (charBody && NetworkServer.active) charBody.AddBuff(AatroxPlugin.massacreBuff);
            }

            //if (charBody) charBody.RecalculateStats();
        }

        public bool CastMassacre()
        {
            massacreFull = false;

            childLocator.FindChild("SwordLightning").gameObject.SetActive(false);
            animator.SetFloat("ultActive", 0f);

            if (massacre)
            {
                AatroxPlugin.devilTriggerActive--;
                massacre = false;
                currentMassacreTimer = Time.time;

                if (massacreFX)
                {
                    massacreFX.Stop();
                }

                if (massacreCancelFX)
                {
                    massacreCancelFX.Play();
                }

                if (!AatroxPlugin.buryTheLight.Value)
                {
                    if (AatroxPlugin.devilTrigger.Value) massacrePlayID = Util.PlaySound(Sounds.DevilTriggerPause, base.gameObject);
                    else AkSoundEngine.StopPlayingID(massacrePlayID);
                }

                //if (charBody) charBody.RecalculateStats();
            }
            else
            {
                AatroxPlugin.devilTriggerActive++;
                massacre = true;
                currentMassacreTimer = Time.time;
                currentStacks = 1;

                if (massacreFX)
                {
                    massacreFX.Play();
                }

                if (!chest) chest = transform;

                GameObject i = Instantiate(Assets.massacreFX, chest.position, chest.rotation);
                i.transform.SetParent(chest);
                Destroy(i, 8);

                if (!AatroxPlugin.buryTheLight.Value)
                {
                    if (AatroxPlugin.devilTrigger.Value)
                    {
                        if (massacrePlayID == 0) massacrePlayID = Util.PlaySound(Sounds.DevilTrigger, base.gameObject);
                        else massacrePlayID = Util.PlaySound(Sounds.DevilTriggerResume, base.gameObject);
                    }
                    else massacrePlayID = Util.PlaySound(Sounds.AatroxMassacreLoop, base.gameObject);
                }
            }

            return massacre;
        }

        public void StartDive()
        {
            if (darkFlightFX) darkFlightFX.Play();
        }

        public void EndDive()
        {
            if (darkFlightFX) darkFlightFX.Stop();
        }

        public void PlayHealEffect()
        {
            if (healFX) healFX.Play();
        }

        public void EnablePrice()
        {
            if (Time.time > bladeSoundTimer + 0.2f) Util.PlaySound(Sounds.AatroxPriceToggle, base.gameObject);

            bladeSoundTimer = Time.time;

            if (priceFX) priceFX.Play();
            if (thirstFX) thirstFX.Stop();
        }

        public void EnableThirst()
        {
            if (Time.time > bladeSoundTimer + 0.2f) Util.PlaySound(Sounds.AatroxThirstToggle, base.gameObject);

            bladeSoundTimer = Time.time;

            if (priceFX) priceFX.Stop();
            if (thirstFX) thirstFX.Play();
        }

        public float GetDamage()
        {
            /*float elapsed = Time.time - currentMassacreTimer;
            if (elapsed > maxMassacreBonusTimer) elapsed = maxMassacreBonusTimer;

            float x = Mathf.Lerp(minDamageBonus, maxDamageBonus, elapsed / maxMassacreBonusTimer);

            if (!massacre || currentMassacreTimer == 0) x = 0;*/

            return (currentStacks * stackDamage);//1 + (currentStacks * stackDamage);

            //return x;
        }

        public float GetArmor()
        {
            /*float elapsed = Time.time - currentMassacreTimer;
            if (elapsed > maxMassacreBonusTimer) elapsed = maxMassacreBonusTimer;

            float x = Mathf.Lerp(minArmorBonus, maxArmorBonus, elapsed / maxMassacreBonusTimer);

            if (!massacre || currentMassacreTimer == 0) x = 0;

            return x;*/

            return currentStacks * stackArmor;
        }

        public float GetHealthCost()
        {
            float elapsed = Time.time - currentMassacreTimer;
            if (elapsed > maxMassacreBonusTimer) elapsed = maxMassacreBonusTimer;

            float x = Mathf.Lerp(minHealthCost, maxHealthCost, elapsed / maxMassacreBonusTimer);

            if (!massacre || currentMassacreTimer == 0) x = 0;

            return x;
        }

        public float GetAttackSpeed()
        {
            float x = 0;

            if (charHealth)
            {
                x = Mathf.Lerp(maxAttackSpeed, minAttackSpeed, charHealth.combinedHealth / charHealth.fullCombinedHealth);
            }

            return x;
        }

        public void SelfDamage()
        {
            if (NetworkServer.active)
            {
                if (!super)
                {
                    if (charHealth)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = charHealth.combinedHealth * StaticValues.skillHealthCost;
                        damageInfo.position = charBody.corePosition;
                        damageInfo.force = Vector3.zero;
                        damageInfo.damageColorIndex = DamageColorIndex.Bleed;
                        damageInfo.crit = false;
                        damageInfo.attacker = null;
                        damageInfo.inflictor = null;
                        damageInfo.damageType = DamageType.NonLethal;
                        damageInfo.procCoefficient = 0f;
                        damageInfo.procChainMask = default(ProcChainMask);
                        charHealth.TakeDamage(damageInfo);

                        //if (charBody) charBody.RecalculateStats();
                    }
                }

                if (super) return;

                if (charBody.skillLocator)
                {
                    if (charBody.skillLocator.special.skillDef.skillNameToken == "AATROX_SPECIAL_WORLDENDERINACTIVE_NAME")
                    {
                        ChargeUltimate(3);
                    }
                }
            }
        }

        public void EndSkill()
        {
            Invoke("SkillShit", 0.2f);
        }

        private void SkillShit()
        {
            skillUseCount--;
        }

        private void OnDestroy()
        {
            if (massacre)
            {
                AatroxPlugin.devilTriggerActive--;
            }

            if (massacrePlayID != 0) AkSoundEngine.StopPlayingID(massacrePlayID);

            if (AatroxPlugin.buryTheLight.Value) AkSoundEngine.StopPlayingID(buryTheLightSystemID);

            //if (styleMeter) Destroy(styleMeter);
        }
    }

}
