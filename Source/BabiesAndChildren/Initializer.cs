using BabiesAndChildren.Harmony;
using Verse;

namespace BabiesAndChildren
{
    [StaticConstructorOnStartup]
    public static class Initializer
    {
        static Initializer()
        {
            ChildrenBase.ModHAR_ON = ModTools.IsModOn("Humanoid Alien Races");
            ChildrenBase.ModFacialAnimation_ON = ModTools.IsModOn("[NL] Facial Animation - WIP");
            ChildrenBase.ModCSL_ON = ModTools.IsModOn("Children, school and learning");
            ChildrenBase.ModRimJobWorld_ON = ModTools.IsModOn("RimJobWorld");
            ChildrenBase.ModAndroid_Tiers_ON = ModTools.IsModOn("Android tiers");
            ChildrenBase.ModDressPatients_ON = ModTools.IsModOn("Dress Patients");
            ChildrenBase.ModDubsBadHygiene_ON = ModTools.IsModOn("Dubs Bad Hygiene");
            ChildrenBase.ModRimsecSecurity_ON = ModTools.IsModOn("Rimsec Security");
            ChildrenBase.ModMechadroids_ON = ModTools.IsModOn("[O21] Mechadroids");
            ChildrenBase.ModKVShowHair_ON = ModTools.IsModOn("[KV] Show Hair With Hats or Hide All Hats");
            
            if (ChildrenBase.ModHAR_ON)
            {
                CLog.Message("Humanoid Alien Races 2.0 is active. Enabling Alien children support.");
                AlienRacePatches.Patch();
            }
            else
            {
                ChildrenSizePatch.Patch();
            }

            if (ChildrenBase.ModCSL_ON)
            {
                CLog.Message("CSL is active.");
            }

            if (ChildrenBase.ModRimJobWorld_ON)
            {
                CLog.Message("RJW is active.");
            }

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

            if (ChildrenBase.ModMechadroids_ON)
            {
                CLog.Message("[O21] Mechadroids is active.");
            }

            if (ChildrenBase.ModKVShowHair_ON)
            {
                CLog.Message("Patching [KV] Show Hair");
                ShowHairPatches.Patch();
            }

        }
    }
}