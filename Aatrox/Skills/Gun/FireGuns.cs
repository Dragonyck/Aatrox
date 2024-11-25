using Aatrox;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox.Gun
{
    public class AatroxFireGuns : BaseSkillState
    {
        public string swordmasterSkillNameToken = "AATROX_SECONDARY_COMBO_NAME";
        public static GameObject bulletTracerEffectPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/Tracers/TracerEmbers");

        public float damageCoefficient = 1.3f;
        public float procCoefficient = 0.85f;
        public static float baseDuration = 0.55f;
        public float fireInterval = 0.25f;
        public static int projectileCount = 2;
        public float bulletRecoil = 0.75f;

        private float duration;
        private float fireDuration;
        private int hasFired;
        private bool usingPrice;
        private float lastFired;
        private Animator animator;
        private string muzzleString;
        private AatroxController aatroxController;

        private bool SwordmasterCheck()
        {
            if (!base.skillLocator) return false;
            if (!base.skillLocator.secondary) return false;
            if (!base.skillLocator.secondary.skillDef) return false;

            return base.skillLocator.secondary.skillDef.skillNameToken == this.swordmasterSkillNameToken;
        }

        private bool SwordmasterInputCheck()
        {
            bool flag = false;

            if (base.skillLocator)
            {
                if (base.skillLocator.secondary.stock > 0)
                {
                    if (base.inputBank.skill2.down && base.inputBank.skill1.down)
                    {
                        bool inputFlag1 = false;
                        bool inputFlag2 = false;

                        if (Input.GetKey(AatroxPlugin.swordmasterForwardKeybind.Value)) inputFlag1 = true;
                        if (Input.GetKey(AatroxPlugin.swordmasterBackKeybind.Value)) inputFlag2 = true;

                        if (inputFlag1)
                        {
                            //if (base.skillLocator) base.skillLocator.primary.skillFamily.variants[1].skillDef.activationStateMachineName = "Body";
                            this.outer.SetNextState(new GunStinger());
                            flag = true;
                            //base.skillLocator.primary.skillDef.activationStateMachineName = "Body";
                            //this.outer.SetNextState(new AatroxLunge());
                            //flag = true;
                        }
                        else if (inputFlag2)
                        {
                            bool flag1 = base.isGrounded;

                            if (flag1)
                            {
                                //if (base.skillLocator) base.skillLocator.primary.skillFamily.variants[1].skillDef.activationStateMachineName = "Body";
                                this.outer.SetNextState(new Flurry());
                                //base.skillLocator.secondary.skillDef.activationState = new SerializableEntityStateType(typeof(Flurry));
                                //base.skillLocator.secondary.skillDef.activationStateMachineName = "Body";
                                //base.skillLocator.secondary.ExecuteIfReady();
                                flag = true;
                            }
                            else
                            {
                                //base.skillLocator.primary.skillDef.activationStateMachineName = "Body";
                                this.outer.SetNextState(new Rainstorm());
                                flag = true;
                            }
                        }
                    }
                }
            }

            if (flag && base.skillLocator)
            {
                base.skillLocator.secondary.DeductStock(1);
            }

            return flag;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = AatroxFireGuns.baseDuration / this.attackSpeedStat;
            this.fireDuration = 0.2f * this.duration;
            base.characterBody.SetAimTimer(2f);
            this.animator = base.GetModelAnimator();
            this.hasFired = 0;
            this.usingPrice = false;
            this.aatroxController = base.GetComponent<AatroxController>();

            if (base.skillLocator) base.skillLocator.primary.skillFamily.variants[1].skillDef.activationStateMachineName = "Weapon";

            if (this.SwordmasterCheck() && this.SwordmasterInputCheck())
            {
                this.usingPrice = true;
                return;
            }

            base.PlayAnimation("Gun, Override", "FireGun", "FireGun.playbackRate", this.duration);

            if (this.aatroxController) this.aatroxController.skillUseCount++;
        }

        public override void OnExit()
        {
            if (this.aatroxController) this.aatroxController.EndSkill();
            base.OnExit();

        }

        private void FireBullet()
        {
            if (this.hasFired < AatroxFireGuns.projectileCount)
            {
                this.muzzleString = "MuzzleL";
                if (this.hasFired == 1) this.muzzleString = "MuzzleR";

                this.hasFired++;
                this.lastFired = Time.time + (this.fireInterval / this.attackSpeedStat);

                base.AddRecoil(-2f * this.bulletRecoil, -3f * this.bulletRecoil, -1f * this.bulletRecoil, 1f * this.bulletRecoil);
                base.characterBody.AddSpreadBloom(0.33f * this.bulletRecoil);
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, base.gameObject, this.muzzleString, false);

                Util.PlaySound(Sounds.AatroxGunshot, base.gameObject);

                if (base.isAuthority)
                {
                    float damage = this.damageCoefficient * this.damageStat;
                    float force = 10;
                    float procCoefficient = this.procCoefficient;
                    bool isCrit = base.RollCrit();

                    Ray aimRay = base.GetAimRay();

                    new BulletAttack
                    {
                        bulletCount = 1,
                        aimVector = aimRay.direction,
                        origin = aimRay.origin,
                        damage = damage,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = DamageType.BypassArmor,
                        falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                        maxDistance = 256,
                        force = force,
                        hitMask = LayerIndex.CommonMasks.bullet,
                        minSpread = 0,
                        maxSpread = 6f,
                        isCrit = isCrit,
                        owner = base.gameObject,
                        muzzleName = muzzleString,
                        smartCollision = false,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 0.75f,
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
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterBody.isSprinting = false;
            if (this.usingPrice) return;

            if (base.fixedAge >= this.fireDuration && Time.time > this.lastFired)
            {
                this.FireBullet();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
