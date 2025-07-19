using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using RimWorld;
using Verse;
using HarmonyLib;

namespace SaveAndLoadIdeoHair
{
    class Dialog_IdeoHairList_Load : Dialog_IdeoHairList
    {
		private Ideo ideoToEdit;
		private Dialog_EditIdeoStyleItems dialog;

		public Dialog_IdeoHairList_Load(Ideo ideo, Dialog_EditIdeoStyleItems dialog)
		{
			interactButLabel = "LoadGameButton".Translate();
			this.ideoToEdit = ideo;
			this.dialog = dialog;
		}

        protected override void DoFileInteraction(string fileName)
        {
			string filePath = Path.Combine(SavedIdeoHairFolderPath, fileName + ".idh");
			//I don't think the mode parameter does anything except for debugging (second parameter of following line)
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, delegate
			{
				//actual loading action to be run if the versions match and checks turn out correct
				//gets saved IdeoStyleTracker and replaces the old one in ideoToEdit with a new one
				//ideoToEdit.style = ideoHair;
				HashSet<string> hashSet = new HashSet<string>();
				if (TryLoadIdeoHair(filePath, out var ideoHair))
				{
					//Log.Message("DoFileInteraction true on loading " + fileName);
					ideoToEdit.style = ideoHair;
					IdeoGenerator.InitLoadedIdeo(ideoToEdit);
					//redraws ideo hair section, getting the private method via harmony
					AccessTools.Method(typeof(Dialog_EditIdeoStyleItems), "Reset").Invoke(dialog, new object[] {  });
					Close();
				}
				else
				{
					//Log.Message("DoFileInteration false on loading " + fileName);
					Close();
				}
			});
        }

		//once it loads, the cancel button doesn't undo the changes
		public bool TryLoadIdeoHair(string absPath, out IdeoStyleTracker ideoHair) {
			ideoHair = null;
			try
			{
				Scribe.loader.InitLoading(absPath);
				try
				{
					//Log.Message("Looking at " + absPath);
					ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, logVersionConflictWarning: true);
					Scribe_Deep.Look(ref ideoHair, "ideohair"); //don't have capital! Can't belive a capitalization error was causing me so much trouble
					Scribe.loader.FinalizeLoading();
				}
				catch
				{
					Scribe.ForceStop();
					throw;
				}
				//ideoHair.ideo.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
			}
			catch (Exception ex) //no error here though
			{
				Log.Error("Exception loading ideo: " + ex.ToString());
				ideoHair = null;
				Scribe.ForceStop();
			}
			return ideoHair != null; //error doesn't return anything
		}
    }
}
