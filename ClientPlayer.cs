using System.Linq;
using Terraria;

namespace DiscordRPCAPI;

public class ClientPlayer : ModPlayer
{
	internal int NearbyTownNPCCount = 0;

	public override void OnEnterWorld()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			DiscordRPCAPIMod.Instance.InWorld = true;

			DiscordRPCAPIMod.Instance.UpdateWorldStaticInfo();
			DiscordRPCAPIMod.Instance.ClientUpdatePlayer();
		}

		DiscordRPCAPIMod.Instance.UpdateLobbyInfo();
		DiscordRPCAPIMod.Instance.ClientForceUpdate();
	}

	public override void PlayerConnect()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			DiscordRPCAPIMod.Instance.InWorld = true;
		}

		DiscordRPCAPIMod.Instance.UpdateLobbyInfo();
		DiscordRPCAPIMod.Instance.ClientForceUpdate();
	}

	public override void PlayerDisconnect()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			DiscordRPCAPIMod.Instance.InWorld = false;
		}

		DiscordRPCAPIMod.Instance.UpdateLobbyInfo();
		DiscordRPCAPIMod.Instance.ClientForceUpdate();
	}

	public override void PreUpdate()
	{
		NearbyTownNPCCount = Main.npc.Count(npc => npc.active && npc.townNPC && npc.position.DistanceSQ(Player.Center) <= 1500 * 1500);
	}

	public static Tile CurrentTile()
	{
		Player player = Main.LocalPlayer;
		return Main.tile[player.Center.ToTileCoordinates()];
	}
}