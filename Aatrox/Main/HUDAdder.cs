using R2API.Utils;
using Rewired;
using Rewired.Data;
using RoR2;
using RoR2.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aatrox.Main
{
    class HUDAdder : MonoBehaviour
    {
        private HUD hud;
        private bool HUDAdded;
        private void Awake()
        {
            hud = base.GetComponent<HUD>();
        }
        private void Start()
        {
            AkSoundEngine.SetRTPCValue("Style_Rank", 100);
        }
        private void FixedUpdate()
        {
            if (HUDAdded)
            {
                Destroy(this);
                return;
            }
            if (hud.targetBodyObject)
            {
                HUDAdded = true;

                if (Util.HasEffectiveAuthority(hud.targetBodyObject) && hud.targetBodyObject.GetComponent<AatroxController>() || hud.targetBodyObject.GetComponent<BorisController>())
                {
                    base.gameObject.AddComponent<ExtraHUD>();
                    base.gameObject.AddComponent<SwapHUD>();
                }
            }
        }
    }
}
