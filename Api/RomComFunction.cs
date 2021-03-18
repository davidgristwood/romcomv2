using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.ML;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using BlazorApp.Shared;

namespace BlazorApp.Api
{
    public class OnnxInput
    {
        [ColumnName("Text_input")]
        public string Text_input { get; set; }
    }

    public class OnnxOutput
    {
        [ColumnName("output_label")]
        public string[] output_label { get; set; }

        [ColumnName("output_probability")]
        [OnnxSequenceType(typeof(IDictionary<string, float>))]
        public IEnumerable<IDictionary<string, float>> output_probability { get; set; }
    }


    // return format from Function call
    public class GenreResults
    {
        public string genre;
        public float probability; 
    }


    public class Analyze
    {
        private readonly PredictionEnginePool<OnnxInput, OnnxOutput> _predictionEnginePool;

        public Analyze(PredictionEnginePool<OnnxInput, OnnxOutput> predictionEnginePool)
        {
            _predictionEnginePool = predictionEnginePool;
        }

        [FunctionName("Predict")]
        public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {
            log.LogInformation("** C# HTTP trigger function processed a request.");

            // Parse HTTP Request Body
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"** Run predict on {requestBody}");

            // convert free format text to model input
            var moviescript = new OnnxInput
            {
                Text_input = requestBody
            };

            // Make Prediction
            OnnxOutput prediction = _predictionEnginePool.Predict(modelName: "RomComModel", example: moviescript);
            log.LogInformation($"Predicted type: {prediction.output_label[0]}");

            // parse prediction to human readable output
            string[] filmTypes = new string[]
            {
                "RomCom",
                "Horror",
                "Heist",
                "Comedy",
                "Sci-Fi"
            };

            List<GenreResults> genreResults = new List<GenreResults>(); // output

            foreach (var predictions in prediction.output_probability)
            {
                // only first entry contains useful data
                int i = 0;
                foreach (var p in predictions)
                {
                    // out line to output
                    genreResults.Add(new GenreResults() 
                            { 
                                genre = filmTypes[i].ToString(),  
                                probability = p.Value
                            });
                    i++;
                }
            }
            string json = JsonConvert.SerializeObject(genreResults);
            log.LogInformation($"**  Predicts are {json}");

            return new OkObjectResult(json);
        }
    }
}