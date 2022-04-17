using System.Threading.Tasks;

namespace Community.Wsa.Sdk;

public interface IUwpPackage
{
    string FamilyName { get; }
    Task Launch();
}
