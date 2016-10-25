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
            Teams = new Team[2];
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Team[] Teams { get; set; }
        public int TeamVictoire { get; set; }
        public bool CurrentUserBet { get; set; }
        public int TeamNumberBet { get; set; }
        public int CurrentUserAmount { get; set; }
        public int Team1TotalBet { get; set; }
        public int Team2TotalBet { get; set; }
    }
}