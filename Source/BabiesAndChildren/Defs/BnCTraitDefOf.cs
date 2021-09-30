using RimWorld;
using System.Security.Policy;

namespace BabiesAndChildren
{
    [DefOf]
    public static class BnCTraitDefOf
    {
        public static TraitDef Innocent = TraitDef.Named("Innocent");
        public static TraitDef Newtype = TraitDef.Named("Newtype");

        static BnCTraitDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCTraitDefOf));

    }
    [DefOf]
    public static class BnCThoughtDefOf
    {
        public static ThoughtDef JustBorn;
        public static ThoughtDef BabyNuzzled;
        public static ThoughtDef ReceivedPraise;
        public static ThoughtDef WeHadBabies;
        public static ThoughtDef BabyStillborn;
        public static ThoughtDef ChildGames;
        public static ThoughtDef IGaveBirth;
        public static ThoughtDef PartnerGaveBirth;
        public static ThoughtDef MyChildGrowing;

        static BnCThoughtDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCThoughtDefOf));
    }
}