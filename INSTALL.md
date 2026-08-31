# Installing Mod List Exporter

This guide is written for someone who has never coded before. You'll set up a
free Google Sheet that automatically updates with your mod list every time you
launch Stardew Valley. It takes about 10-15 minutes the first time.

You need two things before starting:
- **Stardew Valley** installed
- **SMAPI** installed ([smapi.io](https://smapi.io) has a one-click installer) — if
  you already use mods, you have this

---

## Part 1: Install the mod file

1. Download the mod's zip file from
   [neighborhoodofmusic.com/products/sdv-export-mod-list-to-google-sheets](https://neighborhoodofmusic.com/products/sdv-export-mod-list-to-google-sheets).
   It should contain a folder like `StardewModListExporter` with files inside it.
2. Find your **Mods folder**:
   - **If you install mods manually**: it's inside your Stardew Valley folder,
     e.g. `Stardew Valley\Mods`.
   - **If you use Stardrop** (a mod manager app): open Stardrop, go to its
     settings, and look for "Mods folder" or similar — it uses its own folder,
     usually `%AppData%\Stardrop\Data\Selected Mods` on Windows, not the game's
     own `Mods` folder. If you're not sure which you use, check both.
3. Copy the entire `StardewModListExporter` folder into that Mods folder.
4. Launch the game once through SMAPI (or through Stardrop, whichever you
   normally use) and let it fully load to the title screen, then close it again.
   This step creates a new file called `config.json` inside the mod's folder —
   you'll need it in Part 3.

**How to check it worked:** when SMAPI starts, it prints a list of loaded mods
in its console/log window. Look for a line like:
```
Mod List Exporter 1.0.0 by ... | Exports your active SMAPI mod list to a Google Sheet...
```
If you don't see it, re-check step 2 — this is almost always a "copied it to
the wrong Mods folder" problem, especially if you use Stardrop.

---

## Part 2: Set up your Google Sheet

1. Go to [sheets.google.com](https://sheets.google.com) and create a **new
   blank spreadsheet**. Name it whatever you like, e.g. "My Stardew Mods".
2. In the menu bar, click **Extensions → Apps Script**. A new tab opens with a
   code editor.
3. You'll see a file called `Code.gs` with some placeholder text already in
   it. Click inside the editor, select **all** of that text (Ctrl+A) and
   delete it.
4. Paste in the mod's Apps Script code (ask whoever gave you this mod for the
   contents of `GoogleAppsScript/Code.gs` — it's plain text, safe to copy).
   ⚠️ Make sure you deleted *all* the placeholder text first — leftover code
   mixed with the pasted code causes a "Syntax error" when you try to deploy.
5. Save with **Ctrl+S**.
6. Click the blue **Deploy** button (top right) → **New deployment**.
7. Click the gear icon ⚙️ next to "Select type" and choose **Web app**.
8. Set:
   - **Execute as:** Me
   - **Who has access:** Anyone
9. Click **Deploy**.
10. Google will ask you to authorize the script. Click **Authorize access**,
    pick your Google account, and if you see a warning screen saying "Google
    hasn't verified this app" — that's expected for your own script. Click
    **Advanced**, then **Go to (your project name) (unsafe)**, then **Allow**.
11. You'll now see a **Web app URL** that ends in `/exec`. **Copy this whole
    URL** — you'll paste it into the mod's settings next.

---

## Part 3: Connect the mod to your sheet

[Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098) is
completely **optional** — the mod works fine without it — but if you have it
(or don't mind installing it), it turns this part into three clicks instead of
hand-editing a settings file. Worth grabbing if you're not sure.

**If you have Generic Mod Config Menu installed**, this is the easy way:

1. Launch the game and open the pause menu → the mod settings icon (usually a
   gear/cog) → **Mod List Exporter**.
2. Paste your Web App URL (from Part 2, step 11) into the **Web App URL**
   field.
3. Close the menu — it saves automatically.
4. Skip to step 5 below.

**If you don't have Generic Mod Config Menu**, edit the settings file by hand:

1. Go back to your Mods folder and open the `StardewModListExporter` folder.
2. Open `config.json` with a plain text editor (right-click → Open with →
   Notepad works fine — don't use Word).
3. Find the line that says `"WebAppUrl"` and replace the placeholder text
   between the quotes with the URL you copied in Part 2, step 11. It should
   look like this when you're done:
   ```json
   {
     "Enabled": true,
     "WebAppUrl": "https://script.google.com/macros/s/AKfycb.../exec",
     "SharedSecret": "",
     "TimeoutSeconds": 15
   }
   ```
4. Save the file (Ctrl+S) and close it. **Don't** touch `manifest.json` in the
   same folder — that's a different file the mod needs untouched.

**Either way, finish here:**

5. Launch the game again. Check the SMAPI console for a line like:
   ```
   Mod List Exporter | Exported 120 mods to Google Sheets.
   ```
6. Open your Google Sheet — a new tab called "Mod List" should now show your
   full mod list with a "Last updated" timestamp at the top.

That's it — from now on, every time you launch the game, the sheet refreshes
automatically.

---

## Optional: add a shared secret

By default, anyone who somehow got your Web App URL could send data to your
sheet. If that worries you, add a simple password:

1. In the Apps Script editor, find the line `const SHARED_SECRET = "";` near
   the top and put any word or phrase between the quotes, e.g.
   `const SHARED_SECRET = "farmhand42";`
2. Save, then **Deploy → Manage deployments → click the pencil (edit) icon →
   Version: New version → Deploy**. (A plain save does *not* update the live
   URL — you must create a new deployment version.)
3. Set the matching **Shared Secret** to that same word — either in the
   in-game mod settings menu (if you have Generic Mod Config Menu) or by
   editing `"SharedSecret"` in `config.json`.
4. Save and relaunch the game.

---

## Troubleshooting

**"Mod list export skipped: set WebAppUrl in config.json..." in the console**
The mod loaded fine, but `config.json` still has the placeholder URL. Go back
to Part 3.

**The mod doesn't show up in SMAPI's loaded mods list at all**
You copied the mod folder into the wrong Mods folder. If you use Stardrop,
double-check its actual mods folder in Stardrop's own settings — it's
different from the game's built-in `Mods` folder, and Stardrop won't load
anything from the wrong one.

**"Syntax error: Unexpected token" when saving/deploying the Apps Script**
Some of the original placeholder code (usually `function myFunction() {}`) got
left in when you pasted. Select everything in the editor (Ctrl+A), delete it
completely, and paste the mod's code fresh — make sure nothing else is left
before or after it.

**"Windows can't find [some file]" / "Sorry, unable to open the file"**
Usually means a step earlier didn't run yet (e.g. the game hasn't been
launched since you changed something, so a file wasn't created), or you're
looking at a stale/incorrect link. Retrace the last step you completed.

**The sheet says "Invalid shared secret"**
The word in `config.json`'s `"SharedSecret"` doesn't exactly match the one in
`Code.gs`'s `SHARED_SECRET`. They must be identical, including capitalization.

**Nothing happens and there's no error at all**
Check that `"Enabled": true` in `config.json` (not `false`), and that the
`WebAppUrl` ends in `/exec`, not `/dev`.
