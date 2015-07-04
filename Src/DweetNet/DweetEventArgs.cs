using DweetNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DweetNet
{
    public class DweetEventArgs : EventArgs
    {
        public DweetEventArgs(Dweet dweet)
        {
            Dweet = dweet;
        }

        public Dweet Dweet { get; set; }
    }
}
