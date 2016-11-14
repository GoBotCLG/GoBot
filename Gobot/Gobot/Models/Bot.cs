using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class Bot
    {
        public Bot(int id, string name, int kills, int deaths, int assists, string gun = "aléatoire")
        {
            Id = id;
            Name = name;
            Kills = kills;
            Deaths = deaths;
            Assists = assists;
            Gun = gun;
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public string Gun { get; set; }
    }
}