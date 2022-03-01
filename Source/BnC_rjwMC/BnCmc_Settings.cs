using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;


namespace BabiesAndChildren
{

    public class BnC_MCSettingsController : Mod
    {
        public BnC_MCSettingsController(ModContentPack content) : base(content)
        {
            GetSettings<BnCMCSettings>();
        }

        public override string SettingsCategory()
        {
            return "BnCSettings1".Translate() + " - Milkable Colonist";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            BnCMCSettings.DoWindowContents(inRect);
        }
    }

    [HotSwappable]
    internal class BnCMCSettings : ModSettings
    {

        public static bool enabled = true; // mod enabled
        public static float feed = 0.3f; // amount of milk percentage breastfeeding cost


        private static Vector2 scrollPosition = Vector2.zero;
        private static float height = 100f;
        public static void DoWindowContents(Rect inRect)
        {
            inRect.height = inRect.height + 1200f;
            //30f for top page description and bottom close button
            Rect outRect = new Rect(0f, 30f, inRect.width, 550f);
            //-16 for slider, height_modifier - additional height for options
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height * 0.85f);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.ColumnWidth = viewRect.width / 2.05f;

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect, true);
            listingStandard.Begin(viewRect);

            AddSettings(listingStandard, viewRect);
            height = listingStandard.CurHeight;
            listingStandard.End();
            Widgets.EndScrollView();
        }

        private static void AddSettings(Listing_Standard listingStandard, Rect viewRect)
        {
            listingStandard.Gap(4f);

            GUIStyle guistyle = new GUIStyle(Text.CurFontStyle);
            guistyle.fontStyle = FontStyle.Bold;
            guistyle.fontSize = 20;
            GUI.Label(listingStandard.GetRect(Text.CalcHeight("Milkable Colonist Settings", listingStandard.ColumnWidth) + 6f), "Milkable Colonist Settings", guistyle);

            listingStandard.Gap(3f);

            //accelerated growth checkbox 
            listingStandard.CheckboxLabeled("Enabled?", ref enabled, "Should be normal lactating overrided by Milkable Colonist mechanics?\nRequires a restart.");

            listingStandard.Gap(3f);
            listingStandard.Label("Breastfeeding milk percentage cost" + ": " + Math.Round(feed * 100, 0) + "%", -1f, "");
            feed = listingStandard.Slider(feed, 0f, 1f);


        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref enabled, "enabled", true);
            Scribe_Values.Look(ref feed, "feed", 0.3f);
        }
    }
}
