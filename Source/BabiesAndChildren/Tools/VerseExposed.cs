using HarmonyLib;
using Verse;

namespace BabiesAndChildren.Tools
{
    public static class VerseExposed
    {
        /// <summary>
        /// Verse.PawnGenerator:GenerateSkills
        /// Effectively generates random skills and passions
        /// </summary>
        /// <param name="pawn">pawn whose skills will be set</param>
        public static void PawnGenerator_GenerateSkills(Pawn pawn)
        {
            try
            {
                Traverse.CreateWithType("PawnGenerator").Method("GenerateSkills", pawn).GetValue();
                CLog.DevMessage("Skills for: " + pawn.Name.ToStringShort + " randomly generated.");
            }
            catch
            {
                CLog.DevMessage("Skills for: " + pawn.Name.ToStringShort + " failed to randomly generate.");
            }
        }

        /// <summary>
        /// Verse.PawnGenerator:GenerateTraits
        /// Effectively generates random traits
        /// </summary>
        /// <param name="pawn">pawn whose traits will be set</param>
        public static void PawnGenerator_GenerateTraits(Pawn pawn)
        {
            try
            {
                PawnGenerationRequest request = new PawnGenerationRequest(pawn.kindDef, pawn.Faction);
                Traverse.CreateWithType("PawnGenerator").Method("GenerateTraits", pawn, request).GetValue();
                CLog.DevMessage("Traits for: " + pawn.Name.ToStringShort + " randomly generated.");
            }
            catch 
            {
                CLog.DevMessage("Traits for: " + pawn.Name.ToStringShort + " failed to randomly generate.");
            }
        }
    }
}