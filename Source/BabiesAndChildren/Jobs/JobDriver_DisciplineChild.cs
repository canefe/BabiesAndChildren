using RimWorld;
using Verse;
using Verse.AI;
using System.Collections.Generic;
using System.Diagnostics;

namespace BabiesAndChildren
{
    // Child discipline is given from a social fight, so it doesn't use a workgiver
    
    public class JobDriver_DisciplineChild : JobDriver
    {
        private const int disciplineDuration = 100;
        
        protected Pawn Victim {
            get {
                return (Pawn)TargetA.Thing;
            }
        }
        
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Victim, this.job, 1, -1, null);
        }

        internal static BodyPartRecord GetRandomDisciplinePart(Pawn victim)
        {
            List<BodyPartDef> parts = new List<BodyPartDef> {
                BodyPartDefOf.Torso,
                BodyPartDefOf.Head,
                BodyPartDefOf.Leg,
                BodyPartDefOf.Leg,
                BodyPartDefOf.Arm,
                BodyPartDefOf.Arm
            };
            return victim.RaceProps.body.AllParts.FindAll (x => x.def == (BodyPartDef)parts.RandomElement()).RandomElement();
        }

        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull (TargetIndex.A);
            this.FailOnDowned (TargetIndex.A);
            yield return Toils_Goto.GotoThing (TargetIndex.A, PathEndMode.Touch);
            Toil prepare = new Toil();
            prepare.initAction = delegate
            {
                PawnUtility.ForceWait(Victim, disciplineDuration, Victim);
            };
            prepare.defaultCompleteMode = ToilCompleteMode.Delay;
            prepare.defaultDuration = disciplineDuration;
            yield return prepare;
            yield return new Toil
            {
                initAction = delegate
                {
                    int amount = Rand.Range(1,2);
                    Victim.TakeDamage(new DamageInfo(DamageDefOf.Blunt, amount, 0, -1f, GetActor(), GetRandomDisciplinePart(Victim), null));
                    this.AddEndCondition (() => JobCondition.Succeeded);
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}


