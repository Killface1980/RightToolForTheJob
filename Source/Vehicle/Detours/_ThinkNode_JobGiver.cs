﻿using System;
using System.Linq;
using System.Reflection;
using RimWorld;
using ToolsForHaul;
using ToolsForHaul.JobDefs;
using ToolsForHaul.Utilities;
using UnityEngine;
using Verse;
using Verse.AI;
using static ToolsForHaul.GameComponent_ToolsForHaul;

namespace Verse.AI
{
    public abstract class _ThinkNode_JobGiver : ThinkNode
    {
        protected abstract Job TryGiveJob(Pawn pawn);

        //  [Detour(typeof(ThinkNode_JobGiver), bindingFlags = BindingFlags.Instance | BindingFlags.Public)]
        [DetourMethod(typeof(ThinkNode_JobGiver), "TryIssueJobPackage")]
        public override ThinkResult TryIssueJobPackage(Pawn pawn)
        {
            Job job = this.TryGiveJob(pawn);
            bool jobNull = job == null;
            ThinkResult result;

            Apparel_Toolbelt toolbelt = ToolsForHaulUtility.TryGetToolbelt(pawn);
            if (toolbelt != null)
            {
                if (PreviousPawnWeapon.ContainsKey(pawn) && PreviousPawnWeapon[pawn] != null)
                {
                    Pawn wearer = toolbelt.Wearer;
                    ThingWithComps previousWeapon = PreviousPawnWeapon[pawn];
                    if (previousWeapon != null && toolbelt.slotsComp.slots.Contains(previousWeapon))
                    {
                        for (int i = toolbelt.slotsComp.slots.Count-1; i >=0 ; i--)
                        {
                            var thing = toolbelt.slotsComp.slots[i];
                            ThingWithComps item = (ThingWithComps)thing;
                            if (item == previousWeapon)
                            {
                                if (wearer.equipment.Primary != null)
                                {
                                    toolbelt.slotsComp.SwapEquipment(item);
                                }
                                else
                                {
                                    wearer.equipment.AddEquipment(item);
                                    toolbelt.slotsComp.slots.Remove(item);
                                }
                                break;
                            }
                        }
                    }
                }
                PreviousPawnWeapon[pawn] = null;
            }

            if (jobNull)
            {
                result = ThinkResult.NoJob;
            }
            else
            {
                if (pawn.Faction == Faction.OfPlayer && pawn.RaceProps.Humanlike && pawn.RaceProps.IsFlesh)
                {
                    if (job.def == JobDefOf.DoBill)
                    {
                        RightTools.EquipRigthTool(pawn, job.RecipeDef.workSpeedStat);
                    }

                    if (job.def == JobDefOf.FinishFrame || job.def == JobDefOf.Deconstruct || job.def == JobDefOf.Repair || job.def == JobDefOf.BuildRoof || job.def == JobDefOf.RemoveRoof || job.def == JobDefOf.RemoveFloor)
                    {
                        RightTools.EquipRigthTool(pawn, StatDefOf.ConstructionSpeed);
                    }
                    if (job.def == JobDefOf.SmoothFloor)
                    {
                        RightTools.EquipRigthTool(pawn, StatDefOf.SmoothingSpeed);
                    }

                    if (job.def == JobDefOf.Harvest)
                    {
                        RightTools.EquipRigthTool(pawn, StatDefOf.PlantHarvestYield);
                    }
                    if (job.def == JobDefOf.CutPlant || job.def == JobDefOf.Sow)
                    {
                        RightTools.EquipRigthTool(pawn, StatDefOf.PlantWorkSpeed);
                    }

                    if (job.def == JobDefOf.Mine)
                    {
                        RightTools.EquipRigthTool(pawn, StatDefOf.MiningSpeed);
                    }




                }

                result = new ThinkResult(job, this);
            }

            return result;
        }
    }
}
