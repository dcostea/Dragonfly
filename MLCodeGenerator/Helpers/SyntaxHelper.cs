using MLCodeGenerator.Enums;
using MLCodeGenerator.Mappers;
using MLCodeGenerator.Models;
using System.Collections.Generic;
using System.Text;

namespace MLCodeGenerator.Helpers
{
    public static class SyntaxHelper
    {
        private const string __ = "  ";
        private const string GeneratedDataModels = nameof(GeneratedDataModels);

        private static StringBuilder AppendUsing(this StringBuilder syntax, string namespaceName)
        {
            return syntax.AppendLine($"using {namespaceName};");
        }

        private static StringBuilder NamespaceWrap(StringBuilder code, string ns)
        {
            StringBuilder sb = new();
            sb.AppendLine();
            sb.AppendLine($"namespace {ns}");
            sb.AppendLine($"{{");
            sb.Append(code);
            sb.AppendLine($"}}");

            return sb;
        }

        private static StringBuilder InputClassWrap(IEnumerable<Feature> features, string className)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{__}public class {className}");
            sb.AppendLine($"{__}{{");
            foreach (var feature in features)
            {
                sb.AppendLine($"{__}{__}[LoadColumn({feature.Index})]");
                sb.AppendLine($"{__}{__}public {feature.Type.Map()} {feature.Name} {{ get; set; }}");
                sb.AppendLine();
            }
            sb.AppendLine($"{__}}}");

            return sb;
        }

        private static StringBuilder OutputClassWrap(string className, Scenario scenario)
        {
            StringBuilder sb = new();
            sb.AppendLine( $"{__}public class {className}");
            sb.AppendLine( $"{__}{{");
            sb.AppendLine($@"{__}{__}[ColumnName(""PredictedLabel"")]");
            sb.AppendLine( $"{__}{__}public string Prediction {{ get; set; }}");
            sb.AppendLine();
            switch (scenario)
            {
                case Scenario.BinaryClassification:
                    sb.AppendLine($"{__}{__}public float Score {{ get; set; }}");
                    break;
                case Scenario.MultiClassification:
                    sb.AppendLine($"{__}{__}public float Score {{ get; set; }}");
                    break;
                case Scenario.Regression:
                    sb.AppendLine($"{__}{__}public float[] Score {{ get; set; }}");
                    break;
                default:
                    sb.AppendLine($"{__}{__}public float[] Score {{ get; set; }}");
                    break;
            }
            sb.AppendLine( $"{__}}}");

            return sb;
        }

        private static StringBuilder PredictorClassWrap(string className, string modelPath)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{__}public class {className}");
            sb.AppendLine($"{__}{{");

            sb.AppendLine( $"{__}{__}public static ModelOutput Predict(ModelInput sampleData)");
            sb.AppendLine( $"{__}{__}{{");
            sb.AppendLine( $"{__}{__}{__}MLContext mlContext = new MLContext(seed: 1);");
            sb.AppendLine($@"{__}{__}{__}ITransformer model = mlContext.Model.Load(""{modelPath}"", out var modelSchema);");
            sb.AppendLine( $"{__}{__}{__}var predictor = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);");
            sb.AppendLine( $"{__}{__}{__}var predicted = predictor.Predict(sampleData);");
            sb.AppendLine( $"{__}{__}{__}return predicted;");

            sb.AppendLine($"{__}{__}}}");
            sb.AppendLine($"{__}}}");

