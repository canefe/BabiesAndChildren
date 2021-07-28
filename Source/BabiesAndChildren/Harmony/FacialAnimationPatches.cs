using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace BabiesAndChildren.Harmony
{
    public static class FacialAnimationPatches
    {
        
        private static Type drawFaceGraphicsCompType;
        public static void Patch()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("RimWorld.babies.and.children." + nameof(FacialAnimationPatches));
            
            drawFaceGraphicsCompType = AccessTools.TypeByName("FacialAnimation.DrawFaceGraphicsComp");
            
            MethodInfo original = AccessTools.Method(drawFaceGraphicsCompType, "DrawBodyPart", generics:new[] {AccessTools.TypeByName("IFacialAnimationController")});
            HarmonyMethod transpiler = new HarmonyMethod(typeof(FacialAnimationPatches), "DrawBodyPartTranspiler");
            harmony.Patch(original, transpiler: transpiler);
        }


        
        private static IEnumerable<CodeInstruction> DrawBodyPartTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new List<CodeInstruction>(instructions);
            
            //find call to DrawMeshNowOrLaterWithScale
            int index = -1;
            for (int i = 0; i < list.Count; i++)
            {
                CodeInstruction instruction = list[i];
                if((instruction == null) || (instruction.operand == null))
                    continue;
                
                if (instruction.operand.ToString().Contains("DrawMeshNowOrLaterWithScale"))
                {
                    index = i;
                    break;
                }
            }
            //didn't find the call
            if (index == -1)
            {
                CLog.Warning("call to FacialAnimation.GraphicHelper:DrawMeshNowOrLaterWithScale not found");
                return null;
            }
            //remove instruction to load scale 
            index -= 1;
            list.RemoveAt(index);
          
            //
            //Change call to DrawMeshNowOrLaterWithScale to use GetBodySizeScaling(this.pawn) instead of local float
            //
            CodeInstruction loadThis = new CodeInstruction(OpCodes.Ldarg_0);
            //ldfld        class ['Assembly-CSharp']Verse.Pawn FacialAnimation.DrawFaceGraphicsComp::pawn
            CodeInstruction loadPawn = CodeInstruction.LoadField(drawFaceGraphicsCompType, "pawn");
            CodeInstruction callGetBodySizeScaling =
                CodeInstruction.Call(typeof(GraphicTools), "GetBodySizeScaling");
            list.InsertRange(index, new[] {loadThis, loadPawn, callGetBodySizeScaling});
            
            return (IEnumerable<CodeInstruction>) list;
        }
    }
}