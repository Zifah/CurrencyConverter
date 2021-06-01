using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CurrencyConverter.Helpers
{
    public interface IFileOperations
    {
        /// <summary>
        /// Returns a TextReader with a handle into the file at the specified <paramref name="path"/>
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The <see cref="TextReader"/></returns>
        TextReader GetFileTextReader(string filePath);

        /// <summary>
        /// Save the stream to a temporary file at the prescribed location
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="path"></param>
        /// <returns>The path to the file</returns>
        string SaveStreamToTempFile(Stream stream, string fileName);

        /// <summary>
        /// Unzip a compressed file
        /// </summary>
        /// <param name="path">The path to the file to unzip</param>
        /// <returns>The directory holding the results of this unzip</returns>
        string UnzipArchive(string archiveFilePath);
    }
}