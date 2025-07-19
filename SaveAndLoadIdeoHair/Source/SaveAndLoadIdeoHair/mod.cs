using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;

namespace SaveAndLoadIdeoHair
{
    public class harminy : Mod
    {
        //Entire mod should just be a harmony patch
        //Although the original method is labeled override, I'd have to create a child class of that to do anything with that and doing so doesn't seem to be the right path to achieve my goal
        public harminy(ModContentPack content) : base(content)
        {

            //Calls harmony patches. Code is copy-pasted from somewhere since IDK how to set up harmony. You can copy-paste mine too, I don't mind.
            var harmony = new Harmony("Guy258.SaveAndLoadIdeoHair");
            try
            {
                Log.Message("Save and Load Ideoligion Hair Harmony Patching...");
                harmony.PatchAll();

            }
            catch (Exception e)
            {
                Log.Error("(Save and Load Ideoligion Hair) Failed to apply 1 or more harmony patches! See error:");
                Log.Error(e.ToString());
            }
        }
    }
}
