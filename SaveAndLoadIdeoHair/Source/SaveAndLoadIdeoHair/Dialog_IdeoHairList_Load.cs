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
    class Dialog_IdeoHairList_Load : Dialog_IdeoHairList
    {
        #region copy-pasted
        private Action<Ideo> ideoReturner;

		public List<Faction> npcFactions;

		public bool devEditMode;

		//two constructors called with patched Dialog_EditIdeoStyleItems, one for archonexus restart and one else (ignored for now, may need later implementation)
		public Dialog_IdeoHairList_Load(Action<Ideo> ideoReturner)
		{
			interactButLabel = "LoadGameButton".Translate();
			this.ideoReturner = ideoReturner;
		}

		public Dialog_IdeoHairList_Load(Action<Ideo> ideoReturner, List<Faction> npcFactions, bool devEditMode)
			: this(ideoReturner)
		{
			this.npcFactions = npcFactions;
			this.devEditMode = devEditMode;
		}

        #endregion

        protected override void DoFileInteraction(string fileName)
        {
			string filePath = Path.Combine(SavedIdeoHairFolderPath, fileName + ".idh");
			//I don't think the mode parameter does anything except for debugging (second parameter of following line)
			PreLoadUtility.CheckVersionAndLoad(filePath, ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, delegate
			{
				//actual loading action to be run if the versions match and checks turn out correct
				HashSet<string> hashSet = new HashSet<string>();
				if (TryLoadIdeoHair(filePath, out var ideoHair))
				{
					if (!devEditMode && !npcFactions.NullOrEmpty())
					{
						foreach (Faction npcFaction in npcFactions)
						{
							foreach (MemeDef meme in ideo.memes)
							{
								if (!IdeoUtility.IsMemeAllowedFor(meme, npcFaction.def))
								{
									hashSet.Add("UnableToLoadIdeoMemeNotAllowed".Translate(meme.label.Named("MEME"), npcFaction.def.label.Named("FACTIONTYPE")));
								}
							}
							if (npcFaction.def.requiredMemes != null)
							{
								foreach (MemeDef requiredMeme in npcFaction.def.requiredMemes)
								{
									if (!ideo.HasMeme(requiredMeme))
									{
										hashSet.Add("UnableToLoadIdeoMemeRequired".Translate(requiredMeme.label.Named("MEME"), npcFaction.def.label.Named("FACTIONTYPE")));
									}
								}
							}
						}
						foreach (Precept item in ideo.PreceptsListForReading)
						{
							if (!item.def.allowedForNPCFactions)
							{
								hashSet.Add("UnableToLoadIdeoPreceptNotAllowed".Translate(item.Named("PRECEPT")));
							}
						}
					}
					if (hashSet.Count == 0)
					{
						ideoReturner(IdeoGenerator.InitLoadedIdeo(ideo));
						Close();
					}
					else
					{
						TaggedString text = "UnableToLoadIdeoDescription".Translate() + "\n\n" + (from txt in hashSet.AsEnumerable()
																								  orderby txt
																								  select txt).ToLineList(" - ");
						Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, delegate
						{
							Close();
						}, destructive: false, "UnableToLoadIdeoTitle".Translate()));
					}
				}
				else
				{
					Close();
				}
			});
			throw new NotImplementedException();
        }

		public bool TryLoadIdeoHair(string absPath, out IdeoStyleTracker ideoHair) {
			ideoHair = null;
			try
			{
				Scribe.loader.InitLoading(absPath);
				try
				{
					ScribeMetaHeaderUtility.LoadGameDataHeader(ScribeMetaHeaderUtility.ScribeHeaderMode.Ideo, logVersionConflictWarning: true);
					Scribe_Deep.Look(ref ideoHair, "ideoHair");
					Scribe.loader.FinalizeLoading();
				}
				catch
				{
					Scribe.ForceStop();
					throw;
				}
				//ideoHair.ideo.fileName = Path.GetFileNameWithoutExtension(new FileInfo(absPath).Name);
			}
			catch (Exception ex)
			{
				Log.Error("Exception loading ideo: " + ex.ToString());
				ideoHair = null;
				Scribe.ForceStop();
			}
			return ideoHair != null;
		}
    }
}
