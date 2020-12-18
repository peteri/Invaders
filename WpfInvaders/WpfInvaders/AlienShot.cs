using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class AlienShot : TimerObject
    {
        public AlienShot(MainWindow mainWindow, GameData gameData,byte[] sprite) : base(false,0)
        { 
        }

        public override void Action()
        {
            throw new NotImplementedException();
        }
    }
}
