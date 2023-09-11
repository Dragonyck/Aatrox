using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox.Rhaast
{
    public class RhaastUppercut : BaseState
    {
        public static float baseDamageCoefficient = 7.5f;
        public static string slashChildName = "SlashPos";
        public static string hitboxString = "Scythe";
        public static float styleCoefficient = 1.4f;
        public static float baseDuration = 0.8f;

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
            this.animator = base.GetModelAnimator();

            this.duration = Uppercut.baseDuration / this.attackSpeedStat;

            this.overlapAttack = base.InitMeleeOverlap(RhaastUppercut.baseDamageCoefficient, Assets.critFX, base.GetModelTransform(), RhaastUppercut.hitboxString);
            this.overlapAttack.forceVector = 0.8f * Vector3.up * Uppercut.upwardForceStrength;
            this.overlapAttack.damageType = DamageType.Stun1s;

            this.swingEffectPrefab = Assets.uppercutFX;

            this.aatroxController = base.GetComponent<AatroxController>();

            if (base.characterDirection && base.inputBank)
            {
                base.characterDirection.forward = base.inputBank.aimDirection;
            }

            base.PlayAnimation("FullBody, Override", "RhaastUppercut", "Uppercut.playbackRate", 2f * this.duration);
        }

        public override void OnExit()
        {
            base.OnExit();
            //base.PlayAnimation("FullBody, Override", "UppercutExit");

            base.skillLocator.primary.skillDef.activationStateMachineName = "Weapon";
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
                if (this.animator.GetFloat("Scythe.active") > 0.2f && !this.hasSwung)
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
                    if (base.FireMeleeOverlap(this.overlapAttack, this.animator, "Scythe.active", 0f, false))
                    {
                        Util.PlaySound(Sounds.AatroxHeavySlash, base.gameObject);

                        RefundStock();
                        if (this.aatroxController)
                        {
                            this.aatroxController.AddStyle(RhaastUppercut.styleCoefficient);
                            this.aatroxController.ChargeUltimate(2);
                            this.aatroxController.AddMassacreStack();
                        }

                        if (base.modelLocator)
                        {
                            Transform pos = base.modelLocator.modelBaseTransform;
                            //GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.critFX, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                        }

                        /*if (!this.isInHitPause)
                        {
                            this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Uppercut.playbackRate");
                            this.hitPauseTimer = Uppercut.hitPauseDuration / this.attackSpeedStat;
                            this.isInHitPause = true;
                        }*/
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
                    if (base.characterMotor && base.characterDirection && base.fixedAge >= (this.duration * 0.15f))
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
