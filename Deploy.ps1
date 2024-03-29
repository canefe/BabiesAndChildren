# Configurable variables
$repo           = './'
$packing        = 'release'
$outputFormat   = Join-Path -Path '\..' -ChildPath 'BabiesAndChildren-{0}.zip'
$internalPath   = 'BabiesAndChildren'
$pathsToRemove = '.git', '.gitattributes', '.gitignore'

[Console]::ResetColor()

# Progress Bar Variables
$Activity             = "Deploying"
$Id                   = 1

# Complex Progress Bar
$Step                 = 0
$TotalSteps           = 6 
$StatusText           = '"Step $($Step.ToString().PadLeft($TotalSteps.ToString().Length)) of $TotalSteps | $Task"' # Single quotes need to be on the outside
$StatusBlock          = [ScriptBlock]::Create($StatusText) # This script block allows the string above to use the current values of embedded values each time it's run

# Read environvemt
$Task = "Collecting info..."
$Step++
##Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -CurrentOperation " " -PercentComplete ($Step / $TotalSteps * 100)

$startupPath = Get-Location
if ($IsWindows) {
	$7z = (GET-ItemProperty 'HKLM:\SOFTWARE\7-Zip').Path + '7z.exe'
}
else {
	$7z = &which 7z
}

$packingRjw = Join-Path -Path $packing -ChildPath $internalPath

Push-Location -Path $repo

try {
	[string]$version = git describe --tags
} catch {
	[string]$version = ""
}

if ($version -eq "") {
	$manifest = Join-Path -Path '.\About' -ChildPath 'Manifest.xml'
	[string]$version = (Select-Xml -Path $manifest -XPath '/Manifest/version' | Select-Object -ExpandProperty Node).innerText
}

$output = $outputFormat -f $version

Pop-Location

# Cleanup
$Task = "Cleanup..."
$Step++
Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -CurrentOperation " " -PercentComplete ($Step / $TotalSteps * 100)

if (Test-Path $packing) { Remove-Item -Recurse -Force $packing }
if (Test-Path $output) { Remove-Item $output }

# Prepating data
$Task = "Copying..."
$Step++
Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -CurrentOperation " " -PercentComplete ($Step / $TotalSteps * 100)

$items = Get-ChildItem -Force -Path $repo
$items | Foreach-Object -Begin { 
	$i = 0
} -Process {
    $i++
	
	if (-Not ($pathsToRemove -contains $_.Name)) {
		Copy-Item -Recurse $_.FullName -Destination (Join-Path -Path $packingRjw -ChildPath $_.Name)
	}
	
	$p = $i * 100 / $items.Count
	Write-Progress -Id ($Id+1) -ParentId $Id -Activity 'Copying' -Status ('{0:0}% complete' -f $p) -PercentComplete $p
}
Write-Progress -Id ($Id+1) -ParentId $Id -Activity "Copying" -Status "Ready" -Completed

# removing files from subfolders
$Task = "Excluding..."
$Step++
Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -CurrentOperation " " -PercentComplete ($Step / $TotalSteps * 100)

foreach ($path in $pathsToRemove) {
	$p = Join-Path -Path $packingRjw -ChildPath $path
	if (Test-Path $p) { Remove-Item -Recurse -Force $p }	
}

# archiving
$Task = "Archiving..."
$Step++
Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -CurrentOperation " " -PercentComplete ($Step / $TotalSteps * 100)

Push-Location -Path  $packing
& $7z a -r -bsp1 (Join-Path -Path $startupPath.Path -ChildPath $output) + $internalPath | Foreach-Object -Begin {
	Write-Progress -Id ($Id+1) -ParentId $Id -Activity 'Archiving' -Status "Starting..." -PercentComplete 0
} -Process {
	$line = $_.Trim()
	if ($line -ne "") {
		[int]$p = 0
		[bool]$result = [int]::TryParse($line.Split('%')[0], [ref]$p)
		if ($result) {
			Write-Progress -Id ($Id+1) -ParentId $Id -Activity 'Archiving' -Status $line -PercentComplete $p
		}
	}
}

Write-Progress -Id ($Id+1)  -Activity "Archiving" -Status "Ready" -Completed
Pop-Location


# cleanup
$Task = "Cleanup..."
$Step++
Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -CurrentOperation " " -PercentComplete ($Step / $TotalSteps * 100)

if (Test-Path $packing) { Remove-Item -Recurse -Force $packing }

Write-Progress -Id $Id -Activity $Activity -Status (& $StatusBlock) -Completed

# If running in the console, wait for input before closing.
if ($Host.Name -eq "ConsoleHost")
{
	Write-Host "Done"
    Write-Host "Press any key to continue..."
    $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyUp") > $null
}
