using Terraria.Localization;

namespace DiscordRPCAPI;

	/// <summary>
	/// Data structure for any world.<br/>
	/// </summary>
	/// <param name="ImageKey">The image key for the Discord app to reference.</param>
	/// <param name="ImageName">The hover text for the image in Discord.</param>
	/// <param name="ClientId">DiscordRPCAPI instance linked to this boss.</param>
	public readonly record struct World(string ImageKey, LocalizedText ImageName, string ClientId);