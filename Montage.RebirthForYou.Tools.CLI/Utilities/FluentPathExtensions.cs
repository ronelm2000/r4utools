using Fluent.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Montage.RebirthForYou.Tools.CLI.Utilities
{
    public static class FluentPathExtensions
    {
        /// <summary>
        /// Creates a file from the path, exposing a stream in the process.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Path CreateFile(this Path path, string filename, Action<System.IO.FileStream> streamAction)
        {
            // if Path is null TODO: make an exception
            using (var stream = System.IO.File.Create(path.FullPath))
                streamAction?.Invoke(stream);
            return path.Files();
        }

        public static System.IO.Stream GetStream(this Path path)
        {
            return System.IO.File.OpenRead(path.FullPath);
        }

        public static System.IO.FileStream OpenStream(this Path path, System.IO.FileMode fileMode)
        {
            var tcs = new TaskCompletionSource<System.IO.Stream>();
            do try
                {
                    return System.IO.File.Open(path.FullPath, fileMode);
                }
                catch (System.IO.IOException)
                { }
            while (true);
        }

        public static async Task<System.IO.Stream> OpenStreamAsync(this Path path, System.IO.FileMode fileMode, CancellationToken token = default)
        {
            do try
                {
                    token.ThrowIfCancellationRequested();
                    return System.IO.File.Open(path.FullPath, fileMode);
                }
                catch (System.IO.IOException)
                {
                    await Task.Delay(500, token);
                }
            while (true);
        }


        /// 
        /// <summary>
        /// Creates a file under the first path in the set.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file.</param>
        /// <returns>A set with the created file.</returns>
        // public static Path CreateFile(this Path path, string fileName, string fileContent) => path.First().CreateFiles(p => path.Create(fileName), p => fileContent);
    }
}
