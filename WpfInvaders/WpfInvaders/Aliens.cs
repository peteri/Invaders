namespace WpfInvaders
{
    internal class Aliens
    {
        private readonly MainWindow mainWindow;
        private readonly GameData gameData;
        private readonly PlayerData currentPlayer;
        internal static byte[] AlienStartRow = { 0x78, 0x60, 0x50, 0x48, 0x48, 0x48, 0x40, 0x40, 0x40 };

        internal Aliens(MainWindow mainWindow,GameData gameData, PlayerData currentPlayer)
        {
            this.mainWindow = mainWindow;
            this.gameData = gameData;
            this.currentPlayer = currentPlayer;
        }

        private void ExplodeAlienTimer()
        {
            gameData.AlienExplodeTimer--;
            if (gameData.AlienExplodeTimer != 0)
                return;
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
                LineRender.Screen[alienPos] = 0x23;
                alienPos += LineRender.ScreenWidth;
                LineRender.Screen[alienPos] = 0x23;
                alienPos += LineRender.ScreenWidth;
                LineRender.Screen[alienPos] = 0x23;
            }
            else
            {
                byte previousCharacter = LineRender.Screen[alienPos - LineRender.ScreenWidth];
                switch (previousCharacter & 0x0f)
                {
                    case 0x00:
                        break;
                    case 0x0e:
                    case 0x0f:
                        LineRender.Screen[alienPos - LineRender.ScreenWidth] -= 0x40;
                        LineRender.Screen[alienPos] = 0x23;
                        break;
                    case 0x08: LineRender.Screen[alienPos] = (byte)((previousCharacter & 0xf0) | 0x09); break;
                    case 0x05: LineRender.Screen[alienPos] = (byte)((previousCharacter & 0xf0) | 0x06); break;
                    default: LineRender.Screen[alienPos] = 0x23; break;
                }

                alienPos += LineRender.ScreenWidth;
                LineRender.Screen[alienPos] = 0x23;
                alienPos += LineRender.ScreenWidth;
                switch (LineRender.Screen[alienPos])
                {
                    case 0xca:
                    case 0xcc: LineRender.Screen[alienPos] = 0x84; break;
                    case 0xcb: LineRender.Screen[alienPos] = 0x82; break;
                    case 0xda:
                    case 0xdc: LineRender.Screen[alienPos] = 0x94; break;
                    case 0xdb: LineRender.Screen[alienPos] = 0x92; break;
                    case 0xea:
                    case 0xec: LineRender.Screen[alienPos] = 0xa4; break;
                    case 0xeb: LineRender.Screen[alienPos] = 0xa2; break;
                    case 0xb1:
                    case 0xb6:
                    case 0xb9: LineRender.Screen[alienPos] = 0x23; break;
                }
            }
        }

        internal void ExplodeAlien()
        {
            int alienPos = (gameData.AlienExplodeY + gameData.AlienExplodeX * LineRender.ScreenWidth);
            byte alienType = LineRender.Screen[alienPos];
            if (alienType < 0x20)
            {
                // It's a solo alien so we're bitmapped
                int destOffs = 0xe0;
                int srcOffs = 0;
                int count = 0;
                switch (gameData.AlienExplodeXOffset)
                {
                    case 0: srcOffs = 0xb0 * 8; count = 0x10; break;
                    case 2: srcOffs = 0xb2 * 8; count = 0x10; break;
                    case 4: srcOffs = 0xb4 * 8; count = 0x18; break;
                    case 6: srcOffs = 0xb7 * 8; count = 0x18; break;
                }
                for (int i = 0; i < count; i++)
                    LineRender.BitmapChar[destOffs++] = CharacterRom.Characters[srcOffs++];
            }
            else
            {
                if ((alienType < 0x80) || (alienType > 0xaf))
                    return;
                var subType = alienType & 0x0f;
                if ((subType != 9) && (subType != 1))
                {
                    if ((LineRender.Screen[alienPos] & 0x0f) < 0x0a)
                    {
                        if ((LineRender.Screen[alienPos] & 0x0f) == 0x08)
                        {
                            switch (LineRender.Screen[alienPos - 32] & 0x0f)
                            {
                                case 0x0f:
                                case 0x0e: LineRender.Screen[alienPos - 32] += 0x40; break;
                            }
                        }
                        LineRender.Screen[alienPos] |= (byte)0xb0;
                    }
                    else
                    {
                        if ((LineRender.Screen[alienPos + 32] & 0x0f) != 0x05)
                        {
                            switch (LineRender.Screen[alienPos])
                            {
                                // These three special cases get us a bit of character ROM back.
                                case 0x8b: LineRender.Screen[alienPos] = 0xc9; break;
                                case 0x9b: LineRender.Screen[alienPos] = 0xd9; break;
                                case 0xab: LineRender.Screen[alienPos] = 0xe9; break;
                                default: LineRender.Screen[alienPos] += (byte)0x40; break;
                            }
                        }
                        else
                        {
                            // Handle A or C to the left of a 5 (right is done by adding 0x40)
                            switch (LineRender.Screen[alienPos])
                            {
                                case 0x8a: LineRender.Screen[alienPos] = 0x27; break;
                                case 0x9a: LineRender.Screen[alienPos] = 0x28; break;
                                case 0xaa: LineRender.Screen[alienPos] = 0x2b; break;
                                case 0x8c: LineRender.Screen[alienPos] = 0x2c; break;
                                case 0x9c: LineRender.Screen[alienPos] = 0x54; break;
                                case 0xac: LineRender.Screen[alienPos] = 0x55; break;
                            }
                        }
                    }
                }

                // Middle byte
                alienPos += LineRender.ScreenWidth;
                if ((LineRender.Screen[alienPos] & 0x0f) < 0x0a)
                {
                    LineRender.Screen[alienPos] |= (byte)0xb0;
                }
                else
                {
                    switch (LineRender.Screen[alienPos] & 0x0f)
                    {
                        case 0x0e: LineRender.Screen[alienPos] = (byte)0xb9; break;
                        case 0x0f: LineRender.Screen[alienPos] = (byte)0xb1; break;
                        default: LineRender.Screen[alienPos] += (byte)0x40; break;
                    }
                }

                // Go home early? if we're a three don't care which shift we're return
                if ((LineRender.Screen[alienPos] & 0x0f) == 0x03) return;

                alienPos += LineRender.ScreenWidth;

                // If we're shifted by two or three do some fixups
                if (gameData.AlienExplodeXOffset < 3)
                {
                    switch (LineRender.Screen[alienPos] & 0x0f)
                    {
                        case 0x0a: LineRender.Screen[alienPos] += 0x40; break;
                        case 0x06: LineRender.Screen[alienPos] = 0xb6; break;
                    }
                }
                else
                {
                    if ((LineRender.Screen[alienPos] & 0x0f) < 0x0a)
                    {
                        LineRender.Screen[alienPos] |= (byte)0xb0;
                    }
                    else
                    {
                        switch (LineRender.Screen[alienPos] & 0x0f)
                        {
                            case 0x0e: LineRender.Screen[alienPos] = 0xb9; break;
                            case 0x0d: LineRender.Screen[alienPos] = 0xb6; break;
                            default: LineRender.Screen[alienPos] += 0x40; break;
                        }
                    }
                }
            }
        }

        private static bool CheckPlayFieldLineIsBlank(int line)
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

        internal void CursorNextAlien()
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
                gameData.Invaded = true;
                gameData.PlayerBase.Alive = PlayerBase.PlayerAlive.BlowUpOne;
                mainWindow.CurrentPlayer.ShipsRem=1;
                mainWindow.RemoveShip();
                mainWindow.DisplayShipCount();
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
            gameData.AlienCharacterStart = 0x80 + ((alienRow >> 1) << 4);
        }

        internal void DrawAlien()
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
                LineRender.Screen[currOffset + 0x01] = 0x23;
                LineRender.Screen[currOffset + 0x21] = 0x23;
                // If we're advancing Right to left and we only have type C aliens left we go one more
                // step so we don't wipe out the row above us in the NE direction.
                if (gameData.RackDirectionRightToLeft)
                {
                    if (gameData.AlienCharacterCurXOffset != 0)
                        LineRender.Screen[currOffset + 0x41] = 0x23;
                }
                else
                {
                    // If we're advancing left to right and we only have type C aliens left at the edges
                    // with a partial row of B aliens below i.e. the rack looks like this  
                    //     45a5a5a5a5a56   
                    //       45a56 456 
                    //
                    // then as the row of type B aliens moves down it looks like this
                    //     45a5a5a5a5a56   
                    //         a56 456 
                    //       78
                    if (LineRender.Screen[currOffset + 0x41] == gameData.AlienCharacterStart + 0x0a)
                    {
                        // Turn the a into a 4
                        LineRender.Screen[currOffset + 0x41] = (byte)(gameData.AlienCharacterStart + 0x04);
                    }
                    // then as the row of type B aliens moves down it looks like this
                    //     45a5a5a5a5a56      
                    //           6 456 
                    //       7878
                    if (LineRender.Screen[currOffset + 0x41] == gameData.AlienCharacterStart + 0x06)
                    {
                        // Get rid of the 6
                        LineRender.Screen[currOffset + 0x41] = 0x23;
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
                    gameData.SingleAlienIsTypeOne = (gameData.AlienCharacterCurXOffset & 0x02) == 0;
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
                LineRender.Screen[currOffset - 0x1f] = 0x23;
                LineRender.Screen[currOffset - 0x20] = 0x23;
            }
            LineRender.Screen[currOffset + 0x00] = 0x1c;
            LineRender.Screen[currOffset + 0x20] = 0x1d;
            LineRender.Screen[currOffset + 0x40] = 0x1e;
            if (currOffset + 0x60 < LineRender.ScreenWidth * LineRender.ScreenHeight)
            {
                LineRender.Screen[currOffset + 0x41] = 0x23;
                LineRender.Screen[currOffset + 0x61] = 0x23;
                LineRender.Screen[currOffset + 0x60] = 0x23;
            }
            gameData.SingleAlienIsTypeOne = !gameData.SingleAlienIsTypeOne;
            int invaderType = gameData.SingleAlienIsTypeOne ?
                gameData.AlienCharacterStart :
                (gameData.AlienCharacterStart >> 3) + 0xaa;
            invaderType <<= 3;

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
            // cleanup shift to left of us
            if (LineRender.Screen[currOffset - 32] == (byte)(gameData.AlienCharacterStart + 0x07))
                LineRender.Screen[currOffset - 32] = 0x23;
            if (LineRender.Screen[currOffset - 32] == (byte)(gameData.AlienCharacterStart + 0x0f))
                LineRender.Screen[currOffset - 32] = (byte)(gameData.AlienCharacterStart + 0x1);
            LineRender.Screen[currOffset] = (byte)gameData.AlienCharacterStart;
            if (LineRender.Screen[currOffset + 64] == (byte)(gameData.AlienCharacterStart + 0x08))
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 0x0f);
            else
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 1);
        }

        /// <summary>
        /// Draws a type 2 alien with sprite shift right by two pixels.
        /// </summary>
        /// <param name="currOffset">Offset on screen for the current alien.</param>
        private void DrawAlienOffsetTwo(int currOffset)
        {
            // Alien to left of us is type 1 shifted by four
            if (LineRender.Screen[currOffset - 32] == (byte)(gameData.AlienCharacterStart + 0x06))
                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0x0b);
            else
                LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0x02);
            LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 0x3);
            // Going right to left
            if (gameData.RackDirectionRightToLeft)
            {
                if (LineRender.Screen[currOffset + 64] == (byte)(gameData.AlienCharacterStart + 0x0a))
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 0x04);
                else
                    LineRender.Screen[currOffset + 64] = 0x23;
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
                if (LineRender.Screen[currOffset] == gameData.AlienCharacterStart + 0xd) 
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0xa);
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 5);
                if (LineRender.Screen[currOffset + 64] == (gameData.AlienCharacterStart + 0xe))
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 0xd);
                else
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 6);
            }
            else
            {
                // Going left to right... Alien on our left?
                if (LineRender.Screen[currOffset - 32] == gameData.AlienCharacterStart + 0x05)
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0x0a);
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 4);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 5);
                // Going left to right... Another alien on our right?
                if (LineRender.Screen[currOffset + 64] == gameData.AlienCharacterStart + 2)
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 0x0b);
                else
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 6);
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
                if ((LineRender.Screen[currOffset] == gameData.AlienCharacterStart + 9) ||
                    (LineRender.Screen[currOffset] == gameData.AlienCharacterStart + 0x0e))
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0x0e);
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 7);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
                LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 9);
            }
            else
            {
                // Going Left to right... No partial alien to left of us make this cell blank
                if ((gameData.AlienCharacterCurX == 0) || (LineRender.Screen[currOffset - 32] != (gameData.AlienCharacterStart + 8)))
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0x07);
                else
                    LineRender.Screen[currOffset] = (byte)(gameData.AlienCharacterStart + 0x0e);
                LineRender.Screen[currOffset + 32] = (byte)(gameData.AlienCharacterStart + 8);
                // Alien to the right of us?
                if (LineRender.Screen[currOffset + 64] == (gameData.AlienCharacterStart + 0x0a))
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 0x0c);
                else
                    LineRender.Screen[currOffset + 64] = (byte)(gameData.AlienCharacterStart + 0x09);
            }
        }

    }
}