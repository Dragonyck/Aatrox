using Aatrox;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class AatroxDive : BaseState
    {
        public static float diveVelocity = 15f;
        public static float cancelHopVelocity = 8f;
        public static float impactRadius = 12f;
        public static float impactProcCoefficient = 0.8f;
        public static float impactDamageCoefficient = 5f;
        public static float knockupForce = 2000f;
        public static float stylePerEnemy = 0.15f;

        protected bool isCritAuthority;
        private bool cancelled;
        private Vector3 diveVector;
        private Animator animator;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.cancelled = false;

            base.PlayAnimation("FullBody, Override", "Dive");

            if (base.inputBank) diveVector = base.inputBank.aimDirection;

            if (diveVector.y >= 0) diveVector = Vector3.down;

            this.animator.SetFloat("Dive.downVelocity", Mathf.Abs(diveVector.y));

            if (base.characterMotor)
            {
                base.characterMotor.disableAirControlUntilCollision = true;
            }

            Util.PlaySound(Sounds.AatroxDarkFlightDive, base.gameObject);

            this.aatroxController = base.transform.root.GetComponent<AatroxController>();
            if (this.aatroxController)
            {
                aatroxController.StartDive();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.characterDirection.forward = this.diveVector;

            if (base.inputBank)
            {
                if (base.inputBank.jump.down)
                {
                    this.CancelDive();
                    return;
                }
            }

            base.characterBody.isSprinting = true;

            if (base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;

                base.characterMotor.rootMotion += this.diveVector * this.moveSpeedStat * diveVelocity * Time.fixedDeltaTime;

                base.characterDirection.forward = this.diveVector;

                if (!base.characterMotor.disableAirControlUntilCollision)
                {
                    Util.PlaySound(Sounds.AatroxDarkFlightImpact, base.gameObject);

                    base.PlayAnimation("FullBody, Override", "BufferEmpty");

                    //base.PlayAnimation("FullBody, Override", "Land");

                    Vector3 footPosition = base.characterBody.footPosition;

                    EffectManager.SpawnEffect(BeetleGuardMonster.GroundSlam.slamEffectPrefab, new EffectData
                    {
                        origin = footPosition,
                        scale = AatroxDive.impactRadius
                    }, true);

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        baseDamage = this.damageStat * AatroxDive.impactDamageCoefficient,
                        baseForce = 0f,
                        bonusForce = Vector3.up * AatroxDive.knockupForce,
                        crit = this.isCritAuthority,
                        damageType = DamageType.Stun1s,
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = AatroxDive.impactProcCoefficient,
                        radius = AatroxDive.impactRadius,
                        position = footPosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(ImpBossMonster.GroundPound.hitEffectPrefab),
                        teamIndex = base.teamComponent.teamIndex
                    }.Fire();

                    this.CalculateStyle();

                    this.outer.SetNextStateToMain();
                    return;
                }
            }
        }

        private void CalculateStyle()
        {
            int enemiesHit = 0;
            Collider[] array = Physics.OverlapSphere(base.transform.position, AatroxDive.impactRadius, LayerIndex.defaultLayer.mask);
            for (int i = 0; i < array.Length; i++)
            {
                HealthComponent component = array[i].GetComponent<HealthComponent>();
                if (component)
                {
                    TeamComponent component2 = component.GetComponent<TeamComponent>();
                    if (component2.teamIndex != TeamIndex.Player)
                    {
                        enemiesHit++;
                    }
                }
            }

            float style = enemiesHit * AatroxDive.stylePerEnemy;

            if (style > 0 && this.aatroxController) this.aatroxController.AddStyle(style);
        }

        private void CancelDive()
        {
            this.cancelled = true;

            Util.PlayAttackSpeedSound("Play_clayBruiser_attack1_windDown", base.gameObject, 1.5f);

            base.SmallHop(base.characterMotor, AatroxDive.cancelHopVelocity);
            base.characterMotor.disableAirControlUntilCollision = false;

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            this.outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            if (base.characterMotor && !this.cancelled)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            if (this.aatroxController)
            {
                this.aatroxController.EndDive();
            }

            if (!this.cancelled) base.PlayAnimation("FullBody, Override", "BufferEmpty");

            base.OnExit();
        }
    }

}
