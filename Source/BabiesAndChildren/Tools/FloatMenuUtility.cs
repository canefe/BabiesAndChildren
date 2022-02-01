using RimWorld;
using System.Collections.Generic;
using System;
using BabiesAndChildren.api;
using Verse;
using Verse.AI.Group;

namespace BabiesAndChildren
{
    public static class BnC_FloatMenuUtility
    {
        public static void AddPlayOptions(Pawn target, Pawn pawn, List<FloatMenuOption> opts)
        {
            
            bool flag = (BnCSettings.playtime_enabled && pawn.IsChildSupported() && (AgeStages.IsAgeStage(pawn, AgeStages.Child) || AgeStages.IsAgeStage(pawn, AgeStages.Toddler))) && target.IsChildSupported() && (AgeStages.IsAgeStage(target, AgeStages.Child) || AgeStages.IsAgeStage(target, AgeStages.Toddler));
            if (!flag)
            {
                return;
            }



            Action action = delegate ()
            {
                pawn.jobs.StopAll(false, true);
                if (target.GetLord() == null || target.GetLord().LordJob == null)
                {
                    Lord lord = LordMaker.MakeNewLord(pawn.Faction, new LordJob_PlayTime(pawn, target), pawn.Map, new Pawn[]
                    {
            pawn,
            target
                    });
                }
                else if (target.GetLord().LordJob.GetType() == typeof(LordJob_PlayTime))
                {
                    target.GetLord().AddPawn(pawn);
                }

            };
            string str = "Play with ";
            opts.Add(new FloatMenuOption(str + target.NameShortColored, action, MenuOptionPriority.Low, null, null, 0f, null, null));

        }
    }
}
