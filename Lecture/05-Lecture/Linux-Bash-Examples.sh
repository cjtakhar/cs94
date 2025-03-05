### Begin: Install Azure CLI
# Update
sudo apt-get update

# Install dependencies for the Azure CLI
sudo apt-get install ca-certificates curl apt-transport-https lsb-release gnupg

# Download and install the Microsoft signing key
curl -sL https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor | sudo tee /etc/apt/trusted.gpg.d/microsoft.gpg > /dev/null

# Add the Azure CLI repository information
AZ_REPO=$(lsb_release -cs); echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $AZ_REPO main" | sudo tee /etc/apt/sources.list.d/azure-cli.list

# Update the package list again
sudo apt-get update

# Install the Azure CLI
sudo apt-get install azure-cli
### End: Install Azure CLI

# Install CIFS
sudo apt install cifs-utils

# Verify configuration
sudo bash AzFileDiagnostics.sh

# create the mount point
sudo mkdir /home/jficara/azureshare01

# Mount an Azure Share
sudo mount -t cifs //stcscie94demo.file.core.windows.net/share01  azureshare01 -o vers=3.1.1,username=stcscie94demo,password='<storage access key>',dir_mode=0777,file_mode=0777,nosharesock,actimeo=30,sec=ntlmssp