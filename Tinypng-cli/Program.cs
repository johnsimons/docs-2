using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TinifyAPI;

namespace Tinypng_cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
            {
                Console.WriteLine(CommandLine.Text.HelpText.AutoBuild(options));
                return;
            }

            Run(options);

            Console.Out.WriteLine("All done!");
        }

        static void Run(Options options)
        {
            Tinify.Key = options.TinifyKey;

            var processedFiles = new string[0];
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var processedFilesTxt = Path.Combine(location, "processed-files.txt");

            if (File.Exists(processedFilesTxt))
            {
                processedFiles = File.ReadAllLines(processedFilesTxt);
            }

            Environment.CurrentDirectory = options.DocsPath;

            var streamWriter = File.AppendText(processedFilesTxt);
            var lockObj = new object();

            var files = Directory.EnumerateFiles(".", "*.*", SearchOption.AllDirectories);

            Parallel.ForEach(files, new ParallelOptions {MaxDegreeOfParallelism = 5}, file =>
            {
                if (!file.EndsWith(".png", true, CultureInfo.CurrentCulture) &&
                    !file.EndsWith(".jpg", true, CultureInfo.CurrentCulture))
                {
                    return;
                }

                if (processedFiles.Any(s => s == file))
                {
                    Console.Out.WriteLine($"Skipping {file}");

                    return;
                }

                Task.Run(async () =>
                {
                    Source source;
                    try
                    {
                        source = await Tinify.FromFile(file);

                    }
                    catch (System.Exception e)
                    {
                        await Console.Out.WriteLineAsync($"Failed to process {file}. {e}");
                        return;
                    }

                    if (source == null)
                    {
                        return;
                    }

                    await source.ToFile(file);
                    lock (lockObj)
                    {
                        streamWriter.WriteLine(file);
                        streamWriter.Flush();
                    }
                    await Console.Out.WriteLineAsync($"Processed {file}");
                }).ConfigureAwait(false).GetAwaiter().GetResult();
            });
        }
    }
}
