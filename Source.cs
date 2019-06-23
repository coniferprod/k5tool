using System;
using System.Text;

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
            builder.Append(IsShadowOn ? "ON" : "OFF");
            builder.Append("\n\n");

            return builder.ToString();
        }
    }

    public enum LFOShape  // 1 ~ 6
    {
        Triangle,
        InverseTriangle,
        Square,
        InverseSquare,
        Sawtooth,
        InverseSawtooth
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

    public struct LFO
    {
        public LFOShape Shape;
        public byte Speed;  // 0~99
        public byte Delay;  // 0~31
        public byte Trend;  // 0~31

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("*LFO*\n\n");
            builder.Append(String.Format(" SHAPE= {0}\n SPEED= {1,2}\n DELAY= {2,2}\n TREND= {3,2}\n\n\n", Shape, Speed, Delay, Trend));

            return builder.ToString();
        }
    }
    
    public struct KeyScaling
    {
        public sbyte Right;
        public sbyte Left;
        public byte Breakpoint;

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("*KS CURVE*\n\n");
            builder.Append(String.Format("LEFT={0,3}    B.POINT={1,3}    RIGHT={2,3}\n", Left, Right, Breakpoint));
            builder.Append("\n\n");

            return builder.ToString();
        }
    }

    public class Source
    {
        const int EnvelopeSegmentCount = 6;
        const int PitchEnvelopeSegmentCount = 6;
        const int FormantLevelCount = 11;
        const int HarmonicCount = 63;
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
        public LFO LFO;
        public KeyScaling KeyScaling;
        public bool IsFormantOn;  // true Digital Formant filter is on
        public byte[] FormantLevels;  // levels for bands C-1 ... C9 (0 ~ 63)

        public Source(byte[] data)
        {
            int offset = 0;
            byte b = 0;  // reused when getting the next byte

            // DFG
            (b, offset) = Util.GetNextByte(data, offset);
            Coarse = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            Fine = b.ToSignedByte();

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

            (b, offset) = Util.GetNextByte(data, offset);
            EnvelopeDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            PressureDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            BenderDepth = b;

            (b, offset) = Util.GetNextByte(data, offset);
            VelocityEnvelopeDepth = b.ToSignedByte();

            (b, offset) = Util.GetNextByte(data, offset);
            LFODepth = b;

            (b, offset) = Util.GetNextByte(data, offset);
            PressureLFODepth = b.ToSignedByte();

            PitchEnvelope.Segments = new PitchEnvelopeSegment[PitchEnvelopeSegmentCount];
            for (int i = 0; i < PitchEnvelopeSegmentCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
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
                PitchEnvelope.Segments[i].Level = b.ToSignedByte();
            }

            // DHG

            Harmonics = new Harmonic[HarmonicCount];
            for (int i = 0; i < HarmonicCount; i++)
            {
                (b, offset) = Util.GetNextByte(data, offset);
                Harmonics[i].Level = b;
                System.Console.WriteLine(String.Format("HL {0} = {1:X}", i, b));
            }

            byte[] harmData = new byte[HarmonicCount];
            int index = 0;
	        for (int count = 0; count < 31; count++) 
            {
		        (b, offset) = Util.GetNextByte(data, offset);
		        harmData[index] = (byte)(b & 0x0f); // 0b00001111
                index++;
                System.Console.WriteLine(String.Format("{0} {1:X}", index, harmData[index]));
		        harmData[index] = (byte)((b&0xf0) >> 4);
                index++;
                System.Console.WriteLine(String.Format("{0} {1:X}", index, harmData[index]));
		        count++;
	        }

	        // S251 has only harm63 for S1, and S252 has only harm63 for S2
	        // (the others are packed two to a byte)
	        (b, offset) = Util.GetNextByte(data, offset);
	        harmData[index] = (byte)(b & 0x0f); // 0b00001111

	        // Now harmData should have all the 63 harmonics
	        for (int i = 0; i < harmData.Length; i++) 
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
            bool shadow = false;
            for (int ei = 0; ei < HarmonicEnvelopeCount; ei++)
            {
                HarmonicEnvelopeSegment[] segments = new HarmonicEnvelopeSegment[HarmonicEnvelopeSegmentCount];
                for (int si = 0; si < HarmonicEnvelopeSegmentCount; si++)
                {
            	    (b, offset) = Util.GetNextByte(data, offset);
                    if (si == 0)
                    {
                        shadow = b.IsBitSet(7);
                    }
                    segments[si].IsMaxSegment = b.IsBitSet(6);
                    segments[si].Level = (byte)(b & 0x3f);
                }
                harmSet.Envelopes[ei].Segments = segments;
            }
            harmSet.IsShadowOn = shadow;

	        HarmonicSettings = harmSet;  // finally we get to assign this to the source

            // DDF (S381 ... S426)
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.Cutoff = b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.CutoffModulation = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Filter.Slope = b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.SlopeModulation = b;
            (b, offset) = Util.GetNextByte(data, offset);
            Filter.FlatLevel = b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.VelocityDepth = (sbyte)b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.PressureDepth = (sbyte)b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.KeyScalingDepth = (sbyte)b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.EnvelopeDepth = (sbyte)b;
    	    (b, offset) = Util.GetNextByte(data, offset);
            Filter.VelocityEnvelopeDepth = (sbyte)b;
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
            Amplifier.AttackVelocityDepth = (sbyte)b;
            
    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.PressureDepth = (sbyte)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.KeyScalingDepth = (sbyte)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.IsActive = b.IsBitSet(7);
            Amplifier.LFODepth = (byte)(b & 0x7f);

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.AttackVelocityRate = (sbyte)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.ReleaseVelocityRate = (sbyte)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            Amplifier.KeyScalingRate = (sbyte)b;

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
            // Not sure if this is an error in the spec, or my misinterpretation.

            // LFO (S469 ... S472)
    	    (b, offset) = Util.GetNextByte(data, offset);
            LFO.Shape = (LFOShape)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            LFO.Speed = b;

            (b, offset) = Util.GetNextByte(data, offset);
            LFO.Delay = b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            LFO.Trend = b;

            // Keyscaling (S473 ... S478)
    	    (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.Right = (sbyte)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.Left = (sbyte)b;

    	    (b, offset) = Util.GetNextByte(data, offset);
            KeyScaling.Breakpoint = b;

            // DFT (S479 ... S489)
            FormantLevels = new byte[FormantLevelCount];
            for (int i = 0; i < FormantLevelCount; i++)
            {
        	    (b, offset) = Util.GetNextByte(data, offset);
                if (i == 0)
                {
                    IsFormantOn = b.IsBitSet(7);
                    b.UnsetBit(7);  // in the first byte, the low seven bits have the level
                }
                FormantLevels[i] = b;
            }

            // S490 is unused (should be zero, but whatever), so eat it
        	(b, offset) = Util.GetNextByte(data, offset);

            // Checksum (S491 ... S492)
        	(b, offset) = Util.GetNextByte(data, offset);
            byte checksumLow = b;
        	(b, offset) = Util.GetNextByte(data, offset);
            byte checksumHigh = b;
        }

        public override string ToString()
        {
            StringBuilder formantStringBuilder = new StringBuilder();
            for (int i = 0; i < FormantLevelCount; i++)
            {
                formantStringBuilder.Append(String.Format("C{0} ", i - 1));
            }
            formantStringBuilder.Append("\n");
            for (int i = 0; i < FormantLevelCount; i++)
            {
                formantStringBuilder.Append(String.Format("{0,3}", FormantLevels[i]));
            }
            formantStringBuilder.Append("\n");

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
                HarmonicSettings.ToString() + 
                Filter.ToString() +
                Amplifier.ToString() + 
                String.Format("\n*DFT*={0}\n\n{1}\n\n", 
                    IsFormantOn ? "ON" : "--", formantStringBuilder.ToString()) +
                LFO.ToString() +
                KeyScaling.ToString();
        }
    }
}