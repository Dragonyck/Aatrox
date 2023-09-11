using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Aatrox
{
    public class AatroxDownslash : BaseState
    {
        public static float baseDamageCoefficient = 7.5f;
        public static float maxDamageCoefficient = 20f;
        public static float maxDamageTime = 3f;
        public static string slashChildName = "SlashPos";
        public static string hitboxString = "SwordBig";
        public static float startHopVelocity = 16f;
        public static float dropVelocity = 40f;
        public static float styleCoefficient = 1.8f;

        protected Animator animator;
        protected float duration;
        protected float hitInterval;
        protected bool hasSwung;
        private bool hasHit;
        protected bool isJumping;
        protected float hitPauseTimer;
        protected bool isInHitPause;
        private GameObject swingEffectPrefab;
        protected OverlapAttack overlapAttack;
        protected BaseState.HitStopCachedState hitStopCachedState;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                /*base.healthComponent.TakeDamage(new DamageInfo
                {
                    damage = base.healthComponent.combinedHealth * StaticValues.skillHealthCost,
                    attacker = base.characterBody.gameObject,
                    position = base.characterBody.corePosition,
                    damageType = DamageType.NonLethal,
                    damageColorIndex = DamageColorIndex.Bleed
                });*/
            }
            this.animator = base.GetModelAnimator();

            this.duration = 2 * Uppercut.baseDuration / this.attackSpeedStat;

            this.overlapAttack = base.InitMeleeOverlap(AatroxDownslash.baseDamageCoefficient, Assets.critFX, base.GetModelTransform(), AatroxDownslash.hitboxString);
            this.overlapAttack.forceVector = 2 * Vector3.down * Uppercut.upwardForceStrength;

            this.swingEffectPrefab = Assets.downslashFX;

            this.aatroxController = base.GetComponent<AatroxController>();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            if (base.characterDirection && base.inputBank)
            {
                base.characterDirection.forward = base.inputBank.aimDirection;
            }

            if (base.characterMotor)
            {
                base.characterMotor.velocity *= 0.1f;
                base.SmallHop(base.characterMotor, AatroxDownslash.startHopVelocity);
            }

            base.PlayAnimation("FullBody, Override", "DownslashStart", "Downslash.playbackRate", 0.5f * this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            //if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Weapon";

            if (this.aatroxController) this.aatroxController.EndSkill();

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
        }

        public void RefundStock()
        {
            if (base.skillLocator && base.skillLocator.secondary.stock < base.skillLocator.secondary.maxStock) base.skillLocator.secondary.AddOneStock();
        }

        private float CalcDmg()
        {
            float dmg = 0;
            if (base.fixedAge > AatroxDownslash.maxDamageTime) dmg = AatroxDownslash.maxDamageCoefficient;
            else
            {
                float charge = base.fixedAge / AatroxDownslash.maxDamageTime;
                dmg = Util.Remap(charge, 0f, 1f, AatroxDownslash.baseDamageCoefficient, AatroxDownslash.maxDamageCoefficient);
            }

            return dmg;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.hitPauseTimer -= Time.fixedDeltaTime;

            float dmgValue = this.CalcDmg();
            this.overlapAttack.damage = dmgValue * base.damageStat;

            base.StartAimMode(0.1f, true);

            if (base.isAuthority)
            {
                if (base.fixedAge >= this.duration * 0.5f)
                {
                    if (this.animator.GetFloat("SwordBig.active") > 0.2f && !this.hasSwung)
                    {
                        this.hasSwung = true;

                        base.characterMotor.velocity = Vector3.down * AatroxDownslash.dropVelocity;
                        base.characterMotor.disableAirControlUntilCollision = true;

                        Util.PlaySound(Sounds.AatroxDownslash, base.gameObject);

                        Transform pos = base.modelLocator.modelBaseTransform;

                        GameObject tempFX = GameObject.Instantiate<GameObject>(this.swingEffectPrefab, pos.position + (pos.forward * 0.75f) + (Vector3.up * 1.5f), pos.rotation);
                        tempFX.transform.SetParent(base.modelLocator.modelBaseTransform);

                        GameObject.Destroy(tempFX, 1);

                        base.AddRecoil(-1f * AatroxGroundLight.slashRecoil, -2f * AatroxGroundLight.slashRecoil, -0.5f * AatroxGroundLight.slashRecoil, 0.5f * AatroxGroundLight.slashRecoil);

                        base.PlayAnimation("FullBody, Override", "Downslash", "Downslash.playbackRate", 0.25f);

                        if (NetworkServer.active)
                        {
                            base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                        }
                    }

                    if (this.overlapAttack != null)
                    {
                        if (base.FireMeleeOverlap(this.overlapAttack, this.animator, "SwordBig.active", 0f, false))
                        {
                            Util.PlaySound(Sounds.AatroxHeavySlash, base.gameObject);

                            RefundStock();
                            if (this.aatroxController)
                            {
                                this.aatroxController.AddStyle(AatroxDownslash.styleCoefficient);
                                this.aatroxController.ChargeUltimate(2);
                                this.aatroxController.AddMassacreStack();
                            }

                            if (base.modelLocator)
                            {
                                Transform pos = base.modelLocator.modelBaseTransform;
                                //GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.critFX, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                            }

                            if (!this.hasHit)
                            {
                                this.hasHit = true;
                            }

                            /*if (!this.isInHitPause)
                            {
                                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Uppercut.playbackRate");
                                this.hitPauseTimer = Uppercut.hitPauseDuration / this.attackSpeedStat;
                                this.isInHitPause = true;
                            }*/
                        }
                    }

                    /*if (this.hitPauseTimer <= 0f && this.isInHitPause)
                    {
                        base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                        //base.characterMotor.Motor.ForceUnground();
                        this.isInHitPause = false;
                    }*/

                    /*if (this.isInHitPause)
                    {
                        base.fixedAge -= Time.fixedDeltaTime;
                        //base.characterMotor.velocity = Vector3.zero;
                        this.hitPauseTimer -= Time.fixedDeltaTime;
                        this.animator.SetFloat("Uppercut.playbackRate", 0f);
                    }*/

                    if (!base.characterMotor.disableAirControlUntilCollision)
                    {
                        if (this.hasSwung)
                        {
                            this.hasSwung = true;
                            this.overlapAttack.Fire(null);
                        }

                        this.outer.SetNextStateToMain();
                    }
                }

                /*if (this.hasHit && base.inputBank && base.inputBank.jump.down)
                {
                    this.JumpCancel();
                }*/

                /*if (base.fixedAge >= this.duration)
                {
                    if (this.hasSwung)
                    {
                        this.hasSwung = true;
                        this.overlapAttack.Fire(null);
                    }

                    this.outer.SetNextStateToMain();
                }*/
            }
        }

        private void JumpCancel()
        {
            //Util.PlayScaledSound("Play_clayBruiser_attack1_windDown", base.gameObject, 1.5f);

            base.SmallHop(base.characterMotor, AatroxDive.cancelHopVelocity);
            base.characterMotor.disableAirControlUntilCollision = false;

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
