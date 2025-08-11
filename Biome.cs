using System;
using Terraria.Localization;

namespace DiscordRPAPI; 

/// <summary>
/// Data structure representing a biome, structure, event, or other environmental location.<br/>
/// <see cref="Biome"/>s are always overriden by <see cref="Boss"/>es.
/// </summary>
public readonly struct Biome(Func<bool> checker, string imageKey, LocalizedText imageName, float priority, string clientId)
{
	/// <summary>
	/// If true, this biome is active. Shorthand for calling <see cref="Checker"/>.
	/// </summary>
	public readonly bool IsActive => Checker();

	/// <summary>
	/// Delegate for if this biome is active. Use <see cref="IsActive"/> instead.
	/// </summary>
	internal readonly Func<bool> Checker = checker;

	/// <summary>
	/// Image key for the associated large image key; the biome key art's key.
	/// </summary>
	public readonly string ImageKey = imageKey;

	/// <summary>
	/// Hover text for the image in Discord.
	/// </summary>
	public readonly LocalizedText ImageName = imageName;

	/// <summary>
	/// Priority of the boss compared to all other biomes. Higher value = higher priority.
	/// </summary>
	public readonly float Priority = priority;

	/// <summary>
	/// DiscordRP-API instance linked to this biome.
	/// </summary>
	public readonly string ClientId = clientId;
}
