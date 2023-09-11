using EntityStates.Aatrox;
using RoR2;
using RoR2.Skills;
//using SkillsPlusPlus.Modifiers;

namespace Aatrox.SkillModifiers
{
    /*[SkillLevelModifier("AATROX_PRIMARY_SLASH_NAME", typeof(AatroxGroundLight))]
    public class AatroxGroundLightSkillModifier : SimpleSkillModifier<AatroxGroundLight>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            AatroxGroundLight.comboDamageCoefficient = MultScaling(StaticValues.bloodThirstComboDamage, 0.1f, level); // +10% damage per level
            AatroxGroundLight.finisherDamageCoefficient = MultScaling(StaticValues.bloodThirstFinisherDamage, 0.35f, level); // +35% third hit damage per level
            //AatroxGroundLight.baseHealAmount = (int)AdditiveScaling(AatroxGroundLight.baseHealAmount, 0.05f, level); // +5% heal amount per level
        }
    }

    [SkillLevelModifier("AATROX_SECONDARY_COMBO_NAME", typeof(AatroxSwordStance))]
    public class AatroxSwordStanceSkillModifier : SimpleSkillModifier<AatroxSwordStance>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            skillDef.baseMaxStock = AdditiveScaling(1, 1, level); // +1 stock per level
        }
    }

    [SkillLevelModifier("AATROX_SECONDARY_SLASH_NAME", typeof(AatroxFireProjectile))]
    public class AatroxProjectileSkillModifier : SimpleSkillModifier<AatroxFireProjectile>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            AatroxFireProjectile.damageCoefficient = MultScaling(3.5f, 0.3f, level); // +30% damage per level
        }
    }

    [SkillLevelModifier("AATROX_UTILITY_FLIGHT_NAME", typeof(AatroxDive))]
    public class AatroxDiveSkillModifier : SimpleSkillModifier<AatroxDive>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            AatroxDive.impactDamageCoefficient = MultScaling(5, 0.15f, level); // +15% damage per level
            AatroxDive.impactRadius = MultScaling(12, 0.2f, level); // +20% radius per level
        }
    }

    [SkillLevelModifier("AATROX_UTILITY_SHADOW_NAME", typeof(AatroxShadowStep))]
    public class AatroxShadowStepSkillModifier : SimpleSkillModifier<AatroxShadowStep>
    {
        public override void OnSkillLeveledUp(int level, CharacterBody characterBody, SkillDef skillDef)
        {
            base.OnSkillLeveledUp(level, characterBody, skillDef);
            skillDef.baseMaxStock = AdditiveScaling(StaticValues.shadowStepStock, 1, level); // +1 stock per level
        }
    }*/
}
