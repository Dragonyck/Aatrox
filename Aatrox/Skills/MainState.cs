using Aatrox;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class AatroxMain : GenericCharacterMain
    {
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();

            this.aatroxController = base.GetComponent<AatroxController>();
        }

        public override void Update()
        {
            base.Update();

            if (base.isAuthority && base.characterMotor.isGrounded)
            {
                if (Input.GetKeyDown(AatroxPlugin.danceKeybind.Value))
                {
                    this.outer.SetInterruptState(new Dance(), InterruptPriority.Any);
                    return;
                }
            }

            if (base.isAuthority)
            {
                if (Input.GetKeyDown(AatroxPlugin.swapKeybind.Value))
                {
                    this.outer.SetInterruptState(new WeaponSwap(), InterruptPriority.Any);
                    return;
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void ProcessJump()
        {
            base.ProcessJump();
        }
    }
}
