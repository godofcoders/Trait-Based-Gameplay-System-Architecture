using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework.Api;
using NUnit.Framework.Internal;

// Runs the same NUnit domain tests as Unity, using Unity's bundled Mono and NUnit.
// This file lives outside Assets so it never enters a game build.
internal static class DomainTestRunner
{
    private static int Main(string[] args)
    {
        var runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder());
        runner.Load(Assembly.GetExecutingAssembly(), new Dictionary<string, object>());
        var result = runner.Run(TestListener.NULL, TestFilter.Empty);
        string xml = result.ToXml(true).OuterXml;
        if (args.Length > 0) File.WriteAllText(args[0], xml);
        Console.WriteLine($"Domain tests: {result.PassCount} passed, {result.FailCount} failed, {result.SkipCount} skipped.");
        if (result.FailCount > 0) Console.WriteLine(xml);
        return result.FailCount == 0 && result.PassCount > 0 ? 0 : 1;
    }
}
