using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RazorDiagnostics
{
    internal class OutputWindow
    {
        private static IVsOutputWindowPane _outputPane;
        private static Guid _outputPaneGuid = new Guid("EDCCF70D-5A2C-48D4-8536-998F4FD1759C");

        public static void OutputString(string toOutput, bool show = false)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            OutputPane.OutputString(toOutput);

            if (show)
            {
                Show();
            }
        }

        public static void Show()
        {
            ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                IVsUIShell uiShell = await ServiceProvider.GetGlobalServiceAsync<SVsUIShell, IVsUIShell>();

                IVsWindowFrame outputWindowFrame = null;
                Guid outoutToolWindowGuid = new Guid("{ 0x34e76e81, 0xee4a, 0x11d0, {0xae, 0x2e, 0x00, 0xa0, 0xc9, 0x0f, 0xff, 0xc3 } }");
                int hr = uiShell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate, ref outoutToolWindowGuid, out outputWindowFrame);

                if (ErrorHandler.Succeeded(hr) && outputWindowFrame != null)
                {
                    hr = outputWindowFrame.Show();
                    if (ErrorHandler.Succeeded(hr) && OutputPane != null)
                    {
                        OutputPane.Activate();
                    }
                }
            });
        }

        public static IVsOutputWindowPane OutputPane
        {
            get
            {
                if (_outputPane == null)
                {
                    ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                        IVsOutputWindow outputWindow = await ServiceProvider.GetGlobalServiceAsync<SVsOutputWindow, IVsOutputWindow>();
                        if (outputWindow != null)
                        {
                            if (ErrorHandler.Failed(outputWindow.GetPane(ref _outputPaneGuid, out _outputPane)) &&
                                ErrorHandler.Succeeded(outputWindow.CreatePane(ref _outputPaneGuid, "Razor Diagnostics", 0, 0)))
                            {
                                if (ErrorHandler.Succeeded(outputWindow.GetPane(ref _outputPaneGuid, out _outputPane)))
                                {
                                    _outputPane.Activate();
                                }
                            }
                        }
                    });
                }

                return _outputPane;
            }
        }
    }
}
