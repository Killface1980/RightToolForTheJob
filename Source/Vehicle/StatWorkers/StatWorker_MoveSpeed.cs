using System.Collections.Generic;
using System.Text;
#if CR
using Combat_Realism;
#endif
using RimWorld;
using ToolsForHaul.Components;
using ToolsForHaul.Utilities;
using UnityEngine;
using Verse;

namespace ToolsForHaul.StatWorkers
{
    internal class StatWorker_MoveSpeed : StatWorker
    {
        public override string GetExplanation(StatRequest req, ToStringNumberSense numberSense)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(base.GetExplanation(req, numberSense));
            if (req.HasThing)
            {
                Pawn thisPawn = req.Thing as Pawn;

                if (thisPawn != null)
                {
                    if (thisPawn.RaceProps.intelligence >= Intelligence.ToolUser)
                    {
                        CompSlotsBackpack compSlotsBackpack = ToolsForHaulUtility.TryGetBackpack(thisPawn).TryGetComp<CompSlotsBackpack>();
                        if (compSlotsBackpack != null)
                        {

                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine("CR_CarriedWeightBackpack".Translate() + ": x" + compSlotsBackpack.MoveSpeedFactor.ToStringPercent());
                            if (compSlotsBackpack.encumberPenalty > 0f)
                            {
                                stringBuilder.AppendLine("CR_EncumberedBackpack".Translate() + ": -" + compSlotsBackpack.encumberPenalty.ToStringPercent());
                                stringBuilder.AppendLine("CR_FinalModifierBackpack".Translate() + ": x" + this.GetStatFactor(thisPawn).ToStringPercent());
                            }
                        }
                    }
                }
            }

            return stringBuilder.ToString();
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {

            float num = base.GetValueUnfinalized(req, applyPostProcess);

            if (req.HasThing && req.Thing is Pawn)
            {
                Pawn pawn = req.Thing as Pawn;

                if (pawn.RaceProps.intelligence >= Intelligence.ToolUser)

                    if (this.GetStatFactor(pawn) > 1.01f)
                    {
                        num = this.GetStatFactor(pawn);
                    }
                    else
                    {
                        num *= this.GetStatFactor(pawn);
                    }
            }

            return num;

        }

        private float GetStatFactor(Pawn thisPawn)
        {
            float result = 1f;

            Apparel_Backpack apparelBackpack = ToolsForHaulUtility.TryGetBackpack(thisPawn);
            CompSlotsBackpack compSlotsBackpack = apparelBackpack?.SlotsComp;
            if (compSlotsBackpack != null)
            {
                result = Mathf.Clamp(compSlotsBackpack.MoveSpeedFactor - compSlotsBackpack.encumberPenalty, 0.5f, 1f);
            }

#if CR
            CompInventory compInventory = thisPawn.TryGetComp<CompInventory>();
            if (compInventory != null)
            {
                result = Mathf.Clamp(compInventory.moveSpeedFactor - compInventory.encumberPenalty, 0.1f, 1f);
                return result;
            }
#endif
            return result;
        }
    }
}
