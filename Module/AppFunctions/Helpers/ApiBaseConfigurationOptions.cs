﻿using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;

//https://github.com/Azure/azure-functions-openapi-extension/blob/main/docs/openapi-core.md
namespace Module.AppFunctions.Helpers
{
    public class ApiBaseConfigurationOptions : IOpenApiConfigurationOptions
    {
        public OpenApiInfo Info { get; set; } = new OpenApiInfo()
        {
            Version = "1.0.0",
            Title = "OpenAPI for OS2IOT communication",
            Description = "",
            TermsOfService = new Uri("https://github.com/Azure/azure-functions-openapi-extension"),
            Contact = new OpenApiContact()
            {
                Name = "Kenneth Christensen ",
                Email = "kench@hillerod.dk",
                Url = new Uri("https://github.com/hillerod/Warehouse.Modules.OS2IOT"),
            },
            License = new OpenApiLicense()
            {
                Name = "MIT",
                Url = new Uri("https://github.com/hillerod/Warehouse.Modules.OS2IOT/blob/master/License.md"),
            }
        };

        public List<OpenApiServer> Servers { get; set; } = new List<OpenApiServer>();

        public OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V2;

        public bool IncludeRequestingHostName { get; set; } = false;
        public bool ForceHttp { get; set; } = false;
        public bool ForceHttps { get; set; } = false;
        public List<IDocumentFilter> DocumentFilters { get; set; }
    }
}
