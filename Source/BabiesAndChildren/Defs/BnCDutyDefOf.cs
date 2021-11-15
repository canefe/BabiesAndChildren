using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace BabiesAndChildren
{
	[DefOf]
	public static class BnCDutyDefOf
    {	
		public static DutyDef PlayTime;
		static BnCDutyDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCDutyDefOf));
	}
	[DefOf]
	public static class BnCWorkTypeDefOf
    {
		public static WorkTypeDef BnC_Watch;
		static BnCWorkTypeDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCWorkTypeDefOf));
	}
	[DefOf]
	public static class BnCJobDefOf
	{
		public static JobDef BnC_Watch;
		public static JobDef PlayAround;
		public static JobDef ScoldChild;
		public static JobDef DisciplineChild;
		static BnCJobDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCJobDefOf));
	}
	[DefOf]
	public static class BnCSoundDefOf
	{
		public static SoundDef Pawn_BabyCry;
		static BnCSoundDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(BnCSoundDefOf));
	}
}
