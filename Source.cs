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

    	public byte Angle; // 0/-, 1/0, 1/+ (maybe should be 2/+ ?)
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
    }

    public struct AmplifierEnvelopeSegment
    {
        public bool IsRateModulationOn;
        public byte Rate; // 0~31
        public bool IsMaxSegment;  // only one segment can be max
        public byte Level; // 0~31
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
    }

    public struct Amplifier 
    { 
        public bool IsActive;
	    public sbyte AttackVelocityDepth;  // 0~±31
	    public sbyte PressureDepth;  // 0~±31
	    public sbyte KeyScalingDepth;  // 0~±31
	    public byte LFODepth; // 0~31
	    public sbyte AttackVelocityRate;  // 0~±15
	    public sbyte ReleaseVelocityRate;  // 0~±15
	    public sbyte KeyScalingRate;  // 0~±15
        public AmplifierEnvelopeSegment[] EnvelopeSegments;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("*DDA*=");
            builder.Append(IsActive ? "ON" : "--");
            builder.Append("\n\n");
            builder.Append("            <DEPTH>       <RATE>\n");
            builder.Append(String.Format("         AT VEL={0,3}     AT VEL={1,3}\n", AttackVelocityDepth, AttackVelocityRate));
            builder.Append(String.Format("            PRS={0,3}     RL VEL={1,3}\n", PressureDepth, ReleaseVelocityRate));
            builder.Append(String.Format("             KS={0,3}         KS={1,3}\n", KeyScalingDepth, KeyScalingRate));
            builder.Append(String.Format("            LFO={0,3}\n", LFODepth));
            builder.Append("\n\n");

            builder.Append("*DDA ENV*\n\n    SEG  |");
            for (int i = 0; i < Source.AmplifierEnvelopeSegmentCount; i++)
            {
                builder.Append(String.Format("{0,3}|", i + 1));
            }
            builder.Append("\n    ----------------------------------\n");
            builder.Append("    RATE |");
            for (int i = 0; i < Source.AmplifierEnvelopeSegmentCount; i++)
            {
                builder.Append(String.Format("{0,3}|", EnvelopeSegments[i].Rate));
            }
            builder.Append("\n    LEVEL|");
            for (int i = 0; i < Source.AmplifierEnvelopeSegmentCount; i++)
            {
                string levelString = EnvelopeSegments[i].IsMaxSegment ? "  *" : String.Format("{0,3}", EnvelopeSegments[i].Level);
                builder.Append(String.Format("{0}|", levelString));
            }
            builder.Append("\n    RTMOD|");
            for (int i = 0; i < Source.AmplifierEnvelopeSegmentCount; i++)
            {
                string rateModulationString = EnvelopeSegments[i].IsRateModulationOn ? " ON" : " --";
                builder.Append(String.Format("{0}|", rateModulationString));
            }

            builder.Append("\n    MAX SEG = ?\n\n");

            return builder.ToString();
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
        public Harmonic[] Harmonics;
        public HarmonicSettings HarmonicSettings;
        public Filter Filter;
        public Amplifier Amplifier;

        public int SourceNumber;

        public Source(byte[] data, int number)
        {
            SourceNumber = number;

            int offset = 0;
            byte b = 0;  // reused when getting the next byte
            List<byte> buf = new List<byte>();

            // DFG
            (b, offset) = Util.GetNextByte(data, offset);
            Coarse = b.ToSignedByte();
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            Fine = b.ToSignedByte();
            buf.Add(b);

	        (b, offset) = Util.GetNextByte(data, offset);
	        if (b.IsBitSet(7))
            {
                KeyTracking = KeyTracking.Fixed;
                Key = (byte)(b & 0x7f);
            }
            else 
            {
                KeyTracking = KeyTracking.Track;
                Key = 0;  // ignored in this case
            }
            // TODO: Check that the SysEx spec gets the meaning of b7 right
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            EnvelopeDepth = b.ToSignedByte();
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            PressureDepth = b.ToSignedByte();
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            BenderDepth = b;
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            VelocityEnvelopeDepth = b.ToSignedByte();
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            LFODepth = b;
            buf.Add(b);

            (b, offset) = Util.GetNextByte(data, offset);
            PressureLFODepth = b.ToSignedByte();
            buf.Add(b);

            PitchEnvelope.Segments = new PitchEnvelopeSegment[PitchEnvelopeSegmentCount];
            for (int i = 0; i < PitchEnvelopeSegmentCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                buf.Add(b);
                if (i == 0)
                {
                    PitchEnvelope.IsLooping = b.IsBitSet(7);
                    PitchEnvelope.Segments[i].Rate = (byte)(b & 0x7f);
                }
                else
                {
                    PitchEnvelope.Segments[i].Rate = b;
                }

                (b, offset) = Util.GetNextByte(data, offset);
                buf.Add(b);
                PitchEnvelope.Segments[i].Level = b.ToSignedByte();
            }

            //System.Console.WriteLine(String.Format("Parsed DFG bytes:\n{0}", Util.HexDump(buf.ToArray())));
            
            // DHG

            //System.Console.WriteLine("Harmonic levels:");
            Harmonics = new Harmonic[HarmonicCount];
            for (int i = 0; i < HarmonicCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                Harmonics[i].Level = b;
                //System.Console.Write(String.Format("{0,2}:{1,2}({1:X2}H) ", i + 1, b));
            }
            //System.Console.WriteLine();
            //System.Console.WriteLine(String.Format("After harmonic levels, offset = {0}", offset));

            //System.Console.WriteLine("Harmonic modulation flags and envelope selections:");
            // The values are packed into 31 + 1 bytes. The first 31 bytes contain the settings
            // for harmonics 1 to 62. The solitary byte that follows has the harm 63 settings.
            byte[] harmData = new byte[HarmonicCount];
            int count = 0;
            while (count < HarmonicCount - 1)
            {
		        (b, offset) = Util.GetNextByte(data, offset);
                //System.Console.Write(String.Format("{0,2}:{1,2}({1:X2}H) ", count + 1, b));
		        harmData[count] = (byte)(b & 0x0f); // 0b00001111
                count++;
		        harmData[count] = (byte)((b & 0xf0) >> 4);
                count++;
            }

	        (b, offset) = Util.GetNextByte(data, offset);
	        harmData[count] = (byte)(b & 0x0f); // 0b00001111
            //System.Console.Write(String.Format("{0,2}:{1,2}({1:X2}H) ", count + 1, b));
            //System.Console.WriteLine();

	        // Now harmData should have all the 63 harmonics
	        for (int i = 0; i < Harmonics.Length; i++) 
            {
		        Harmonics[i].IsModulationActive = harmData[i].IsBitSet(3);
		        Harmonics[i].EnvelopeNumber = (byte)(harmData[i] & 0x03);
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
            // odd and even are in the same byte
        	harmSet.Odd = new HarmonicModulation 
            { 
                IsOn = b.IsBitSet(7), 
                EnvelopeNumber = (byte)(((b & 0x30) >> 4) + 1) 
            };
	        harmSet.Even = new HarmonicModulation 
            {
		        IsOn = b.IsBitSet(3),
		        EnvelopeNumber = (byte)((b & 0x03) + 1)
	        };

    	    (b, offset) = Util.GetNextByte(data, offset);
            // octave and fifth are in the same byte
        	harmSet.Octave = new HarmonicModulation 
            {
		        IsOn = b.IsBitSet(7),
		        EnvelopeNumber = (byte)(((b & 0x30) >> 4) + 1)
            };
        	harmSet.Fifth = new HarmonicModulation
            {
		        IsOn = b.IsBitSet(3),
		        EnvelopeNumber = (byte)((b & 0x03) + 1)
            };

    	    (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.All = new HarmonicModulation
            {
		        IsOn = b.IsBitSet(7),
		        EnvelopeNumber = (byte)(((b & 0x30) >> 4) + 1)
            };

    	    (b, offset) = Util.GetNextByte(data, offset);
        	harmSet.Angle = b;

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
                    segments[si].Level = (byte)(b & 0x3f);
                }

                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
                    (b, offset) = Util.GetNextByte(data, offset);
                    //System.Console.WriteLine(String.Format("b = {0:X2}H offset = {1}", b, offset));
                    harmonicEnvelopeDataCount++;
                    segments[si].Rate = (byte)(b & 0x3f);
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
            Filter.Cutoff = (byte)(b & 0x7f);  // just to be sure
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.CutoffModulation = (byte)(b & 0x1f);
            (b, offset) = Util.GetNextByte(data, offset);
            Filter.Slope = (byte)(b & 0x1f);
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.SlopeModulation = (byte)(b & 0x1f);
            (b, offset) = Util.GetNextByte(data, offset);
            Filter.FlatLevel = (byte)(b & 0x1f);
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
            Filter.LFODepth = (byte)(b & 0x1f);

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

            // DDA (S427 ... S468)
    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.AttackVelocityDepth = b.ToSignedByte();
            
    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.PressureDepth = b.ToSignedByte();

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.KeyScalingDepth = b.ToSignedByte();

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.IsActive = b.IsBitSet(7);
            Amplifier.LFODepth = (byte)(b & 0x7f);

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
                Amplifier.EnvelopeSegments[i].IsRateModulationOn = b.IsBitSet(6);
                Amplifier.EnvelopeSegments[i].Rate = (byte)(b & 0x3f);
            }

            // Then, the levels and max settings:
            for (int i = 0; i < AmplifierEnvelopeSegmentCount; i++)
            {
        	    (b, offset) = Util.GetNextByte(data, offset);
                Amplifier.EnvelopeSegments[i].IsMaxSegment = b.IsBitSet(6);
                Amplifier.EnvelopeSegments[i].Level = (byte)(b & 0x3f);
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
                String.Format("*DFG*              \n\n" +
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
                PitchEnvelope.ToString() +
                //harmonicBuilder.ToString() + 
                HarmonicSettings.ToString() + 
                Filter.ToString() +
                Amplifier.ToString();
        }

        public byte[] ToData()
        {
            var buf = new List<byte>();

            // DFG
            buf.Add(Coarse.ToByte());
            buf.Add(Fine.ToByte());

            byte b = Key;  // the tracking key if fixed, 0 if track
            if (KeyTracking == KeyTracking.Fixed)
            {
                b.SetBit(7);
            }
            else
            {
                b.UnsetBit(7);
            }
            buf.Add(b);  // this looks a bit fishy

            buf.Add(EnvelopeDepth.ToByte());
            buf.Add(PressureDepth.ToByte());
            buf.Add(BenderDepth);
            buf.Add(VelocityEnvelopeDepth.ToByte());
            buf.Add(LFODepth);
            buf.Add(PressureLFODepth.ToByte());
            // so far, so good

            for (int i = 0; i < PitchEnvelopeSegmentCount; i++)
            {
                b = PitchEnvelope.Segments[i].Rate;
                if (PitchEnvelope.IsLooping)
                {
                    b.SetBit(7);
                }
                buf.Add(b);
            }

            for (int i = 0; i < PitchEnvelopeSegmentCount; i++)
            {
                sbyte sb = PitchEnvelope.Segments[i].Level;
                buf.Add(sb.ToByte());
            }

            //System.Console.WriteLine(String.Format("Emitted DFG bytes:\n{0}", Util.HexDump(buf.ToArray())));
            
            for (int i = 0; i < HarmonicCount; i++)
            {
                buf.Add(Harmonics[i].Level);
            }

            byte lowNybble = 0, highNybble = 0;
            int count = 0;
            // Harmonics 1...62
            while (count < HarmonicCount - 1)
            {
                lowNybble = Harmonics[count].EnvelopeNumber;
                lowNybble.UnsetBit(2);
                if (Harmonics[count].IsModulationActive)
                {
                    lowNybble.SetBit(3);
                }
                count++;

                highNybble = Harmonics[count].EnvelopeNumber;
                highNybble.UnsetBit(2);
                if (Harmonics[count].IsModulationActive)
                {
                    highNybble.SetBit(3);
                }
                count++;

                buf.Add(Util.ByteFromNybbles(highNybble, lowNybble));
            }

            // harmonic 63
            b = Harmonics[count].EnvelopeNumber;
            b.UnsetBit(2);
            if (Harmonics[count].IsModulationActive)
            {
                b.SetBit(3);
            }

            buf.Add(HarmonicSettings.VelocityDepth.ToByte());
            buf.Add(HarmonicSettings.PressureDepth.ToByte());
            buf.Add(HarmonicSettings.KeyScalingDepth.ToByte());
            buf.Add(HarmonicSettings.LFODepth);

            for (int i = 0; i < HarmonicEnvelopeCount; i++)
            {
                b = HarmonicSettings.Envelopes[i].Effect;
                if (HarmonicSettings.Envelopes[i].IsActive)
                {
                    b.SetBit(7);
                }
                buf.Add(b);
            }

            b = (byte)HarmonicSettings.Selection;
            if (HarmonicSettings.IsModulationActive)
            {
                b.SetBit(7);
            }
            buf.Add(b);

            buf.Add(HarmonicSettings.RangeFrom);
            buf.Add(HarmonicSettings.RangeTo);

            lowNybble = HarmonicSettings.Even.EnvelopeNumber;
            if (HarmonicSettings.Even.IsOn)
            {
                lowNybble.SetBit(3);
            }
            highNybble = HarmonicSettings.Odd.EnvelopeNumber;
            if (HarmonicSettings.Odd.IsOn)
            {
                highNybble.SetBit(3);
            }
            buf.Add(Util.ByteFromNybbles(highNybble, lowNybble));

            lowNybble = HarmonicSettings.Fifth.EnvelopeNumber;
            if (HarmonicSettings.Fifth.IsOn)
            {
                lowNybble.SetBit(3);
            }
            highNybble = HarmonicSettings.Octave.EnvelopeNumber;
            if (HarmonicSettings.Octave.IsOn)
            {
                highNybble.SetBit(3);
            }
            buf.Add(Util.ByteFromNybbles(highNybble, lowNybble));

            lowNybble = 0;
            highNybble = HarmonicSettings.All.EnvelopeNumber;
            if (HarmonicSettings.All.IsOn)
            {
                highNybble.SetBit(3);
            }
            buf.Add(Util.ByteFromNybbles(highNybble, lowNybble));

            buf.Add(HarmonicSettings.Angle);
            buf.Add(HarmonicSettings.HarmonicNumber);

            for (int ei = 0; ei < HarmonicEnvelopeCount; ei++)
            {
                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
                    b = HarmonicSettings.Envelopes[ei].Segments[si].Level;
                    if (HarmonicSettings.Envelopes[ei].Segments[si].IsMaxSegment)
                    {
                        b.SetBit(6);
                    }
                    else
                    {
                        b.UnsetBit(6);
                    }
                    if (ei == 0)
                    {
                        if (HarmonicSettings.IsShadowOn)
                        {
                            b.SetBit(7);
                        }
                        else
                        {
                            b.UnsetBit(7);
                        }
                    }
                    buf.Add(b);
                }
                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
                    b = HarmonicSettings.Envelopes[ei].Segments[si].Rate;
                    buf.Add(b);
                }
            }

            // DDF
            buf.Add(Filter.Cutoff);
            buf.Add(Filter.CutoffModulation);
            buf.Add(Filter.Slope);
            buf.Add(Filter.SlopeModulation);
            buf.Add(Filter.FlatLevel);
            buf.Add(Filter.VelocityDepth.ToByte());
            buf.Add(Filter.PressureDepth.ToByte());
            buf.Add(Filter.KeyScalingDepth.ToByte());
            buf.Add(Filter.EnvelopeDepth.ToByte());
            buf.Add(Filter.VelocityEnvelopeDepth.ToByte());

            b = Filter.LFODepth;
            if (Filter.IsModulationActive)
            {
                b.SetBit(6);
            }
            else
            {
                b.UnsetBit(6);
            }
            if (Filter.IsActive)
            {
                b.SetBit(7);
            }
            else
            {
                b.UnsetBit(7);
            }
            buf.Add(b);

            for (int i = 0; i < FilterEnvelopeSegmentCount; i++)
            {
                buf.Add(Filter.EnvelopeSegments[i].Rate);
            }

            for (int i = 0; i < FilterEnvelopeSegmentCount; i++)
            {
                b = Filter.EnvelopeSegments[i].Level;
                if (Filter.EnvelopeSegments[i].IsMaxSegment)
                {
                    b.SetBit(6);
                }
                else
                {
                    b.UnsetBit(6);
                }
                buf.Add(b);
            }

            // DDA

            buf.Add(Amplifier.AttackVelocityDepth.ToByte());
            buf.Add(Amplifier.PressureDepth.ToByte());
            buf.Add(Amplifier.KeyScalingDepth.ToByte());
            b = Amplifier.LFODepth;
            if (Amplifier.IsActive)
            {
                b.SetBit(7);
            }
            else
            {
                b.UnsetBit(7);
            }
            buf.Add(b);
            buf.Add(Amplifier.AttackVelocityRate.ToByte());
            buf.Add(Amplifier.ReleaseVelocityRate.ToByte());
            buf.Add(Amplifier.KeyScalingRate.ToByte());
            
            for (int i = 0; i < AmplifierEnvelopeSegmentCount; i++)
            {
                b = Amplifier.EnvelopeSegments[i].Rate;
                if (Amplifier.EnvelopeSegments[i].IsRateModulationOn)
                {
                    b.SetBit(6);
                }
                else
                {
                    b.UnsetBit(6);
                }
                buf.Add(b);
            }

            for (int i = 0; i < AmplifierEnvelopeSegmentCount; i++)
            {
                b = Amplifier.EnvelopeSegments[i].Level;
                if (Amplifier.EnvelopeSegments[i].IsMaxSegment)
                {
                    b.SetBit(6);
                }
                else
                {
                    b.UnsetBit(6);
                }
                buf.Add(b);
            }
            
            System.Console.WriteLine(String.Format("S{0} bytes:\n{1}", SourceNumber, Util.HexDump(buf.ToArray())));

            return buf.ToArray();
        }
    }
}