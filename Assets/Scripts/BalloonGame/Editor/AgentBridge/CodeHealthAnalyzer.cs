#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Inkshot.Editor.AgentBridge
{
    public static class CodeHealthAnalyzer
    {
        private const string ScriptsRoot = "Assets/Scripts/BalloonGame";

        private const int LimitMonoBehaviour = 300;
        private const int LimitUIScreen = 250;
        private const int LimitManager = 250;
        private const int LimitEditor = 200;
        private const int LimitVFX = 200;
        private const int LimitPartial = 150;
        private const int LimitData = 150;
        private const int LimitUtility = 150;
        private const int SeverityEscalationThreshold = 50;

        private static readonly Regex UpdateMethodPattern = new(
            @"\b(void\s+)(Update|FixedUpdate|LateUpdate)\s*\(",
            RegexOptions.Compiled);

        private static readonly Regex GetComponentInBodyPattern = new(
            @"\b(GetComponent|FindObjectOfType|FindAnyObjectByType)\s*[<(]",
            RegexOptions.Compiled);

        private static readonly Regex DebugLogPattern = new(
            @"\bDebug\.(Log|LogWarning|LogError)\s*\(",
            RegexOptions.Compiled);

        public static HealthCheckResult Analyze()
        {
            var violations = new List<HealthViolation>();
            int filesScanned = 0;

            if (!Directory.Exists(ScriptsRoot))
            {
                return new HealthCheckResult { clean = true, filesScanned = 0 };
            }

            string[] csFiles = Directory.GetFiles(ScriptsRoot, "*.cs", SearchOption.AllDirectories);

            foreach (string filePath in csFiles)
            {
                if (filePath.Contains("/AgentBridge/"))
                {
                    continue;
                }

                filesScanned++;
                string relativePath = filePath.Replace("\\", "/");
                string[] lines = File.ReadAllLines(filePath);

                CheckFileSize(relativePath, lines.Length, violations);
                CheckDebugLog(relativePath, lines, violations);
                CheckGetComponentInUpdate(relativePath, lines, violations);
            }

            var result = new HealthCheckResult
            {
                clean = violations.Count == 0,
                violationCount = violations.Count,
                filesScanned = filesScanned,
                violations = violations,
            };

            return result;
        }

        private static void CheckFileSize(string filePath, int lineCount, List<HealthViolation> violations)
        {
            int limit = GetFileSizeLimit(filePath);
            if (lineCount <= limit)
            {
                return;
            }

            violations.Add(new HealthViolation
            {
                file = filePath,
                lineNumber = lineCount,
                rule = "FileSize",
                severity = lineCount > limit + SeverityEscalationThreshold ? "error" : "warning",
                message = $"File has {lineCount} lines (limit: {limit})",
            });
        }

        private static int GetFileSizeLimit(string filePath)
        {
            if (filePath.Contains("/Editor/")) return LimitEditor;
            if (filePath.Contains("/Data/")) return LimitData;
            if (filePath.Contains("/VFX/") || filePath.Contains("/Visuals/")) return LimitVFX;
            if (filePath.Contains("/UI/")) return LimitUIScreen;
            if (filePath.Contains("/Managers/")) return LimitManager;

            string fileName = Path.GetFileName(filePath);
            if (fileName.Contains(".") && fileName.IndexOf('.') != fileName.LastIndexOf('.'))
            {
                return LimitPartial;
            }

            if (fileName == "GameConstants.cs" || fileName.Contains("Utility") || fileName.Contains("Helper"))
            {
                return LimitUtility;
            }

            return LimitMonoBehaviour;
        }

        private static void CheckDebugLog(string filePath, string[] lines, List<HealthViolation> violations)
        {
            int editorGuardDepth = 0;
            int preprocessorDepth = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("#if"))
                {
                    preprocessorDepth++;
                    if (line.Contains("UNITY_EDITOR"))
                    {
                        editorGuardDepth = preprocessorDepth;
                    }
                }
                else if (line.StartsWith("#endif"))
                {
                    if (preprocessorDepth == editorGuardDepth)
                    {
                        editorGuardDepth = 0;
                    }

                    preprocessorDepth--;
                }

                bool insideEditorGuard = editorGuardDepth > 0;
                if (!insideEditorGuard && DebugLogPattern.IsMatch(line))
                {
                    violations.Add(new HealthViolation
                    {
                        file = filePath,
                        lineNumber = i + 1,
                        rule = "NoDebugLog",
                        severity = "warning",
                        message = "Debug.Log without #if UNITY_EDITOR guard",
                    });
                }
            }
        }

        private static void CheckGetComponentInUpdate(string filePath, string[] lines, List<HealthViolation> violations)
        {
            bool insideUpdateMethod = false;
            int braceDepth = 0;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (UpdateMethodPattern.IsMatch(line))
                {
                    insideUpdateMethod = true;
                    braceDepth = 0;
                }

                if (insideUpdateMethod)
                {
                    foreach (char c in line)
                    {
                        if (c == '{') braceDepth++;
                        else if (c == '}') braceDepth--;
                    }

                    if (GetComponentInBodyPattern.IsMatch(line))
                    {
                        violations.Add(new HealthViolation
                        {
                            file = filePath,
                            lineNumber = i + 1,
                            rule = "NoGetComponentInUpdate",
                            severity = "error",
                            message = "GetComponent/FindObjectOfType in Update loop (cache in Awake)",
                        });
                    }

                    if (braceDepth <= 0 && i > 0)
                    {
                        insideUpdateMethod = false;
                    }
                }
            }
        }

    }
}
#endif
