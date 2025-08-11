using log4net;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.Localization;

namespace DiscordRPAPI;

/// <summary>
/// Adds all relevant Data for the mod
/// </summary>
internal class DataPopulator
{
	private static ILog Logger => DiscordRPCAPIMod.Instance.Logger;
	private static Player LocalPlayer => Main.LocalPlayer;
	private static ClientPlayer LocalClient => Main.LocalPlayer?.GetModPlayer<ClientPlayer>();

	/// <summary>
	/// Add all Vanilla Events to the Biome List
	/// </summary>
	public static void AddVanillaEvents()
	{
		AddBiome(
			() => LocalClient?.NearbyTownNPCCount >= 3 && BirthdayParty.PartyIsUp,
			"event_party", "Party", 90f
		);
		AddBiome(
			() => Sandstorm.Happening && LocalPlayer.ZoneDesert,
			"event_sandstorm", "Sandstorm", 91f
		);
		AddBiome(
			() => Main.slimeRain && LocalPlayer.ZoneOverworldHeight,
			"event_slime", "Slime Rain", 91f
		);
		AddBiome(
			() => DD2Event.Ongoing && LocalPlayer.ZoneOverworldHeight,
			"event_oldone", "Old One's Army", 95f
		);
		AddBiome(
			() => Main.invasionType == 1 && Main.invasionSize > 0 && LocalPlayer.ZoneOverworldHeight,
			"event_goblin", "Goblin Invasion", 95f
		);
		AddBiome(
			() => Main.bloodMoon && LocalPlayer.ZoneOverworldHeight && !Main.dayTime,
			"event_bloodmoon", "Blood Moon", 96f
		);
		AddBiome(
			() => Main.invasionType == 2 && Main.invasionSize > 0 && LocalPlayer.ZoneOverworldHeight,
			"event_frostlegion", "Frost Legion", 100f
		);
		AddBiome(
			() => Main.invasionType == 3 && Main.invasionSize > 0 && LocalPlayer.ZoneOverworldHeight,
			"event_pirate", "Pirate Invasion", 110f
		);
		AddBiome(
			() => Main.pumpkinMoon && LocalPlayer.ZoneOverworldHeight && !Main.dayTime,
			"event_pumpkinmoon", "Pumpkin Moon", 115f
		);
		AddBiome(
			() => Main.snowMoon && LocalPlayer.ZoneOverworldHeight && !Main.dayTime,
			"event_frostmoon", "Frost Moon", 115f
		);
		AddBiome(
			() => Main.eclipse && LocalPlayer.ZoneOverworldHeight && Main.dayTime,
			"event_eclipse", "Solar Eclipse", 115f
		);
		AddBiome(
			() => Main.invasionType == 4 && Main.invasionSize > 0 && LocalPlayer.ZoneOverworldHeight,
			"event_martian", "Martian Madness", 120f
		);
		AddBiome(
			() => LocalPlayer.ZoneTowerSolar,
			"event_solarmoon", "Solar Pillar area", 130f
		);
		AddBiome(
			() => LocalPlayer.ZoneTowerVortex,
			"event_vortexmoon", "Vortex Pillar area", 130f
		);
		AddBiome(
			() => LocalPlayer.ZoneTowerNebula,
			"event_nebulamoon", "Nebula Pillar area", 130f
		);
		AddBiome(
			() => LocalPlayer.ZoneTowerStardust,
			"event_stardustmoon", "Stardust Pillar area", 130f
		);
	}
	
