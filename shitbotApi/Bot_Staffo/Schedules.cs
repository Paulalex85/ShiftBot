using System;
using System.Collections.Generic;

namespace Bot_Staffo
{
    public class Schedules
    {
        public string Id;
        public string State;
        public Locations location;
        public DateTime Debut;
        public DateTime Fin;
        public List<Shifts> list_shift_inscription;
    }

    
}
