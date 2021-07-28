using RimWorld;
using Verse;

namespace BabiesAndChildren
{
    /// <summary>
    /// Static accessors for child hediff defs
    /// </summary>
    [DefOf]
    public static class BnCHediffDefOf
    {
        public static HediffDef UnhappyBaby;
        public static HediffDef BabyState0;

        static BnCHediffDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCHediffDefOf));

    }
}