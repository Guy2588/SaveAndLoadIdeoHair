using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RimWorld;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace SaveAndLoadIdeoHair
{
    [HarmonyPatch(typeof(Dialog_EditIdeoStyleItems), "DoWindowContents")]
    public static class Patcher_Dialog_EditIdeoStyleItems_DoWindowContents
    {
        [HarmonyPostfix]
        public static void DoWindowContents_ButWithSaveAndLoadButtons(ref Rect rect, Dialog_EditIdeoStyleItems __instance)
        {
            Dialog_EditIdeoStyleItems dialog_EditIdeoStyleItems = __instance;

            //copied over needed variables from the original class and method
            rect = rect.ExpandedBy(18f); //resets the contraction
            Vector2 ButSize = new Vector2(150f, 38f);
            Rect rect4 = new Rect(rect.xMax - ButSize.x - 10f, rect.y + 10f, ButSize.x, 30f);
            //aquires ideoligion via reflection (code from a good friend who knows harmony way better than I do)
            Traverse ideoField = Traverse.Create(dialog_EditIdeoStyleItems).Field("ideo");
            Ideo ideo = ideoField.GetValue<Ideo>();
            //aquires editMode via reflection (I'm not going to change either of these values, I just need to look at them)
            Traverse editModeField = Traverse.Create(dialog_EditIdeoStyleItems).Field("editMode");
            IdeoEditMode editMode = editModeField.GetValue<IdeoEditMode>();

            //save
            if (Widgets.ButtonText(new Rect(rect4.x - ButSize.x - 10f, rect4.y, ButSize.x, 30f), "Save".Translate())) {  //button to the left of expand all
                //Log.Message("Save");
                //updates the ideoligion with the new hairstyles so that saving scribes the correct hairstyles (copy pasted from core (am I allowed to do this?))
                //cancel button won't work after you hit the save button: your changes to the hairstyles will be saved to the ideoligion. This shouldn't be that bad of a problem though
                //could try something like AssignManager.SaveCurrentState from BetterPawnControl
                if (editMode != 0)
                {
                    //aquires the three style frequencies via reflection
                    Traverse hairFrequenciesField = Traverse.Create(dialog_EditIdeoStyleItems).Field("hairFrequencies");
                    DefMap<HairDef, StyleItemSpawningProperties> hairFrequencies = hairFrequenciesField.GetValue<DefMap<HairDef, StyleItemSpawningProperties>>();
                    Traverse beardFrequenciesField = Traverse.Create(dialog_EditIdeoStyleItems).Field("beardFrequencies");
                    DefMap<BeardDef, StyleItemSpawningProperties> beardFrequencies = beardFrequenciesField.GetValue<DefMap<BeardDef, StyleItemSpawningProperties>>();
                    Traverse tattooFrequenciesField = Traverse.Create(dialog_EditIdeoStyleItems).Field("tattooFrequencies");
                    DefMap<TattooDef, StyleItemSpawningProperties> tattooFrequencies = tattooFrequenciesField.GetValue<DefMap<TattooDef, StyleItemSpawningProperties>>();

                    foreach (KeyValuePair<HairDef, StyleItemSpawningProperties> hairFrequency in hairFrequencies)
                    {
                        ideo.style.SetFrequency(hairFrequency.Key, hairFrequency.Value.frequency);
                        ideo.style.SetGender(hairFrequency.Key, hairFrequency.Value.gender);
                    }
                    foreach (KeyValuePair<BeardDef, StyleItemSpawningProperties> beardFrequency in beardFrequencies)
                    {
                        ideo.style.SetFrequency(beardFrequency.Key, beardFrequency.Value.frequency);
                        ideo.style.SetGender(beardFrequency.Key, beardFrequency.Value.gender);
                    }
                    foreach (KeyValuePair<TattooDef, StyleItemSpawningProperties> tattooFrequency in tattooFrequencies)
                    {
                        ideo.style.SetFrequency(tattooFrequency.Key, tattooFrequency.Value.frequency);
                        ideo.style.SetGender(tattooFrequency.Key, tattooFrequency.Value.gender);
                    }
                    ideo.style.EnsureAtLeastOneStyleItemAvailable();
                }

                Find.WindowStack.Add(new Dialog_IdeoHairList_Save(ideo));
            }

            //load (check if allow load), similar to above
            if (editMode != 0) //enum, 0 = not allowed to edit, 1-3 is some form of able to edit
            {
                if (Widgets.ButtonText(new Rect(rect4.x - ButSize.x - 10f, rect4.yMax + 4f, ButSize.x, 30f), "Load".Translate()))
                {  //button to the left of collapse all
                   //Log.Message("Load");
                    Find.WindowStack.Add(new Dialog_IdeoHairList_Load(ideo, dialog_EditIdeoStyleItems));
                }
            }
        }
    }
}
