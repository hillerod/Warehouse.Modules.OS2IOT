//using Bygdrift.CsvTools;
//using Bygdrift.DataLakeTools;
//using Bygdrift.Warehouse;
//using System;
//using System.Threading.Tasks;

///// <summary>
///// A refine file, refines incomming data and save it to the public property 'Csv'
///// Afterwards, it describes, how to save the csv to datalake and/or database
///// </summary>
//namespace Module.Refines
//{
//    public class DataRefine
//    {
//        public static async Task Refine(AppBase<Settings> app, string[][] data)
//        {
//            app.Log.LogInformation("Refining data...");
//            //Save raw data to dataLake so you always can go back:
//            var dataAsString = dataToString(data);
//            await app.DataLake.SaveStringAsync(dataAsString, "Raw", "Data.txt", FolderStructure.DatePath);
            
//            //Refine data
//            var csv = CreateCsv(data, app.LoadedLocal, app.Settings.DataFromSetting, app.Settings.DataFromSecret);

//            //Saves refined as Data.csv to the datalake in a folder called Refined
//            await app.DataLake.SaveCsvAsync(csv, "Refined", "Data.csv", FolderStructure.DatePath);

//            //Updates data into database:
//            app.Mssql.MergeCsv(csv, "Data", "Id", false, false);

//            //Saves data to database accumulated:
//            app.Mssql.InserCsv(csv, "DataAccumulated", false, false);
//        }

//        private static string dataToString(string[][] data)
//        {
//            var res = "";
//            foreach (var row in data)
//                res += string.Join(",", row) + "\n";

//            return res;
//        }

//        /// <summary>Data gets washed (a date gets added) and saved to Csv</summary>
//        private static Csv CreateCsv(string[][] data, DateTime appLoaded, string dataFromSetting, string dataFromSecret)
//        {
//            var csv = new Csv("Id, Text, Date, DataFromSetting, DataFromSecret");  //Csv is a part of RefineBase and consumes data as csv. There are many methods that can be used with Csv
//            for (int row = 0; row < data.Length; row++)
//                csv.AddRow(data[row][0], data[row][1], appLoaded, dataFromSetting, dataFromSecret);

//            return csv;
//        }
//    }
//}