using Aatrox;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Aatrox.Assets;

namespace EntityStates.Aatrox
{
    public class AatroxBeginFlight : BaseState
    {
        public float basePrepDuration = 0.2f;
        public float jumpDuration = 0.5f;
        public float jumpCoefficient = 250f;

        public float maxJumpDuration = 0.6f;
        public float minJumpDuration = 0.3f;

        private Transform modelTransform;
        [SerializeField]
        public Vector3 jumpVector;
        private float prepDuration;
        private bool beginJump;
        private CharacterModel characterModel;
        private HurtBoxGroup hurtboxGroup;
        public static CameraTargetParams.AimRequest request;

        public override void OnEnter()
        {
            base.OnEnter();
            this.modelTransform = base.GetModelTransform();

            if (this.modelTransform)
            {
                this.characterModel = this.modelTransform.GetComponent<CharacterModel>();
                this.hurtboxGroup = this.modelTransform.GetComponent<HurtBoxGroup>();
            }

            this.prepDuration = this.basePrepDuration / this.attackSpeedStat;

            base.PlayAnimation("FullBody, Override", "BeginFlight");

            if (base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            if (base.cameraTargetParams)
            {
                request = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            if (this.modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                temporaryOverlay.duration = 0.6f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire");
                temporaryOverlay.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());

                TemporaryOverlayInstance temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                temporaryOverlay2.duration = 0.7f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                temporaryOverlay2.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            //Vector3 direction = base.GetAimRay().direction;
            //direction.y = 0f;
            //direction.Normalize();

            //Vector3 up = Vector3.up;

            //this.worldBlinkVector = Matrix4x4.TRS(base.transform.position, Util.QuaternionSafeLookRotation(direction, up), new Vector3(1f, 1f, 1f)).MultiplyPoint3x4(this.blinkVector) - base.transform.position;
            //this.worldBlinkVector.Normalize();
        }

        private void CreateFlightEffect(Vector3 origin)
        {
            GameObject.Destroy(GameObject.Instantiate<GameObject>(flightFX, origin, Quaternion.Euler(Vector3.zero)), 5);
            /*EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(this.worldBlinkVector);
            effectData.origin = origin;
            EffectManager.SpawnEffect(Huntress.BaseBeginArrowBarrage.blinkPrefab, effectData, false);*/
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge >= this.prepDuration && !this.beginJump)
            {
                this.beginJump = true;

                base.GetModelAnimator().SetBool("isGrounded", false);

                base.PlayAnimation("FullBody, Override", "Flight");

                Util.PlaySound(Sounds.AatroxDarkFlightStart, base.gameObject);

                this.CreateFlightEffect(base.transform.position);
            }

            if (this.beginJump && base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.up * (base.characterBody.jumpPower * this.jumpCoefficient * Time.fixedDeltaTime);
                //base.characterMotor.rootMotion += Vector3.up * (base.characterBody.jumpPower * this.jumpCoefficient * Time.fixedDeltaTime);
            }

            bool flag = (base.fixedAge >= this.minJumpDuration + this.prepDuration && base.isAuthority && !inputBank.skill3.down) || (base.fixedAge >= this.maxJumpDuration + this.prepDuration && base.isAuthority);

            if (flag)
            {
                AatroxAimDive nextState = new AatroxAimDive();
                nextState.SetAimRequest(request);
                this.outer.SetNextState(nextState);
            }
        }

        public override void OnExit()
        {
            //this.CreateBlinkEffect(base.transform.position);
            this.modelTransform = base.GetModelTransform();

            if (this.modelTransform)
            {
                TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                temporaryOverlay.duration = 0.6f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire");
                temporaryOverlay.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());

                TemporaryOverlayInstance temporaryOverlay2 = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                temporaryOverlay2.duration = 0.7f;
                temporaryOverlay2.animateShaderAlpha = true;
                temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay2.destroyComponentOnEnd = true;
                temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                temporaryOverlay2.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            base.OnExit();
        }
    }

}
