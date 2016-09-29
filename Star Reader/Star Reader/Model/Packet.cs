using System;
using System.Collections.Generic;
using System.Globalization;

namespace Star_Reader.Model
{
    public class Packet
    {
        //Found code at "http://sanity-free.org/146/crc8_implementation_in_csharp.html" 22/09/16
        public static byte[] Table =
        {
            0x00, 0x91, 0xe3, 0x72, 0x07, 0x96, 0xe4, 0x75, 0x0e, 0x9f, 0xed, 0x7c, 0x09, 0x98,
            0xea, 0x7b, 0x1c, 0x8d, 0xff, 0x6e, 0x1b, 0x8a, 0xf8, 0x69, 0x12, 0x83, 0xf1, 0x60, 0x15, 0x84, 0xf6, 0x67,
            0x38, 0xa9, 0xdb, 0x4a, 0x3f, 0xae, 0xdc, 0x4d, 0x36, 0xa7, 0xd5, 0x44, 0x31, 0xa0, 0xd2, 0x43, 0x24, 0xb5,
            0xc7, 0x56, 0x23, 0xb2, 0xc0, 0x51, 0x2a, 0xbb, 0xc9, 0x58, 0x2d, 0xbc, 0xce, 0x5f, 0x70, 0xe1, 0x93, 0x02,
            0x77, 0xe6, 0x94, 0x05, 0x7e, 0xef, 0x9d, 0x0c, 0x79, 0xe8, 0x9a, 0x0b, 0x6c, 0xfd, 0x8f, 0x1e, 0x6b, 0xfa,
            0x88, 0x19, 0x62, 0xf3, 0x81, 0x10, 0x65, 0xf4, 0x86, 0x17, 0x48, 0xd9, 0xab, 0x3a, 0x4f, 0xde, 0xac, 0x3d,
            0x46, 0xd7, 0xa5, 0x34, 0x41, 0xd0, 0xa2, 0x33, 0x54, 0xc5, 0xb7, 0x26, 0x53, 0xc2, 0xb0, 0x21, 0x5a, 0xcb,
            0xb9, 0x28, 0x5d, 0xcc, 0xbe, 0x2f, 0xe0, 0x71, 0x03, 0x92, 0xe7, 0x76, 0x04, 0x95, 0xee, 0x7f, 0x0d, 0x9c,
            0xe9, 0x78, 0x0a, 0x9b, 0xfc, 0x6d, 0x1f, 0x8e, 0xfb, 0x6a, 0x18, 0x89, 0xf2, 0x63, 0x11, 0x80, 0xf5, 0x64,
            0x16, 0x87, 0xd8, 0x49, 0x3b, 0xaa, 0xdf, 0x4e, 0x3c, 0xad, 0xd6, 0x47, 0x35, 0xa4, 0xd1, 0x40, 0x32, 0xa3,
            0xc4, 0x55, 0x27, 0xb6, 0xc3, 0x52, 0x20, 0xb1, 0xca, 0x5b, 0x29, 0xb8, 0xcd, 0x5c, 0x2e, 0xbf, 0x90, 0x01,
            0x73, 0xe2, 0x97, 0x06, 0x74, 0xe5, 0x9e, 0x0f, 0x7d, 0xec, 0x99, 0x08, 0x7a, 0xeb, 0x8c, 0x1d, 0x6f, 0xfe,
            0x8b, 0x1a, 0x68, 0xf9, 0x82, 0x13, 0x61, 0xf0, 0x85, 0x14, 0x66, 0xf7, 0xa8, 0x39, 0x4b, 0xda, 0xaf, 0x3e,
            0x4c, 0xdd, 0xa6, 0x37, 0x45, 0xd4, 0xa1, 0x30, 0x42, 0xd3, 0xb4, 0x25, 0x57, 0xc6, 0xb3, 0x22, 0x50, 0xc1,
            0xba, 0x2b, 0x59, 0xc8, 0xbd, 0x2c, 0x5e, 0xcf
        };

        public Packet(DateTime dt, string contents, char pt, string pe)
        {
            Time = dt;
            Payload = contents;
            PacketType = pt;
            PacketEnd = pe;
        }

