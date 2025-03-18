using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordRPCAPI;

internal static class ModCalls
{
	/// <summary>
	///	Call
	/// </summary>
	/// <param name="args"></param>
	/// <returns>String of call results</returns>
	public static object Call(params object[] args)
	{
		if (DiscordRPCAPIMod.Instance == null)
		{
			return "Instance not yet loaded.";
		}

		int argsCount = args.Length;
		Array.Resize(ref args, 15);

		try
		{
			string message = args[0] as string;

			switch (message)
			{
				//e.g Call("AddBoss", List<int>Id, "Angry Slimey", "boss_placeholder", float default:16f, client="default")
				case "AddBoss":
					{
						List<int> Id = DiscordRPCTooling.ObjectToListInt(args[1]);
						var ImageKey = (args[3] as string, args[2] as string);
						float priority = DiscordRPCTooling.ObjectToFloat(args[4], 16f);
						string client = args[5] is string ? args[5] as string : "default";

						DataPopulator.AddBoss(Id, ImageKey, priority, client);
						return "Success";
					}

				//e.g Call("AddBiome", Func<bool> checker, "SlimeFire Biome", "biome_placeholder", float default:50f/150f, client="default")
				case "AddEvent":
				case "AddBiome":
					{
						var checker = args[1] as Func<bool>;
						(string texturePath, string langKey) = (args[2] as string, args[3] as string);
						float priority = DiscordRPCTooling.ObjectToFloat(args[4], message == "AddBiome" ? 50f : 150f);
						string client = args[5] is string ? args[5] as string : "default";
						DataPopulator.AddBiome(checker, texturePath, langKey, priority, client);
						return "Success";
					}

				//e.g. Call("MainMenu", "details", "belowDetails", "mod_placeholder", "modName")
				case "MainMenu":
				case "MainMenuOverride":
					{
						if (!DiscordRPCAPIMod.Instance.CanCreateClient)
						{
							return "Failure";
						}

						string details = args[1] as string;
						string additionalDetails = args[2] as string;
						var largeImage = (args[3] as string, args[4] as string);
						var smallImage = (args[5] as string, args[6] as string);
						DiscordRPCTooling.NewMenuStatus(details, additionalDetails, largeImage, smallImage);
						return "Success";
					}

				//e.g. Call("AddClient", "716207249902796810", "angryslimey")
				case "AddClient":
				case "AddNewClient":
				case "NewClient":
				case "AddDiscordClient":
					{
						if (argsCount != 3 || !DiscordRPCAPIMod.Instance.CanCreateClient)
						{
							return "Failure";
						}

						string newID = args[1] as string;
						string idKey = args[2] as string;
						DiscordRPCAPIMod.Instance?.AddDiscordAppID(idKey, newID);
						return "Success";
					}
				default:
					break;
			};
		}
		catch
		{
		}

		return "Failure";
	}
}
