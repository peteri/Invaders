using System;
using System.Collections.Generic;
using WpfInvaders;

namespace Stm8autogen
{
    class Program
    {
        static List<(string name, Sprite sprite)> sprites = new List<(string name, Sprite sprite)>();
        [STAThread]
        static void Main()
        {
            var mainWindow = new MainWindow();
            mainWindow.CurrentPlayer = new PlayerData();
            Console.WriteLine("Generating character rom file");
            GenerateCharacterRom.Generate();
            Console.WriteLine("Generating sprites rom file");
            var playershot = new PlayerShot(null, null);
            var squigglyshot = new AlienShotSquigly(mainWindow, null);
            sprites.Add(("alien_moving_y",new SplashAlienAnimation().AlienMovingY));
            sprites.Add(("player_shot",playershot.ShotSprite));
            sprites.Add(("player_shot_exp",playershot.ShotExplodeSprite));
            sprites.Add(("alien_shot_plunger", new AlienShotPlunger(mainWindow, null).Shot));
            sprites.Add(("alien_shot_rolling", new AlienShotRolling(mainWindow, null).Shot));
            sprites.Add(("alien_shot_squigly", squigglyshot.Shot));
            sprites.Add(("alien_shot_explode", squigglyshot.ShotExplosion));
            GenerateSpritesRom.Generate(sprites);
            Console.WriteLine("Done");
        }
    }
}
