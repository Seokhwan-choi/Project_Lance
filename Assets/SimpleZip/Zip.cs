using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Assets.SimpleZip
{
    public static class Zip
    {
        //public static byte[] Inflate(byte[] data)
        //{
        //    int outputSize = 1024;
        //    byte[] output = new Byte[outputSize];
        //    bool expectRfc1950Header = true;
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        ZlibCodec compressor = new ZlibCodec();
        //        compressor.InitializeInflate(expectRfc1950Header);

        //        compressor.InputBuffer = data;
        //        compressor.AvailableBytesIn = data.Length;
        //        compressor.NextIn = 0;
        //        compressor.OutputBuffer = output;

        //        foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
        //        {
        //            int bytesToWrite = 0;
        //            do
        //            {
        //                compressor.AvailableBytesOut = outputSize;
        //                compressor.NextOut = 0;
        //                compressor.Inflate(f);

        //                bytesToWrite = outputSize - compressor.AvailableBytesOut;
        //                if (bytesToWrite > 0)
        //                    ms.Write(output, 0, bytesToWrite);
        //            }
        //            while ((f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
        //                (f == FlushType.Finish && bytesToWrite != 0));
        //        }

        //        compressor.EndInflate();

        //        return ms.ToArray();
        //    }
        //}

        //public static byte[] Deflate(byte[] data)
        //{
        //    int outputSize = 1024;
        //    byte[] output = new Byte[outputSize];
        //    int lengthToCompress = data.Length;

        //    // If you want a ZLIB stream, set this to true.  If you want
        //    // a bare DEFLATE stream, set this to false.
        //    bool wantRfc1950Header = true;

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        ZlibCodec compressor = new ZlibCodec();
        //        compressor.InitializeDeflate(CompressionLevel.BestCompression, wantRfc1950Header);

        //        compressor.InputBuffer = data;
        //        compressor.AvailableBytesIn = lengthToCompress;
        //        compressor.NextIn = 0;
        //        compressor.OutputBuffer = output;

        //        foreach (var f in new FlushType[] { FlushType.None, FlushType.Finish })
        //        {
        //            int bytesToWrite = 0;
        //            do
        //            {
        //                compressor.AvailableBytesOut = outputSize;
        //                compressor.NextOut = 0;
        //                compressor.Deflate(f);

        //                bytesToWrite = outputSize - compressor.AvailableBytesOut;
        //                if (bytesToWrite > 0)
        //                    ms.Write(output, 0, bytesToWrite);
        //            }
        //            while ((f == FlushType.None && (compressor.AvailableBytesIn != 0 || compressor.AvailableBytesOut == 0)) ||
        //                (f == FlushType.Finish && bytesToWrite != 0));
        //        }

        //        compressor.EndDeflate();

        //        ms.Flush();
        //        return ms.ToArray();
        //    }
        //}

        /// <summary>
        /// Compress byte array to byte array.
        /// </summary>
        public static byte[] Compress(byte[] bytes)
        {
            using var memoryStream = new MemoryStream();
            using var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal);

            gzipStream.Write(bytes, 0, bytes.Length);
            gzipStream.Dispose();

            return memoryStream.ToArray();
        }

        /// <summary>
        /// Decompress byte array to byte array.
        /// </summary>
        public static byte[] Decompress(byte[] bytes)
        {
            using(var compressStream = new MemoryStream(bytes))
            {
                using (var zipStream = new GZipStream(compressStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);

                        return resultStream.ToArray();
                    }
                }
             }

            //using var memoryStream = new MemoryStream(bytes);
            //using var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress);
            //using var outputStream = new MemoryStream();

            //decompressStream.CopyTo(outputStream);

            //return outputStream.ToArray();
        }

        /// <summary>
        /// Compress plain text to byte array.
        /// </summary>
        public static byte[] Compress(string text)
        {
            return Compress(Encoding.UTF8.GetBytes(text));
        }

        /// <summary>
        /// Compress plain text to compressed base-64 string.
        /// </summary>
        public static string CompressToString(string text)
        {
            return Convert.ToBase64String(Compress(text));
        }

        /// <summary>
        /// Decompress compressed base-64 string to plain text.
        /// </summary>
        public static string Decompress(string data)
        {
            return DecompressString(Convert.FromBase64String(data));
        }

        /// <summary>
        /// Decompress byte array to plain text.
        /// </summary>
        public static string DecompressString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(Decompress(bytes));
        }

        /// <summary>
        /// Compress directory to ZIP archive.
        /// </summary>
        public static void CompressDirectory(string sourcePath, string archivePath, bool overwrite = true)
        {
            if (overwrite)
            {
                File.Delete(archivePath);
            }

            ZipFile.CreateFromDirectory(sourcePath, archivePath);
        }

        /// <summary>
        /// Compress file to ZIP archive.
        /// </summary>
        public static void CompressFile(string filePath, string archivePath, bool overwrite = true)
        {
            if (overwrite)
            {
                File.Delete(archivePath);
            }

            using var zipArchive = ZipFile.Open(archivePath, ZipArchiveMode.Create);
            
            zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
        }

        /// <summary>
        /// Decompress ZIP archive to directory.
        /// </summary>
        public static void DecompressArchive(string archivePath, string directoryPath, bool overwrite = true)
        {
            using var fileStream = File.OpenRead(archivePath);

            DecompressArchive(fileStream, directoryPath, overwrite);
        }

        /// <summary>
        /// Decompress ZIP archive to directory.
        /// </summary>
        public static void DecompressArchive(byte[] archiveBytes, string directoryPath, bool overwrite = true)
        {
            using var memoryStream = new MemoryStream(archiveBytes);

            DecompressArchive(memoryStream, directoryPath, overwrite);
        }

        private static void DecompressArchive(Stream stream, string directoryPath, bool overwrite = true)
        {
            using var zipArchive = new ZipArchive(stream);

            foreach (ZipArchiveEntry file in zipArchive.Entries)
            {
                var completeFileName = Path.Combine(directoryPath, file.FullName);
                var directory = Path.GetDirectoryName(completeFileName);

                if (directory != null && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (file.Name != "")
                {
                    if (overwrite || !File.Exists(completeFileName))
                    {
                        file.ExtractToFile(completeFileName, overwrite);
                    }
                }
            }
        }
    }
}