namespace DiscordRPCAPI;

internal class ExitHookPlayer : ModSystem
{
	/// <summary>
	/// Show the player status as on main menu on exit of world
	/// </summary>
	public override void ClearWorld()
	{
		if (!Main.dedServ && Main.gameMenu)
		{
			DiscordRPCAPIMod.Instance.ClientOnMainMenu();
		}
	}
}
