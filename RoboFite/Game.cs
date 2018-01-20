using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoboFite
{
    public class Team
    {
        protected string _name;
        protected List<Robot> _members = new List<Robot>();
        protected const int FirstMember = 0, SecondMember = 1;
        public Robot GetMember(int place)
        {
            try
            {
                return _members[place];
            }
            catch
            {
                throw new Exception("No robot exists.");
            }
        }

        public List<Robot> Members => _members;

        public string Name => _name;

        public int GetTeamCount => _members.Count;

        public Robot FirstRobot => GetMember(FirstMember);
        public Robot SecondRobot => GetMember(SecondMember);

        public void AddMember(Robot member)
        {
            _members.Add(member);
        }
    }

    public class RedTeam : Team
    {
        public RedTeam() : base()
        {
            _name = "Red Team";
        }
    }

    public class BlueTeam : Team
    {
        public BlueTeam() : base()
        {
            _name = "Blue Team";
        }
    }

    public class TeamRoster
    {
        private Team redTeam, blueTeam;

        public Team RedTeam => redTeam;
        public Team BlueTeam => blueTeam;

        public List<Robot> AllRobots = new List<Robot>();

        public TeamRoster()
        {
            redTeam = new RedTeam();
            blueTeam = new BlueTeam();
        }
    }

    public class Game
    {
        public class ActiveScene
        {
            private Scene _scene, _previousScene;
            private bool _isFinishedLooping = false;

            public void SwitchScene(Scene newScene)
            {
                try
                {
                    _previousScene = _scene;
                }
                catch
                {
                    _previousScene = newScene;
                }

                _scene = newScene;
            }

            public void LaunchLoop()
            {
                CombatResults results = new CombatResults();
                do
                {
                    results = _scene.Execute();
                } while (results.Winner == Robot.Team.None);

                Console.Clear();
                Console.WriteLine(results.Winner + " team won.");
                Console.ReadKey(true);
            }

            public ActiveScene(Scene startingScene)
            {
                SwitchScene(startingScene);
            }
        }

        private readonly ActiveScene _activeScene;
        private Combat _combat;
        private RobotStorage _robotStorage;
        private TeamRoster _teamRoster;

        public Game(int teamPreset = 0)
        {
            _teamRoster = new TeamRoster();
            _robotStorage = new RobotStorage(_teamRoster);


            switch (teamPreset)
            {
                case 2:
                    _combat = new CombatPresetTeamsA(_teamRoster);
                    break;
                default: _combat = new Combat(_teamRoster); break;
            }

            _activeScene = new ActiveScene(_combat);

            _activeScene.LaunchLoop();

        }
    }
}
