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

        public int Id { get; set; }
        public string Name { get; set; }
        public int Wins { get; set; }
        public int Games { get; set; }
        public Bot[] TeamComp { get; set; }
    }
}