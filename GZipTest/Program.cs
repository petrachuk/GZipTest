using System;

namespace GZipTest
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var argsParser = new ArgsParser(args);
                argsParser.TryParse(out var srcFile, out var dstFile, out var mode);

                using (var processing = new Compressing(srcFile, dstFile, mode))
                {
                    processing.Start();
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Use syntax: GZipTest.exe (compress|decompress) source_file_name result_file_name");
                return 1;
            }
        }
    }
}
