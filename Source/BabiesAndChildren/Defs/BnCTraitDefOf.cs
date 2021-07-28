using RimWorld;

namespace BabiesAndChildren
{
    [DefOf]
    public static class BnCTraitDefOf
    {
        public static TraitDef Innocent = TraitDef.Named("Innocent");
        public static TraitDef Newtype = TraitDef.Named("Newtype");

        static BnCTraitDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCTraitDefOf));

    }
}