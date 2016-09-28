using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Star_Reader.Model
{
    public class Recording
    {
        public List<Packet> ListOfPackets { get; set; }
        public DateTime PacketStartTime { get; set; }
        public DateTime PacketEndTime { get; set; }
        public int Port { get; set; }
        public int ErrorsPresent { get; set; }

        public Recording()
        {
            ErrorsPresent = 0;
            ListOfPackets = new List<Packet>();
        }

        public void AddPacket(Packet toAdd)
        {
            ListOfPackets.Add(toAdd);
            if (toAdd.ErrorType != null)
            {
                ErrorsPresent++;
            }
            
        }

        public List<double>  getDataRates()
        {
            int timeInterval = 60;
            TimeSpan RecordingLength = getDurationOfRecording();
            DateTime DataStartPoint = PacketStartTime;
            DateTime DataEndPoint = PacketStartTime.AddSeconds(timeInterval);
            int seconds = (int) Math.Round(RecordingLength.TotalSeconds / timeInterval);
            List<double> datarate = new List<double>();
            for(int i=0;i<seconds;i++)
            {
                DataStartPoint = DataStartPoint.AddSeconds(timeInterval);
                DataEndPoint = DataEndPoint.AddSeconds(timeInterval);
                int packets = 0;
                for(int j=0;j<ListOfPackets.Count;j++)
                {
                    TimeSpan a = ListOfPackets[j].Time.Subtract(DataStartPoint);
                    TimeSpan b = DataEndPoint.Subtract(ListOfPackets[j].Time);
                    if (ListOfPackets[j].Time.Subtract(DataStartPoint).TotalSeconds <= timeInterval && DataEndPoint.Subtract(ListOfPackets[j].Time).TotalSeconds <= timeInterval)
                        packets+= ListOfPackets[j].getNumberOfBytes();
                }
                datarate.Add(packets);
            }
            return datarate;
        }

        /// <summary>
        /// Returns the total number of bytes contained in the recording. Calculated by each packet stored in the list
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfCharacters()
        {
            int rate = 0;
            for(int i=0;i<ListOfPackets.Count;i++)
            {
                rate += ListOfPackets[i].getNumberOfBytes();
            }
            return rate;
        }

        /// <summary>
        /// Returns the total time the recording takes. Use .TotalSeconds or .TotalMinutes to use it.
        /// </summary>
        /// <returns></returns>
        public TimeSpan getDurationOfRecording()
        {
            return PacketEndTime.Subtract(PacketStartTime);
        }

        /// <summary>
        /// Tests the recording for repeating packets being transmitted
        /// </summary>
        public void testForBabblingIdiot()
        {
            List<int> idiots = new List<int>();
            for (int i = 4; i < ListOfPackets.Count; i++)
            {
                string payload = ListOfPackets[i].Payload;
                if (payload == ListOfPackets[i - 1].Payload
                    && payload == ListOfPackets[i - 2].Payload
                    && payload == ListOfPackets[i - 3].Payload
                    && payload == ListOfPackets[i - 4].Payload)
                {
                    for (int j = (i - 4); j <= i; j++)
                        if (idiots.Contains(j) == false)
                        {
                            idiots.Add(j);
                        }
                }
            }
            if(idiots.Count!=0)
            {
                for(int k=0;k<idiots.Count();k++)
                {
                    ListOfPackets[idiots[k]].ErrorType += "Babbling Idiot Detected";
                    ErrorsPresent++;
                }
            }
        }

        public int findcounter()
        {
            List<int> possiblecounters = new List<int>();
            List<int> possiblepossibles = new List<int>();
            List<int> templist = new List<int>();
            //if(possiblecounters.Count!=1)
            for (int i = 0; i < ListOfPackets.Count() - 1; i++)
            {
                Packet a = ListOfPackets[i];
                Packet b = ListOfPackets[i + 1];

                if (a.Payload != null && b.Payload != null && possiblecounters.Count != 1)
                {
                    int aStart = a.findStartOfPacket(a.Payload);
                    int bStart = b.findStartOfPacket(b.Payload);
                    byte[] byteArrayA = a.createByteArray(a.Payload.Substring(aStart));
                    byte[] byteArrayB = b.createByteArray(b.Payload.Substring(bStart));
                    if (byteArrayA.Length == byteArrayB.Length)
                    {
                        for (int j = 0; j < byteArrayA.Length - 1; j++)
                        {
                                int x = a.convert24bitToint(00, byteArrayA[j], byteArrayA[j + 1]) -
                            a.convert24bitToint(00, byteArrayB[j], byteArrayB[j + 1]);
                            if (x < 0 && x >-255)
                            {
                                if (possiblepossibles.Contains(j) == false)
                                { possiblepossibles.Add(j); }
                            }
                        }
                        if(possiblecounters.Count==0)
                            {
                            possiblecounters.Clear();
                            for (int y = 0; y < possiblepossibles.Count; y++)
                            {
                                possiblecounters.Add(possiblepossibles[y]);
                            }
                        }
                            //Console.WriteLine(possiblecounters.Count);
                        for (int k = 0; k < possiblepossibles.Count; k++)
                        {
                            if (possiblecounters.Contains(possiblepossibles[k]))
                            {
                                templist.Add(possiblepossibles[k]);
                            }  
                        }
                            //if(i==8)Console.WriteLine(templist[0]+ " " + templist[1]);
                        possiblecounters.Clear();
                        for(int x=0; x<templist.Count;x++)
                        {
                            possiblecounters.Add(templist[x]);
                        }
                            possiblepossibles.Clear();
                            templist.Clear();
                    }
                }
            }
            Console.WriteLine("possible indexes: "+ possiblecounters[0]);
            return possiblecounters[0];
        }
        public void findoutofsequencepackets()
        {
            int counterloc = findcounter();
            for(int i=0;i<ListOfPackets.Count-1;i++)
            {
                Packet a = ListOfPackets[i];
                Packet b = ListOfPackets[i + 1];

                if (a.Payload != null && b.Payload != null)
                {
                    int aStart = a.findStartOfPacket(a.Payload);
                    int bStart = b.findStartOfPacket(b.Payload);
                    byte[] byteArrayA = a.createByteArray(a.Payload.Substring(aStart));
                    byte[] byteArrayB = b.createByteArray(b.Payload.Substring(bStart));
                    if(byteArrayA.Length==byteArrayB.Length)
                    if (a.convert24bitToint(00, byteArrayA[counterloc], byteArrayA[counterloc + 1]) -
                        a.convert24bitToint(00, byteArrayB[counterloc], byteArrayB[counterloc + 1]) == 1)
                    {
                        b.ErrorType += "Out of Sequence";
                        ErrorsPresent++;
                    }
                }
            }
        }
    }
}
