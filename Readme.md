# Warehouse module: OS2IOT

## Introduction

With this Azure module, you can easily setup an environment in Azure, to consume data from OS2IOT.
 
It can fetch data one or multiple times each day, from OS2IOT's API and save them in the database.
It also fetches data from sensors. In a later version of this module, data will also be refined into data per hour, data per day and data per month.

The module is build with [Bygdrift Warehouse](https://github.com/Bygdrift/Warehouse), that enables one to attach multiple modules within the same azure environment, that can collect and wash data from all kinds of services, in a cheap data lake and database.
By saving data to a MS SQL database, it is:
- easy to fetch data with Power BI, Excel and other systems
- easy to control who has access to what - actually, it can be controlled with AD so you don't have to handle credentials
- It's cheap

## How it works

![The flow](https://raw.githubusercontent.com/hillerod/Warehouse.Modules.OS2IOT/master/Docs/Images/setup-in-azure-and-os2iot.drawio.png)

### OS2IOT's API

In OS2IOT, there is an [API](https://backend.os2iot.gate21.dk/api/v1/docs/#/) that contains a lot of useful data that gets fetched each day from this module and saved into the database. Under [Database content](#Data-from-the-OS2IOT-API), there is a list of data that gets loaded.
When installing this module, you can define how often the API should be called by the parameter `OS2IOTApiScheduleExpression = 0 0 1 * * *` That is each day 1AM UTC time. [Read about how to change it](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer?tabs=csharp#ncrontab-expressions).

### Getting payloads

Data from the sensors gets send to the server and then transferred to the OS2IOT server as a payload. These packages are compressed to be as tiny as possible, so In OS2IOT, technical users can setup their own payload decoders, or use the library of decoders, to decompress the payloads.

OS2IOT then has a HTTP Push service that pushes the decoded payloads out to a receiver. To set OS2IOT up to send data to the Azure module, you need to set up a data-target in OS2IOT with a datatarget-URL and an Authorization header.
The URL is a path to the `PostPayloads` function, that's like: `https://os2iot-v3xshh3xyxllq.azurewebsites.net/api/PostPayloads`.
You defined The Authorization header when installing this Azure module. It can be changed At the Azure Functions under Configuration where it has the name: `PostPayloadsAuthorizationKey`.

Payloads are received and saved directly down to the data lake as queue massages.

Each 5 minute another function called `IngestQueuedPayloads`, asks for all messages in the data lake and converts them to a CSV-stream (a comma separated file-format). The CSV gets saved into the data lake for history purpose, and the csv gets send to the database in one bulk.
If data contains column names that are not defined in the database, new columns will be added. And if the datatype has been changed on an already created column, the datatype will be updated.

All the ingested queue messages from the data lake gets deleted.
If the function `IngestQueuedPayloads` or the database is not working properly, messages will be saved for up to 7 days before the oldest will expire. So if the database has been down for 2 days, no data will be lost.

### Cleaning data lake

The function `CleanDataLake` gets called each day and deletes all data from the data lake that's older than 6 months.
The amount of months to go back, is defined when installing the Azure module as `MonthsToKeepDataInDataLake = 6`. This setting can later be changed under the Azure Functions `Configuration`.

## Contact

For information or consultant hours, please write to bygdrift@gmail.com.

## Videos

How this module is used in Hillerød Municipality and how it's installed (remember first to install the [Bygdrift Warehouse base module](https://github.com/Bygdrift/Warehouse)):
<div align="left">
      <a href="https://www.youtube.com/watch?v=jVe_HdMg5R8">
         <img src="https://img.youtube.com/vi/jVe_HdMg5R8/0.jpg">
      </a>
</div>

2022-05-27: [Detailed description of how this module fetches data from OS2IOT (in Danish)](https://youtu.be/cuDi3phDrAU)

2022-05-26: [How to update the Azure module if a new version is released (in Danish)](https://youtu.be/esQRiZJ81_M)

## Installation

All modules can be installed and facilitated with ARM templates (Azure Resource Management): [Use ARM templates to setup and maintain this module](https://github.com/hillerod/Warehouse.Modules.OS2IOT/blob/master/Deploy).

## Database content

### Data From payloads

Data from payloads are saved exactly as the payload decoder in OS2IOT describes. If data comes from multiple decoders, data is saved into columns with similar names, and if columns isn't present, they will be added:

| TABLE_NAME         | COLUMN_NAME             | DATA_TYPE |
| :----------------- | :---------------------- | :-------- |
| Payloads           | temperature             | varchar   |
| Payloads           | humidity                | bigint    |
| Payloads           | light                   | bigint    |
| Payloads           | motion                  | bigint    |
| Payloads           | co2                     | bigint    |
| Payloads           | vdd                     | bigint    |
| Payloads           | deviceId                | varchar   |
| Payloads           | location.type           | varchar   |
| Payloads           | location.coordinates    | varchar   |
| Payloads           | commentOnLocation       | varchar   |
| Payloads           | name                    | varchar   |
| Payloads           | timeStamp               | datetime  |

### Data from the OS2IOT API

| TABLE_NAME         | COLUMN_NAME             | DATA_TYPE |
| :----------------- | :---------------------- | :-------- |
| Organizations      | id                      | int       |
| Organizations      | name                    | varchar   |
| Organizations      | applicationIds          | int       |
| Organizations      | createdAt               | datetime  |
| Organizations      | updatedAt               | datetime  |
| ChirpstackGateways | id                      | varchar   |
| ChirpstackGateways | name                    | varchar   |
| ChirpstackGateways | description             | varchar   |
| ChirpstackGateways | createdAt               | datetime  |
| ChirpstackGateways | updatedAt               | datetime  |
| ChirpstackGateways | firstSeenAt             | varchar   |
| ChirpstackGateways | lastSeenAt              | varchar   |
| ChirpstackGateways | organizationID          | int       |
| ChirpstackGateways | networkServerID         | int       |
| ChirpstackGateways | location.latitude       | real      |
| ChirpstackGateways | location.longitude      | real      |
| ChirpstackGateways | location.altitude       | int       |
| ChirpstackGateways | location.source         | varchar   |
| ChirpstackGateways | location.accuracy       | int       |
| ChirpstackGateways | networkServerName       | varchar   |
| ChirpstackGateways | internalOrganizationId  | int       |
| Applications       | id                      | int       |
| Applications       | name                    | varchar   |
| Applications       | description             | varchar   |
| Applications       | createdAt               | datetime  |
| Applications       | updatedAt               | datetime  |
| DeviceModels       | id                      | int       |
| DeviceModels       | name                    | varchar   |
| DeviceModels       | type                    | varchar   |
| DeviceModels       | category                | varchar   |
| DeviceModels       | brandName               | varchar   |
| DeviceModels       | modelName               | varchar   |
| DeviceModels       | manufacturerName        | varchar   |
| DeviceModels       | controlledProperties    | varchar   |
| DeviceModels       | createdAt               | datetime  |
| DeviceModels       | updatedAt               | datetime  |
| IotDevices         | id                      | int       |
| IotDevices         | applicationId           | int       |
| IotDevices         | deviceModelId           | int       |
| IotDevices         | chirpstackApplicationId | int       |
| IotDevices         | name                    | varchar   |
| IotDevices         | deviceEUI               | varchar   |
| IotDevices         | comment                 | varchar   |
| IotDevices         | commentOnLocation       | varchar   |
| IotDevices         | metadata                | varchar   |
| IotDevices         | latitude                | real      |
| IotDevices         | longitude               | real      |
| IotDevices         | lastRecievedMessageTime | datetime  |
| IotDevices         | loraActivationType      | varchar   |
| IotDevices         | loraBatteryStatus       | int       |
| IotDevices         | createdAt               | datetime  |
| IotDevices         | updatedAt               | datetime  |
| IotDevices         | createdById             | int       |
| IotDevices         | updatedById             | int       |


## Data lake content

When installing the OS2IOT-module, it i possible to select, for how long this data should be saved. As a standard, it is set for 6 months.

In the data lake container with this modules name, there are three main folders:
- `ApiRaw` contains all the API-calls as json.
- `ApiRefined` contains all the API-calls as json.
- `PayloadRefined` contains all payloads, cleaned and saved into csv-files


 The folder structure:

+ ApiRaw
    - {yyyy the year}
        - {MM the month}
            - {dd the day}
                - Organizations.json
                - ChirpstackGateways.json
                - Applications.json
                - DeviceModels.json
                - IotDevices.json
+ ApiRefined
    - {yyyy the year}
        - {MM the month}
            - {dd the day}
                - Organizations.csv
                - ChirpstackGateways.csv
                - Applications.csv
                - DeviceModels.csv
                - IotDevices.csv
+ PayloadRefined
    - {yyyy the year}
        - {MM the month}
            - {dd the day}
                - 2022-05-23-00.30.00_payload_9vCnZnkBUi.json
                - 2022-05-23-00.34.59_payload_ugf7Kqa4z0.json

Messages are also saved into the data lake Queues: `os2iot-payloads`, containing payloads, ready to be refined and loaded into the database. If the modules stops working by an accident, data will continue to be stacked up as messages for up to seven days.

# Updates
- 0.4.0: Added OpenAPI so it is easy for a company to hook up directly to this service and fetch message queues directly from DataLake instead of getting data from database

# License

[MIT License](https://github.com/Bygdrift/Warehouse.Modules.Example/blob/master/License.md)
