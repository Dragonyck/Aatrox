﻿using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Aatrox.Assets;

namespace EntityStates.Aatrox
{
    public class AatroxUppercut : BaseState
    {
        public static float baseDamageCoefficient = 5f;
        public static string slashChildName = "SlashPos";
        public static string hitboxString = "SwordBig";
        public static float styleCoefficient = 1.2f;

        protected Animator animator;
        protected float duration;
        protected float hitInterval;
        protected bool hasSwung;
        protected float hitPauseTimer;
        protected bool isInHitPause;
        private GameObject swingEffectPrefab;
        protected OverlapAttack overlapAttack;
        protected BaseState.HitStopCachedState hitStopCachedState;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            /*if (NetworkServer.active)
            {
                base.healthComponent.TakeDamage(new DamageInfo
                {
                    damage = base.healthComponent.combinedHealth * StaticValues.skillHealthCost,
                    attacker = base.characterBody.gameObject,
                    position = base.characterBody.corePosition,
                    damageType = DamageType.NonLethal,
                    damageColorIndex = DamageColorIndex.Bleed
                });
            }*/
            this.animator = base.GetModelAnimator();

            this.duration = Uppercut.baseDuration / this.attackSpeedStat;

            this.overlapAttack = base.InitMeleeOverlap(AatroxUppercut.baseDamageCoefficient, critFX, base.GetModelTransform(), AatroxUppercut.hitboxString);
            this.overlapAttack.forceVector = 0.8f * Vector3.up * Uppercut.upwardForceStrength;
            this.overlapAttack.damageType = DamageType.Stun1s;

            this.swingEffectPrefab = uppercutFX;

            this.aatroxController = base.GetComponent<AatroxController>();
            aatroxController.SelfDamage();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            if (base.characterDirection && base.inputBank)
            {
                base.characterDirection.forward = base.inputBank.aimDirection;
            }

            this.PlayAnim();
        }

        protected virtual void PlayAnim()
        {
            base.PlayAnimation("FullBody, Override", "Uppercut", "Uppercut.playbackRate", this.duration);
        }

        public override void OnExit()
        {
            //base.PlayAnimation("FullBody, Override", "UppercutExit");

            //if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Weapon";

            if (this.aatroxController) this.aatroxController.EndSkill();
            base.OnExit();
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
                if (this.animator.GetFloat("SwordBig.active") > 0.2f && !this.hasSwung)
                {
                    this.hasSwung = true;

                    Util.PlaySound(Sounds.AatroxUppercut, base.gameObject);

                    Transform pos = base.modelLocator.modelBaseTransform;

                    GameObject tempFX = GameObject.Instantiate<GameObject>(this.swingEffectPrefab, pos.position + (pos.forward * 1.25f) + (Vector3.up * 1.15f), pos.rotation);
                    tempFX.transform.SetParent(base.modelLocator.modelBaseTransform);

                    GameObject.Destroy(tempFX, 1);
                }

                if (this.overlapAttack != null)
                {
                    if (base.FireMeleeOverlap(this.overlapAttack, this.animator, "SwordBig.active", 0f, false))
                    {
                        Util.PlaySound(Sounds.AatroxHeavySlash, base.gameObject);

                        this.RefundStock();
                        if (this.aatroxController)
                        {
                            this.aatroxController.AddStyle(AatroxUppercut.styleCoefficient);
                            this.aatroxController.ChargeUltimate(2);
                            this.aatroxController.AddMassacreStack();
                        }

                        if (base.modelLocator)
                        {
                            Transform pos = base.modelLocator.modelBaseTransform;
                            //GameObject.Destroy(GameObject.Instantiate<GameObject>(critFX, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                        }

                        if (!this.isInHitPause)
                        {
                            this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Uppercut.playbackRate");
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
                    if (base.characterMotor && base.characterDirection)
                    {
                        Vector3 velocity = base.characterDirection.forward * this.moveSpeedStat * Mathf.Lerp(Uppercut.moveSpeedBonusCoefficient, 0f, base.age / this.duration);
                        velocity.y = Uppercut.yVelocityCurve.Evaluate(base.fixedAge / this.duration);

                        base.characterMotor.velocity = velocity;
                    }
                }
                else
                {
                    base.fixedAge -= Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    this.hitPauseTimer -= Time.fixedDeltaTime;
                    this.animator.SetFloat("Uppercut.playbackRate", 0f);
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
