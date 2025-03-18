global using Terraria;
global using Terraria.ModLoader;
global using Microsoft.Xna.Framework;

using System;
using System.Collections.Generic;
using DiscordRPC;
using Terraria.ID;

namespace DiscordRPCAPI;

public class DiscordRPCAPIMod : Mod
{
	public static DiscordRPCAPIMod Instance => ModContent.GetInstance<DiscordRPCAPIMod>();

	internal DiscordRpcClient Client
	{
		get; set;
	}

	internal RichPresence RichPresenceInstance
	{
		get; private set;
	}

	internal static int NowSeconds => (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	internal static ClientPlayer ModPlayer => Main.LocalPlayer.GetModPlayer<ClientPlayer>();
	internal static ClientConfig Config => ModContent.GetInstance<ClientConfig>();

	public string CurrentClient = "default";

	internal int PrevSend = 0;
	internal bool InWorld = false;
	internal bool CanCreateClient;
	internal string WorldStaticInfo = null;
	internal Dictionary<string, string> SavedDiscordAppId;
	internal Timestamps TimeStamp = null;

	private Dictionary<int, Boss> presentableBosses = [];
	private ClientStatus customStatus = null;
	private List<Biome> presentableBiomes = [];

	public void AddBoss(int bossId, Boss boss)
	{
		presentableBosses.Add(bossId, boss);
	}

	private bool BossExists(int bossId)
	{
		return presentableBosses.ContainsKey(bossId);
	}

	private Boss GetBossById(int bossId)
	{
		return presentableBosses[bossId];
	}

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

	public void AddBiome(Biome biome)
	{
		presentableBiomes.Add(biome);
	}

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

		return state.Trim();
	}

	public void SetCustomStatus(ClientStatus status)
	{
		customStatus = status;
	}

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
	/// Change the Discord App ID, currently takes 3s to change
	/// </summary>
	/// <param name="newClient">New Discord App ID key</param>
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
	/// Create new DiscordRP client, currently only used once
	/// </summary>
	/// <param name="appId">Discord App ID</param>
	/// <param name="key">key for App ID</param>
	internal void CreateNewDiscordRPCRichPresenceInstance(string key = "default")
	{
		// https://github.com/PurplefinNeptuna
		// string discordAppId = "404654478072086529";

		// The images tied to this app were copied from the
		// original author's Discord app - I made a new app
		// purely so I could continue to add images to the
		// project based off the content added in 1.4.
		// Social anxiety really sucks.
		// https://github.com/staticfox
		string discordAppId = "792583749040209960";

		// This should never change
		const string steamAppID = "1281930";

		SavedDiscordAppId.TryAdd(key, discordAppId);
		Client = new DiscordRpcClient(applicationID: discordAppId, autoEvents: false);

		bool failedToRegisterScheme = false;

		try
		{
			Client.RegisterUriScheme(steamAppID);
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
			LargeImageKey = "payload_test",
			LargeImageText = "tModLoader",
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
		RichPresenceInstance.Assets = RichPresenceInstance.Assets ?? new Assets();
		RichPresenceInstance.State = status.State;
		RichPresenceInstance.Details = status.Description;

		if (status.LargeImageKey == null)
		{
			RichPresenceInstance.Assets.LargeImageKey = null;
			RichPresenceInstance.Assets.LargeImageText = null;
		}
		else
		{
			RichPresenceInstance.Assets.LargeImageKey = status.LargeImageKey;
			RichPresenceInstance.Assets.LargeImageText = status.LargeImageText;
		}

		if (status.SmallImageKey == null)
		{
			RichPresenceInstance.Assets.SmallImageKey = null;
			RichPresenceInstance.Assets.SmallImageText = null;
		}
		else
		{
			RichPresenceInstance.Assets.SmallImageKey = status.SmallImageKey;
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
	///	run this everytick to update
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

	public override void Unload()
	{
		Main.OnTickForThirdPartySoftwareOnly -= ClientUpdate;
		Client?.Dispose();
	}

	public override object Call(params object[] args)
	{
		if (!Main.dedServ)
		{
			return ModCalls.Call(args);
		}

		return "Can't call on server";
	}

	/// <summary>
	/// Former description: Update the party info.<br/>
	/// At the moment, only called a method named ClientSetParty, which invariably set two values to null.<br/>
	/// Unsure of usage, will stay in case it's useful for future functionality.
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
	/// Get the player's item stat
	/// </summary>
	/// <returns>key and text for small images</returns>
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