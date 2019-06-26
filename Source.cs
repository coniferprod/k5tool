using System;
using System.Text;
using System.Collections.Generic;

namespace k5tool
{
    public enum KeyTracking
    {
        Track,
        Fixed
    }

    public struct PitchEnvelopeSegment  // DFG ENV
    {
        public byte Rate; // 0~31
        public sbyte Level; // 0~±31

        public override string ToString()
        {
            return $"Rate={Rate} Level={Level}";
        }
    }

    public struct PitchEnvelope
    {
        public PitchEnvelopeSegment[] Segments;
        public bool IsLooping;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("*DFG ENV*\n");
            builder.Append("    SEG  | 1 | 2 | 3 | 4 | 5 | 6 |\n");
            builder.Append("    ------------------------------\n");
            builder.Append("    RATE |");
            for (int i = 0; i < Segments.Length; i++)
            {
                builder.Append(String.Format("{0,3}|", Segments[i].Rate));
            }
            builder.Append("\n");
            builder.Append("    LEVEL|");
            for (int i = 0; i < Segments.Length; i++)
            {
                //string levelString = Segments[i].Level.ToString("+#;-#;0");
                //builder.Append(levelString);
                //builder.Append("|");
                builder.Append(String.Format("{0,3}|", Segments[i].Level));
            }
            builder.Append("\n\n");
            builder.Append("    LOOP<3-4>=");
            builder.Append(IsLooping ? "YES" : "--");
            builder.Append("\n\n");
            return builder.ToString();
        }
    }

    public struct EnvelopeSegment
    {
        public byte Rate;
        public byte Level;

        // true if this segment is the MAX in this envelope, false otherwise
        public bool IsMax;
        public bool IsMod;
    }
    
    public struct Harmonic
    {
        public byte Level;
	    public bool IsModulationActive;  // true if modulation is on for the containing source
	    public byte EnvelopeNumber; // user harmonic envelope number 0/1, 1/2, 2/3 or 3/4
    }

    public struct HarmonicEnvelopeSegment
    {
        public bool IsMaxSegment;
        public byte Level; // 0~31
        public byte Rate;  // 0~31
    }

    public struct HarmonicEnvelope
    {
        public HarmonicEnvelopeSegment[] Segments;

        public bool IsActive;
	
	    public byte Effect; // 0~31 (SysEx manual says "s<x> env<y> off", maybe should be "eff"?)
    }

    public enum HarmonicSelection
    {
        Live,
        Die,
        All
    }

    public struct HarmonicModulation
    {
        public bool IsOn;  // will the selected harmonic be modulated (provided that master mod is on)
	    public byte EnvelopeNumber;  // assigns the selected harmonic to one of the four DHG envelopes
    }

    public enum HarmonicAngle
    {
        Negative = 0,
        Neutral,
        Positive
    }

    public struct HarmonicSettings
    {
	    public sbyte VelocityDepth;  // 0~±31
	    public sbyte PressureDepth;  // 0~±31
    	public sbyte KeyScalingDepth;  // 0~±31
	    public byte LFODepth; // 0~31
	    public HarmonicEnvelope[] Envelopes;
	    public bool IsModulationActive; // master modulation control - if false, all DHG modulation is defeated
	    public HarmonicSelection Selection;
	    public byte RangeFrom; // 1~63
	    public byte RangeTo; // 1~63

	    public HarmonicModulation Odd;
	    public HarmonicModulation Even;
	    public HarmonicModulation Octave;
	    public HarmonicModulation Fifth;
	    public HarmonicModulation All;

    	public HarmonicAngle Angle; // 0/-, 1/0, 1/+ (maybe should be 2/+ ?)
	    public byte HarmonicNumber; // 1~63

	    public bool IsShadowOn;  // this is in S285 bit 7

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(String.Format("*DHG 1*  MOD={0}\n\n", IsModulationActive ? "ON" : "OFF"));
            builder.Append(String.Format("*DHG 2*\n<DEPTH>\n VEL={0,3}  KS={1,3}\n PRS={2,3} LFO={3,3}\n\n",
                VelocityDepth, KeyScalingDepth, PressureDepth, LFODepth));
            builder.Append("ENV|");
            for (int i = 0; i < Source.HarmonicEnvelopeCount; i++)
            {
                builder.Append(String.Format("{0,2}|", i + 1));
            }
            builder.Append("\nACT|");
            for (int i = 0; i < Source.HarmonicEnvelopeCount; i++)
            {
                builder.Append(String.Format("{0}|", Envelopes[i].IsActive ? "ON" : "--"));
            }
            builder.Append("\nEFF|");
            for (int i = 0; i < Source.HarmonicEnvelopeCount; i++)
            {
                builder.Append(String.Format("{0,2}|", Envelopes[i].Effect));
            }
            builder.Append("\n\n");

            builder.Append("*DHG ENV*\n\nSEG |");
            for (int i = 0; i < Source.HarmonicEnvelopeSegmentCount; i++)
            {
                builder.Append(String.Format("{0,5}|", i + 1));
            }
            builder.Append("\n----|RT|LV|RT|LV|RT|LV|RT|LV|RT|LV|RT|LV|\n");

            for (int ei = 0; ei < Source.HarmonicEnvelopeCount; ei++) 
            {
                builder.Append(String.Format("ENV{0}|", ei + 1));
                for (int si = 0; si < Source.HarmonicEnvelopeSegmentCount; si++)
                {
                    HarmonicEnvelopeSegment segment = Envelopes[ei].Segments[si];
                    string levelString = segment.IsMaxSegment ? " *" : String.Format("{0,2}", segment.Level);
                    builder.Append(String.Format("{0,2}|{1}|", segment.Rate, levelString));
                }
                builder.Append("\n");
            }
            builder.Append("MAX  .... SHADOW=");
            builder.Append(IsShadowOn ? "ON" : "--");
            builder.Append("\n\n");

            return builder.ToString();
        }

        const int DataLength = 16 + 4 * 12; // without the 63 harmonic levels

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add(VelocityDepth.ToByte());
            data.Add(PressureDepth.ToByte());
            data.Add(KeyScalingDepth.ToByte());
            data.Add(LFODepth);

            byte b = 0;
            for (int i = 0; i < Source.HarmonicEnvelopeCount; i++)
            {
                b = Envelopes[i].Effect;
                if (Envelopes[i].IsActive)
                {
                    b = b.SetBit(7);
                }
                data.Add(b);
            }

            b = (byte)Selection;
            if (IsModulationActive)
            {
                b = b.SetBit(7);
            }
            data.Add(b);

            data.Add(RangeFrom);
            data.Add(RangeTo);

            byte lowNybble = (byte)(Even.EnvelopeNumber - 1);
            if (Even.IsOn)
            {
                lowNybble = lowNybble.SetBit(3);
            }
            byte highNybble = (byte)(Odd.EnvelopeNumber - 1);
            if (Odd.IsOn)
            {
                highNybble = highNybble.SetBit(3);
            }
            b = Util.ByteFromNybbles(highNybble, lowNybble);
            System.Console.WriteLine(String.Format("odd-even byte = {0:X2}", b));
            data.Add(b);

            lowNybble = (byte)(Fifth.EnvelopeNumber - 1);
            if (Fifth.IsOn)
            {
                lowNybble = lowNybble.SetBit(3);
            }
            highNybble = (byte)(Octave.EnvelopeNumber - 1);
            if (Octave.IsOn)
            {
                highNybble = highNybble.SetBit(3);
            }
            b = Util.ByteFromNybbles(highNybble, lowNybble);
            System.Console.WriteLine(String.Format("fifth-octave byte = {0:X2}", b));
            data.Add(b);

            lowNybble = 0;
            highNybble = (byte)(All.EnvelopeNumber - 1);
            if (All.IsOn)
            {
                highNybble = highNybble.SetBit(3);
            }
            b = Util.ByteFromNybbles(highNybble, lowNybble);
            System.Console.WriteLine(String.Format("all byte = {0:X2}", b));
            data.Add(b);

            data.Add((byte)Angle);
            data.Add(HarmonicNumber);

            for (int ei = 0; ei < Source.HarmonicEnvelopeCount; ei++)
            {
                for (int si = 0; si < Source.HarmonicEnvelopeSegmentCount; si++)
                {
                    b = Envelopes[ei].Segments[si].Level;
                    if (Envelopes[ei].Segments[si].IsMaxSegment)
                    {
                        b = b.SetBit(6);
                    }
                    else
                    {
                        b = b.UnsetBit(6);
                    }
                    if (ei == 0)
                    {
                        if (IsShadowOn)
                        {
                            b = b.SetBit(7);
                        }
                        else
                        {
                            b = b.UnsetBit(7);
                        }
                    }
                    data.Add(b);
                }
                for (int si = 0; si < Source.HarmonicEnvelopeSegmentCount; si++)
                {
                    b = Envelopes[ei].Segments[si].Rate;
                    data.Add(b);
                }
            }

            if (data.Count != DataLength)
            {
                System.Console.WriteLine(String.Format("WARNING: DHG length, expected = {0}, actual = {1}", DataLength, data.Count));
            }

            return data.ToArray();
        }
    }

    public struct FilterEnvelopeSegment
    {
        public byte Rate;
        public bool IsMaxSegment;
        public byte Level;
    }

    public struct Filter
    {
        public byte Cutoff;
        public byte CutoffModulation;
        public byte Slope;
        public byte SlopeModulation;
        public byte FlatLevel;
        public sbyte VelocityDepth; // 0~±31
        public sbyte PressureDepth; // 0~±31
        public sbyte KeyScalingDepth;  // 0~±31
        public sbyte EnvelopeDepth; // 0~±31
        public sbyte VelocityEnvelopeDepth;  // 0~±31
        public bool IsActive;
        public bool IsModulationActive;
        public byte LFODepth; // 0~31
        public FilterEnvelopeSegment[] EnvelopeSegments;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("*DDF*=");
            builder.Append(IsActive ? "ON" : "--");
            builder.Append("   MOD=");
            builder.Append(IsModulationActive ? "ON" : "--");
            builder.Append("\n");
            builder.Append("                   <DEPTH>\n");
            builder.Append(String.Format(" CUTOFF={0,2}-MOD={1,2}  ENV={2,3}-VEL={3,3}\n", Cutoff, CutoffModulation, EnvelopeDepth, VelocityEnvelopeDepth));
            builder.Append(String.Format(" SLOPE ={0,2}-MOD={1,2}  VEL={2,3}\n", Slope, SlopeModulation, VelocityDepth));
            builder.Append(String.Format("FLAT.LV={0,2}         PRS={1,3}\n", FlatLevel, PressureDepth));
            builder.Append(String.Format("                    KS={0,3}\n", KeyScalingDepth));
            builder.Append(String.Format("                   LFO={0,3}\n", LFODepth));
            builder.Append("\n\n");

            builder.Append("*DDF ENV*\n\n    SEG  |");
            for (int i = 0; i < Source.FilterEnvelopeSegmentCount; i++)
            {
                builder.Append(String.Format("{0,3}|", i + 1));
            }
            builder.Append("\n    ------------------------------\n");
            builder.Append("    RATE |");
            for (int i = 0; i < Source.FilterEnvelopeSegmentCount; i++)
            {
                builder.Append(String.Format("{0,3}|", EnvelopeSegments[i].Rate));
            }
            builder.Append("\n    LEVEL|");
            for (int i = 0; i < Source.FilterEnvelopeSegmentCount; i++)
            {
                string levelString = EnvelopeSegments[i].IsMaxSegment ? "  *" : String.Format("{0,3}", EnvelopeSegments[i].Level);
                builder.Append(String.Format("{0}|", levelString));
            }
            builder.Append("\n\n    MAX SEG = ?\n\n");

            return builder.ToString();
        }

        const int DataLength = 23;

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();

            data.Add(Cutoff);
            data.Add(CutoffModulation);
            data.Add(Slope);
            data.Add(SlopeModulation);
            data.Add(FlatLevel);
            data.Add(VelocityDepth.ToByte());
            data.Add(PressureDepth.ToByte());
            data.Add(KeyScalingDepth.ToByte());
            data.Add(EnvelopeDepth.ToByte());
            data.Add(VelocityEnvelopeDepth.ToByte());

            byte b = LFODepth;
            if (IsModulationActive)
            {
                b = b.SetBit(6);
            }
            else
            {
                b = b.UnsetBit(6);
            }
            if (IsActive)
            {
                b = b.SetBit(7);
            }
            else
            {
                b = b.UnsetBit(7);
            }
            data.Add(b);

            for (int i = 0; i < Source.FilterEnvelopeSegmentCount; i++)
            {
                data.Add(EnvelopeSegments[i].Rate);
            }

            for (int i = 0; i < Source.FilterEnvelopeSegmentCount; i++)
            {
                b = EnvelopeSegments[i].Level;
                if (EnvelopeSegments[i].IsMaxSegment)
                {
                    b = b.SetBit(6);
                }
                else
                {
                    b = b.UnsetBit(6);
                }
                data.Add(b);
            }

            if (data.Count != DataLength)
            {
                System.Console.WriteLine(String.Format("WARNING: DDF length, expected = {0}, actual = {1}", DataLength, data.Count));
            }

            System.Console.WriteLine(String.Format("DDF data:\n{0}", Util.HexDump(data.ToArray())));

            return data.ToArray();
        }
    }


    public struct PitchSettings
    {
        public sbyte Coarse; // 0~±48
        public sbyte Fine; // 0~±31
        public KeyTracking KeyTracking;
        public byte Key;  // the keytracking key, zero if not used
        public sbyte EnvelopeDepth; // 0~±24
        public sbyte PressureDepth; // 0~±31
        public byte BenderDepth; // 0~24
        public sbyte VelocityEnvelopeDepth; // 0~±31
        public byte LFODepth; // 0~31
        public sbyte PressureLFODepth; // 0~±31
        public PitchEnvelope PitchEnvelope;

        const int DataLength = 21;

        public override string ToString()
        {
            return String.Format("*DFG*              \n\n" +
                "COARSE= {0,2}        <DEPTH>\n" + 
                "FINE  = {1,2}        ENV= {2,2}-VEL {3}\n" + 
                "                  PRS= {4}\n" + 
                "                  LFO= {5,2}-PRS= {6,3}\n" + 
                "KEY    ={7}     BND= {8}\n" + 
                "FIXNO  ={9}\n\n", 
                Coarse, Fine, EnvelopeDepth, VelocityEnvelopeDepth,
                PressureDepth, LFODepth, PressureLFODepth,
                KeyTracking, BenderDepth,
                Key) + 
                PitchEnvelope.ToString();
        }

        public byte[] ToData()
        {
            List<byte> data = new List<byte>();
            byte b = 0;

            data.Add(Coarse.ToByte());
            //System.Console.WriteLine(String.Format("coarse = {0:X2}", b));

            data.Add(Fine.ToByte());
            //System.Console.WriteLine(String.Format("fine = {0:X2}", b));

            b = Key;  // the tracking key if fixed, 0 if track
            if (KeyTracking == KeyTracking.Fixed)
            {
                b = b.SetBit(7);
            }
            else
            {
                b = b.UnsetBit(7);
            }
            data.Add(b);
            //System.Console.WriteLine(String.Format("key tracking = {0:X2}", b));

            data.Add(EnvelopeDepth.ToByte());
            //System.Console.WriteLine(String.Format("envelope depth = {0:X2}", b));

            data.Add(PressureDepth.ToByte());
            //System.Console.WriteLine(String.Format("pressure depth = {0:X2}", b));

            data.Add(BenderDepth);
            data.Add(VelocityEnvelopeDepth.ToByte());
            data.Add(LFODepth);
            data.Add(PressureLFODepth.ToByte());

            for (int i = 0; i < Source.PitchEnvelopeSegmentCount; i++)
            {
                b = PitchEnvelope.Segments[i].Rate;

                // Set the envelope looping bit for the first rate only:
                if (i == 0)
                {
                    if (PitchEnvelope.IsLooping)
                    {
                        b = b.SetBit(7);
                    }
                }
                data.Add(b);
            }

            for (int i = 0; i < Source.PitchEnvelopeSegmentCount; i++)
            {
                sbyte sb = PitchEnvelope.Segments[i].Level;
                data.Add(sb.ToByte());
            }

            if (data.Count != DataLength)
            {
                System.Console.WriteLine(String.Format("WARNING: DFG length, expected = {0}, actual = {1}", DataLength, data.Count));
            }

            return data.ToArray();
        }
    }

    public class Source
    {
        public const int EnvelopeSegmentCount = 6;
        public const int PitchEnvelopeSegmentCount = 6;
        public const int HarmonicCount = 63;
        public const int HarmonicEnvelopeCount = 4;
        public const int HarmonicEnvelopeSegmentCount = 6;
        public const int FilterEnvelopeSegmentCount = 6;
        public const int AmplifierEnvelopeSegmentCount = 7;
    
        public PitchSettings Pitch;
        public Harmonic[] Harmonics;
        public Harmonic Harmonic63bis;
        public HarmonicSettings HarmonicSettings;
        public Filter Filter;
        public Amplifier Amplifier;

        public int SourceNumber;

        public Source(byte[] data, int number)
        {
            SourceNumber = number;

            System.Console.WriteLine($"S{SourceNumber} data:");
            System.Console.WriteLine(Util.HexDump(data));

            int offset = 0;
            byte b = 0;  // reused when getting the next byte
            List<byte> buf = new List<byte>();

            // DFG
            Pitch = new PitchSettings();

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.Coarse = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.Fine = b.ToSignedByte();

	        (b, offset) = Util.GetNextByte(data, offset);
	        if (b.IsBitSet(7))
            {
                Pitch.KeyTracking = KeyTracking.Fixed;
                Pitch.Key = (byte)(b & 0b01111111);
            }
            else 
            {
                Pitch.KeyTracking = KeyTracking.Track;
                Pitch.Key = (byte)(b & 0b01111111);
            }
            // TODO: Check that the SysEx spec gets the meaning of b7 right

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.EnvelopeDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.PressureDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.BenderDepth = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.VelocityEnvelopeDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.LFODepth = b;

            (b, offset) = Util.GetNextByte(data, offset);
            Pitch.PressureLFODepth = b.ToSignedByte();

            Pitch.PitchEnvelope.Segments = new PitchEnvelopeSegment[PitchEnvelopeSegmentCount];
            for (int i = 0; i < PitchEnvelopeSegmentCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                buf.Add(b);
                if (i == 0)
                {
                    Pitch.PitchEnvelope.IsLooping = b.IsBitSet(7);
                    Pitch.PitchEnvelope.Segments[i].Rate = (byte)(b & 0x7f);
                }
                else
                {
                    Pitch.PitchEnvelope.Segments[i].Rate = b;
                }
            }

            for (int i = 0; i < PitchEnvelopeSegmentCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                buf.Add(b);
                Pitch.PitchEnvelope.Segments[i].Level = b.ToSignedByte();
            }

            System.Console.WriteLine(String.Format("Parsed DFG bytes:\n{0}", Util.HexDump(buf.ToArray())));
            
            // DHG

            //System.Console.WriteLine("Harmonic levels:");
            Harmonics = new Harmonic[HarmonicCount];
            for (int i = 0; i < HarmonicCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                Harmonics[i].Level = b;
                System.Console.Write(String.Format("{0,2}:{1,2}({1:X2}H) ", i + 1, b));
            }
            System.Console.WriteLine();
            //System.Console.WriteLine(String.Format("After harmonic levels, offset = {0}", offset));

            //System.Console.WriteLine("Harmonic modulation flags and envelope selections:");
            // The values are packed into 31 + 1 bytes. The first 31 bytes contain the settings
            // for harmonics 1 to 62. The solitary byte that follows has the harm 63 settings.
            List<byte> harmData = new List<byte>();
            int count = 0;
            byte lowNybble = 0;
            byte highNybble = 0;
            while (count < HarmonicCount - 1)
            {
		        (b, offset) = Util.GetNextByte(data, offset);
                (highNybble, lowNybble) = Util.NybblesFromByte(b);
                //System.Console.Write(String.Format("{0,2}:{1,2}({1:X2}H) ", count + 1, b));
		        //harmData[count] = (byte)(b & 0x0f); // 0b00001111
                harmData.Add(lowNybble);
                harmData.Add(highNybble);
                //System.Console.WriteLine(String.Format("{0:X2} => {1:X2} {2:X2}", b, lowNybble, highNybble));
                count += 2;
            }

            // NOTE: Seems that for harmonics with zero level and modulation off, the envelope number
            // could be something else than 0...3 (1...4). For example, 12 is a typical value.
            // Probably doesn't matter.

	        (b, offset) = Util.GetNextByte(data, offset);
            (highNybble, lowNybble) = Util.NybblesFromByte(b);
	        //harmData.Add((byte)(b & 0x0f)); // AND with 0b00001111
            //harmData.Add(b);
            harmData.Add(highNybble);
            Harmonic63bis = new Harmonic();
            Harmonic63bis.IsModulationActive = lowNybble.IsBitSet(2);
            Harmonic63bis.EnvelopeNumber = (byte)(lowNybble + 1);
            Harmonic63bis.Level = 99;  // don't care really

            //System.Console.Write(String.Format("{0,2}:{1,2}({1:X2}H) ", count + 1, b));
            //System.Console.WriteLine();

            // OK, here's the thing: there might be an error in the Kawai K5 System Exclusive specification
            // regarding the last harmonic (63) and how it is packed into a byte. Since it is not a very prominent
            // harmonic, we leave it as it is, even though it means that the comparison between parsed and
            // emitted SysEx data will fail. But it is close enough for now.

	        // Now harmData should have data for all the 63 harmonics
	        for (int i = 0; i < Harmonics.Length; i++) 
            {
		        Harmonics[i].IsModulationActive = harmData[i].IsBitSet(2);
		        Harmonics[i].EnvelopeNumber = (byte)(harmData[i] + 1);  // add one to make env number 1...4
                System.Console.WriteLine(String.Format("H{0} IsMod = {1} Env = {2}", i, Harmonics[i].IsModulationActive, Harmonics[i].EnvelopeNumber));
	        }

            // DHG harmonic settings (S253 ... S260)
            HarmonicSettings harmSet;

	        (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.VelocityDepth = b.ToSignedByte();

	        (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.PressureDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.KeyScalingDepth = b.ToSignedByte();

	        (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.LFODepth = b;

            // Harmonic envelope 1 - 4 settings (these will be augmented later in the process)
            harmSet.Envelopes = new HarmonicEnvelope[HarmonicEnvelopeCount];
            for (int i = 0; i < HarmonicEnvelopeCount; i++) 
            {
    	        (b, offset) = Util.GetNextByte(data, offset);
                harmSet.Envelopes[i].IsActive = b.IsBitSet(7);
			    harmSet.Envelopes[i].Effect = (byte)(b & 0x1f);
		    }

            // The master modulation setting is packed with the harmonic selection value 
    	    (b, offset) = Util.GetNextByte(data, offset);
	        harmSet.IsModulationActive = b.IsBitSet(7);

            HarmonicSelection selection = HarmonicSelection.All;
            byte v = (byte)(b & 0x03);
	        switch (v) {
	        case 0:
		        selection = HarmonicSelection.Live;
                break;
	        case 1:
		        selection = HarmonicSelection.Die;
                break;
	        case 2:
		        selection = HarmonicSelection.All;
                break;
	        default:
		        selection = HarmonicSelection.All;
                break;
	        }
	        harmSet.Selection = selection;

    	    (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.RangeFrom = b;

    	    (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.RangeTo = b;

            // Harmonic envelope selections = 0/1, 1/2, 2/3, 3/4
    	    (b, offset) = Util.GetNextByte(data, offset);
            (highNybble, lowNybble) = Util.NybblesFromByte(b);
            System.Console.WriteLine(String.Format("b = {0:X2}, low = {1:X2}, high = {2:X2}", b, lowNybble, highNybble));
            // odd and even are in the same byte
        	harmSet.Odd = new HarmonicModulation 
            { 
                IsOn = highNybble.IsBitSet(3), 
                EnvelopeNumber = (byte)((highNybble & 0b00000011) + 1) 
            };
	        harmSet.Even = new HarmonicModulation 
            {
		        IsOn = lowNybble.IsBitSet(3),
		        EnvelopeNumber = (byte)((lowNybble & 0b00000011) + 1)
	        };

    	    (b, offset) = Util.GetNextByte(data, offset);
            (highNybble, lowNybble) = Util.NybblesFromByte(b);
            System.Console.WriteLine(String.Format("b = {0:X2}, low = {1:X2}, high = {2:X2}", b, lowNybble, highNybble));
            // octave and fifth are in the same byte
        	harmSet.Octave = new HarmonicModulation 
            {
		        IsOn = highNybble.IsBitSet(3),
		        EnvelopeNumber = (byte)((highNybble & 0b00000011) + 1)
            };
        	harmSet.Fifth = new HarmonicModulation
            {
		        IsOn = lowNybble.IsBitSet(3),
		        EnvelopeNumber = (byte)((lowNybble & 0b00000011) + 1)
            };

    	    (b, offset) = Util.GetNextByte(data, offset);
            (highNybble, lowNybble) = Util.NybblesFromByte(b);
            System.Console.WriteLine(String.Format("b = {0:X2}, low = {1:X2}, high = {2:X2}", b, lowNybble, highNybble));
        	harmSet.All = new HarmonicModulation
            {
		        IsOn = highNybble.IsBitSet(3),
		        EnvelopeNumber = (byte)((highNybble & 0b00000011) + 1)
            };

            System.Console.WriteLine(String.Format("odd :{0}/{1}", harmSet.Odd.IsOn ? "Y" : "N", harmSet.Odd.EnvelopeNumber));
            System.Console.WriteLine(String.Format("even:{0}/{1}", harmSet.Even.IsOn ? "Y" : "N", harmSet.Even.EnvelopeNumber));
            System.Console.WriteLine(String.Format("oct :{0}/{1}", harmSet.Octave.IsOn ? "Y" : "N", harmSet.Octave.EnvelopeNumber));
            System.Console.WriteLine(String.Format("5th :{0}/{1}", harmSet.Fifth.IsOn ? "Y" : "N", harmSet.Fifth.EnvelopeNumber));
            System.Console.WriteLine(String.Format("all :{0}/{1}", harmSet.All.IsOn ? "Y" : "N", harmSet.All.EnvelopeNumber));

    	    (b, offset) = Util.GetNextByte(data, offset);
            switch (b)
            {
            case 0:
                harmSet.Angle = HarmonicAngle.Negative;
                break;
            case 1:
                harmSet.Angle = HarmonicAngle.Neutral;
                break;
            case 2:
                harmSet.Angle = HarmonicAngle.Positive;
                break;
            default:
                harmSet.Angle = HarmonicAngle.Neutral;  // just to keep the compiler happy
                break;
            }

    	    (b, offset) = Util.GetNextByte(data, offset);
	        harmSet.HarmonicNumber = b;

            // Harmonic envelopes (S285 ... S380) - these were created earlier.
            // There are six segments for each of the four envelopes.
            int harmonicEnvelopeDataCount = 0;
            int desiredHarmonicEnvelopeDataCount = 6 * 4 * 2;  // 4 envs, 6 segs each, level + rate for each seg
            bool shadow = false;
            for (int ei = 0; ei < HarmonicEnvelopeCount; ei++)
            {
                HarmonicEnvelopeSegment[] segments = new HarmonicEnvelopeSegment[HarmonicEnvelopeSegmentCount];
                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
            	    (b, offset) = Util.GetNextByte(data, offset);
                    //System.Console.WriteLine(String.Format("b = {0:X2}H offset = {1}", b, offset));
                    harmonicEnvelopeDataCount++;
                    if (si == 0)
                    {
                        shadow = b.IsBitSet(7);
                    }
                    segments[si].IsMaxSegment = b.IsBitSet(6);
                    segments[si].Level = (byte)(b & 0b00111111);
                }

                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
                    (b, offset) = Util.GetNextByte(data, offset);
                    //System.Console.WriteLine(String.Format("b = {0:X2}H offset = {1}", b, offset));
                    harmonicEnvelopeDataCount++;
                    segments[si].Rate = (byte)(b & 0b00111111);
                }

                harmSet.Envelopes[ei].Segments = segments;
            }
            if (harmonicEnvelopeDataCount != desiredHarmonicEnvelopeDataCount)
            {
                System.Console.WriteLine(String.Format("Should have {0} bytes of HE data, have {1}", desiredHarmonicEnvelopeDataCount, harmonicEnvelopeDataCount));
            }
            harmSet.IsShadowOn = shadow;

            /*
            for (int ei = 0; ei < HarmonicEnvelopeCount; ei++)
            {
                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
                    System.Console.WriteLine(String.Format("env{0} seg{1} rate = {2} level = {3}{4}", ei + 1, si + 1, harmSet.Envelopes[ei].Segments[si].Rate, harmSet.Envelopes[ei].Segments[si].Level, harmSet.Envelopes[ei].Segments[si].IsMaxSegment ? "*": ""));
                }
            }
             */

	        HarmonicSettings = harmSet;  // finally we get to assign this to the source

            // DDF (S381 ... S426)
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.Cutoff = (byte)(b & 0b01111111);
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.CutoffModulation = (byte)(b & 0b00011111);
            (b, offset) = Util.GetNextByte(data, offset);
            Filter.Slope = (byte)(b & 0b00011111);
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.SlopeModulation = (byte)(b & 0b00011111);
            (b, offset) = Util.GetNextByte(data, offset);
            Filter.FlatLevel = (byte)(b & 0b00011111);
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.VelocityDepth = b.ToSignedByte();
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.PressureDepth = b.ToSignedByte();
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.KeyScalingDepth = b.ToSignedByte();
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.EnvelopeDepth = b.ToSignedByte();
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.VelocityEnvelopeDepth = b.ToSignedByte();
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.IsActive = b.IsBitSet(7);
            Filter.IsModulationActive = b.IsBitSet(6);
            Filter.LFODepth = (byte)(b & 0b00011111);

            Filter.EnvelopeSegments = new FilterEnvelopeSegment[FilterEnvelopeSegmentCount];
            for (int i = 0; i < FilterEnvelopeSegmentCount; i++)
            {
        	    (b, offset) = Util.GetNextByte(data, offset);
                Filter.EnvelopeSegments[i].Rate = b;
            }
            for (int i = 0; i < FilterEnvelopeSegmentCount; i++)
            {
        	    (b, offset) = Util.GetNextByte(data, offset);
                Filter.EnvelopeSegments[i].IsMaxSegment = b.IsBitSet(6);
                Filter.EnvelopeSegments[i].Level = (byte)(b & 0x3f);
            }

            //System.Console.WriteLine(String.Format("DDF:\n{0}", Util.HexDump(Filter.ToData())));

            // DDA (S427 ... S468)
    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.AttackVelocityDepth = b.ToSignedByte();
            
    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.PressureDepth = b.ToSignedByte();

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.KeyScalingDepth = b.ToSignedByte();

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.IsActive = b.IsBitSet(7);
            Amplifier.LFODepth = (byte)(b & 0b01111111);

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.AttackVelocityRate = b.ToSignedByte();

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.ReleaseVelocityRate = b.ToSignedByte();

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.KeyScalingRate = b.ToSignedByte();

            // Amplifier envelope segments:
            // Bit 7 is always zero, bit 6 is a boolean toggle, and bits 5...0 are the value
            Amplifier.EnvelopeSegments = new AmplifierEnvelopeSegment[AmplifierEnvelopeSegmentCount];

            // First, the rates:
            for (int i = 0; i < AmplifierEnvelopeSegmentCount; i++)
            {
        	    (b, offset) = Util.GetNextByte(data, offset);
                System.Console.WriteLine(String.Format("{0:X2}", b));
                Amplifier.EnvelopeSegments[i].IsRateModulationOn = b.IsBitSet(6);
                Amplifier.EnvelopeSegments[i].Rate = (byte)(b & 0b00111111);
                System.Console.WriteLine(String.Format("{0:X2} => rate={1} mod={2}", b, Amplifier.EnvelopeSegments[i].Rate, Amplifier.EnvelopeSegments[i].IsRateModulationOn));
            }

            // Then, the levels and max settings:
            for (int i = 0; i < AmplifierEnvelopeSegmentCount - 1; i++)
            {
        	    (b, offset) = Util.GetNextByte(data, offset);
                Amplifier.EnvelopeSegments[i].IsMaxSegment = b.IsBitSet(6);
                Amplifier.EnvelopeSegments[i].Level = (byte)(b & 0b00111111);
            }

            // Actually, S467 and S468 are marked as zero in the SysEx description:
            //   S467 00000000 s1
            //   S468 00000000 s2
            // But we'll use them as the max setting and level for the 7th amp env segment.
            // Not sure if this is an error in the spec, or my misinterpretation (maybe the
            // 7th segment doesn't have a level?)
        }

        public override string ToString()
        {
            /*
            StringBuilder harmonicBuilder = new StringBuilder();
            harmonicBuilder.Append("Harmonics:\n");
            for (int i = 0; i < HarmonicCount; i++)
            {
                harmonicBuilder.Append(String.Format("{0,2}: {1,2} {2}\n", i, Harmonics[i].Level, Harmonics[i].IsModulationActive ? "Y" : "N"));
            }
            harmonicBuilder.Append("\n");
             */

            return 
                Pitch.ToString() + 
                //harmonicBuilder.ToString() + 
                HarmonicSettings.ToString() + 
                Filter.ToString() +
                Amplifier.ToString();
        }

        public byte[] ToData()
        {
            var buf = new List<byte>();

            buf.AddRange(Pitch.ToData());
            //System.Console.WriteLine(String.Format("S{0} DFG:\n{1}", SourceNumber, Util.HexDump(Pitch.ToData())));

            for (int i = 0; i < HarmonicCount; i++)
            {
                buf.Add(Harmonics[i].Level);
            }

            List<byte> harmonicsBytes = new List<byte>();
            byte b = 0;
            byte lowNybble = 0, highNybble = 0;
            int count = 0;
            // Harmonics 1...62 (0...61)
            while (count < HarmonicCount - 2)
            {
                lowNybble = (byte)(Harmonics[count].EnvelopeNumber - 1);
                lowNybble = lowNybble.UnsetBit(2);
                if (Harmonics[count].IsModulationActive)
                {
                    lowNybble = lowNybble.SetBit(3);
                }
                System.Console.WriteLine("H{0} = {1:X2}", count + 1, lowNybble);

                count++;

                highNybble = (byte)(Harmonics[count].EnvelopeNumber - 1);
                highNybble = highNybble.UnsetBit(2);
                if (Harmonics[count].IsModulationActive)
                {
                    highNybble = highNybble.SetBit(3);
                }
                System.Console.WriteLine("H{0} = {1:X2}", count + 1, highNybble);

                count++;

                b = Util.ByteFromNybbles(highNybble, lowNybble);
                System.Console.WriteLine(String.Format("ToData: H{0} {1:X2} {2:X2} => {3:X2}", count + 1, highNybble, lowNybble, b));
                //System.Console.WriteLine(String.Format("H{0} IsMod = {1} Env = {2}", i, Harmonics[i].IsModulationActive, Harmonics[i].EnvelopeNumber));
                buf.Add(b);

                harmonicsBytes.Add(b);
            }

            // harmonic 63 (count = 62) -- this is the problem
            b = (byte)(Harmonics[count].EnvelopeNumber - 1);
            byte originalByte = b;
            b = b.UnsetBit(3);
            if (Harmonics[count].IsModulationActive)
            {
                b = b.SetBit(3);
            }
            System.Console.WriteLine("H{0} = {1:X2}", count + 1, b);
            System.Console.WriteLine(String.Format("ToData: H{0}: original byte = {1:X2}, final byte = {2:X2} (mod = {3})", count + 1, originalByte, b, Harmonics[count].IsModulationActive));
            //System.Console.WriteLine(String.Format("H{0} IsMod = {1} Env = {2}", i, Harmonics[i].IsModulationActive, Harmonics[i].EnvelopeNumber));

            byte extraByte = (byte)(Harmonic63bis.EnvelopeNumber - 1);
            if (Harmonic63bis.IsModulationActive)
            {
                extraByte = extraByte.SetBit(3);
            }
            byte finalByte = Util.ByteFromNybbles(b, extraByte);
            buf.Add(finalByte);
            //buf.Add(b);
            harmonicsBytes.Add(b);

            System.Console.WriteLine(String.Format("DHG levels data:\n{0}", Util.HexDump(harmonicsBytes.ToArray())));

            System.Console.WriteLine(String.Format("Harmonic settings data:\n{0}", Util.HexDump(HarmonicSettings.ToData())));

            buf.AddRange(HarmonicSettings.ToData());
            buf.AddRange(Filter.ToData());
            buf.AddRange(Amplifier.ToData());

            return buf.ToArray();
        }
    }
}