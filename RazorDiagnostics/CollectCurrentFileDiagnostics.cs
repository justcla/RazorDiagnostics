using System;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using RazorDiagnostics.DiagnosticUtilities;
using Task = System.Threading.Tasks.Task;

namespace RazorDiagnostics.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class CollectCurrentFileDiagnostics
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("aaf4d89c-f285-42d7-95b4-9fec65577fc2");

        private const string RAZOR_MESSAGE_BOX_TITLE = "Razor Diagnostics";

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectCurrentFileDiagnostics"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private CollectCurrentFileDiagnostics(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static CollectCurrentFileDiagnostics Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in CollectCurrentFileDiagnostics's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new CollectCurrentFileDiagnostics(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                OutputWindow.Show();
                OutputWindow.OutputString($"Visual Studio Binary in {InstallationStateUtilities.IDEPath}:\r\n    {InstallationStateUtilities.IDEInfo}\r\n");
                OutputWindow.OutputString($"Is Razor Dependency Installed:\r\n    {InstallationStateUtilities.IsRazorDependencyInstalled}\r\n");
                OutputWindow.OutputString($"Installed Web Editor Assemblies in {InstallationStateUtilities.WebEditorFolder}: \r\n{InstallationStateUtilities.InstalledWebEditorAssemblies}\r\n");
                OutputWindow.OutputString($"Installed Web Editor .Net Core Razor Assemblies in {InstallationStateUtilities.WebEditorRazor4Folder}: \r\n{InstallationStateUtilities.InstalledWebEditorRazorV4Assemblies}\r\n");
                OutputWindow.OutputString($"Installed Razor Extension Assemblies in {InstallationStateUtilities.RazorExtensionFolder}: \r\n{InstallationStateUtilities.InstalledRazorExtensionAssemblies}\r\n");

                PopSuccessMessage();
                OutputWindow.Show();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occurred while attempting to output Razor diagnostics.", ex);
                PopFailureMessage(ex.Message);
            }
        }

        private static void PopSuccessMessage()
        {
            const string Message = "Razor diagnostics information has been printed to the Output window.\n" +
                "Open the Output window to read and copy the text.";

            ShowRazorMessageBox(Message, MessageBoxIcon.Information);
        }

        private static void PopFailureMessage(string message)
        {
            string Message = $"Something went wrong while attempting to output Razor diagnostics information\n\n" +
                $"Error message: {message}\n\n" +
                $"Check the Output window for details.";

            ShowRazorMessageBox(Message, MessageBoxIcon.Error);
        }
        private static void ShowRazorMessageBox(string message, MessageBoxIcon messageBoxIcon)
        {
            MessageBox.Show(message, RAZOR_MESSAGE_BOX_TITLE, MessageBoxButtons.OK, messageBoxIcon);
        }

    }
}
