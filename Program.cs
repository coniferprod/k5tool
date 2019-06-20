using System;
using System.Collections.Generic;
using System.IO;

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

            byte[] fileData = File.ReadAllBytes(fileName);
            System.Console.WriteLine($"SysEx file: '{fileName}' ({fileData.Length} bytes)");

            List<byte[]> messages = Util.SplitBytesByDelimiter(fileData, 0xf7);
            System.Console.WriteLine($"Got {messages.Count} messages");

            foreach (byte[] message in messages)
            {
                //System.Console.WriteLine(HexDump(message));

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

                //System.Console.WriteLine("SysEx header: {0}", header);
                //System.Console.WriteLine("Patch = {0}", GetPatchName(header.Substatus2));

                // Extract the patch bytes (discarding the SysEx header)
                int dataLength = message.Length - SystemExclusiveHeaderLength;
                byte[] rawData = new byte[dataLength];
                Array.Copy(message, SystemExclusiveHeaderLength, rawData, 0, dataLength);
                //System.Console.WriteLine(HexDump(rawData));

                byte[] data = Util.ConvertFromTwoNybbleFormat(rawData);
                //System.Console.WriteLine(Util.HexDump(data));

                Single s = new Single(data);
                if (command.Equals("dump"))
                {
                    System.Console.WriteLine(s.ToString());
                }
                else if (command.Equals("list"))
                {
                    System.Console.WriteLine(String.Format("{0} {1}", GetPatchName(header.Substatus2), s.Name));
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
    }
}
