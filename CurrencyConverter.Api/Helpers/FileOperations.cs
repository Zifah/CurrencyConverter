using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace CurrencyConverter.Api.Helpers
{
    public class FileOperations : IFileOperations
    {
        public TextReader GetFileTextReader(string filePath)
        {
            return new StreamReader(filePath);
        }

        /// <inheritdoc/>
        public string SaveStreamToTempFile(Stream stream, string fileName)
        {
            string tempFilePath = Path.Combine(Path.GetTempPath(), fileName);
            using var fileStream = File.Create(tempFilePath);
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(fileStream);
            return tempFilePath;
        }

        /// <summary>
        /// Unzip directory
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string UnzipArchive(string archiveFilePath)
        {
            var fileInfo = new FileInfo(archiveFilePath);
            var destinationFolder = Path.Combine(fileInfo.DirectoryName, Path.GetFileNameWithoutExtension(archiveFilePath));
            ZipFile.ExtractToDirectory(archiveFilePath, destinationFolder);
            return destinationFolder;
        }
    }
}