	public static void AddVanillaBiomes()
	{
		AddBiome(
			() => true,
			"biome_forest", "Forest", 0f
		);
		AddBiome(
			() => LocalPlayer.ZoneDirtLayerHeight,
			"biome_underground", "Underground", 1f
		);
		AddBiome(
			() => LocalPlayer.ZoneRockLayerHeight,
			"biome_cavern", "Cavern", 2f
		);
		AddBiome(
			() => LocalPlayer.ZoneBeach,
			"biome_ocean", "Ocean", 3f
		);
		AddBiome(
			() => LocalPlayer.ZoneGlowshroom,
			"biome_mushroom", "Mushroom", 4f
		);
		AddBiome(
			() => LocalPlayer.ZoneJungle && LocalPlayer.ZoneOverworldHeight,
			"biome_jungle", "Jungle", 5f
		);
		AddBiome(
			() => LocalPlayer.ZoneJungle && !LocalPlayer.ZoneOverworldHeight,
			"biome_ujungle", "Underground Jungle", 6f
		);
		// The music in the underground jungle gets kinda epic when you
		// get near the cavern layer, so I thought it would be neat to
		// give it a darker image when that happens. It feels like a
		// transition to a new zone, even though it technically isn't.
		AddBiome(
			() => LocalPlayer.ZoneJungle && !LocalPlayer.ZoneOverworldHeight && Main.curMusic == 54,
			"biome_cjungle", "Underground Jungle", 6.5f
		);
		AddBiome(
			() => LocalPlayer.ZoneDesert,
			"biome_desert", "Desert", 7f
		);
		AddBiome(
			() => LocalPlayer.ZoneUndergroundDesert,
			"biome_udesert", "Underground Desert", 8f
		);
		AddBiome(
			() => LocalPlayer.ZoneSnow,
			"biome_snow", "Snow", 9f
		);
		AddBiome(
			() => LocalPlayer.ZoneSnow && !LocalPlayer.ZoneOverworldHeight,
			"biome_usnow", "Underground Snow", 10f
		);
		AddBiome(
			() => LocalPlayer.ZoneHallow,
			"biome_holy", "Hollow", 11f
		);
		AddBiome(
			() => LocalPlayer.ZoneHallow && !LocalPlayer.ZoneOverworldHeight,
			"biome_uholy", "Underground Hollow", 12f
		);
		AddBiome(
			() => LocalPlayer.ZoneCorrupt,
			"biome_corrupt", "Corruption", 13f
		);
		AddBiome(
			() => LocalPlayer.ZoneCorrupt && !LocalPlayer.ZoneOverworldHeight,
			"biome_ucorrupt", "Underground Corruption", 14f
		);
		AddBiome(
			() => LocalPlayer.ZoneCrimson,
			"biome_crimson", "Crimson", 15f
		);
		AddBiome(
			() => LocalPlayer.ZoneCrimson && !LocalPlayer.ZoneOverworldHeight,
			"biome_ucrimson", "Underground Crimson", 16f
		);
		AddBiome(
			() => ClientPlayer.CurrentTile().WallType == WallID.SpiderUnsafe,
			"biome_spider", "Spider Cave", 17f
		);
		AddBiome(
			() => LocalPlayer.ZoneGlowshroom && !LocalPlayer.ZoneOverworldHeight,
			"biome_umushroom", "Underground Mushroom", 18f
		);
		AddBiome(
			() => LocalPlayer.ZoneGranite,
			"biome_granite", "Granite Cave", 19f
		);
		AddBiome(
			() => LocalPlayer.ZoneMarble,
			"biome_marble", "Marble Cave", 20f
		);
		AddBiome(
			() => LocalPlayer.ZoneHive,
			"biome_beehive", "Bee Hive", 21f
		);
		AddBiome(
			() => LocalPlayer.ZoneDungeon,
			"biome_dungeon", "Dungeon", 22f
		);
		AddBiome(
			() => ClientPlayer.CurrentTile().WallType == WallID.LihzahrdBrickUnsafe,
			"biome_temple", "Jungle Temple", 23f
		);
		AddBiome(
			() => LocalPlayer.ZoneSkyHeight,
			"biome_sky", "Space", 24f
		);
		AddBiome(
			() => ClientWorld.NearClouds,
			"biome_skylake", "Floating Lake", 25f
		);
		AddBiome(
			() => ClientWorld.NearClouds && ClientWorld.NearDirt,
			"biome_skyisland", "Floating Island", 26f
		);
		AddBiome(
			() => LocalPlayer.ZoneUnderworldHeight,
			"biome_hell", "Underworld", 27f
		);
		AddBiome(
			() => LocalPlayer.ZoneMeteor,
			"biome_meteor", "Meteor", 28f
		);
		AddBiome(
			() => LocalClient?.NearbyTownNPCCount >= 3,
			"biome_town", "Town", 29f
		);
	}

