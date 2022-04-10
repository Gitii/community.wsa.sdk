using System.Diagnostics;

namespace Community.Wsx.Shared;

internal class Win32ProcessManager : IProcessManager
{
    public IProcess? Start(ProcessStartInfo startInfo)
    {
        var process = Process.Start(startInfo);
        if (process == null)
        {
            return null;
        }

        return new Win32Process(process);
    }

    public IProcess? Find(string programName)
    {
        var p = Process.GetProcessesByName(programName).FirstOrDefault();
        if (p == null)
        {
            return null;
        }

        return new Win32Process(p);
    }
}
