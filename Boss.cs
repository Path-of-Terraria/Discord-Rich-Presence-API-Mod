namespace DiscordRPCAPI;

/// <summary>
/// Represents an NPC that is presentable through
/// Discord Rich Presence.
/// </summary>
public readonly struct Boss(string imageKey, string imageName, float priority, string clientId)
{
	/// <value>
	/// name of the uploaded image for the large profile artwork
	/// </value>
	public readonly string ImageKey = imageKey;

	/// <value>
	/// tooltip for the imageKey
	/// </value>
	public readonly string ImageName = imageName;

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