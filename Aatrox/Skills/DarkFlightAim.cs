using RoR2;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class AatroxAimDive : BaseState
    {
        public float maxAimTime = 2.5f;
        public static float impactRadius = 12f;

        private GameObject areaIndicatorInstance;
        private float stopwatch;
        private CameraTargetParams.AimRequest request;
        public void SetAimRequest(CameraTargetParams.AimRequest request)
        {
            this.request = request;
        }
        public override void OnEnter()
        {
            base.OnEnter();

            stopwatch = 0;

            if (Huntress.ArrowRain.areaIndicatorPrefab)
            {
                this.areaIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(Huntress.ArrowRain.areaIndicatorPrefab);
                this.areaIndicatorInstance.transform.localScale = new Vector3(AatroxAimDive.impactRadius, AatroxAimDive.impactRadius, AatroxAimDive.impactRadius);
            }

            //Util.PlaySound(Assaulter.beginSoundString, base.gameObject);
        }

        private void UpdateAreaIndicator()
        {
            if (this.areaIndicatorInstance)
            {
                float maxDistance = 1000f;

                RaycastHit raycastHit;

                bool flag = (Physics.Raycast(base.GetAimRay(), out raycastHit, maxDistance, LayerIndex.CommonMasks.bullet)) && (raycastHit.point.y < base.transform.position.y);

                if (flag)
                {
                    this.areaIndicatorInstance.transform.position = raycastHit.point;
                    this.areaIndicatorInstance.transform.up = raycastHit.normal;
                    this.areaIndicatorInstance.SetActive(true);
                }
                else
                {
                    this.areaIndicatorInstance.SetActive(false);
                }

            }
        }

        public override void Update()
        {
            base.Update();
            this.UpdateAreaIndicator();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            stopwatch += Time.fixedDeltaTime;

            if (base.characterMotor)
            {
                base.characterMotor.velocity = Vector3.zero;
            }

            if (base.isAuthority && base.inputBank)
            {
                /*if (base.skillLocator && base.skillLocator.special.IsReady() && base.inputBank.skill4.justPressed)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }*/

                if (base.inputBank.skill1.justPressed || base.inputBank.skill2.justPressed || base.inputBank.skill3.justPressed || base.inputBank.skill4.justPressed)
                {
                    AatroxDive nextState = new AatroxDive();
                    this.outer.SetNextState(nextState);
                    return;
                }
            }

            if (stopwatch > maxAimTime)
            {
                AatroxDive nextState = new AatroxDive();
                this.outer.SetNextState(nextState);
                return;
            }
        }

        public override void OnExit()
        {
            //Util.PlaySound(Sounds.AatroxDarkFlightDive, base.gameObject);

            if (base.cameraTargetParams)
            {
                this.request.Dispose();
            }

            if (this.areaIndicatorInstance)
            {
                EntityState.Destroy(this.areaIndicatorInstance.gameObject);
            }

            base.OnExit();
        }
    }

}
