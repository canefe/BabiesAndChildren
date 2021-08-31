using System;
using RimWorld;
using Verse.AI;

namespace BabiesAndChildren.Defs
{
	[DefOf]
	public static class BnCDutyDefOf
    {	
		public static DutyDef PlayTime;
		static BnCDutyDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCDutyDefOf));
	}
}
