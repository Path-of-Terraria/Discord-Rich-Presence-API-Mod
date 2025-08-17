using System;
using Terraria.ID;

namespace DiscordRPCAPI;

public class ClientWorld : ModSystem
{
	internal static bool NearClouds;
	internal static bool NearDirt;
	
	/// <summary>
	/// Sets <see cref="NearClouds"/> or <see cref="NearDirt"/> to true if enough blocks are in the vicinity.
	/// </summary>
	/// <param name="tileCounts"></param>
	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
	{
		base.TileCountsAvailable(tileCounts);
		NearClouds = (tileCounts[TileID.Cloud] + tileCounts[TileID.RainCloud]) > 40;
		NearDirt = tileCounts[TileID.Dirt] > 20;
	}
}