using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using System.Collections;

namespace Aatrox
{
    public class MenuAnim : MonoBehaviour
    {
        private uint playID;

        internal void OnEnable()
        {
            /*bool flag = base.gameObject.transform.parent.gameObject.name == "CharacterPad";
            if (flag)
            {
                base.StartCoroutine(this.SpawnAnim());
            }*/

            if (GetComponentInChildren<AatroxController>()) DestroyImmediate(GetComponentInChildren<AatroxController>());

            foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
            {
                if (i.transform.parent.name == "PriceEffect") i.Play();
            }

            EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, new EffectData
            {
                origin = base.gameObject.transform.position
            }, false);

            playID = Util.PlaySound(Sounds.AatroxShadowStep, base.gameObject);
        }

        internal void OnDisable()
        {
            AkSoundEngine.StopPlayingID(playID);
        }

        private IEnumerator SpawnAnim()
        {
            //Animator animator = base.GetComponentInChildren<Animator>();

            foreach (ParticleSystem i in GetComponentsInChildren<ParticleSystem>())
            {
                if (i.transform.parent.name == "PriceEffect") i.Play();
            }

            EffectManager.SpawnEffect(EntityStates.ImpMonster.BlinkState.blinkPrefab, new EffectData
            {
                origin = base.gameObject.transform.position
            }, false);

            Util.PlaySound(Sounds.AatroxShadowStep, base.gameObject);
            //PlayAnimation("FullBody, Override", "Menu", "", 1, animator);
            //GetComponentInChildren<EntityStateMachine>().SetNextState(new AatroxMenuState());

            yield break;
        }

        private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            animator.SetFloat(playbackRateParam, 1f);
            animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
            animator.Update(0f);
            float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
            animator.SetFloat(playbackRateParam, length / duration);
        }
    }

    public class BorisMenuAnim : MonoBehaviour
    {
        private uint playID;

        internal void OnEnable()
        {
            base.StartCoroutine(this.SpawnAnim());
        }

        internal void OnDisable()
        {
            AkSoundEngine.StopPlayingID(playID);
        }

        private IEnumerator SpawnAnim()
        {
            Animator animator = base.GetComponentInChildren<Animator>();

            EffectManager.SpawnEffect(EntityStates.ImpBossMonster.SpawnState.spawnEffectPrefab, new EffectData
            {
                origin = base.gameObject.transform.position
            }, false);

            playID = Util.PlaySound(EntityStates.ImpBossMonster.SpawnState.spawnSoundString, base.gameObject);

            PlayAnimation("FullBody, Override", "Menu", "", 1, animator);

            yield break;
        }

        private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
        {
            int layerIndex = animator.GetLayerIndex(layerName);
            animator.SetFloat(playbackRateParam, 1f);
            animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
            animator.Update(0f);
            float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
            animator.SetFloat(playbackRateParam, length / duration);
        }
    }
}
