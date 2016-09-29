using System.Collections.Generic;

namespace Star_Reader.Model
{
    public class Graphing
    {
        public List<double> GetPlots(Recording r)
        {
            var dataRatePerMinute = 0;
            var plot = new List<double>();
            var currentPacket = r.ListOfPackets[0];
            var interval = currentPacket.Time;
            var end = r.ListOfPackets[r.ListOfPackets.Count - 1];
            var increment = 1;
            var smallFileincrement = 1;

            if (r.ListOfPackets.Count < 150)
                for (var x = 0; x < r.ListOfPackets.Count - 1; x++)
                {
                    if (increment >= r.ListOfPackets.Count) break;
                    do
                    {
                        if (currentPacket.Payload != null)
                        {
                            var removeWhitespace = currentPacket.Payload.Replace(" ", "");
                            dataRatePerMinute += removeWhitespace.Length/2;
                        }
                        //Console.WriteLine(x);
                        currentPacket = r.ListOfPackets[smallFileincrement];

                        smallFileincrement++;
                    } while (smallFileincrement < increment);
                    increment += 5;
                    plot.Add(dataRatePerMinute);
                    dataRatePerMinute = 0;
                }

            else
                for (var x = 0; x < r.ListOfPackets.Count - 1; x++)
                {
                    if (interval >= end.Time) break;
                    do
                    {
                        if (currentPacket.Payload != null)
                        {
                            var removeWhitespace = currentPacket.Payload.Replace(" ", "");
                            dataRatePerMinute += removeWhitespace.Length/2;
                        }
                        currentPacket = r.ListOfPackets[increment];
                        increment++;
                    } while (currentPacket.Time <= interval);

                    plot.Add(dataRatePerMinute);
                    interval = interval.AddMinutes(1);
                    dataRatePerMinute = 0;
                }
            return plot;
        } //end getPlots

        public List<double> GetBars(Recording r)
        {
            var barsDcPaEeEr = new List<double>();
            Packet currentPacket;
            var packetCount = r.ListOfPackets.Count;
            var DC = 0;
            var Parity = 0;
            var EEPs = 0;
            var Errors = 0;
            for (var x = 0; x < packetCount; x++)
            {
                currentPacket = r.ListOfPackets[x];
                switch (currentPacket.ErrorType)
                {
                    case "Disconnect":
                        DC++;
                        Errors++;
                        break;
                    case "Parity":
                        Parity++;
                        Errors++;
                        break;
                    case "EEP":
                        EEPs++;
                        Errors++;
                        break;
                    case "Error":
                        Errors++;
                        break;
                    default:
                        break;
                }

                switch (currentPacket.PacketType.ToString())
                {
                    case "Disconnect":
                        DC++;
                        Errors++;
                        break;
                    case "Parity":
                        Parity++;
                        Errors++;
                        break;
                    case "EEP":
                        EEPs++;
                        Errors++;
                        break;
                    case "Error":
                        Errors++;
                        break;
                    default:
                        break;
                }
            }
            barsDcPaEeEr.Add(DC);
            barsDcPaEeEr.Add(Parity);
            barsDcPaEeEr.Add(EEPs);
            barsDcPaEeEr.Add(Errors);
            return barsDcPaEeEr;
        } //end getBars
    }
}