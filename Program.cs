using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;
using CommandLine;

namespace k5tool
{
    public struct SystemExclusiveHeader
    {
        public const int DataSize = 7;

        public byte ManufacturerID;
	    public byte Channel;
	    public byte Function;
	    public byte Group;
	    public byte MachineID;
	    public byte Substatus1;
	    public byte Substatus2;

        public override string ToString()
        {
            return String.Format("ManufacturerID = {0,2:X2}H, Channel = {1}, Function = {2,2:X2}H, Group = {3,2:X2}H, MachineID = {4,2:X2}H, Substatus1 = {5,2:X2}H, Substatus2 = {6,2:X2}H", ManufacturerID, Channel, Function, Group, MachineID, Substatus1, Substatus2);
        }
    }

    public enum SystemExclusiveFunction
    {
        OneBlockDataRequest = 0x00,
        AllBlockDataRequest = 0x01,
        ParameterSend = 0x10,
        OneBlockDataDump = 0x20,
        AllBlockDataDump = 0x21,
        ProgramSend = 0x30,
        WriteComplete = 0x40,
        WriteError = 0x41,
        WriteErrorProtect = 0x42,
        WriteErrorNoCard = 0x43,
        MachineIDRequest = 0x60,
        MachineIDAcknowledge = 0x61
    }

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

    class Program
    {
        static SystemExclusiveHeader GetSystemExclusiveHeader(byte[] data)
        {
            SystemExclusiveHeader header;
            // data[0] is the SysEx identifier F0H
            header.ManufacturerID = data[1];
            header.Channel = data[2];
		    header.Function = data[3];
		    header.Group = data[4];
		    header.MachineID = data[5];
		    header.Substatus1 = data[6];
		    header.Substatus2 = data[7];
            return header;
        }

        static int Main(string[] args)
        {
            for (int i = 0; i < 48; i++)
            {
                Console.WriteLine(GetProgramName(i));
            }

            var parserResult = Parser.Default.ParseArguments<CreateOptions, GenerateOptions, DumpOptions, ExtractOptions>(args);
            parserResult.MapResult(
                (CreateOptions opts) => RunCreateAndReturnExitCode(opts),
                (ListOptions opts) => RunListAndReturnExitCode(opts),
                (GenerateOptions opts) => RunGenerateAndReturnExitCode(opts),
                (DumpOptions opts) => RunDumpAndReturnExitCode(opts),
                (ExtractOptions opts) => RunExtractAndReturnExitCode(opts),
                errs => 1
            );

            return 0;
        }

        public static byte[] ExtractPatchData(byte[] message)
        {
            int dataLength = message.Length - SystemExclusiveHeader.DataSize;
            byte[] rawData = new byte[dataLength];
            Array.Copy(message, SystemExclusiveHeader.DataSize, rawData, 0, dataLength);
            return rawData;
        }

        public static int RunCreateAndReturnExitCode(CreateOptions opts)
        {
            //byte[] data = ExtractPatchData(message);
            //Single s = new Single(data);

            Single s = new Single();
            s.Name = "FRANKENP";

            byte[] newHarmonics = LeiterEngine.GetHarmonicLevels("saw", Source.HarmonicCount);
            for (int i = 0; i < Source.HarmonicCount; i++)
            {
                s.Source1.Harmonics[i].Level = newHarmonics[i];
                s.Source2.Harmonics[i].Level = newHarmonics[i];
            }
            s.Source1.Amplifier.EnvelopeSegments = Amplifier.Envelopes["regular"].Segments;
            s.Source2.Amplifier.EnvelopeSegments = Amplifier.Envelopes["silent"].Segments;

            int channel = 1;
            byte[] newHeader = new byte[] { 0xF0, 0x40, (byte)(channel - 1), 0x20, 0x00, 0x02, 0x00, 0x2F };
            List<byte> newData = new List<byte>();
            newData.AddRange(newHeader);
            newData.AddRange(Util.ConvertToTwoNybbleFormat(s.ToData()));
            newData.Add(0xf7);

            System.Console.WriteLine("Creating new patch, new SysEx data:\n{0}", Util.HexDump(newData.ToArray()));

            var folder = Environment.SpecialFolder.Desktop;
            var newFileName = Path.Combine(new string[] { Environment.GetFolderPath(folder), "Frankenpatch.syx" });
            System.Console.WriteLine($"Writing SysEx data to '{newFileName}'...");
            File.WriteAllBytes(newFileName, newData.ToArray());

            return 0;
        }

