using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace SWFactions
{
    public class WarComponent : WorldComponent
    {
        public bool hostilityDeclared = false;

        public WarComponent(World world) : base(world: world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % 500 == 0 &&
                !hostilityDeclared)
            {
                hostilityDeclared = true;
                if (Find.FactionManager.FirstFactionOfDef(facDef: DefDatabase<FactionDef>.GetNamedSilentFail(defName: "PJ_RebelFac")) is { } rebelFaction &&
                    Find.FactionManager.FirstFactionOfDef(
                            facDef: DefDatabase<FactionDef>.GetNamedSilentFail(defName: "PJ_GalacticEmpire"))
                        is { } impFaction)
                {
                    impFaction.ChangeGoodwill_Debug(rebelFaction, -100);
                    rebelFaction.ChangeGoodwill_Debug(impFaction, -100);
                    
                    //impFaction.SetRelationDirect(other: rebelFaction,kind: FactionRelationKind.Hostile,canSendHostilityLetter: false);
                    //rebelFaction.SetRelationDirect(other: impFaction, kind: FactionRelationKind.Hostile, canSendHostilityLetter: false);

                    Find.MusicManagerPlay.disabled = true;
                    Find.MusicManagerPlay.ForceSilenceFor(time: 10f);
                    Find.MusicManagerPlay.disabled = false;
                    Find.LetterStack.ReceiveLetter(label: "PJ_WarDeclared".Translate(),   
                        text: "PJ_WarDeclaredDesc".Translate(rebelFaction.def.label, impFaction.def.label)
                    , textLetterDef: DefDatabase<LetterDef>.GetNamed(defName: "PJ_BadUrgent"), debugInfo: null);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(value: ref this.hostilityDeclared, label: "hostilityDeclared", defaultValue: false);
        }
    }
}