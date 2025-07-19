using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RimWorld;
using Verse;

namespace SaveAndLoadIdeoHair
{
    class Dialog_IdeoHairList_Save : Dialog_IdeoHairList
    {
        private Ideo savingIdeo; //pass in a whole ideoligion, but we don't have to save the entire thing, just pieces that we need
        //potential problem when loading and have to repackage the pieces into an ideoligion?

        protected override bool ShouldDoTypeInField => true; //no idea what this does, maybe allows saving while a game is in progress? I'll have to follow it to find out...

        //constructor, called in the patched method of Dialog_EditIdeoStyleItems
        public Dialog_IdeoHairList_Save(Ideo ideo) { 
            interactButLabel = "OverwriteButton".Translate();
            typingName = ideo.name;
            savingIdeo = ideo;
        }

        //activates when override button or save button is clicked on the list of ideo hairs to save
        protected override void DoFileInteraction(string fileName)
        {
            fileName = GenFile.SanitizedFileName(fileName);
            string absPath = Path.Combine(SavedIdeoHairFolderPath, fileName + ".idh"); //.idh when i figure out the save-load system

            //original code delegates this to a helper class

            LongEventHandler.QueueLongEvent(delegate
            {
                SaveIdeoHair(savingIdeo.style, absPath);
            }, "SavingLongEvent", doAsynchronously: false, null);

            Messages.Message("SavedAs".Translate(fileName), MessageTypeDefOf.SilentInput, historical: false);
            Close();
        }

        public void SaveIdeoHair(IdeoStyleTracker ideoHair, string absFilePath) {
            try
            {
                //ideoHair.ideo.fileName = Path.GetFileNameWithoutExtension(absFilePath);
                //luckily, IdeoStyleTracker inherits from IExposable so I don't have to define any sort of save functions and mess with things that I don't know
                //Thanks Rimworld developers!
                SafeSaver.Save(absFilePath, "savedideohair", delegate
                {
                    ScribeMetaHeaderUtility.WriteMetaHeader();
                    Scribe_Deep.Look(ref ideoHair, "ideohair");
                });
            }
            catch (Exception ex)
            {
                Log.Error("Exception while saving ideo hair: " + ex.ToString());
            }
        }
    }
}
