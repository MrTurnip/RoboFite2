using System;
using System.Collections.Generic;

namespace RoboFite
{
    public class Accutron : Robot
    {
        public class Rifle : Primary
        {
            private const int MinimumDamage = 30;
            private const int MaximumDamage = 50;
            public Rifle(Robot owner) : base(owner, MinimumDamage, MaximumDamage, 1, 3, 75)
            {
                _call = 'r';
                _name = "Rifle";
            }
        }

        public class Pistol : Secondary
        {
            private const int MinimumDamage = 15;
            private const int MaximumDamage = 25;
            public Pistol(Robot owner) : base(owner, MinimumDamage, MaximumDamage, 2, 5, 56)
            {
                _call = 'p';
                _name = "Pistol";
            }
        }

        public class ShellGenerator : Ability
        {
            public override void Use(Robot target)
            {
                Console.WriteLine("Restoring Shell.");
                _owner.GetHealth.Shell.Remaining = 1;
                _owner.GetHealth.ListHealthBlocks();

                Console.ReadKey(true);
            }

            public ShellGenerator(Robot owner) : base(owner)
            {
                _call = 's';
                _name = "Shield Generator";
            }
        }

        public class LaserSweep : Ultimate
        {
            private void DamageTarget(Robot target)
            {
                FireAtTarget(target);
            }

            public override void Use(Robot target)
            {
                List<Robot> allRobots = _owner.Options.FullRoster.AllRobots;
                foreach (Robot robot in allRobots)
                {
                    if (robot.GetTeam != _owner.GetTeam)
                    {
                        DamageTarget(robot);
                        robot.GetHealth.ListHealthBlocks();
                    }
                }

                Console.ReadKey(true);
            }

            private const int MinimumDamage = 45;
            private const int MaximumDamage = 55;
            public LaserSweep(Robot owner) : base(owner, MinimumDamage, MaximumDamage, 1, 1, 100, 100)
            {
                _call = 'l';
                _name = "Laser Sweep";
            }
        }

        public Accutron(TeamRoster teamRoster, Team team) : base(teamRoster, team)
        {
            _profile = new Profile("Accutron", "High accuracy, low armor sniper", 'a');

            primary = new Rifle(this);
            secondary = new Pistol(this);
            ability = new ShellGenerator(this);
            ultimate = new LaserSweep(this);

            _options.Add(primary);
            _options.Add(secondary);
            _options.Add(ability);
            _options.Add(ultimate);
        }
    }
}