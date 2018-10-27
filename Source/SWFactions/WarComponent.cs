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
            if (Find.TickManager.TicksGame % 100 == 0 &&
                !hostilityDeclared)
            {
                hostilityDeclared = true;


                if (Find.FactionManager.FirstFactionOfDef(DefDatabase<FactionDef>.GetNamedSilentFail("PJ_RebelFac")) is
                        Faction rebelFaction &&
                    Find.FactionManager.FirstFactionOfDef(
                            DefDatabase<FactionDef>.GetNamedSilentFail("PJ_GalacticEmpire"))
                        is Faction impFaction)
                {

                    impFaction.TrySetNotAlly(rebelFaction, true);
                    rebelFaction.TrySetNotAlly(impFaction, true);
                    

                    impFaction.TrySetRelationKind(rebelFaction, FactionRelationKind.Hostile);
                    rebelFaction.TrySetRelationKind(impFaction, FactionRelationKind.Hostile);
                    

                    Find.LetterStack.ReceiveLetter("PJ_WarDeclared".Translate(), "PJ_WarDeclaredDesc".Translate(
                        new object[] {rebelFaction.def.label, impFaction.def.label}
                    ), DefDatabase<LetterDef>.GetNamed("PJ_BadUrgent"), null);


                }
            }
        }
    }
}