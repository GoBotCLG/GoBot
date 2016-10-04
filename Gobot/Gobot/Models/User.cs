﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Gobot.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public System.Drawing.Bitmap ProfilPic { get; set; }
        public int Credits { get; set; }
        public string SteamID { get; set; }
        public int Wins { get; set; }
        public int Games { get; set; }
        public int TotalCredits { get; set; }
        public int EXP { get; set; }
        public int Level { get; set; }
    }
}