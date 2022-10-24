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

        public ImperialTax_Data(World world) : base(world: world)
        {

        }

        #region Meeting

        public void ResolveMeetingGalacticEmpire(Pawn imperial)
        {
            if (!metEmpire)
            {

                metEmpire = true;

                Find.MusicManagerPlay.disabled = true;
                Find.MusicManagerPlay.ForceSilenceFor(time: 10f);
                Find.MusicManagerPlay.disabled = false;
                SoundDef.Named(defName: "PJ_ImperialMarchBanjo").PlayOneShotOnCamera();
            }
            else if (metEmpire && !receivedMessage)
            {
                receivedMessage = true;
                SendFirstMeetingDialog(imperial: imperial);
            }
        }

        public void SendFirstMeetingDialog(Pawn imperial)
        {
            Settlement playerSettlement = imperial.Map.info.parent as Settlement;

            string text = "PJ_ImperialGreeting".Translate(args: new object[]
            {
                imperial.Name.ToStringFull,
                imperial.kindDef.label,
                playerSettlement.Label,
                empireLandRightsTax.ToString(),
                empireTaxRate.ToStringPercent()
            });
            DiaNode diaNode = new DiaNode(text: text);
            DiaOption diaOption = new DiaOption(text: "PJ_ImperialGreeting_Accept".Translate());
            diaOption.action = delegate
            {
                taxedColony = true;
            };
            diaOption.resolveTree = true;
            diaNode.options.Add(item: diaOption);
            
            string text2 = "PJ_ImperialGreeting_Rejected".Translate(args: new object[]
            {
                imperial.LabelShort
            });
            DiaNode diaNode2 = new DiaNode(text: text2);
            DiaOption diaOption2 = new DiaOption(text: "OK".Translate());
            diaOption2.resolveTree = true;
            diaNode2.options.Add(item: diaOption2);
            DiaOption diaOption3 = new DiaOption(text: "PJ_ImperialGreeting_Reject".Translate());
            diaOption3.action = delegate
            {
                ResolveDeclarationOfHostility(imperial: imperial);
            };
            diaOption3.link = diaNode2;
            diaNode.options.Add(item: diaOption3);
            string title = "PJ_ImperialGreeting_Title".Translate();
            Find.WindowStack.Add(window: new Dialog_NodeTree(nodeRoot: diaNode, delayInteractivity: true, radioMode: true, title: title));
        }

        #endregion Meeting
        public void ResolveDeclarationOfHostility(Pawn imperial)
        {
            imperial.Faction.SetRelationDirect(other: Faction.OfPlayer, kind: FactionRelationKind.Hostile, canSendHostilityLetter: true);
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
                    ReceiveTaxes(imperial: imperial, amountOwed: empireLandRightsTax);
                }
                else if (Find.TickManager.TicksGame > nextTaxCollectionTicks)
                {
                    int totalCurrency = DetermineSilverAvailable(imperial: imperial);
                    int resolvedTax = Math.Max(val1: empireLandRightsTax, val2: (int)(totalCurrency * 0.2));
                    ReceiveTaxes(imperial: imperial, amountOwed: resolvedTax);
                }
            }
        }

        public int DetermineSilverAvailable(Pawn imperial)
        {
            int result = 0;
            List<Thing> currencies = imperial.Map.listerThings.ThingsOfDef(def: ThingDefOf.Silver);
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
            List<Thing> currencies = imperial.Map.listerThings.ThingsOfDef(def: ThingDefOf.Silver);
            if (currencies != null && currencies.Count > 0)
            {
                foreach (Thing currency in currencies.InRandomOrder<Thing>())
                {
                    if (amountUnpaid <= 0) { break; }
                    int num = Math.Min(val1: amountUnpaid, val2: currency.stackCount);
                    currency.SplitOff(count: num).Destroy(mode: DestroyMode.Vanish);
                    amountUnpaid -= num;
                    empireFunds += num;
                }
            }

            if (amountUnpaid > 0)
            {
                empireDebt = amountUnpaid;
                int amountPaid = amountOwed - amountUnpaid;
                if (amountPaid < 0) amountPaid = 0;
                Messages.Message(text: "PJ_ImperialTaxes_Owed".Translate(args: new object[]
                {
                                imperial.Map.info.parent.Label,
                                amountPaid,
                                empireDebt
                }), def: MessageTypeDefOf.PositiveEvent);
            }
            else
            {
                Messages.Message(text: "PJ_ImperialTaxes_Paid".Translate(args: new object[]
                    {
                                imperial.Map.info.parent.Label,
                                amountOwed
                    }), def: MessageTypeDefOf.PositiveEvent);
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
                            Find.Maps.ForEach(action: delegate (Map map)
                            {
                                List<Pawn> pawns = map.mapPawns.AllPawnsSpawned.Where(predicate: (Pawn p) => p.Faction != null).ToList();
                                Pawn imperial = pawns.FirstOrDefault(predicate: (Pawn p) => p.Name != null && p.Faction.def.defName.EqualsIgnoreCase(B: "PJ_GalacticEmpire"));
                                if (imperial != null)
                                {
                                    ResolveMeetingGalacticEmpire(imperial: imperial);
                                    ResolveGalacticEmpireTaxDeal(imperial: imperial);
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
            Scribe_Values.Look<bool>(value: ref this.metEmpire, label: "metEmpire", defaultValue: false);
            Scribe_Values.Look<bool>(value: ref this.receivedMessage, label: "receivedEmpire", defaultValue: false);
            Scribe_Values.Look<bool>(value: ref this.taxedColony, label: "taxedColony", defaultValue: false);
            Scribe_Values.Look<int>(value: ref this.empireFunds, label: "empireFunds", defaultValue: 0);
            Scribe_Values.Look<int>(value: ref this.empireDebt, label: "empireDebt", defaultValue: 0);
            Scribe_Values.Look<int>(value: ref this.nextTaxCollectionTicks, label: "lastTaxCollectionTicks", defaultValue: -1);
        }
    }
}
