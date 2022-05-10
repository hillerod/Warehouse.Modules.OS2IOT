using System;
using System.Collections.Generic;

/// <summary>
/// A web service class fetches data from external data sources like web service, Rest API, mail server, FTP and os on
/// </summary>
namespace Module.Services
{
    public class WebService
    {
        public static string[][] GetData()
        {
            return new List<string[]>
            {
                new string[] { "1", "Some static text", DateTime.Now.ToString("s") },
                new string[] { new Random().Next(2, 1000).ToString(), "Some text with a random ID", DateTime.Now.ToString("s") }
            }.ToArray();
        }
    }
}
