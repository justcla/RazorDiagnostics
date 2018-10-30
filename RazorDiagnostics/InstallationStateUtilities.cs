using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RazorDiagnostics.DiagnosticUtilities
{
    internal class InstallationStateUtilities
    {
        private static bool? _isRazorDependencyInstalled;

        const string _webComponentGroupId = "Microsoft.VisualStudio.ComponentGroup.Web";

        internal static bool IsRazorDependencyInstalled
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if (!_isRazorDependencyInstalled.HasValue)
                {
                    try
                    {
                        _isRazorDependencyInstalled = false;

                        IVsSetupCompositionService setupCompositionService =
                            ServiceProvider.GlobalProvider.GetService(typeof(SVsSetupCompositionService)) as IVsSetupCompositionService;

                        Debug.Assert(setupCompositionService != null);
                        if (setupCompositionService != null)
                        {
                            _isRazorDependencyInstalled = setupCompositionService.IsPackageInstalled(_webComponentGroupId);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Fail($"Exception while checking for {_webComponentGroupId} presence: \n{e.ToString()}");
                    }

                }

                return _isRazorDependencyInstalled.Value;
            }
        }

        internal static string InstalledWebEditorAssemblies => GetAssembliesInFolder(WebEditorFolder);

        internal static string InstalledWebEditorRazorV4Assemblies => GetAssembliesInFolder(WebEditorRazor4Folder);

        internal static string InstalledRazorExtensionAssemblies => GetAssembliesInFolder(RazorExtensionFolder);

        internal static string WebEditorFolder => Path.Combine(IDEFolder, @"Extensions\Microsoft\Web Tools\Editors");

        internal static string WebEditorRazor4Folder => Path.Combine(WebEditorFolder, @"Razor\v4.0");

        internal static string RazorExtensionFolder => Path.Combine(IDEFolder, @"CommonExtensions\Microsoft\RazorLanguageServices");

        internal static string IDEFolder => Path.GetDirectoryName(IDEPath);

        internal static string IDEPath => Process.GetCurrentProcess().MainModule.FileName;

        internal static string IDEInfo => GetAssemblyInfo(IDEPath);

        internal static string GetAssembliesInFolder(string folderPath)
        {
            return "    " + string.Join("\r\n    ", Directory.GetFiles(folderPath, "*.dll", SearchOption.TopDirectoryOnly).Select(GetAssemblyInfo));
        }

        internal static string GetAssemblyInfo(string assemblyPath)
        {
            return Path.GetFileName(assemblyPath) + ", " + FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion;
        }
    }
}
