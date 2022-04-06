using System.Xml;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BabiesAndChildren.Harmony {

    /// <summary>
    /// Babies should not develop a tolerance for social joy since this is currently their only available joy source.
    /// </summary>
    [HarmonyPatch(typeof(Need_Joy), "GainJoy")]
    internal static class Need_Joy_GainJoy_Patch {
        [HarmonyPostfix]
        static void GainJoy_Patch(Need_Joy __instance, JoyKindDef joyKind, Pawn ___pawn)
        {
            if (!RaceUtility.PawnUsesChildren(___pawn) || 
                joyKind != JoyKindDefOf.Social ||
                AgeStages.IsOlderThan(___pawn, AgeStages.Toddler))
                return;
            
            DefMap<JoyKindDef, float> tolerances = Traverse.Create(__instance.tolerances).Field("tolerances").GetValue<DefMap<JoyKindDef, float>>();
            DefMap<JoyKindDef, bool> bored = Traverse.Create(__instance.tolerances).Field("bored").GetValue<DefMap<JoyKindDef, bool>>();
            tolerances[JoyKindDefOf.Social] = 0;
            bored[JoyKindDefOf.Social] = false;
        }
    }

    [HarmonyPatch(typeof(DirectXmlLoader), nameof(DirectXmlLoader.DefFromNode))]
    static class YDefFromNode
    {
        [HarmonyPrefix]
        public static bool Prefix(XmlNode node, LoadableXmlAsset loadingAsset, ref Def __result)
        {
            CLog.Warning("H");
            YDefFromNode.nodecount++;
            if (node.NodeType != XmlNodeType.Element)
            {
                return true;
            }
            CLog.Warning("H6");
            if (BnCSettings.toysdisabled)
            {
                CLog.Warning("H9");
                XmlAttributeCollection attributes = node.Attributes;
                if (((attributes != null) ? attributes["BnCToy"] : null) != null)
                {
                    CLog.Warning("H9797");
                    __result = null;
                    return false;
                }
            }
            return true;
        }

        public static int nodecount;
    }
}