using System;
using System.Collections.Generic;
using System.IO;

namespace Star_Reader.Model
{
    public class FileReader
    {

        //constructor
        public FileReader()
        {

        }

        public Recording StoreRecording(string path)
        {
            string[] lines = File.ReadAllLines(path);
            Recording r = new Recording
            {
                PacketStartTime = Convert.ToDateTime(lines[0]),
                Port = int.Parse(lines[1]),
                PacketEndTime = Convert.ToDateTime(lines[lines.Length - 1])
            };

            int jump = 5;
            for (int i = 3; i < lines.Length - 3; i += jump)
            {
                DateTime dt = Convert.ToDateTime(lines[i]);
                char pt = Convert.ToChar(lines[i + 1]);
                Packet p;
                switch (pt)
                {
                    case 'P':
                        string payload = lines[i + 2];
                        string message = lines[i + 3];
                        string packetend = lines[i + 3];

                        p = new Packet(dt, payload, pt, packetend);
                        //toggle off next line to dissable CRC Checks
                        p.CheckRMAP();
                        r.AddPacket(p);
                        jump = 5;
                        break;
                    case 'E':
                        string errorType = lines[i + 2];
                        p = new Packet(dt, pt, errorType);
                        r.AddPacket(p);
                        jump = 4;
                        break;
                }
            }
            return r;
        }//end StoreRecording
    }
}