	public static void AddVanillaBosses()
	{
		AddBoss([NPCID.KingSlime], ("boss_kingslime", "King Slime"), 1f);
		AddBoss([NPCID.EyeofCthulhu], ("boss_eoc", "Eye of Cthulhu"), 2f);
		AddBoss([
			NPCID.EaterofWorldsHead,
			NPCID.EaterofWorldsBody,
			NPCID.EaterofWorldsTail
		], ("boss_eow", "Eater of Worlds"), 3f);
		AddBoss([NPCID.BrainofCthulhu], ("boss_boc", "Brain of Cthulhu"), 4f);
		AddBoss([NPCID.QueenBee], ("boss_queenbee", "Queen Bee"), 5f);
		AddBoss([NPCID.SkeletronHead], ("boss_skeletron", "Skeletron"), 6f);
		AddBoss([NPCID.WallofFlesh], ("boss_wof", "Wall of Flesh"), 7f);

		//hardmode
		AddBoss([
			NPCID.Retinazer,
			NPCID.Spazmatism
		], ("boss_twins", "The Twins"), 8f);
		AddBoss([NPCID.TheDestroyer], ("boss_destroyer", "The Destroyer"), 9f);
		AddBoss([NPCID.SkeletronPrime], ("boss_prime", "Skeletron Prime"), 10f);
		AddBoss([NPCID.Plantera], ("boss_plantera", "Plantera"), 11f);
		AddBoss([NPCID.Golem], ("boss_golem", "Golem"), 12f);
		AddBoss([NPCID.DukeFishron], ("boss_fishron", "Duke Fishron"), 13f);
		AddBoss([NPCID.CultistBoss], ("boss_lunatic", "Lunatic Cultist"), 14f);
		AddBoss([
			NPCID.MoonLordHead,
			NPCID.MoonLordHand,
			NPCID.MoonLordCore
		], ("boss_moonlord", "Moon Lord"), 15f);
		// 1.4.0.1 bosses
		AddBoss([NPCID.QueenSlimeBoss], ("boss_queenslime", "Queen Slime"), 16f);
		AddBoss([NPCID.HallowBoss], ("boss_empress_of_light", "Empress Of Light"), 17f);
	}

	/// <summary>
	/// Add new npc to boss list to detect
	/// </summary>
	/// <param name="ids">the npc ids</param>
	/// <param name="imageKey">image key</param>
	/// <param name="priority">priority</param>
	/// <param name="client">discord app id key</param>
	public static void AddBoss(List<int> ids, (string, string) imageKey, float priority = 16f, string client = "default")
	{
		if (ids == null)
		{
			return;
		}

		if (!DiscordRPCAPIMod.Instance.SavedDiscordAppId.ContainsKey(client))
		{
			Logger.Error($"Instance {client} not found, redirected to default Instance!");
			client = "default";
		}

		foreach (int id in ids)
		{
			if (string.IsNullOrWhiteSpace(imageKey.Item1))
			{
				imageKey.Item1 = "boss_placeholder";
			}

			var boss = new Boss(imageKey.Item1, imageKey.Item2, priority, client);

			DiscordRPCAPIMod.Instance.AddBoss(id, boss);
		}
	}

	/// <summary>
	/// Add new biome/event to detect
	/// </summary>
	/// <param name="checker">function to check, detect biome/event if returns true</param>
	/// <param name="imageKey">image key</param>
	/// <param name="priority">priority</param>
	/// <param name="client">discord app id key</param>
	public static void AddBiome(Func<bool> checker, string texturePath, string langKey, float priority = 50f, string client = "default")
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			texturePath = "biome_placeholder";
		}

		//Logger.Info($"Adding biome {imageKey.Item2} in {client} Instance...");
		if (!DiscordRPCAPIMod.Instance.SavedDiscordAppId.ContainsKey(client))
		{
			Logger.Error($"Instance {client} not found, redirected to default Instance!");
			client = "default";
		}

		var biome = new Biome(checker, texturePath, Language.GetText(langKey), priority, client);
		DiscordRPCAPIMod.Instance.AddBiome(biome);
	}
}
