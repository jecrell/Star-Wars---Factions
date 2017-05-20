using RimWorld;
using System;
using Verse;
using Verse.Grammar;

namespace SWFactions
{
    public class Rule_PlanetName : Rule
    {
        public int selectionWeight = 1;
        
        public override float BaseSelectionWeight
        {
            get
            {
                return (float)this.selectionWeight;
            }
        }

        public override string Generate()
        {
            return Find.World.info.name;
        }

        public override string ToString()
        {
            return this.keyword + "->(worldname)";
        }
    }
}
