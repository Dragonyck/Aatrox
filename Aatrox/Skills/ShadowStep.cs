using Aatrox;
using EntityStates.ImpMonster;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class AatroxShadowStep : BaseState
    {
        public static float groundBlinkDistance = 20f;
        public static float airSpeedCoefficient = 6.5f;
        public static float duration = 0.25f;

        private bool groundBlink;
        private Vector3 airVelocity;
        private Transform modelTransform;
        private float stopwatch;
        private Vector3 blinkDestination = Vector3.zero;
        private Vector3 blinkStart = Vector3.zero;
        private Animator animator;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;

        public override void OnEnter()
        {
            base.OnEnter();

            this.groundBlink = false;
            Util.PlaySound(Sounds.AatroxShadowStep, base.gameObject);
            this.modelTransform = base.GetModelTransform();

            if (this.modelTransform)
            {
                this.animator = this.modelTransform.GetComponent<Animator>();
                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }

            if (this.characterModel)
            {
                this.characterModel.invisibilityCount++;
            }

            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            this.blinkDestination = base.transform.position;
            this.blinkStart = base.transform.position;

            if (base.characterMotor)
            {
                if (base.characterMotor.isGrounded)
                {
                    this.groundBlink = true;
                    base.characterMotor.enabled = false;

                    Vector3 b = base.inputBank.moveVector * AatroxShadowStep.groundBlinkDistance;

                    NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                    NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + b, base.characterBody.hullClassification);

                    groundNodes.GetNodePosition(nodeIndex, out this.blinkDestination);

                    this.blinkDestination += base.transform.position - base.characterBody.footPosition;
                }
                else
                {
                    if (base.inputBank)
                    {
                        this.airVelocity = base.inputBank.moveVector.normalized * (this.moveSpeedStat * AatroxShadowStep.airSpeedCoefficient);
                        this.airVelocity.y = (base.inputBank.aimDirection * (this.moveSpeedStat * AatroxShadowStep.airSpeedCoefficient)).y;
                    }
                }
            }

            this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
        }

        private void CreateBlinkEffect(Vector3 origin)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.blinkDestination - this.blinkStart);
            effectData.origin = origin;
            EffectManager.SpawnEffect(BlinkState.blinkPrefab, effectData, false);
        }

        private void SetPosition(Vector3 newPosition)
        {
            if (base.characterMotor)
            {
                base.characterMotor.Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if (base.characterMotor)
            {
                if (this.groundBlink) base.characterMotor.velocity = Vector3.zero;
                else base.characterMotor.velocity = this.airVelocity;
            }

            if (this.groundBlink) this.SetPosition(Vector3.Lerp(this.blinkStart, this.blinkDestination, this.stopwatch / BlinkState.duration));

            if (this.stopwatch >= AatroxShadowStep.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(BlinkState.endSoundString, base.gameObject);

            this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
            this.modelTransform = base.GetModelTransform();

            if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;

            if (this.modelTransform && BlinkState.destealthMaterial)
            {
                /*TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.animator.gameObject);
                temporaryOverlay.duration = 1f;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = BlinkState.destealthMaterial;
                temporaryOverlay.inspectorCharacterModel = this.animator.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;*/
            }

            if (this.characterModel)
            {
                this.characterModel.invisibilityCount--;
            }

            if (this.hurtboxGroup)
            {
                HurtBoxGroup hurtBoxGroup = this.hurtboxGroup;
                int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
                hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
            }

            if (base.characterMotor)
            {
                base.characterMotor.enabled = true;
            }

            base.PlayAnimation("Gesture, Additive", "BlinkEnd");
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }

}
