using System.Reflection;
using System.Runtime.InteropServices;

using IDHIPlugIns;
using IDHIUtils;

#region Assembly attributes

/*
 * These attributes define various meta-information of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in Info.
 */
[assembly: AssemblyTitle(Constants.Prefix + "_" + PMRLogs.PluginName + " (" + PMRLogs.GUID + ")")]
[assembly: AssemblyProduct(Constants.Prefix + "_" + PMRLogs.PluginName)]
[assembly: AssemblyVersion(PMRLogs.Version)]
[assembly: AssemblyFileVersion(PMRLogs.Version)]

#endregion Assembly attributes

namespace IDHIPlugIns
{
    public partial class PMRLogs
    {
        public const string GUID = "com.ihavenoidea.pmrlog";
#if DEBUG
        public const string PluginDisplayName = "Rotate Log (Debug)";
#else
        public const string PluginDisplayName = "Rotate Log";
#endif
        public const string PluginName = "PMRlog";
        public const string Version = "1.0.0.0";
    }
}
