# Warehouse module: OS2IOT

## Introduction

With this module, you can easily setup an environment in Azure, to consume data from OS2IOT.
![The flow](https://raw.githubusercontent.com/hillerod/Warehouse.Modules.OS2IOT/master/Docs/Images/setup-in-azure-and-os2iot.drawio.png)
 
It can fetch data one or multiple times each day, from OS2IOT's API and save them in the database.
It also fetches data from sensors. In a later version, data will also be refined into data per hour, data per day and data per month.

The module is build with [Bygdrift Warehouse](https://github.com/Bygdrift/Warehouse), that enables one to attach multiple modules within the same azure environment, that can collect and wash data from all kinds of services, in a cheap data lake and database.
By saving data to a MS SQL database, it is:
- easy to fetch data with Power BI, Excel and other systems
- easy to control who has access to what - actually, it can be controlled with AD so you don't have to handle credentials
- It's cheap

## Installation

All modules can be installed and facilitated with ARM templates (Azure Resource Management): [Use ARM templates to setup and maintain this module](https://github.com/hillerod/Warehouse.Modules.OS2IOT/blob/master/Deploy).


## Database content

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

Data from the OS2IOT API:
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
- none yet

# License

[MIT License](https://github.com/Bygdrift/Warehouse.Modules.Example/blob/master/License.md)
