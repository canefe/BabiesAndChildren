using System;
using System.Collections.Generic;
using System.Linq;
using BabiesAndChildren.Tools;
using BabiesAndChildren.api;
using Verse;
using RimWorld;
using UnityEngine;

namespace BabiesAndChildren
{
	[HotSwappable]
    public class RaceEditorWindow : Window
    {
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(500f, 500f);
			}
		}


		public override void DoWindowContents(Rect inRect)
		{
			inRect.yMin += 30f;
			Listing_Standard listing_Standard = new Listing_Standard(GameFont.Small);
			listing_Standard.ColumnWidth = Mathf.Min(360f, inRect.width)/1.6f;
			listing_Standard.Begin(inRect);
			listing_Standard.Label("Race Editor : " + alienRace.defName);
			listing_Standard.Label("Save to see changes!");
			if (alienRace.defName == "Human")
				listing_Standard.Label("Only headoffset applies to humans here! Other sizes can be changed from the size settings.");
			List<PawnKindDef> pDefs = new List<PawnKindDef>();
			if (this.pawn == null)
			{
						foreach (PawnKindDef def in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def =>
							def?.race == alienRace))
						{
							pDefs.Add(def);
							break;
						}
							//Pawn pawn = PawnGenerator.GeneratePawn(pDefs.RandomElement());
							Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pDefs.RandomElement(), null, PawnGenerationContext.NonPlayer, -1, true, false, false, false, true, true, 20f, false, true, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, 10f, null, Gender.Female, null, null, null, null, null, false, false, false));
						if (pawn.def.race.Humanlike)
						{
							this.pawn = pawn;
							foreach (ThingWithComps thingWithComps in pawn.apparel.WornApparel)
							{
								ApparelProperties apparel = thingWithComps.def.apparel;
								if (apparel != null && apparel.layers.Contains(ApparelLayerDefOf.Overhead))
								{
									try
									{
										this.pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
										break;
									}
									catch
									{
										break;
									}
								}
								pawn.Drawer.renderer.graphics.ResolveAllGraphics();
							}
							PortraitsCache.SetDirty(this.pawn);
						}
			}
			void DrawPortraitWidget(float left, float top)
			{
				Rect position = new Rect(left, top, 192f, 192f);
				GUI.BeginGroup(position);
				Vector2 vector = new Vector2(128f, 180f);
				Rect position2 = new Rect(position.width * 0.5f - vector.x * 0.5f, 10f + position.height * 0.5f - vector.y * 0.5f, vector.x, vector.y);
				RenderTexture image = PortraitsCache.Get(this.pawn, vector, Rot4.South, default(Vector3), 1f, true, true, true, true, null, null, false);
				GUI.DrawTexture(position2, image);
				GUI.EndGroup();
			}
			listing_Standard.GapLine(15f);
			listing_Standard.Label("Head offset" + ": " + Math.Round(headOffset, 3), -1f, "AlienChildrenZroc_desc".Translate());
			headOffset = listing_Standard.Slider(headOffset, -1f, 2f);
			listing_Standard.GapLine(5f);
			if (alienRace.defName != "Human")
            {
				listing_Standard.Label("Size modifier" + ": " + Math.Round(sizeModifier, 3), -1f, "AlienChildrenZroc_desc".Translate());
				sizeModifier = listing_Standard.Slider(sizeModifier, 0.1f, 2f);
				listing_Standard.GapLine(5f);
				listing_Standard.Label("Hair size" + ": " + Math.Round(hairSizeModifier, 3), -1f, "AlienChildrenZroc_desc".Translate());
				hairSizeModifier = listing_Standard.Slider(hairSizeModifier, 0.1f, 2f);
				listing_Standard.GapLine(5f);
				listing_Standard.Label("Head size" + ": " + Math.Round(headSizeModifier, 3), -1f, "AlienChildrenZroc_desc".Translate());
				headSizeModifier = listing_Standard.Slider(headSizeModifier, 0.1f, 2f);
				listing_Standard.GapLine(5f);
			}
			listing_Standard.GapLine(5f);
			listing_Standard.CheckboxLabeled("Scale children", ref scaleChild, "Enable child scaling");
			listing_Standard.GapLine(5f);
			listing_Standard.CheckboxLabeled("Scale teenagers", ref scaleTeen, "Enable teen scaling");

			if (listing_Standard.ButtonText("Save"))
            {
				RaceUtility.NewSizeSetting(new RaceSettings
				{
					defName = alienRace.defName,
					sizeModifier = sizeModifier,
					headOffset = headOffset,
					hairSizeModifier = hairSizeModifier,
					headSizeModifier = headSizeModifier,
					scaleChild = scaleChild,
					scaleTeen = scaleTeen
				}, alienRace.defName);
				this.Close();
				RaceEditorWindow window = new RaceEditorWindow();
				window.alienRace = DefDatabase<ThingDef>.GetNamed(alienRace.defName);
				RaceSettings raceSettings = RaceUtility.GetSizeSettings(DefDatabase<ThingDef>.GetNamed(alienRace.defName, false));
				CLog.Message("raceSettings " + raceSettings);
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
			if (listing_Standard.ButtonText("Reset Settings"))
			{
				DefaultSizeValues(alienRace.defName);
			}
			listing_Standard.NewColumn();
			listing_Standard.Gap(4f);
			if (pawn != null)
            {
				DrawPortraitWidget(listing_Standard.ColumnWidth, listing_Standard.CurHeight);
			}
			listing_Standard.End();
		}

		protected override float Margin
		{
			get
			{
				return 8f;
			}
		}
		public RaceEditorWindow()
        {
            this.resizeable = false;
            this.draggable = true;
            this.preventCameraMotion = false;
            this.doCloseX = true;
            this.windowRect.x = 5f;
            this.windowRect.y = 5f;
        }

		public ThingDef alienRace;
		public RaceSettings raceSettings;
		public float headOffset = 1f;
		public float sizeModifier = 1f;
		public float headSizeModifier = 1f;
		public float hairSizeModifier = 1f;
		public bool scaleChild = true;
		public bool scaleTeen = true;
		private Pawn pawn;

		private void DefaultSizeValues(string defName)
        {
            switch (defName)
            {

				case "Alien_Moyo":
					headOffset = 0.7666342f;
					hairSizeModifier = 1f;
					headSizeModifier = 1;
					sizeModifier = 1f;
					break;
				case "Alien_Cutebold":
					headOffset = 0.8706897f;
					hairSizeModifier = 1f;
					headSizeModifier = 1;
					sizeModifier = 1f;
					break;
				case "StarWarsRaces_Wookiee":
					headOffset = 0.648f;
					hairSizeModifier = 1f;
					headSizeModifier = 1;
					sizeModifier = 1f;
					break;
				default:
					headOffset = 1f;
					hairSizeModifier = 1f;
					headSizeModifier = 1;
					sizeModifier = 1f;
					break;


			}
        }
    }
}
