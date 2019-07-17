﻿using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BossAssist
{
	[Label("User Interface Configuration")]
	public class ClientConfiguration : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		private int DespawnState;

		[Header("[i:149] [c/ffeb6e:Boss Log UI]")]

		[DefaultValue(false)]
		[Label("Reset Records Option")]
		[Tooltip("Allows you to reset you records for bosses by right-clicking the boss's icon")]
		public bool ResetRecordsBool { get; set; }

		[DefaultValue(true)]
		[Label("Check Next Boss")]
		[Tooltip("Puts a circle in the checkbox to indicate it is the next undefeated boss to fight")]
		public bool DrawNextMark { get; set; }

		[DefaultValue(true)]
		[Label("Colored Boss Text")]
		[Tooltip("The boss text in the table of contents will be green when defeated and red when not.\nIf next check is enabled, the next boss will be yellow.")]
		public bool ColoredBossText { get; set; }

		// Somehow auto default this to false if checkmark type is strikethrough?
		
		[DrawTicks]
		[Label("Checklist Markings Type")]
		[OptionStrings(new string[] { "✓ and  ☐", "✓ and  X", "X and  ☐", "Strike-through" })]
		[DefaultValue("✓ and  ☐")]
		public string SelectedCheckmarkType { get; set; }

		[Header("[i:3037] [c/ffeb6e:Despawn Messages]")]

		[DefaultValue(true)]
		[Label("Pillar Defeated Messages")]
		[Tooltip("The Lunar Pillars will send defeated messages in chat")]
		public bool PillarMessages { get; set; }

		[DefaultValue(false)]
		[Label("Segment Defeated Messages")]
		[Tooltip("Multi-segmented bosses will send messages when a segment is defeated")]
		public bool LimbMessages { get; set; }

		[DefaultValue(true)]
		[Label("Boss Victory Messages: Custom")]
		public bool CDespawnBool
		{
			get { return DespawnState == 0; }
			set { if (value) DespawnState = 0; }
		}

		[DefaultValue(false)]
		[Label("Boss Victory Messages: Generic")]
		public bool GDespawnBool
		{
			get { return DespawnState == 1; }
			set { if (value) DespawnState = 1; }
		}

		[DefaultValue(false)]
		[Label("Boss Victory Messages: None")]
		public bool ODespawnBool
		{
			get { return DespawnState == 2; }
			set { if (value) DespawnState = 2; }
		}

		[Header("[i:893] [c/ffeb6e:Item Map Detection]")]

		[DefaultValue(true)]
		[Label("Fragments display on map")]
		public bool FragmentsBool { get; set; }

		[DefaultValue(false)]
		[Label("Shadow Scales and Tissue Samples display on map")]
		public bool ScalesBool { get; set; }

		[Header("[i:3084] [c/ffeb6e:Boss Radar]")]

		[DefaultValue(true)]
		[Label("Boss Radar")]
		[Tooltip("If disabled, the configs below have no effect")]
		public bool BossRadarBool { get; set; }

		[DefaultValue(0.85f)]
		[Label("Radar Opacity -- Currently Unavailable")]
		[Range(0.25f, 1f)]
		public float OpacityFloat { get; set; }

		public override void OnLoaded()
		{
			BossAssist.ClientConfig = this;
		}

		public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
		{
			return true;
		}
	}
}