using Aatrox;
using EntityStates.Aatrox;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Boris
{
    public class BorisFireProjectile : BaseSkillState
    {
        public float damageCoefficient = 6f;
        public float baseDuration = 0.8f;
        public float recoil = 5;

        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;
        private BorisController borisController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / this.attackSpeedStat;
            this.fireDuration = 0.5f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.muzzleString = "Weapon_Tip";
            this.borisController = GetComponent<BorisController>();

            base.PlayAnimation("FullBody, Override", "FireProjectile");

            if (this.borisController) this.borisController.SpendResource(AatroxPlugin.borisSecondaryCost);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.skillLocator.special.stock <= 0)
            {
                this.outer.SetNextState(new BorisEndTransformation());
                return;
            }
        }

        private void FireBlast()
        {
            if (!this.hasFired)
            {
                this.hasFired = true;

                Ray aimRay = base.GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);
                base.AddRecoil(-1f * AatroxGroundLight.slashRecoil, -2f * AatroxGroundLight.slashRecoil, -0.5f * AatroxGroundLight.slashRecoil, 0.5f * AatroxGroundLight.slashRecoil);

                if (base.isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(AatroxPlugin.blastProjectile, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageCoefficient * this.damageStat, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
                }

                Util.PlaySound(Sounds.BorisDash, base.gameObject);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterMotor)
            {
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y = 0;
            }

            if (base.fixedAge >= this.fireDuration)
            {
                FireBlast();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }

}
