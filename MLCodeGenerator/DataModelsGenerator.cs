using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using MLCodeGenerator.Enums;
using MLCodeGenerator.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace MLCodeGenerator
{
    [Generator]
    public class DataModelsGenerator : ISourceGenerator
    {
        const string ModelInput = nameof(ModelInput);
        const string ModelOutput = nameof(ModelOutput);
        const string Predictor = nameof(Predictor);
        const string Program = nameof(Program);

        private static readonly DiagnosticDescriptor NotFoundMLNETWarning = new DiagnosticDescriptor(
            id: "MYMLNETGEN001",
            title: "Couldn't open ML.NET model file",
            messageFormat: "Couldn't open ML.NET model file '{0}'",
            category: "MyMLNETGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        private static readonly DiagnosticDescriptor InvalidMLNETWarning = new DiagnosticDescriptor(
            id: "MYMLNETGEN002",
            title: "Couldn't parse ML.NET model file",
            messageFormat: "Couldn't parse ML.NET model file '{0}'",
            category: "MyMLNETGenerator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public void Initialize(GeneratorInitializationContext context)
        {
            //#if DEBUG
            //            if (!Debugger.IsAttached)
            //            {
            //                Debugger.Launch();
            //            }
            //#endif
            //            Debug.WriteLine("Initalize code generator");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            (Scenario? scenario, _) = GetAdditionalFileOptions(context);

            var zipFiles = context.AdditionalFiles.Where(f => Path.GetExtension(f.Path).Equals(".zip", StringComparison.OrdinalIgnoreCase));
            var zipFile = zipFiles.ToArray()[0].Path;

            Stream zip = null;
            try
            {
                zip = StreamHelper.GetZipFileStream(zipFile);
                using var reader = new BinaryReader(zip, Encoding.UTF8);

                var features = StreamHelper.ExtractFeatures(reader);
                StringBuilder modelInputBuilder = SyntaxHelper.ModelInputBuilder(features, ModelInput);
                SourceText sourceText1 = SourceText.From(modelInputBuilder.ToString(), Encoding.UTF8);
                context.AddSource($"{ModelInput}.cs", sourceText1);

                StringBuilder modelOutputBuilder = SyntaxHelper.ModelOutputBuilder(ModelOutput, scenario.Value);
                SourceText sourceText2 = SourceText.From(modelOutputBuilder.ToString(), Encoding.UTF8);
                context.AddSource($"{ModelOutput}.cs", sourceText2);

                StringBuilder clientBuilder = SyntaxHelper.PredictorBuilder(Predictor, zipFile);
                SourceText sourceText3 = SourceText.From(clientBuilder.ToString(), Encoding.UTF8);
                context.AddSource($"{Predictor}.cs", sourceText3);

                StringBuilder webapiBuilder = SyntaxHelper.ProgramBuilder(Program, zipFile);
                SourceText sourceText4 = SourceText.From(webapiBuilder.ToString(), Encoding.UTF8);
                context.AddSource($"{Program}.cs", sourceText4);
            }
            catch (System.IO.InvalidDataException)
            {
                context.ReportDiagnostic(Diagnostic.Create(InvalidMLNETWarning, Location.None, zipFile));
            }
            catch (System.IO.IOException)
            {
                context.ReportDiagnostic(Diagnostic.Create(NotFoundMLNETWarning, Location.None, zipFile));
            }
        }

        private (Scenario?, AdditionalText) GetAdditionalFileOptions(GeneratorExecutionContext context)
        {
            var file = context.AdditionalFiles.First();
            if (Path.GetExtension(file.Path).Equals(".zip", StringComparison.OrdinalIgnoreCase))
            {
                context.AnalyzerConfigOptions.GetOptions(file).TryGetValue("build_metadata.additionalfiles.Scenario", out string scenarioValue);
                Enum.TryParse(scenarioValue, true, out Scenario scenario);

                return (scenario, file);
            }

            return (null, null);
        }
    }
}
