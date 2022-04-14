// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Usage",
    "VSTHRD111:Use ConfigureAwait(bool)",
    Justification = "Simplify test code, optimizations are not necessary",
    Scope = "module"
)]
[assembly: SuppressMessage(
    "Style",
    "VSTHRD200:Use \"Async\" suffix for async methods",
    Justification = "Simplify test code, optimizations are not necessary",
    Scope = "module"
)]
[assembly:
    SuppressMessage("AsyncUsage", "AsyncFixer01:Unnecessary async/await usage",
        Justification = "Simplify test code, optimizations are not necessary", Scope = "module")]
