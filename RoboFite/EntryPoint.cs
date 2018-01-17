using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboFite
{
    public class EntryPoint
    {
        static void Main(string[] args)
        {
            const int teamPresetA = 2;
            Game game = new Game(teamPresetA);
        }
    }
}
