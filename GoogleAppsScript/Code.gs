/**
 * Stardew Mod List Exporter — Google Apps Script Web App
 *
 * Receives a POST from the SMAPI mod with the player's current mod list and
 * rewrites a Google Sheet with a fresh timestamped table.
 *
 * Deployment: see the "Google Apps Script deployment" section in this repo's README.md.
 */

// ID of the spreadsheet to write to (from its URL: /d/<THIS PART>/edit).
// Leave blank ("") to write to the spreadsheet this script is bound to instead.
const SPREADSHEET_ID = "";

// Name of the sheet/tab to write the mod list to. Created automatically if missing.
const SHEET_NAME = "Mod List";

// Optional shared secret. Must match ModConfig.SharedSecret in config.json.
// Leave blank ("") to accept requests without checking a secret.
const SHARED_SECRET = "";

function doPost(e) {
  try {
    if (!e || !e.postData || !e.postData.contents) {
      return jsonResponse({ status: "error", message: "No request body received." });
    }

    const payload = JSON.parse(e.postData.contents);

    if (SHARED_SECRET && payload.sharedSecret !== SHARED_SECRET) {
      return jsonResponse({ status: "error", message: "Invalid shared secret." });
    }

    const mods = Array.isArray(payload.mods) ? payload.mods : [];
    const timestamp = payload.timestamp || new Date().toISOString();

    writeModListToSheet(mods, timestamp);

    return jsonResponse({ status: "ok", modCount: mods.length });
  } catch (err) {
    return jsonResponse({ status: "error", message: String(err) });
  }
}

function writeModListToSheet(mods, timestamp) {
  const spreadsheet = SPREADSHEET_ID
    ? SpreadsheetApp.openById(SPREADSHEET_ID)
    : SpreadsheetApp.getActiveSpreadsheet();

  let sheet = spreadsheet.getSheetByName(SHEET_NAME);
  if (!sheet) {
    sheet = spreadsheet.insertSheet(SHEET_NAME);
  }

  sheet.clear();

  // Row 1: last-updated timestamp banner, merged across the four data columns.
  const lastUpdatedLabel = "Last updated: " + formatTimestamp(timestamp);
  sheet.getRange(1, 1, 1, 4).merge().setValue(lastUpdatedLabel)
    .setFontWeight("bold")
    .setBackground("#4a86e8")
    .setFontColor("#ffffff");

  // Row 2: column headers.
  const headers = ["Mod Name", "Author", "Version", "ID"];
  sheet.getRange(2, 1, 1, headers.length)
    .setValues([headers])
    .setFontWeight("bold")
    .setBackground("#eeeeee");

  // Row 3+: one row per mod.
  if (mods.length > 0) {
    const rows = mods.map(mod => [
      mod.name || "",
      mod.author || "",
      mod.version || "",
      mod.id || ""
    ]);
    sheet.getRange(3, 1, rows.length, headers.length).setValues(rows);
  }

  sheet.autoResizeColumns(1, headers.length);
  sheet.setFrozenRows(2);
}

function formatTimestamp(isoTimestamp) {
  const date = new Date(isoTimestamp);
  if (isNaN(date.getTime())) {
    return isoTimestamp;
  }
  return Utilities.formatDate(date, Session.getScriptTimeZone(), "yyyy-MM-dd HH:mm:ss z");
}

function jsonResponse(obj) {
  return ContentService.createTextOutput(JSON.stringify(obj))
    .setMimeType(ContentService.MimeType.JSON);
}
