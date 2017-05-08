using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Scoreboard
{
    public class GameEventList : BindingList<GameEvent>
    {
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            foreach (GameEvent gameEvent in this)
            {
                result.AppendLine(gameEvent.ToString());
            }

            return result.ToString();
        }
    }
}
