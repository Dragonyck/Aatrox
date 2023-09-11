using Aatrox;
using EntityStates.Aatrox;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Boris
{
    public class BorisLuce : BaseSkillState
    {
        public static float damageCoefficient = 3.5f;
        public static float projectileForce = 50f;
        public static float missileDelay = 0.3f;
        public static float missileSpawnFrequency = 0.3f;
        public static float maxSpread = 8f;
        public static string muzzleString = "Muzzle";
        public static float resourceCostFrequency = 0.25f;

        private float stopwatch;
        private float missileStopwatch;
        private float costStopwatch;
        private ChildLocator childLocator;
        private BorisController borisController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.missileStopwatch -= BorisLuce.missileDelay;
            this.borisController = GetComponent<BorisController>();

            Transform modelTransform = base.GetModelTransform();
            if (modelTransform)
            {
                this.childLocator = modelTransform.GetComponent<ChildLocator>();
            }

            Util.PlayAttackSpeedSound("ror2_mon_lunar_wisp_attack2_windUp_01", base.gameObject, 1.5f);

            base.PlayAnimation("FullBody, Override", "FireLuce", "FireLuce.playbackRate", 0.5f / this.attackSpeedStat);
        }

        private void FireBlob(Ray projectileRay, float bonusPitch, float bonusYaw)
        {
            projectileRay.direction = Util.ApplySpread(projectileRay.direction, 0f, BorisLuce.maxSpread, 1f, 1f, bonusYaw, bonusPitch);
            EffectManager.SimpleMuzzleFlash(EntityStates.GolemMonster.FireLaser.effectPrefab, base.gameObject, BorisLuce.muzzleString, false);

            //base.PlayAnimation("FullBody, Override", "FireProjectile");

            base.AddRecoil(-1f * AatroxGroundLight.slashRecoil, -2f * AatroxGroundLight.slashRecoil, -0.5f * AatroxGroundLight.slashRecoil, 0.5f * AatroxGroundLight.slashRecoil);
            Util.PlayAttackSpeedSound("ror2_mon_lunar_wisp_attack2_launch_04", base.gameObject, 1.5f);

            if (NetworkServer.active)
            {
                ProjectileManager.instance.FireProjectile(AatroxPlugin.luceProjectile, base.GetModelChildLocator().FindChild(BorisLuce.muzzleString).position, Util.QuaternionSafeLookRotation(projectileRay.direction), base.gameObject, this.damageStat * BorisLuce.damageCoefficient, BorisLuce.projectileForce, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.PlayAnimation("FullBody, Override", "BufferEmpty");

            if (base.skillLocator.special.stock <= 0)
            {
                this.outer.SetNextState(new BorisEndTransformation());
                return;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.SetAimTimer(0.2f);

            if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;

            this.stopwatch += Time.fixedDeltaTime;
            this.missileStopwatch += Time.fixedDeltaTime;
            this.costStopwatch += Time.fixedDeltaTime;

            if (this.missileStopwatch >= BorisLuce.missileSpawnFrequency / this.attackSpeedStat)
            {
                this.missileStopwatch -= BorisLuce.missileSpawnFrequency / this.attackSpeedStat;
                Transform transform = this.childLocator.FindChild(BorisLuce.muzzleString);

                if (transform)
                {
                    Ray projectileRay = default(Ray);

                    projectileRay.origin = transform.position;
                    projectileRay.direction = base.GetAimRay().direction;

                    float maxDistance = 1000f;
                    RaycastHit raycastHit;
                    if (Physics.Raycast(base.GetAimRay(), out raycastHit, maxDistance, LayerIndex.world.mask))
                    {
                        projectileRay.direction = raycastHit.point - transform.position;
                    }

                    this.FireBlob(projectileRay, 0f, 0f);
                }
            }

            if (this.costStopwatch >= BorisLuce.resourceCostFrequency)
            {
                this.costStopwatch -= BorisLuce.resourceCostFrequency;
                if (this.borisController) this.borisController.SpendResource(AatroxPlugin.borisSecondaryCost);
            }

            if (base.skillLocator.special.stock <= 0)
            {
                this.outer.SetNextStateToMain();
            }

            if (!this.inputBank.skill2.down && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }

}
