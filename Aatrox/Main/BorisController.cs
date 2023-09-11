using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Aatrox
{
    public class BorisController : MonoBehaviour
    {
        public StyleMeter styleMeter;
        public bool infiniteSDT;

        private CharacterBody charBody;
        private CharacterMotor charMotor;
        private HealthComponent charHealth;
        private InputBankTest inputBank;

        private uint activePlayID;

        private void Start()
        {
            charBody = GetComponentInChildren<CharacterBody>();
            charMotor = GetComponentInChildren<CharacterMotor>();
            charHealth = GetComponentInChildren<HealthComponent>();
            inputBank = GetComponentInChildren<InputBankTest>();

            Invoke("ApplyBuff", 0.1f);

            AatroxPlugin.devilTriggerActive++;
            if (AatroxPlugin.devilTrigger.Value)
            {
                activePlayID = Util.PlaySound(Sounds.SinDevilTrigger, this.gameObject);
            }
            else
            {
                activePlayID = Util.PlaySound(Sounds.BorisLoop, this.gameObject);
            }
        }

        private void ApplyBuff()
        {
            if (NetworkServer.active && charBody)
            {
                charBody.AddBuff(AatroxPlugin.worldEnderBuff);
            }
        }

        public void AddStyle(float coefficient)
        {
            if (styleMeter) styleMeter.AddStyle(coefficient);
        }

        public void SpendResource(int amount)
        {
            if (infiniteSDT) return;

            if (charBody && charBody.skillLocator)
            {
                charBody.skillLocator.special.DeductStock(amount);
            }
        }

        private void OnDestroy()
        {
            if (activePlayID != 0) AkSoundEngine.StopPlayingID(activePlayID);
            AatroxPlugin.devilTriggerActive--;
        }
    }

}
