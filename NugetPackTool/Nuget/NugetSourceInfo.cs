using NugetPackTool.Commands;

namespace NugetPackTool.Nuget
{
    public class NugetSourceInfo
    {
        public string NugetSource { get; set; }

        public string Password { get; set; }

        public PackageKind PackageKind { get; set; }
    }
}
