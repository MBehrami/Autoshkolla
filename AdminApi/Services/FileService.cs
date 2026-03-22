using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AdminApi.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Saves a file asynchronously with validation
        /// </summary>
        /// <param name="file">The file to save</param>
        /// <param name="allowedMimeTypes">List of allowed MIME types (optional - if null, all types allowed)</param>
        /// <param name="maxFileSizeBytes">Maximum file size in bytes (default: 5 MB)</param>
        /// <returns>Tuple with success status, file GUID, extension, and error message</returns>
        Task<(bool Success, string? FileGuid, string? Extension, string ErrorMessage)> SaveFileAsync(
            IFormFile file,
            IEnumerable<string>? allowedMimeTypes = null,
            long maxFileSizeBytes = 5242880); // 5 MB default

        /// <summary>
        /// Deletes a file by GUID and optional extension
        /// </summary>
        bool DeleteFile(string fileGuid, string? extension = null);
    }

    public class FileService : IFileService
    {
        private readonly string _filesDirectory;

        public FileService(IWebHostEnvironment environment)
        {
            _filesDirectory = Path.Combine(environment.ContentRootPath, "files");
            
            // Ensure directory exists
            if (!Directory.Exists(_filesDirectory))
            {
                Directory.CreateDirectory(_filesDirectory);
            }
        }

        public async Task<(bool Success, string? FileGuid, string? Extension, string ErrorMessage)> SaveFileAsync(
            IFormFile file,
            IEnumerable<string>? allowedMimeTypes = null,
            long maxFileSizeBytes = 5242880)
        {
            try
            {
                // Validate file existence
                if (file == null || file.Length == 0)
                {
                    return (false, null, null, "Fajlli nuk u sigurua.");
                }

                // Validate file size
                if (file.Length > maxFileSizeBytes)
                {
                    var maxSizeMB = maxFileSizeBytes / (1024.0 * 1024.0);
                    var actualSizeMB = file.Length / (1024.0 * 1024.0);
                    return (false, null, null, $"Madhësia maksimale e fajllit është {maxSizeMB} MB. Fajlli juaj është {Math.Round(actualSizeMB, 2)} MB.");
                }

                // Validate MIME type if restrictions provided
                if (allowedMimeTypes != null && !allowedMimeTypes.Contains(file.ContentType.ToLower()))
                {
                    return (false, null, null, "Lloji i fajllit nuk është i lejuar.");
                }

                // Get file extension
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (string.IsNullOrWhiteSpace(fileExtension))
                {
                    return (false, null, null, "Fajlli duhet të ketë një zgjedhje (extension).");
                }

                // Generate GUID and filename (flat structure - no subdirectories)
                var fileGuid = Guid.NewGuid().ToString();
                var filename = $"{fileGuid}{fileExtension}";
                var filePath = Path.Combine(_filesDirectory, filename);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, fileGuid, fileExtension, "Fajlli u ruajt me sukses.");
            }
            catch (Exception ex)
            {
                return (false, null, null, $"Ndodhi një gabim gjatë ruajtjes së fajllit: {ex.Message}");
            }
        }

        public bool DeleteFile(string fileGuid, string? extension = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileGuid))
                    return false;

                // If extension is provided, delete that specific file
                if (!string.IsNullOrWhiteSpace(extension))
                {
                    var ext = extension.StartsWith(".") ? extension : $".{extension}";
                    var filePath = Path.Combine(_filesDirectory, $"{fileGuid}{ext}");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                        return true;
                    }
                }
                else
                {
                    // No extension provided, search for any matching file
                    var matchingFiles = Directory.GetFiles(_filesDirectory, $"{fileGuid}.*");
                    if (matchingFiles.Length > 0)
                    {
                        File.Delete(matchingFiles[0]);
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
