using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public enum GameResult
    {
        None,
        Draw,
        [Description("Team 1 Wins")]
        Team1,
        [Description("Team 2 Wins")]
        Team2
    };
}
