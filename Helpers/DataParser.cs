using System;
using System.IO;

namespace UnitySourceEngine
{
    public static class DataParser
    {
        public static bool bigEndian = false;
        private readonly static byte[] bitMasks = new byte[] { 0xff, 0xfe, 0xfc, 0xf8, 0xf0, 0xe0, 0xc0, 0x80 };

        #region Base Reader Functions
        /*public static byte[] ReadBits(byte[] data, int bitIndex, int bitsToRead)
        {
            byte[] outputBytes = new byte[(bitsToRead / 8) + (bitsToRead % 8 > 0 ? 1 : 0)];
            byte bitOffset = (byte)(bitIndex % 8);

            int bitsRead = 0;
            for (int outputByteIndex = 0; outputByteIndex < outputBytes.Length; outputByteIndex++)
            {
                int dataByteIndex = (bitIndex / 8) + outputByteIndex;

                //Start reading bits of current byte
                outputBytes[outputByteIndex] |= (byte)((data[dataByteIndex] & bitMasks[bitOffset]) >> bitOffset);
                bitsRead += (byte)(8 - bitOffset);

                //If we did not get an entire byte, continue reading bits
                if (bitsRead < bitsToRead && bitOffset > 0)
                {
                    outputBytes[outputByteIndex] |= (byte)((data[dataByteIndex + 1] & ~bitMasks[bitOffset]) << (8 - bitOffset));
                    bitsRead += bitOffset;
                }
                //Trim off excess bits
                if (bitsRead > bitsToRead)
                    outputBytes[outputByteIndex] &= (byte)~bitMasks[bitsToRead % 8];
            }

            if (bigEndian)
                outputBytes = outputBytes.Reverse().ToArray();

            return outputBytes.Length > 0 ? outputBytes : new byte[] { 0 };
        }
        public static byte[] ReadBits(byte[] data, int bitIndex, int bitsToRead, int resultByteArraySize)
        {
            byte[] result = ReadBits(data, bitIndex, bitsToRead);
            if (result.Length < resultByteArraySize)
            {
                byte[] fixedResult = new byte[resultByteArraySize];
                for (uint i = 0; i < result.Length; i++)
                    fixedResult[i] = result[i];
                result = fixedResult;
            }

            return result;
        }
        public static byte[] ReadBits(Stream stream, int bitIndex, int bitsToRead)
        {
            byte[] outputBytes = new byte[(bitsToRead / 8) + (bitsToRead % 8 > 0 ? 1 : 0)];
            byte bitOffset = (byte)(bitIndex % 8);

            int bitsRead = 0;
            for (int outputByteIndex = 0; outputByteIndex < outputBytes.Length; outputByteIndex++)
            {
                int dataByteIndex = (bitIndex / 8) + outputByteIndex;

                stream.Position = dataByteIndex;
                outputBytes[outputByteIndex] |= (byte)((stream.ReadByte() & bitMasks[bitOffset]) >> bitOffset);
                bitsRead += (byte)(8 - bitOffset); //Start reading bits of current byte

                //If we did not get an entire byte, continue reading bits
                if (bitsRead < bitsToRead && bitOffset > 0)
                {
                    outputBytes[outputByteIndex] |= (byte)((stream.ReadByte() & ~bitMasks[bitOffset]) << (8 - bitOffset));
                    bitsRead += bitOffset;
                }
                //Trim off excess bits
                if (bitsRead > bitsToRead)
                    outputBytes[outputByteIndex] &= (byte)~bitMasks[bitsToRead % 8];
            }

            if (bigEndian)
                outputBytes = outputBytes.Reverse().ToArray();

            return outputBytes.Length > 0 ? outputBytes : new byte[] { 0 };
        }
        public static byte[] ReadBits(Stream stream, int bitIndex, int bitsToRead, int resultByteArraySize)
        {
            byte[] result = ReadBits(stream, bitIndex, bitsToRead);
            if (result.Length < resultByteArraySize)
            {
                byte[] fixedResult = new byte[resultByteArraySize];
                for (uint i = 0; i < result.Length; i++)
                    fixedResult[i] = result[i];
                result = fixedResult;
            }

            return result;
        }

        public static byte[] ReadBytes(byte[] data, int byteIndex, int bytesToRead)
        {
            byte[] bytesRead = new byte[bytesToRead];
            if (data != null && byteIndex >= 0 && bytesToRead > 0)
            {
                Array.Copy(data, byteIndex, bytesRead, 0, bytesToRead > data.Length - byteIndex ? data.Length - byteIndex : bytesToRead);
                //AlternateCopy(data, byteIndex, bytesRead, 0, bytesToRead > data.Length - byteIndex ? data.Length - byteIndex : bytesToRead);
                if (bigEndian)
                    bytesRead = bytesRead.Reverse().ToArray();
            }
            return bytesRead;
        }
        public static byte[] ReadBytes(Stream stream, int amount)
        {
            byte[] buffer = new byte[amount];
            stream.Read(buffer, 0, amount);
            if (bigEndian)
                buffer = buffer.Reverse().ToArray();
            return buffer;
        }
        public static byte[] ReadBytes(Stream stream, int byteIndex, int amount)
        {
            byte[] buffer = new byte[amount];
            stream.Position = byteIndex;
            stream.Read(buffer, 0, amount);
            if (bigEndian)
                buffer = buffer.Reverse().ToArray();
            return buffer;
        }*/
        #endregion

