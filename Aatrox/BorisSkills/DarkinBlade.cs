using Aatrox;
using EntityStates.Merc;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Boris
{
    public class BorisGroundLight : BaseState
    {
        public static float comboDamageCoefficient = 7.5f;
        public static float finisherDamageCoefficient = 12.5f;
        public static float styleCoefficient = 1f;

        private float stopwatch;
        private float attackDuration;
        private float earlyExitDuration;
        private Animator animator;
        private OverlapAttack overlapAttack;
        private float hitPauseTimer;
        private bool isInHitPause;
        private bool hasSwung;
        private bool hasHit;
        public BorisGroundLight.ComboState comboState;
        private string slashChildName;
        private BaseState.HitStopCachedState hitStopCachedState;
        private GameObject swingEffectPrefab;
        private GameObject hitEffectPrefab;
        private string attackSoundString;
        private BorisController borisController;

        public enum ComboState
        {
            GroundLight1,
            GroundLight2,
            GroundLight3
        }

        private struct ComboStateInfo
        {
            private string mecanimStateName;
            private string mecanimPlaybackRateName;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            this.borisController = GetComponent<BorisController>();

            if (this.borisController) this.borisController.SpendResource(AatroxPlugin.borisPrimaryCost);

            this.stopwatch = 0f;
            this.earlyExitDuration = 0.5f;// / this.attackSpeedStat;
            this.animator = base.GetModelAnimator();
            this.hasSwung = false;

            float attackSpeedModifier = this.attackSpeedStat;

            bool @bool = this.animator.GetBool("isMoving");
            bool bool2 = this.animator.GetBool("isGrounded");

            switch (this.comboState)
            {
                case BorisGroundLight.ComboState.GroundLight1:
                    this.attackDuration = 1.25f;// / this.attackSpeedStat;
                    this.overlapAttack = base.InitMeleeOverlap(BorisGroundLight.comboDamageCoefficient * attackSpeedModifier, this.hitEffectPrefab, base.GetModelTransform(), "Sword");
                    if (@bool || !bool2)
                    {
                        base.PlayAnimation("Gesture, Override", "GroundLight1", "GroundLight.playbackRate", this.attackDuration);
                    }
                    else
                    {
                        base.PlayAnimation("FullBody, Override", "GroundLight1", "GroundLight.playbackRate", this.attackDuration);
                    }
                    this.slashChildName = "SlashPos";
                    this.swingEffectPrefab = Assets.borisSlash1FX;
                    this.hitEffectPrefab = Assets.borisHitFX;
                    this.attackSoundString = Sounds.BorisSwing;
                    break;

                case BorisGroundLight.ComboState.GroundLight2:
                    this.attackDuration = 1.25f;// / this.attackSpeedStat;
                    this.overlapAttack = base.InitMeleeOverlap(BorisGroundLight.comboDamageCoefficient * attackSpeedModifier, this.hitEffectPrefab, base.GetModelTransform(), "Sword");
                    if (@bool || !bool2)
                    {
                        base.PlayAnimation("Gesture, Override", "GroundLight2", "GroundLight.playbackRate", this.attackDuration);
                    }
                    else
                    {
                        base.PlayAnimation("FullBody, Override", "GroundLight2", "GroundLight.playbackRate", this.attackDuration);
                    }
                    this.slashChildName = "SlashPos";
                    this.swingEffectPrefab = Assets.borisSlash2FX;
                    this.hitEffectPrefab = Assets.borisHitFX;
                    this.attackSoundString = Sounds.BorisSwing;
                    break;

                case BorisGroundLight.ComboState.GroundLight3:
                    this.attackDuration = 1.5f;// / this.attackSpeedStat;
                    float damage = BorisGroundLight.finisherDamageCoefficient * attackSpeedModifier;
                    this.overlapAttack = base.InitMeleeOverlap(damage, this.hitEffectPrefab, base.GetModelTransform(), "Sword");
                    if (@bool || !bool2)
                    {
                        base.PlayAnimation("Gesture, Override", "GroundLight3", "GroundLight.playbackRate", this.attackDuration);
                    }
                    else
                    {
                        base.PlayAnimation("FullBody, Override", "GroundLight3", "GroundLight.playbackRate", this.attackDuration);
                    }
                    this.slashChildName = "SlashPos";
                    this.swingEffectPrefab = Assets.borisSlash3FX;
                    this.hitEffectPrefab = Assets.borisHitFX;
                    this.attackSoundString = Sounds.BorisThrust;
                    break;
            }

            base.characterBody.SetAimTimer(this.attackDuration + 1f);

            this.overlapAttack.hitEffectPrefab = this.hitEffectPrefab;
        }

        public override void OnExit()
        {
            base.OnExit();

            if (base.skillLocator.special.stock <= 0)
            {
                this.outer.SetNextState(new BorisEndTransformation());
                return;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            this.hitPauseTimer -= Time.fixedDeltaTime;

            if (base.isAuthority)
            {
                bool flag = base.FireMeleeOverlap(this.overlapAttack, this.animator, "Sword.active", GroundLight.forceMagnitude, true);

                this.hasHit = (this.hasHit || flag);

                if (flag)
                {
                    if (this.borisController) this.borisController.AddStyle(BorisGroundLight.styleCoefficient);

                    if (this.comboState == BorisGroundLight.ComboState.GroundLight3)
                    {
                        Util.PlaySound(Sounds.BorisThrustHit, base.gameObject);
                    }
                    else
                    {
                        Util.PlaySound(Sounds.BorisHit, base.gameObject);
                    }

                    if (base.modelLocator)
                    {
                        Transform pos = base.modelLocator.modelBaseTransform;
                        //GameObject.Destroy(GameObject.Instantiate<GameObject>(this.hitEffectPrefab, pos.position + (pos.forward * 1.5f) + (Vector3.up * 0.9f) + (pos.right * UnityEngine.Random.Range(-2, 2)), pos.rotation), 1);
                    }


                    if (!this.isInHitPause)
                    {
                        this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "GroundLight.playbackRate");
                        this.hitPauseTimer = GroundLight.hitPauseDuration / this.attackSpeedStat;
                        this.isInHitPause = true;
                    }
                }

                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    this.isInHitPause = false;
                }
            }

            if (this.stopwatch >= (this.attackDuration * 0.25f) && !this.hasSwung)
            {
                Util.PlayAttackSpeedSound(this.attackSoundString, base.gameObject, 0.5f);
                HealthComponent healthComponent = base.characterBody.healthComponent;
                CharacterDirection component = base.characterBody.GetComponent<CharacterDirection>();

                if (base.isAuthority)
                {
                    if (Input.GetAxis("Vertical") > 0)
                    {
                        if (healthComponent)
                        {
                            healthComponent.TakeDamageForce(3 * GroundLight.selfForceMagnitude * component.forward, true, false);
                        }
                    }
                }

                this.hasSwung = true;

                if (base.modelLocator)
                {
                    Transform pos = base.modelLocator.modelBaseTransform;

                    GameObject.Destroy(GameObject.Instantiate<GameObject>(this.swingEffectPrefab, pos.position + (pos.forward * 0.75f) + (Vector3.up * 0.85f), pos.rotation), 1);
                }

                this.animator.SetFloat("Sword.active", 1f);
            }
            else
            {
                this.animator.SetFloat("Sword.active", 0f);
            }

            if (!this.isInHitPause)
            {
                this.stopwatch += Time.fixedDeltaTime;
            }
            else
            {
                if (base.characterMotor) base.characterMotor.velocity = Vector3.zero;
                if (this.animator) this.animator.SetFloat("GroundLight.playbackRate", 0f);
            }

            if (base.isAuthority && this.stopwatch >= this.attackDuration - this.earlyExitDuration)
            {
                if (!this.hasSwung)
                {
                    if (this.overlapAttack != null) this.overlapAttack.Fire(null);
                }

                if (base.inputBank.skill1.down && this.comboState != BorisGroundLight.ComboState.GroundLight2)
                {
                    BorisGroundLight groundLight = new BorisGroundLight();
                    groundLight.comboState = this.comboState + 1;
                    this.outer.SetNextState(groundLight);
                    return;
                }

                if (this.stopwatch >= this.attackDuration)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)this.comboState);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.comboState = (BorisGroundLight.ComboState)reader.ReadByte();
        }

    }

}
