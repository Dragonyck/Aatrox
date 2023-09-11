using UnityEngine;

namespace EntityStates.Boris
{
    public class BorisMain : GenericCharacterMain
    {
        public static float hoverVelocity = -0.1f;
        public static float hoverAcceleration = 240f;

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void Update()
        {
            base.Update();

            if (this.hasCharacterMotor && this.hasInputBank && base.isAuthority)
            {
                bool flag = base.inputBank.jump.down && !base.characterMotor.isGrounded && base.characterMotor.velocity.y <= 0;

                if (flag)
                {
                    Hover();
                }
            }
        }

        private void Hover()
        {
            if (base.isAuthority)
            {
                float num = base.characterMotor.velocity.y;
                num = Mathf.MoveTowards(num, BorisMain.hoverVelocity, BorisMain.hoverAcceleration * Time.fixedDeltaTime);
                base.characterMotor.velocity = new Vector3(base.characterMotor.velocity.x, num, base.characterMotor.velocity.z);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }

}
