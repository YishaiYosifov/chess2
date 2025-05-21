# Set the path to your C# project directory
$projectDirectory = "."

# Get all C# files in the project directory (recursively)
$files = Get-ChildItem -Recurse -Filter *.cs -Path $projectDirectory

# Iterate through each file and compare the namespace
foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $namespaceLine = $content | Where-Object { $_ -match "namespace\s+" }
    
    if ($namespaceLine) {
        # Extract the namespace (assuming no additional whitespace issues)
        $namespace = $namespaceLine -replace "namespace\s+", ""
        
        # Get the directory structure
        $relativePath = $file.FullName.Substring($projectDirectory.Length + 1)  # Relative path from the project root
        $relativeNamespace = $relativePath.Replace("\", ".").Replace(".cs", "")
        
        # Compare namespace and directory structure
        if ($namespace -ne $relativeNamespace) {
            Write-Host "Mismatch found: $($file.FullName) - Namespace: $namespace, Directory: $relativeNamespace"
        }
    }
}