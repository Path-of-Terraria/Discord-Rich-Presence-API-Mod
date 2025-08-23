using System;
using System.Collections.Generic;

namespace DiscordRPCAPI;

/// <summary>
/// Provides a set of methods to handle mod calls for the discord RPC API mod, allowing other mods to interact with discord rich presence functionality.
/// </summary>
internal static class ModCalls
{
	/// <summary>
	/// Processes mod calls to configure discord rich presence settings, such as adding bosses, biomes, clients, or setting main menu status.
	/// </summary>
	/// <param name="args">An array of arguments specifying the type of call and its parameters. The first argument is the message type (e.g., "AddBoss", "AddBiome", "MainMenu", "AddClient").</param>
	/// <returns><c>true</c> if the call is processed successfully; otherwise, throws an exception for invalid operations.</returns>
	/// <exception cref="InvalidOperationException">Thrown if <see cref="DiscordRPCAPIMod.Instance"/> is null or if the call is invalid.</exception>
	public static bool Call(params object[] args)
	{
		if (DiscordRPCAPIMod.Instance == null)
		{
			throw new InvalidOperationException("DiscordRPCAPIMod.Instance is null, mod isn't loaded. Call this later.");
		}

		int argsCount = args.Length;
		Array.Resize(ref args, 15);

		if (args[0] is string message)
		{
			switch (message.ToLower())
			{
				//e.g Call("AddBoss", List<int>Id, "Angry Slimey", "boss_placeholder", float default:16f, client="default")
				case "addboss":
					return CallAddBoss(args);

				//e.g Call("AddBiome", Func<bool> checker, "SlimeFire Biome", "biome_placeholder", float default:50f/150f, client="default")
				case "addevent":
				case "addbiome":
					return CallAddBiome(args, message);
				
				//e.g Call("AddWorld", "Brain Domain", "domain_placeholder", client="default")
				case "addworld":
					return CallAddWorld(args);

				//e.g. Call("MainMenu", "details", "belowDetails", "mod_placeholder", "modName")
				case "mainmenu":
				case "mainmenuoverride":
					return CallAddMainMenu(args);

				//e.g. Call("AddClient", "716207249902796810", "angryslimey")
				case "addclient":
				case "addnewclient":
				case "newclient":
				case "adddiscordclient":
					return CallAddClient(args, argsCount);
				//e.g. Call("AddPlayerStat", string statName, Func<string> statValue)
				case "addplayerstat":
					return CallAddPlayerStat(args);
				default:
					break;
			}
		}

		;

		return true;
	}

	/// <summary>
	/// Adds a new Discord client with the specified application ID and key.
	/// </summary>
	/// <param name="args">The arguments array containing the client ID and key.</param>
	/// <param name="argsCount">The number of arguments provided in the original call.</param>
	/// <returns><c>true</c> if the client is added successfully.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the argument count is not 3 or if a client cannot be created at this time.</exception>
	private static bool CallAddClient(object[] args, int argsCount)
	{
		if (argsCount != 3 || !DiscordRPCAPIMod.Instance.CanCreateClient)
		{
			string result = "";

			if (argsCount != 3)
			{
				result = "argsCount is not three; ";
			}

			if (!DiscordRPCAPIMod.Instance.CanCreateClient)
			{
				result += "Client cannot be created as call is too late.";
			}

			throw new InvalidOperationException(result);
		}

		string newID = args[1] as string;
		string idKey = args[2] as string;
		DiscordRPCAPIMod.Instance?.AddDiscordAppID(idKey, newID);

		return true;
	}

	/// <summary>
	/// Sets a custom main menu status for discord rich presence.
	/// </summary>
	/// <param name="args">The arguments array containing details, additional details, and image keys for the discord status.</param>
	/// <returns><c>true</c> if the main menu status is set successfully.</returns>
	/// <exception cref="InvalidOperationException">Thrown if a client cannot be created at this time.</exception>
	private static bool CallAddMainMenu(object[] args)
	{
		if (!DiscordRPCAPIMod.Instance.CanCreateClient)
		{
			throw new InvalidOperationException("Client cannot be created as call is too late.");
		}

		string details = args[1] as string;
		string additionalDetails = args[2] as string;
		var largeImage = (args[3] as string, args[4] as string);
		var smallImage = (args[5] as string, args[6] as string);
		DiscordRPCTooling.NewMenuStatus(details, additionalDetails, largeImage, smallImage);

		return true;
	}

	/// <summary>
	/// Adds a biome or event to the discord rich presence data with a specified checker function, texture, and priority.
	/// </summary>
	/// <param name="args">The arguments array containing the checker function, texture path, language key, priority, and client.</param>
	/// <param name="message">The type of call ("AddBiome" or "AddEvent").</param>
	/// <returns><c>true</c> if the biome or event is added successfully.</returns>
	private static bool CallAddBiome(object[] args, string message)
	{
		var checker = args[1] as Func<bool>;
		(string texturePath, string langKey) = (args[2] as string, args[3] as string);
		float priority = DiscordRPCTooling.ObjectToFloat(args[4], message == "AddBiome" ? 50f : 150f);
		string client = args[5] is string ? args[5] as string : "default";
		DataPopulator.AddBiome(checker, texturePath, langKey, priority, client);

		return true;
	}

	/// <summary>
	/// Adds a boss to the discord rich presence data with specified IDs, image key, and priority.
	/// </summary>
	/// <param name="args">The arguments array containing the boss IDs, image key, priority, and client.</param>
	/// <returns><c>true</c> if the boss is added successfully.</returns>
	private static bool CallAddBoss(object[] args)
	{
		List<int> Id = DiscordRPCTooling.ObjectToListInt(args[1]);
		var ImageKey = (args[3] as string, args[2] as string);
		float priority = DiscordRPCTooling.ObjectToFloat(args[4], 16f);
		string client = args[5] is string ? args[5] as string : "default";

		DataPopulator.AddBoss(Id, ImageKey, priority, client);
		return true;
	}

	/// <summary>
	/// Adds a custom player stat for discord rich presence with <see cref="statName"/> and <see cref="statValue"/> from args
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	private static bool CallAddPlayerStat(object[] args)
	{
		string statName = args[1] as string;
		if (args[2] is not Func<string> statValue)
		{
			return false;
		}
		DataPopulator.AddCustomStat(statName, ref statValue);
		return true;
	}
	
	/// <summary>
	/// Adds a custom world to the discord rich presence data with a specified texture.
	/// </summary>
	/// <param name="args">The arguments array containing texture path, language key, and client.</param>
	/// <returns><c>true</c> if the world is added successfully.</returns>
	private static bool CallAddWorld(object[] args)
	{
		var checker = args[1] as Func<bool>;
		(string texturePath, string langKey) = (args[1] as string, args[2] as string);
		string client = args[3] is string ? args[3] as string : "default";
		DataPopulator.AddCustomWorld(texturePath, langKey, client);

		return true;
	}
}
