using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class Bet
    {
        public int Id { get; set; }
        public int Amount { get; set; }
        public int Profit { get; set; }
        public User User { get; set; }
        public int TeamId { get; set; }
        public int MatchId { get; set; }
    }
}