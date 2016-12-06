using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string ProfilPic { get; set; }
        public long Credits { get; set; }
        public string SteamID { get; set; }
        public int Wins { get; set; }
        public int Games { get; set; }
        public int EXP { get; set; }
        public int Level { get; set; }
        public bool SessionUser { get; set; }
        public string favoriteTeam { get; set; }
        public int totalWon { get; set; }
        public int totalLoss { get; set; }
        public int gamesWatched { get; set; }

        public static int xpLvl = 1500;
    }
}