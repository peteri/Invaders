using System;

namespace WpfInvaders
{
    internal class AlienShotSquigly : AlienShot
    {
        private static readonly byte[] SquiglyShotSprite = { 0x44, 0xaa, 0x10, 0x88, 0x54, 0x22, 0x10, 0xaa, 0x44, 0x22, 0x54, 0x88 };
        private static readonly byte[] shotColumns = { 0x0B, 0x01, 0x06, 0x03, 0x01, 0x01, 0x0B, 0x09, 0x02, 0x08, 0x02, 0x0B, 0x04, 0x07, 0x0A };
        public static readonly byte[] SaucerScores = { 0x10, 0x05, 0x05, 0x10, 0x15, 0x10, 0x10, 0x05, 0x30, 0x10, 0x10, 0x10, 0x05, 0x15, 0x10 };
        private static readonly string[] Saucers =
            { "\x20\x21\x22", "\x24\x25\x26", "\x23\x29\x2a", "\x23\x2d\x2e\x2f" };
        private static readonly string[] ExplodedSaucers =
            { "\xf0\xf1\xf2", "\xf4\xf5\xf6", "\xf8\xf9\xfa\xfb", "\xfc\xfd\xfe\xff" };

        private int currentShotColumn;
        private int saucerTimer;

        internal AlienShotSquigly(MainWindow mainWindow, GameData gameData) : base(mainWindow, gameData, SquiglyShotSprite)
        {
            ResetShotData();
            saucerTimer = 0x20;
        }

        internal override void Action()
        {
            if (gameData.ShotSync != 2)
                return;
            if (gameData.SaucerStart && (ShotStepCount == 0))
            {
                if ((!gameData.SaucerActive) && (mainWindow.CurrentPlayer.NumAliens >= 8))
                {
                    gameData.SaucerActive = true;
                    DrawSaucer(false, false, 0);
                }
                if (((gameData.SaucerX + 0x20) & 0x80) != gameData.VblankStatus)
                    return;
                if (gameData.SaucerHit)
                {
                    saucerTimer--;
                    switch (saucerTimer)
                    {
                        case 0x1f:
                            DrawSaucer(false, true, 0);
                            break;
                        case 0x18:
                            DrawSaucer(false, true, AddSaucerScore());
                            break;
                        case 0x00:
                            DrawSaucer(true, false, 0);
                            ResetSaucerData(); break;
                    }
                }
                else
                {
                    gameData.SaucerX += gameData.SaucerDelta;
                    if ((gameData.SaucerX < 0x08) || (gameData.SaucerX > 0xc0))
                    {
                        DrawSaucer(true, false, 0);
                        ResetSaucerData();
                    }
                    else
                    {
                        DrawSaucer(false, false, 0);
                    }
                }
                return;
            }
            bool resetRequired = HandleAlienShot(gameData.AlienShotPlunger, gameData.AlienShotRolling);
            if (resetRequired)
                ResetShotData();
        }

        private ushort AddSaucerScore()
        {
            gameData.AdjustScore = true;
            gameData.ScoreDelta = (ushort)(SaucerScores[gameData.SaucerScoreIndex] << 4);
            return gameData.ScoreDelta;
        }

        private void ResetSaucerData()
        {
            gameData.SaucerX = 0x08;
            gameData.SaucerDelta = 2;
            gameData.SaucerActive = false;
            gameData.SaucerHit = false;
            gameData.SaucerStart = false;
            saucerTimer = 0x20;
        }

        private void DrawSaucer(bool blank, bool explode, ushort score)
        {
            int image = (gameData.SaucerX & 0x07) >> 1;
            int currPos = 0x1a + ((gameData.SaucerX >> 3) * LineRender.ScreenWidth);
            string saucerData = "\x23\x23\x23\x23";
            if (!blank)
                saucerData = explode ? ExplodedSaucers[image] : Saucers[image];
            if (score != 0)
            {
                saucerData = score.ToString("X") + "\x23";
                if (saucerData.Length == 3)         // Score was 50 put a space in front
                    saucerData = "\x23" + saucerData;
            }
            if (currPos > LineRender.ScreenWidth)
                LineRender.Screen[currPos - LineRender.ScreenWidth] = 0x23;
            foreach (var c in saucerData)
            {
                LineRender.Screen[currPos] = (byte)c;
                currPos += LineRender.ScreenWidth;
            }
            if (currPos < LineRender.ScreenWidth * LineRender.ScreenHeight)
                LineRender.Screen[currPos] = 0x23;
        }

        protected override int ShotColumn()
        {
            int shotColumn = shotColumns[currentShotColumn];
            currentShotColumn++;
            if (currentShotColumn >= shotColumns.Length)
                currentShotColumn = 0;
            return shotColumn;
        }

        internal override void ResetShotData()
        {
            base.ResetShotData();
        }
    }
}

