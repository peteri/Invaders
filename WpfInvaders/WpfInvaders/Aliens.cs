using System;
using System.Collections.Generic;
using System.Text;

namespace WpfInvaders
{
    public class Aliens
    {
        private readonly GameData gameData;
        private PlayerData currentPlayer;

        public Aliens(GameData gameData, PlayerData currentPlayer)
        {
            this.gameData = gameData;
            this.currentPlayer = currentPlayer;
        }

        private void ExplodeAlienTimer()
        {
            throw new NotImplementedException();
        }

        private void KillPlayer()
        {
            throw new NotImplementedException();
        }

        private bool CheckPlayFieldLineIsBlank(int line)
        {
            var data = LineRender.RenderLine(line);
            for (int i = 4; i < 27; i++)
            {
                if (data[i] != 0) return false;
            }
            return true;
        }

        private void RackBump()
        {
            if (gameData.RackDirectionRightToLeft)
            {
                if (CheckPlayFieldLineIsBlank(9) == false)
                {
                    gameData.RefAlienDeltaX = (currentPlayer.NumAliens == 1) ? 3 : 2;
                    gameData.RefAlienDeltaY = -8;
                    gameData.RackDirectionRightToLeft = false;
                }
            }
            else
            {
                if (CheckPlayFieldLineIsBlank(213) == false)
                {
                    gameData.RefAlienDeltaX = -2;
                    gameData.RefAlienDeltaY = -8;
                    gameData.RackDirectionRightToLeft = true;
                }
            }
        }

        public void CursorNextAlien()
        {
            if (!gameData.PlayerOk)
                return;
            // Alien been drawn yet?
            if (gameData.WaitOnDraw)
                return;
            int timesThroughAliens = 0;
            do
            {
                gameData.AlienCurIndex++;
                if (gameData.AlienCurIndex == 55)
                {
                    RackBump();
                    gameData.AlienCurIndex = 0;
                    timesThroughAliens++;
                    gameData.RefAlienX += gameData.RefAlienDeltaX;
                    gameData.RefAlienY += gameData.RefAlienDeltaY;
                    gameData.RefAlienDeltaY = 0;
                    //StopIsr();
                }
            } while ((currentPlayer.Aliens[gameData.AlienCurIndex] == 0) && (timesThroughAliens < 2));
            if (timesThroughAliens >= 2)
            {
                // TODO: Should this flag something.
                return;
            }
            CalculateAlienPosition();
            if (gameData.AlienCharacterCurY == 4)
            {
                KillPlayer();
                return;
            }
            gameData.WaitOnDraw = true;
        }

        private void CalculateAlienPosition()
        {
            // Find the positon of our alien
            gameData.AlienCharacterCurY = gameData.RefAlienY >> 3;
            int curAlien = gameData.AlienCurIndex;
            int alienRow = 0;
            while (curAlien >= 11)
            {
                curAlien -= 11;
                gameData.AlienCharacterCurY += 2;
                alienRow++;
            }
            gameData.AlienCharacterCurX = (gameData.RefAlienX >> 3) + (curAlien << 1);
            gameData.AlienCharacterOffset = gameData.RefAlienX & 0x7;
            gameData.AlienCharacterStart = 0x60 + ((alienRow >> 1) << 4);
        }

        public void DrawAlien()
        {
            if (gameData.AlienExploding)
            {
                ExplodeAlienTimer();
            }

            if (currentPlayer.Aliens[gameData.AlienCurIndex] != 0)
            {
                int currOffset = gameData.AlienCharacterCurY + gameData.AlienCharacterCurX * LineRender.ScreenWidth;
                // Side effect of the original shift logic is that the row above the current invader is cleared
                // When an alien is drawn.
                LineRender.Screen[currOffset + 0x01] = 0x20;
                LineRender.Screen[currOffset + 0x21] = 0x20;
                // If we're advancing Right to left and we only have type C aliens left we go one more
                // step so we don't wipe out the row above us in the NE direction.
                if (gameData.RackDirectionRightToLeft)
                {
                    if (gameData.AlienCharacterOffset != 0)
                        LineRender.Screen[currOffset + 0x41] = 0x20;
                }
                else
                {
                    // If we're advancing left to right and we only have type C aliens left at the edges
                    // with a partial row of B aliens below i.e. the rack looks like this  
                    //     BCDCDCDCDCDCE   
                    //       bcdce bce 
                    //
                    // then as the row of type B aliens moves down it looks like this
                    //     BCDCDCDCDCDCE   
                    //         dce bce 
                    //       hi
                    if (LineRender.Screen[currOffset + 0x41] == gameData.AlienCharacterStart + 0x04)
                    {
                        // Turn the d into a b
                        LineRender.Screen[currOffset + 0x41] = (byte)(gameData.AlienCharacterStart + 0x02);
                    }
                    // then as the row of type B aliens moves down it looks like this
                    //     BCDCDCDCDCDCE   
                    //           e bce 
                    //       hihi
                    if (LineRender.Screen[currOffset + 0x41] == gameData.AlienCharacterStart + 0x05)
                    {
                        // Get rid of the e
                        LineRender.Screen[currOffset + 0x41] = 0x20;
                    }
                }
                if (currentPlayer.NumAliens == 1)
                {
                    DrawFastSingleAlien(currOffset);
                }
                else
                {
                    switch (gameData.AlienCharacterOffset)
                    {
                        case 0: DrawAlienOffsetZero(currOffset); break;
                        case 2: DrawAlienOffsetTwo(currOffset); break;
                        case 4: DrawAlienOffsetFour(currOffset); break;
                        case 6: DrawAlienOffsetSix(currOffset); break;
                    }
                    gameData.SingleAlienOffset = ((gameData.AlienCharacterOffset & 0x02) == 0) ? 12 : 0;
                }
            }
            gameData.WaitOnDraw = false;
        }

