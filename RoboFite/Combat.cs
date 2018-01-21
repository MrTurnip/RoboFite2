using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;

namespace RoboFite
{
    public class Turn
    {
        public virtual void Take()
        {

        }

        public Turn()
        {

        }
    }

    public class CombatTurn : Turn
    {
        protected Robot _actor, _subject;
        protected Robot.Option _chosenOption;
        protected TeamRoster _roster;
        protected Robot.Team ActiveTeam;
        protected int _memberIndex = 0;
        protected Robot ActiveMember;
        protected bool TeamHasTakenTurn = false;
        public static Robot.Team winningTeam = Robot.Team.None;

        protected readonly Scene.Coordinates PrintCoordinates;

        private void SetCursorToPrintCoordinates()
        {
            Console.SetCursorPosition(PrintCoordinates.X, PrintCoordinates.Y);
        }

        public static void ClearRow(int startingRow, int distanceCleared)
        {
            int totalDistance = startingRow + distanceCleared;
            int originRow = startingRow;
            for (; startingRow < totalDistance; startingRow++)
            {
                Console.SetCursorPosition(0, startingRow);
                for (int i = 0; i < Console.WindowWidth; i++)
                {
                    Console.Write(' ');
                }
            }
            Console.SetCursorPosition(0, originRow);
        }

        private void ClearWindow() => Console.Clear();

        public static void WriteLine(object message) => Console.WriteLine(message);

        protected void ResetConsoleForNextPhase()
        {
            //ClearRow(PrintCoordinates.Y, 5);
            ClearWindow();
            SetCursorToPrintCoordinates();
        }

        protected ConsoleKey GetKey() => Console.ReadKey(true).Key;
        protected char GetKeyChar() => Console.ReadKey(true).KeyChar;

        private static void SetColors(ConsoleColor background = ConsoleColor.Black, ConsoleColor foreground = ConsoleColor.White)
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
        }

        public static void WriteBanner(object message, ConsoleColor background = ConsoleColor.Red, ConsoleColor foreground = ConsoleColor.Black)
        {
            SetColors(background, foreground);
            ClearRow(0, 1);
            WriteLine(message);
            SetColors();
        }

        public static void WriteError(object message)
        {
            SetColors(ConsoleColor.Red, ConsoleColor.White);
            ClearRow(Console.BufferHeight - 1, 1);
            WriteLine("No.");
            SetColors();
        }

        protected void ActorUseOptionOnSubject()
        {
            ResetConsoleForNextPhase();
            WriteBanner(ActiveMember.Name + " uses " + _chosenOption.Name + " on " + _subject.Name + ".");
            _chosenOption.Use(_subject);
        }

        //TODO have team displays switch when user targets.

        protected virtual void AssignSubject()
        {
            ResetConsoleForNextPhase();

            WriteBanner("Choose your target. Using " + _chosenOption.Name + ".");

            foreach (var robot in _roster.AllRobots)
            {
                if (robot.GetTeam != ActiveTeam && !robot.GetHealth.IsDefeated)
                {
                    WriteLine(robot.GetProfile.Details + " " + robot.GetHealth.Total.GetComparison());
                }
            }

            bool robotFound = false;
            do
            {
                if (_chosenOption.GetTargeting == Robot.Option.Targeting.Self)
                {
                    _subject = ActiveMember;
                    break;
                }

                char input = GetKeyChar();

                foreach (var robot in _roster.AllRobots)
                {
                    if (robot.GetProfile.Call == input && !robot.GetHealth.IsDefeated)
                    {
                        _subject = robot;
                        robotFound = true;
                    }
                }
            } while (!robotFound);
        }

        protected void ListOptions(Robot member)
        {
            foreach (var option in member.Options)
            {
                if (option.IsAvailable)
                    WriteLine(option.Details);
            }
        }

        protected void AssignUsedOptionFromActiveMember()
        {
            bool optionFound = false;
            do
            {
                char keyChar = GetKeyChar();
                foreach (var option in ActiveMember.Options)
                {
                    if (keyChar == option.Call && option.IsAvailable)
                    {
                        _chosenOption = option;
                        optionFound = true;
                    }
                }
            } while (!optionFound);
        }

