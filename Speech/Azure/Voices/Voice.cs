using System;
using System.Collections.Generic;
using System.Text;

namespace VRCTTS
{
    class Voice
    {
        public string name { get; set; }
        public string[] style { get; set; }
        public string[] pitch { get;}

        public Voice(string name, string[] style) {
            this.name = name;
            this.style = style;

            pitch = new string[6];

            pitch[0] = "x-low";
            pitch[1] = "low";
            pitch[2] = "medium";
            pitch[3] = "high";
            pitch[4] = "x-high";
            pitch[5] = "default";
        }
    }
}
