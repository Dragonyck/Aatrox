using Aatrox;
using EntityStates.Aatrox;
using EntityStates.ImpMonster;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.Boris
{
    public class BorisBlink : BaseState
    {
        public static float groundBlinkDistance = 75f;
        public static float airBlinkDistance = 75f;
        public static float duration = 0.1f;

        private Transform modelTransform;
        private float stopwatch;
        private Vector3 blinkDestination = Vector3.zero;
        private Vector3 blinkStart = Vector3.zero;
        private Animator animator;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        private BorisController borisController;

        public override void OnEnter()
        {
            base.OnEnter();

            Util.PlaySound(Sounds.BorisBlink, base.gameObject);
            this.modelTransform = base.GetModelTransform();

            this.borisController = GetComponent<BorisController>();

            if (this.borisController) this.borisController.SpendResource(AatroxPlugin.borisUtilityCost);

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
                base.characterMotor.enabled = false;

                if (base.characterMotor.isGrounded)
                {
                    Vector3 b = base.inputBank.moveVector * AatroxShadowStep.groundBlinkDistance;

                    NodeGraph groundNodes = SceneInfo.instance.groundNodes;
                    NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + b, base.characterBody.hullClassification);

                    groundNodes.GetNodePosition(nodeIndex, out this.blinkDestination);

                    this.blinkDestination += base.transform.position - base.characterBody.footPosition;
                }
                else
                {
                    Vector3 b = base.inputBank.aimDirection * AatroxShadowStep.groundBlinkDistance;

                    float maxDistance = airBlinkDistance;

                    RaycastHit raycastHit;

                    bool flag = Physics.Raycast(base.GetAimRay(), out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet);

                    if (flag)
                    {
                        this.blinkDestination = transform.position = raycastHit.point - (base.inputBank.aimDirection * 2f) + new Vector3(0, 0.5f, 0);
                    }
                    else
                    {
                        this.blinkDestination += b + new Vector3(0, 0.75f, 0) - (0.1f * b);
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

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            this.SetPosition(Vector3.Lerp(this.blinkStart, this.blinkDestination, this.stopwatch / BorisBlink.duration));

            if (this.stopwatch >= BorisBlink.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(BlinkState.endSoundString, base.gameObject);

            this.CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
            this.modelTransform = base.GetModelTransform();

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

            if (base.skillLocator.special.stock <= 0)
            {
                this.outer.SetNextState(new BorisEndTransformation());
                return;
            }

            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
