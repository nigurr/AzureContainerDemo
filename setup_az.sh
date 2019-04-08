#!/bin/bash

<<'COMMENT'

Functions
	1. Create Resource Group
	2. Create Storage Account
	3. Create Container Registry
	4. Create Key Vault
	5. Start Container with ACR image
	6. Delete Containers & registry
	7. Delete Resource Group
COMMENT

# user command
command=$1
# User input values
DOCKER_FILE=$2


## Universal vairables
AZURE_RESOURCE_GROUP="onebranchrg"
AZURE_LOCATION="eastus"
AZURE_STORAGE_ACCOUNT="onebranchstorage"
AZURE_SHARE_NAME="onebranchstorageshare"
AZURE_STORAGE_CONTAINER="onebranchstoragecontainer"
AZURE_CONTAINER_REGISTRY="onebranchacr"
AZURE_STORAGE_ACCESS_KEY=""
AZURE_STORAGE_SAS=""
AZURE_KEY_VAULT="onebranchkeyvault"

if [ "$1" = "create_container_registry" ]; then
	function_create_container_registry
elif [ "$1" = "create_container" ]; then
	function_create_docker_container
elif [ "$1" = "delete_container" ]; then
	function_delete_containers
fi


function_create_resource_group() {
	# 1. Creating the resource group
	echo "\n Create resource group $AZURE_RESOURCE_GROUP"
	az group create -n $AZURE_RESOURCE_GROUP -l $AZURE_LOCATION
}

function_create_storage_account() {
	# 2. create storage account
	echo "\n Creating the storage account"
	az storage account create --name $AZURE_STORAGE_ACCOUNT --resource-group $AZURE_RESOURCE_GROUP --location $AZURE_LOCATION --sku Standard_LRS
	
	echo "\n Getting the storage access key"
	AZURE_STORAGE_ACCESS_KEY=$(az storage account keys list --resource-group $AZURE_RESOURCE_GROUP --account-name $AZURE_STORAGE_ACCOUNT --query "[0].value" --output tsv)

	echo "\n Creating the storage container"
	az storage container create --name $AZURE_STORAGE_CONTAINER --account-name $AZURE_STORAGE_ACCOUNT --account-key $AZURE_STORAGE_ACCESS_KEY

	echo "\n Creating the shared drive for storage"
	az storage share create --name $AZURE_SHARE_NAME --account-name $AZURE_STORAGE_ACCOUNT

	echo "\n Generating the SAS token for accessing storage"
	end=`date -v+90M '+%Y-%m-%dT%H:%MZ'`
	AZURE_STORAGE_SAS=$(az storage account generate-sas --permissions cdlruwap --account-name $AZURE_STORAGE_ACCOUNT --services bfqt --resource-types sco --expiry $end -otsv | sed 's/\"//g')
}

function_create_container_registry() {
	# 3. create container registry
	echo "\n Creating the container registry"
	az acr create -g $AZURE_RESOURCE_GROUP -n $AZURE_CONTAINER_REGISTRY --sku Basic --admin-enabled

	# build image @container registry
	dockerimage="microsoft:dotnetcore"

	echo "\n Building docker image $dockerimage from $DOCKER_FILE"
	az acr build -r $AZURE_CONTAINER_REGISTRY -f $DOCKER_FILE -t $dockerimage .

	# get list of images
	echo "\n Listing out the available images"
	az acr repository list -n $AZURE_CONTAINER_REGISTRY
}

function_create_keyvault(){
	# 4. Create key vault for storing secrets -> this can be resused for accessing container images

	echo "\n Creating the azure key vault"
	az keyvault create -g $AZURE_RESOURCE_GROUP -n $AZURE_KEY_VAULT

	echo "\n Generating the container image secret"
	az keyvault secret set \
	--vault-name $AZURE_KEY_VAULT \
  	--name $AZURE_CONTAINER_REGISTRY-pull-pwd \
  	--value $(az ad sp create-for-rbac \
	          	--name http://$AZURE_CONTAINER_REGISTRY-pull \
	            --scopes $(az acr show --name $AZURE_CONTAINER_REGISTRY --query id --output tsv) \
	            --role acrpull \
	            --query password \
	            --output tsv)

	echo "\n Generating the container image username"
	az keyvault secret set \
    --vault-name $AZURE_KEY_VAULT \
    --name $AZURE_CONTAINER_REGISTRY-pull-usr \
    --value $(az ad sp show --id http://$AZURE_CONTAINER_REGISTRY-pull --query appId --output tsv)
}

function_create_docker_container() {
	# 4. Start containers

	dockerimage="microsoft:dotnetcore"
	containername="samplenetcore"

	echo "\n Bringing up container instance"
	# create a new ACI instance to run this container
	az container create \
    --resource-group $AZURE_RESOURCE_GROUP \
    --name $containername \
    --image $AZURE_CONTAINER_REGISTRY.azurecr.io/$dockerimage \
    --registry-username $(az keyvault secret show --vault-name $AZURE_KEY_VAULT -n $AZURE_CONTAINER_REGISTRY-pull-usr --query value -o tsv) \
    --registry-password $(az keyvault secret show --vault-name $AZURE_KEY_VAULT -n $AZURE_CONTAINER_REGISTRY-pull-pwd --query value -o tsv) \
    --dns-name-label $containername \
    --ports 22 \
    --azure-file-volume-account-name $AZURE_STORAGE_ACCOUNT \
    --azure-file-volume-account-key $AZURE_STORAGE_ACCESS_KEY \
    --azure-file-volume-share-name $AZURE_SHARE_NAME \
    --azure-file-volume-mount-path /share \
    --query ipAddress.fqdn \

    echo "\n Getting info from container........."
    az container logs --resource-group $AZURE_RESOURCE_GROUP --name $containername
}

function_delete_containers() {
	az container delete --resource-group $AZURE_RESOURCE_GROUP --name "samplenetcore" --yes
	az acr repository delete --resource-group $AZURE_RESOURCE_GROUP --name $AZURE_CONTAINER_REGISTRY --repository microsoft --yes
}

function_delete_resource_group() {
	az group delete -n $AZURE_RESOURCE_GROUP --yes
}
