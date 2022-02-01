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

        public static ThoughtDef TeenFeelingGood;
        public static ThoughtDef TeenFeelingBored;
        public static ThoughtDef TeenFeelingBad;
        public static ThoughtDef TeenFeelingStressed;
        public static ThoughtDef TeenFeelingCheerful;
        public static ThoughtDef TeenFeelingSad;
        public static ThoughtDef TeenFeelingTired;

        static BnCThoughtDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCThoughtDefOf));
    }
}