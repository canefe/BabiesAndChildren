using RimWorld;
using Verse;
using Verse.Sound;

namespace BabiesAndChildren
{
    
    /// <summary>
    /// Whether a pawn should cry
    /// </summary>
    public class Hediff_UnhappyBaby : HediffWithComps
    {
        
        // Hide the hediff
        public override bool Visible => false;
        private bool CanBabyCry()
        {
            return pawn.health.capacities.CapableOf(PawnCapacityDefOf.Breathing) && pawn.health.capacities.CanBeAwake;
        }

        public void WhineAndCry()
        {
            if (!CheckUnhappy(pawn)) {
                pawn.health.RemoveHediff(this);
            } else if(CanBabyCry()){
                // Whine and cry
                MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDefOf.Mote_ColonistFleeing);
                SoundInfo info = SoundInfo.InMap (new TargetInfo (pawn.PositionHeld, pawn.MapHeld));
                info.volumeFactor = BnCSettings.cryVolume;
                SoundDef.Named ("Pawn_BabyCry").PlayOneShot(info);
            }
        }


        
        public override void PostMake ()
        {
            WhineAndCry ();
            base.PostMake ();
        }

        public override void Tick()
        {
            if (pawn.Spawned) {
                if (pawn.IsHashIntervalTick (1000)) {
                    LongEventHandler.ExecuteWhenFinished (delegate {
                        WhineAndCry ();
                    });
                }
            }
        }


        /// <summary>
        /// checks if a pawn satisfies the requirements for this hediff
        /// </summary>
        /// <returns>whether the pawn meets the requirements for this hediff</returns>
        public static bool CheckUnhappy(Pawn pawn)
        {
            if ((pawn?.needs == null) || (pawn.needs.food == null) || (pawn.needs.joy == null))
            {
                return false;
            }

            return
                (pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry) ||
                (pawn.needs.joy.CurLevelPercentage < 0.1f) ||
                pawn.health.HasHediffsNeedingTend();
        }
    }
}