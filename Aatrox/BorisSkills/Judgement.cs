using Aatrox;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Boris
{
    public class BorisFinisher : BaseSkillState
    {
        public float damageCoefficient = 10f;
        public float finisherDamageCoefficient = 25f;
        public float procCoefficient = 1f;
        public float baseDuration = 4f;
        public float damageRadius = 32f;
        public static float styleCoefficient = 0.8f;
        public static float finisherStyleCoefficient = 2f;

        private float duration;
        private float fire1Duration;
        private float fire2Duration;
        private float fire3Duration;
        private float fire4Duration;
        private bool hasFired1;
        private bool hasFired2;
        private bool hasFired3;
        private bool hasFired4;
        private Animator animator;
        private Transform modelTransform;
        private ChildLocator childLocator;
        private BorisController borisController;
        private CameraTargetParams.AimRequest request;

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration;
            this.fire1Duration = this.baseDuration * 0.4f;
            this.fire2Duration = this.baseDuration * 0.6f;
            this.fire3Duration = this.baseDuration * 0.8f;
            this.fire4Duration = this.baseDuration * 0.75f;
            base.characterBody.SetAimTimer(5f);
            this.animator = base.GetModelAnimator();
            this.modelTransform = base.GetModelTransform();
            this.borisController = base.GetComponent<BorisController>();

            base.characterMotor.mass = 1500f;

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            if (base.cameraTargetParams)
            {
                request = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            if (this.modelTransform) this.childLocator = this.modelTransform.GetComponent<ChildLocator>();

            base.PlayAnimation("FullBody, Override", "Judgement", "Judgement.playbackRate", this.duration);

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.5f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            if (this.childLocator)
            {
                Transform chestPos = this.childLocator.FindChild("Spine3");
                GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisJudgementStartFX, chestPos.position, chestPos.rotation), 32);
            }

            //if (base.skillLocator) base.skillLocator.special.RemoveAllStocks();

            Util.PlaySound(Sounds.BorisJudgementStart, base.gameObject);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.cameraTargetParams)
            {
                this.request.Dispose();
            }
        }

        private void FireBlast1()
        {
            if (!this.hasFired1)
            {
                this.hasFired1 = true;

                if (this.borisController) this.borisController.AddStyle(BorisFinisher.styleCoefficient);

                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.BorisSwing, base.gameObject);

                    Vector3 blastPosition = this.childLocator.FindChild("Weapon_Tip").position;

                    /*EffectManager.SpawnEffect(BeetleGuardMonster.GroundSlam.slamEffectPrefab, new EffectData
                    {
                        origin = blastPosition,
                        scale = AatroxDive.impactRadius
                    }, true);*/

                    if (this.childLocator)
                    {
                        Transform slashPos = this.childLocator.FindChild("SlashPos");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisCombo1FX, slashPos.position, slashPos.rotation), 1);

                        Transform effectPos = this.childLocator.FindChild("Weapon_Tip");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisJudgementFX, effectPos), 16);
                    }

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        baseDamage = this.damageStat * this.damageCoefficient,
                        baseForce = 0f,
                        bonusForce = Vector3.zero,
                        crit = this.RollCrit(),
                        damageType = DamageType.Stun1s,
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = this.procCoefficient,
                        radius = this.damageRadius,
                        position = blastPosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(ImpBossMonster.GroundPound.hitEffectPrefab),
                        teamIndex = base.teamComponent.teamIndex
                    }.Fire();
                }
            }
        }

        private void FireBlast2()
        {
            if (!this.hasFired2)
            {
                this.hasFired2 = true;

                if (this.borisController) this.borisController.AddStyle(BorisFinisher.styleCoefficient);

                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.BorisThrust, base.gameObject);

                    Vector3 blastPosition = this.childLocator.FindChild("Weapon_Tip").position;

                    if (this.childLocator)
                    {
                        Transform slashPos = this.childLocator.FindChild("SlashPos");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisCombo2FX, slashPos.position, slashPos.rotation), 1);

                        Transform effectPos = this.childLocator.FindChild("Weapon_Tip");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisJudgementFX, effectPos), 16);
                    }

                    /*EffectManager.SpawnEffect(BeetleGuardMonster.GroundSlam.slamEffectPrefab, new EffectData
                    {
                        origin = blastPosition,
                        scale = AatroxDive.impactRadius
                    }, true);*/

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        baseDamage = this.damageStat * this.damageCoefficient,
                        baseForce = 0f,
                        bonusForce = Vector3.zero,
                        crit = this.RollCrit(),
                        damageType = DamageType.Stun1s,
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = this.procCoefficient,
                        radius = this.damageRadius,
                        position = blastPosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(ImpBossMonster.GroundPound.hitEffectPrefab),
                        teamIndex = base.teamComponent.teamIndex
                    }.Fire();
                }
            }
        }

        private void FireBlast3()
        {
            if (!this.hasFired3)
            {
                this.hasFired3 = true;

                if (this.borisController) this.borisController.AddStyle(BorisFinisher.styleCoefficient);

                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.BorisSwing, base.gameObject);

                    Vector3 blastPosition = this.childLocator.FindChild("Weapon_Tip").position;

                    /*EffectManager.SpawnEffect(BeetleGuardMonster.GroundSlam.slamEffectPrefab, new EffectData
                    {
                        origin = blastPosition,
                        scale = AatroxDive.impactRadius
                    }, true);*/

                    if (this.childLocator)
                    {
                        Transform slashPos = this.childLocator.FindChild("SlashPos");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisCombo1FX, slashPos.position, slashPos.rotation), 1);

                        Transform effectPos = this.childLocator.FindChild("Weapon_Tip");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisJudgementFX, effectPos), 16);
                    }

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        baseDamage = this.damageStat * this.damageCoefficient,
                        baseForce = 0f,
                        bonusForce = Vector3.zero,
                        crit = this.RollCrit(),
                        damageType = DamageType.Stun1s,
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = this.procCoefficient,
                        radius = this.damageRadius,
                        position = blastPosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(ImpBossMonster.GroundPound.hitEffectPrefab),
                        teamIndex = base.teamComponent.teamIndex
                    }.Fire();
                }
            }
        }

        private void FireBlast4()
        {
            if (!this.hasFired4)
            {
                this.hasFired4 = true;

                if (base.isAuthority)
                {
                    Util.PlaySound(Sounds.BorisDash, base.gameObject);
                }

                if (this.modelTransform)
                {
                    TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = 1.5f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire");
                    temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.characterMotor)
            {
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y = 0;
            }

            if (base.fixedAge >= this.fire1Duration)
            {
                FireBlast1();
            }

            if (base.fixedAge >= this.fire2Duration)
            {
                FireBlast2();
            }

            if (base.fixedAge >= this.fire3Duration)
            {
                FireBlast3();
            }

            if (base.fixedAge >= this.fire4Duration)
            {
                FireBlast4();
            }

            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                if (base.isAuthority)
                {
                    Vector3 blastPosition = this.childLocator.FindChild("Spine3").position;

                    new BlastAttack
                    {
                        attacker = base.gameObject,
                        baseDamage = this.damageStat * this.finisherDamageCoefficient,
                        baseForce = 0f,
                        bonusForce = Vector3.zero,
                        crit = this.RollCrit(),
                        damageType = DamageType.BypassArmor,
                        falloffModel = BlastAttack.FalloffModel.None,
                        procCoefficient = this.procCoefficient,
                        radius = this.damageRadius * 1.5f,
                        position = blastPosition,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        impactEffect = EffectCatalog.FindEffectIndexFromPrefab(ImpBossMonster.GroundPound.hitEffectPrefab),
                        teamIndex = base.teamComponent.teamIndex
                    }.Fire();

                    if (this.childLocator)
                    {
                        Transform pos = this.childLocator.FindChild("Spine3");
                        GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisExplosionFX, pos.position, pos.rotation), 32);
                    }

                    if (this.borisController) this.borisController.AddStyle(BorisFinisher.finisherStyleCoefficient);
                }

                if (base.skillLocator) base.skillLocator.special.RemoveAllStocks();

                Util.PlaySound(Sounds.BorisJudgementExplosion, base.gameObject);
                this.outer.SetNextState(new BorisEndTransformation());
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

}
