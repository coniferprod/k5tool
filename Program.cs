using System;
using System.Collections.Generic;
using System.IO;

using CommandLine;

using KSynthLib.Common;
using KSynthLib.K5;

namespace k5tool
{
    class Program
    {
        static int Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<CreateOptions, ListOptions, GenerateOptions, DumpOptions, ExtractOptions>(args);
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

            SinglePatch s = new SinglePatch();
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

            Console.WriteLine("Creating new patch, new SysEx data:\n{0}", Util.HexDump(newData.ToArray()));

            var folder = Environment.SpecialFolder.Desktop;
            var newFileName = Path.Combine(new string[] { Environment.GetFolderPath(folder), "Frankenpatch.syx" });
            Console.WriteLine($"Writing SysEx data to '{newFileName}'...");
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
                SystemExclusiveHeader header = new SystemExclusiveHeader(message);

                if (header.Substatus2 != programNumber)
                {
                    continue;
                }

                byte[] rawData = ExtractPatchData(message);
                byte[] data = Util.ConvertFromTwoNybbleFormat(rawData);

                SinglePatch s = new SinglePatch(data);

                int channel = opts.Channel - 1;  // adjust channel 1...16 to range 0...15
                byte[] newHeader = new byte[] { 0xF0, 0x40, (byte)(channel - 1), 0x20, 0x00, 0x02, 0x00, (byte)programNumber };

                List<byte> newData = new List<byte>();
                newData.AddRange(newHeader);
                newData.AddRange(Util.ConvertToTwoNybbleFormat(s.ToData()));
                newData.Add(0xf7);

                var folder = Environment.SpecialFolder.Desktop;
                var newFileName = Path.Combine(new string[] { Environment.GetFolderPath(folder), $"{s.Name.Trim()}.syx" });
                Console.WriteLine($"Writing SysEx data to '{newFileName}'...");
                File.WriteAllBytes(newFileName, newData.ToArray());
            }

            return 0;
        }

        public static int RunGenerateAndReturnExitCode(GenerateOptions opts)
        {
            string waveformName = opts.WaveformName;
            byte[] harmonicLevels = LeiterEngine.GetHarmonicLevels(waveformName, 63);
            Console.WriteLine("Harmonic levels for '{0}':", waveformName);
            for (int i = 0; i < harmonicLevels.Length; i++)
            {
                Console.WriteLine(String.Format("{0}: {1}", i, harmonicLevels[i]));
            }
            Console.WriteLine(Util.HexDump(harmonicLevels));
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

                SystemExclusiveHeader header = new SystemExclusiveHeader(message);
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

                SinglePatch s = new SinglePatch(data);
                System.Console.WriteLine(s.ToString());

            }

            return 0;
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
