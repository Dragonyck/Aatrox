using Aatrox;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Aatrox
{
    public class AatroxMassacre : BaseState
    {
        //public float duration = 0.25f;

        //private float stopwatch;
        private AatroxController aatroxController;
        private Transform modelTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            this.modelTransform = base.GetModelTransform();

            this.aatroxController = base.GetComponent<AatroxController>();
            if (this.aatroxController)
            {
                if (aatroxController.CastMassacre())
                {
                    if (NetworkServer.active && base.characterBody)
                    {
                        base.characterBody.AddBuff(AatroxPlugin.massacreBuff);
                        base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 0.5f);
                    }

                    //Util.PlaySound(EntityStates.ImpBossMonster.SpawnState.spawnSoundString, base.gameObject);
                    Util.PlaySound(Sounds.AatroxMassacreCast, base.gameObject);
                    //Util.PlaySound(Sounds.AatroxMassacreLoop, base.gameObject);

                    base.PlayAnimation("Gesture, Override", "Massacre", "", 0.25f);

                    if (this.modelTransform)
                    {
                        TemporaryOverlayInstance temporaryOverlay = TemporaryOverlayManager.AddOverlay(this.modelTransform.gameObject);
                        temporaryOverlay.duration = 1.5f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matDoppelganger");
                        temporaryOverlay.AddToCharacterModel(this.modelTransform.GetComponent<CharacterModel>());
                    }

                    if (base.characterMotor)
                    {
                        if (base.characterMotor.velocity.y < 0) base.characterMotor.velocity.y = 0;
                    }

                    if (base.skillLocator)
                    {
                        base.skillLocator.special.SetBaseSkill(AatroxPlugin.massacreCancelDef);
                        base.skillLocator.special.RemoveAllStocks();
                    }
                }
                else
                {
                    if (NetworkServer.active && base.characterBody)
                    {
                        int j = base.characterBody.GetBuffCount(AatroxPlugin.massacreBuff);
                        for (int i = 0; i < j; i++)
                        {
                            base.characterBody.RemoveBuff(AatroxPlugin.massacreBuff);
                        }
                    }

                    base.PlayAnimation("Gesture, Override", "Cancel", "", 0.25f);

                    Util.PlaySound(Sounds.AatroxMassacreCancel, base.gameObject);

                    if (base.skillLocator)
                    {
                        base.skillLocator.special.SetBaseSkill(AatroxPlugin.massacreDef);
                        base.skillLocator.special.RemoveAllStocks();
                    }
                }
            }

            this.outer.SetNextStateToMain();
        }

        /*public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.stopwatch += Time.fixedDeltaTime;

            if (this.stopwatch >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }*/

        public override void OnExit()
        {
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

}
