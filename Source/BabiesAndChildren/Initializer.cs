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
            ChildrenBase.ModAgeMatters_ON = ModTools.IsModOn("Age Matters 2.0 [1.2]");
            ChildrenBase.ModMedpod_ON = ModTools.IsModOn("MedPod");
            ChildrenBase.ModSOS2_ON = ModTools.IsModOn("Save Our Ship 2");
            
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
                BnCSettings.HumanHeadSize = BnCSettings.HumanHeadSize == 1.4083f ? 0.9402f : BnCSettings.HumanHeadSize;
                BnCSettings.HumanHairSize = BnCSettings.HumanHeadSize == 1.2589f ? 1.0697f : BnCSettings.HumanHairSize;
            }
            else
            {
                if (BnCSettings.HumanHeadSize == 0.9402f)
                {
                    BnCSettings.HumanHeadSize = 1.4083f;
                }
                if (BnCSettings.HumanHairSize == 1.0697f)
                {
                    BnCSettings.HumanHairSize = 1.2589f;
                }
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

            if (ChildrenBase.ModMedpod_ON)
            {
                CLog.Message("Patching MedPod");
                MedPodPatches.Patch();
            }

            if (BnCSettings.AlienBodySize ==  0.842f || BnCSettings.HumanHeadSize == 0.922f || BnCSettings.HumanHairSize == 0.730f)
            {
                CLog.Warning("Old settings values are found, changing to new values.");
                BnCSettings.HumanBodySize = 1.1095f;
                BnCSettings.HumanHeadSize = ChildrenBase.ModFacialAnimation_ON ? 0.9402f : 1.4083f;
                BnCSettings.HumanHairSize = ChildrenBase.ModFacialAnimation_ON ? 1.0697f : 1.2589f;
                BnCSettings.HumanrootlocZ = -0.11f;
                BnCSettings.AlienTeenagerModifier = 1f;
                BnCSettings.HumanTeenagerModifier = 1f;
                BnCSettings.AlienBodySize = 1f;
                BnCSettings.AlienHeadSizeA = 1f;
                BnCSettings.AlienHeadSizeB = 1f;
                BnCSettings.AlienHairSize = 1f;
                BnCSettings.AlienrootlocZ = -0.038f;
                BnCSettings.FAModifier = 0.8058f;
            }
            //typo fix
            if (BnCSettings.AlienHairSize == 1.896f)
            {
                BnCSettings.AlienHairSize = 1f;
            }
        }
    }
}