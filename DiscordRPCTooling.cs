using System;
using System.Collections.Generic;

namespace DiscordRPCAPI;

/// <summary>
/// Provides utility methods for handling data conversions and discord rich presence status updates in the discord RPC API mod.
/// </summary>
public static class DiscordRPCTooling
{
	/// <summary>
	/// Converts an object to a list of integers.
	/// </summary>
	/// <param name="data">The object to convert. Expected to be either a <see cref="List{T}"/> of <see cref="int"/> or a single <see cref="int"/>.</param>
	/// <returns>A <see cref="List{T}"/> containing integers if the input is a valid list or single integer; otherwise, <c>null</c>.</returns>
	public static List<int> ObjectToListInt(object data)
	{
		return data is List<int> ? data as List<int> : data is int ? new List<int>() { Convert.ToInt32(data) } : null;
	}
	
	/// <summary>
	/// Converts an object to a float, returning a default value if the conversion is not possible.
	/// </summary>
	/// <param name="data">The object to convert. Expected to be a <see cref="float"/>.</param>
	/// <param name="def">The default value to return if the conversion fails.</param>
	/// <returns>The converted <see cref="float"/> value if the input is a float; otherwise, the default value.</returns>
	public static float ObjectToFloat(object data, float def)
	{
		return data is float single ? single : def;
	}

	/// <summary>
	/// Sets a custom discord rich presence status for the main menu.
	/// </summary>
	/// <param name="details">The upper text to display in the discord status.</param>
	/// <param name="additionalDetails">The lower text (state) to display in the discord status.</param>
	/// <param name="largeImage">A tuple containing the key and tooltip text for the large image in the discord status.</param>
	/// <param name="smallImage">A tuple containing the key and tooltip text for the small image in the discord status.</param>
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
