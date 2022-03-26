using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BabiesAndChildren.api;
using HarmonyLib;
using Verse;
using QuickFast;
namespace BabiesAndChildren.Harmony
{
	public static class DubsApparelTweaksPatches
	{

		public static void Patch()
		{
			HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("RimWorld.babies.and.children." + nameof(DubsApparelTweaksPatches));
			harmony.Patch(AccessTools.Method(typeof(bs.H_DrawHeadHair), "Prefix"), transpiler: new HarmonyMethod(typeof(DubsApparelTweaksPatches), nameof(H_DrawHeadHair_Patch)));
		}


        private static IEnumerable<CodeInstruction> H_DrawHeadHair_Patch(IEnumerable<CodeInstruction> instructions)
        {
            var list = new List<CodeInstruction>(instructions);

            //find call to DrawMeshNowOrLaterWithScale
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var instruction = list[i];
                if ((instruction == null) || (instruction.operand == null))
                    continue;
                if (instruction.opcode == OpCodes.Callvirt && instruction.operand.ToString().Contains("get_HairMeshSet"))
                    index = i;
            }
            //didn't find the call
            if (index == -1)
            {
                CLog.Warning("call to FacialAnimation.GraphicHelper:DrawMeshNowOrLaterWithScale not found");
                return null;
            }
            //remove instruction to load scale 


            //
            //Change call to DrawMeshNowOrLaterWithScale to use GetBodySizeScaling(this.pawn) instead of local float
            //
            index -= 2;
            var instructionsToInsert = new List<CodeInstruction>();
            //ldfld        class ['Assembly-CSharp']Verse.Pawn FacialAnimation.DrawFaceGraphicsComp::pawn
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_R4, 0f));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert.Add(CodeInstruction.LoadField(typeof(PawnRenderer), "graphics"));
            instructionsToInsert.Add(CodeInstruction.LoadField(typeof(PawnGraphicSet), "pawn"));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ChildrenUtility), "GetHairSize")));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert.Add(CodeInstruction.LoadField(typeof(PawnRenderer), "graphics"));
            instructionsToInsert.Add(CodeInstruction.LoadField(typeof(PawnGraphicSet), "pawn"));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AlienRacePatches), "GetModifiedHairMeshSet")));
            if (index != -1)
            {
                list.InsertRange(index, instructionsToInsert);
            }
            int a = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var instruction = list[i];
                if ((instruction == null) || (instruction.operand == null))
                    continue;
                if (instruction.opcode == OpCodes.Call && instruction.operand.ToString().Contains("GetModifiedHairMeshSet"))
                    a = i;
            }
            list.RemoveRange(a + 1, 3);
            return list.AsEnumerable();
        }
    }
}