        #region 1 byte structures
        public static bool ReadBool(Stream stream)
        {
            byte[] boolArray = new byte[1];
            stream.Read(boolArray, 0, 1);
            return BitConverter.ToBoolean(boolArray, 0);
        }
        /*public static bool ReadBool(byte[] data, int bitIndex, byte bitsToRead = 8)
        {
            if (bitsToRead > 8) bitsToRead = 8;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToBoolean(ReadBits(data, bitIndex, bitsToRead), 0);
        }*/
        /*public static bool ReadBit(byte[] data, int bitIndex)
        {
            return ReadBits(data, bitIndex, 1)[0] != 0;
        }*/
        public static sbyte ReadSByte(Stream stream)
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            return (sbyte)buffer[0];
        }
        /*public static sbyte ReadSByte(byte[] data, int bitIndex, byte bitsToRead = 8)
        {
            if (bitsToRead > 8) bitsToRead = 8;
            if (bitsToRead < 1) bitsToRead = 1;
            return (sbyte)ReadBits(data, bitIndex, bitsToRead)[0];
        }*/
        public static byte ReadByte(Stream stream, int byteIndex)
        {
            byte[] buffer = new byte[1];
            stream.Position = byteIndex;
            stream.Read(buffer, 0, 1);
            return buffer[0];
        }

