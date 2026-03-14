#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Inkshot.Editor.AgentBridge
{
    [Serializable]
    public sealed class CompilerError
    {
        public string file;
        public int line;
        public int column;
        public string message;
        public string assembly;
        public bool isWarning;
    }

    [Serializable]
    public sealed class CompilationResult
    {
        public bool success;
        public int errorCount;
        public int warningCount;
        public List<CompilerError> errors = new();
        public List<CompilerError> warnings = new();
        public double durationSeconds;
    }

    [Serializable]
    public sealed class ValidationCheck
    {
        public string category;
        public string description;
        public bool passed;
        public string detail;
    }

    [Serializable]
    public sealed class ValidationResult
    {
        public bool allPassed;
        public int passCount;
        public int failCount;
        public List<ValidationCheck> checks = new();
    }

    [Serializable]
    public sealed class HealthViolation
    {
        public string file;
        public int lineNumber;
        public string rule;
        public string severity;
        public string message;
    }

    [Serializable]
    public sealed class HealthCheckResult
    {
        public bool clean;
        public int violationCount;
        public int filesScanned;
        public List<HealthViolation> violations = new();
    }

    [Serializable]
    public sealed class TestCaseResult
    {
        public string name;
        public string fullName;
        public bool passed;
        public double durationSeconds;
        public string failureMessage;
        public string stackTrace;
    }

    [Serializable]
    public sealed class TestRunResult
    {
        public bool allPassed;
        public int passCount;
        public int failCount;
        public int skipCount;
        public double durationSeconds;
        public List<TestCaseResult> tests = new();
    }

    [Serializable]
    public sealed class AgentFeedbackReport
    {
        public string timestamp;
        public string projectName;
        public string unityVersion;
        public CompilationResult compilation;
        public ValidationResult validation;
        public HealthCheckResult health;
        public TestRunResult tests;

        public bool IsFullyClean()
        {
            bool compileClean = compilation == null || compilation.success;
            bool validationClean = validation == null || validation.allPassed;
            bool healthClean = health == null || health.clean;
            bool testsClean = tests == null || tests.allPassed;
            return compileClean && validationClean && healthClean && testsClean;
        }
    }

    public static class AgentReportSerializer
    {
        public static string ToJson(AgentFeedbackReport report)
        {
            return UnityEngine.JsonUtility.ToJson(report, true);
        }

        public static AgentFeedbackReport FromJson(string json)
        {
            return UnityEngine.JsonUtility.FromJson<AgentFeedbackReport>(json);
        }
    }
}
#endif