        public Packet(DateTime dt, char pt, string et)
        {
            Time = dt;
            PacketType = pt;
            ErrorType = et;
        }

        public DateTime Time { get; set; }
        public string Payload { get; set; }
        public char PacketType { get; set; }
        public string ErrorType { get; set; }
        public string PacketEnd { get; set; }

        public int GetNumberOfBytes()
        {
            var bytes = 0;
            if (Payload == null) return bytes;
            for (var i = 0; i < Payload.Length - 1; i += 3)
                bytes++;
            return bytes;
        }

        public void CheckRMAP()
        {
            if (!IsRMAP()) return;
            var dataLength = FindDataLength();
            var headerstart = FindStartOfRmap();
            if (dataLength == 0)
            {
                var payloadcrc = ComputeChecksum(CreateByteArray(Payload.Substring(headerstart)));
                if (payloadcrc != 0)
                    ErrorType = "Header Only CRC Error";
            }
            else
                //if (dataLength>0)
            {
                var datapart = (dataLength + 1)*3;
                var headercrc =
                    ComputeChecksum(
                        CreateByteArray(Payload.Substring(headerstart, Payload.Length - datapart - headerstart)));
                var datacrc = ComputeChecksum(CreateByteArray(Payload.Substring(Payload.Length - datapart)));
                if (headercrc != 0)
                    ErrorType = "Header CRC Error";
                if (datacrc != 0)
                    ErrorType += "Data CRC Error";
            }
        }

        public static byte ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if ((bytes != null) && (bytes.Length > 0))
                foreach (var b in bytes)
                    crc = Table[crc ^ b];
            return crc;
        }

        public byte[] CreateByteArray(string x)
        {
            x = x.Replace(" ", "");
            var z = new List<byte>();
            for (var i = 0; i < x.Length; i += 2)
                z.Add((byte) int.Parse(x.Substring(i, 2), NumberStyles.HexNumber));
            return z.ToArray();
        }

        private bool IsRMAP()
        {
            var bytes = CreateByteArray(Payload);
            for (var i = 0; i < bytes.Length; i++)
                if (int.Parse(bytes[i].ToString(), NumberStyles.HexNumber) > 31)
                    if (bytes[i + 1].ToString().Equals("1"))
                        return true;
                    else
                        return false;
            return false;
        }

        private int FindDataLength()
        {
            var ba = CreateByteArray(Payload);
            for (var i = 0; i < ba.Length - 2; i++)
            {
                var possibleDataLength = Convert24BitToint(ba[i], ba[i + 1], ba[i + 2]);
                if (i + 5 + possibleDataLength == ba.Length)
                    return possibleDataLength;
            }
            //Console.WriteLine("Length not found" +x);
            return -1;
        }

        public int Convert24BitToint(byte a, byte b, byte c)
        {
            var x = int.Parse(a.ToString(), NumberStyles.HexNumber);
            var y = int.Parse(b.ToString(), NumberStyles.HexNumber);
            var z = int.Parse(c.ToString(), NumberStyles.HexNumber);
            return c + b*256 + a*65536;
        }

        private int FindStartOfRmap()
        {
            var bytes = CreateByteArray(Payload);
            for (var i = 0; i < bytes.Length; i++)
                if (int.Parse(bytes[i].ToString(), NumberStyles.HexNumber) > 31)
                    if (bytes[i + 1].ToString().Equals("1"))
                        return i*3;
            return 0;
        }

        public int FindStartOfPacket()
        {
            var bytes = CreateByteArray(Payload);
            for (var i = 0; i < bytes.Length; i++)
                if (int.Parse(bytes[i].ToString(), NumberStyles.HexNumber) > 31)
                    return i*3;
            return 0;
        }

        public int FindHeaderEnd()
        {
            var ba = CreateByteArray(Payload.Substring(FindStartOfPacket()));
            for (var i = 0; i < ba.Length - 2; i++)
            {
                var possibleDataLength = Convert24BitToint(ba[i], ba[i + 1], ba[i + 2]);
                if ((i + 5 + possibleDataLength == ba.Length) && (possibleDataLength != 0))
                    return (i + 4)*3 - 1;
            }
            return -1;
        }
    }
}