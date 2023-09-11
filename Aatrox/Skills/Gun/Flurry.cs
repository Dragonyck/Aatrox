using Aatrox;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox.Gun
{
    public class Flurry : BaseSkillState
    {
        public static GameObject bulletTracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerEmbers");

        public float damageCoefficient = StaticValues.flurryDamage;
        public float procCoefficient = StaticValues.flurryProcCoefficient;
        public float baseFireInterval = 0.085f;
        public float bulletRecoil = 0.45f;
        public static float baseStartDuration = 0.4f;

        private float startDuration;
        private bool shitBool;
        private bool anotherFuckinShitBool;
        private float fireInterval;
        private float fireStopwatch;
        private Animator animator;
        private string muzzleString;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            base.characterBody.SetAimTimer(2f);
            this.anotherFuckinShitBool = false;
            this.fireInterval = this.baseFireInterval / this.attackSpeedStat;
            this.startDuration = Flurry.baseStartDuration / this.attackSpeedStat;
            this.animator = base.GetModelAnimator();
            this.aatroxController = base.GetComponent<AatroxController>();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            base.PlayAnimation("FullBody, Override", "Flurry", "Flurry.playbackRate", 0);
            Util.PlaySound(Sounds.AatroxFlurryStart, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.skillLocator) base.skillLocator.primary.skillFamily.variants[1].skillDef.activationStateMachineName = "Weapon";
            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            base.PlayAnimation("Gesture, Override", "FlurryEnd");
            Util.PlaySound(Sounds.AatroxFlurryEnd, base.gameObject);

            if (this.aatroxController) this.aatroxController.EndSkill();
        }

        private void FireBullet()
        {
            this.shitBool = !this.shitBool;

            this.fireInterval = this.baseFireInterval / this.attackSpeedStat;

            base.AddRecoil(-2f * this.bulletRecoil, -3f * this.bulletRecoil, -1f * this.bulletRecoil, 1f * this.bulletRecoil);
            base.characterBody.AddSpreadBloom(0.33f * this.bulletRecoil);
            EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);

            Util.PlaySound(Sounds.AatroxGunshot, base.gameObject);

            if (base.isAuthority)
            {
                float damage = this.damageCoefficient * this.damageStat;
                float force = 100;
                float procCoefficient = this.procCoefficient;
                bool isCrit = base.RollCrit();

                this.muzzleString = "MuzzleL";
                if (this.shitBool) this.muzzleString = "MuzzleR";

                Vector3 pos = base.GetModelChildLocator().FindChild(this.muzzleString).position;

                Ray aimRay = base.GetAimRay();

                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = pos,
                    damage = damage,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = 256,
                    force = force,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = 0,
                    maxSpread = 20f,
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

            base.characterMotor.velocity = Vector3.zero;

            if (this.fireStopwatch <= 0 && this.anotherFuckinShitBool)
            {
                this.fireStopwatch = this.fireInterval;
                this.FireBullet();
            }

            if (base.fixedAge >= this.startDuration && !this.anotherFuckinShitBool)
            {
                this.anotherFuckinShitBool = true;
                base.PlayAnimation("FullBody, Override", "Flurry", "Flurry.playbackRate", 8 * this.fireInterval);
            }

            if (!base.inputBank.skill1.down && base.fixedAge >= this.fireInterval && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }

            if (base.inputBank.jump.down)
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
