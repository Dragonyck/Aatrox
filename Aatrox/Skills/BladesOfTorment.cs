using Aatrox;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Aatrox
{
    public class AatroxFireProjectile : BaseState
    {
        public static float baseDuration = 0.5f;
        public static float damageCoefficient = 3.5f;
        public static float force = 25f;
        public static float recoilAmplitude = 3f;
        public static float spread = 1.5f;
        private float duration;
        private bool projectileFired;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(2f, false);
            this.aatroxController = base.GetComponent<AatroxController>();

            base.AddRecoil(-1f * AatroxFireProjectile.recoilAmplitude, -2f * AatroxFireProjectile.recoilAmplitude, -0.5f * AatroxFireProjectile.recoilAmplitude, 0.5f * AatroxFireProjectile.recoilAmplitude);

            this.duration = baseDuration / this.attackSpeedStat;

            projectileFired = false;

            base.PlayAnimation("FullBody, Override", "FireBlades");

            Util.PlaySound(Sounds.AatroxBladesOfTorment, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            string muzzleName = "Spine3";

            if (base.characterMotor)
            {
                //if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y = 0;
            }

            if (base.fixedAge >= (0.25f * this.duration) && base.isAuthority)
            {
                if (!projectileFired)
                {
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
                    EffectManager.SimpleMuzzleFlash(LemurianBruiserMonster.FireMegaFireball.muzzleflashEffectPrefab, base.gameObject, muzzleName, false);

                    Ray aimRay = base.GetAimRay();

                    Vector3 forward = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, 0, 0f);

                    ProjectileManager.instance.FireProjectile(AatroxPlugin.bladeProjectile, aimRay.origin - (spread * base.modelLocator.modelBaseTransform.right), Util.QuaternionSafeLookRotation(forward), base.gameObject, this.damageStat * damageCoefficient, force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, LemurianBruiserMonster.FireMegaFireball.projectileSpeed * 2f);
                    ProjectileManager.instance.FireProjectile(AatroxPlugin.bladeProjectile, aimRay.origin + (spread * base.modelLocator.modelBaseTransform.right), Util.QuaternionSafeLookRotation(forward), base.gameObject, this.damageStat * damageCoefficient, force, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, LemurianBruiserMonster.FireMegaFireball.projectileSpeed * 2f);

                    this.projectileFired = true;

                    if (this.aatroxController) this.aatroxController.SelfDamage();
                }
            }
            
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }


    }

}
