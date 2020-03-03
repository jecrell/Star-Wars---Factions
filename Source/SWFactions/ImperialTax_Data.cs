using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse.Sound;
using Verse.AI.Group;

namespace SWFactions
{
    public class ImperialTax_Data : WorldComponent
    {
        public bool metEmpire = false;
        public bool receivedMessage = false;
        public bool taxedColony = false;
        public int nextTaxCollectionTicks = -1;
        public int empireFunds = 0;
        public int empireDebt = 0;
        

        public static readonly float empireTaxRate = 0.2f;
        public static readonly int empireLandRightsTax = 100;

        public ImperialTax_Data(World world) : base(world)
        {

        }

        #region Meeting

        public void ResolveMeetingGalacticEmpire(Pawn imperial)
        {
            if (!metEmpire)
            {

                metEmpire = true;

                Find.MusicManagerPlay.disabled = true;
                Find.MusicManagerPlay.ForceSilenceFor(10f);
                Find.MusicManagerPlay.disabled = false;
                SoundDef.Named("PJ_ImperialMarchBanjo").PlayOneShotOnCamera();
            }
            else if (metEmpire && !receivedMessage)
            {
                receivedMessage = true;
                SendFirstMeetingDialog(imperial);
            }
        }

        public void SendFirstMeetingDialog(Pawn imperial)
        {
            Settlement playerSettlement = imperial.Map.info.parent as Settlement;

            string text = "PJ_ImperialGreeting".Translate(imperial.Name.ToStringFull, imperial.kindDef.label,
                playerSettlement.Label,
                empireLandRightsTax.ToString(),
                empireTaxRate.ToStringPercent());

            DiaNode diaNode = new DiaNode(text);
            DiaOption diaOption = new DiaOption("PJ_ImperialGreeting_Accept".Translate());
            diaOption.action = delegate
            {
                taxedColony = true;
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(diaOption);
            
            string text2 = "PJ_ImperialGreeting_Rejected".Translate(
            
                imperial.LabelShort
            );
            DiaNode diaNode2 = new DiaNode(text2);
            DiaOption diaOption2 = new DiaOption("OK".Translate());
            diaOption2.resolveTree = true;
            diaNode2.options.Add(diaOption2);
            DiaOption diaOption3 = new DiaOption("PJ_ImperialGreeting_Reject".Translate());
            diaOption3.action = delegate
            {
                ResolveDeclarationOfHostility(imperial);
            };
            diaOption3.link = diaNode2;
            diaNode.options.Add(diaOption3);
            string title = "PJ_ImperialGreeting_Title".Translate();
            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, title));
        }

        #endregion Meeting
        public void ResolveDeclarationOfHostility(Pawn imperial)
        {
            imperial.Faction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, canSendLetter: true);
            //List<Pawn> imperialsOnSite = imperial.Map.mapPawns.AllPawnsSpawned.FindAll((x) => x.Faction == imperial.Faction);
            //if (imperialsOnSite != null && imperialsOnSite.Count > 0)
            //    {
            //    if (imperial?.GetLord() is Lord imperialLord) imperial.Map.lordManager.RemoveLord(imperialLord);
            //        LordMaker.MakeNewLord(imperial.Faction, new LordJob_AssaultColony(imperial.Faction, false, false, false, false, false), imperial.Map, imperialsOnSite);
            //    }
        }


        public void ResolveGalacticEmpireTaxDeal(Pawn imperial)
        {
            Map map = imperial.MapHeld;
            if (taxedColony)
            {
                if (nextTaxCollectionTicks == -1)
                {
                    //nextTaxCollectionTicks += 2000; //GenDate.DaysPerSeason * GenDate.TicksPerYear;
                    nextTaxCollectionTicks += GenDate.DaysPerSeason * GenDate.TicksPerYear;
                    ReceiveTaxes(imperial, empireLandRightsTax);
                }
                else if (Find.TickManager.TicksGame > nextTaxCollectionTicks)
                {
                    int totalCurrency = DetermineSilverAvailable(imperial);
                    int resolvedTax = Math.Max(empireLandRightsTax, (int)(totalCurrency * 0.2));
                    ReceiveTaxes(imperial, resolvedTax);
                }
            }
        }

        public int DetermineSilverAvailable(Pawn imperial)
        {
            int result = 0;
            List<Thing> currencies = imperial.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
            if (currencies != null && currencies.Count > 0)
            {
                foreach (Thing currency in currencies)
                {
                    result += currency.stackCount;
                }
            }
            return result;
        }

        public void ReceiveTaxes(Pawn imperial, int amountOwed)
        {
            int amountUnpaid = amountOwed + empireDebt;
            List<Thing> currencies = imperial.Map.listerThings.ThingsOfDef(ThingDefOf.Silver);
            if (currencies != null && currencies.Count > 0)
            {
                foreach (Thing currency in currencies.InRandomOrder<Thing>())
                {
                    if (amountUnpaid <= 0) { break; }
                    int num = Math.Min(amountUnpaid, currency.stackCount);
                    currency.SplitOff(num).Destroy(DestroyMode.Vanish);
                    amountUnpaid -= num;
                    empireFunds += num;
                }
            }

            if (amountUnpaid > 0)
            {
                empireDebt = amountUnpaid;
                int amountPaid = amountOwed - amountUnpaid;
                if (amountPaid < 0) amountPaid = 0;
                Messages.Message("PJ_ImperialTaxes_Owed".Translate(
                
                                imperial.Map.info.parent.Label,
                                amountPaid,
                                empireDebt
                ), MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message("PJ_ImperialTaxes_Paid".Translate(
                                imperial.Map.info.parent.Label,
                                amountOwed
                    ), MessageTypeDefOf.PositiveEvent);
            }
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            try
            {
                if (Find.TickManager != null)
                {
                    if (Find.TickManager.TicksGame % 1000 == 0)
                    {
                        if (Find.Maps != null)
                        {
                            Find.Maps.ForEach(delegate (Map map)
                            {
                                List<Pawn> pawns = map.mapPawns.AllPawnsSpawned.Where((Pawn p) => p.Faction != null).ToList();
                                Pawn imperial = pawns.FirstOrDefault((Pawn p) => p.Name != null && p.Faction.def.defName.EqualsIgnoreCase("PJ_GalacticEmpire"));
                                if (imperial != null)
                                {
                                    ResolveMeetingGalacticEmpire(imperial);
                                    ResolveGalacticEmpireTaxDeal(imperial);
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception) { }

        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.metEmpire, "metEmpire", false);
            Scribe_Values.Look<bool>(ref this.receivedMessage, "receivedEmpire", false);
            Scribe_Values.Look<bool>(ref this.taxedColony, "taxedColony", false);
            Scribe_Values.Look<int>(ref this.empireFunds, "empireFunds", 0);
            Scribe_Values.Look<int>(ref this.empireDebt, "empireDebt", 0);
            Scribe_Values.Look<int>(ref this.nextTaxCollectionTicks, "lastTaxCollectionTicks", -1);
        }
    }
}
