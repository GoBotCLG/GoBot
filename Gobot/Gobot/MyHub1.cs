using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Gobot
{
    public class LiveStreamHub : Hub
    {
        public void UpdateScore(string jsonScoreString)
        {
            Clients.All.updateScore(jsonScoreString);
        }
    }
}