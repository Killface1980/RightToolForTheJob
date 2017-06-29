using System.Linq;
using RimWorld;
using ToolsForHaul.Utilities;
using UnityEngine;
using Verse;
using static ToolsForHaul.GameComponent_ToolsForHaul;

namespace ToolsForHaul
{
    [StaticConstructorOnStartup]
    public static class RightTools
    {
        static RightTools()
        {
            PreLoad();
        }

        private static void PreLoad()
        {
        }

        public static float GetMaxStat(ThingWithComps thing, StatDef def)
        {
            float result;
            if (thing == null || thing.def.equippedStatOffsets == null)
            {
                result = 0f;
            }
            else
            {
                foreach (StatModifier current in thing.def.equippedStatOffsets)
                {
                    if (current.stat == def)
                    {
                        result = current.value;
                        return result;
                    }
                }

                result = 0f;
            }

            return result;
        }

        /// <summary>
        /// Equips tools
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="def"></param>
        public static void EquipRigthTool(Pawn pawn, StatDef def)
        {
            if (pawn.story != null && pawn.story.WorkTagIsDisabled(WorkTags.Violent))
                return;

            Apparel_Toolbelt toolbelt = ToolsForHaulUtility.TryGetToolbelt(pawn);

            if (toolbelt != null)
            {
                ThingWithComps thingWithComps = pawn.equipment.Primary;
                float currentStat = GetMaxStat(thingWithComps, def);

                foreach (Thing slot in toolbelt.slotsComp.slots)
                {
                    ThingWithComps thingWithComps2 = slot as ThingWithComps;
                    if (thingWithComps2 != null)
                    {
                        if (thingWithComps2.def.IsRangedWeapon || thingWithComps2.def.IsMeleeWeapon)
                        {
                            float candidateStat = GetMaxStat(thingWithComps2, def);
                            if (candidateStat > currentStat)
                            {
                                currentStat = candidateStat;
                                thingWithComps = thingWithComps2;
                            }
                        }
                    }
                }

                bool unEquipped = thingWithComps != pawn.equipment.Primary;
                if (unEquipped)
                {
                    if (!PreviousPawnWeapon.ContainsKey(pawn))
                    {
                        PreviousPawnWeapon.Add(pawn, pawn.equipment.Primary);
                    }
                    else
                    {
                        PreviousPawnWeapon[pawn] = pawn.equipment.Primary;
                    }

                    toolbelt.slotsComp.SwapEquipment(thingWithComps);

                    // pawn.equipment.TryTransferEquipmentToContainer(pawn.equipment.Primary, pawn.inventory.innerContainer, out dummy);
                    // pawn.equipment.AddEquipment(thingWithComps);
                    // pawn.inventory.innerContainer.Remove(thingWithComps);
                }

                // else
                // {
                // bool flag5 = stat == 0f && def != StatDefOf.WorkSpeedGlobal;
                // if (flag5)
                // {
                // EquipRigthTool(pawn, StatDefOf.WorkSpeedGlobal);
                // }
                // }
            }
        }
    }
}
