<#
.SYNOPSIS
  Sets <GamePath> and <GameModsPath> in StardewModListExporter.csproj so the mod
  build package deploys to the right folder (e.g. Stardrop's "Selected Mods" folder
  instead of the game's default Mods folder).

.EXAMPLE
  .\scripts\Set-GameModsPath.ps1
  .\scripts\Set-GameModsPath.ps1 -GamePath "D:\Games\Stardew Valley" -GameModsPath "D:\Stardrop\Selected Mods"
#>

param(
    [string]$CsprojPath = "$PSScriptRoot\..\StardewModListExporter\StardewModListExporter.csproj",
    [string]$GamePath = "F:\SteamLibrary\steamapps\Common\Stardew Valley",
    [string]$GameModsPath = "$env:APPDATA\Stardrop\Data\Selected Mods"
)

$CsprojPath = Resolve-Path $CsprojPath -ErrorAction Stop

Write-Host "Target project file: $CsprojPath"
Write-Host "GamePath:            $GamePath"
Write-Host "GameModsPath:        $GameModsPath"

if (-not (Test-Path $GamePath)) {
    Write-Warning "GamePath does not exist on disk: $GamePath (continuing anyway, but double-check it)"
}
if (-not (Test-Path $GameModsPath)) {
    Write-Warning "GameModsPath does not exist on disk: $GameModsPath (continuing anyway, but double-check it)"
}

# Back up the original file once, next to itself.
$backupPath = "$CsprojPath.bak"
if (-not (Test-Path $backupPath)) {
    Copy-Item $CsprojPath $backupPath
    Write-Host "Backed up original to: $backupPath"
}

$xml = New-Object System.Xml.XmlDocument
$xml.PreserveWhitespace = $true
$xml.Load($CsprojPath)

# Find the PropertyGroup that already has (or should have) GamePath/GameModsPath.
$propertyGroup = $xml.Project.PropertyGroup | Select-Object -First 1
if (-not $propertyGroup) {
    throw "Couldn't find a <PropertyGroup> in $CsprojPath"
}

function Set-OrAddElement {
    param($ParentNode, $Xml, [string]$Name, [string]$Value)

    $existing = $ParentNode.SelectSingleNode($Name)
    if ($existing) {
        $existing.InnerText = $Value
        return
    }

    $newNode = $Xml.CreateElement($Name)
    $newNode.InnerText = $Value

    # Insert before the trailing whitespace so the new element lines up with its siblings
    # instead of landing jammed against the closing </PropertyGroup> tag.
    $lastChild = $ParentNode.LastChild
    $lastChildIsWhitespace = $lastChild -and ($lastChild.NodeType -eq "Text" -or $lastChild.NodeType -eq "Whitespace")
    if ($lastChildIsWhitespace) {
        $ParentNode.InsertBefore($newNode, $lastChild) | Out-Null
        $ParentNode.InsertBefore($Xml.CreateTextNode("`n    "), $newNode) | Out-Null
    } else {
        $ParentNode.AppendChild($newNode) | Out-Null
    }
}

Set-OrAddElement -ParentNode $propertyGroup -Xml $xml -Name "GamePath" -Value $GamePath
Set-OrAddElement -ParentNode $propertyGroup -Xml $xml -Name "GameModsPath" -Value $GameModsPath

$xml.Save($CsprojPath)

Write-Host "`nDone. Updated $CsprojPath with:"
Write-Host "  <GamePath>$GamePath</GamePath>"
Write-Host "  <GameModsPath>$GameModsPath</GameModsPath>"
Write-Host "`nNext: cd StardewModListExporter; dotnet build"
