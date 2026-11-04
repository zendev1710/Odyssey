using Odyssey.Models.Documents;
using Odyssey.Utils;
using Odyssey.ViewModels;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Odyssey.Models.Dal
{
    /// <summary>
    /// Provides functionality for working with ZIP archives, including loading report files from a specified ZIP
    /// archive.
    /// </summary>
    /// <remarks>The <see cref="ZipFileService"/> class contains methods for interacting with ZIP archives,
    /// such as extracting and loading specific files. It is designed to handle scenarios where report files are stored
    /// within ZIP archives and need to be programmatically accessed. This class is static and cannot be
    /// instantiated.</remarks>
    public static class ZipFileService
    {
        /// <summary>
        /// Attempts to load a report file from a specified ZIP archive.
        /// </summary>
        /// <remarks>This method searches the specified ZIP archive for a report file with an extension
        /// matching the expected report file type. If a valid report file is found, it is loaded into the <paramref
        /// name="report"/> parameter. If no valid report file is found or an error occurs, the method returns <see
        /// langword="false"/> and provides an error message in the <paramref name="outError"/> parameter.</remarks>
        /// <param name="zipPathName">The full path to the ZIP archive containing the report file.</param>
        /// <param name="report">When this method returns, contains the loaded <see cref="EresseaDocument"/> if the operation succeeds;
        /// otherwise, contains a default instance of <see cref="EresseaDocument"/>. This parameter is passed
        /// uninitialized.</param>
        /// <param name="outError">When this method returns, contains an error message if the operation fails; otherwise, an empty string. This
        /// parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if a report file is successfully loaded from the ZIP archive; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool LoadFile(string zipPathName, out EresseaDocument report, out string outError)
        {
            outError = "";
            string reportName = string.Empty;
            string? directoryName = Path.GetDirectoryName(zipPathName);
            if (directoryName == null) {
                outError = "Invalid zip file path name: " + zipPathName;
                report = new CRDocument(zipPathName, []);
                return false;
            }

            using (ZipArchive archive = ZipFile.OpenRead(zipPathName))
            {
                string crExtension = FileUtils.GetFileExtensionWithDot(StorageService.CrExt);
                foreach (var entry in from ZipArchiveEntry entry in archive.Entries
                                      where entry.FullName.EndsWith(crExtension, StringComparison.OrdinalIgnoreCase)
                                      select entry)
                {
                    // Gets the full path to ensure that relative segments are removed.
                    string reportPathName = Path.GetFullPath(Path.Combine(directoryName, entry.FullName));
                    // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that are case-insensitive.
                    if (reportPathName.StartsWith(directoryName, StringComparison.Ordinal))
                    {
                        // NOTE: an alternative is :
                        // - entry.ExtractToFile(reportPathName) to save file but existence check should be needed
                        // - then ReportFileService.LoadFile(reportPathName, out report, out outError)
                        reportName = Path.GetFileNameWithoutExtension(reportPathName);
                        if (ReportFileService.LoadStream(reportName, entry.Open(), out report, out outError))
                        { 
                            return true;
                        }
                        // The zip is supposed to contain only one report file, so we can break after the first match.
                        break;
                    }
                }
            }

            report = new CRDocument(reportName, []);
            return false;
        }
    }
}

