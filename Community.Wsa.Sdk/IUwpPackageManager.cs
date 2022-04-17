using System.Collections.Generic;
using System.Text;

namespace Community.Wsa.Sdk;

public interface IUwpPackageManager
{
    IUwpPackage? FindPackage(string packageName, string packagePublisher);
}
