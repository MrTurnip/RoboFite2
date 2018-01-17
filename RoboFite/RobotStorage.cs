using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace RoboFite
{
    public class RobotStorage
    {
        public TeamRoster roster;

        public Accutron GenerateAccutron(Robot.Team assignToTeam)
        {
            return new Accutron(roster, assignToTeam);
        }

        public RobotStorage(TeamRoster roster)
        {
            this.roster = roster;
        }
    }
}
