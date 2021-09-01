using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BabiesAndChildren.api;
using HarmonyLib;
using Verse;

namespace BabiesAndChildren.Harmony
{
	public static class ShowHairPatches
	{

		private static Type showHairType;
		public static void Patch()
		{
			HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("RimWorld.babies.and.children." + nameof(ShowHairPatches));
			showHairType = AccessTools.TypeByName("ShowHair.Patch_PawnRenderer_DrawHeadHair");
			MethodInfo original = AccessTools.Method(showHairType, "HideHats");
			HarmonyMethod transpiler = new HarmonyMethod(typeof(ShowHairPatches), "HideHatsTranspiler");
			harmony.Patch(original, transpiler: transpiler);
		}


		private static IEnumerable<CodeInstruction> HideHatsTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
		{
			var code = new List<CodeInstruction>(instructions);

			int insertionIndex = -1;
			Label return566Label = il.DefineLabel();
			for (int i = 0; i < code.Count; i++) // -1 since we will be checking i + 1
			{
				if ((code[i] == null) || (code[i].operand == null))
					continue;
				if (code[i].opcode == OpCodes.Stsfld && code[i].operand.ToString().Contains("skipDontShaveHead"))
				{
					insertionIndex = i;
					code[i].labels.Add(return566Label);
					CLog.Warning(code[i].operand.ToString());
					break;
				}
			}
			if (insertionIndex == -1)
			{
				CLog.Warning("call to showhair not found");
				return null;
			}
			var instructionsToInsert = new List<CodeInstruction>();
			CodeInstruction loadPawn = CodeInstruction.LoadField(showHairType, "pawn");
			instructionsToInsert.Add(loadPawn);
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ModTools), "ShouldHideHair")));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_0));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Brfalse_S, return566Label));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4, 1));
			instructionsToInsert.Add(new CodeInstruction(OpCodes.Stind_I, 1));
			if (insertionIndex != -1)
			{
				code.InsertRange(insertionIndex, instructionsToInsert);
			}
			return code;
		}
	}
}