        public static int RunListAndReturnExitCode(ListOptions opts)
        {
            Console.WriteLine("List command not implemented yet.");
            return 0;
        }

        public static int RunExtractAndReturnExitCode(ExtractOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            Console.WriteLine($"Got {messages.Count} messages");

            int programNumber = GetProgramNumber(opts.PatchNumber);

            foreach (byte[] message in messages)
            {
                SystemExclusiveHeader header = GetSystemExclusiveHeader(message);

                if (header.Substatus2 != programNumber)
                {
                    continue;
                }

                byte[] rawData = ExtractPatchData(message);
                byte[] data = Util.ConvertFromTwoNybbleFormat(rawData);

                Single s = new Single(data);

                int channel = opts.Channel - 1;  // adjust channel 1...16 to range 0...15
                byte[] newHeader = new byte[] { 0xF0, 0x40, (byte)(channel - 1), 0x20, 0x00, 0x02, 0x00, (byte)programNumber };

                List<byte> newData = new List<byte>();
                newData.AddRange(newHeader);
                newData.AddRange(Util.ConvertToTwoNybbleFormat(s.ToData()));
                newData.Add(0xf7);

                var folder = Environment.SpecialFolder.Desktop;
                var newFileName = Path.Combine(new string[] { Environment.GetFolderPath(folder), $"{s.Name.Trim()}.syx" });
                System.Console.WriteLine($"Writing SysEx data to '{newFileName}'...");
                File.WriteAllBytes(newFileName, newData.ToArray());
            }

            return 0;
        }

        public static int RunGenerateAndReturnExitCode(GenerateOptions opts)
        {
            //string waveformName = opts.WaveformName;
            foreach (string waveformName in LeiterEngine.WaveformParameters.Keys)
            {
                byte[] harmonicLevels = LeiterEngine.GetHarmonicLevels(waveformName, 63);
                System.Console.WriteLine(String.Format("Harmonic levels for '{0}':\n{1}", waveformName, Util.HexDump(harmonicLevels)));
            }

            return 0;
        }

        public static int RunDumpAndReturnExitCode(DumpOptions opts)
        {
            string fileName = opts.FileName;
            byte[] fileData = File.ReadAllBytes(fileName);
            Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                byte[] rawData = ExtractPatchData(message);
                byte[] data = Util.ConvertFromTwoNybbleFormat(rawData);

                SystemExclusiveHeader header = GetSystemExclusiveHeader(message);
                Console.WriteLine(header);
                // TODO: Check the SysEx file header for validity

                Console.WriteLine(String.Format("Function = {0}", ((SystemExclusiveFunction) header.Function)).ToString().ToUpper());

		        if (header.Substatus1 == 0x00) 
                {
			        Console.WriteLine("SINGLE");
		        }
                else if (header.Substatus1 == 0x01)
                {
                    Console.WriteLine("MULTI");
                }

                Console.WriteLine(String.Format("Program = {0}", header.Substatus2));
                int programNumber = header.Substatus2;
                string programType = "INTERNAL";
                if (programNumber >= 48)
                {
                    programType = "EXTERNAL";
                    programNumber -= 48;
                }
                string programName = GetProgramName(programNumber);
                Console.WriteLine(String.Format("{0} {1}", programType, programName));

                Single s = new Single(data);
                System.Console.WriteLine(s.ToString());

            }

            return 0;
        }

        public static string GetPatchName(byte p, int patchCount = 12)
        {
        	int bankIndex = p / patchCount;
	        char bankLetter = "ABCD"[bankIndex];
	        int patchIndex = (p % patchCount) + 1;

	        return String.Format("{0}-{1,2}", bankLetter, patchIndex);
        }

        // Expects the usual Kawai K5 patch names from "SIA-1" to "SID-12".
        // S = Single, I = Internal, ABCD = Bank, 1...12 = number in bank
        public static byte GetProgramNumber(string patchName)
        {
            // Right now we only care about internal, single banks, so assume S and I.
            char bankName = patchName[2];
            string patchNumberString = patchName.Substring(4);
            int patchNumber = Int32.Parse(patchNumberString);
            int bankNumber = "ABCD".IndexOf(bankName);
            return (byte)((bankNumber * 12 + patchNumber) - 1);
        }

        public static string GetProgramName(int programNumber)
        {
            string bankName = "ABCD".Substring(programNumber / 12, 1);
            string patchNumber = Convert.ToString((programNumber % 12) + 1);
            return String.Format("{0}-{1}", bankName, patchNumber);
        }
    }
}
