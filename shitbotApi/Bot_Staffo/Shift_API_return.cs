using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot_Staffo
{

    public class Shift_API_return
    {
        public bool finPage;
        public bool shiftComplet;
        public bool presenceHeaderPage;
        public int pagesMax;

        public Shift_API_return()
        {
            finPage = false;
            shiftComplet = true;
            presenceHeaderPage = false;
            pagesMax = -1;
        }
    }
}
