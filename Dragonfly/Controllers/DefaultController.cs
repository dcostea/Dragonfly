using Dragonfly.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dragonfly.Models;
using System.IO;
using System.Linq;
using System.Text;
using GeneratedDataModels;
using System;

namespace Dragonfly.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        private readonly ILogger<DefaultController> _logger;
        private readonly string _modelPath;

        public DefaultController(ILogger<DefaultController> logger, Settings settings)
        {
            _logger = logger;
            _modelPath = settings.MLModelPath;
        }

        [HttpGet]
        public IActionResult GetMLModel()
        {
            var zip = StreamHelper.GetZipFileStream(_modelPath);
            using var reader = new BinaryReader(zip, Encoding.UTF8);
            var features = StreamHelper.ExtractFeatures(reader);

            return Ok(features);
        }

        [HttpGet("modeloutput/file")]
        public IActionResult GetModelOutputSchema()
        {
            var path = $"{GetGeneratedFilesPath()}ModelOutput.cs";
            string readText = System.IO.File.ReadAllText(path);

            return Ok(readText);
        }

        [HttpGet("modelinput/file")]
        public IActionResult GetModelInputFile()
        {
            var path = $"{GetGeneratedFilesPath()}ModelInput.cs";
            string readText = System.IO.File.ReadAllText(path);

            return Ok(readText);
        }

        [HttpGet("modelinput/schema")]
        public IActionResult GetModelInputSchema()
        {
            var properties = typeof(ModelInput).GetProperties().Select(p => p.Name);

            return Ok(properties);
        }

        [HttpGet("predictor/file")]
        public IActionResult GetMLPredictor()
        {
            var path = $"{GetGeneratedFilesPath()}Predictor.cs";
            string readText = System.IO.File.ReadAllText(path);

            return Ok(readText);
        }

        [HttpGet("program/file")]
        public IActionResult GetMLProgram()
        {
            var path = $"{GetGeneratedFilesPath()}Program.cs";
            string readText = System.IO.File.ReadAllText(path);

            return Ok(readText);
        }

        [HttpPost("predict")]
        public IActionResult Predict(ModelInput sample)
        {
            // calling the generated Predictor class
            var prediction = GeneratedDataModels.Predictor.Predict(sample);

            return Ok(prediction);
        }

        private string GetGeneratedFilesPath()
        {
            var binPath = AppDomain.CurrentDomain.BaseDirectory;
            var filePath = binPath.Substring(0, binPath.LastIndexOf("bin"));

            return @$"{filePath}obj\GeneratedMLNETFiles\MLCodeGenerator\MLCodeGenerator.DataModelsGenerator\";
        }
    }
}
