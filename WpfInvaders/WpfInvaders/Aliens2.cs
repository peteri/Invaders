using System;

namespace WpfInvaders
{
    public class Aliens2
    {
        private readonly GameData gameData;
        private PlayerData currentPlayer;

        public Aliens2(GameData gameData, PlayerData currentPlayer)
        {
            this.gameData = gameData;
            this.currentPlayer = currentPlayer;
        }

        private void ExplodeAlienTimer()
        {
            gameData.AlienExplodeTimer--;
            EraseExplosion();
            gameData.PlayerShot.Status = PlayerShot.ShotStatus.AlienExploded;
            gameData.AlienExploding = false;
        }

        internal void EraseExplosion()
        {
            int alienPos = (gameData.AlienExplodeY + gameData.AlienExplodeX * LineRender.ScreenWidth);
            if (LineRender.Screen[alienPos] < 0x20)
            {
                // It was a bitmapped single alien so erase with spaces
                LineRender.Screen[alienPos] = 0x20;
                alienPos += LineRender.ScreenWidth;
                LineRender.Screen[alienPos] = 0x20;
                alienPos += LineRender.ScreenWidth;
                LineRender.Screen[alienPos] = 0x20;
            }
            else
            {
            }
        }

        internal void ExplodeAlien()
        {
            int alienPos = (gameData.AlienExplodeY + gameData.AlienExplodeX * LineRender.ScreenWidth);
            byte alienType = LineRender.Screen[alienPos + LineRender.ScreenWidth];
            if (alienType < 0x20)
            {
                // It's a solo alien so we're bitmapped
                int destOffs = 0xe0;
                int srcOffs = 0;
                int count = 0;
                switch (gameData.AlienExplodeXOffset)
                {
                    case 0: srcOffs = 0x21 * 8; count = 0x10; break;
                    case 2: srcOffs = 0x23 * 8; count = 0x10; break;
                    case 4: srcOffs = 0x25 * 8; count = 0x18; break;
                    case 5: srcOffs = 0x28 * 8; count = 0x18; break;
                }
                for (int i = 0; i < count; i++)
                    LineRender.BitmapChar[destOffs++] = CharacterRom.Characters[srcOffs++];
            }
            else
            {
                switch (alienType & 0x0f)
                {
                    // Shifted by 0 pixels
                    case 0x00:
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x21;
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x22;
                        break;
                    // Shifted by 0 pixels
                    case 0x01:
                        LineRender.Screen[alienPos] = 0x21;
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x22;
                        break;
                    // Shifted by two pixels
                    case 0x07:
                        // Alien to left of us is a shift by four alien?
                        if ((LineRender.Screen[alienPos] & 0xf) == 0x0a)
                            LineRender.Screen[alienPos] = (byte)((LineRender.Screen[alienPos] & 0xf0) | 0x0e);
                        else
                            LineRender.Screen[alienPos] = 0x23;
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x24;
                        break;
                    case 0x03:
                        int alienToLeft = LineRender.Screen[alienPos];
                        switch (alienToLeft)
                        {
                            case 0x62: LineRender.Screen[alienPos] = 0x25; break;
                            case 0x72: LineRender.Screen[alienPos] = 0x25; break;
                            case 0x82: LineRender.Screen[alienPos] = 0x25; break;

                            case 0x64: LineRender.Screen[alienPos] = 0x2c; break;
                            case 0x74: LineRender.Screen[alienPos] = 0x2e; break;
                            case 0x84: LineRender.Screen[alienPos] = 0x25; break;

                            case 0x6b: LineRender.Screen[alienPos] = 0x6f; break;
                            case 0x7b: LineRender.Screen[alienPos] = 0x7f; break;
                            case 0x9b: LineRender.Screen[alienPos] = 0x8f; break;
                        }
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x26;
                        alienPos += LineRender.ScreenWidth;
                        int alienToRight = LineRender.Screen[alienPos];
                        switch (alienToRight)
                        {
                            case 0x65: LineRender.Screen[alienPos] = 0x27; break;
                            case 0x75: LineRender.Screen[alienPos] = 0x27; break;
                            case 0x85: LineRender.Screen[alienPos] = 0x27; break;

                            case 0x64: LineRender.Screen[alienPos] = 0x2b; break;
                            case 0x74: LineRender.Screen[alienPos] = 0x2d; break;
                            case 0x84: LineRender.Screen[alienPos] = 0x20; break;

                            case 0x6a: LineRender.Screen[alienPos] = 0x93; break;
                            case 0x7a: LineRender.Screen[alienPos] = 0x94; break;
                            case 0x8a: LineRender.Screen[alienPos] = 0x95; break;
                        }
                        break;
                    case 0x08:
                        switch (LineRender.Screen[alienPos])
                        {
                            case 0x69: LineRender.Screen[alienPos] = 0x5b; break;
                            case 0x79: LineRender.Screen[alienPos] = 0x5c; break;
                            case 0x89: LineRender.Screen[alienPos] = 0x5d; break;
                            case 0x65: LineRender.Screen[alienPos] = 0x90; break;
                            case 0x75: LineRender.Screen[alienPos] = 0x91; break;
                            case 0x85: LineRender.Screen[alienPos] = 0x92; break;
                            default: LineRender.Screen[alienPos] = 0x28; break;
                        }
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x29;
                        alienPos += LineRender.ScreenWidth;
                        switch (LineRender.Screen[alienPos])
                        {
                            case 0x6b:
                                LineRender.Screen[alienPos] = 0x5e; break;
                            case 0x7b:
                                LineRender.Screen[alienPos] = 0x96; break;
                            case 0x8b:
                                LineRender.Screen[alienPos] = 0x97; break;
                            default:
                                LineRender.Screen[alienPos] = 0x2a; break;
                        }
                        break;
                    case 0x09:
                        LineRender.Screen[alienPos] = 0x29;
                        alienPos += LineRender.ScreenWidth;
                        LineRender.Screen[alienPos] = 0x2a;
                        break;
                }
            }
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
            gameData.AlienCharacterCurXOffset = gameData.RefAlienX & 0x7;
            gameData.AlienCharacterStart = 0x60 + ((alienRow >> 1) << 4);
        }

