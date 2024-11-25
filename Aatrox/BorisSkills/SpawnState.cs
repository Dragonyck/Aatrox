using Aatrox;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static Aatrox.Assets;

namespace EntityStates.Boris
{
    public class BorisSpawnState : BaseState
    {
        public static float duration = 1.5f;
        public static string spawnSoundString;
        public static GameObject spawnEffectPrefab;
        public static string spawnEffectChildString = "Spine3";
        private CameraTargetParams.AimRequest request;

        private Transform modelTransform;
        private ChildLocator childLocator;

        public override void OnEnter()
        {
            base.OnEnter();
            this.modelTransform = base.GetModelTransform();

            if (this.modelTransform) this.childLocator = this.modelTransform.GetComponent<ChildLocator>();

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
            }

            if (base.cameraTargetParams)
            {
                request = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
            }

            Util.PlaySound(Sounds.BorisSpawn, base.gameObject);

            if (this.childLocator)
            {
                Transform pos = this.childLocator.FindChild(BorisSpawnState.spawnEffectChildString);
                GameObject.Destroy(GameObject.Instantiate<GameObject>(borisSpawnFX, pos.position, pos.rotation), 32);
            }

            base.PlayAnimation("Body", "Spawn", "Spawn.playbackRate", BorisSpawnState.duration);
        }

        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            }
            base.OnExit();

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams)
            {
                //base.cameraTargetParams._currentCameraParamsData.idealLocalCameraPos = new Vector3(0f, -0.5f, -16f);
            }

            if (base.characterMotor)
            {
                if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y = 0;
            }

            if (base.fixedAge >= BorisSpawnState.duration && base.isAuthority)
            {
                if (base.cameraTargetParams)
                {
                    this.request.Dispose();
                }

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
