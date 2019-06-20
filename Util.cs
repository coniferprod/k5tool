using System;
using System.Collections.Generic;
using System.Text;

namespace k5tool
{
    public class Util
    {
        // Adapted from https://stackoverflow.com/a/39439915
        // Probably should be augmented to include the delimiter.
        public static List<byte[]> SplitBytesByDelimiter(byte[] data, byte delimiter)
        {
            List<byte[]> retList = new List<byte[]>();
            if (data == null || data.Length < 1)
            {
                return retList; // rather return empty than null, or than throw an exception
            }

            int start = 0;
            int pos = 0;
            byte[] remainder = null; // in case data found at end without terminating delimiter

            while (true)
            {
                // Console.WriteLine("pos " + pos + " start " + start);
                if (pos >= data.Length) break;

                if (data[pos] == delimiter)
                {
                    // Console.WriteLine("delimiter found at pos " + pos + " start " + start);

                    // separator found
                    if (pos == start)
                    {
                        // Console.WriteLine("first char is delimiter, skipping");
                        // skip if first character is delimiter
                        pos++;
                        start++;
                        if (pos >= data.Length)
                        {
                            // last character is a delimiter, yay!
                            remainder = null;
                            break;
                        }
                        else
                        {
                            // remainder exists
                            remainder = new byte[data.Length - start];
                            Buffer.BlockCopy(data, start, remainder, 0, (data.Length - start));
                            continue;
                        }
                    }
                    else
                    {
                        // Console.WriteLine("creating new byte[] at pos " + pos + " start " + start);
                        byte[] ba = new byte[(pos - start)];
                        Buffer.BlockCopy(data, start, ba, 0, (pos - start));
                        retList.Add(ba);

                        start = pos + 1;
                        pos = start;

                        if (pos >= data.Length)
                        {
                            // last character is a delimiter, yay!
                            remainder = null;
                            break;
                        }
                        else
                        {
                            // remainder exists
                            remainder = new byte[data.Length - start];
                            Buffer.BlockCopy(data, start, remainder, 0, (data.Length - start));
                        }
                    }
                }
                else
                {
                    // payload character, continue;
                    pos++;
                }
            }

            if (remainder != null)
            {
                // Console.WriteLine("adding remainder");
                retList.Add(remainder);
            }

            //return retList.ToArray();
            return retList;
        }

                public static byte ByteFromNybbles(byte n1, byte n2)
        {
            return (byte)((n1 << 4) | n2);
        }

        public static (byte, byte) NybblesFromByte(byte b)
        {
            byte n1 = (byte) (b & 0x0F);
            byte n2 = (byte)((b & 0xF0) >> 4);
            return (n1, n2);
        }

        public static byte[] ConvertFromTwoNybbleFormat(byte[] data)
        {
            int count = data.Length / 2;  // NOTE: length must be even!
            byte[] result = new byte[count];
            int index = 0;
            int offset = 0;
            while (index < count)
            {
                result[index] = ByteFromNybbles(data[offset], data[offset + 1]);
                index++;
                offset += 2;
            }
        	return result;
        }

                // From https://www.codeproject.com/Articles/36747/Quick-and-Dirty-HexDump-of-a-Byte-Array
        public static string HexDump(byte[] bytes, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            int bytesLength = bytes.Length;

            char[] HexChars = "0123456789ABCDEF".ToCharArray();

            int firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            int firstCharColumn = firstHexColumn
                + bytesPerLine * 3       // - 2 digit for the hexadecimal value and 1 space
                + (bytesPerLine - 1) / 8 // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            int lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            char[] line = (new String(' ', lineLength - Environment.NewLine.Length) + Environment.NewLine).ToCharArray();
            int expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            StringBuilder result = new StringBuilder(expectedLines * lineLength);

            for (int i = 0; i < bytesLength; i += bytesPerLine)
            {
                line[0] = HexChars[(i >> 28) & 0xF];
                line[1] = HexChars[(i >> 24) & 0xF];
                line[2] = HexChars[(i >> 20) & 0xF];
                line[3] = HexChars[(i >> 16) & 0xF];
                line[4] = HexChars[(i >> 12) & 0xF];
                line[5] = HexChars[(i >> 8) & 0xF];
                line[6] = HexChars[(i >> 4) & 0xF];
                line[7] = HexChars[(i >> 0) & 0xF];

                int hexColumn = firstHexColumn;
                int charColumn = firstCharColumn;

                for (int j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        byte b = bytes[i + j];
                        line[hexColumn] = HexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = HexChars[b & 0xF];
                        line[charColumn] = (b < 32 ? '·' : (char)b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }

        public static (byte, int) GetNextByte(byte[] data, int offset)
        {
            return (data[offset], offset + 1);
        }

    }

    // Byte extensions from https://derekwill.com/2015/03/05/bit-processing-in-c/
    public static class ByteExtensions
    {
        public static bool IsBitSet(this byte b, int pos)
        {
            if (pos < 0 || pos > 7)
                throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");
 
            return (b & (1 << pos)) != 0;
        }
 
        public static byte SetBit(this byte b, int pos)
        {
            if (pos < 0 || pos > 7)
                throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");
 
            return (byte)(b | (1 << pos));
        }
 
        public static byte UnsetBit(this byte b, int pos)
        {
            if (pos < 0 || pos > 7)
                throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");
 
            return (byte)(b & ~(1 << pos));
        }
 
        public static byte ToggleBit(this byte b, int pos)
        {
            if (pos < 0 || pos > 7)
                throw new ArgumentOutOfRangeException("pos", "Index must be in the range of 0-7.");
 
            return (byte)(b ^ (1 << pos));
        }
 
        public static string ToBinaryString(this byte b)
        {
            return Convert.ToString(b, 2).PadLeft(8, '0');
        }
    }
}
