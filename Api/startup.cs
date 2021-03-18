using System;
using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.ML;
using BlazorApp.Api;


[assembly: FunctionsStartup(typeof(Startup))]
namespace BlazorApp.Api
{
    public class Startup : FunctionsStartup
    {
        private  string _environment;
        private  string _modelPath;

        public Startup()
        {
            _environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
            Console.WriteLine($"_environment {_environment}");
        }
        

        public override void Configure(IFunctionsHostBuilder builder)
        {
            string uri = Environment.GetEnvironmentVariable("MODELURI");
            Console.WriteLine($"model uri {uri}");
            builder.Services.AddPredictionEnginePool<OnnxInput, OnnxOutput>()
                    .FromUri(modelName: "RomComModel", uri);
                    
        }
    }
}