using System.Collections.Generic;
using System.Linq;

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
                            dataRatePerMinute += removeWhitespace.Length / 2;
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
                            dataRatePerMinute += removeWhitespace.Length / 2;
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
            var packetCount = r.ListOfPackets.Count;
            var DC = 0;
            var Parity = 0;
            var oos = 0;
            var babbling = 0;
            var Errors = 0;
            var dCrc = 0;
            var hCrc = 0;
            var length = 0;
            for (var x = 0; x < packetCount; x++)
            {
                var currentPacket = r.ListOfPackets[x];

                var errors = new string[]
                {
                    "Disconnect", "Parity", "Out of Sequence.", "Babbling Idiot Detected.", "Data CRC Error.",
                    "Header CRC Error.", "Incorrect Data Length"
                };
                if (currentPacket.ErrorType != null)
                {
                    var contains = errors.FirstOrDefault<string>(s => currentPacket.ErrorType.Contains(s));
                    switch (contains)
                    {
                        case "Disconnect":
                            DC++;
                            Errors++;
                            break;
                        case "Parity":
                            Parity++;
                            Errors++;
                            break;
                        case "Out of Sequence.":
                            oos++;
                            Errors++;
                            break;
                        case "Babbling Idiot Detected.":
                            babbling++;
                            Errors++;
                            break;
                        case "Data CRC Error":
                            dCrc++;
                            Errors++;
                            break;
                        case "Header CRC Error":
                            hCrc++;
                            Errors++;
                            break;
                        case "Incorrect Data Length":
                            length++;
                            Errors++;
                            break;
                        default:
                            Errors++;
                            break;
                    }
                }
            }
            barsDcPaEeEr.Add(DC);
            barsDcPaEeEr.Add(Parity);
            barsDcPaEeEr.Add(oos);
            barsDcPaEeEr.Add(babbling);
            barsDcPaEeEr.Add(dCrc);
            barsDcPaEeEr.Add(hCrc);
            barsDcPaEeEr.Add(length);
            barsDcPaEeEr.Add(Errors);
            return barsDcPaEeEr;
        } //end getBars
    }
}