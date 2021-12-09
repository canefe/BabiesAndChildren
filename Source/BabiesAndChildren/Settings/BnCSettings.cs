using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;
using System.Linq;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
namespace BabiesAndChildren
{
    //snake_case is used too much here
    [HotSwappable]
    public class BnCSettings : ModSettings
    {
        public enum BabyInheritPercentageHandleEnum
        {
            _None,
            _25,
            _50,
            _75,
            _90,
            _Random,
        }

        //constants
        public const int MAX_TRAIT_COUNT = 3;
        public const double GET_NEW_TYPE_CHANCE = 0.08;
        public const double GET_SPECIFIC_SEXUALITY = 0.08;
        public const double GET_GAY_SEXUALITY = 0.05;
        public const int MAX_ACCELERATED_GROWTH_FACTOR = 20;
        public const float MAX_CRIB_BODYSIZE = 0.6f;

        //settings
        public static bool accelerated_growth = true;
        public static int accelerated_growth_end_age = 14;
        public static int baby_accelerated_growth = 7;
        public static int toddler_accelerated_growth = 7;
        public static int child_accelerated_growth = 7;
        public static int teenager_accelerated_growth = 1;

        public static BabyInheritPercentageHandleEnum baby_Inherit_percentage = BabyInheritPercentageHandleEnum._50;

        public static bool OptionChildrenDropWeapons = true;
        public static float option_child_max_weapon_mass = 2.0f;

        public static bool GestationPeriodDays_Enable = false;
        public static float GestationPeriodDays = 45f;
        public static bool enable_postpartum = true;
        public static float cryVolume = 0.8f;
        public static float STILLBORN_CHANCE = 0.09f;
        public static bool patchhumans = true;
        public static bool playtime_enabled = true;
        public static bool watchworktype_enabled = true;
        public static float watchexpgainmultiplier = 1f;

        public static int minage = 10;
        public static bool rarekids = false;

        public static bool debug_and_gsetting = false;
        public static bool child_cute_act_enabled = true;

        public static bool breastfeed_only = true;

        //make alien heads be human heads if alien heads are fucked
        public static bool human_like_head_enabled = false;
        public static bool Rabbie_Child_head_enabled = false;

        //tweak-able values to visually determine what looks good
        public static float HumanBodySize = 1.1095f;
        public static float HumanHeadSize = 1.4083f;
        public static float HumanHairSize = 1.2589f;
        public static float HumanrootlocZ = -0.11f;
        public static float AlienBodySize = 1f;
        public static float AlienHeadSizeA = 1f;
        public static float AlienHeadSizeB = 1f;
        public static float AlienHairSize = 1f;
        public static float HumanTeenagerModifier = 1f;
        public static float AlienTeenagerModifier = 1f;

        public static float AlienrootlocZ = -0.038f;
        public static float ShowHairSize = 1f;
        public static float ShowHairLocY = -0.003f;
        public static float ShowHairHumanLocZ = 0.092f;
        public static float ShowHairAlienLocZ = 0.035f;
        public static float ShowHairAlienHFLocZ = 0.048f;
        public static float FAModifier = 0.8058f;

        public static List<string> races;
        public static List<string> disabledRaces = new List<string>();
        private static string racesSearch = "";
        private static float hs = 50f;

        private static List<string> rak = new List<string>();
        private static List<RaceSettings> raks = new List<RaceSettings>();


        //special flag to reinitialize all childeren on all maps (not saves) at map load once per "game"/ once per mod added
        //TODO make this a game component

        private static Vector2 scrollPosition = Vector2.zero;
        private static Vector2 racesListScrollPos;
        private static float height = 100f;

