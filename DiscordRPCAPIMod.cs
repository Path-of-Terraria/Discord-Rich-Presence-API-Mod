global using Terraria;
global using Terraria.ModLoader;
global using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using DiscordRPC;
using Terraria.ID;

namespace DiscordRPCAPI;

/// <summary>
/// The main mod class for the discord RPC API mod, responsible for managing discord rich presence integration with tModLoader.
/// </summary>
public class DiscordRPCAPIMod : Mod
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="DiscordRPCAPIMod"/> class.
	/// </summary>
	public static DiscordRPCAPIMod Instance => ModContent.GetInstance<DiscordRPCAPIMod>();

	/// <summary>
	/// Gets or sets the discord RPC client used for communication with Discord.
	/// </summary>
	internal DiscordRpcClient Client
	{
		get; set;
	}

	/// <summary>
	/// Gets the current discord rich presence instance.
	/// </summary>
	internal RichPresence RichPresenceInstance
	{
		get; private set;
	}

	/// <summary>
	/// Gets the current time in seconds since the Unix epoch (January 1, 1970).
	/// </summary>
	internal static int NowSeconds => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	
	/// <summary>
	/// Gets the <see cref="ClientPlayer"/> instance associated with the local player.
	/// </summary>
	internal static ClientPlayer ModPlayer => Main.LocalPlayer.GetModPlayer<ClientPlayer>();
	internal static ClientConfig Config => ModContent.GetInstance<ClientConfig>();

	/// <summary>
	/// Gets or sets the current discord client key (default is "default").
	/// </summary>
	public string CurrentClient = "default";

	// <summary>
	/// Gets or sets the timestamp of the last discord presence update.
	/// </summary>
	internal int PrevSend = 0;
	internal bool InWorld = false;
	internal bool CanCreateClient;
	
	/// <summary>
	/// Gets or sets the static world information displayed in the discord status.
	/// </summary>
	internal string WorldStaticInfo = null;
	internal Dictionary<string, string> SavedDiscordAppId;
	internal Timestamps TimeStamp = null;

	private Dictionary<int, Boss> presentableBosses = [];
	private ClientStatus customStatus = null;
	private List<Biome> presentableBiomes = [];

	private static Dictionary<string, Func<string>> customStats = [];

	/// <summary>
	/// Adds a boss to the collection of presentable bosses for discord rich presence.
	/// </summary>
	/// <param name="bossId">The ID of the boss.</param>
	/// <param name="boss">The <see cref="Boss"/> object to add.</param>
	public void AddBoss(int bossId, Boss boss)
	{
		presentableBosses.Add(bossId, boss);
	}

	/// <summary>
	/// Checks if a boss exists in the presentable bosses collection.
	/// </summary>
	/// <param name="bossId">The ID of the boss to check.</param>
	/// <returns><c>true</c> if the boss exists; otherwise, <c>false</c>.</returns>
	private bool BossExists(int bossId)
	{
		return presentableBosses.ContainsKey(bossId);
	}

	/// <summary>
	/// Retrieves a boss by its ID from the presentable bosses collection.
	/// </summary>
	/// <param name="bossId">The ID of the boss to retrieve.</param>
	/// <returns>The <see cref="Boss"/> object associated with the ID.</returns>
	private Boss GetBossById(int bossId)
	{
		return presentableBosses[bossId];
	}

	/// <summary>
	/// Gets the highest-priority active boss currently in the world.
	/// </summary>
	/// <returns>The <see cref="Boss"/> with the highest priority, or <c>null</c> if no boss is active.</returns>
	private Boss? GetCurrentBoss()
	{
		Boss? currentBoss = null;

		foreach (NPC npc in Main.npc)
		{
			if (!npc.active || !BossExists(npc.type))
			{
				continue;
			}

			Boss boss = GetBossById(npc.type);

			if (currentBoss != null && currentBoss.Value.Priority > boss.Priority)
			{
				continue;
			}

			currentBoss = boss;
		}

		return currentBoss;
	}

	/// <summary>
	/// Adds a biome to the collection of presentable biomes for discord rich presence.
	/// </summary>
	/// <param name="biome">The <see cref="Biome"/> object to add.</param>
	public void AddBiome(Biome biome)
	{
		presentableBiomes.Add(biome);
	}

	/// <summary>
	/// Gets the highest-priority active biome the player is currently in.
	/// </summary>
	/// <returns>The <see cref="Biome"/> with the highest priority, or <c>null</c> if no biome is active.</returns>
	private Biome? GetCurrentBiome()
	{
		Biome? currentBiome = null;

		foreach (Biome biome in presentableBiomes)
		{
			if (!biome.IsActive || currentBiome != null && currentBiome.Value.Priority > biome.Priority)
			{
				continue;
			}

			currentBiome = biome;
		}

		return currentBiome;
	}

	/// <summary>
	/// Adds a custom stat to the list <see cref="customStats"/>
	/// </summary>
	/// <param name="stat"></param>
	/// <param name="statValue"></param>
	public void AddCustomStat(string stat, Func<string> statValue)
	{
		customStats.Add(stat,statValue);
	}

	/// <summary>
	/// Determines the current time of day in the game world for discord status display.
	/// </summary>
	/// <returns>A string indicating the time of day ("Dawn", "Day", "Dusk", or "Night"), or <c>null</c> if time display is disabled.</returns>
	private static string GetTimeOfDay()
	{
		if (!Config.ShowTimeCycle)
		{
			return null;
		}

		// Notes on the time system in Terraria - 'time'
		// is referred to as the amount of seconds that
		// have passed since the day transitioned to night
		// or vice-versa. The key is to check Main.dayTime
		// to determine whether 0.0 is 4:30 AM (day) or
		// 7:30 PM (night).
		//
		// 1 hour   = 3600
		// 1 minute = 60
		// 1 second = 1
		//
		// 15 hours of day
		//
		// Day start = 4:30 AM
		// dayLength = 54000.0
		//
		// 9 hours of night
		//
		// Night start = 7:30 PM
		// nightLength = 32400.0
		if (Main.dayTime)
		{
			// We'll consider 6:00 AM as the time when
			// day "officially" starts and 6:00 PM as
			// the time when day winds down. Partially
			// going off of IRL parallels and partially
			// because peak fishing ends at
			// 6:00 AM and the merchant will always
			// leave at 6pm.
			if (Main.time < 7200.0)
			{
				return "Dawn";
			}
			else if (Main.time >= 46800.0)
			{
				return "Dusk";
			}
			else
			{
				return "Day";
			}
		}
		else
		{
			// The vast majority of checks are in the
			// day time since it's completely dark in
			// Terraria for the entire duration of the
			// night.
			return "Night";
		}
	}

	/// <summary>
	/// Retrieves the player's current state (e.g., health, DPS, mana, defense) for Discord status.
	/// </summary>
	/// <returns>A string summarizing the player's stats, or <c>null</c> if stat display is disabled.</returns>
	private static string GetPlayerState()
	{
		if (!Config.ShowPlayerStats())
		{
			return null;
		}

		if (ModPlayer.Player.dead)
		{
			return "Dead";
		}

		string state = "";

		if (Config.ShowHealth)
		{
			state += $"HP: {Main.LocalPlayer.statLife} ";
		}

		if (Config.ShowDPS)
		{
			state += $"DPS: {Main.LocalPlayer.getDPS()} ";
		}

		if (Config.ShowMana)
		{
			state += $"MP: {Main.LocalPlayer.statMana} ";
		}

		if (Config.ShowDefense)
		{
			state += $"DEF: {Main.LocalPlayer.statDefense} ";
		}

		if (!Config.ShowCustomStat)
		{
			return state.Trim();
		}

		if (customStats.Count > 0)
		{
			state = customStats.Aggregate(state, (current, stat) => current + (stat.Key + ": " + stat.Value()));
		}

		return state.Trim();
	}

	/// <summary>
	/// Sets a custom discord rich presence status.
	/// </summary>
	/// <param name="status">The <see cref="ClientStatus"/> object containing the custom status details.</param>
	public void SetCustomStatus(ClientStatus status)
	{
		customStatus = status;
	}

	/// <summary>
	/// Initializes the mod, setting up the discord rich presence client and related data structures.
	/// </summary>
	public override void Load()
	{
		if (!Main.dedServ)
		{
			CurrentClient = "default";
			CanCreateClient = true;

			presentableBiomes = [];
			presentableBosses = [];
			SavedDiscordAppId = [];

			RichPresenceInstance = new RichPresence
			{
				Secrets = new Secrets()
			};

			TimeStamp = Timestamps.Now;

			CreateNewDiscordRPCRichPresenceInstance();
		}
	}

	/// <summary>
	/// Sets up vanilla bosses, biomes, and events, and registers the client update tick handler.
	/// </summary>
	public override void PostSetupContent()
	{
		if (!Main.dedServ)
		{
			DataPopulator.AddVanillaBosses();
			DataPopulator.AddVanillaBiomes();
			DataPopulator.AddVanillaEvents();

			Main.OnTickForThirdPartySoftwareOnly += ClientUpdate;
			//finished
			CanCreateClient = false;
			ClientOnMainMenu();
		}
	}

	/// <summary>
	/// Change the discord app ID, currently takes 3s to change
	/// </summary>
	/// <param name="newClient">New discord app ID key</param>
	public void ChangeDiscordClient(string newClient)
	{
		if (newClient == CurrentClient)
		{
			return;
		}

		if (!SavedDiscordAppId.ContainsKey(newClient))
		{
			return;
		}

		CurrentClient = newClient;
	}

	/// <summary>
	/// Create new discordRPC client, currently only used once
	/// </summary>
	/// <param name="appId">Discord app ID</param>
	/// <param name="key">key for app ID</param>
	internal void CreateNewDiscordRPCRichPresenceInstance(string key = "default")
	{
		const string TerrariaAppId = "1281930";
		const string DiscordAppId = "1351686373786255431";

		SavedDiscordAppId.TryAdd(key, DiscordAppId);
		Client = new DiscordRpcClient(applicationID: DiscordAppId, autoEvents: false);

		bool failedToRegisterScheme = false;

		try
		{
			Client.RegisterUriScheme(TerrariaAppId);
		}
		catch (Exception)
		{
			failedToRegisterScheme = true;
		}

		if (!failedToRegisterScheme)
		{
			Client.OnJoinRequested += ClientOnJoinRequested;
			Client.OnJoin += ClientOnJoin;
		}

		if (Config.Enable)
		{
			Client.Initialize();
		}
	}

	/// <summary>
	/// Add other Discord App ID
	/// </summary>
	/// <param name="key">the key</param>
	/// <param name="appID">Discord App ID</param>
	public void AddDiscordAppID(string key, string appID)
	{
		SavedDiscordAppId.TryAdd(key, appID);
	}

	/// <summary>
	/// Discord OnJoin event, called on the joiner
	/// </summary>
	private void ClientOnJoin(object sender, DiscordRPC.Message.JoinMessage args)
	{
		//this is empty lol
		//SocialAPI.Network.Connect(new SteamAddress(new CSteamID(Convert.ToUInt64(args.Secret))));
	}

	/// <summary>
	/// Discord OnJoinRequested event, called on the host, currently deny everything lol
	/// </summary>
	private void ClientOnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args)
	{
		Client.Respond(args, false);
	}

	/// <summary>
	/// Change the status to main menu
	/// </summary>
	public void ClientOnMainMenu()
	{
		ChangeDiscordClient("default");

		ClientStatus status = customStatus ?? new ClientStatus()
		{
			Description = "In Main Menu",
			LargeImageKey = "forest",
			LargeImageText = "tModLoader",
			SmallImageKey = "forest",
			SmallImageText = "tModLoader"
		};

		ClientSetStatus(status);
		ClientForceUpdate();
	}

	/// <summary>
	/// Changes the status for the current user.
	/// </summary>
	/// <param name="status">An instance of <see cref="ClientStatus"/>.</param>
	public void ClientSetStatus(ClientStatus status)
	{
		RichPresenceInstance.Assets ??= new Assets();
		RichPresenceInstance.State = status.State;
		RichPresenceInstance.Details = status.Description;

		if (status.LargeImageKey == null)
		{
			RichPresenceInstance.Assets.LargeImageKey = null;
			RichPresenceInstance.Assets.LargeImageText = null;
		}
		else
		{
			RichPresenceInstance.Assets.LargeImageKey = "forest"; //status.LargeImageKey;
			RichPresenceInstance.Assets.LargeImageText = status.LargeImageText;
		}

		if (status.SmallImageKey == null)
		{
			RichPresenceInstance.Assets.SmallImageKey = null;
			RichPresenceInstance.Assets.SmallImageText = null;
		}
		else
		{
			RichPresenceInstance.Assets.SmallImageKey = "forest"; //status.SmallImageKey;
			RichPresenceInstance.Assets.SmallImageText = status.SmallImageText;
		}
	}

	///// <summary>
	///// set the party settings
	///// </summary>
	///// <param name="secret">party secret</param>
	///// <param name="id">party id</param>
	///// <param name="partysize">party current size</param>
	//public void ClientSetParty(string secret = null, string id = null, int partysize = 0)
	//{
	//	if (partysize == 0 || id == null)
	//	{
	//		RichPresenceInstance.Secrets.JoinSecret = null;
	//		RichPresenceInstance.Party = null;
	//	}
	//	else
	//	{
	//		//RichPresenceInstance.Secrets.JoinSecret = secret;
	//		//RichPresenceInstance.Party = RichPresenceInstance.Party ?? new Party();
	//		//RichPresenceInstance.Party.Size = partysize;
	//		//RichPresenceInstance.Party.Max = 256;
	//		//RichPresenceInstance.Party.ID = id;
	//		RichPresenceInstance.Secrets.JoinSecret = null;
	//		RichPresenceInstance.Party = null;
	//	}
	//}

	/// <summary>
	/// Forces a rich presence update.
	/// </summary>
	public void ClientForceUpdate()
	{
		if (Client != null && !Client.IsDisposed)
		{
			if (!Client.IsInitialized && Config.Enable)
			{
				Client.Initialize();
			}

			RichPresenceInstance.Timestamps = Config.ShowTime ? TimeStamp : null;
			Client.SetPresence(RichPresenceInstance);
			Client.Invoke();
		}
	}

	/// <summary>
	/// Updates the discord rich presence status every tick, based on the game state.
	/// </summary>
	public void ClientUpdate()
	{
		if (!Main.gameMenu && !Main.dedServ)
		{
			if (Main.gamePaused || Main.gameInactive || !InWorld)
			{
				return;
			}

			int now = NowSeconds;

			if (now != PrevSend && ((now - PrevSend) % Config.Timer) == 0)
			{
				ClientUpdatePlayer();
				ClientForceUpdate();

				PrevSend = now;
			}
		}
	}

	/// <summary>
	/// Cleans up resources when the mod is unloaded.
	/// </summary>
	public override void Unload()
	{
		Main.OnTickForThirdPartySoftwareOnly -= ClientUpdate;
		Client?.Dispose();
	}

	/// <summary>
	/// Handles mod calls from other mods to configure discord rich presence settings.
	/// </summary>
	/// <param name="args">The arguments for the mod call.</param>
	/// <returns>The result of the mod call, or an error message if called on a dedicated server.</returns>
	public override object Call(params object[] args)
	{
		if (!Main.dedServ)
		{
			return ModCalls.Call(args);
		}

		return "Can't call on server";
	}

	/// <summary>
	/// Updates the discord rich presence lobby information, currently setting party settings to null.
	/// </summary>
	internal void UpdateLobbyInfo()
	{
		if (Main.LobbyId != 0UL)
		{
			RichPresenceInstance.Secrets.JoinSecret = null;
			RichPresenceInstance.Party = null;

			//string sId = SteamUser.GetSteamID().ToString();
			//ClientSetParty(null, Main.LocalPlayer.name, Main.CurrentFrameFlags.ActivePlayersCount);
		}
	}

	/// <summary>
	/// method for update the status, checking from item to biome/boss/events
	/// </summary>
	internal void ClientUpdatePlayer()
	{
		if (Main.LocalPlayer == null)
		{
			return;
		}

		ClientStatus status = new()
		{
			State = GetPlayerState(),
			Description = null,
			LargeImageText = WorldStaticInfo,
		};

		(string itemKey, string itemText) = GetItemStat();
		status.SmallImageKey = itemKey;
		status.SmallImageText = itemText;

		Boss? checkBoss = GetCurrentBoss();
		Biome? checkBiome = GetCurrentBiome();
		string selectedClient = "default";

		if (checkBoss != null)
		{
			Boss boss = checkBoss.Value;
			status.LargeImageKey = boss.ImageKey;
			status.Description = "Fighting " + boss.ImageName;
			selectedClient = boss.ClientId;
		}
		else if (checkBiome != null)
		{
			Biome biome = checkBiome.Value;
			status.LargeImageKey = biome.ImageKey;
			status.Description = "In " + biome.ImageName;
			selectedClient = biome.ClientId;

			string timeOfDay = GetTimeOfDay();

			if (timeOfDay != null)
			{
				status.Description += $" ({timeOfDay})";
			}
		}

		ClientSetStatus(status);
		UpdateLobbyInfo();
		ChangeDiscordClient(selectedClient);

		if (ModPlayer.Player.dead)
		{
			ClientForceUpdate();
		}
	}

	/// <summary>
	/// Retrieves the player's held item stats for discord rich presence display.
	/// </summary>
	/// <returns>A tuple containing the image key and text description for the player's held item.</returns>
	internal static (string, string) GetItemStat()
	{
		int atk = -1;
		string key = null;
		string text = null;
		string atkType = "";
		Item item = Main.LocalPlayer?.HeldItem;

		List<(DamageClass, string)> DamageClasses = 
		[
			(DamageClass.Melee, "Melee"),
			(DamageClass.Ranged, "Range"),
			(DamageClass.Magic, "Magic"),
			(DamageClass.Throwing, "Throw"),
			(DamageClass.Summon, "Summon"),
		];

		if (item != null)
		{
			text = "";

			if (Config.ShowPrefix && item.prefix != 0 && item.prefix < PrefixID.Count)
			{
				string prefix = PrefixID.Search.GetName(item.prefix);
				text += prefix + " ";
			}

			text += item.Name;

			foreach ((DamageClass damageClass, string damageName) in DamageClasses)
			{
				if (item.DamageType == damageClass)
				{
					atk = (int)Math.Ceiling(Main.LocalPlayer.GetDamage(damageClass).ApplyTo(item.damage));
					atkType = damageName;
					break;
				}
			}
		}

		if (atk >= 0)
		{
			key = atkType;

			if (Config.ShowDamage)
			{
				text += $" ({atk} Damage)";
			}
		}

		return (key, text);
	}

	/// <summary>
	/// Updates the static world information for discord rich presence, including world name and difficulty.
	/// </summary>
	public void UpdateWorldStaticInfo()
	{
		string wName = Main.worldName;
		string wDiff = Main.masterMode ? "(Master)" : Main.expertMode ? "(Expert)" : "(Normal)";

		if (!Config.ShowWorldName)
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
			{
				wName = "Single Player";
			}
			else
			{
				wName = "Multiplayer";
			}
		}

		WorldStaticInfo = $"Playing {wName} {wDiff}";
	}
}