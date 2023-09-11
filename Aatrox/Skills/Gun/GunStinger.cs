using Aatrox;
using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox.Gun
{
    public class GunStinger : BaseSkillState
    {
        public static float dashPrepDuration = 0.05f;
        public static float dashDuration = 0.3f;
        public static float speedCoefficient = 6.5f;

        private Transform modelTransform;
        private float stopwatch;
        private Vector3 dashVector = Vector3.zero;
        private Animator animator;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        //private ChildLocator childLocator;
        private bool isDashing;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlayAttackSpeedSound(Sounds.BorisDash, base.gameObject, 1.5f);

            this.modelTransform = base.GetModelTransform();
            this.aatroxController = base.GetComponent<AatroxController>();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            if (base.characterMotor) base.characterMotor.disableAirControlUntilCollision = false;

            if (this.modelTransform)
            {
                this.animator = this.modelTransform.GetComponent<Animator>();
                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }

            base.PlayAnimation("FullBody, Override", "GunStinger");
            this.dashVector = base.inputBank.aimDirection;
            this.dashVector.y = 0;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            base.characterDirection.forward = this.dashVector;
            base.characterBody.isSprinting = true;
            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch > GunStinger.dashPrepDuration / this.attackSpeedStat && !this.isDashing)
            {
                this.isDashing = true;
                this.dashVector = base.inputBank.aimDirection;

                base.gameObject.layer = LayerIndex.fakeActor.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();

                if (this.modelTransform)
                {
                    TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                    temporaryOverlay.duration = 0.7f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire");
                    temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
                }
            }

            if (base.isAuthority && this.isDashing)
            {
                base.characterMotor.velocity = Vector3.zero;
                base.characterMotor.rootMotion += this.dashVector * this.moveSpeedStat * AatroxLunge.speedCoefficient * Time.fixedDeltaTime;
            }

            if (this.stopwatch >= GunStinger.dashDuration + GunStinger.dashPrepDuration / this.attackSpeedStat && base.isAuthority)
            {
                if (base.inputBank.skill1.down)
                {
                    if (base.characterMotor.isGrounded)
                    {
                        this.outer.SetNextState(new Flurry());
                    }
                    else
                    {
                        this.outer.SetNextState(new Rainstorm());
                    }
                }
                else
                {
                    this.outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.characterMotor.disableAirControlUntilCollision = false;

            if (base.characterMotor.isGrounded) base.PlayAnimation("FullBody, Override", "BufferEmpty");
            else base.PlayCrossfade("FullBody, Override", "StingerHop", 0.2f);

            if (base.skillLocator) base.skillLocator.primary.stateMachine.customName = "Weapon";

            if (base.isAuthority)
            {
                base.characterMotor.velocity *= 0.1f;
            }

            if (this.aatroxController) this.aatroxController.EndSkill();

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}