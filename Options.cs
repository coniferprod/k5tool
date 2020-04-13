using System;

using CommandLine;

namespace k5tool
{
    [Verb("create", HelpText = "Create new patch or bank.")]
    class CreateOptions
    {
        [Value(0, MetaName = "type", HelpText = "Patch type: single or multi.")]
        public string Type { get; set; }
    }

    [Verb("list", HelpText = "List contents of patch or bank.")]
    public class ListOptions
    {
        [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true)]
        public string FileName { get; set; }
    }

    [Verb("generate", HelpText = "Generate harmonic levels for a waveform.")]
    public class GenerateOptions
    {
        [Value(0, MetaName = "waveform", HelpText = "Name of waveform to generate.", Required = true)]
        public string WaveformName { get; set; }
    }

    [Verb("dump", HelpText = "Dump information about a patch.")]
    public class DumpOptions
    {
        [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true)]
        public string FileName { get; set; }

        [Value(0, MetaName = "patch number", HelpText = "Number of patch (ignored if input file represent one patch).")]
        public string PatchNumber { get; set; }
    }

    [Verb("extract", HelpText = "Extract a patch in System Exclusive format.")]
    public class ExtractOptions
    {
        [Value(0, MetaName = "input file", HelpText = "Input file to be processed.", Required = true)]
        public string FileName { get; set; }

        [Value(0, MetaName = "patch number", HelpText = "Number of patch to extract.", Required = true)]
        public string PatchNumber { get; set; }

        [Value(1, MetaName = "channel", HelpText = "MIDI channel to write to SysEx file.")]
        public int Channel { get; set; }
    }
    
}