using System;
using System.Text;
using System.Linq;

namespace k5tool
{
    public enum ModulationAssign
    {
        DFGLFO,
        DHG,
        Cutoff,
        Slope,
        Off
    }

    public enum SourceMode
    {
        Twin,
        Full
    }

    public enum PicMode
    {
        S1,
        S2,
        Both
    }

    public struct SourceSettings
    {
        public byte Delay; // 0~31
        public sbyte PedalDepth; // 0~±31
        public sbyte WheelDepth; // 0~±31
        public ModulationAssign PedalAssign;
        public ModulationAssign WheelAssign;

        public override string ToString()
        {
            return $"Delay = {Delay}, pedal depth = {PedalDepth}, wheel depth = {WheelDepth}, pedal assign = {PedalAssign}, wheel assign = {WheelAssign}";
        }
    }

    public class Single
    {
        public string Name;
        public byte Volume;  // 0~63
        public sbyte Balance; // 0~±31
        public SourceSettings Source1Settings;
        public SourceSettings Source2Settings;
        public bool Portamento;
        public byte PortamentoSpeed; // 0~63
        public SourceMode SMode;
        public PicMode PMode;
        public Source Source1;
        public Source Source2;

        public Single(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // will be reused when getting the next byte

            Name = GetName(data, offset);

            offset += 8;
            (Volume, offset) = Util.GetNextByte(data, offset);  // S9

            (b, offset) = Util.GetNextByte(data, offset);
            Balance = (sbyte)b; // S10

            // Source 1 and Source 2 settings.
        	// Note that the pedal assign and the wheel assign for one source are in the same byte.
            SourceSettings s1s;
            s1s.Delay = data[offset];          // S11
            s1s.PedalDepth = (sbyte)data[offset + 2]; // S13
            s1s.WheelDepth = (sbyte)data[offset + 4]; // S15
            s1s.PedalAssign = (ModulationAssign)Util.HighNybble(data[offset + 6]); // S17 high nybble
            s1s.WheelAssign = (ModulationAssign)Util.LowNybble(data[offset + 6]); // S17 low nybble

            SourceSettings s2s;
            s2s.Delay = data[offset + 1];                           // S12
            s2s.PedalDepth = (sbyte)data[offset + 3];               // S14
            s2s.WheelDepth = (sbyte)data[offset + 5];               // S16
            s2s.PedalAssign = (ModulationAssign)Util.HighNybble(data[offset + 7]); // S18 high nybble
            s2s.WheelAssign = (ModulationAssign)Util.LowNybble(data[offset + 7]); // S18 low nybble

            Source1Settings = s1s;
            Source2Settings = s2s;

            offset += 8;  // advance past the source settings

	        // portamento and p. speed - S19
            (b, offset) = Util.GetNextByte(data, offset);
            Portamento = b.IsBitSet(7);  // use the byte extensions defined in Util.cs
	        PortamentoSpeed = (byte)(b & 0x3f); // 0b00111111

            // mode and "pic mode" - S20
	        (b, offset) = Util.GetNextByte(data, offset);
	        SMode = b.IsBitSet(2) ? SourceMode.Full : SourceMode.Twin;

            byte picModeValue = (byte)(b & 0x03);
	        switch (picModeValue) 
            {
	        case 0:
		        PMode = PicMode.S1;
                break;
	        case 1:
		        PMode = PicMode.S2;
                break;
	        case 2:
		        PMode = PicMode.Both;
                break;
	        default:
		        PMode = PicMode.Both;
                break;
            }

            // Could have handled the source settings as part of the source data,
            // but the portamento and mode settings would have to be special cased.
            // So just copy everything past that, and use it as the source data.

            byte[] sourceData = new byte[data.Length];
            int dataLength = data.Length - offset;
            Array.Copy(data, offset, sourceData, 0, dataLength);
            //System.Console.WriteLine(Util.HexDump(sourceData));

            // Separate S1 and S2 data. Even bytes are S1, odd bytes are S2.
            // Note that this kind of assumes that the original data length is even.
            byte[] s1d = new byte[dataLength / 2];
            byte[] s2d = new byte[dataLength / 2];
            for (int src = 0, dst = 0; src < dataLength; src += 2, dst++)
            {
                s1d[dst] = sourceData[src];
                s2d[dst] = sourceData[src + 1];
            }

            System.Console.WriteLine("Source 1 data:");
            System.Console.WriteLine(Util.HexDump(s1d));
            Source1 = new Source(s1d);

            System.Console.WriteLine("Source 2 data:");
            System.Console.WriteLine(Util.HexDump(s2d));
            Source2 = new Source(s2d);
        }

        private string GetName(byte[] data, int offset)
        {
            // Brute-forcing the name in: S1 ... S8
            byte[] bytes = { data[0], data[1], data[2], data[3], data[4], data[5], data[6], data[7] };
        	string name = Encoding.ASCII.GetString(bytes);
            return name;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(String.Format("*SINGLE BASIC*          {0}\n\n", Name));
            builder.Append(String.Format("-----|  S1  |  S2  |   NAME={0}\n", Name));
            builder.Append(String.Format("BAL  |      {0}      |   MODE={1}\n", Balance, SMode));
            builder.Append(String.Format("DELAY|  {0,2}  |  {1,2}   |    VOL ={2}\n", Source1Settings.Delay, Source2Settings.Delay, Volume));
            builder.Append(String.Format("PEDAL|{0}|{1}|   POR ={2}--SPD={3}\n", Source1Settings.PedalAssign, Source2Settings.PedalAssign, Portamento, PortamentoSpeed));
            builder.Append(String.Format("P DEP| {0,3}  | {1,3}  |\n", Source1Settings.PedalDepth, Source2Settings.PedalDepth));;
            builder.Append(String.Format("WHEEL|{0}|{1}|\n", Source1Settings.WheelAssign, Source2Settings.WheelAssign));
            builder.Append(String.Format("W DEP| {0,2} | {1,2}  |\n", Source1Settings.WheelDepth, Source2Settings.WheelDepth));

            builder.Append("\n");
            builder.Append(Source1.ToString());
            builder.Append("\n");
            builder.Append(Source2.ToString());
            builder.Append("\n");

            return builder.ToString();
        }
    }
}