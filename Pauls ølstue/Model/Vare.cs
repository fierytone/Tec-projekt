﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Vare
    {
        public int Id { get; set; }
        public string Navn { get; set; }
        public decimal Pris { get; set; }
        public DateTime Tidsstempel { get; set; }
    }
}
