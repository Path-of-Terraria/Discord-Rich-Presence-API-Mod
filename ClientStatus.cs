namespace DiscordRPAPI;

/// <summary>
/// Defines the required fields when updating
/// the client's Rich Presence status.
/// </summary>
public class ClientStatus
{
	/// <value>
	/// the user's current party status
	/// </value>
	public string State = "";

	/// <value>
	/// what the player is currently doing
	/// </value>
	public string Description = "";

	/// <value>
	/// name of the uploaded image for the large profile artwork
	/// </value>
	public string LargeImageKey = null;

	/// <value>
	/// tooltip for the largeImageKey
	/// </value>
	public string LargeImageText = null;

	/// <value>
	/// name of the uploaded image for the small profile artwork
	/// </value>
	public string SmallImageKey = null;

	/// <value>
	/// tooltip for the smallImageKey
	/// </value>
	public string SmallImageText = null;
}