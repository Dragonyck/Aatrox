using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Aatrox
{
    public class AatroxGroundLight : BaseState
    {
        public string swordmasterSkillNameToken = "AATROX_SECONDARY_COMBO_NAME";

        public static float comboDamageCoefficient = 1.75f;
        public static float finisherDamageCoefficient = 3.5f;
        public static float priceFinisherDamageCoefficient = 5f;
        public static float hitHopVelocity = 5f;
        public static float styleCoefficient = 0.9f;
        public static float finisherStyleCoefficient = 1.5f;
        public static float slashRecoil = 1f;

        private float stopwatch;
        private float attackDuration;
        private float earlyExitDuration;
        private Animator animator;
        private OverlapAttack overlapAttack;
        private float hitPauseTimer;
        private bool isInHitPause;
        private bool hasSwung;
        private bool hasHit;
        private bool hasHopped;
        public AatroxGroundLight.ComboState comboState;
        private BaseState.HitStopCachedState hitStopCachedState;
        private GameObject swingEffectPrefab;
        private GameObject hitEffectPrefab;
        private string attackSoundString;
        private string hitSoundString;
        private bool usingPrice;
        private bool usedPrice;
        private bool spendingHealth;
        private AatroxController aatroxController;

        public enum ComboState
        {
            GroundLight1,
            GroundLight2,
            GroundLight3
        }

        private bool SwordmasterCheck()
        {
            if (!base.skillLocator) return false;
            if (!base.skillLocator.secondary) return false;
            if (!base.skillLocator.secondary.skillDef) return false;

            return base.skillLocator.secondary.skillDef.skillNameToken == this.swordmasterSkillNameToken;
        }

        private bool SwordmasterInputCheck()
        {
            this.usingPrice = false;

            bool flag = false;

            if (base.skillLocator)
            {
                if (base.skillLocator.secondary.stock > 0)
                {
                    if (base.inputBank.skill2.down && base.inputBank.skill1.down)
                    {
                        bool inputFlag1 = false;
                        bool inputFlag2 = false;

                        if (Input.GetKey(AatroxPlugin.swordmasterForwardKeybind.Value)) inputFlag1 = true;
                        if (Input.GetKey(AatroxPlugin.swordmasterBackKeybind.Value)) inputFlag2 = true;

                        if (inputFlag1)
                        {
                            //base.skillLocator.primary.stateMachine.customName = "Body";
                            this.outer.SetNextState(new AatroxLunge());
                            flag = true;
                        }
                        else if (inputFlag2)
                        {
                            bool flag1 = base.isGrounded;

                            if (flag1)
                            {
                                //base.skillLocator.primary.stateMachine.customName = "Body";
                                this.outer.SetNextState(new AatroxUppercut());
                                flag = true;
                            }
                            else
                            {
                                //base.skillLocator.primary.stateMachine.customName = "Body";
                                this.outer.SetNextState(new AatroxDownslash());
                                flag = true;
                            }
                        }
                        else this.usingPrice = true;
                    }
                }
            }

            if (flag && base.skillLocator)
            {
                base.skillLocator.secondary.DeductStock(1);
            }

            if (flag) this.spendingHealth = true;

            return flag;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.spendingHealth = false;
            this.usedPrice = false;
            this.stopwatch = 0f;
            this.earlyExitDuration = GroundLight.baseEarlyExitDuration / this.attackSpeedStat;
            this.animator = base.GetModelAnimator();
            this.aatroxController = base.GetComponent<AatroxController>();

            this.hasSwung = false;
            this.hasHopped = false;

            //if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Weapon";

            bool @bool = this.animator.GetBool("isMoving");
            bool bool2 = this.animator.GetBool("isGrounded");

            string hitboxString = "Sword";
            if (this.aatroxController)
            {
                this.aatroxController.skillUseCount++;
                if (this.aatroxController.massacreFull)
                {
                    hitboxString = "SwordBig";
                }
            }

            switch (this.comboState)
            {
                case AatroxGroundLight.ComboState.GroundLight1:
                    this.attackDuration = GroundLight.baseComboAttackDuration / this.attackSpeedStat;
                    this.overlapAttack = base.InitMeleeOverlap(AatroxGroundLight.comboDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), hitboxString);
                    if (@bool || !bool2)
                    {
                        base.PlayAnimation("Gesture, Override", "GroundLight1", "GroundLight.playbackRate", this.attackDuration);
                    }
                    else
                    {
                        base.PlayAnimation("FullBody, Override", "GroundLight1", "GroundLight.playbackRate", this.attackDuration);
                    }
                    this.swingEffectPrefab = Assets.combo1FX;
                    this.hitEffectPrefab = Assets.hitFX;
                    this.attackSoundString = Sounds.AatroxSwordSwing;
                    this.hitSoundString = Sounds.AatroxLightSlash;
                    this.overlapAttack.damageType = DamageType.BypassArmor;
                    break;

                case AatroxGroundLight.ComboState.GroundLight2:
                    this.attackDuration = GroundLight.baseComboAttackDuration / this.attackSpeedStat;
                    this.overlapAttack = base.InitMeleeOverlap(AatroxGroundLight.comboDamageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), hitboxString);
                    if (@bool || !bool2)
                    {
                        base.PlayAnimation("Gesture, Override", "GroundLight2", "GroundLight.playbackRate", this.attackDuration);
                    }
                    else
                    {
                        base.PlayAnimation("FullBody, Override", "GroundLight2", "GroundLight.playbackRate", this.attackDuration);
                    }
                    this.swingEffectPrefab = Assets.combo2FX;
                    this.hitEffectPrefab = Assets.hitFX;
                    this.attackSoundString = Sounds.AatroxSwordSwing;
                    this.hitSoundString = Sounds.AatroxLightSlash;
                    this.overlapAttack.damageType = DamageType.BypassArmor;
                    break;

                case AatroxGroundLight.ComboState.GroundLight3:
                    this.attackDuration = GroundLight.baseFinisherAttackDuration / this.attackSpeedStat;
                    this.usedPrice = this.usingPrice;
                    float damage = AatroxGroundLight.finisherDamageCoefficient;
                    if (this.usedPrice) damage = AatroxGroundLight.priceFinisherDamageCoefficient;
                    this.overlapAttack = base.InitMeleeOverlap(damage, this.hitEffectPrefab, base.GetModelTransform(), hitboxString);
                    if (@bool || !bool2)
                    {
                        //base.PlayAnimation("Gesture, Additive", "GroundLight3", "GroundLight.playbackRate", this.attackDuration);
                        base.PlayAnimation("Gesture, Override", "GroundLight3", "GroundLight.playbackRate", this.attackDuration);
                    }
                    else
                    {
                        base.PlayAnimation("FullBody, Override", "GroundLight3", "GroundLight.playbackRate", this.attackDuration);
                    }

                    if (usedPrice)
                    {
                        this.swingEffectPrefab = Assets.downslashFX;
                        this.hitEffectPrefab = Assets.critFX;
                        this.attackSoundString = Sounds.AatroxSwordSwing;
                        this.hitSoundString = Sounds.AatroxHeavySlash;
                        this.overlapAttack.damageType = DamageType.BypassArmor;
                        this.usingPrice = true;
                    }
                    else
                    {
                        this.swingEffectPrefab = Assets.combo3FX;
                        this.hitEffectPrefab = Assets.healFX;
                        this.attackSoundString = Sounds.AatroxSwordSwing;
                        this.hitSoundString = Sounds.AatroxHeavySlash;
                        this.overlapAttack.damageType = DamageType.BypassOneShotProtection;
                        this.usingPrice = false;
                    }
                    break;
            }

            base.characterBody.SetAimTimer(this.attackDuration + 1f);

            this.overlapAttack.hitEffectPrefab = this.hitEffectPrefab;

            if (SwordmasterCheck() && SwordmasterInputCheck()) return;
        }

        public override void OnExit()
        {
            base.OnExit();
            if (this.aatroxController)
            {
                this.aatroxController.EndSkill();
                if (this.spendingHealth) this.aatroxController.SelfDamage();
            }

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.hitPauseTimer -= Time.fixedDeltaTime;

            if (base.isAuthority)
            {
                bool flag = base.FireMeleeOverlap(this.overlapAttack, this.animator, "Sword.active", 0.5f * GroundLight.forceMagnitude, true);

                this.hasHit = (this.hasHit || flag);

                if (flag)
                {
                    Util.PlaySound(this.hitSoundString, base.gameObject);

                    if (this.comboState == AatroxGroundLight.ComboState.GroundLight3 && !this.usedPrice)
                    {
                        if (base.healthComponent)
                        {
                            if (base.healthComponent.combinedHealth <= (0.25f * base.healthComponent.fullCombinedHealth))
                            {
                                Util.PlaySound(Sounds.AatroxBigHeal, base.gameObject);

                                if (this.aatroxController)
                                {
                                    this.aatroxController.AddStyle(4f);
                                }
                            }

                            if (this.aatroxController)
                            {
                                this.aatroxController.PlayHealEffect();
                                this.aatroxController.ChargeUltimate(1);
                            }
                        }
                    }

                    if (base.modelLocator)
                    {
                        Transform pos = base.modelLocator.modelBaseTransform;
                        //GameObject.Destroy(GameObject.Instantiate<GameObject>(this.hitEffectPrefab, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                    }

                    if (!this.hasHopped)
                    {
                        if (base.characterMotor && !base.characterMotor.isGrounded)
                        {
                            base.SmallHop(base.characterMotor, AatroxGroundLight.hitHopVelocity);
                        }

                        this.hasHopped = true;
                    }

                    if (this.aatroxController)
                    {
                        if (this.comboState == AatroxGroundLight.ComboState.GroundLight3) this.aatroxController.AddStyle(AatroxGroundLight.finisherStyleCoefficient / this.attackSpeedStat);
                        else this.aatroxController.AddStyle(AatroxGroundLight.styleCoefficient / this.attackSpeedStat);
                    }


                    if (!this.isInHitPause)
                    {
                        this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "GroundLight.playbackRate");
                        this.hitPauseTimer = GroundLight.hitPauseDuration / this.attackSpeedStat;
                        this.isInHitPause = true;
                    }
                }

                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    this.isInHitPause = false;
                }
            }

            if (this.stopwatch >= (this.attackDuration * 0.15f) && !this.hasSwung)
            {
                Util.PlayAttackSpeedSound(this.attackSoundString, base.gameObject, this.attackSpeedStat);
                HealthComponent healthComponent = base.characterBody.healthComponent;
                CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();

                if (healthComponent && NetworkServer.active)
                {
                    healthComponent.TakeDamageForce(GroundLight.selfForceMagnitude * component.forward, true, false);
                }

                this.hasSwung = true;

                if (base.modelLocator)
                {
                    Transform pos = base.modelLocator.modelBaseTransform;

                    GameObject.Destroy(GameObject.Instantiate<GameObject>(this.swingEffectPrefab, pos.position + (pos.forward * 0.75f) + (Vector3.up * 0.85f), pos.rotation), 1);
                }

                base.AddRecoil(-1f * AatroxGroundLight.slashRecoil, -2f * AatroxGroundLight.slashRecoil, -0.5f * AatroxGroundLight.slashRecoil, 0.5f * AatroxGroundLight.slashRecoil);

                this.animator.SetFloat("Sword.active", 1f);
                this.animator.SetFloat("SwordBig.active", 1f);
            }
            else
            {
                this.animator.SetFloat("Sword.active", 0f);
                this.animator.SetFloat("SwordBig.active", 1f);
            }

            if (!this.isInHitPause)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("GroundLight.playbackRate", 0f);
            }

            if (base.isAuthority && this.stopwatch >= this.attackDuration - this.earlyExitDuration)
            {
                if (!this.hasSwung)
                {
                    if (this.overlapAttack != null) this.overlapAttack.Fire(null);
                }

                if (base.inputBank.skill1.down && this.comboState != AatroxGroundLight.ComboState.GroundLight3)
                {
                    AatroxGroundLight groundLight = new AatroxGroundLight();
                    groundLight.comboState = this.comboState + 1;
                    this.outer.SetNextState(groundLight);
                    return;
                }

                if (this.stopwatch >= this.attackDuration)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }

            if (this.hasHit && SwordmasterCheck()) SwordmasterInputCheck();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)this.comboState);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.comboState = (AatroxGroundLight.ComboState)reader.ReadByte();
        }
    }

}
