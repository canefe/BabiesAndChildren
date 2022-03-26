using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using System.Collections.Generic;
using HugsLib;
using RimWorld;
using Verse;

namespace BabiesAndChildren
{
    public class ChildrenBase : ModBase
    {
        public static ChildrenBase Instance { get; private set; }
        
        public override string ModIdentifier => "Babies_and_Children";

        //Humanoid Alien Races
        public static bool ModHAR_ON = false;
        //Children School and Learning
        public static bool ModCSL_ON = false;
        //RimJobWorld
        public static bool ModRimJobWorld_ON = false;
        //Facial Animation - WIP
        public static bool ModFacialAnimation_ON = false;
        //Android tiers
        public static bool ModAndroid_Tiers_ON = false;
        //Dress Patients
        public static bool ModDressPatients_ON = false;
        //Dubs Bad Hygiene
        public static bool ModDubsBadHygiene_ON;
        //Rimsec Security
        public static bool ModRimsecSecurity_ON;
        //Mechadroids
        public static bool ModMechadroids_ON;
        //ModKVShowHair_ON
        public static bool ModKVShowHair_ON;
        //Age Matters
        public static bool ModAgeMatters_ON;
        //Medpod
        public static bool ModMedpod_ON;
        //SOS2
        public static bool ModSOS2_ON;
        //Dubs Apparel Tweaks
        public static bool ModDAT_ON;

        private ChildrenBase()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            RaceUtility.ClearCache();
            CLog.Message("Adding CompProperties_Growing to races.");
            foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (RaceUtility.ThingUsesChildren(thingDef))
                {
                    thingDef.comps.Add(new CompProperties_Growing());         
                }

                List<string> list = new List<string>();
                bool flag13 = thingDef.inspectorTabsResolved == null;
                if (flag13)
                {
                    thingDef.inspectorTabsResolved = new List<InspectTabBase>(1);
                }
                thingDef.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Watch)));
            }

        }

        public static void ReinitializeChildren(Map map)
        {
                CLog.Message("Resetting body types of all children.");
                    foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
                    {
                        if (pawn?.story == null)
                            continue;
                        if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsYoungerThan(pawn ,AgeStages.Teenager))
                        {
                            if(pawn.story.bodyType == null)
                                pawn.story.bodyType = ((pawn.gender == Gender.Female) ? BodyTypeDefOf.Female : BodyTypeDefOf.Male);
                            
                            Growing_Comp comp = pawn.TryGetComp<Growing_Comp>();
                            
                            comp?.Initialize(true);
                        }
                    }
            
        }

        public override void MapLoaded(Map map)
        {
            if (map != Current.Game.CurrentMap) return;
            if (BnCSettings.GestationPeriodDays_Enable)
            {
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
                {
                    if (RaceUtility.ThingUsesChildren(thingDef))
                    {
                        thingDef.race.gestationPeriodDays = BnCSettings.GestationPeriodDays;
                    }
                }
            }
            if (!BnCSettings.watchworktype_enabled)
            {
                WatchCardUtility.RemoveWorkType(BnCWorkTypeDefOf.BnC_Watch, map);
            }
            else {
                WatchCardUtility.RemoveWorkTypeAdult(BnCWorkTypeDefOf.BnC_Watch, map);
            }
            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsYoungerThan(pawn, AgeStages.Adult, true))
                {
                    if (pawn.story.bodyType == null)
                        pawn.story.bodyType = ((pawn.gender == Gender.Female) ? BodyTypeDefOf.Female : BodyTypeDefOf.Male);
                        Growing_Comp comp = pawn.TryGetComp<Growing_Comp>();
                        comp?.Initialize();
                }
            }
        }
    }


}