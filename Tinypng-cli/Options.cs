using CommandLine;

namespace Tinypng_cli
{
    class Options
    {
        [Option('p', "path", Required = true, HelpText = "Path of docs.")]
        public string DocsPath { get; set; }

        [Option('k', "key", Required = true, HelpText = "Tinify API key.")]
        public string TinifyKey { get; set; }
    }
}
