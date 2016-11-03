using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class Team
    {
        public Team()
        {
            TeamComp = new Bot[5];
        }

        public Team(int id, string name, int wins, int games, string imagepath, Bot b1, Bot b2, Bot b3, Bot b4, Bot b5)
        {
            Id = id;
            Name = name;
            Wins = wins;
            Games = games;
            ImagePath = imagepath;
            TeamComp = new Bot[5];
            TeamComp[0] = b1;
            TeamComp[1] = b2;
            TeamComp[2] = b3;
            TeamComp[3] = b4;
            TeamComp[4] = b5;

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Wins { get; set; }
        public int Games { get; set; }
        public string ImagePath { get; set; }
        public Bot[] TeamComp { get; set; }
    }
}