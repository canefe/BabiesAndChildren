using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BabiesAndChildren.api;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace BabiesAndChildren.Harmony
{
    

    // Babies wake up if they're unhappy
    [HarmonyPatch(typeof(RestUtility), "WakeThreshold")]
    static class RestUtility_WakeThreshold_Patch {
        static void Postfix(ref float __result, ref Pawn p) {
            if (!p.RaceProps.Humanlike) return;
            if (AgeStages.IsYoungerThan(p, AgeStages.Child) && p.health.hediffSet.HasHediff(BnCHediffDefOf.UnhappyBaby)) {
                __result = 0.15f;
            }
            // Adults nearby wake up too (this caused performance problems and was bugged. it was still performance heavy when it was fixed. another way should be used to wake the adults.)
        }
    } 

    [HarmonyPatch(typeof(RestUtility), "IsValidBedFor")]
    internal static class RestUtility_IsValidBedFor_Patch {
        /// <summary>
        /// Failsafe check to ensure that any pawn looking for a bed will not find a crib if
        /// they do not belong in a crib
        /// </summary>
        [HarmonyPostfix]

        static void Postfix(Pawn sleeper, Thing bedThing, bool __result) {
            Building_Bed building_Bed = bedThing as Building_Bed;
            if (building_Bed != null) // carrying baby causes an exception.
            {

                if (ChildrenUtility.IsBedCrib((Building_Bed)bedThing) && !ChildrenUtility.ShouldUseCrib(sleeper))
                {
                    __result = false;
                }

            }
        }
    }

    [HarmonyPatch(typeof(RestUtility), "FindBedFor", 
        typeof(Pawn), 
        typeof(Pawn), 
        typeof(bool), 
        typeof(bool), 
        typeof(GuestStatus))]
    internal static class RestUtility_FindBedFor_Patch
    {

        private static MethodInfo GetSortedBeds_MethodInfo = SymbolExtensions.GetMethodInfo((Pawn p) => ChildrenUtility.GetSortedBeds_RestEffectiveness(p));

        private static FieldInfo bedDefsBestToWorst_RestEffectivenessInfo = AccessTools.Field(typeof(RestUtility), "bedDefsBestToWorst_RestEffectiveness");
        /// <summary>
        /// Modify FindBedFor to pass in a different order for bedDefs when looking
        /// for the best bed for a pawn. We do not touch the order of beds for
        /// medical effectiveness, as a baby still needs to go to the most effective
        /// hospital bed if it is actually unwell.
        /// </summary>
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            foreach (CodeInstruction instruction in instructions) {
                //Convert all references to bedDefsBestToWorst_RestEffectiveness for toddler pawns
                if (instruction.opcode == OpCodes.Ldsfld && instruction.OperandIs(bedDefsBestToWorst_RestEffectivenessInfo)) {
                    yield return new CodeInstruction(OpCodes.Ldarg_1) { labels = instruction.labels.ListFullCopy() };
                    yield return new CodeInstruction(OpCodes.Call, GetSortedBeds_MethodInfo);
                }
                else {
                    yield return instruction;
                }
                    
            }
        }
    }
        
}