        public void DrawAlien()
        {
            if (gameData.AlienExploding)
            {
                ExplodeAlienTimer();
                return;
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
                    if (gameData.AlienCharacterCurXOffset != 0)
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
                    switch (gameData.AlienCharacterCurXOffset)
                    {
                        case 0: DrawAlienOffsetZero(currOffset); break;
                        case 2: DrawAlienOffsetTwo(currOffset); break;
                        case 4: DrawAlienOffsetFour(currOffset); break;
                        case 6: DrawAlienOffsetSix(currOffset); break;
                    }
                    gameData.SingleAlienOffset = ((gameData.AlienCharacterCurXOffset & 0x02) == 0) ? 12 : 0;
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
                if ((i >= gameData.AlienCharacterCurXOffset) && (i < (gameData.AlienCharacterCurXOffset + 16)))
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
            LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
            // Going right to left
            if (gameData.RackDirectionRightToLeft)
            {
                if (LineRender.Screen[currOffset + 64] == (byte)(gameData.AlienCharacterStart + 10))
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
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
            //if (gameData.RackDirectionRightToLeft)
            //{
            //    // Going Right to left
            //    if (LineRender.Screen[currOffset] == gameData.AlienCharacterStart + 5)
            //        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
            //    else
            //        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
            //    LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
            //    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
            //}
            //else
            //{
            //    // Going left to right... Alien on our left?
            //    if (LineRender.Screen[currOffset - 32] == gameData.AlienCharacterStart + 3)
            //        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
            //    else
            //        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 2);
            //    LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 3);
            //    // Going left to right... Another alien on our right?
            //    if (LineRender.Screen[currOffset + 64] == gameData.AlienCharacterStart + 6)
            //        LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 10);
            //    else
            //        LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 5);
            //}
        }

        /// <summary>
        /// Draws a type 2 alien with sprite shift right by six pixels.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawAlienOffsetSix(int currOffset)
        {
            //if (gameData.RackDirectionRightToLeft)
            //{
            //    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 7);
            //    LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
            //    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
            //}
            //else
            //{
            //    // Going Left to right... No partial alien to left of us make this cell blank
            //    if ((gameData.AlienCharacterCurX == 0) || (LineRender.Screen[currOffset - 32] != (gameData.AlienCharacterStart + 8)))
            //        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 9); 
            //    else
            //        LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 9);
            //    LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
            //    // Alien to the right of us?
            //    if (LineRender.Screen[currOffset + 64] == (gameData.AlienCharacterStart + 4))
            //        LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 11);
            //    else
            //        LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
            //}
        }

    }
}