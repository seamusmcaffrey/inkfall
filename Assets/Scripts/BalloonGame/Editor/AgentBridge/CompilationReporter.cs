#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;

namespace Inkshot.Editor.AgentBridge
{
    [InitializeOnLoad]
    public static class CompilationReporter
    {
        private const string OutputDirectory = "Logs/agent-feedback";
        private const string CompilationOutputFile = "compilation.json";

        private static CompilationResult _currentResult;
        private static Stopwatch _stopwatch;

        static CompilationReporter()
        {
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;
        }

        public static CompilationResult GetLastResult()
        {
            string path = Path.Combine(OutputDirectory, CompilationOutputFile);
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            return UnityEngine.JsonUtility.FromJson<CompilationResult>(json);
        }

        public static CompilationResult TriggerAndCapture()
        {
            _currentResult = new CompilationResult();
            _stopwatch = Stopwatch.StartNew();

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

            _stopwatch.Stop();

            if (_currentResult.errorCount == 0)
            {
                _currentResult.success = true;
            }

            _currentResult.durationSeconds = _stopwatch.Elapsed.TotalSeconds;
            WriteResult(_currentResult);
            return _currentResult;
        }

        private static void OnCompilationStarted(object context)
        {
            _currentResult = new CompilationResult();
            _stopwatch = Stopwatch.StartNew();
        }

        private static void OnCompilationFinished(object context)
        {
            if (_currentResult == null)
            {
                return;
            }

            if (_stopwatch != null)
            {
                _stopwatch.Stop();
                _currentResult.durationSeconds = _stopwatch.Elapsed.TotalSeconds;
            }

            _currentResult.success = _currentResult.errorCount == 0;
            WriteResult(_currentResult);
        }

        private static void OnAssemblyCompilationFinished(string assemblyPath, CompilerMessage[] messages)
        {
            if (_currentResult == null)
            {
                _currentResult = new CompilationResult();
            }

            string assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);

            foreach (CompilerMessage message in messages)
            {
                var entry = new CompilerError
                {
                    file = message.file,
                    line = message.line,
                    column = message.column,
                    message = message.message,
                    assembly = assemblyName,
                    isWarning = message.type == CompilerMessageType.Warning,
                };

                if (message.type == CompilerMessageType.Error)
                {
                    _currentResult.errors.Add(entry);
                    _currentResult.errorCount++;
                }
                else if (message.type == CompilerMessageType.Warning)
                {
                    _currentResult.warnings.Add(entry);
                    _currentResult.warningCount++;
                }
            }
        }

        private static void WriteResult(CompilationResult result)
        {
            EnsureOutputDirectory();
            string path = Path.Combine(OutputDirectory, CompilationOutputFile);
            string json = UnityEngine.JsonUtility.ToJson(result, true);
            File.WriteAllText(path, json);
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