        protected virtual void AssignOption()
        {
            ResetConsoleForNextPhase();
            WriteBanner(ActiveTeam + "'s turn. " + ActiveMember.Name + "(" + ActiveMember.GetHealth.Total.GetComparison() + ")" + " is awaiting orders.");
            ListOptions(ActiveMember);
            AssignUsedOptionFromActiveMember();
        }

        protected virtual void AssignActor(Robot target)
        {
            ActiveMember = target;
            ActiveTeam = ActiveMember.GetTeam;
        }

        protected void ToggleActiveTeam()
        {
            ActiveTeam = ActiveTeam == Robot.Team.Red ? Robot.Team.Blue : Robot.Team.Red;
        }

        public CombatTurn(TeamRoster roster, Scene.Coordinates printCoordinates)
        {
            _roster = roster;
            PrintCoordinates = printCoordinates;
        }
    }

    public class RedTeamCombatTurn : CombatTurn
    {
        public void RobotTakeTurn(Robot target)
        {
            if (target.GetHealth.IsDefeated)
                return;

            AssignActor(target);
            AssignOption();
            AssignSubject();
            ActorUseOptionOnSubject();

            int redDefeated = 0;
            int blueDefeated = 0;
            foreach (Robot robot in _roster.AllRobots)
            {
                if (robot.GetTeam == Robot.Team.Blue && robot.GetHealth.IsDefeated)
                    blueDefeated++;
                if (blueDefeated == 2)
                {
                    winningTeam = Robot.Team.Red;
                }

                if (robot.GetTeam == Robot.Team.Red && robot.GetHealth.IsDefeated)
                    redDefeated++;
                if (redDefeated == 2)
                {
                    winningTeam = Robot.Team.Blue;
                }

            }
        }

        public RedTeamCombatTurn(TeamRoster roster) : base(roster, new Scene.Coordinates(0, 1))
        {

        }
    }

    public class CombatResults
    {
        public Robot.Team Winner { get; set; } = Robot.Team.None;
        public Robot.Team Loser { get; set; } = Robot.Team.None;
    }

    public class Combat : Scene
    {
        protected TeamRoster _teamRoster;
        protected RedTeamCombatTurn _redTeamTurn, _blueTeamTurn;

        protected override CombatResults Update()
        {
            CombatResults results = new CombatResults();

            foreach (Robot robot in _teamRoster.AllRobots)
            {
                _redTeamTurn.RobotTakeTurn(robot);
                if (CombatTurn.winningTeam != Robot.Team.None)
                {
                    results.Winner = CombatTurn.winningTeam;
                    break;
                }
            }

            return results;
        }

        public Combat(TeamRoster teamRoster) : base()
        {
            _teamRoster = teamRoster;
            _redTeamTurn = new RedTeamCombatTurn(_teamRoster);
            _blueTeamTurn = new RedTeamCombatTurn(_teamRoster);

        }
    }

    public class CombatPresetTeamsA : Combat
    {
        public CombatPresetTeamsA(TeamRoster teamRoster) : base(teamRoster)
        {
            var accutronA = new Accutron(teamRoster, Robot.Team.Red);
            var accutronB = new Accutron(teamRoster, Robot.Team.Red);
            var accutronC = new Accutron(teamRoster, Robot.Team.Blue);
            var accutronD = new Accutron(teamRoster, Robot.Team.Blue);

            accutronA.GetProfile.Name = "Accutron A";
            accutronA.GetProfile.Call = 'a';
            //accutronA.GetHealth.ReduceToInstakill();
            accutronB.GetProfile.Name = "Accutron B";
            accutronB.GetProfile.Call = 'b';
           // accutronB.GetHealth.ReduceToInstakill();
            accutronC.GetProfile.Name = "Accutron C";
            accutronC.GetProfile.Call = 'c';
          //  accutronC.GetHealth.ReduceToInstakill();
            accutronD.GetProfile.Name = "Accutron D";
            accutronD.GetProfile.Call = 'd';
            //accutronD.GetHealth.ReduceToInstakill();

            _teamRoster.AllRobots.Add(accutronA);
            _teamRoster.AllRobots.Add(accutronB);
            _teamRoster.AllRobots.Add(accutronC);
            _teamRoster.AllRobots.Add(accutronD);
        }
    }
}