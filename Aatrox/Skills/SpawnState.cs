using Aatrox;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Aatrox
{
    public class AatroxSpawnState : BaseState
    {
        public static float duration = 1.5f;
        public static string spawnSoundString = Sounds.AatroxMassacreCancel;
        public static string spawnEffectChildString = "Spine3";

        private Transform modelTransform;
        private ChildLocator childLocator;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.modelTransform = base.GetModelTransform();
            this.aatroxController = base.GetComponent<AatroxController>();

            if (this.modelTransform) this.childLocator = this.modelTransform.GetComponent<ChildLocator>();

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            //EffectManager.SimpleMuzzleFlash(SpawnState.spawnEffectPrefab, base.gameObject, BorisSpawnState.spawnEffectChildString, false);
            Util.PlayAttackSpeedSound(spawnSoundString, base.gameObject, 0.25f);

            if (this.childLocator)
            {
                Transform pos = this.childLocator.FindChild(AatroxSpawnState.spawnEffectChildString);
                GameObject.Destroy(GameObject.Instantiate<GameObject>(Assets.borisDeathFX, pos.position, pos.rotation), 32);
            }

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 2.5f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matOnFire");
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            base.PlayAnimation("FullBody, Override", "Spawn", "Spawn.playbackRate", AatroxSpawnState.duration + 0.5f);
        }

        public override void OnExit()
        {
            base.OnExit();

            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            if (this.modelTransform)
            {
                TemporaryOverlay temporaryOverlay = this.modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = 0.25f;
                temporaryOverlay.animateShaderAlpha = true;
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                temporaryOverlay.AddToCharacerModel(this.modelTransform.GetComponent<CharacterModel>());
            }

            if (this.aatroxController) this.aatroxController.ResetWeapon();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams)
            {
                //base.cameraTargetParams._currentCameraParamsData.idealLocalCameraPos = new Vector3(0f, -0.5f, -8f);
            }

            if (base.characterMotor)
            {
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y = 0;
            }

            if (base.fixedAge >= AatroxSpawnState.duration && base.isAuthority)
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
