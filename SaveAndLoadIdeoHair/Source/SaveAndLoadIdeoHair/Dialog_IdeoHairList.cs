using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace SaveAndLoadIdeoHair
{
    //parent class for the scrollable window that displays what set of hair styles you want to save or load
    public abstract class Dialog_IdeoHairList : Dialog_IdeoList //Rimsaves tampers with Dialog_FileList so I'll inherit off Dialog_IdeoList and re-override their override. Hopefully no problems emerge.
    {
		#region May need to be moved into a static helper class

		//basically prepare carefully's save and load code. I wonder why their accessing private methods stuff are so complicated?
		//originally, they're all static. Do I have to make these static? Perhaps put them in a static helper class?
		MethodInfo GenFilePaths_FolderUnderSaveData = AccessTools.Method(typeof(GenFilePaths), "FolderUnderSaveData");
		//public MethodInfo GenFilePaths_FolderUnderSaveData = typeof(GenFilePaths).GetMethod("FolderUnderSaveData", BindingFlags.NonPublic | BindingFlags.Static, null, CallingConventions.Any, new Type[] { typeof(string) }, null);

		//recreation of the base game's folder path generator method by reflection
		public string FolderUnderSaveData(string name)
		{
			try
			{
				return (string)GenFilePaths_FolderUnderSaveData.Invoke(null, new object[] { name });
			}
			catch (Exception e)
			{
				Log.Error("Failed to invoke reflected method to return save path for files"); //possible that reflection didn't work
				throw e;
			}
		}

		//gets the save folder for the hairstyles with a fancy looking null check (although I did throw out null check code for the two methods above so that might become a problem at some point)
		//not strictly neccesary, I could just invoke the FolderUnderSaveData method in AllCustonIdeoHairFiles, but I guess a null check at some point is neccesary, can't just be going through without
		public string SavedIdeoHairFolderPath
		{
			get
			{
				try
				{
					return FolderUnderSaveData("SaveAndLoadIdeoHair");
				}
				catch (Exception e)
				{
					Log.Error("Failed to get ideologion hair save directory"); //error thrown, null reference exception (either method doesn't work, or path doesn't exist and should be created beforehand)
					throw e;
				}
			}
		}

		//a list of all the hairstyle files so that they can be displayed on a scroll list
		public IEnumerable<FileInfo> AllCustomIdeoHairFiles {
			get {
				DirectoryInfo directoryInfo = new DirectoryInfo(SavedIdeoHairFolderPath);
				//if the place where they're supposed to be stored doesn't exist, create it
				if (!directoryInfo.Exists)
				{
					directoryInfo.Create();
				}
				//no idea what this means, just jumbled nonsense that was copy-pasted that makes a list of the files in the place
				return from f in directoryInfo.GetFiles()
					   where f.Extension == ".idh" //this probably should be changed (maybe .idh (for IDeoligion Hair)), gonna have to mess around with the saver + loader
					   orderby f.LastWriteTime descending
					   select f;
			}
		}

        #endregion
        #region Override void, essential for the class

        //files is a property/variable of parent class Dialog_FileList, this method repopulates the list so a child class can display
        protected override void ReloadFiles()
        {
			files.Clear();
			foreach (FileInfo allCustomIdeoHairFile in AllCustomIdeoHairFiles)
			{
				try
				{
					SaveFileInfo saveFileInfo = new SaveFileInfo(allCustomIdeoHairFile);
					saveFileInfo.LoadData();
					files.Add(saveFileInfo);
				}
				catch (Exception ex)
				{
					Log.Error("Exception loading " + allCustomIdeoHairFile.Name + ": " + ex.ToString());
				}
			}
		}

		#endregion Override void, essential for the class
	}
}
