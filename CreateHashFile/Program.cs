using System;
using System.IO;
using System.Windows.Forms;

namespace CreateHashFile
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Initialize WinForms application configuration
            ApplicationConfiguration.Initialize();

            // If startup arguments contain -sha1 or -md5 and a file path, compute and write hashes, then exit.
            if (args != null && args.Length > 0)
            {
                bool wantSha1 = false;
                bool wantMd5 = false;
                string filePath = null;

                foreach (var a in args)
                {
                    var lower = a.Trim().ToLowerInvariant();
                    if (lower == "-sha1") wantSha1 = true;
                    else if (lower == "-md5") wantMd5 = true;
                    else if (!lower.StartsWith("-") && filePath == null) filePath = a;
                }

                if ((wantSha1 || wantMd5) && !string.IsNullOrEmpty(filePath))
                {
                    try
                    {
                        HashFileWriter.WriteHashes(filePath, wantSha1, wantMd5);
                        // Completed work — exit without showing the UI
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Show an error and exit
                        MessageBox.Show($"Error writing hash file(s): {ex.Message}", "CreateHashFile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }

            // No valid args — launch the WinForms UI as before
            Application.Run(new FormMain());
        }
    }
}