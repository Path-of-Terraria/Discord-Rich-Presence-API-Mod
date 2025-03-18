namespace DiscordRPCAPI;

internal class ExitHookPlayer : ModSystem
{
	public override void ClearWorld()
	{
		if (!Main.dedServ)
		{
			DiscordRPCAPIMod.Instance.ClientOnMainMenu();
		}
	}
}