        public static void AddDebugSettings(Listing_Standard listingStandard)
        {
            GUIStyle guistyle = new GUIStyle(Text.CurFontStyle);
            guistyle.fontStyle = FontStyle.Bold;
            guistyle.fontSize = 20;
            GUIStyle guistyle_desc = new GUIStyle(Text.CurFontStyle);
            guistyle_desc.fontStyle = FontStyle.Italic;
            guistyle_desc.fontSize = 16;
            listingStandard.Gap(5f);
            listingStandard.CheckboxLabeled("humanlikeHeadEnabled_Title".Translate(), ref human_like_head_enabled, "humanlikeHeadEnabled_desc".Translate());
            listingStandard.Gap(5f);
            listingStandard.CheckboxLabeled("RabbieChildHeadEnabled_Title".Translate(), ref Rabbie_Child_head_enabled, "RabbieChildHeadEnabled_desc".Translate());
            listingStandard.Gap(5f);
            listingStandard.GapLine(5f);
            GUI.Label(listingStandard.GetRect(Text.CalcHeight("Human Children Size", listingStandard.ColumnWidth) + 6f), "Human Children Size", guistyle);
            listingStandard.Label("HumanChildrenBodysize_Title".Translate() + ": " + Math.Round(HumanBodySize, 4), -1f, "HumanChildrenBodysize_desc".Translate());
            HumanBodySize = listingStandard.Slider(HumanBodySize, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("HumanChildrenHeadsize_Title".Translate() + ": " + Math.Round(HumanHeadSize, 4), -1f, "HumanChildrenHeadsize_desc".Translate());
            HumanHeadSize = listingStandard.Slider(HumanHeadSize, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("HumanChildrenHairsize_Title".Translate() + ": " + Math.Round(HumanHairSize, 4), -1f, "HumanChildrenHairsize_desc".Translate());
            HumanHairSize = listingStandard.Slider(HumanHairSize, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("HumanChildrenZroc_Title".Translate() + ": " + Math.Round(HumanrootlocZ, 4), -1f, "HumanChildrenZroc_desc".Translate());
            HumanrootlocZ = listingStandard.Slider(HumanrootlocZ, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("HumanTeenagerModifier_Title".Translate() + ": " + Math.Round(HumanTeenagerModifier, 4), -1f, "HumanTeenagerModifier_desc".Translate());
            HumanTeenagerModifier = listingStandard.Slider(HumanTeenagerModifier, -2f, 2f);
            listingStandard.GapLine(5f);
            GUI.Label(listingStandard.GetRect(Text.CalcHeight("Alien Children Size", listingStandard.ColumnWidth) + 6f), "Alien Children Size", guistyle);
            GUI.Label(listingStandard.GetRect(Text.CalcHeight("Specific size settings available under race settings.\nThis applies to all aliens.", listingStandard.ColumnWidth) + 6f), "Specific size settings available under race settings.\nThis applies to all aliens.", guistyle_desc);
            listingStandard.Gap(3f);
            listingStandard.Label("AlienChildrenBodysize_Title".Translate() + ": " + Math.Round(AlienBodySize, 4), -1f, "AlienChildrenBodysize_desc".Translate());
            AlienBodySize = listingStandard.Slider(AlienBodySize, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("AlienChildrenOriginalHeadsize_Title".Translate() + ": " + Math.Round(AlienHeadSizeA, 4), -1f, "AlienChildrenOriginalHeadsize_desc".Translate());
            AlienHeadSizeA = listingStandard.Slider(AlienHeadSizeA, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("AlienChildrenHumanlikeHeadsize_Title".Translate() + ": " + Math.Round(AlienHeadSizeB, 4), -1f, "AlienChildrenHumanlikeHeadsize_desc".Translate());
            AlienHeadSizeB = listingStandard.Slider(AlienHeadSizeB, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("FAModifier_Title".Translate() + ": " + Math.Round(FAModifier, 4), -1f, "FAModifier_desc".Translate());
            FAModifier = listingStandard.Slider(FAModifier, -1f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("AlienChildrenHairsize_Title".Translate() + ": " + Math.Round(AlienHairSize, 4), -1f, "AlienChildrenHairsize_desc".Translate());
            AlienHairSize = listingStandard.Slider(AlienHairSize, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("AlienChildrenZroc_Title".Translate() + ": " + Math.Round(AlienrootlocZ, 4), -1f, "AlienChildrenZroc_desc".Translate());
            AlienrootlocZ = listingStandard.Slider(AlienrootlocZ, -2f, 2f);
            listingStandard.Gap(5f);
            listingStandard.Label("AlienTeenagerModifier_Title".Translate() + ": " + Math.Round(AlienTeenagerModifier, 4), -1f, "AlienTeenagerModifier_desc".Translate());
            AlienTeenagerModifier = listingStandard.Slider(AlienTeenagerModifier, -2f, 2f);
            listingStandard.GapLine(5f);
            /*{
                listingStandard.Label("ShowHairSize_Title".Translate() + ": " + Math.Round(ShowHairSize, 4), -1f, "ShowHairSize_desc".Translate());
                ShowHairSize = listingStandard.Slider(ShowHairSize, -2f, 2f);
                listingStandard.Gap(5f);
                listingStandard.Label("ShowHairLocY_Title".Translate() + ": " + Math.Round(ShowHairLocY, 4), -1f, "ShowHairLocY_desc".Translate());
                ShowHairLocY = listingStandard.Slider(ShowHairLocY, -2f, 2f);
                listingStandard.Gap(5f);
                listingStandard.Label("ShowHairHumanZLoc_Title".Translate() + ": " + Math.Round(ShowHairHumanLocZ, 4), -1f, "ShowHairHumanZLoc_desc".Translate());
                ShowHairHumanLocZ = listingStandard.Slider(ShowHairHumanLocZ, -2f, 2f);
                listingStandard.Gap(5f);
                listingStandard.Label("ShowHairAlienZLoc_Title".Translate() + ": " + Math.Round(ShowHairAlienLocZ, 4), -1f, "ShowHairAlienZLoc_desc".Translate());
                ShowHairAlienLocZ = listingStandard.Slider(ShowHairAlienLocZ, -2f, 2f);
                listingStandard.Gap(5f);
                listingStandard.Label("ShowHairAlienHFLocZ_Title".Translate() + ": " + Math.Round(ShowHairAlienHFLocZ, 4), -1f, "ShowHairAlienHFLocZ_desc".Translate());
                ShowHairAlienHFLocZ = listingStandard.Slider(ShowHairAlienHFLocZ, -2f, 2f);
            }*/
        }
        public static void Scrol(Listing_Standard raceList, float height, ref float hs)
        {
            Rect rect = raceList.GetRect(height);
            Rect rect2 = rect;
            rect2.height = hs;
            rect2.width -= 16f;
            float lineHeight = Text.LineHeight;
            float num = 0f;
            races = (from x in RaceUtility.thingUsesChildrenCache
                     where !ModTools.IsRobot(x.Key) && !Races.IsBlacklisted(x.Key)
                     select x.Key.defName).ToList<string>();
            RaceSettings(new Rect(0f, 60f, rect2.width, 300f), races, ref disabledRaces);

            //Widgets.DrawBoxSolid(rect, Color.red);
            //Widgets.DrawBoxSolid(rect2, Color.white);
            hs = num;
            raceList.Gap(raceList.verticalSpacing);
        }
        public static void RaceSettings(Rect rect, List<string> label, ref List<string> alienRaces, string tooltip = null)
        {
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                TooltipHandler.TipRegion(rect, tooltip);
            }
            if (alienRaces.NullOrEmpty<string>())
            {
                alienRaces = new List<string>();
            }
            Listing_Standard listing_Standard = new Listing_Standard();
            Rect outRect = rect;
            Rect rect2 = rect;
            rect2.height = label.Count * 30f;
            rect2.width -= 16f;
            Widgets.BeginScrollView(outRect, ref racesListScrollPos, rect2);
            listing_Standard.Begin(rect2);
            for (int i = 0; i < races.Count; i++)
            {
                bool flag = (!alienRaces.NullOrEmpty() ? !alienRaces.Contains(label[i]) : true);
                if (racesSearch == null || Contai(races[i], racesSearch.ToLower(), StringComparison.OrdinalIgnoreCase))
                {
                    WidgetRow widgetRow = new WidgetRow(rect.x, listing_Standard.CurHeight, UIDirection.RightThenUp, 99999f, 1f);
                    widgetRow.Label(label[i], rect.width * 0.8f, null, -1f);
                    if(label[i] != "Human")
                        widgetRow.ToggleableIcon(ref flag, TexButton.IconBook, "Disable/Enable race (restart needed)", null, null);
                    if (widgetRow.ButtonIcon(TexButton.ToggleTweak, "Size settings"))
                    {
                        if (Current.Game != null)
                        {
                            if (!Find.WindowStack.TryRemove(typeof(RaceEditorWindow), true))
                            {
                                RaceSettings raceSettings = RaceUtility.GetSizeSettings(DefDatabase<ThingDef>.GetNamed(label[i], false));
                                RaceEditorWindow window = new RaceEditorWindow();
                                window.alienRace = DefDatabase<ThingDef>.GetNamed(label[i]);
                                if (raceSettings != null)
                                {
                                    window.raceSettings = raceSettings;
                                    window.headOffset = raceSettings.headOffset;
                                    window.sizeModifier = raceSettings.sizeModifier;
                                    window.hairSizeModifier = raceSettings.hairSizeModifier;
                                    window.headSizeModifier = raceSettings.headSizeModifier;
                                    window.scaleChild = raceSettings.scaleChild;
                                    window.scaleTeen = raceSettings.scaleTeen;
                                }
                                Find.WindowStack.Add(window);
                            }
                        }
                        else
                        {
                            Messages.Message("You need to be in-game to open size editor", MessageTypeDefOf.RejectInput);
                        }
                    }
                    if (!flag)
                    {
                        if (!alienRaces.Contains(label[i]))
                        {
                            alienRaces.Add(label[i]);
                        }
                    }
                    else
                    {
                        if (alienRaces.Contains(label[i]))
                        {
                            alienRaces.Remove(label[i]);
                        }
                    }
                    listing_Standard.Gap(30f);
                }
            }
            listing_Standard.End();
            Widgets.EndScrollView();
            
        }
        public static bool Contai(string source, string toCheck, StringComparison comp)
        {
            return source != null && source.IndexOf(toCheck, comp) >= 0;
        }

        private static void AddSettings (Listing_Standard listingStandard, Rect viewRect){
            listingStandard.Gap(4f);
            //Reset button
            if (listingStandard.ButtonText("BnCSetting_Init".Translate()))
            {
                SoundDefOf.Click.PlayOneShotOnCamera(null);
                Reset_Settings();
            }

            listingStandard.GapLine(5f);

            GUIStyle guistyle = new GUIStyle(Text.CurFontStyle);
            guistyle.fontStyle = FontStyle.Bold;
            guistyle.fontSize = 20;
            GUI.Label(listingStandard.GetRect(Text.CalcHeight("AcceleratedGrowth_title".Translate(), listingStandard.ColumnWidth) + 6f), "AcceleratedGrowth_title".Translate(), guistyle);

            listingStandard.Gap(3f);

            //accelerated growth checkbox 
            listingStandard.CheckboxLabeled("AcceleratedGrowth_title".Translate(), ref accelerated_growth, "AcceleratedGrowth_desc".Translate());
            if (accelerated_growth)
            {
                listingStandard.Gap(3f);
                listingStandard.Label("AcceleratedGrowthEndAge_title1".Translate() + ": " + accelerated_growth_end_age, -1f, "AcceleratedGrowthEndAge_desc1".Translate());
                accelerated_growth_end_age = (int)listingStandard.Slider(accelerated_growth_end_age, 0, 18);
                listingStandard.Gap(3f);
                listingStandard.Label("BabyAcceleratedGrowth_title".Translate() + ": " + baby_accelerated_growth, -1f, "BabyAcceleratedGrowth_desc".Translate());
                baby_accelerated_growth = (int)listingStandard.Slider(baby_accelerated_growth, 1, MAX_ACCELERATED_GROWTH_FACTOR);
                listingStandard.Gap(3f);
                listingStandard.Label("ToddlerAcceleratedGrowth_title".Translate() + ": " + toddler_accelerated_growth, -1f, "ToddlerAcceleratedGrowth_desc".Translate());
                toddler_accelerated_growth = (int)listingStandard.Slider(toddler_accelerated_growth, 1, MAX_ACCELERATED_GROWTH_FACTOR);
                listingStandard.Gap(3f);
                listingStandard.Label("ChildAcceleratedGrowth_title".Translate() + ": " + child_accelerated_growth, -1f, "ChildAcceleratedGrowth_desc".Translate());
                child_accelerated_growth = (int)listingStandard.Slider(child_accelerated_growth, 1, MAX_ACCELERATED_GROWTH_FACTOR);
                listingStandard.Gap(3f);
                listingStandard.Label("TeenagerAcceleratedGrowth_title".Translate() + ": " + teenager_accelerated_growth, -1f, "TeenagerAcceleratedGrowth_desc".Translate());
                teenager_accelerated_growth = (int)listingStandard.Slider(teenager_accelerated_growth, 1, MAX_ACCELERATED_GROWTH_FACTOR);
            }

            listingStandard.GapLine(5f);

            GUI.Label(listingStandard.GetRect(Text.CalcHeight("SpawningSettings".Translate(), listingStandard.ColumnWidth) + 6f), "SpawningSettings".Translate(), guistyle);

            listingStandard.Gap(3f);

            listingStandard.CheckboxLabeled("EnableChildrenSpawning_Title".Translate(), ref patchhumans, "EnableChildrenSpawning_desc".Translate());
            if (patchhumans)
            {
                listingStandard.Gap(5f);

                listingStandard.Label("SettingMinAge_Title".Translate() + ": " + minage, -1f, "SettingMinAge_desc".Translate());
                minage = (int)listingStandard.Slider(minage, 5, 13);

                listingStandard.Gap(5f);

                listingStandard.CheckboxLabeled("SettingChildRarity_Title".Translate(), ref rarekids, "SettingChildRarity_desc".Translate());
            }

            listingStandard.GapLine(5f);

            GUI.Label(listingStandard.GetRect(Text.CalcHeight("WatchingSettings".Translate(), listingStandard.ColumnWidth) + 6f), "WatchingSettings".Translate(), guistyle);

            listingStandard.Gap(3f);

            listingStandard.CheckboxLabeled("EnableChildrenWatching_Title".Translate(), ref watchworktype_enabled, "EnableChildrenWatching_desc".Translate());
            listingStandard.Gap(2f);
            listingStandard.Label("ChildrenWatchingExpGainMultiplier_Title".Translate() + ": " + Math.Round(watchexpgainmultiplier * 100, 0) + "%", -1f, "ChildrenWatchingExpGainMultiplier_desc".Translate());
            watchexpgainmultiplier = listingStandard.Slider(watchexpgainmultiplier, 0.1f, 2f);
            listingStandard.GapLine(5f);

            GUI.Label(listingStandard.GetRect(Text.CalcHeight("MiscSettings".Translate(), listingStandard.ColumnWidth) + 6f), "MiscSettings".Translate(), guistyle);

            listingStandard.Gap(3f);

            listingStandard.CheckboxLabeled("ChildCuteActEnabled_Title".Translate(), ref child_cute_act_enabled, "ChildCuteActEnabled_desc".Translate());

            listingStandard.Gap(5f);

            listingStandard.CheckboxLabeled("EnableChildrenPlaying_Title".Translate(), ref playtime_enabled, "EnableChildrenPlaying_desc".Translate());

            listingStandard.Gap(5f);

            listingStandard.CheckboxLabeled("BreastfeedOnly_Title".Translate(), ref breastfeed_only, "BreastfeedOnly_desc".Translate());

            listingStandard.Gap(5f);

            //Children drop weapons
            listingStandard.CheckboxLabeled("OptionChildrenDropWeapons".Translate(), ref OptionChildrenDropWeapons, "OptionChildrenDropWeaponsDesc".Translate());
            if (OptionChildrenDropWeapons)
            {
                //Child max weapon mass
                listingStandard.Gap(5f);
                listingStandard.Label("OptionChildMaxWeaponMass".Translate() + ": " + Math.Round(option_child_max_weapon_mass, 1) + "Kg", -1f, "OptionChildMaxWeaponMass_desc".Translate());
                option_child_max_weapon_mass = listingStandard.Slider(option_child_max_weapon_mass, 0f, 5f);
            }

            listingStandard.GapLine(5f);

            GUI.Label(listingStandard.GetRect(Text.CalcHeight("BabySettings".Translate(), listingStandard.ColumnWidth) + 6f), "BabySettings".Translate(), guistyle);

            listingStandard.Gap(3f);
            //Gestation period settings
            listingStandard.CheckboxLabeled("GestationPeriodDays_Enable".Translate(), ref GestationPeriodDays_Enable, "GestationPeriodDays_Enable_Desc".Translate());
            if (GestationPeriodDays_Enable)
            {
                listingStandard.Gap(3f);
                listingStandard.Label("GestationPeriodDays_Title".Translate() + ": " + Math.Round(GestationPeriodDays, 0) + "GestationPeriodDays_Days".Translate(), -1f, "GestationPeriodDays_desc".Translate());
                GestationPeriodDays = listingStandard.Slider(GestationPeriodDays, 1f, 50f);
            }
            listingStandard.Gap(5f);

            //Post pregnancy setting thingy
            listingStandard.CheckboxLabeled("EnablePostPartum_title".Translate(), ref enable_postpartum, "EnablePostPartum_desc".Translate());

            listingStandard.Gap(5f);

            //Cry volume slider
            listingStandard.Label("CryVolumeSetting_title".Translate() + ": " + Math.Round(cryVolume * 100, 0) + "%", -1f, "CryVolumeSetting_desc".Translate());
            cryVolume = listingStandard.Slider(cryVolume, 0f, 1f);

            listingStandard.Gap(5f);

            listingStandard.Label("StillbornChance_Title".Translate() + ": " + Math.Round(STILLBORN_CHANCE * 100, 0) + "%", -1f, "StillbornChance_desc".Translate());
            STILLBORN_CHANCE = listingStandard.Slider(STILLBORN_CHANCE, 0f, 0.5f);


            listingStandard.GapLine(5f);
            GUI.Label(listingStandard.GetRect(Text.CalcHeight("HARSettings".Translate(), listingStandard.ColumnWidth) + 6f), "HARSettings".Translate(), guistyle);

            listingStandard.Gap(3f);
            Listing_Standard raceList = new Listing_Standard();
            raceList.Begin(listingStandard.GetRect(viewRect.height - listingStandard.CurHeight));
            raceList.Label("Race Settings", -1f, null);
            racesSearch = raceList.TextEntry(racesSearch, 1);
            raceList.GapLine(4f);
            Scrol(raceList, 450f, ref hs);

            raceList.End();

            //////////////////////////// right column
            listingStandard.NewColumn();
            GUI.contentColor = Color.white;

            listingStandard.Gap(4f);

            //debug logging and settings checkbox
            listingStandard.CheckboxLabeled("DebugInfoMessagesEnabled_Title".Translate(), ref debug_and_gsetting, "DebugInfoMessagesEnabled_desc".Translate());
            if (debug_and_gsetting)
            {
                AddDebugSettings(listingStandard);
            }


        }

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

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref accelerated_growth, "accelerated_growth");
            Scribe_Values.Look(ref accelerated_growth_end_age, "accelerated_growth_end_age");
            Scribe_Values.Look(ref baby_accelerated_growth, "baby_accelerated_growth");
            Scribe_Values.Look(ref toddler_accelerated_growth, "toddler_accelerated_growth");
            Scribe_Values.Look(ref child_accelerated_growth, "child_accelerated_growth");
            Scribe_Values.Look(ref teenager_accelerated_growth, "teenager_accelerated_growth");

            Scribe_Values.Look(ref baby_Inherit_percentage, "baby_Inherit_percentage");
            Scribe_Values.Look(ref OptionChildrenDropWeapons, "OptionChildrenDropWeapons");
            Scribe_Values.Look(ref option_child_max_weapon_mass, "option_child_max_weapon_mass");
            Scribe_Values.Look(ref GestationPeriodDays_Enable, "GestationPeriodDays_Enable");
            Scribe_Values.Look(ref GestationPeriodDays, "GestationPeriodDays");
            Scribe_Values.Look(ref enable_postpartum, "enable_postpartum");
            Scribe_Values.Look(ref patchhumans, "patchhumans", true);
            Scribe_Values.Look(ref playtime_enabled, "playtime_enabled", true);
            Scribe_Values.Look(ref watchworktype_enabled, "watchworktype_enabled", true);
            Scribe_Values.Look(ref watchexpgainmultiplier, "watchexpgainmultiplier", 1f);
            Scribe_Values.Look(ref minage, "minage", 10);
            Scribe_Values.Look(ref rarekids, "rarekids", false);
            Scribe_Values.Look(ref cryVolume, "cryVolume");
            Scribe_Values.Look(ref STILLBORN_CHANCE, "STILLBORN_CHANCE", 0.09f);

            Scribe_Values.Look(ref debug_and_gsetting, "debug_and_gsetting");
            Scribe_Values.Look(ref child_cute_act_enabled, "child_cute_act_enabled");
            Scribe_Values.Look(ref breastfeed_only, "breastfeed_only", true);
            Scribe_Values.Look(ref human_like_head_enabled, "human_like_head_enabled");
            Scribe_Values.Look(ref Rabbie_Child_head_enabled, "Rabbie_Child_head_enabled");
            Scribe_Values.Look(ref HumanBodySize, "HumanBodySize");
            Scribe_Values.Look(ref HumanHeadSize, "HumanHeadSize");
            Scribe_Values.Look(ref HumanHairSize, "HumanHairSize");
            Scribe_Values.Look(ref HumanrootlocZ, "HumanrootlocZ");
            Scribe_Values.Look(ref AlienBodySize, "AlienBodySize");
            Scribe_Values.Look(ref AlienHeadSizeA, "AlienHeadSizeA");
            Scribe_Values.Look(ref AlienHeadSizeB, "AlienHeadSizeB");
            Scribe_Values.Look(ref AlienHairSize, "AlienHairSize");
            Scribe_Values.Look(ref AlienrootlocZ, "AlienrootlocZ");
            Scribe_Values.Look(ref AlienTeenagerModifier, "AlienTeenagerModifier", 1f);
            Scribe_Values.Look(ref HumanTeenagerModifier, "HumanTeenagerModifier", 1f);
            Scribe_Values.Look(ref AlienrootlocZ, "AlienrootlocZ");
            Scribe_Values.Look(ref ShowHairSize, "ShowHairSize");
            Scribe_Values.Look(ref ShowHairLocY, "ShowHairLocY");
            Scribe_Values.Look(ref ShowHairHumanLocZ, "ShowHairHumanLocZ");
            Scribe_Values.Look(ref ShowHairAlienLocZ, "ShowHairAlienLocZ");
            Scribe_Values.Look(ref ShowHairAlienHFLocZ, "ShowHairAlienHFLocZ");
            Scribe_Values.Look(ref FAModifier, "FAModifier");
            Scribe_Collections.Look<string>(ref disabledRaces, "DisabledRaces", LookMode.Undefined, Array.Empty<object>());
            Scribe_Collections.Look(ref RaceUtility.alienRaceSettings, "alienRaceSettings", LookMode.Value, LookMode.Deep, ref rak, ref raks);
        }

        public static void Reset_Settings()
        {
            accelerated_growth = true;
            accelerated_growth_end_age = 14;
            baby_accelerated_growth = 7;
            toddler_accelerated_growth = 7;
            child_accelerated_growth = 7;
            teenager_accelerated_growth = 1;
            baby_Inherit_percentage = BabyInheritPercentageHandleEnum._50;
            OptionChildrenDropWeapons = true;
            option_child_max_weapon_mass = 2.0f;
            GestationPeriodDays_Enable = false;
            GestationPeriodDays = 45f;
            enable_postpartum = true;
            patchhumans = true;
            playtime_enabled = true;
            watchworktype_enabled = true;
            watchexpgainmultiplier = 1f;
            minage = 10;
            rarekids = false;
            cryVolume = 0.8f;
            STILLBORN_CHANCE = 0.09f;
            debug_and_gsetting = false;
            child_cute_act_enabled = true;
            breastfeed_only = true;
            human_like_head_enabled = false;
            Rabbie_Child_head_enabled = false;
            HumanBodySize = 1.1095f;
            HumanHeadSize = ChildrenBase.ModFacialAnimation_ON ? 0.9402f : 1.4083f;
            HumanHairSize = ChildrenBase.ModFacialAnimation_ON ? 1.0697f : 1.2589f;
            HumanrootlocZ = -0.11f;
            AlienTeenagerModifier = 1f;
            HumanTeenagerModifier = 1f;
            AlienBodySize = 1f;
            AlienHeadSizeA = 1f;
            AlienHeadSizeB = 1f;
            AlienHairSize = 1f;
            AlienrootlocZ = -0.038f;
            ShowHairSize = 1f;
            ShowHairLocY = -0.003f;
            ShowHairHumanLocZ = 0.092f;
            ShowHairAlienLocZ = 0.035f;
            ShowHairAlienHFLocZ = 0.048f;
            FAModifier = 0.8058f;
            disabledRaces = new List<string>();
            RaceUtility.SizeSettings(true);
        }
    }
}