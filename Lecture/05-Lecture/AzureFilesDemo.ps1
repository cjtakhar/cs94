# See common parameters info: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_commonparameters?view=powershell-6
# create a context for account and key (Account and Key will be prompted fo)
$ctx=New-AzureStorageContext -StorageAccountName stcscie94demo 

# Get the share named share01
$s = Get-AzureStorageShare -Name share01 -context $ctx

# Check if the directory exists
$dir = Get-AzureStorageFile -Share $s -Path ClassDemo -ErrorAction Ignore 

# If directory does not exist create it
if ($dir.Name -ne 'ClassDemo')
{
    # create a directory in the share
    New-AzureStorageDirectory -Share $s -Path ClassDemo
}

# Check if file exists
$file = Get-AzureStorageFile -Share $s -Path ClassDemo\HelloFile.txt -ErrorAction Ignore

# If file does not exist create it
if ($file.Name -ne 'HelloFile.txt')
{
    # upload a local file to the new directory
    Set-AzureStorageFileContent -Share $s -Source D:\Harvard2025\SampleFiles\HelloFile.txt -Path ClassDemo
}

# list files in the new directory 
Get-AzureStorageFile -Context $ctx -ShareName $s.Name -Path ClassDemo | Get-AzureStorageFile
 
# download the HelloFile.txt file
 Get-AzureStorageFileContent -Context $ctx -ShareName $s.Name -Path "ClassDemo/HelloFile.txt"
