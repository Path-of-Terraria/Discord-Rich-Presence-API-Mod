namespace DiscordRPAPI;

internal class ExitHookPlayer : ModSystem
{
	/// <summary>
	/// Show the Player status as on MainMenu on Exit of World
	/// </summary>
	public override void ClearWorld()
	{
		if (!Main.dedServ && Main.gameMenu)
		{
			DiscordRPCAPIMod.Instance.ClientOnMainMenu();
		}
	}
}
