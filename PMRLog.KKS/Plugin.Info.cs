using System.Reflection;
using System.Runtime.InteropServices;

using IDHIUtils;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(Constants.Prefix + "_" + PMRLog.PInfo.PluginName + " (" + PMRLog.PInfo.GUID + ")")]
[assembly: AssemblyProduct(Constants.Prefix + "_" + PMRLog.PInfo.PluginName)]
[assembly: AssemblyVersion(PMRLog.PInfo.Version)]
[assembly: AssemblyFileVersion(PMRLog.PInfo.Version)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9289ae95-49f0-460c-bcd0-8fde608464e0")]

#endregion Assembly attributes

namespace PMRLog
{
    public struct PInfo
    {
        public const string GUID = "com.ihavenoidea.pmrlog";
#if DEBUG
        public const string PluginDisplayName = "Rotate Log (Debug)";
#else
        public const string PluginDisplayName = "Rotate Log";
#endif
        public const string PluginName = "PMRlog";
        public const string Version = "0.0.1.0";
    }
}
