using Aatrox;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox.Gun
{
    public class Rainstorm : BaseSkillState
    {
        public static GameObject bulletTracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerEmbers");

        public float damageCoefficient = 1.25f;
        public float baseFireInterval = 0.1f;
        public float bulletRecoil = 0.5f;
        public static float hopHeight = 22f;

        private bool shitBool;
        private float fireInterval;
        private float fireStopwatch;
        private Animator animator;
        private string muzzleString;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetAimTimer(2f);
            this.fireInterval = this.baseFireInterval / this.attackSpeedStat;
            this.animator = base.GetModelAnimator();
            this.aatroxController = base.GetComponent<AatroxController>();

            base.characterMotor.velocity *= 0.25f;
            base.SmallHop(base.characterMotor, Rainstorm.hopHeight);

            base.PlayAnimation("FullBody, Override", "FireGunDown", "FireGunDown.playbackRate", 1 / this.attackSpeedStat);
            Util.PlaySound(Sounds.AatroxRainstorm, base.gameObject);

            if (this.aatroxController) this.aatroxController.skillUseCount++;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.characterMotor.isGrounded) base.PlayAnimation("FullBody, Override", "BufferEmpty");
            else base.PlayAnimation("FullBody, Override", "RainstormExit", "FireGunDown.playbackRate", 1 / this.attackSpeedStat);

            if (this.aatroxController) this.aatroxController.EndSkill();
            //base.skillLocator.primary.skillDef.activationStateMachineName = "Weapon";
        }

        private void FireBullet()
        {
            base.AddRecoil(-2f * this.bulletRecoil, -3f * this.bulletRecoil, -1f * this.bulletRecoil, 1f * this.bulletRecoil);
            base.characterBody.AddSpreadBloom(0.33f * this.bulletRecoil);
            EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);

            Util.PlaySound(Sounds.AatroxGunshot, base.gameObject);

            if (base.isAuthority)
            {
                float damage = this.damageCoefficient * this.damageStat;
                float force = 10;
                float procCoefficient = 1f;
                bool isCrit = base.RollCrit();
                this.shitBool = !this.shitBool;

                this.muzzleString = "MuzzleL";
                if (this.shitBool) this.muzzleString = "MuzzleR";

                Vector3 pos = base.GetModelChildLocator().FindChild(this.muzzleString).position;
                Vector3 aimDir = base.GetModelChildLocator().FindChild(this.muzzleString).forward;

                if (base.fixedAge >= (10f * this.fireInterval)) aimDir = Vector3.down;

                Ray aimRay = base.GetAimRay();

                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimDir,
                    origin = pos,
                    damage = damage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = 256,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0,
                    maxSpread = 10f,
                    isCrit = isCrit,
                    owner = base.gameObject,
                    muzzleName = muzzleString,
                    smartCollision = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 0.5f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = bulletTracerEffectPrefab,
                    spreadPitchScale = 0.25f,
                    spreadYawScale = 0.25f,
                    queryTriggerInteraction = QueryTriggerInteraction.UseGlobal,
                    hitEffectPrefab = ClayBruiser.Weapon.MinigunFire.bulletHitEffectPrefab,
                    HitEffectNormal = ClayBruiser.Weapon.MinigunFire.bulletHitEffectNormal
                }.Fire();
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.StartAimMode(0.2f, false);
            this.fireStopwatch -= Time.fixedDeltaTime;
            base.skillLocator.secondary.rechargeStopwatch = 0;
            base.characterBody.isSprinting = false;

            if (this.fireStopwatch <= 0)
            {
                this.fireStopwatch = this.fireInterval;
                this.FireBullet();
            }

            if (base.characterMotor)
            {
                if (base.characterMotor.velocity.y < -5) base.characterMotor.velocity.y = -5;
            }

            if (base.inputBank)
            {
                if (base.inputBank.skill3.down && base.skillLocator.utility.CanExecute())
                {
                    this.outer.SetNextStateToMain();
                }
            }

            if (!base.inputBank.skill1.down && base.fixedAge >= (10f * this.fireInterval) && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }

            if (base.characterMotor.isGrounded && base.isAuthority)
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
