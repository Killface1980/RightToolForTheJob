﻿//#define DEBUG
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using ToolsForHaul.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ToolsForHaul.WorkGivers
{
    public class WorkGiver_HaulWithBackpack : WorkGiver_Scanner
    {
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            List<Thing> list = new List<Thing>();
            Apparel_Backpack backpack = ToolsForHaulUtility.TryGetBackpack(pawn);
            foreach (Thing thing in pawn.Map.listerHaulables.ThingsPotentiallyNeedingHauling())
            {
                if (
                    thing.def.thingCategories.Exists(
                        category =>
                            backpack.SlotsComp.Properties.allowedThingCategoryDefs.Exists(
                                subCategory => subCategory.ThisAndChildCategoryDefs.Contains(category))
                            && !backpack.SlotsComp.Properties.forbiddenSubThingCategoryDefs.Exists(
                                subCategory => subCategory.ThisAndChildCategoryDefs.Contains(category)))) list.Add(thing);

                // return ToolsForHaulUtility.Cart();
            }

            return list;
        }

        public override bool ShouldSkip(Pawn pawn)
        {
            Trace.DebugWriteHaulingPawn(pawn);

            Apparel_Backpack backpack = ToolsForHaulUtility.TryGetBackpack(pawn);

            // Should skip pawn that don't have backpack.
            if (backpack == null)
            {
                return true;
            }
            if (backpack.MaxItem - backpack.SlotsComp.slots.Count == 0)
            {
                return true;
            }

            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced)
        {
            if (t is Corpse)
            {
                return null;
            }

            if (!HaulAIUtility.PawnCanAutomaticallyHaul(pawn, t, forced))
            {
                return null;
            }

            Apparel_Backpack backpack = ToolsForHaulUtility.TryGetBackpack(pawn);
            if (backpack != null)
            {
                if (
                    !t.def.thingCategories.Exists(
                        category =>
                            backpack.SlotsComp.Properties.allowedThingCategoryDefs.Exists(
                                subCategory => subCategory.ThisAndChildCategoryDefs.Contains(category)) &&
                            !backpack.SlotsComp.Properties.forbiddenSubThingCategoryDefs.Exists(
                                subCategory => subCategory.ThisAndChildCategoryDefs.Contains(category))))
                {
                    JobFailReason.Is("Backpack can't hold that thing");
                    return null;
                }
                else
                {

                    return ToolsForHaulUtility.HaulWithTools(pawn, pawn.Map);
                }
            }

            JobFailReason.Is("NoBackpack".Translate());
            return null;
        }
    }
}