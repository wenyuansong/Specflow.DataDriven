using Specflow.DataDriven.DataDriven;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples
{
    class TestObj
    {
        public string name { get; set; }
        public int value { get; set; }

        public Nested nest { get; set; }
        public List<string> stringList { get; set; }
        public List<int> intList { get; set; }
        public List<bool> boolList { get; set; }
        public List<Nested> nestList { get; set; }
    }

    class Nested 
    {
        public string name { get; set; }
        public string address { get; set; }
    }
}
