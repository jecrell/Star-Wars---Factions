using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace SWSaber
{
    public static class Utility
    {
        public static bool modCheck = false;
        public static bool loadedForcePowers = false;
        public static bool loadedLightsabers = false;
        //public static bool loadedFactions = false;

        public static bool AreForcePowersLoaded()
        {
            if (!modCheck) ModCheck();
            return loadedForcePowers;
        }
        //public static bool AreFactionsLoaded()
        //{
        //    if (!modCheck) ModCheck();
        //    return loadedFactions;
        //}
        public static bool AreLightsabersLoaded()
        {
            if (!modCheck) ModCheck();
            return loadedLightsabers;
        }

        public static void ModCheck()
        {
            Log.Message("Mod Check Called");
            loadedForcePowers = false;
            loadedLightsabers = false;
            //loadedFactions = false;
            foreach (ModContentPack ResolvedMod in LoadedModManager.RunningMods)
            {
                if (loadedForcePowers && loadedLightsabers) break; //Save some loading
                if (ResolvedMod.Name.Contains("Star Wars - The Force"))
                {
                    Log.Message("Lightsabers :: Star Wars - The Force Detected.");
                    loadedForcePowers = true;
                }
                //if (ResolvedMod.Name.Contains("Star Wars - Factions"))
                //{
                //    Log.Message("Lightsabers :: Star Wars - Factions Detected.");
                //    loadedFactions = true;
                //}
                if (ResolvedMod.Name.Contains("Star Wars - Fully Functional Lightsabers"))
                {
                    Log.Message("Lightsabers :: Star Wars - Lightsabers.");
                    loadedLightsabers = true;
                }
            }
            modCheck = true;
            return;
        }
    }
}
