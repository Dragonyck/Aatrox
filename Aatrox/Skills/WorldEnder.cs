using Aatrox;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Aatrox
{
    public class AatroxTransformation : BaseState
    {
        public static float duration = 2f;

        private float stabDuration;
        private bool hasStabbed;
        private AatroxController aatroxController;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            this.modelTransform = base.GetModelTransform();
            this.stabDuration = AatroxTransformation.duration - 0.3f;
            this.hasStabbed = false;

            this.aatroxController = base.GetComponent<AatroxController>();

            if (this.aatroxController) this.aatroxController.skillUseCount++;

            base.PlayAnimation("Fullbody, Override", "Transformation");

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            base.characterBody.hideCrosshair = true;

            if (this.aatroxController)
            {
                this.aatroxController.EquipWeapon(0);

                if (this.aatroxController.styleMeter.styleScore >= 95f) base.fixedAge += 2 * AatroxTransformation.duration;
            }

            if (base.skillLocator) base.skillLocator.special.RemoveAllStocks();
        }

        public override void OnExit()
        {

            if (NetworkServer.active)
            {
                if (base.characterBody.master)
                {
                    base.characterBody.master.bodyPrefab = AatroxPlugin.borisPrefab;
                    base.characterBody.master.Respawn(base.characterBody.transform.position, base.characterBody.transform.rotation);
                }
            }
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams)
            {
                //base.cameraTargetParams._currentCameraParamsData.idealLocalCameraPos = new Vector3(1f, -0.5f, -5.5f);
            }

            if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;

            if (base.fixedAge >= this.stabDuration && !this.hasStabbed)
            {
                this.hasStabbed = true;

                Util.PlaySound(Sounds.AatroxStabbed, base.gameObject);

                if (this.modelTransform)
                {
                    TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                    temporaryOverlay.duration = 0.5f;
                    temporaryOverlay.animateShaderAlpha = true;
                    temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                    temporaryOverlay.destroyComponentOnEnd = true;
                    temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                    temporaryOverlay.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());
                }
            }

            if (base.fixedAge >= AatroxTransformation.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }

}
