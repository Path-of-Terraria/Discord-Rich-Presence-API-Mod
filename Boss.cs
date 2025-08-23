namespace DiscordRPCAPI;

/// <summary>
/// Data structure for any given NPC, boss or otherwise.<br/>
/// <see cref="Boss"/>es always override any <see cref="Biome"/>s.
/// </summary>
/// <param name="ImageKey">The image key for the Discord app to reference.</param>
/// <param name="ImageName">The hover text for the image in Discord.</param>
/// <param name="Priority">Priority of the boss compared to all other bosses/NPCs. Higher value = higher priority.</param>
/// <param name="ClientId">DiscordRPCAPI instance linked to this boss.</param>
public readonly record struct Boss(string ImageKey, string ImageName, float Priority, string ClientId);