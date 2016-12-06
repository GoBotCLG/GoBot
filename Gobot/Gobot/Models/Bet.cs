using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class Bet
    {
        public Bet(int id, long amount, int profit, string username, int teamid, int matchid)
        {
            Id = id;
            Amount = amount;
            Profit = profit;
            Username = username;
            TeamId = teamid;
            MatchId = matchid;
        }

        public int Id { get; set; }
        public long Amount { get; set; }
        public int Profit { get; set; }
        public string Username { get; set; }
        public int TeamId { get; set; }
        public int MatchId { get; set; }

        public static int xpBet = 100;
    }
}