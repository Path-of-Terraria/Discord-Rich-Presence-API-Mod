using System;
using Terraria.ID;

namespace DiscordRPCAPI;

public class ClientWorld : ModSystem
{
	internal static bool NearClouds;
	internal static bool NearDirt;

	public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
	{
		NearClouds = (tileCounts[TileID.Cloud] + tileCounts[TileID.RainCloud]) > 40;
		NearDirt = tileCounts[TileID.Dirt] > 20;
	}
}