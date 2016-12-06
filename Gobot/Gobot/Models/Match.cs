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

        public Match(int id, DateTime date, Team t1, Team t2, int teamvictoire, int t1totalbet, int t2totalbet, string Map)
        {
            Id = id;
            Teams = new Team[2];
            Teams[0] = t1;
            Teams[1] = t2;
            TeamVictoire = teamvictoire;
            Team1TotalBet = t1totalbet;
            Team2TotalBet = t2totalbet;
        }

        public int Id { get; set; }
        public DateTime Date { get; set; }
        public Team[] Teams { get; set; }
        public int TeamVictoire { get; set; }
        public bool CurrentUserBet { get; set; }
        public int TeamNumberBet { get; set; }
        public long CurrentUserAmount { get; set; }
        public long Team1TotalBet { get; set; }
        public long Team2TotalBet { get; set; }
        public int Team1Rounds { get; set; }
        public int Team2Rounds { get; set; }
        public string Map { get; set; }

        public static int xpReward = 10;
    }
}