using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class Match
    {
        public Match()
        {
            TeamIds = new int[2];
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int[] TeamIds { get; set; }
        public int TeamVictoire { get; set; }
    }
}