        /// <summary>
        /// Draws a single alien with any sprite shift.
        /// Toggles between drawing a type 1 or 2 alien depending
        /// the last alien drawn by the main 0,2,4,6 alien routines.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawFastSingleAlien(int currOffset)
        {
            if (currOffset >= 0x20)
            {
                LineRender.Screen[currOffset - 0x1f] = 0x20;
                LineRender.Screen[currOffset - 0x20] = 0x20;
            }
            LineRender.Screen[currOffset + 0x00] = 0x1c;
            LineRender.Screen[currOffset + 0x20] = 0x1d;
            LineRender.Screen[currOffset + 0x40] = 0x1e;
            if (currOffset + 0x60 < LineRender.ScreenWidth * LineRender.ScreenHeight)
            {
                LineRender.Screen[currOffset + 0x41] = 0x20;
                LineRender.Screen[currOffset + 0x61] = 0x20;
                LineRender.Screen[currOffset + 0x60] = 0x20;
            }
            gameData.SingleAlienOffset = 12 - gameData.SingleAlienOffset;
            int invaderType = gameData.AlienCharacterStart + gameData.SingleAlienOffset;
            invaderType = invaderType << 3;

            for (int i = 0; i < 24; i++)
            {
                if ((i >= gameData.AlienCharacterOffset) && (i < (gameData.AlienCharacterOffset + 16)))
                    LineRender.BitmapChar[0xe0 + i] = CharacterRom.Characters[invaderType++];
                else
                    LineRender.BitmapChar[0xe0 + i] = 0;
            }
            return;
        }

        /// <summary>
        /// Draws a type 1 alien with zero sprite shift.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawAlienOffsetZero(int currOffset)
        {
            LineRender.Screen[currOffset] = (byte)gameData.AlienCharacterStart;
            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 1);
        }

        /// <summary>
        /// Draws a type 2 alien with sprite shift right by two pixels.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawAlienOffsetTwo(int currOffset)
        {
            LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 6);
            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 7);
            // Going right to left
            if (gameData.RackDirectionRightToLeft)
            {
                if (LineRender.Screen[currOffset + 64] == (byte)(gameData.AlienCharacterStart + 4))
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 2);
                else
                    LineRender.Screen[currOffset + 64] = 0x20;
            }
        }

        /// <summary>
        /// Draws a type 1 alien with sprite shift right by four pixels.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawAlienOffsetFour(int currOffset)
        {
            if (gameData.RackDirectionRightToLeft)
            {
                // Going Right to left
                if (LineRender.Screen[currOffset] == gameData.AlienCharacterStart + 5)
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
                if (LineRender.Screen[currOffset + 64] != 0x20)
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
                else
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 4);
            }
            else
            {
                // Going left to right... Alien on our left?
                if (LineRender.Screen[currOffset - 32] == gameData.AlienCharacterStart + 3)
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
                // Going left to right... Another alien on our right?
                if (LineRender.Screen[currOffset + 64] == gameData.AlienCharacterStart + 6)
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 10);
                else
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
            }
        }

        /// <summary>
        /// Draws a type 2 alien with sprite shift right by six pixels.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawAlienOffsetSix(int currOffset)
        {
            if (gameData.RackDirectionRightToLeft)
            {
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
            }
            else
            {
                // Going Left to right... No partial alien to left of us make this cell blank
                if ((gameData.AlienCharacterCurX == 0) || (LineRender.Screen[currOffset - 32] != (gameData.AlienCharacterStart + 8)))
                    LineRender.Screen[currOffset] = 0x20;
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 9);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
                // Alien to the right of us?
                if (LineRender.Screen[currOffset + 64] == (gameData.AlienCharacterStart + 4))
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 11);
                else
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
            }
        }

    }
}
