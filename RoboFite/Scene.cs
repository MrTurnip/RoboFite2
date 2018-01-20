using System;

namespace RoboFite
{
    public class Scene
    {
        public struct Coordinates
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Coordinates(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        protected virtual CombatResults Update()
        {
            return new CombatResults();
        }

        protected void Write(object message, Coordinates coordinates)
        {
            Console.CursorLeft = coordinates.X;
            Console.CursorTop = coordinates.Y;
            Console.Write(message);
        }

        protected ConsoleKey GetKey() => Console.ReadKey().Key;
        protected void ClearConsole() => Console.Clear();

        public CombatResults Execute()
        {
            return Update();
        }
    }
}