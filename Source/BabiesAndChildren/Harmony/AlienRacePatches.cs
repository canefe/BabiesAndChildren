﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AlienRace;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace BabiesAndChildren.Harmony
{
    [HarmonyPatch]
    internal static class AlienRacePatches
    {
        static Dictionary<float, GraphicMeshSet> humanlikeBodySetModified = new Dictionary<float, GraphicMeshSet>();
        static Dictionary<float, GraphicMeshSet> humanlikeHeadSetModified = new Dictionary<float, GraphicMeshSet>();

        static Dictionary<float, GraphicMeshSet> humanlikeHairAverageSetModified =
            new Dictionary<float, GraphicMeshSet>();

        static Dictionary<float, GraphicMeshSet> humanlikeHairNarrowSetModified =
            new Dictionary<float, GraphicMeshSet>();

        static MethodInfo meshInfo = AccessTools.Method(AccessTools.TypeByName("MeshMakerPlanes"), "NewPlaneMesh", new[]
            {typeof(Vector2), typeof(bool), typeof(bool), typeof(bool)}, null);

        static GraphicMeshSet GetModifiedBodyMeshSet(float bodySizeFactor, Pawn pawn)
        {
            if (!humanlikeBodySetModified.ContainsKey(bodySizeFactor))
            {
                humanlikeBodySetModified.Add(bodySizeFactor, new GraphicMeshSet(1.5f * bodySizeFactor));
            }

            return humanlikeBodySetModified[bodySizeFactor];
        }

        static GraphicMeshSet GetModifiedHeadMeshSet(float headSizeFactor, Pawn pawn)
        {
            if (!humanlikeHeadSetModified.ContainsKey(headSizeFactor))
            {
                humanlikeHeadSetModified.Add(headSizeFactor, new GraphicMeshSet(1.5f * headSizeFactor));
            }

            return humanlikeHeadSetModified[headSizeFactor];
        }

        static GraphicMeshSet GetModifiedHairMeshSet(float hairSizeFactor, Pawn pawn)
        {
            GraphicMeshSet result;
            if (pawn.story.crownType == CrownType.Average)
            {
                if (!humanlikeHairAverageSetModified.ContainsKey(hairSizeFactor))
                {
                    humanlikeHairAverageSetModified.Add(hairSizeFactor, new GraphicMeshSet(1.5f * hairSizeFactor));
                }

                result = humanlikeHairAverageSetModified[hairSizeFactor];
            }
            else
            {
                if (pawn.story.crownType == CrownType.Narrow)
                {
                    if (!humanlikeHairNarrowSetModified.ContainsKey(hairSizeFactor))
                    {
                        humanlikeHairNarrowSetModified.Add(hairSizeFactor,
                            new GraphicMeshSet(1.3f * hairSizeFactor, 1.5f * hairSizeFactor));
                    }

                    result = humanlikeHairNarrowSetModified[hairSizeFactor];
                }
                else
                {
                    if (!humanlikeHairAverageSetModified.ContainsKey(hairSizeFactor))
                    {
                        humanlikeHairAverageSetModified.Add(hairSizeFactor, new GraphicMeshSet(1.5f * hairSizeFactor));
                    }

                    result = humanlikeHairAverageSetModified[hairSizeFactor];
                }
            }

            return result;
        }


        //get scaled mesh sets for children bodies and heads
        [HarmonyPatch(typeof(HarmonyPatches), "GetPawnMesh")]
        static class GetPawnMesh_Patch
        {
            [HarmonyPostfix]
            static void Postfix(PawnRenderFlags renderFlags, Pawn pawn, Rot4 facing, bool wantsBody, ref Mesh __result)
            {
                if (pawn == null ||
                    !RaceUtility.PawnUsesChildren(pawn) ||
                    !AgeStages.IsAgeStage(pawn, AgeStages.Child))
                    return;

                __result = wantsBody ? 
                    GetModifiedBodyMeshSet(ChildrenUtility.GetBodySize(pawn), pawn).MeshAt(facing) : 
                    GetModifiedHeadMeshSet(ChildrenUtility.GetHeadSize(pawn), pawn).MeshAt(facing);
            }
        }

        
        //Get scaled hair meshes for children bodies and heads
        [HarmonyPatch(typeof(HarmonyPatches), "GetPawnHairMesh")]
        static class GetPawnHairMesh_Patch
        {
            [HarmonyPostfix]
            static void Postfix(PawnRenderFlags renderFlags, Pawn pawn, Rot4 headFacing, PawnGraphicSet graphics, ref Mesh __result)
            {
                if (pawn == null || 
                    !RaceUtility.PawnUsesChildren(pawn) ||
                    !AgeStages.IsAgeStage(pawn, AgeStages.Child)) 
                    return;
                
                float hairSizeFactor = ChildrenUtility.GetHairSize(0, pawn);
                __result = GetModifiedHairMeshSet(hairSizeFactor, pawn).MeshAt(headFacing);
            }
        }

        [HarmonyPatch(typeof(HarmonyPatches), "DrawAddons")]
        static class DrawAddons_Patch
        {
            static Dictionary<Vector2, Mesh> addonMeshs = new Dictionary<Vector2, Mesh>();
            static Dictionary<Vector2, Mesh> addonMeshsFlipped = new Dictionary<Vector2, Mesh>();


            
            //Patch to draw addons for children
            [HarmonyPrefix]
            static bool Prefix(PawnRenderFlags renderFlags, Vector3 vector, Vector3 headOffset, Pawn pawn, Quaternion quat, Rot4 rotation)
            {
                try
                {
                    
                    if (!(pawn.def is ThingDef_AlienRace alienProps) ||
                        !RaceUtility.PawnUsesChildren(pawn) || 
                        AgeStages.IsOlderThan(pawn, AgeStages.Child)) //draw addons normally
                        return true;

                    if (AgeStages.IsYoungerThan(pawn, AgeStages.Child)) //don't draw addons for babies and toddlers
                        return false;
                    
                    
                    
                    float bodySizeFactor = ChildrenUtility.GetBodySize(pawn);
                    
                    float moffsetZfb = 1f;
                    float moffsetXfb = 1f;
                    
                    float moffsetZfa = 1f;
                    float moffsetXfa = 1f;
                    
                    bool IsEastWestFlipped = false;

                    
                    if (pawn.def.defName == "Alien_Orassan")
                    {
                        moffsetZfb = 1.4f;
                        moffsetXfb = 0.1f;
                        moffsetZfa = 1.4f;
                        moffsetXfa = 1.4f;

                        if (rotation == Rot4.West)
                        {
                            IsEastWestFlipped = true;
                        }
                    }
                    else if (pawn.kindDef.race.ToString().ToLower().Contains("lizardman") && rotation == Rot4.East)
                    {
                        IsEastWestFlipped = true;
                    }
                    else if(rotation == Rot4.West)
                    {
                        IsEastWestFlipped = true;
                    }

                    List<AlienPartGenerator.BodyAddon> addons = alienProps.alienRace.generalSettings.alienPartGenerator.bodyAddons;
                    AlienPartGenerator.AlienComp alienComp = pawn.GetComp<AlienPartGenerator.AlienComp>();

                    for (int i = 0; i < addons.Count; i++)
                    {
                        //straight from alien race
                        AlienPartGenerator.BodyAddon ba = addons[index: i];
                        if (!ba.CanDrawAddon(pawn: pawn)) continue;
                        
                        //Special code 2.
                        if (BnCSettings.human_like_head_enabled && 
                            RaceUtility.HasHumanlikeHead(pawn) &&
                            ba.bodyPart.Contains("Head")) 
                            continue;
                        
                        //straight from alien race
                        AlienPartGenerator.RotationOffset offset = rotation == Rot4.South ? ba.offsets.south :
                            rotation == Rot4.North ? ba.offsets.north :
                            rotation == Rot4.East ? ba.offsets.east :
                            ba.offsets.west;
                        

                        Vector3 offsetVector = (ba.defaultOffsets.GetOffset(rotation)?.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, alienComp.crownType) ?? Vector3.zero) +
                                          (ba.offsets.GetOffset(rotation)?.GetOffset(renderFlags.FlagSet(PawnRenderFlags.Portrait), pawn.story.bodyType, alienComp.crownType) ?? Vector3.zero);

                        //straight from alien race
                        offsetVector.y = ba.inFrontOfBody ? 0.3f + offsetVector.y : -0.3f - offsetVector.y;
                        float num = ba.angle;
                        
                        //special code 3 that initializes mesh passed to DrawMeshNowOrLater at end
                        if (!addonMeshsFlipped.ContainsKey(ba.drawSize * bodySizeFactor))
                        {
                            addonMeshsFlipped.Add(ba.drawSize * bodySizeFactor, (Mesh) meshInfo.Invoke(null,
                                new object[]
                                    {new Vector2(bodySizeFactor * 1.5f, bodySizeFactor * 1.5f), true, false, false}));
                        }

                        if (!addonMeshs.ContainsKey(ba.drawSize * bodySizeFactor))
                        {
                            addonMeshs.Add(ba.drawSize * bodySizeFactor, (Mesh) meshInfo.Invoke(null, new object[]
                                {new Vector2(bodySizeFactor * 1.5f, bodySizeFactor * 1.5f), false, false, false}));
                        }

                        Mesh mesh = IsEastWestFlipped
                            ? addonMeshsFlipped[ba.drawSize * bodySizeFactor]
                            : addonMeshs[ba.drawSize * bodySizeFactor];

                        
                        //Straight from alien race
                        if (rotation == Rot4.North)
                        {
                            if (ba.layerInvert)
                                offsetVector.y = -offsetVector.y;
                            num = 0;
                        }
                        
                        //straight from alien race 


                        //special code 4.1 (initialize offset vector coords)
                        /* if (ba.bodyPart.Contains("tail"))
                         {
                             moffsetX *= bodySizeFactor * moffsetXfa;
                             moffsetZ *= bodySizeFactor * moffsetZfa;
                         }
                         else
                         {
                             moffsetX *= bodySizeFactor * moffsetXfb;
                             moffsetZ *= bodySizeFactor * moffsetZfb;
                         }*/

                        //straight from alien race

                        if (rotation == Rot4.East)
                        {
                            num = -num; //Angle
                            offsetVector.x = -offsetVector.x;
                        }

                        /* //special code 4.2 (instantiate offset vector)
                        Vector3 offsetVector = new Vector3(x: moffsetX, y: moffsetY,
                            z: (moffsetZ * Tweaks.G_offsetfac) + Tweaks.G_offset);

                        GenDraw.DrawMeshNowOrLater(mesh,
                            vector + offsetVector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f),
                            Quaternion.AngleAxis(baAngle, Vector3.up) * quat,
                            alienComp.addonGraphics[i].MatAt(rotation, null), portrait); */
                    }

                    return false;
                }
                catch
                {
                    // Ignored
                }

                return true;
            }
        }
    }
}