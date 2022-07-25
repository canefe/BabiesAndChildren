using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI.Group;
using BabiesAndChildren.api;
namespace BabiesAndChildren
{
    public class ThoughtWorker_Precept_Children : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager)))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager)))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager)))
                        return true;
                }
                else if (map.mapPawns.AllPawnsSpawned.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && !p.HostileTo(c)))
                {
                    return true;
                }
            }

            return false;
        }
    }
    public class ThoughtWorker_Precept_Children_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn) =>
            ChildrenUtility.IsChildSupported(otherPawn) && AgeStages.IsYoungerThan(otherPawn, AgeStages.Teenager);
    }



    public class ThoughtWorker_Precept_SlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (lord.ownedPawns.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (car.PawnsListForReading.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && c.IsSlave))
                        return true;
                }
                else if (map.mapPawns.AllPawnsSpawned.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class ThoughtWorker_Precept_Slavery_NoSlavesInColony : ThoughtWorker_Precept
    {
        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            Lord lord = p.GetLord();
            if (lord != null)
                if (!lord.ownedPawns.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && c.IsSlave))
                    return true;

            Caravan car = p.GetCaravan();
            if (car != null)
            {
                if (!car.PawnsListForReading.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && c.IsSlave))
                    return true;
            }

            Map map = p.MapHeld;
            if (map != null)
            {
                Faction fac = p.Faction;
                if (fac != null)
                {
                    if (!map.mapPawns.SpawnedPawnsInFaction(fac).Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && c.IsSlave))
                        return true;
                }
                else if (!map.mapPawns.AllPawnsSpawned.Any(c => ChildrenUtility.IsChildSupported(c) && AgeStages.IsYoungerThan(c, AgeStages.Teenager) && !p.HostileTo(c) && c.IsSlave))
                {
                    return true;
                }
            }

            return false;
        }
    }
}