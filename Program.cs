using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;


namespace k5tool
{
    public struct SystemExclusiveHeader
    {
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

    class Program
    {
        private const int SystemExclusiveHeaderLength = 8;

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
            if (args.Length < 2)
            {
                System.Console.WriteLine("Usage: k5tool cmd filename.syx");
                return 1;
            }

            string command = args[0];
            string fileName = args[1];
            string patchName = "";
            if (args.Length > 2)
            {
                patchName = args[2];
            }

            byte[] fileData = File.ReadAllBytes(fileName);
            System.Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            System.Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                SystemExclusiveHeader header = GetSystemExclusiveHeader(message);
                // TODO: Check the SysEx file header for validity

                // Process only "one block data dump" type messages for now
		        if (header.Function != 0x20) 
                {
			        //System.Console.WriteLine("Not a one block data dump (function 20H), ignoring");
        			continue;
		        }

		        if (header.Substatus1 != 0x00) 
                {
			        //System.Console.WriteLine("Not a SINGLE dump, ignoring");
			        continue;
		        }

                byte programNumber = 0;
                if (command.Equals("extract"))
                {
                    programNumber = GetProgramNumber(patchName);
                    if (header.Substatus2 != programNumber)
                    {
                        continue;
                    }

                    System.Console.WriteLine($"Extracting patch {patchName} (program {programNumber}). sub status 2 = {header.Substatus2}");
                }

                System.Console.WriteLine(String.Format("==========\nProgram number = {0}, message length = {1}", header.Substatus2, message.Length));
                //System.Console.WriteLine(Util.HexDump(message));

                //System.Console.WriteLine("SysEx header: {0}", header);
                //System.Console.WriteLine("Patch = {0}", GetPatchName(header.Substatus2));

                // Extract the patch bytes (discarding the SysEx header)
                int dataLength = message.Length - SystemExclusiveHeaderLength;
                byte[] rawData = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeaderLength, rawData, 0, dataLength);
                //System.Console.WriteLine(String.Format("After SysEx header of {0} bytes, got {1} bytes of data.", SystemExclusiveHeaderLength, dataLength));
                //System.Console.WriteLine(Util.HexDump(rawData));
                
                byte[] data = Util.ConvertFromTwoNybbleFormat(rawData);
                //System.Console.WriteLine(String.Format("Converted from two-nybble format, that makes {0} bytes:", data.Length));
                //System.Console.WriteLine(Util.HexDump(data));

                Single s = new Single(data);
                if (command.Equals("dump"))
                {
                    System.Console.WriteLine(s.ToString());

                    System.Console.WriteLine(String.Format("Original single data = {0}", data.Length));
                    System.Console.WriteLine(Util.HexDump(data));

                    byte[] sd = s.ToData();
                    //System.Console.WriteLine("Converted to data model and back to bytes:");
                    System.Console.WriteLine(String.Format("Emitted single data = {0}", sd.Length));
                    System.Console.WriteLine(Util.HexDump(sd));

                    bool result = false;
                    int index = -1;
                    (result, index) = Util.ByteArrayCompare(data, sd);
                    if (!result)
                    {
                        if (index == -1)
                        {
                            System.Console.WriteLine("Byte arrays are of different lengths");
                        }
                        else
                        {
                            System.Console.WriteLine($"Byte arrays differ at index {index}");
                        }
                    }
                    else
                    {
                        System.Console.WriteLine("Byte arrays are identical, yay!");
                    }

                    string output = JsonConvert.SerializeObject(s, Formatting.Indented);
                    System.Console.WriteLine(String.Format("Single patch as JSON:\n{0}", output));
                }
                else if (command.Equals("list"))
                {
                    System.Console.WriteLine(String.Format("{0} {1}", GetPatchName(header.Substatus2), s.Name));
                }
                else if (command.Equals("extract"))
                {
                    //System.Console.WriteLine(String.Format("SIA-1 = {0}", GetProgramNumber("SIA-1")));
                    //System.Console.WriteLine(String.Format("SIC-6 = {0}", GetProgramNumber("SIC-6")));
                    //System.Console.WriteLine(String.Format("SID-12 = {0}", GetProgramNumber("SID-12")));

                    int channel = 1;
                    byte[] newHeader = new byte[] { 0xF0, 0x40, (byte)(channel - 1), 0x20, 0x00, 0x02, 0x00, programNumber };

                    List<byte> newData = new List<byte>();
                    newData.AddRange(newHeader);
                    newData.AddRange(Util.ConvertToTwoNybbleFormat(s.ToData()));
                    newData.Add(0xf7);

                    System.Console.WriteLine("Extracting {0}, new SysEx data:\n{1}", patchName, Util.HexDump(newData.ToArray()));

                    var folder = Environment.SpecialFolder.Desktop;
                    var newFileName = Path.Combine(new string[] { Environment.GetFolderPath(folder), $"{s.Name.Trim()}.syx" });
                    System.Console.WriteLine($"Writing SysEx data to '{newFileName}'...");
                    File.WriteAllBytes(newFileName, newData.ToArray());
                }
                else if (command.Equals("generate"))
                {
                    foreach (string waveformName in LeiterEngine.WaveformParameters.Keys)
                    {
                        byte[] harmonicLevels = LeiterEngine.GetHarmonicLevels(waveformName, 63);
                        System.Console.WriteLine(String.Format("Harmonic levels for '{0}':\n{1}", waveformName, Util.HexDump(harmonicLevels)));
                    }
                }
                else if (command.Equals("create"))
                {
                    int pn = GetProgramNumber("SIA-5");
                    if (header.Substatus2 == pn)
                    {
                        // Modify the existing patch a little bit:
                        s.Name = "FRANKENP";
                        byte[] newHarmonics = LeiterEngine.GetHarmonicLevels("square", Source.HarmonicCount);
                        for (int i = 0; i < Source.HarmonicCount; i++)
                        {
                            s.Source1.Harmonics[i].Level = newHarmonics[i];
                            s.Source2.Harmonics[i].Level = newHarmonics[i];
                        }
                        s.Source1.Amplifier.EnvelopeSegments = Amplifier.Envelopes["regular"].Segments;
                        s.Source2.Amplifier.EnvelopeSegments = Amplifier.Envelopes["regular"].Segments;

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
                    }
                }
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
    }
}
