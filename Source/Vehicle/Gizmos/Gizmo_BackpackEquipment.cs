﻿
using System.Collections.Generic;
using RimWorld;
using ToolsForHaul.Components;
using ToolsForHaul.Designators;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace ToolsForHaul.Gizmos
{
    [StaticConstructorOnStartup]
    public class Gizmo_BackpackEquipment : Gizmo
    {
        private static string txtNoItem;
        private static string txtThingInfo;
        private static string txtDropThing;

        public static readonly Texture2D texPutInArrow = ContentFinder<Texture2D>.Get("UI/Gizmoes/PutIn");
        private static readonly Texture2D texOccupiedSlotBG = SolidColorMaterials.NewSolidColorTexture(1f, 1f, 1f, 0.1f);


        // Links
        public Apparel_Backpack backpack;

        // Constants
        private static readonly Texture2D FilledTex = SolidColorMaterials.NewSolidColorTexture(1f, 1f, 1f, 0.10f);
        private static readonly Texture2D EmptyTex = SolidColorMaterials.NewSolidColorTexture(0.4f, 0.4f, 0.4f, 0.15f);
        private static readonly Texture2D NoAvailableTex = SolidColorMaterials.NewSolidColorTexture(0.0f, 0.0f, 0.0f, 1.0f);

        private static readonly Texture2D MeleeWeaponTex = SolidColorMaterials.NewSolidColorTexture(0.6f, 0.15f, 0.15f, 0.75f);
        private static readonly Texture2D RangedWeaponTex = SolidColorMaterials.NewSolidColorTexture(0.09f, 0.25f, 0.6f, 0.75f);

        // private static readonly Color HighlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        // private static readonly ThingCategoryDef weaponMelee = DefDatabase<ThingCategoryDef>.GetNamed("WeaponsMelee");
        // private static readonly ThingCategoryDef weaponRanged = DefDatabase<ThingCategoryDef>.GetNamed("WeaponsRanged");
        private static readonly ThingCategoryDef medicine = DefDatabase<ThingCategoryDef>.GetNamed("Medicine");
        private static readonly ThingCategoryDef foodMeals = DefDatabase<ThingCategoryDef>.GetNamed("FoodMeals");

        // Properties
        private const int textHeight = 30;
        private const int textWidth = 60;

        // EquipmentSlot Properties
        private const int numOfRow = 2;

        private int iconsPerRow
        {
            get
            {
                if (this.backpack.SlotsComp.slots.Count <= 4)
                    return 2;
                else
                {
                    int count = Mathf.FloorToInt(this.backpack.SlotsComp.slots.Count - 4) / 2;

                    return 2 + count;
                }

            }
        }

        private float curWidth
        {
            get
            {
                if (this.backpack.SlotsComp.slots.Count <= 4)
                    return Height;
                else
                {
                    int count = Mathf.FloorToInt(this.backpack.SlotsComp.slots.Count - 4) / 2;

                    return Height + count * Height * 0.5f;
                }

            }
        }

        public override float Width => this.curWidth;

        // IconClickSound
        private SoundDef thingIconSound;

        public Gizmo_BackpackEquipment()
        {
            txtNoItem = "NoItem".Translate();
            txtThingInfo = "ThingInfo".Translate();
            txtDropThing = "DropThing".Translate();
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.curWidth, Height);
            Widgets.DrawWindowBackground(overRect);

            // Equipment slot
            Pawn wearer = this.backpack.wearer;
            {
                // Slots CompSlots
                Rect inventoryRect = new Rect(topLeft.x, topLeft.y, this.Width, Height);

                Widgets.DrawWindowBackground(inventoryRect);
                this.DrawSlots(wearer, this.backpack.SlotsComp, inventoryRect);
            }

            return new GizmoResult(GizmoState.Clear);
        }

        public void DrawSlots(Pawn wearer, CompSlotsBackpack SlotsBackpackComp, Rect inventoryRect)
        {
            // draw text message if no contents inside
            if (SlotsBackpackComp.slots.Count == 0)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(inventoryRect, "No Items");
                Text.Anchor = TextAnchor.UpperLeft;
                Text.Font = GameFont.Tiny;
            }

            // draw slots
            else
            {
                Rect slotRect = new Rect(
                                    inventoryRect.x,
                                    inventoryRect.y,
                                    this.Width / this.iconsPerRow - 1f,
                                    Height / 2 - 1f);
                for (int currentSlotInd = 0; currentSlotInd < this.iconsPerRow * numOfRow; currentSlotInd++)
                {
                    if (currentSlotInd >= this.backpack.MaxItem)
                    {
                        slotRect.x = inventoryRect.x
                                     + inventoryRect.width / this.iconsPerRow * (currentSlotInd % this.iconsPerRow);
                        slotRect.y = inventoryRect.y + Height / 2 * (currentSlotInd / this.iconsPerRow);
                        Widgets.DrawTextureFitted(slotRect, NoAvailableTex, 1.0f);
                    }

                    if (currentSlotInd >= SlotsBackpackComp.slots.Count)
                    {
                        slotRect.x = inventoryRect.x
                                     + inventoryRect.width / this.iconsPerRow * (currentSlotInd % this.iconsPerRow);
                        slotRect.y = inventoryRect.y + Height / 2 * (currentSlotInd / this.iconsPerRow);
                        Widgets.DrawTextureFitted(slotRect, EmptyTex, 1.0f);
                    }

                    // draw occupied slots
                    if (currentSlotInd < SlotsBackpackComp.slots.Count)
                    {
                        slotRect.x = inventoryRect.x
                                     + inventoryRect.width / this.iconsPerRow * (currentSlotInd % this.iconsPerRow);
                        slotRect.y = inventoryRect.y + Height / 2 * (currentSlotInd / this.iconsPerRow);

                        Thing currentThing = SlotsBackpackComp.slots[currentSlotInd];

                        // draws greyish slot background
                        Widgets.DrawTextureFitted(slotRect.ContractedBy(3f), texOccupiedSlotBG, 1f);

                        if (currentThing.def.IsMeleeWeapon)
                        {
                            Widgets.DrawTextureFitted(slotRect, MeleeWeaponTex, 1.0f);
                        }

                        if (currentThing.def.IsRangedWeapon)
                        {
                            Widgets.DrawTextureFitted(slotRect, RangedWeaponTex, 1.0f);
                        }

                        Widgets.DrawBox(slotRect);

                        // highlights slot if mouse over
                        Widgets.DrawHighlightIfMouseover(slotRect.ContractedBy(3f));

                        // tooltip if mouse over
                        TooltipHandler.TipRegion(slotRect, currentThing.def.LabelCap);

                        // draw thing texture
                        Widgets.ThingIcon(slotRect, currentThing);

                        // interaction with slots
                        if (Widgets.ButtonInvisible(slotRect))
                        {
                            // mouse button pressed
                            if (Event.current.button == 0)
                            {
                                // Weapon
                                if (currentThing != null && currentThing.def.equipmentType == EquipmentType.Primary
                                    && (currentThing.def.IsRangedWeapon || currentThing.def.IsMeleeWeapon))
                                {
                                    SlotsBackpackComp.SwapEquipment(currentThing as ThingWithComps);
                                }

                                //// equip weapon in slot
                                // if (currentThing.def.equipmentType == EquipmentType.Primary)
                                // {
                                // slotsComp.SwapEquipment(currentThing as ThingWithComps);
                                // }
                            }

                            // mouse button released
                            else if (Event.current.button == 1)
                            {
                                List<FloatMenuOption> options = new List<FloatMenuOption>
                                                                    {
                                                                        new FloatMenuOption(
                                                                            "Info",
                                                                            () =>
                                                                                {
                                                                                    Find.WindowStack.Add(
                                                                                        new Dialog_InfoCard(currentThing));
                                                                                }),
                                                                        new FloatMenuOption(
                                                                            "Drop",
                                                                            () =>
                                                                                {
                                                                                    Thing resultThing;
                                                                                    SlotsBackpackComp.slots.TryDrop(
                                                                                        currentThing,
                                                                                        wearer.Position,
                                                    wearer.Map,
                                                                                        ThingPlaceMode.Near,
                                                                                        out resultThing);
                                                                                })
                                                                    };

                                // get thing info card
                                // drop thing

                                // Else

                                // Weapon
                                if (currentThing != null && currentThing.def.equipmentType == EquipmentType.Primary)
                                    options.Add(
                                        new FloatMenuOption(
                                            "Equip".Translate(currentThing.LabelCap),
                                            () => { SlotsBackpackComp.SwapEquipment(currentThing as ThingWithComps); }));



                                // Food
                                if (
                                    currentThing.def.thingCategories.Contains(
                                        DefDatabase<ThingCategoryDef>.GetNamed("FoodMeals")))
                                {
                                    // if (compSlots.owner.needs.food.CurCategory != HungerCategory.Fed)
                                    options.Add(
                                        new FloatMenuOption(
                                            "ConsumeThing".Translate(currentThing.LabelCap),
                                            () =>
                                                {
                                                    Thing dummy;
                                                    SlotsBackpackComp.slots.TryDrop(
                                                        currentThing,
                                                        wearer.Position,
                                                    wearer.Map,
                                                        ThingPlaceMode.Direct,
                                                        currentThing.def.ingestible.maxNumToIngestAtOnce,
                                                        out dummy);

                                                    Job jobNew = new Job(JobDefOf.Ingest, dummy);
                                                    jobNew.count =
                                                        currentThing.def.ingestible.maxNumToIngestAtOnce;
                                                    jobNew.ignoreForbidden = true;
                                                    wearer.jobs.TryTakeOrderedJob(jobNew);
                                                }));
                                }

                                // Drugs
                                if (currentThing.def.ingestible?.drugCategory >= DrugCategory.Any)
                                    options.Add(
                                        new FloatMenuOption(
                                            "ConsumeThing".Translate(currentThing.LabelCap),
                                            () =>
                                                {
                                                    Thing dummy;
                                                    SlotsBackpackComp.slots.TryDrop(
                                                        currentThing,
                                                        wearer.Position,
                                                    wearer.Map,
                                                        ThingPlaceMode.Direct,
                                                        currentThing.def.ingestible.maxNumToIngestAtOnce,
                                                        out dummy);

                                                    Job jobNew =
                                                        new Job(JobDefOf.Ingest, dummy)
                                                        {
                                                            count = dummy.def.ingestible
                                                                    .maxNumToIngestAtOnce,
                                                            ignoreForbidden = true
                                                        };
                                                    wearer.jobs.TryTakeOrderedJob(jobNew);
                                                }));

                                Find.WindowStack.Add(new FloatMenu(options, currentThing.LabelCap));
                            }

                            // plays click sound on each click
                            SoundDefOf.Click.PlayOneShotOnCamera();
                        }
                    }

                    slotRect.x = inventoryRect.x + Height / 2 * (currentSlotInd % this.iconsPerRow);
                    slotRect.y = inventoryRect.y + Height / 2 * (currentSlotInd / this.iconsPerRow);

                    // slotRect.x += Height;
                }
            }
        }



    }

}