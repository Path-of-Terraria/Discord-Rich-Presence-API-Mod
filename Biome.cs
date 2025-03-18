using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria.Localization;

namespace DiscordRPCAPI; 
/// <summary>
/// Represents a Biome that is presentable through
/// Discord Rich Presence.
/// </summary>
public readonly struct Biome(Func<bool> checker, string imageKey, LocalizedText imageName, float priority, string clientId)
{
	/// <value>
	/// Whether or not the player is currently in the biome.
	/// This should be used to be somewhat consistent with
	/// the rest of the Terraria API.
	/// </value>
	public readonly bool IsActive => Checker();

	/// <value>
	/// function to determine if the player is currently
	/// within the biome. You should probably use
	/// <see cref="IsActive"/> when checking if the biome
	/// is active within normal code.
	/// </value>
	internal readonly Func<bool> Checker = checker;

	/// <value>
	/// name of the uploaded image for the large profile artwork
	/// </value>
	public readonly string ImageKey = imageKey;

	/// <value>
	/// tooltip for the imageKey
	/// </value>
	public readonly LocalizedText ImageName = imageName;

	/// <value>
	/// floating point value to allow for boss precedence.
	/// the higher the value, the higher the precedence.
	/// </value>
	public readonly float Priority = priority;

	/// <value>
	/// DiscordRP instance linked to this boss
	/// </value>
	public readonly string ClientId = clientId;
}
