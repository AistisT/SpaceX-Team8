using System;
using System.Collections.Generic;
using System.Linq;

namespace Star_Reader.Model
{
    public class Recording
    {
        public Recording()
        {
            ErrorsPresent = 0;
            ListOfPackets = new List<Packet>();
        }

        public List<Packet> ListOfPackets { get; set; }
        public DateTime PacketStartTime { get; set; }
        public DateTime PacketEndTime { get; set; }
        public int Port { get; set; }
        public int ErrorsPresent { get; set; }
        public int HeaderLength { get; set; }
        public int DataLength { get; set; }

        public void AddPacket(Packet toAdd)
        {
            ListOfPackets.Add(toAdd);
            if (toAdd.ErrorType != null)
                ErrorsPresent++;
        }

        public List<double> GetDataRates()
        {
            var timeInterval = 60;
            var recordingLength = GetDurationOfRecording();
            var dataStartPoint = PacketStartTime;
            var dataEndPoint = PacketStartTime.AddSeconds(timeInterval);
            var seconds = (int)Math.Round(recordingLength.TotalSeconds / timeInterval);
            var datarate = new List<double>();
            for (var i = 0; i < seconds; i++)
            {
                dataStartPoint = dataStartPoint.AddSeconds(timeInterval);
                dataEndPoint = dataEndPoint.AddSeconds(timeInterval);
                var packets = 0;
                foreach (Packet packet in ListOfPackets)
                {
                    var a = packet.Time.Subtract(dataStartPoint);
                    var b = dataEndPoint.Subtract(packet.Time);
                    if ((packet.Time.Subtract(dataStartPoint).TotalSeconds <= timeInterval) &&
                        (dataEndPoint.Subtract(packet.Time).TotalSeconds <= timeInterval))
                        packets += packet.GetNumberOfBytes();
                }
                datarate.Add(packets);
            }
            return datarate;
        }

        /// <summary>
        ///     Returns the total number of bytes contained in the recording. Calculated by each packet stored in the list
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfCharacters()
        {
            var rate = 0;
            foreach (Packet packet in ListOfPackets)
                rate += packet.GetNumberOfBytes();
            return rate;
        }

        /// <summary>
        ///     Returns the total time the recording takes. Use .TotalSeconds or .TotalMinutes to use it.
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetDurationOfRecording()
        {
            return PacketEndTime.Subtract(PacketStartTime);
        }

        /// <summary>
        ///     Tests the recording for repeating packets being transmitted
        /// </summary>
        public void TestForBabblingIdiot()
        {
            var idiots = new List<int>();
            for (var i = 4; i < ListOfPackets.Count; i++)
            {
                var payload = ListOfPackets[i].Payload;
                if ((payload != ListOfPackets[i - 1].Payload) || (payload != ListOfPackets[i - 2].Payload) ||
                    (payload != ListOfPackets[i - 3].Payload) || (payload != ListOfPackets[i - 4].Payload)) continue;
                for (var j = i - 4; j <= i; j++)
                    if (idiots.Contains(j) == false)
                        idiots.Add(j);
            }
            if (idiots.Count == 0) return;
            for (var k = 0; k < idiots.Count(); k++)
            {
                ListOfPackets[idiots[k]].ErrorType += "Babbling Idiot Detected. ";
                ErrorsPresent++;
            }
        }

