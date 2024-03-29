# Use ARM templates to setup and maintain this module

## Prerequisites

If you don't have an Azure subscription, create a free [account](https://azure.microsoft.com/free/?ref=microsoft.com&utm_source=microsoft.com&utm_medium=docs&utm_campaign=visualstudio) before you begin.

Install the [Warehouse environment](https://github.com/Bygdrift/Warehouse/tree/master/Deploy), before installing this or any module.

## Videos

2022-01-28: [Setup the Example module](https://www.youtube.com/watch?v=itwd2XdHIkM) (in Danish):

2022-01-28: [Update an already installed module, once a new update has been pushed to GitHub](https://www.youtube.com/watch?v=XywfV_n-320) (in Danish):

## Add this module through Azure Portal (easiest):

[![Deploy To Azure](https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/Images/deploytoazureButton.svg)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fhillerod%2FWarehouse.Modules.OS2IOT%2Fmaster%2FDeploy%2FWarehouse.Modules.OS2IOT_ARM.json)
[![Visualize](https://raw.githubusercontent.com/Bygdrift/Warehouse/master/Docs/Images/visualizebutton.svg)](http://armviz.io/#/?load=https%3A%2F%2Fraw.githubusercontent.com%2Fhillerod%2FWarehouse.Modules.OS2IOT%2Fmaster%2FDeploy%2FWarehouse.Modules.OS2IOT_ARM.json)

This will setup a Windows hosting plan and a function app that contains the software from this GitHub repository.

If you have to change some settings, you can run the setup again, and it should not affect data, but better take backup to be sure.

## Alternative: Add this module with Azure CLI

You can also run the ARM from PowerShell.

Either run the PowerShell from computer by installing [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli), or use the [Azure Cloud Shell](https://shell.azure.com/bash) from the Azure portal. This instruction will focus on the run from a computer.

Download this [warehouse_ARM.parameters.json](https://raw.githubusercontent.com/hillerod/Warehouse.Modules.OS2IOT/master/Deploy/Warehouse.Modules.OS2IOT_ARM.parameters.json) to a folder and carefully fill in each variable.

Download [warehouse_ARM.json](https://raw.githubusercontent.com/hillerod/Warehouse.Modules.OS2IOT/master/Deploy/Warehouse.Modules.OS2IOT_ARM.json) to the same folder.

Login to azure: `az login`.

And then: `az deployment group create -g <resourceGroupName> --template-file ./Warehouse.Modules.OS2IOT_ARM.json --parameters ./Warehouse.Modules.OS2IOT_ARM.parameters.json`

Replace `<resourceGroupName>` with actual name.

I personally prefer to use CLI so I can collect all parameters in one json.