            return sb;
        }

        private static StringBuilder ProgramClassWrap(string className, string zipPath)
        {
            StringBuilder sb = new();
            sb.AppendLine($"{__}class {className}");
            sb.AppendLine($"{__}{{");

            sb.AppendLine( $"{__}{__}static void Main(string[] args)");
            sb.AppendLine( $"{__}{__}{{");
            sb.AppendLine( $"{__}{__}{__}WebHost.CreateDefaultBuilder()");
            sb.AppendLine(@$"{__}{__}{__}{__}.ConfigureServices(services => {{");
            sb.AppendLine( $"{__}{__}{__}{__}{__}services.AddPredictionEnginePool<ModelInput,ModelOutput>()");
            sb.AppendLine(@$"{__}{__}{__}{__}{__}{__}.FromFile(""{zipPath}"");");
            sb.AppendLine( $"{__}{__}{__}{__}}})");
            sb.AppendLine( $"{__}{__}{__}{__}.Configure(app => {{");
            sb.AppendLine( $"{__}{__}{__}{__}{__}app.UseHttpsRedirection();");
            sb.AppendLine( $"{__}{__}{__}{__}{__}app.UseRouting();");
            sb.AppendLine( $"{__}{__}{__}{__}{__}app.UseEndpoints(routes => {{");
            sb.AppendLine(@$"{__}{__}{__}{__}{__}{__}routes.MapPost(""/predict"", PredictHandler);");
            sb.AppendLine( $"{__}{__}{__}{__}{__}}});");
            sb.AppendLine( $"{__}{__}{__}{__}}})");
            sb.AppendLine( $"{__}{__}{__}{__}.Build()");
            sb.AppendLine( $"{__}{__}{__}{__}.Run();");
            sb.AppendLine( $"{__}{__}}}");

            sb.AppendLine();

            sb.AppendLine($"{__}{__}static async Task PredictHandler(HttpContext http)");
            sb.AppendLine($"{__}{__}{{");
            sb.AppendLine($"{__}{__}{__}var predEngine = http.RequestServices.GetRequiredService<PredictionEnginePool<ModelInput,ModelOutput>>();");
            sb.AppendLine($"{__}{__}{__}var input = await JsonSerializer.DeserializeAsync<ModelInput>(http.Request.Body);");
            sb.AppendLine($"{__}{__}{__}var prediction = predEngine.Predict(input);");
            sb.AppendLine($"{__}{__}{__}await http.Response.WriteAsJsonAsync(prediction);");
            sb.AppendLine($"{__}{__}}}");

            sb.AppendLine($"{__}}}");

            return sb;
        }

        internal static StringBuilder PredictorBuilder(string className, string modelPath)
        {
            var sourceCode = new StringBuilder();

            return sourceCode
                .AppendUsing("System")
                .AppendUsing("Microsoft.ML")
                .AppendUsing("Microsoft.ML.FastTree")
                .AppendUsing("Microsoft.ML.Data")
                .Append(NamespaceWrap(PredictorClassWrap(className, modelPath), GeneratedDataModels));
        }

        internal static StringBuilder ProgramBuilder(string className, string zipPath)
        {
            var sourceCode = new StringBuilder();

            return sourceCode
                .AppendUsing("Microsoft.AspNetCore")
                .AppendUsing("Microsoft.AspNetCore.Http")
                .AppendUsing("Microsoft.AspNetCore.Hosting")
                .AppendUsing("Microsoft.ML.Data")
                .AppendUsing("Microsoft.Extensions.ML")
                .AppendUsing("Microsoft.AspNetCore.Builder")
                .AppendUsing("Microsoft.Extensions.DependencyInjection")
                .AppendUsing("System.Text.Json")
                .AppendUsing("System.Threading.Tasks")
                .Append(NamespaceWrap(ProgramClassWrap(className, zipPath), GeneratedDataModels));
        }

        internal static StringBuilder ModelOutputBuilder(string className, Scenario scenario)
        {
            var sourceCode = new StringBuilder();

            return sourceCode
                .AppendUsing("System")
                .AppendUsing("Microsoft.ML.Data")
                .Append(NamespaceWrap(OutputClassWrap(className, scenario), GeneratedDataModels));
        }

        internal static StringBuilder ModelInputBuilder(IEnumerable<Feature> features, string className)
        {
            var sourceCode = new StringBuilder();

            return sourceCode
                .AppendUsing("System")
                .AppendUsing("Microsoft.ML.Data")
                .Append(NamespaceWrap(InputClassWrap(features, className), GeneratedDataModels));
        }
    }
}
