using System;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using RimWorld;
using UnityEngine;
using Verse;
using LifeStageUtility = BabiesAndChildren.Tools.LifeStageUtility;

namespace BabiesAndChildren
{
    public static class GraphicTools
    {
        public static void ResolveAgeGraphics(PawnGraphicSet graphics)
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (!RaceUtility.PawnUsesChildren(graphics.pawn)) return;
                if (graphics.pawn.story.hairDef != null)
                {
                    graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>(graphics.pawn.story.hairDef.texPath, ShaderDatabase.Cutout, Vector2.one, graphics.pawn.story.hairColor);
                }
                if (AgeStages.IsOlderThan(graphics.pawn, AgeStages.Child)) return;                

                // The pawn is a baby
                if (AgeStages.IsAgeStage(graphics.pawn, AgeStages.Baby))
                {
                    graphics.nakedGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Humanlike/Children/Bodies/Newborn", ShaderDatabase.CutoutSkin, Vector2.one, graphics.pawn.story.SkinColor);
                    graphics.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Children/Bodies/Newborn", ShaderDatabase.CutoutSkin, Vector2.one, PawnGraphicSet.RottingColorDefault);
                    graphics.headGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/null", ShaderDatabase.Cutout, Vector2.one, Color.white);
                    graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/null", ShaderDatabase.Cutout, Vector2.one, Color.white);
                }

                // The pawn is a toddler
                if (AgeStages.IsAgeStage(graphics.pawn, AgeStages.Toddler))
                {
                    string upright = "";
                    if (ChildrenUtility.ToddlerIsUpright(graphics.pawn))
                    {
                        upright = "Upright";
                    }
                    graphics.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Children/Bodies/Toddler" + upright, ShaderDatabase.CutoutSkin, Vector2.one, graphics.pawn.story.SkinColor);
                    graphics.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Children/Bodies/Toddler" + upright, ShaderDatabase.CutoutSkin, Vector2.one, PawnGraphicSet.RottingColorDefault);
                    graphics.headGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/null", ShaderDatabase.Cutout, Vector2.one, Color.white);
                    graphics.hairGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/null", ShaderDatabase.Cutout, Vector2.one, Color.white);                    
                }
                // The pawn is a child
                else if (AgeStages.IsAgeStage(graphics.pawn, AgeStages.Child))
                {
                    if (RaceUtility.IsHuman(graphics.pawn))
                    {               
                        graphics.headGraphic = GetChildHeadGraphics(ShaderDatabase.CutoutSkin, graphics.pawn.story.SkinColor);
                    }
                    else if (BnCSettings.human_like_head_enabled && RaceUtility.HasHumanlikeHead(graphics.pawn))
                    {
                        graphics.headGraphic = GetChildHeadGraphics(ShaderDatabase.CutoutSkin, graphics.pawn.story.SkinColor);
                    }
                    else if (BnCSettings.Rabbie_Child_head_enabled && graphics.pawn.def.defName == "Rabbie")
                    {
                        graphics.headGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Children/Heads/RabbieChild", ShaderDatabase.Cutout, Vector2.one, Color.white);
                    }
                }
            });
        }

        public static Graphic GetChildHeadGraphics(Shader shader, Color skinColor)
        {
            Graphic_Multi graphic = null;
            string str = "Male_Child";
            string path = "Things/Pawn/Humanlike/Children/Heads/" + str;
            graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, Vector2.one, skinColor) as Graphic_Multi;
            return graphic;
        }

        public static Vector3 ModifyChildYPosOffset(Vector3 pos, Pawn pawn)
        {
            Vector3 newPos = pos;

            // move the draw target down to compensate for child shortness
            if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsAgeStage(pawn, AgeStages.Child) && !pawn.InBed())
            {
                if (RaceUtility.IsHuman(pawn)) newPos.z += BnCSettings.HumanrootlocZ;
                else newPos.z += BnCSettings.AlienrootlocZ;
                //newPos.z -= 0.15f;            
            }
            if (pawn.InBed() && pawn.CurrentBed().def.size.z == 1)
            {
                Building_Bed bed = pawn.CurrentBed();
                // Babies and toddlers get drawn further down along the bed
                if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsYoungerThan(pawn, AgeStages.Child))
                {
                    Vector3 vector = new Vector3(0, 0, 0.1f).RotatedBy(bed.Rotation.AsAngle);
                    newPos -= vector;
                    // ... as do children, but to a lesser extent
                }
                else if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsAgeStage(pawn, AgeStages.Child))
                {
                    if (RaceUtility.IsHuman(pawn)) newPos.z += BnCSettings.HumanrootlocZ;
                    else newPos.z += BnCSettings.AlienrootlocZ;
                    // Are we in a crib?
                    Vector3 vector = new Vector3(0, 0, 0.2f).RotatedBy(bed.Rotation.AsAngle);
                    
                    newPos -= vector;
                }
                newPos += new Vector3(0, 0, 0.2f);

            }
            return newPos;
        }

        public static float GetBodySizeScaling(Pawn pawn)
        {
            float num = 1f;
            float num2 = 1f;
            Pawn_AgeTracker ageTracker = pawn.ageTracker;

            float RoundFloat(float x) => (float) Math.Round((double) x, 2);

            try
            {
                int curLifeStageIndex = AgeStages.GetAgeStage(pawn);
                int lastLifeStageIndex = pawn.RaceProps.lifeStageAges.Count - 1;

                LifeStageAge curLifeStageAge = LifeStageUtility.GetLifeStageAge(pawn, curLifeStageIndex);
                float curBodySizeFactor = curLifeStageAge.def.bodySizeFactor;
                float currLifeStageProgression = ageTracker.AgeBiologicalYearsFloat - curLifeStageAge.minAge;

                num = curBodySizeFactor;
                    
                //at the last lifestage and the last lifestage is not the first
                if ((lastLifeStageIndex == curLifeStageIndex) && (curLifeStageIndex != 0) && (curBodySizeFactor != 1f))
                {
                    LifeStageAge prevLifeStageAge = LifeStageUtility.GetPreviousLifeStageAge(pawn);
                    float prevBodySizeFactor = prevLifeStageAge.def.bodySizeFactor;
                    float prevLifeStageDuration = curLifeStageAge.minAge - prevLifeStageAge.minAge;
                        
                    num = prevBodySizeFactor + RoundFloat(
                        (curBodySizeFactor - prevBodySizeFactor) /
                        (curLifeStageAge.minAge - prevLifeStageAge.minAge) *
                        (currLifeStageProgression + prevLifeStageDuration));
                } 
                else if (pawn.RaceProps.lifeStageAges.Count <= 1)
                {
                    num = pawn.RaceProps.baseBodySize;
                }
                else
                {
                    LifeStageAge nextLifeStageAge = LifeStageUtility.GetNextLifeStageAge(pawn);
                    float nextBodySizeFactor = nextLifeStageAge.def.bodySizeFactor;
                    float currLifeStageDuration = nextLifeStageAge.minAge - curLifeStageAge.minAge;
                        
                    num = curLifeStageAge.def.bodySizeFactor + RoundFloat((nextBodySizeFactor - curBodySizeFactor) /
                        currLifeStageDuration * currLifeStageProgression);
                }
                if (pawn.RaceProps.baseBodySize > 0f)
                {
                    num2 = pawn.RaceProps.baseBodySize;
                }
            }
            catch
            {
                // Ignored
            }
            return num * num2;
            
        }
    }
}