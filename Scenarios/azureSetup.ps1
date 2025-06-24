#Setting variables
$resourceGroup="Az204Groupgs-$(Get-Random -Maximum 100)"
echo $resourceGroup
$storageAccount="az204storagegs$(Get-Random -Maximum 100)" #lowername
$functionAppName = "Az204Funcgs-$(Get-Random -Maximum 100)" 
$webAppName = "Az204Webgs-$(Get-Random -Maximum 100)" 

$location = "northeurope" #az functionapp list-flexconsumption-locations --output table

$dotnetVersion = "9.0"
$osType = "Windows"
$runtime = "dotnet"
$functionFlexConsumption = "Y1"


New-AzResourceGroup -Name $resourceGroup -Location $location


New-AzStorageAccount `
    -ResourceGroupName $resourceGroup `
    -Name $storageAccount `
    -Location $location `
    -SkuName "Standard_LRS" `
    -Kind "StorageV2"

# Get-Help new-AzAppServicePlan
# (Get-Command New-AzAppServicePlan).Parameters 
# (Get-Command New-AzAppServicePlan).Parameters["Tier"].Attributes - Getting info on attributes
New-AzAppServicePlan -ResourceGroupName $resourceGroup -Name "webappPlan" -Location $location -Tier "Free"

# Get-Help New-AzWebApp 
# Get-Help New-AzWebApp -Examples
New-AzWebApp -Location $location -ResourceGroupName $resourceGroup -Name $webAppName -AppServicePlan "webappPlan"