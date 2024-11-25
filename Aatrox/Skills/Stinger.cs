using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Aatrox.Assets;

namespace EntityStates.Aatrox
{
    public class AatroxLunge : BaseState
    {
        public bool hasHit { get; private set; }
        public int dashIndex { private get; set; }

        public static float dashPrepDuration = 0.15f;
        public static float dashDuration = 0.3f;
        public static float speedCoefficient = 5.5f;
        public static float damageCoefficient = 1.5f;
        public static float procCoefficient = 0.75f;
        public static float smallHopVelocity = 12f;
        public static float styleCoefficient = 0.8f;

        private Transform modelTransform;
        private float stopwatch;
        private Vector3 dashVector = Vector3.zero;
        private Animator animator;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        private OverlapAttack overlapAttack;
        //private ChildLocator childLocator;
        private bool isDashing;
        private bool inHitPause;
        private float hitPauseTimer;
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
            Util.PlaySound(Sounds.AatroxStinger, base.gameObject);

            this.modelTransform = base.GetModelTransform();
            this.aatroxController = base.GetComponent<AatroxController>();
            aatroxController.SelfDamage();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            /*if (base.cameraTargetParams)
            {
                base.cameraTargetParams.aimMode = CameraTargetParams.AimType.Aura;
            }*/

            if (base.characterMotor) base.characterMotor.disableAirControlUntilCollision = false;

            if (this.modelTransform)
            {
                this.animator = this.modelTransform.GetComponent<Animator>();
                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                //this.childLocator = this.modelTransform.GetComponent<ChildLocator>();
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();

                /*if (this.childLocator)
                {
                    this.childLocator.FindChild("PreDashEffect").gameObject.SetActive(true);
                }*/
            }

            base.PlayAnimation("FullBody, Override", "AssaulterPrep", "AssaulterPrep.playbackRate", AatroxLunge.dashPrepDuration);
            //base.PlayAnimation("Gesture, Override", "Stinger", "AssaulterPrep.playbackRate", AatroxLunge.dashPrepDuration);
            this.dashVector = base.inputBank.aimDirection;
            this.dashVector.y = 0;

            this.overlapAttack = base.InitMeleeOverlap(AatroxLunge.damageCoefficient, critFX, this.modelTransform, "Sword");
            this.overlapAttack.damageType = DamageType.BonusToLowHealth;

            /*if (NetworkServer.active)
            {
                base.characterBody.AddBuff(BuffIndex.HiddenInvincibility);
            }*/
        }

        /*private void CreateDashEffect()
        {
            Transform transform = this.childLocator.FindChild("DashCenter");
            if (transform && Assaulter.dashPrefab)
            {
                UnityEngine.Object.Instantiate<GameObject>(Assaulter.dashPrefab, transform.position, Util.QuaternionSafeLookRotation(this.dashVector), transform);
            }
            if (this.childLocator)
            {
                this.childLocator.FindChild("PreDashEffect").gameObject.SetActive(false);
            }
        }*/

        public void RefundStock()
        {
            if (base.skillLocator && base.skillLocator.secondary.stock < base.skillLocator.secondary.maxStock) base.skillLocator.secondary.AddOneStock();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterDirection.forward = this.dashVector;
            base.characterBody.isSprinting = true;

            if (this.stopwatch > AatroxLunge.dashPrepDuration / this.attackSpeedStat && !this.isDashing)
            {
                this.isDashing = true;
                this.dashVector = base.inputBank.aimDirection;
                //this.CreateDashEffect();
                base.PlayCrossfade("FullBody, Override", "AssaulterLoop", 0.1f);

                base.gameObject.layer = LayerIndex.fakeActor.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();

                if (this.modelTransform)
                {
                    TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                    temporaryOverlay.duration = 0.7f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire");
                    temporaryOverlay.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());
                }

                Util.PlaySound(Sounds.AatroxStinger, base.gameObject);
            }

            if (!this.isDashing)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else if (base.isAuthority)
            {
                base.characterMotor.velocity = Vector3.zero;

                if (!this.inHitPause)
                {
                    bool flag = this.overlapAttack.Fire();
                    this.stopwatch += Time.fixedDeltaTime;

                    if (flag)
                    {
                        if (!this.hasHit)
                        {
                            this.hasHit = true;
                            RefundStock();
                            if (this.aatroxController)
                            {
                                this.aatroxController.AddStyle(AatroxLunge.styleCoefficient);
                                this.aatroxController.ChargeUltimate(1);
                            }
                        }

                        /*if (base.modelLocator)
                        {
                            Transform pos = base.modelLocator.modelBaseTransform;
                            GameObject.Destroy(GameObject.Instantiate<GameObject>(critFX, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                        }*/

                        Util.PlaySound(Sounds.AatroxHeavySlash, base.gameObject);

                        this.inHitPause = true;
                        this.hitPauseTimer = Assaulter.hitPauseDuration / this.attackSpeedStat;


                        if (this.modelTransform)
                        {
                            TemporaryOverlayInstance temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                            temporaryOverlay2.duration = Assaulter.hitPauseDuration / this.attackSpeedStat;
                            temporaryOverlay2.animateShaderAlpha = true;
                            temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                            temporaryOverlay2.destroyComponentOnEnd = true;
                            temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                            temporaryOverlay2.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());
                        }

                        stopwatch = AatroxLunge.dashDuration + AatroxLunge.dashPrepDuration / this.attackSpeedStat;
                        this.dashVector = Vector3.zero;
                    }

                    base.characterMotor.rootMotion += this.dashVector * this.moveSpeedStat * AatroxLunge.speedCoefficient * Time.fixedDeltaTime;
                }
                else
                {
                    this.hitPauseTimer -= Time.fixedDeltaTime;

                    if (this.hitPauseTimer < 0f)
                    {
                        this.inHitPause = false;
                    }
                }
            }

            if (this.stopwatch >= AatroxLunge.dashDuration + AatroxLunge.dashPrepDuration / this.attackSpeedStat && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.characterMotor.disableAirControlUntilCollision = false;

            if (base.characterMotor.isGrounded) base.PlayAnimation("FullBody, Override", "BufferEmpty");
            else base.PlayCrossfade("FullBody, Override", "StingerHop", 0.2f);

            //if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Weapon";

            if (base.isAuthority)
            {
                base.characterMotor.velocity *= 0.1f;

                if (!base.isGrounded && this.hasHit)
                {
                    base.SmallHop(base.characterMotor, AatroxLunge.smallHopVelocity);
                }

                if (Input.GetAxis("Vertical") > 0 && base.inputBank.skill1.down && base.inputBank.skill2.down && this.hasHit)
                {
                    //if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Body";
                    this.outer.SetNextState(new AatroxLungeFollowup());
                }
            }

            if (this.aatroxController) this.aatroxController.EndSkill();

            /*if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(BuffIndex.HiddenInvincibility);
            }*/

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}