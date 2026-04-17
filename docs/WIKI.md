# DiscordRPCAPI Wiki

## Overview

`DiscordRPCAPI` updates Discord Rich Presence for Terraria automatically on the client.

Out of the box, the mod already:

- Connects to its built-in Discord application ID.
- Shows a main menu status.
- Tracks play time if enabled in config.
- Shows your held item as the small image text.
- Uses built-in support for vanilla bosses, vanilla biomes, and vanilla events.

You do not need to set up a custom Discord application just to use the mod normally.

## Default behavior

On load, the mod creates a Discord RPC client with its built-in default application ID:

- `1351686373786255431`

By default, the client config currently starts with:

- `Enable = true`
- `Timer = 1`
- `ShowTime = true`
- `ShowTimeCycle = true`
- `ShowPrefix = true`
- `ShowCustomStat = true`
- `ShowCustomWorldName = true`
- `ShowWorldName = false`
- `ShowBoss = false`
- `ShowBiome = false`
- `ShowHealth = false`
- `ShowDPS = false`
- `ShowMana = false`
- `ShowDefense = false`
- `ShowDamage = false`

That means the mod works immediately after enabling it, but the default display is intentionally conservative. In particular:

- Boss names are not shown until `ShowBoss` is enabled.
- Biome and event names are not shown until `ShowBiome` is enabled.
- Player stats are hidden until their matching config flags are enabled.
- Weapon/item text still appears, and prefix text is shown by default.

## What players should configure

Open the mod's client config and adjust the visibility flags to match what you want Discord to show.

Recommended settings if you want the "full" presence:

- Enable `ShowBoss`
- Enable `ShowBiome`
- Enable any of `ShowHealth`, `ShowDPS`, `ShowMana`, `ShowDefense`
- Enable `ShowDamage` if you want weapon damage included in the held-item text
- Enable `ShowWorldName` if you want the actual world name instead of `Single Player` or `Multiplayer`

## What the mod can display

The built-in data includes:

- Vanilla bosses
- Vanilla biomes
- Vanilla world events
- Main menu status
- Held weapon/item info
- Day/night text for biome and event displays

The world difficulty text is taken from the current world and displayed as one of:

- `(Normal)`
- `(Expert)`
- `(Master)`

## Adding your own Discord application ID

Custom application IDs are for mod authors or pack authors who want their own Discord art assets and keys.

The important rule is timing:

- `AddClient` only works during mod loading, before `PostSetupContent` finishes.
- If you call it too late, the mod throws an error and refuses the client registration.

Register a new application ID with a mod call:

```csharp
public override void Load() {
    Mod rpc = ModLoader.GetMod("DiscordRPCAPI");
    rpc?.Call("AddClient", "YOUR_DISCORD_APPLICATION_ID", "yourclientkey");
}
```

Parameters:

- First argument: the call name, `AddClient`
- Second argument: the Discord application ID as a string
- Third argument: the internal key used by this mod to refer to that client

Example:

```csharp
rpc?.Call("AddClient", "123456789012345678", "mymod");
```

After that, use `mymod` as the `client` argument when registering bosses, biomes, or worlds.

## Adding your own images

This mod does not load image files directly from your Terraria mod folder into Discord. The image keys must exist as Rich Presence assets in the Discord application you are using.

The workflow is:

1. Create or choose a Discord application.
2. Copy its application ID.
3. Upload your Rich Presence image assets in Discord for that application.
4. Note each uploaded asset key exactly as Discord stores it.
5. Register that application ID with `AddClient`.
6. Use those asset keys in `AddBoss`, `AddBiome`, `AddWorld`, or `MainMenu`.

Important details:

- The image key used in code must exactly match the asset key uploaded to Discord.
- If you use the built-in `default` client, only asset keys available on the built-in DiscordRPCAPI application will work.
- If you want your own art, you almost always want your own Discord application ID.

## Using your custom application for bosses, biomes, and worlds

### Add a boss

```csharp
rpc?.Call(
    "AddBoss",
    ModContent.NPCType<MyBoss>(),
    "My Boss",
    "boss_myboss",
    50f,
    "mymod"
);
```

Arguments:

- NPC ID or `List<int>`
- Display name
- Discord image key
- Priority
- Client key

### Add a biome or event

```csharp
rpc?.Call(
    "AddBiome",
    (Func<bool>)(() => Main.LocalPlayer.InModBiome(ModContent.GetInstance<MyBiome>())),
    "biome_mybiome",
    "Mods.MyMod.Biomes.MyBiome",
    60f,
    "mymod"
);
```

Arguments:

- `Func<bool>` checker
- Discord image key
- localization key
- priority
- client key

`AddEvent` is handled the same way as `AddBiome`, but with a higher default priority.

### Add a custom world image

```csharp
rpc?.Call(
    "AddWorld",
    "world_mycustomworld",
    "Mods.MyMod.Worlds.MyCustomWorld",
    "mymod"
);
```

This is used when `ShowCustomWorldName` is enabled.

Important details:

- The first argument is the Discord image key.
- The second argument is a localization key.
- The localized text for that key must match the world name the player is in, because the mod compares that localized value against `Main.worldName`.

## Main menu override

You can replace the default main menu presence:

```csharp
rpc?.Call(
    "MainMenu",
    "Browsing My Mod",
    "In the main menu",
    "mod_logo",
    "My Mod",
    "mod_icon_small",
    "My Mod"
);
```

If the large image key is empty, the mod falls back to `mod_placeholder`.

## Forcing a specific client at runtime

If you have already registered multiple application IDs, you can force one:

```csharp
rpc?.Call("SetClientOverride", "mymod");
```

Clear the override:

```csharp
rpc?.Call("ClearClientOverride");
```

This is useful if you want one Discord application to be used regardless of which biome or boss is active.

## Notes and limitations

- This is a client-side mod config.
- The mod does nothing on dedicated servers.
- Boss presence overrides biome presence.
- If a client key is missing, the mod falls back to `default`.
- `AddClient` and `MainMenu` must be called early during loading.
- World-name matching for `AddWorld` is name-based, not file-based.
- The current `AddWorld` mod call is implemented as `(imageKey, localizationKey, clientKey)`.

## Quick start examples

### Player-only use

1. Enable the mod.
2. Open client config.
3. Turn on `ShowBoss` and `ShowBiome`.
4. Optionally enable stats and damage.
5. Launch Terraria with Discord running.

### Mod author use with custom art

1. Create a Discord application.
2. Upload Rich Presence assets there.
3. In your mod `Load`, call `AddClient`.
4. Register your bosses, biomes, events, or worlds using your client key.
5. Use Discord asset keys exactly as uploaded.
