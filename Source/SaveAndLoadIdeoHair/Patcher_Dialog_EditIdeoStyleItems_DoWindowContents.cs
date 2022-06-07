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
