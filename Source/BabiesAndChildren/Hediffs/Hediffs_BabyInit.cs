using BabiesAndChildren.api;
using RimWorld;
using Verse;
using Verse.Sound;
using HealthUtility = BabiesAndChildren.Tools.HealthUtility;

namespace BabiesAndChildren
{

    /// <summary>
    /// class for BabyState HediffDef
    /// Added by Growing_Comp if pawn is a little baby
    /// </summary>
    // why is this a hediff? if there is no reason it would be cleaner to move this into Growing_Comp
    public class Hediff_BabyState : HediffWithComps
    {
        public override void PostRemoved()
        {
            var comp = pawn.TryGetComp<Growing_Comp>();
            if (comp == null) return;

            comp.GrowToStage(AgeStages.GetAgeStage(pawn));

            Pawn mother = pawn.GetMother();
            Pawn father = pawn.GetFather();
            MathTools.Fixed_Rand rand;
            if (mother != null)
            {
                rand = new MathTools.Fixed_Rand((int) mother.ageTracker.AgeBiologicalTicks);
                mother.ageTracker.AgeBiologicalTicks += 2;
                mother.ageTracker.AgeChronologicalTicks += 2;
            }
            else if (father != null)
            {
                rand = new MathTools.Fixed_Rand((int) father.ageTracker.AgeBiologicalTicks);
                father.ageTracker.AgeBiologicalTicks += 2;
                father.ageTracker.AgeBiologicalTicks += 2;
            }
            else
            {
                rand = new MathTools.Fixed_Rand((int) pawn.ageTracker.AgeBiologicalTicks);
            }

            if (rand.Fixed_RandChance(BnCSettings.STILLBORN_CHANCE))
            {
                BabyTools.Miscarry(pawn, mother, father);
                return;
            }


            if (mother != null)
            {
                HealthUtility.TryAddHediff(mother, HediffDef.Named("PostPregnancy"));
                HealthUtility.TryAddHediff(mother, HediffDef.Named("Lactating"),
                    HealthUtility.GetPawnBodyPart(mother, "Torso"));
            }

            if (ChildrenBase.ModRimJobWorld_ON && BnCSettings.enable_postpartum)
            {
                HealthUtility.TryAddHediff(mother, HediffDef.Named("BnC_RJW_PostPregnancy"));
            }


            //Make crying sound when baby is born
            SoundInfo info = SoundInfo.InMap(new TargetInfo(pawn.PositionHeld, pawn.MapHeld));
            SoundDef.Named("Pawn_BabyCry").PlayOneShot(info);

            HealthUtility.ClearImplantAndAddiction(pawn);
            ChildrenUtility.RenamePawn(pawn, mother, father);

            //For rabbie
            if (pawn.def.defName == "Rabbie")
            {
                HealthUtility.TryAddHediff(pawn, HediffDef.Named("PlanetariumAddiction"));
            }

            if (!ChildrenBase.ModCSL_ON)
            {
                BabyTools.SetBabyTraits(pawn, mother, father, rand);
                BabyTools.SetBabySkillsAndPassions(pawn, mother, father, rand);
            }

            comp.Props.ColonyBorn = true;

            pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("JustBorn"));
        }
    }
}