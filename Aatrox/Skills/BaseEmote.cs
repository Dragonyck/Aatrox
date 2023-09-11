using RoR2;
using Aatrox;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class BaseEmote : BaseState
    {
        public string soundString;
        public string animString;

        private uint activePlayID;
        private float initialTime;
        private Animator animator;
        private ChildLocator childLocator;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.childLocator = base.GetModelChildLocator();
            this.aatroxController = base.GetComponent<AatroxController>();

            base.characterBody.hideCrosshair = true;

            if (base.GetAimAnimator()) base.GetAimAnimator().enabled = false;

            base.PlayAnimation("FullBody, Override", this.animString);
            this.activePlayID = Util.PlaySound(soundString, base.gameObject);

            this.initialTime = Time.fixedTime;
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterBody.hideCrosshair = false;

            if (base.GetAimAnimator()) base.GetAimAnimator().enabled = true;

            base.PlayAnimation("FullBody, Override", "BufferEmpty");
            if (this.activePlayID != 0) AkSoundEngine.StopPlayingID(this.activePlayID);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            bool flag = false;

            if (base.characterMotor)
            {
                if (!base.characterMotor.isGrounded) flag = true;
                if (base.characterMotor.velocity != Vector3.zero) flag = true;
            }

            if (base.inputBank)
            {
                if (base.inputBank.skill1.down) flag = true;
                if (base.inputBank.skill2.down) flag = true;
                if (base.inputBank.skill3.down) flag = true;
                if (base.inputBank.skill4.down) flag = true;
                if (base.inputBank.jump.down) flag = true;

                if (base.inputBank.moveVector != Vector3.zero) flag = true;
            }

            if (this.aatroxController)
            {
                if (this.aatroxController.styleMeter)
                {
                    if (this.aatroxController.styleMeter.styleScore >= 50)
                    {
                        this.aatroxController.AddStyle(0);
                    }
                }
            }

            //dance cancels lol
            if (base.isAuthority && base.characterMotor.isGrounded)
            {
                if (Input.GetKeyDown("z"))
                {
                    flag = false;
                    this.outer.SetInterruptState(new Dance(), InterruptPriority.Any);
                    return;
                }
                /*else if (Input.GetKeyDown("x"))
                {
                    flag = false;
                    this.outer.SetInterruptState(EntityState.Instantiate(new SerializableEntityStateType(typeof(Floss))), InterruptPriority.Any);
                    return;
                }*/
            }

            CameraTargetParams ctp = base.cameraTargetParams;
            float denom = (1 + Time.fixedTime - this.initialTime);
            float smoothFactor = 8 / Mathf.Pow(denom, 2);
            Vector3 smoothVector = new Vector3(-3 / 20, 1 / 16, -1);
            //ctp._currentCameraParamsData.idealLocalCameraPos = new Vector3(0f, -1.4f, -6f) + smoothFactor * smoothVector;

            if (flag)
            {
                this.outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }

    public class Dance : BaseEmote
    {
        public override void OnEnter()
        {
            this.animString = "Dance";
            //this.soundString = EnforcerPlugin.Sounds.DefaultDance;
            base.OnEnter();
        }
    }
}
