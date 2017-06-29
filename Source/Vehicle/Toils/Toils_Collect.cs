using System.Collections.Generic;
using System.Linq;
using RimWorld;
using ToolsForHaul.Utilities;
using Verse;
using Verse.AI;

namespace ToolsForHaul.Toils
{
    public static class Toils_Collect
    {
        private const int NearbyCell = 30;

        public static Toil Extract(TargetIndex ind)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    List<LocalTargetInfo> targetQueue = toil.actor.jobs.curJob.GetTargetQueue(ind);
                    if (!targetQueue.NullOrEmpty())
                    {
                        toil.actor.jobs.curJob.SetTarget(ind, targetQueue.First());
                        targetQueue.RemoveAt(0);
                    }
                };
            return toil;
        }

        #region Toil Collect

        public static Toil CheckDuplicates(Toil jumpToil, TargetIndex CarrierInd, TargetIndex HaulableInd)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    IntVec3 storeCell = IntVec3.Invalid;
                    Pawn actor = toil.GetActor();

                    LocalTargetInfo target = toil.actor.jobs.curJob.GetTarget(HaulableInd);
                    if (target.Thing.def.stackLimit <= 1) return;
                    List<LocalTargetInfo> targetQueue = toil.actor.jobs.curJob.GetTargetQueue(HaulableInd);
                    if (!targetQueue.NullOrEmpty() && target.Thing.def.defName == targetQueue.First().Thing.def.defName)
                    {
                        toil.actor.jobs.curJob.SetTarget(HaulableInd, targetQueue.First());
                        actor.Map.reservationManager.Reserve(actor, targetQueue.First());
                        targetQueue.RemoveAt(0);
                        toil.actor.jobs.curDriver.JumpToToil(jumpToil);
                        return;
                    }

                    Apparel_Backpack backpack = toil.actor.jobs.curJob.GetTarget(CarrierInd).Thing as Apparel_Backpack;

                    if (backpack == null)
                    {
                        Log.Error(actor.LabelCap + " Report: Don't have Carrier");
                        toil.actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
                        return;
                    }

                    int curItemCount = (backpack.SlotsComp.slots.Count)
                                       + targetQueue.Count;
                    int curItemStack = (backpack.SlotsComp.slots.TotalStackCount)
                                       + targetQueue.Sum(item => item.Thing.stackCount);
                    int maxItem =  backpack.MaxItem;
                    int maxStack = backpack.MaxStack;
                    if (curItemCount >= maxItem || curItemStack >= maxStack) return;

                    // Check target's nearby
                    Thing thing = GenClosest.ClosestThing_Global_Reachable(
                        actor.Position,
                        actor.Map,
                        actor.Map.listerHaulables.ThingsPotentiallyNeedingHauling(),
                        PathEndMode.Touch,
                        TraverseParms.For(actor, Danger.Some),
                        NearbyCell,
                        item =>
                            !targetQueue.Contains(item) && item.def.defName == target.Thing.def.defName
                            && !item.IsBurning() && actor.Map.reservationManager.CanReserve(actor, item));
                    if (thing != null)
                    {
                        toil.actor.jobs.curJob.SetTarget(HaulableInd, thing);
                        actor.Map.reservationManager.Reserve(actor, thing);
                        toil.actor.jobs.curDriver.JumpToToil(jumpToil);
                    }
                };
            return toil;
        }

        // OLD
        /* public static Toil CollectInInventory(TargetIndex HaulableInd)
         {
 
             Toil toil = new Toil();
             toil.initAction = () =>
             {
                 Pawn actor = toil.actor;
                 Job curJob = actor.jobs.curJob;
                 Thing haulThing = curJob.GetTarget(HaulableInd).Thing;
 
                 //Check haulThing is human_corpse. If other race has apparel, It need to change
                 if (haulThing.ThingID.IndexOf("Human_Corpse") <= -1 ? false : true)
                 {
                     Corpse corpse = (Corpse)haulThing;
                     List<Apparel> wornApparel = corpse.innerPawn.apparel.WornApparel;
 
                     //Drop wornApparel. wornApparel cannot Add to container directly because it will be duplicated.
                     corpse.innerPawn.apparel.DropAll(corpse.innerPawn.Position, false);
 
                     //Transfer in container
                     foreach (Thing apparel in wornApparel)
                     {
                         if (actor.inventory.innerContainer.TryAdd(apparel))
                         {
                             apparel.holdingContainer = actor.inventory.GetContainer();
                             apparel.holdingContainer.owner = actor.inventory;
                         }
                     }
                 }
                 //Collecting TargetIndex ind
                 if (actor.inventory.innerContainer.TryAdd(haulThing))
                 {
                     haulThing.holdingContainer = actor.inventory.GetContainer();
                     haulThing.holdingContainer.owner = actor.inventory;
                 }
 
             };
             toil.FailOn(() =>
             {
                 Pawn actor = toil.actor;
                 Job curJob = actor.jobs.curJob;
                 Thing haulThing = curJob.GetTarget(HaulableInd).Thing;
 
                 if (!actor.inventory.innerContainer.CanAcceptAnyOf(haulThing))
                     return true;
 
 
 
                 return false;
             });
             return toil;
         }
         */
        public static Toil CollectInBackpack(TargetIndex HaulableInd, Apparel_Backpack backpack)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    Pawn actor = toil.actor;
                    Job curJob = actor.jobs.curJob;
                    Thing haulThing = curJob.GetTarget(HaulableInd).Thing;

                    // Collecting TargetIndex ind
                    if (backpack.SlotsComp.slots.TryAdd(haulThing))
                    {
                        haulThing.holdingContainer = backpack.SlotsComp.GetInnerContainer();
                        haulThing.holdingContainer.owner = backpack.SlotsComp;
                    }
                };
            toil.FailOn(
                () =>
                    {
                        Pawn actor = toil.actor;
                        Job curJob = actor.jobs.curJob;
                        Thing haulThing = curJob.GetTarget(HaulableInd).Thing;

                        if (!backpack.SlotsComp.slots.CanAcceptAnyOf(haulThing)) return true;

                        return false;
                    });
            return toil;
        }


        #endregion

        #region Toil Drop

        public static Toil CheckNeedStorageCell(Toil jumpToil, TargetIndex CarrierInd, TargetIndex StoreCellInd)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    Pawn actor = toil.actor;

                    Apparel_Backpack backpack = toil.actor.jobs.curJob.GetTarget(CarrierInd).Thing as Apparel_Backpack;
                    if (backpack == null)
                    {
                        Log.Error(actor.LabelCap + " Report: Don't have Carrier");
                        toil.actor.jobs.curDriver.EndJobWith(JobCondition.Errored);
                    }

                    ThingContainer container = backpack.SlotsComp.slots;
                    if (container.Count == 0) return;

                    IntVec3 cell = ToolsForHaulUtility.FindStorageCell(actor, container.First(), actor.Map);
                    if (cell != IntVec3.Invalid)
                    {
                        toil.actor.jobs.curJob.SetTarget(StoreCellInd, cell);
                        actor.Map.reservationManager.Reserve(actor, cell);
                        toil.actor.jobs.curDriver.JumpToToil(jumpToil);
                    }
                };
            return toil;
        }

        public static Toil DropTheCarriedFromBackpackInCell(
            TargetIndex StoreCellInd,
            ThingPlaceMode placeMode,
            Apparel_Backpack backpack)
        {
            Toil toil = new Toil();
            toil.initAction = () =>
                {
                    Pawn actor = toil.actor;
                    Job curJob = actor.jobs.curJob;
                    if (backpack.SlotsComp.slots.Count <= 0) return;

                    // Check dropThing is last item that should not be dropped
                    Thing dropThing = null;

                    dropThing = backpack.SlotsComp.slots.First();

                    if (dropThing == null)
                    {
                        Log.Error(
                            toil.actor + " tried to drop null thing in "
                            + actor.jobs.curJob.GetTarget(StoreCellInd).Cell);
                        return;
                    }

                    IntVec3 destLoc = actor.jobs.curJob.GetTarget(StoreCellInd).Cell;
                    Thing dummy;

                    if (destLoc.GetStorable(actor.Map) == null)
                    {
                        actor.Map.designationManager.RemoveAllDesignationsOn(dropThing);
                        backpack.SlotsComp.slots.TryDrop(dropThing, destLoc, actor.Map, placeMode, out dummy);
                    }
                };
            return toil;
        }

        #endregion
    }
}

