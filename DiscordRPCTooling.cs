using System;
using System.Collections.Generic;

namespace DiscordRPCAPI;

public static class DiscordRPCTooling
{
	public static List<int> ObjectToListInt(object data)
	{
		return data is List<int> ? data as List<int> : data is int ? new List<int>() { Convert.ToInt32(data) } : null;
	}

	public static float ObjectToFloat(object data, float def)
	{
		return data is float single ? single : def;
	}

	/// <summary>
	/// Override main menu status
	/// </summary>
	/// <param name="details">upper text</param>
	/// <param name="additionalDetails">lower text</param>
	/// <param name="largeImage">key and text for large image</param>
	/// <param name="smallImage">key and text for small image</param>
	public static void NewMenuStatus(string details, string additionalDetails, (string, string) largeImage, (string, string) smallImage)
	{
		if (string.IsNullOrEmpty(largeImage.Item1))
		{
			largeImage.Item1 = "mod_placeholder";
		}

		if (string.IsNullOrEmpty(smallImage.Item1))
		{
			smallImage.Item1 = null;
		}

		ClientStatus customStatus = new()
		{
			State = additionalDetails,
			Description = details,
			LargeImageKey = largeImage.Item1,
			LargeImageText = largeImage.Item2,
			SmallImageKey = smallImage.Item1,
			SmallImageText = smallImage.Item2,
		};

		DiscordRPCAPIMod.Instance.SetCustomStatus(customStatus);
	}
}
