using Dragonfly.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Dragonfly.Models;
using System.IO;
using System.Linq;
using System.Text;
using GeneratedDataModels;

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
            string readText = System.IO.File.ReadAllText(@"c:\repos\Dragonfly\Dragonfly\obj\GeneratedMLNETFiles\MLCodeGenerator\MLCodeGenerator.DataModelsGenerator\ModelOutput.cs");

            return Ok(readText);
        }

        [HttpGet("modelinput/file")]
        public IActionResult GetModelInputFile()
        {
            string readText = System.IO.File.ReadAllText(@"c:\repos\Dragonfly\Dragonfly\obj\GeneratedMLNETFiles\MLCodeGenerator\MLCodeGenerator.DataModelsGenerator\ModelInput.cs");

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
            string readText = System.IO.File.ReadAllText(@"c:\repos\Dragonfly\Dragonfly\obj\GeneratedMLNETFiles\MLCodeGenerator\MLCodeGenerator.DataModelsGenerator\Predictor.cs");

            return Ok(readText);
        }

        [HttpGet("program/file")]
        public IActionResult GetMLProgram()
        {
            string readText = System.IO.File.ReadAllText(@"c:\repos\Dragonfly\Dragonfly\obj\GeneratedMLNETFiles\MLCodeGenerator\MLCodeGenerator.DataModelsGenerator\Program.cs");

            return Ok(readText);
        }

        [HttpPost("predict")]
        public IActionResult Predict(ModelInput sample)
        {
            // calling the generated Predictor class
            var prediction = GeneratedDataModels.Predictor.Predict(sample);

            return Ok(prediction);
        }
    }
}
