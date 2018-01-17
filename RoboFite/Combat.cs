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
        protected Team ActiveTeam;
        private int _memberIndex = 0;
        protected Robot ActiveMember => ActiveTeam.GetMember(_memberIndex);

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


        protected virtual void AssignSubject()
        {
            ResetConsoleForNextPhase();

            WriteBanner("Choose your target. Using " + _chosenOption.Name + ".");


            foreach (var robot in _roster.allMembersList)
            {
                if (robot.GetTeam == Robot.Team.Blue)
                {
                    WriteLine(robot.GetProfile.Details);
                }
            }

            bool robotFound = false;
            do
            {
                char input = GetKeyChar();

                foreach (var robot in _roster.allMembersList)
                {
                    if (robot.GetProfile.Call == input)
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
            WriteBanner(ActiveTeam.Name + "'s turn. " + ActiveMember.Name + " is awaiting orders.");
            ListOptions(ActiveMember);
            AssignUsedOptionFromActiveMember();
        }

        protected virtual void AssignActor()
        {
            // Actor is automatically assigned.
        }

        public CombatTurn(TeamRoster roster, Scene.Coordinates printCoordinates)
        {
            _roster = roster;
            PrintCoordinates = printCoordinates;
        }
    }


    public class RedTeamCombatTurn : CombatTurn
    {
        public override void Take()
        {
            Console.Write("Red Team active.");
            AssignActor();
            AssignOption();
            AssignSubject();

            ActorUseOptionOnSubject();
        }

        public RedTeamCombatTurn(TeamRoster roster) : base(roster, new Scene.Coordinates(0, 1))
        {
            ActiveTeam = roster.RedTeam;
        }
    }

    public class Combat : Scene
    {
        protected TeamRoster _teamRoster;
        protected RedTeamCombatTurn _redTeamTurn, _blueTeamTurn;

        protected override void Update()
        {
            _redTeamTurn.Take();
            _blueTeamTurn.Take();
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

            Team redTeam = teamRoster.RedTeam;
            redTeam.AddMember(accutronA);
            redTeam.AddMember(accutronB);
            Team blueTeam = teamRoster.BlueTeam;
            blueTeam.AddMember(accutronC);
            blueTeam.AddMember(accutronD);
        }
    }
}