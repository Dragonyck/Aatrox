using Aatrox;
using EntityStates.ClayBruiser.Weapon;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Aatrox
{
    public class WeaponSwap : BaseState
    {
        //private GameObject swapInstance;
        private AatroxController aatroxController;

        public override void OnEnter()
        {
            base.OnEnter();
            this.aatroxController = base.GetComponent<AatroxController>();

            SkillDef skill = base.characterBody.skillLocator.primary.skillDef;

            if (skill.skillNameToken == "AATROX_PRIMARY_SLASH_NAME")
            {
                Util.PlaySound(Sounds.AatroxSwapGun, base.gameObject);
                base.characterBody.skillLocator.primary.SetBaseSkill(base.characterBody.skillLocator.primary.skillFamily.variants[1].skillDef);
            }
            else if (skill.skillNameToken == "AATROX_PRIMARY_GUN_NAME")
            {
                Util.PlaySound(Sounds.AatroxSwapSword, base.gameObject);
                base.characterBody.skillLocator.primary.SetBaseSkill(base.characterBody.skillLocator.primary.skillFamily.variants[0].skillDef);
            }

            base.PlayAnimation("Gesture, Override", "Cancel", "", 0.25f);

            //this.swapInstance = UnityEngine.Object.Instantiate<GameObject>(ParentMonster.LoomingPresence.blinkPrefab, base.FindModelChild("L_Hand").position, base.FindModelChild("L_Hand").rotation);
            //this.swapInstance.transform.parent = base.FindModelChild("L_Hand");

            if (this.aatroxController) this.aatroxController.ResetWeapon();

            this.outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }

}
