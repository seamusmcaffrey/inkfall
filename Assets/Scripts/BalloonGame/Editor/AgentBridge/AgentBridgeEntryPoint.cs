#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Inkshot.Editor.AgentBridge
{
    public static class AgentBridgeEntryPoint
    {
        private const string OutputDirectory = "Logs/agent-feedback";

        [MenuItem("Inkshot/Agent Bridge/Compile Check")]
        public static void Compile()
        {
            EnsureOutputDirectory();
            var result = CompilationReporter.TriggerAndCapture();
            WriteOutput("compilation.json", result);
            LogSummary("Compilation", result.success, $"{result.errorCount} errors, {result.warningCount} warnings");

            if (IsBatchMode())
            {
                EditorApplication.Exit(result.success ? 0 : 1);
            }
        }

        [MenuItem("Inkshot/Agent Bridge/Validate Scene")]
        public static void Validate()
        {
            EnsureOutputDirectory();
            var result = SceneValidator.Validate();
            WriteOutput("validation.json", result);
            LogSummary("Validation", result.allPassed, $"{result.passCount} passed, {result.failCount} failed");

            if (IsBatchMode())
            {
                EditorApplication.Exit(result.allPassed ? 0 : 1);
            }
        }

        [MenuItem("Inkshot/Agent Bridge/Health Check")]
        public static void Health()
        {
            EnsureOutputDirectory();
            var result = CodeHealthAnalyzer.Analyze();
            WriteOutput("health.json", result);
            LogSummary("Health", result.clean, $"{result.violationCount} violations in {result.filesScanned} files");

            if (IsBatchMode())
            {
                EditorApplication.Exit(result.clean ? 0 : 1);
            }
        }

        [MenuItem("Inkshot/Agent Bridge/Full Report")]
        public static void FullReport()
        {
            EnsureOutputDirectory();

            var report = new AgentFeedbackReport
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                projectName = Application.productName,
                unityVersion = Application.unityVersion,
                compilation = CompilationReporter.TriggerAndCapture(),
                validation = SceneValidator.Validate(),
                health = CodeHealthAnalyzer.Analyze(),
            };

            string json = AgentReportSerializer.ToJson(report);
            string reportPath = Path.Combine(OutputDirectory, "report.json");
            File.WriteAllText(reportPath, json);

            if (IsBatchMode())
            {
                Console.WriteLine(json);
                EditorApplication.Exit(report.IsFullyClean() ? 0 : 1);
            }
        }

        [MenuItem("Inkshot/Agent Bridge/Capture Screenshot")]
        public static void Screenshot()
        {
            string path = ScreenshotCapture.CaptureScene("scene");
            string message = path != null
                ? $"[AgentBridge] Screenshot saved: {path}"
                : "[AgentBridge] Screenshot FAILED — no camera or scene";

            if (IsBatchMode())
            {
                Console.WriteLine(message);
                EditorApplication.Exit(path != null ? 0 : 1);
            }
        }

        [MenuItem("Inkshot/Agent Bridge/Capture Gameplay")]
        public static void CaptureGameplay()
        {
            PlayModeCapture.Run();
        }

        [MenuItem("Inkshot/Agent Bridge/Smoke Test")]
        public static void SmokeTest()
        {
            BalloonSmokeRunner.Run();
        }

        private static void WriteOutput<T>(string fileName, T data)
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(OutputDirectory, fileName);
            File.WriteAllText(path, json);

            if (IsBatchMode())
            {
                Console.WriteLine(json);
            }
        }

        private static void LogSummary(string checkName, bool passed, string detail)
        {
            string status = passed ? "PASS" : "FAIL";
            string message = $"[AgentBridge] {checkName}: {status} — {detail}";

            if (IsBatchMode())
            {
                Console.WriteLine(message);
            }
        }

        private static bool IsBatchMode()
        {
            return Application.isBatchMode;
        }

        private static void EnsureOutputDirectory()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }
    }
}
#endif
