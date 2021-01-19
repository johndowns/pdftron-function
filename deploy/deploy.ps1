param(
    [Parameter(Mandatory)]
    [string]
    $ResourceGroupName,

    [Parameter(Mandatory)]
    [string]
    $ResourceGroupLocation
)

# Cause all errors to halt the script.
$ErrorActionPreference = 'Stop' 

Write-Host "Creating resource group $ResourceGroupName in location $ResourceGroupLocation."
New-AzResourceGroup -Name $ResourceGroupName -Location $ResourceGroupLocation -Force

Write-Host 'Starting deployment of ARM template.'
$templateFilePath = Join-Path $PSScriptRoot 'template.json'
$deploymentOutputs = New-AzResourceGroupDeployment -ResourceGroupName $ResourceGroupName -TemplateFile $templateFilePath
$functionAppName = $deploymentOutputs.Outputs.functionsAppName.value

Write-Host 'Waiting 60 seconds for function app to become ready for deployment.'
Start-Sleep -Seconds 60

Write-Host "Deploying to Azure Functions app $functionAppName."
$functionAppFolder = Join-Path $PSScriptRoot '..' 'src' 'FunctionPdf'
Push-Location $functionAppFolder
func azure functionapp publish $functionAppName
Pop-Location
