using System.Linq;
using Terraria;

namespace DiscordRPCAPI;

/// <summary>
/// A custom player class for handling client-side logic in the discord RPC API mod.
/// This class extends <see cref="ModPlayer"/> to manage discord presence updates based on player events and state.
/// </summary>
public class ClientPlayer : ModPlayer
{
	/// <summary>
	/// The number of nearby town NPCs within a 1500-unit squared distance from the player.
	/// This value is updated in the <see cref="PreUpdate"/> method.
	/// </summary>
	internal int NearbyTownNPCCount = 0;
	
	/// <summary>
	/// Called when the player enters a world.
	/// Updates the mod's state to indicate the player is in a world, refreshes world and player information for Discord RPC,
	/// and forces a lobby info update and client refresh.
	/// </summary>
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
	/// <summary>
	/// Called when the player connects to a multiplayer session.
	/// Sets the mod's state to indicate the player is in a world if it's the local player,
	/// updates lobby information, and forces a client refresh for Discord RPC.
	/// </summary>
	public override void PlayerConnect()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			DiscordRPCAPIMod.Instance.InWorld = true;
		}

		DiscordRPCAPIMod.Instance.UpdateLobbyInfo();
		DiscordRPCAPIMod.Instance.ClientForceUpdate();
	}
	/// <summary>
	/// Called when the player disconnects from a multiplayer session.
	/// Resets the mod's state to indicate the player is no longer in a world if it's the local player,
	/// updates lobby information, and forces a client refresh for Discord RPC.
	/// </summary>
	public override void PlayerDisconnect()
	{
		if (Player.whoAmI == Main.myPlayer)
		{
			DiscordRPCAPIMod.Instance.InWorld = false;
		}

		DiscordRPCAPIMod.Instance.UpdateLobbyInfo();
		DiscordRPCAPIMod.Instance.ClientForceUpdate();
	}
	/// <summary>
	/// Called before the player's update logic each frame.
	/// Calculates and updates the count of nearby active town NPCs within a 1500-unit squared distance.
	/// </summary>
	public override void PreUpdate()
	{
		NearbyTownNPCCount = Main.npc.Count(npc => npc.active && npc.townNPC && npc.position.DistanceSQ(Player.Center) <= 1500 * 1500);
	}
	/// <summary>
	/// Retrieves the tile at the center position of the local player.
	/// </summary>
	/// <returns>The <see cref="Tile"/> object at the local player's center coordinates.</returns>
	public static Tile CurrentTile()
	{
		Player player = Main.LocalPlayer;
		return Main.tile[player.Center.ToTileCoordinates()];
	}
}