namespace DiscordRPCAPI;

internal class ExitHookPlayer : ModSystem
{
	private static Mod _subworldLibrary;
	private static Mod _pathOfTerraria;
	private bool _inSubworld = false;

	public override void Load()
	{
		_subworldLibrary = ModLoader.GetMod("SubworldLibrary");
		_pathOfTerraria = ModLoader.GetMod("PathOfTerraria");
	}

	/// <summary>
	/// Show the player status as on main menu only when exiting to the actual main menu
	/// </summary>
	public override void OnWorldUnload()
	{
		base.OnWorldUnload();
		
		if (Main.dedServ)
		{
			return;
		}
		
		if (_subworldLibrary?.Call("AnyActive",_pathOfTerraria) is bool and true)
		{
			_inSubworld=true;
			return;
		}
		
		if (_inSubworld && _subworldLibrary?.Call("AnyActive",_pathOfTerraria) is bool and false)
		{
			_inSubworld = false;
			return;
		}
		
		if (Main.gameMenu && !_inSubworld)
		{
			DiscordRPCAPIMod.Instance.ClientOnMainMenu();
		}
	}
	
	public override void OnWorldLoad()
	{
		base.OnWorldLoad();
		Main.gameMenu = false;
	}
}
