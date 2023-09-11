using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class AatroxLungeFollowup : BaseSkillState
    {
        public static float baseDamageCoefficient = 6f;
        public static string slashChildName = "SlashPos";
        public static string hitboxString = "SwordBig";
        public static float styleCoefficient = 1.25f;
        public static float baseDuration = 0.85f;

        protected Animator animator;
        protected float duration;
        protected float slashDuration = 0.3f;
        protected float slashTime;
        protected float hitInterval;
        protected bool hasSwung;
        protected float hitPauseTimer;
        protected bool isInHitPause;
        private Vector3 forwardDirection;
        private GameObject swingEffectPrefab;
        protected OverlapAttack overlapAttack;
        protected BaseState.HitStopCachedState hitStopCachedState;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.duration = AatroxLungeFollowup.baseDuration / this.attackSpeedStat;
            this.slashTime = this.duration * 0.25f;

            this.overlapAttack = base.InitMeleeOverlap(AatroxLungeFollowup.baseDamageCoefficient, Assets.critFX, base.GetModelTransform(), AatroxLungeFollowup.hitboxString);
            this.overlapAttack.damageType = DamageType.Generic;

            this.swingEffectPrefab = Assets.lungeFX;

            this.aatroxController = base.GetComponent<AatroxController>();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            if (base.inputBank)
            {
                this.forwardDirection = base.inputBank.aimDirection;
            }

            base.PlayAnimation("FullBody, Override", "StingerFollowup", "StingerFollowup.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();

            //if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Weapon";
            if (this.aatroxController) this.aatroxController.EndSkill();
        }

        public void RefundStock()
        {
            if (base.skillLocator && base.skillLocator.secondary.stock < base.skillLocator.secondary.maxStock) base.skillLocator.secondary.AddOneStock();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.hitPauseTimer -= Time.fixedDeltaTime;

            if (base.isAuthority)
            {
                if (base.fixedAge <= this.slashTime && base.characterMotor) base.characterMotor.velocity = Vector3.zero;

                if (base.fixedAge >= this.slashTime && base.fixedAge <= (this.slashTime + this.slashDuration))
                {
                    this.animator.SetFloat("SwordBig.active", 1);
                }
                else this.animator.SetFloat("SwordBig.active", 0);

                if (this.animator.GetFloat("SwordBig.active") > 0.2f && !this.hasSwung)
                {
                    this.hasSwung = true;

                    Util.PlaySound(Sounds.AatroxSwordSwing, base.gameObject);
                    if (base.modelLocator)
                    {
                        Transform pos = base.modelLocator.modelBaseTransform;

                        GameObject.Destroy(GameObject.Instantiate<GameObject>(this.swingEffectPrefab, pos.position + (pos.forward * 0.75f) + (Vector3.up * 0.85f), pos.rotation), 1);
                    }

                    base.AddRecoil(-1f * AatroxGroundLight.slashRecoil, -2f * AatroxGroundLight.slashRecoil, -0.5f * AatroxGroundLight.slashRecoil, 0.5f * AatroxGroundLight.slashRecoil);
                }

                if (this.overlapAttack != null)
                {
                    if (base.FireMeleeOverlap(this.overlapAttack, this.animator, "SwordBig.active", 0f, false))
                    {
                        Util.PlaySound(Sounds.AatroxHeavySlash, base.gameObject);

                        if (this.aatroxController)
                        {
                            this.aatroxController.AddStyle(AatroxLungeFollowup.styleCoefficient);
                            this.aatroxController.ChargeUltimate(2);
                            this.aatroxController.AddMassacreStack();
                        }

                        if (base.modelLocator)
                        {
                            Transform pos = base.modelLocator.modelBaseTransform;
                            //GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.critFX, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                        }

                        this.RefundStock();

                        if (!this.isInHitPause)
                        {
                            this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "StingerFollowup.playbackRate");
                            this.hitPauseTimer = Uppercut.hitPauseDuration / this.attackSpeedStat;
                            this.isInHitPause = true;
                        }
                    }
                }

                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    base.characterMotor.Motor.ForceUnground();
                    this.isInHitPause = false;
                }

                if (!this.isInHitPause)
                {
                    if (base.characterMotor && base.characterDirection && base.fixedAge >= this.slashTime)
                    {
                        Vector3 velocity = -this.forwardDirection * this.moveSpeedStat * Mathf.Lerp(1.5f * Uppercut.moveSpeedBonusCoefficient, 0f, base.age / this.duration);

                        base.characterMotor.velocity = velocity;
                    }
                }
                else
                {
                    base.fixedAge -= Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    this.animator.SetFloat("StingerFollowup.playbackRate", 0f);
                }

                if (base.fixedAge >= this.duration)
                {
                    if (this.hasSwung)
                    {
                        this.hasSwung = true;
                        this.overlapAttack.Fire(null);
                    }

                    this.outer.SetNextStateToMain();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}