        public int Findcounter()
        {
            var possiblecounters = new List<int>();
            var possiblepossibles = new List<int>();
            var templist = new List<int>();
            //if(possiblecounters.Count!=1)
            for (var i = 0; i < ListOfPackets.Count() - 1; i++)
            {
                var a = ListOfPackets[i];
                var b = ListOfPackets[i + 1];

                if ((a.Payload == null) || (b.Payload == null) || (possiblecounters.Count == 1)) continue;
                var aStart = a.FindStartOfPacket();
                var bStart = b.FindStartOfPacket();
                var byteArrayA = a.CreateByteArray(a.Payload.Substring(aStart));
                var byteArrayB = b.CreateByteArray(b.Payload.Substring(bStart));
                if (byteArrayA.Length != byteArrayB.Length) continue;
                for (var j = 0; j < byteArrayA.Length - 1; j++)
                {
                    var x = a.Convert24BitToint(00, byteArrayA[j], byteArrayA[j + 1]) -
                            a.Convert24BitToint(00, byteArrayB[j], byteArrayB[j + 1]);
                    if ((x >= 0) || (x <= -255)) continue;
                    if (possiblepossibles.Contains(j) == false)
                        possiblepossibles.Add(j);
                }
                if (possiblecounters.Count == 0)
                {
                    possiblecounters.Clear();
                    foreach (int counter in possiblepossibles)
                        possiblecounters.Add(counter);
                }
                //Console.WriteLine(possiblecounters.Count);
                foreach (int counter in possiblepossibles)
                    if (possiblecounters.Contains(counter))
                        templist.Add(counter);
                //if(i==8)Console.WriteLine(templist[0]+ " " + templist[1]);
                possiblecounters.Clear();
                foreach (int counter in templist)
                    possiblecounters.Add(counter);
                possiblepossibles.Clear();
                templist.Clear();
            }
            // Console.WriteLine("possible indexes: " + possiblecounters[0]);
            return possiblecounters[0];
        }

        public void Findoutofsequencepackets()
        {
            var counterloc = Findcounter();
            for (var i = 0; i < ListOfPackets.Count - 1; i++)
            {
                var a = ListOfPackets[i];
                var b = ListOfPackets[i + 1];

                if ((a.Payload != null) && (b.Payload != null))
                {
                    var aStart = a.FindStartOfPacket();
                    var bStart = b.FindStartOfPacket();
                    var byteArrayA = a.CreateByteArray(a.Payload.Substring(aStart));
                    var byteArrayB = b.CreateByteArray(b.Payload.Substring(bStart));
                    if (byteArrayA.Length == byteArrayB.Length)
                        if (a.Convert24BitToint(00, byteArrayA[counterloc], byteArrayA[counterloc + 1]) -
                            a.Convert24BitToint(00, byteArrayB[counterloc], byteArrayB[counterloc + 1]) == 1)
                        {
                            b.ErrorType += "Out of Sequence. ";
                            ErrorsPresent++;
                        }
                }
            }
        }

        public void FindHeaderLength()
        {
            foreach (var p in ListOfPackets)
            {
                if ((HeaderLength != 0) || (p.Payload == null)) continue;
                var startofpacket = p.FindStartOfPacket();
                HeaderLength = p.FindHeaderEnd();
                //Console.WriteLine(p.Payload.Substring(p.findStartOfPacket(); + HeaderLength + 1));
            }
        }

        public void FindDataLength()
        {
            foreach (var p in ListOfPackets)
            {
                if ((DataLength != 0) || (p.Payload == null) || (HeaderLength == 0)) continue;
                var startofpacket = p.FindStartOfPacket();
                DataLength = p.Payload.Substring(p.FindStartOfPacket() + HeaderLength + 1).Length;
                //Console.WriteLine(p.Payload.Substring(p.findStartOfPacket() + HeaderLength + 1));
                // Console.WriteLine(p.Payload.Substring(p.Payload.Length - DataLength));
                //Console.WriteLine(startofpacket + " " + HeaderLength + " " + DataLength + 1 + ": " +p.Payload.Length);
            }
        }

        public void CheckDataLengths()
        {
            foreach (var p in ListOfPackets)
            {
                if ((DataLength == 0) || (p.Payload == null) || (HeaderLength == 0)) continue;
                if (p.FindStartOfPacket() + HeaderLength + DataLength + 1 == p.Payload.Length == false)
                    p.ErrorType += "Incorrect Data Length. ";
            }
        }
    }
}