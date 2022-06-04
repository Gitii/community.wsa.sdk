using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Community.Wsx.Shared;
using FakeItEasy;

namespace Community.Wsa.Sdk.Tests;

internal static class ProcessTestHelpers
{
    public static IProcess Exits(
        string output = "",
        string errorOutput = "",
        int executionTime = 10,
        int exitCode = 0
    )
    {
        var p = A.Fake<IProcess>();

        var stdOut = new StreamReader(
            new MemoryStream(Encoding.UTF8.GetBytes(output)),
            Encoding.UTF8
        );
        var stdErr = new StreamReader(
            new MemoryStream(Encoding.UTF8.GetBytes(errorOutput)),
            Encoding.UTF8
        );

        A.CallTo(() => p.StandardOutput).Returns(stdOut);
        A.CallTo(() => p.StandardError).Returns(stdErr);
        A.CallTo(() => p.WaitForExitAsync())
            .Returns(Task.Delay(executionTime).ContinueWith((_) => exitCode));
        A.CallTo(() => p.WaitForExitAsync(A<int>._))
            .Returns(Task.Delay(executionTime).ContinueWith<int?>((_) => exitCode));
        A.CallTo(() => p.ExitCode).Returns(exitCode);

        return p;
    }

    public static IProcess TimesOut()
    {
        var p = A.Fake<IProcess>();

        var stdOut = new StreamReader(Stream.Null, Encoding.UTF8);
        var stdErr = new StreamReader(Stream.Null, Encoding.UTF8);

        A.CallTo(() => p.StandardError).Returns(stdErr);
        A.CallTo(() => p.StandardOutput).Returns(stdOut);
        A.CallTo(() => p.WaitForExitAsync(A<int>._))
            .Returns(Task.Delay(10).ContinueWith<int?>((_) => null));
        A.CallTo(() => p.ExitCode).Returns(0);

        return p;
    }
}
