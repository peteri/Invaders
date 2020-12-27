using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    internal class SplashAlienAnimation
    {
        private static readonly byte[] splashAlienSprite =
        {
            // Alien C1 & C2
            0x00, 0x00, 0x00, 0x00, 0x19, 0x3A, 0x6D, 0xFA, 0xFA, 0x6D, 0x3A, 0x19, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x1A, 0x3D, 0x68, 0xFC, 0xFC, 0x68, 0x3D, 0x1A, 0x00, 0x00, 0x00, 0x00,
            // Alien C1 & C2 pulling upside down Y
            0x00, 0x03, 0x04, 0x78, 0x14, 0x13, 0x08, 0x1A, 0x3D, 0x68, 0xFC, 0xFC, 0x68, 0x3D, 0x1A, 0x00,
            0x00, 0x00, 0x03, 0x04, 0x78, 0x14, 0x0B, 0x19, 0x3A, 0x6D, 0xFA, 0xFA, 0x6D, 0x3A, 0x19, 0x00,
            // Alien C1 & C2 pushing Y
            0x60, 0x10, 0x0F, 0x10, 0x60, 0x30, 0x18, 0x1A, 0x3D, 0x68, 0xFC, 0xFC, 0x68, 0x3D, 0x1A, 0x00,
            0x00, 0x60, 0x10, 0x0F, 0x10, 0x60, 0x38, 0x19, 0x3A, 0x6D, 0xFA, 0xFA, 0x6D, 0x3A, 0x19, 0x00
        };

        internal readonly Sprite AlienMovingY;
        private int targetX;
        private int deltaX;
        private int animateCount;
        private int image;

        internal void Init(int y, int startX, int targetX, int image)
        {
            AlienMovingY.X = startX;
            AlienMovingY.Y = y;
            AlienMovingY.Image = image;
            this.image = image;
            this.targetX = targetX;
            deltaX = (targetX < startX) ? -1 : 1;
            animateCount = 4;
            AlienMovingY.Visible = true;
        }

        internal SplashAlienAnimation()
        {
            AlienMovingY = new Sprite(splashAlienSprite, 6);
            LineRender.Sprites.Add(AlienMovingY);
        }

        internal MainWindow.SplashMinorState Animate()
        {
            animateCount--;
            if (animateCount==0)
            {
                animateCount = 4;
                if (AlienMovingY.Image == image)
                    AlienMovingY.Image = image + 1;
                else
                    AlienMovingY.Image = image;
            }
            AlienMovingY.X+=deltaX;
            return (AlienMovingY.X == targetX) ? 
                MainWindow.SplashMinorState.Idle : 
                MainWindow.SplashMinorState.AnimateYAlien;
        }
    }
}
