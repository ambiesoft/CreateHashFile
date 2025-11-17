using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;

namespace CreateHashFile
{
    static class HashFileWriter
    {
        public static void WriteHashes(string filePath, bool writeSha1, bool writeMd5)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is required.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Input file not found.", filePath);

            // Open the file once and reuse the stream for multiple hash computations.
            using var stream = File.OpenRead(filePath);

            if (writeSha1)
            {
                stream.Position = 0;
                using var sha1 = SHA1.Create();
                var hash = sha1.ComputeHash(stream);
                WriteHashFile(filePath, ".sha1", hash);
            }

            if (writeMd5)
            {
                stream.Position = 0;
                using var md5 = MD5.Create();
                var hash = md5.ComputeHash(stream);
                WriteHashFile(filePath, ".md5", hash);
            }
        }

        static void WriteHashFile(string sourceFilePath, string extension, byte[] hash)
        {
            // Ensure extension starts with a dot (unless it's empty/null which removes extension)
            if (!string.IsNullOrEmpty(extension) && !extension.StartsWith("."))
                extension = "." + extension;

            // Replace the original extension with the new one.
            var dest = Path.ChangeExtension(sourceFilePath, extension);

            var hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();

            // If destination exists, warn and ask the user to confirm overwrite.
            if (File.Exists(dest))
            {
                Console.WriteLine($"Warning: The file '{dest}' already exists.");

                // If no interactive desktop (e.g., service/CI), do not prompt — skip overwrite.
                if (!Environment.UserInteractive)
                {
                    Console.WriteLine("No interactive desktop available; skipping overwrite.");
                    return;
                }

                // Ask user via a message box. MessageBox must be shown on an STA thread.
                var message = $"The file '{dest}' already exists.\n\nOverwrite?";
                var caption = "Confirm Overwrite";

                if (!ConfirmWithMessageBox(message, caption))
                {
                    Console.WriteLine("File not overwritten.");
                    return;
                }
            }

            File.WriteAllText(dest, hex);
        }

        static bool ConfirmWithMessageBox(string text, string caption)
        {
            // Show a Yes/No message box on a new STA thread and return true if the user clicked Yes.
            DialogResult result = DialogResult.None;

            var thread = new Thread(() =>
            {
                // Use MessageBoxDefaultButton.Button2 to default to "No"
                result = MessageBox.Show(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            return result == DialogResult.Yes;
        }
    }
}