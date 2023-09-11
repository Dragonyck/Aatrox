using R2API;
using RoR2;
using RoR2.Projectile;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace Aatrox
{
    class Utils
    {
        internal static void RegisterEffect(GameObject effect, float duration)
        {
            effect.AddComponent<DestroyOnTimer>().duration = duration;
            effect.AddComponent<NetworkIdentity>();
            effect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            var effectcomponent = effect.AddComponent<EffectComponent>();
            effectcomponent.applyScale = false;
            effectcomponent.effectIndex = EffectIndex.Invalid;
            effectcomponent.parentToReferencedTransform = true;
            effectcomponent.positionAtReferencedTransform = true;
            effectcomponent.soundName = "";
            ContentAddition.AddEffect(effect);
        }
    }
}
