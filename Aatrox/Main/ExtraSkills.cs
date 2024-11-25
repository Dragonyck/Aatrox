using R2API.Utils;
using Rewired;
using Rewired.Data;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aatrox
{
    public class SwapHUD : MonoBehaviour
    {
        public GameObject swapIcon;

        private HUD hud;

        private void Awake()
        {
            Invoke("InitHUD", 0.25f);
        }

        private void InitHUD()
        {
            Debug.LogWarning("SwapHUD-InitHUD");
            this.hud = this.gameObject.GetComponent<HUD>();
            if (this.hud.targetMaster)
            {
                if (this.hud.targetMaster.bodyPrefab == null) return;
                if (this.hud.targetMaster.bodyPrefab != AatroxPlugin.aatroxPrefab) return;

                Transform skillsContainer = this.hud.mainUIPanel.transform.Find("SpringCanvas").Find("BottomRightCluster").Find("Scaler");

                var skill = skillsContainer.Find($"Skill{4}Root");
                var skillCopy = GameObject.Instantiate(skill, skill.parent);

                //Lift up copy
                var skillCopyRectTransform = skillCopy.GetComponent<RectTransform>();
                skillCopyRectTransform.anchorMin = new Vector2(1, 2.15F);
                skillCopyRectTransform.anchorMax = new Vector2(1, 2.15F);

                //Changing visual input binding
                var skillKeyText = skillCopy.Find("SkillBackgroundPanel").Find("SkillKeyText");
                Destroy(skillKeyText.GetComponent<InputBindingDisplayController>());
                skillKeyText.GetComponent<HGTextMeshProUGUI>().text = AatroxPlugin.swapKeybind.Value.ToString();

                skillCopy.Find("IconPanel").GetComponent<Image>().color = Color.white;

                skillCopy.Find("IsReadyPanel").gameObject.SetActive(true);

                skillCopy.Find("Skill4StockRoot").gameObject.SetActive(false);
                skillCopy.Find("CooldownPanel").gameObject.SetActive(false);
                skillCopy.Find("CooldownText").gameObject.SetActive(false);

                Vector3 oldPos = skillCopy.Find("CooldownText").GetComponent<RectTransform>().anchoredPosition3D;
                skillCopy.Find("CooldownText").GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 4);

                skillCopy.GetComponent<SkillIcon>().iconImage.sprite = Assets.swapIcon;
                skillCopy.GetComponent<SkillIcon>().tooltipProvider = new TooltipProvider
                {
                    bodyColor = AatroxPlugin.aatroxColor,
                    bodyToken = "AATROX_NAME",
                    overrideBodyText = "Swap your primary skill",
                    overrideTitleText = "Weapon Swap"
                };
                Destroy(skillCopy.gameObject.GetComponent<SkillIcon>());

                swapIcon = skillCopy.gameObject;
            }
            else
            {
                Invoke("InitHUD", 0.25f);
            }
        }

        public void FixedUpdate()
        {
            if (hud != null)
            {
                if (hud.targetBodyObject != null)
                {
                    var component = hud.targetBodyObject.GetComponent<AatroxController>();
                    if (component != null)
                    {
                        if (swapIcon != null)
                        {
                            swapIcon.gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (swapIcon != null)
                        {
                            swapIcon.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    internal static class ExtraInputs
    {
        internal static void AddActionsToInputCatalog()
        {
            var extraActionAxisPair = new InputCatalog.ActionAxisPair("AATROX_SKILL_SWAP_NAME", AxisRange.Full);

            InputCatalog.actionToToken.Add(extraActionAxisPair, "AATROX_SKILL_SWAP_NAME");
        }

        internal static void AddCustomActions(Action<UserData> orig, UserData self)
        {
            var firstAction = CreateInputAction(100, "AATROX_SKILL_SWAP_NAME", InputActionType.Button);
            var secondAction = CreateInputAction(101, "AATROX_SKILL_DANCE_NAME", InputActionType.Button);

            var actions = self.GetFieldValue<List<InputAction>>("actions");

            actions?.Add(firstAction);
            actions?.Add(secondAction);

            orig(self);
        }

        internal static InputAction CreateInputAction(int id, string name, InputActionType type = InputActionType.Button)
        {
            var action = new InputAction();

            action.SetFieldValue("_id", id);
            action.SetFieldValue("_name", name);
            action.SetFieldValue("_type", type);
            action.SetFieldValue("_descriptiveName", name);
            action.SetFieldValue("_behaviorId", 0);
            action.SetFieldValue("_userAssignable", true);
            action.SetFieldValue("_categoryId", 0);

            return action;
        }
    }
}
