namespace DiscordRPAPI;


public class Boss
{
	public string ImageKey { get; set; }
	public string ImageName { get; set; }
	public float Priority { get; set; }
	public string ClientId { get; set; }
	
	/// <summary>
	/// Data structure for any given NPC, boss or otherwise.<br/>
	/// <see cref="Boss"/>es always override any <see cref="Biome"/>s.
	/// </summary>
	/// <param name="imageKey">The image key for the Discord app to reference.</param>
	/// <param name="imageName">The hover text for the image in Discord.</param>
	/// <param name="priority">Priority of the boss compared to all other bosses/NPCs. Higher value = higher priority.</param>
	/// <param name="clientId">DiscordRPAPI instance linked to this boss.</param>
	public Boss(string imageKey, string imageName, float priority, string clientId)
	{
		ImageKey = imageKey;
		ImageName = imageName;
		Priority = priority;
		ClientId = clientId;
	}
}