using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;
using System.Reflection;
using UnityEngine;
using CompSlotLoadable;

namespace SWSaber
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("rimworld.jecrell.starwars.lightsaber");
            harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "AddEquipment"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("AddEquipment_PostFix")), null);
            harmony.Patch(AccessTools.Method(typeof(PawnInventoryGenerator), "GenerateInventoryFor"), null, new HarmonyMethod(typeof(HarmonyPatches).GetMethod("GenerateInventoryFor_PostFix")));
            //harmony.Patch(AccessTools.Method(typeof(Pawn_EquipmentTracker), "Remove"), new HarmonyMethod(typeof(HarmonyPatches).GetMethod("Remove_PostFix")), null);
        }

        public static Thing GenerateCrystal(ThingDef crystalDef, float chance = 1.0f)
        {
            if (Rand.Value <= chance)
            {
                if (crystalDef != null)
                {
                    Thing thing = ThingMaker.MakeThing(crystalDef, null);
                    thing.stackCount = 1;
                    return thing;
                }
            }
            return null;
        }

        // RimWorld.PawnInventoryGenerator
        public static void GenerateInventoryFor_PostFix(Pawn p, PawnGenerationRequest request)
        {
            //Log.Message("1");
            if (!Utility.AreFactionsLoaded()) return;
            //Log.Message("2");

            if (p.kindDef == null) return;
            //Log.Message("3");

            if (p.kindDef.defName == "PJ_ImpCommander" ||
                p.kindDef.defName == "PJ_RebCouncilman" ||
                p.kindDef.defName == "PJ_ScumBoss")
            {
                //Log.Message("4");

                List<ThingDef> legendaryCrystals = new List<ThingDef>()
                {
                    ThingDef.Named("PJ_UltimaPearl"),
                    ThingDef.Named("PJ_BlackPearl"),
                    ThingDef.Named("PJ_KaiburrCrystal"),
                    ThingDef.Named("PJ_UltimaPearl"),
                    ThingDef.Named("PJ_AnkSapphire")
                };
                List<ThingDef> rareCrystals = new List<ThingDef>()
                {
                    ThingDef.Named("PJ_BarabIngot"),
                    ThingDef.Named("PJ_PontiteCrystal"),
                    ThingDef.Named("PJ_FirkrannCrystal"),
                    ThingDef.Named("PJ_RubatCrystal"),
                    ThingDef.Named("PJ_HurCrystal"),
                    ThingDef.Named("PJ_DragiteCrystal"),
                    ThingDef.Named("PJ_DamindCrystal"),
                    ThingDef.Named("PJ_AdeganCrystal"),
                    ThingDef.Named("PJ_EralamCrystal"),
                    ThingDef.Named("PJ_PontiteCrystal")
                };
                Thing result = GenerateCrystal(legendaryCrystals.RandomElement<ThingDef>(), 0.7f);
                if (result != null)
                {
                    //Log.Message("5a");

                    p.inventory.innerContainer.TryAdd(result, true);
                    return;
                }
                else
                {
                    //Log.Message("5b");

                    result = GenerateCrystal(rareCrystals.RandomElement<ThingDef>());
                    p.inventory.innerContainer.TryAdd(result, true);
                    return;
                }
            }
        }

            //public static void Remove_PostFix(Pawn_EquipmentTracker __instance, ThingWithComps eq)
            //{
            //    CompLightsaberActivatableEffect lightsaberEffect = eq.TryGetComp<CompLightsaberActivatableEffect>();
            //    if (lightsaberEffect != null)
            //    {

            //    }
            //}

            public static void AddEquipment_PostFix(Pawn_EquipmentTracker __instance, ThingWithComps newEq)
        {
            Pawn pawn = (Pawn)AccessTools.Field(typeof(Pawn_EquipmentTracker), "pawn").GetValue(__instance);

            CompLightsaberActivatableEffect lightsaberEffect = newEq.TryGetComp<CompLightsaberActivatableEffect>();
            if (lightsaberEffect != null)
            {
                if (pawn != null)
                {
                    if (pawn.Faction != Faction.OfPlayer)
                    {
                        Log.Message("1");
                        CompCrystalSlotLoadable crystalSlot = newEq.GetComp<CompCrystalSlotLoadable>();
                        if (crystalSlot != null)
                        {
                            crystalSlot.Initialize();
                            Log.Message("2");
                            List<string> randomCrystals = new List<string>()
                            {
                                "PJ_KyberCrystal",
                                "PJ_KyberCrystalBlue",
                                "PJ_KyberCrystalCyan",
                                "PJ_KyberCrystalAzure",
                                "PJ_KyberCrystalRed",
                                "PJ_KyberCrystalPurple",
                            };
                            ThingWithComps thingWithComps = (ThingWithComps)ThingMaker.MakeThing(ThingDef.Named(randomCrystals.RandomElement<string>()), null);
                            Log.Message(thingWithComps.Label);
                            foreach (SlotLoadable slot in crystalSlot.Slots)
                            {
                                Log.Message("3");
                                slot.TryLoadSlot(thingWithComps);
                            }
                            lightsaberEffect.Activate();
                        }
                    }
                }
            }
        }
    }
}
