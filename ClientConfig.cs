using System.ComponentModel;
using DiscordRPC;
using Terraria;
using Terraria.ModLoader.Config;

namespace DiscordRPCAPI;

public class ClientConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	private static DiscordRpcClient Client => DiscordRPCAPIMod.Instance?.Client;

	[Header("RichPresenceSettings")]
	[DefaultValue(true)]
	public bool Enable;

	[Range(1u, 120u)]
	[DefaultValue(1u)]
	[Slider]
	public uint Timer;

	[Header("VisibilitySettings")]
	[DefaultValue(true)]
	public bool ShowTime;

	[DefaultValue(true)]
	public bool ShowTimeCycle;

	[DefaultValue(true)]
	public bool ShowWorldName;

	[DefaultValue(true)]
	public bool ShowHealth;

	[DefaultValue(true)]
	public bool ShowDPS;

	[DefaultValue(false)]
	public bool ShowMana;

	[DefaultValue(false)]
	public bool ShowDefense;

	[DefaultValue(false)]
	public bool ShowDamage;

	[DefaultValue(true)]
	public bool ShowPrefix;

	public override void OnLoaded()
	{
		DiscordRPCAPIMod.Instance.Config = this;
	}

	public override void OnChanged()
	{
		if (DiscordRPCAPIMod.Instance == null || DiscordRPCAPIMod.Instance.Client == null)
		{
			return;
		}

		if (Enable)
		{
			if (Client.IsDisposed)
			{
				DiscordRPCAPIMod.Instance.CreateNewDiscordRPCRichPresenceInstance();
			}
			else if (!Client.IsInitialized)
			{
				Client.Initialize();
			}

			string currentClient = DiscordRPCAPIMod.Instance.CurrentClient;
			DiscordRPCAPIMod.Instance.CurrentClient = "default";
			DiscordRPCAPIMod.Instance.ChangeDiscordClient(currentClient);
			if (!Main.gameMenu)
			{
				DiscordRPCAPIMod.Instance.UpdateWorldStaticInfo();
				DiscordRPCAPIMod.Instance.ClientUpdatePlayer();
			}
			DiscordRPCAPIMod.Instance.ClientForceUpdate();
		}
		else if (!Client.IsDisposed)
		{
			Client.Dispose();
		}
	}

	public bool ShowPlayerStats()
	{
		return ShowHealth || ShowMana || ShowDefense || ShowDPS;
	}
}