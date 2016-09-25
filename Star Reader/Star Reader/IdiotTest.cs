using Star_Reader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Star_Reader
{
    class IdiotTest
    {
        //returns true for babbling idiot otherwise false
        public bool Test(Recording r)
        {
            if(r.ListOfPackets == null || r.ListOfPackets.Count < 4)
            {
                return false;
            }
            for (int x = 0; x < r.ListOfPackets.Count; x++)
            {
                if((x+ 4) > r.ListOfPackets.Count)
                {
                    return false;
                }
                if (r.ListOfPackets[x].Time == r.ListOfPackets[x + 1].Time
                    && r.ListOfPackets[x + 1].Time == r.ListOfPackets[x + 2].Time
                    && r.ListOfPackets[x + 2].Time == r.ListOfPackets[x + 3].Time
                    && r.ListOfPackets[x + 3].Time == r.ListOfPackets[x + 4].Time)
                {
                    return true;
                }
                if (r.ListOfPackets[x].Payload == r.ListOfPackets[x + 1].Payload
                    && r.ListOfPackets[x + 1].Payload == r.ListOfPackets[x + 2].Payload
                    && r.ListOfPackets[x + 2].Payload == r.ListOfPackets[x + 3].Payload
                    && r.ListOfPackets[x + 3].Payload == r.ListOfPackets[x + 4].Payload)
                {
                    return true;
                }
            }
            return false;
        }//end Test

    }
}
