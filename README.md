# Stardew Mod List Exporter

A SMAPI mod that automatically exports your active mod list to a Google Sheet
every time Stardew Valley launches, plus the Google Apps Script Web App that
receives the data.

## Contents

- `StardewModListExporter/` — the C# SMAPI mod
  - `manifest.json` — mod metadata read by SMAPI
  - `ModEntry.cs` — hooks `GameLaunched`, reads the mod list via `IModRegistry`, and POSTs it as JSON
  - `ModConfig.cs` — settings schema (`config.json` is generated from this on first run)
  - `StardewModListExporter.csproj` — build config using Pathoschild's SMAPI mod build package
- `GoogleAppsScript/Code.gs` — the Web App that receives the POST and rewrites the sheet

## How it works

1. On game launch, the mod calls `Helper.ModRegistry.GetAll()` to get every loaded mod's
   `Name`, `Author`, `Version`, and `UniqueID`.
2. It serializes that list (plus a UTC timestamp and an optional shared secret) to JSON.
3. It POSTs the JSON to the Web App URL configured in `config.json`.
4. The Apps Script clears the target sheet and rewrites it: a "Last updated" banner row,
   a header row (`Mod Name | Author | Version | ID`), then one row per mod.

## Building the mod

Requirements: [.NET 6 SDK](https://dotnet.microsoft.com/download), SMAPI installed, and
Stardew Valley installed (the build package auto-detects the game folder on
Windows/macOS/Linux; if it can't find it, set `<GamePath>` in the `.csproj`).

```bash
cd StardewModListExporter
dotnet build
```

This copies the built mod into your `Mods` folder automatically (via
`Pathoschild.Stardew.ModBuildConfig`). Launch the game through SMAPI once so it
generates `config.json` next to the mod's DLL, then edit that file:

```json
{
  "Enabled": true,
  "WebAppUrl": "https://script.google.com/macros/s/AKfycb.../exec",
  "SharedSecret": "some-random-string",
  "TimeoutSeconds": 15
}
```

## Google Apps Script deployment

1. Create (or open) the Google Sheet you want the mod list written to.
2. In the sheet, go to **Extensions → Apps Script**.
3. Replace the default `Code.gs` contents with this repo's `GoogleAppsScript/Code.gs`.
4. (Optional) Set `SHARED_SECRET` in `Code.gs` to the same value you put in the mod's
   `config.json` — this stops random requests from overwriting your sheet.
5. Leave `SPREADSHEET_ID` blank to target the sheet the script is bound to, or set it to
   another spreadsheet's ID (the long string in its URL between `/d/` and `/edit`).
6. Click **Deploy → New deployment**.
   - Type: **Web app**
   - Execute as: **Me**
   - Who has access: **Anyone** (required so SMAPI's `HttpClient` can call it without OAuth)
7. Click **Deploy**, authorize the script when prompted, and copy the **Web app URL**
   (it ends in `/exec`).
8. Paste that URL into `WebAppUrl` in the mod's `config.json`.

Whenever you edit `Code.gs` after the first deployment, use **Deploy → Manage deployments →
Edit (pencil icon) → New version** so the `/exec` URL picks up your changes — a plain save
does not update a live deployment.

## Notes / limitations

- Apps Script Web Apps enforce daily quotas on free Google accounts; launching the game
  repeatedly in a short time won't hit them under normal use.
- The shared secret is a basic anti-spam check, not real authentication — anyone with the
  URL and secret can still write to the sheet. Don't put sensitive data in the payload.
- If the request fails (bad URL, network issue, quota), the mod logs a warning/error to the
  SMAPI console and continues loading the game normally — it never blocks startup.
