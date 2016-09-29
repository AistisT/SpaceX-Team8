using System;
using System.IO;

namespace Star_Reader.Model
{
    public class FileReader
    {
        public Recording StoreRecording(string path)
        {
            var lines = File.ReadAllLines(path);
            var r = new Recording
            {
                PacketStartTime = Convert.ToDateTime(lines[0]),
                Port = int.Parse(lines[1]),
                PacketEndTime = Convert.ToDateTime(lines[lines.Length - 1])
            };

            var jump = 5;
            for (var i = 3; i < lines.Length - 3; i += jump)
            {
                var dt = Convert.ToDateTime(lines[i]);
                var pt = Convert.ToChar(lines[i + 1]);
                Packet p;
                switch (pt)
                {
                    case 'P':
                        var payload = lines[i + 2];
                        var message = lines[i + 3];
                        var packetend = lines[i + 3];

                        p = new Packet(dt, payload, pt, packetend);
                        //toggle off next line to dissable CRC Checks
                        p.CheckRMAP();
                        r.AddPacket(p);
                        jump = 5;
                        break;
                    case 'E':
                        var errorType = lines[i + 2];
                        p = new Packet(dt, pt, errorType);
                        r.AddPacket(p);
                        jump = 4;
                        break;
                }
            }

            r.TestForBabblingIdiot();
            r.Findoutofsequencepackets();
            r.FindHeaderLength();
            r.FindDataLength();
            r.CheckDataLengths();
            return r;
        } //end StoreRecording
    }
}