
# Verify configuration
.\AzFileDiagnostics.ps1

# Mount an azure share as drive Y
net use Q: \\stcscie94demo.file.core.windows.net\share01 /User:Azure\stcscie94demo <storage account key> /persistent:yes