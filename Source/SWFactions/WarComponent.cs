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

        public WarComponent(World world) : base(world)
        {
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (Find.TickManager.TicksGame % 500 == 0 &&
                !hostilityDeclared)
            {
                hostilityDeclared = true;
                if (Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("PJ_RebelFac")) is
                        Faction rebelFaction &&
                    Find.FactionManager.FirstFactionOfDef(
                            DefDatabase<FactionDef>.GetNamedSilentFail("PJ_GalacticEmpire"))
                        is Faction impFaction)
                {
                    impFaction.TrySetRelationKind(rebelFaction, FactionRelationKind.Hostile, canSendLetter: false);
                    rebelFaction.TrySetRelationKind(impFaction, FactionRelationKind.Hostile, canSendLetter: false);

                    Find.MusicManagerPlay.disabled = true;
                    Find.MusicManagerPlay.ForceSilenceFor(10f);
                    Find.MusicManagerPlay.disabled = false;
                    Find.LetterStack.ReceiveLetter("PJ_WarDeclared".Translate(), "PJ_WarDeclaredDesc".Translate(
                        rebelFaction.def.label, impFaction.def.label
                    ), DefDatabase<LetterDef>.GetNamed("PJ_BadUrgent"), null);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<bool>(ref this.hostilityDeclared, "hostilityDeclared", false);
        }
    }
}