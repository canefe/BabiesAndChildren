using System;
using BabiesAndChildren.api;
using UnityEngine;
using Verse;

namespace BabiesAndChildren
{
    public static class ModTools
    {
        public static bool IsModOn(string modName)
        {
            foreach (ModMetaData modMetaData in ModLister.AllInstalledMods)
            {
                if ((modMetaData != null) && (modMetaData.Active) && (modMetaData.Name.ToLower().StartsWith(modName.ToLower())))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsRobot(Pawn pawn)
        {
            return pawn != null && IsRobot(pawn.def);
        }

        public static bool IsRobot(ThingDef thingDef)
        {
            if (thingDef?.race == null)
                return false;

            bool isRobot = thingDef.race.IsMechanoid;

            if (!isRobot && ChildrenBase.ModAndroid_Tiers_ON)
            {
                string defName = thingDef.defName.ToLower();
                isRobot = defName.Contains("robot") || defName.Contains("android");
            }

            if (!isRobot && ChildrenBase.ModRimsecSecurity_ON)
            {
                string defName = thingDef.defName;
                isRobot = defName.Contains("RSPeacekeeperDefender") || defName.Contains("RSPeacekeeperEnforcer") || defName.Contains("RSPeacekeeperSentinel");
                
            }

            return isRobot;
        }

        public static float getMaxRJWHediffSeverity(Pawn pawn)
        {
            switch (AgeStages.GetAgeStage(pawn))
            {
                case AgeStages.Baby: return 0.07f;
                case AgeStages.Toddler: return ChildrenUtility.ToddlerIsUpright(pawn) ? 0.10f : 0.08f;
                case AgeStages.Child: return 0.12f;
                case AgeStages.Teenager: return 0.80f;
                default: return 1f;
            } 
        }
        public static void ChangeRJWHediffSeverity(Pawn pawn, bool Is_SizeInit, MathTools.Fixed_Rand rand)
        {
            if (!ChildrenBase.ModRimJobWorld_ON) return;

            float Maxsize = getMaxRJWHediffSeverity(pawn);

            
            Hediff anusHediff = Tools.HealthUtility.GetHediffNamed(pawn, "anus");
            if (anusHediff != null)
            {
                float newSize = Rand.Range(0.01f, Maxsize);
                float oldSize = anusHediff.Severity;
                anusHediff.Severity = Is_SizeInit ? newSize : Math.Max(oldSize, newSize);
            }
            Hediff penisHediff = Tools.HealthUtility.GetHediffNamed(pawn, "penis");
            if (penisHediff != null)
            {
                float newSize = Rand.Range(0.01f, Maxsize);
                float oldSize = penisHediff.Severity;
                penisHediff.Severity = Is_SizeInit ? newSize : Math.Max(newSize, oldSize);
            }

            Hediff vaginaHediff = Tools.HealthUtility.GetHediffNamed(pawn, "vagina");
            if (vaginaHediff != null)
            {
                float maxVaginaSize = Math.Max(0.35f, Maxsize);
                float newSize = Rand.Range(0.02f, maxVaginaSize);
                float oldSize = vaginaHediff.Severity;
                vaginaHediff.Severity = Is_SizeInit ? newSize : Math.Max(newSize, oldSize);
            }

            Hediff breastsHediff = Tools.HealthUtility.GetHediffNamed(pawn, "breasts");
            if (breastsHediff != null && vaginaHediff != null)
            {
                float maxBreastsSize = Maxsize;
                if (AgeStages.IsYoungerThan(pawn, AgeStages.Teenager))
                {
                    maxBreastsSize = Math.Min(maxBreastsSize, 0.07f);
                }
                float newSize = Rand.Range(0.01f, maxBreastsSize);
                float oldSize = breastsHediff.Severity;
                breastsHediff.Severity = Is_SizeInit ? newSize : Math.Max(oldSize, newSize);
                
            }
        }

        public static bool RemoveFacialAnimationComps(ThingWithComps thing)
        {
            ThingComp comp = ChildrenUtility.GetCompByClassName(thing, "FacialAnimation.DrawFaceGraphicsComp");
            return comp != null && thing.AllComps.Remove(comp);
        }
        
    }
}
