using BabiesAndChildren.Harmony;
using Verse;

namespace BabiesAndChildren
{
    [StaticConstructorOnStartup]
    public static class Initializer
    {
        static Initializer()
        {
            ChildrenBase.ModFacialAnimation_ON = ModTools.IsModOn("[NL] Facial Animation - WIP");
            ChildrenBase.ModCSL_ON = ModTools.IsModOn("Children, school and learning");
            ChildrenBase.ModRimJobWorld_ON = ModTools.IsModOn("RimJobWorld");
            ChildrenBase.ModAndroid_Tiers_ON = ModTools.IsModOn("Android tiers");
            ChildrenBase.ModDressPatients_ON = ModTools.IsModOn("Dress Patients");
            ChildrenBase.ModDubsBadHygiene_ON = ModTools.IsModOn("Dubs Bad Hygiene");
            ChildrenBase.ModRimsecSecurity_ON = ModTools.IsModOn("Rimsec Security");
            if (ChildrenBase.ModFacialAnimation_ON)
            {
                CLog.Message("Patching Facial Animation");
                FacialAnimationPatches.Patch();
            }

            if (ChildrenBase.ModDressPatients_ON)
            {
                CLog.Message("Patching Dress Patients");
                DressPatientsPatches.Patch();
            }

            if (ChildrenBase.ModDubsBadHygiene_ON)
            {
                CLog.Message("Patching Dubs Bad Hygiene");
                DubsBadHygienePatches.Patch();
            }
        }
    }
}