        public static byte ReadByte(Stream stream)
        {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 1);
            return buffer[0];
        }
        /*public static byte ReadByte(byte[] data, int bitIndex, byte bitsToRead = 8)
        {
            if (bitsToRead > 8) bitsToRead = 8;
            if (bitsToRead < 1) bitsToRead = 1;
            return ReadBits(data, bitIndex, bitsToRead)[0];
        }*/
        public static char ReadChar(Stream stream)
        {
            byte[] buffer = new byte[2];
            stream.Read(buffer, 0, 1);
            return BitConverter.ToChar(buffer, 0);
        }
        #endregion

        #region 2 byte structures
        public static short ReadShort(Stream stream)
        {
            byte[] shortBytes = new byte[2];
            stream.Read(shortBytes, 0, 2);
            return BitConverter.ToInt16(shortBytes, 0);
        }
        /*public static short ReadShort(byte[] data, int bitIndex, byte bitsToRead = 16)
        {
            if (bitsToRead > 16) bitsToRead = 16;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToInt16(ReadBits(data, bitIndex, bitsToRead, 2), 0);
        }*/
        public static ushort ReadUShort(Stream stream)
        {
            byte[] ushortBytes = new byte[2];
            stream.Read(ushortBytes, 0, 2);
            return BitConverter.ToUInt16(ushortBytes, 0);
        }
        /*public static ushort ReadUShort(Stream stream, int bitIndex, byte bitsToRead = 16)
        {
            if (bitsToRead > 16) bitsToRead = 16;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToUInt16(ReadBits(stream, bitIndex, bitsToRead, 2), 0);
        }
        public static ushort ReadUShort(byte[] data, int bitIndex, byte bitsToRead = 16)
        {
            if (bitsToRead > 16) bitsToRead = 16;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToUInt16(ReadBits(data, bitIndex, bitsToRead, 2), 0);
        }*/
        #endregion

        #region 4 byte structures
        public static float ReadFloat(Stream stream)
        {
            byte[] floatBytes = new byte[4];
            stream.Read(floatBytes, 0, 4);
            return BitConverter.ToSingle(floatBytes, 0);
        }
        /*public static float ReadFloat(Stream stream, int bitIndex, byte bitsToRead = 32)
        {
            if (bitsToRead > 32) bitsToRead = 32;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToSingle(ReadBits(stream, bitIndex, bitsToRead, 4), 0);
        }
        public static float ReadFloat(byte[] data, int bitIndex, byte bitsToRead = 32)
        {
            if (bitsToRead > 32) bitsToRead = 32;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToSingle(ReadBits(data, bitIndex, bitsToRead, 4), 0);
        }*/
        public static int ReadInt(Stream stream)
        {
            byte[] intBytes = new byte[4];
            stream.Read(intBytes, 0, 4);
            return BitConverter.ToInt32(intBytes, 0);
        }
        /*public static int ReadInt(Stream stream, int bitIndex, byte bitsToRead = 32)
        {
            if (bitsToRead > 32) bitsToRead = 32;
            if (bitsToRead < 1) bitsToRead = 1;

            return BitConverter.ToInt32(ReadBits(stream, bitIndex, bitsToRead, 4), 0);
        }
        public static int ReadInt(byte[] data, int bitIndex, byte bitsToRead = 32)
        {
            if (bitsToRead > 32) bitsToRead = 32;
            if (bitsToRead < 1) bitsToRead = 1;

            return BitConverter.ToInt32(ReadBits(data, bitIndex, bitsToRead, 4), 0);
        }*/
        public static uint ReadUInt(Stream stream)
        {
            byte[] uintBytes = new byte[4];
            stream.Read(uintBytes, 0, 4);
            return BitConverter.ToUInt32(uintBytes, 0);
        }
        /*public static uint ReadUInt(Stream stream, int bitIndex, byte bitsToRead = 32)
        {
            if (bitsToRead > 32) bitsToRead = 32;
            if (bitsToRead < 1) bitsToRead = 1;

            return BitConverter.ToUInt32(ReadBits(stream, bitIndex, bitsToRead, 4), 0);
        }
        public static uint ReadUInt(byte[] data, int bitIndex, byte bitsToRead = 32)
        {
            if (bitsToRead > 32) bitsToRead = 32;
            if (bitsToRead < 1) bitsToRead = 1;

            return BitConverter.ToUInt32(ReadBits(data, bitIndex, bitsToRead, 4), 0);
        }*/
        #endregion

        #region 8 byte structures
        public static double ReadDouble(Stream stream)
        {
            byte[] doubleBytes = new byte[8];
            stream.Read(doubleBytes, 0, 8);
            return BitConverter.ToDouble(doubleBytes, 0);
        }
        /*public static double ReadDouble(byte[] data, int bitIndex, byte bitsToRead = 64)
        {
            if (bitsToRead > 64) bitsToRead = 64;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToDouble(ReadBits(data, bitIndex, bitsToRead, 8), 0);
        }*/
        public static long ReadLong(Stream stream)
        {
            byte[] longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            return BitConverter.ToInt64(longBytes, 0);
        }
        /*public static long ReadLong(byte[] data, int bitIndex, byte bitsToRead = 64)
        {
            if (bitsToRead > 64) bitsToRead = 64;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToInt64(ReadBits(data, bitIndex, bitsToRead, 8), 0);
        }*/
        public static ulong ReadULong(Stream stream)
        {
            byte[] ulongBytes = new byte[8];
            stream.Read(ulongBytes, 0, 8);
            return BitConverter.ToUInt64(ulongBytes, 0);
        }
        /*public static ulong ReadULong(byte[] data, int bitIndex, byte bitsToRead = 64)
        {
            if (bitsToRead > 64) bitsToRead = 64;
            if (bitsToRead < 1) bitsToRead = 1;
            return BitConverter.ToUInt64(ReadBits(data, bitIndex, bitsToRead, 8), 0);
        }*/
        #endregion

        #region 16 byte structures
        public static decimal ReadDecimal(Stream stream)
        {
            return new decimal(new int[] { ReadInt(stream), ReadInt(stream), ReadInt(stream), ReadInt(stream) }); //Big endian probably doesn't work, each individual int is flipped but their ordering is probably wrong
        }
        /*public static decimal ReadDecimal(byte[] data, int bitIndex, byte bitsToRead = 128)
        {
            if (bitsToRead > 128) bitsToRead = 128;
            if (bitsToRead < 1) bitsToRead = 1;

            int[] decimalParts = new int[4];
            for (byte i = 0; i < UnityEngine.Mathf.CeilToInt(bitsToRead / 32f); i++)
                decimalParts[i] = ReadInt(data, bitIndex + i * 32, (byte)((bitsToRead - i * 32) < 32 ? (bitsToRead - i * 32) : 32));

            return new decimal(decimalParts);
        }*/
        #endregion

        #region Protobuf
        /*public static int ReadProtoInt(byte[] data, int index, out int bytesRead)
        {
            int protoInt = 0;
            bytesRead = 0;

            if (index < data.Length)
            {
                byte currentByte = 0;

                do
                {
                    if (index + bytesRead < data.Length) currentByte = data[index + bytesRead];
                    if (bytesRead < 4 || (bytesRead == 4 && ((currentByte & 0xf8) == 0 || (currentByte & 0xf8) == 0xf8)))
                        protoInt |= (currentByte & ~0x80) << (7 * bytesRead);
                    bytesRead++;
                }
                while (bytesRead < 10 && (currentByte & 0x80) != 0);
            }

            return protoInt;
        }
        public static int ReadProtoInt(byte[] data, int index)
        {
            int bytesRead;
            return ReadProtoInt(data, index, out bytesRead);
        }
        public static int ReadProtoInt(Stream stream, out int bytesRead)
        {
            int protoInt = 0;
            byte currentByte;
            bytesRead = 0;

            do
            {
                currentByte = ReadByte(stream);
                if (bytesRead < 4 || (bytesRead == 4 && ((currentByte & 0xf8) == 0 || (currentByte & 0xf8) == 0xf8)))
                    protoInt |= (currentByte & ~0x80) << (7 * bytesRead);
                bytesRead++;
            }
            while (bytesRead < 10 && (currentByte & 0x80) != 0);

            return protoInt;
        }
        public static int ReadProtoInt(Stream stream)
        {
            int bytesRead;
            return ReadProtoInt(stream, out bytesRead);
        }
        public static string ReadProtoString(byte[] data, int index, out int bytesRead)
        {
            int sizeOfProtoInt;
            int stringSize = ReadProtoInt(data, index, out sizeOfProtoInt);
            bytesRead = sizeOfProtoInt + stringSize;
            return System.Text.Encoding.UTF8.GetString(ReadBytes(data, index + sizeOfProtoInt, stringSize));
        }
        public static string ReadProtoString(byte[] data, int index)
        {
            int bytesRead;
            return ReadProtoString(data, index, out bytesRead);
        }
        public static string ReadProtoString(Stream stream, out int bytesRead)
        {
            int protoIntSize;
            int stringSize = ReadProtoInt(stream, out protoIntSize);
            bytesRead = protoIntSize + stringSize;
            return System.Text.Encoding.UTF8.GetString(ReadBytes(stream, stringSize));
        }
        public static string ReadProtoString(Stream stream)
        {
            int bytesRead;
            return ReadProtoString(stream, out bytesRead);
        }*/
        #endregion

        #region Strings
        public static string ReadNullTerminatedString(Stream stream)
        {
            int bytesRead;

            return ReadNullTerminatedString(stream, out bytesRead);
        }
        public static string ReadNewlineTerminatedString(Stream stream)
        {
            int bytesRead;

            return ReadNewlineTerminatedString(stream, out bytesRead);
        }
        public static string ReadNewlineTerminatedString(Stream stream, out int bytesRead)
        {
            bytesRead = 0;
            string builtString = "";
            char nextChar = '\0';
            do
            {
                if (stream.CanRead)
                    nextChar = ReadChar(stream);

                if (nextChar != '\0' && nextChar != '\n')
                    builtString += nextChar;

                bytesRead += 1;
            }
            while (nextChar != '\0' && nextChar != '\n' && stream.CanRead);

            return builtString;
        }
        public static string ReadNullTerminatedString(Stream stream, out int bytesRead)
        {
            bytesRead = 0;
            string builtString = "";
            char nextChar = '\0';
            do
            {
                if (stream.CanRead)
                    nextChar = ReadChar(stream);

                if (nextChar != '\0')
                    builtString += nextChar;

                bytesRead += 1;
            }
            while (nextChar != '\0' && stream.CanRead);

            return builtString;
        }
        /*public static string ReadNullTerminatedString(byte[] data, int byteIndex, out int bytesRead)
        {
            bytesRead = 0;
            string builtString = "";
            char nextChar = '\0';
            do
            {
                nextChar = BitConverter.ToChar(data, byteIndex + bytesRead);
                if (nextChar != '\0') builtString += nextChar;
                bytesRead++;
            }
            while (nextChar != '\0' && byteIndex + bytesRead < data.Length);

            return builtString;
        }
        public static string ReadNullTerminatedString(byte[] data, int byteIndex)
        {
            int bytesRead;
            return ReadNullTerminatedString(data, byteIndex, out bytesRead);
        }
        public static string ReadCString(byte[] data, int byteIndex, int bytesToRead, System.Text.Encoding encoding)
        {
            return encoding.GetString(ReadBytes(data, byteIndex, bytesToRead)).Split(new char[] { '\0' }, 2)[0];
        }
        public static string ReadCString(byte[] data, int byteIndex, int bytesToRead)
        {
            return ReadCString(data, byteIndex, bytesToRead, System.Text.Encoding.UTF8);
        }
        public static string ReadLimitedString(byte[] data, int bitIndex, out int bitsRead, int byteLimit)
        {
            bitsRead = 0;
            List<byte> output = new List<byte>();
            for(uint i = 0; i < byteLimit; i++)
            {
                byte input = ReadByte(data, bitIndex + bitsRead);
                bitsRead += 8;
                if (input == 0 || input == 10)
                    break;
                output.Add(input);
            }
            return System.Text.Encoding.ASCII.GetString(output.ToArray());
        }*/
        #endregion

        #region Other
        public static byte[] GetFileHash(string filePath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
        public static void CopyTo(this Stream from, Stream to, long amount, int bufferSize = 81920)
        {
            long totalCopied = 0;
            byte[] buffer = new byte[bufferSize];
            int actualAmountRead;
            do
            {
                int readLength = (int)Math.Min(amount - totalCopied, bufferSize);
                actualAmountRead = from.Read(buffer, 0, readLength);
                if (actualAmountRead > 0)
                    to.Write(buffer, 0, actualAmountRead);
                totalCopied += actualAmountRead;
            }
            while (actualAmountRead > 0);
        }
        /*public static string ReadDataTableString(byte[] data, int byteIndex, out int bytesRead)
        {
            bytesRead = 0;
            List<byte> builtString = new List<byte>();
            byte nextChar = 0;
            do
            {
                nextChar = data[byteIndex + bytesRead];
                if (nextChar != 0) builtString.Add(nextChar);
                bytesRead++;
            }
            while (nextChar != 0 && byteIndex + bytesRead < data.Length);

            return System.Text.Encoding.Default.GetString(builtString.ToArray());
        }
        public static uint ReadUBitInt(byte[] data, int bitIndex, out int bitsRead)
        {
            uint uBitInt = ReadUInt(data, bitIndex, 6);
            bitsRead = 6;
            if ((uBitInt & (16 | 32)) == 16)
            {
                uBitInt = (uBitInt & 15) | (ReadUInt(data, bitIndex + bitsRead, 4) << 4);
                bitsRead += 4;
            }
            else if ((uBitInt & (16 | 32)) == 32)
            {
                uBitInt = (uBitInt & 15) | (ReadUInt(data, bitIndex + bitsRead, 8) << 4);
                bitsRead += 8;
            }
            else if ((uBitInt & (16 | 32)) == 48)
            {
                uBitInt = (uBitInt & 15) | (ReadUInt(data, bitIndex + bitsRead, 32 - 4) << 4);
                bitsRead += 28;
            }
            return uBitInt;
        }
        public static uint ReadVarInt32(byte[] data, int bitIndex, out int bitsRead)
        {
            bitsRead = 0;
            uint tmpByte = 0x80;
            uint result = 0;
            for (int count = 0; (tmpByte & 0x80) != 0; count++)
            {
                if (count > 5)
                    throw new Exception("VarInt32 out of range");
                tmpByte = ReadByte(data, bitIndex);
                bitIndex += 8; bitsRead += 8;
                result |= (tmpByte & 0x7f) << (7 * count);
            }
            return result;
        }*/
        #endregion
    }
}