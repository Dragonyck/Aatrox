using RoR2;
using RoR2.UI;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Aatrox
{
    public class ExtraHUD : MonoBehaviour
    {
        private void Awake()
        {
            Invoke("InitHUD", 0.25f);
        }

        private void InitHUD()
        {
            Debug.LogWarning("ExtraHUD-InitHUD");
            HUD self = this.gameObject.GetComponent<HUD>();
            if (self.targetMaster)
            {
                GameObject styleMeter = GameObject.Instantiate(Assets.styleMeter, self.mainUIPanel.transform.Find("SpringCanvas").Find("BottomCenterCluster"));
                styleMeter.GetComponent<RectTransform>().localPosition = Vector3.zero;
                styleMeter.GetComponent<RectTransform>().anchoredPosition = new Vector3(-128f, 0);
                styleMeter.GetComponent<RectTransform>().localScale = 1.5f * Vector3.one;

                StyleMeter temp = styleMeter.AddComponent<StyleMeter>();
                temp.characterMaster = self.targetMaster;

                foreach (Image i in temp.GetComponentsInChildren<Image>())
                {
                    if (i)
                    {
                        if (i.gameObject.name == "StyleRank") temp.styleRankImage = i;
                        if (i.gameObject.name == "StyleText") temp.styleTextImage = i;
                    }
                }
            }
            else
            {
                Invoke("InitHUD", 0.25f);
            }
        }
    }

    public class StyleMeter : MonoBehaviour
    {
        public CharacterMaster characterMaster;
        public Image styleRankImage;
        public Image styleTextImage;

        public Image styleMeterFill;

        public float styleScore;
        private float maxStyle;
        private float styleDecayRate;
        private float styleDecayGrowth;
        private float styleMaxDecay;
        private float baseStyleGain;

        private AatroxController aatroxController;
        private BorisController borisController;
        private GenericStyleController genericController;
        private Animator anim;
        private string styleString;
        private string oldStyleString;
        private float oldStyleScore;
        private bool isHidden;

        public static event Action<float> OnStyleChanged = delegate { };

        private void Awake()
        {
            anim = GetComponent<Animator>();

            styleScore = 0;
            maxStyle = 110;
            styleDecayRate = 0f;
            styleDecayGrowth = 1.1f;
            styleMaxDecay = 5.8f;
            baseStyleGain = 2.25f * AatroxPlugin.styleMultiplier.Value;

            styleMeterFill = transform.Find("StyleMeter").Find("StyleFill").GetComponent<Image>();

            isHidden = true;
        }

        private void FixedUpdate()
        {
            if (!characterMaster)
            {
                return;
            }

            bool hasPlayer = false;

            if (aatroxController != null || borisController != null || genericController != null) hasPlayer = true;

            if (!hasPlayer)
            {
                if (characterMaster.hasBody)
                {
                    //
                    bool foundPlayer = false;

                    var component1 = characterMaster.GetBody().gameObject.GetComponent<AatroxController>();
                    var component2 = characterMaster.GetBody().gameObject.GetComponent<BorisController>();
                    var component3 = characterMaster.GetBody().gameObject.GetComponent<GenericStyleController>();

                    if (component1 != null || component2 != null || component3 != null) foundPlayer = true;

                    if (!foundPlayer)
                    {
                        Destroy(this.gameObject);
                        return;
                    }

                    if (component1 != null)
                    {
                        aatroxController = component1;
                        aatroxController.styleMeter = this;
                    }

                    if (component2 != null)
                    {
                        borisController = component2;
                        borisController.styleMeter = this;
                    }

                    if (component3 != null)
                    {
                        genericController = component3;
                        genericController.styleMeter = this;
                    }
                }
                else
                {
                    return;
                }
            }

            StyleDecay();

            string fuckOff = "yeah i know this is sloppy hardcoded garbage leave me alone :c if it works it works";
            styleString = "D";

            if (styleScore >= 10) styleString = "C";
            if (styleScore >= 30) styleString = "B";
            if (styleScore >= 50) styleString = "A";
            if (styleScore >= 70) styleString = "S";
            if (styleScore >= 90) styleString = "SS";
            if (styleScore >= 95) styleString = "SSS";

            UpdateStyleMeter(styleString);

            AkSoundEngine.SetRTPCValue("Style_Rank", styleScore);

            //BURY THE LIIIIIGHT DEEP WITHIIIIINNNNN

            if (AatroxPlugin.buryTheLight.Value)
            {
                string stateString = "F";

                if (styleScore < 70) stateString = "D_C_B_A";
                if (styleScore >= 70 && styleScore < 95) stateString = "S_SS";
                if (styleScore >= 95) stateString = "SSS";

                if (aatroxController && !aatroxController.massacre) stateString = "F";

                AkSoundEngine.SetState("Ranking", stateString);
            }

            //Chat.AddMessage("Stylish rank: " + styleString);
        }

        private void StyleDecay()
        {
            styleScore = Mathf.Clamp(styleScore - (styleDecayRate * Time.fixedDeltaTime), 0, maxStyle);

            if (characterMaster)
            {
                if (characterMaster.inventory.GetItemCount(RoR2Content.Items.LunarDagger) > 0 && styleScore > 29) styleScore = 29;
            }

            float growth = styleDecayGrowth;
            if (styleScore > 80) growth *= 1.3f;
            styleDecayRate = Mathf.Clamp(styleDecayRate + (styleDecayGrowth * Time.fixedDeltaTime), 0, styleMaxDecay);
        }

        public void AddStyle(float coefficient)
        {
            float gain = baseStyleGain * coefficient;
            float mod = maxStyle / styleScore;
            mod = Mathf.Clamp(mod - 1, 0.4f, 4f);

            if (aatroxController)
            {
                if (aatroxController.super) mod *= 0.2f;
                else if (aatroxController.massacre) mod *= 1.35f;
            }

            if (borisController) mod *= 2.5f;

            if (genericController) mod *= 1.25f;

            if (DifficultyIndex.Hard <= Run.instance.selectedDifficulty) mod *= 1.15f;
            if (DifficultyIndex.Easy >= Run.instance.selectedDifficulty) mod *= 0.5f;

            if (characterMaster)
            {
                if (characterMaster.inventory.GetItemCount(RoR2Content.Items.ShieldOnly) > 0 && (aatroxController || borisController)) mod *= 5;
            }

            gain *= mod;

            styleScore = Mathf.Clamp(styleScore + gain, 0, maxStyle);
            styleDecayRate = 0f;
        }

        private void UpdateStyleMeter(string tempStyleString)
        {
            float barMin = 0;
            float barMax = 100;

            if (tempStyleString == "D")
            {
                styleRankImage.sprite = Assets.styleD;
                styleTextImage.sprite = Assets.styleTextD;

                barMin = 0;
                barMax = 10;
            }
            else if (tempStyleString == "C")
            {
                styleRankImage.sprite = Assets.styleC;
                styleTextImage.sprite = Assets.styleTextC;

                barMin = 10;
                barMax = 30;
            }
            else if (tempStyleString == "B")
            {
                styleRankImage.sprite = Assets.styleB;
                styleTextImage.sprite = Assets.styleTextB;

                barMin = 30;
                barMax = 50;
            }
            else if (tempStyleString == "A")
            {
                styleRankImage.sprite = Assets.styleA;
                styleTextImage.sprite = Assets.styleTextA;

                barMin = 50;
                barMax = 70;
            }
            else if (tempStyleString == "S")
            {
                styleRankImage.sprite = Assets.styleS;
                styleTextImage.sprite = Assets.styleTextS;

                barMin = 70;
                barMax = 90;
            }
            else if (tempStyleString == "SS")
            {
                styleRankImage.sprite = Assets.styleSS;
                styleTextImage.sprite = Assets.styleTextSS;

                barMin = 90;
                barMax = 95;
            }
            else if (tempStyleString == "SSS")
            {
                styleRankImage.sprite = Assets.styleSSS;
                styleTextImage.sprite = Assets.styleTextSSS;

                barMin = 95;
                barMax = maxStyle;
            }

            if (styleMeterFill)
            {
                float fillValue = (styleScore - barMin) / (barMax - barMin);
                styleMeterFill.fillAmount = fillValue;
            }

            if (tempStyleString != oldStyleString || (styleScore > 0 && isHidden))
            {
                OnStyleChanged(styleScore);

                if (styleScore > 0)
                {
                    isHidden = false;

                    if (styleScore >= oldStyleScore)
                    {
                        anim.SetTrigger("Good");
                    }
                    else
                    {
                        anim.SetTrigger("Bad");
                    }
                }

            }

            if (styleScore <= 0 && !isHidden)
            {
                anim.SetTrigger("Reset");
                isHidden = true;
            }

            oldStyleString = tempStyleString;
            oldStyleScore = styleScore;
        }
    }

}
