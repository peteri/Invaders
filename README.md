# Invaders

Invaders for the STM8 and WPF using a racing the beam style of coding, loosely based on the following two projects (thanks to both).

[Code archeology](http://computerarcheology.com/Arcade/SpaceInvaders/Code.html) - Original 8080 source code using Z80 Mnemonics

[Cycle accurate re-implementation in C](https://github.com/loadzero/si78c) - C based version of space invaders

The overall design is based on mapping the original arcade hardware of 256x224 raster screen to a 32x28 character map mostly based out of ROM with some user defined characters to allow a bitmap for the players bases as the missiles destroy them. There are a number of sprites for the bullets that get merged in at render time.

## WPF invaders

Prototype of Space Invaders in C# designed to allow minimal RAM usage and test out some design ideas for the STM8 version. Renders one line at time into a image then displays it, the plan is that the STM8 version clocks the line out over the SPI interface using DMA @ 8Mhz. Code is a pretty raw port of the 8080 code so is not production quality and while there are some splits into classes the code shares a lot of state.

Differences from the arcade version

- Shield destruction by the aliens getting low is on an 8x8 character matrix so is sometimes a bit worse than the arcade game. Potentially fixable with a lot of code and hacking not worth it for the few frames the user can see.
- Explosion graphics when the users base is destroyed are a not a pixel perfect match, where the explosion of the aliens bullet overlaps the player explosion sprite. The two sprites exist together for a bit too long. After the explosions are removed the result is correct.
- Bug in the original when the bottom row invades where the invaders drawn are wrong type isn't duplicated.
- Timings of some of the splash screen stuff might be out by few frames.

## STM8 invaders

Partially completed. Works on a STM8L Discovery board with a classic resistor and diode circuit to get a video signal.

Uses a racing the beam strategy to draw the screen. Currently has the first two demo screens matching Mame and the WPF version.

STM8L Discovery board user button is a pause / single step button, short press pauses, long press un-pauses.

Note: the screen ripples a bit if you use the internal 16Mhz clock due to jitter. With an external clock the image is more stable, see commented out code in init_cpu (boardsetup.asm) to enable using external 16Mhz clocks (note code is for an oscillator not a crystal).

When my cheap 9" lcd monitor is plugged in there is mild interference banding (I suspect from the LCD backlight) it might also cause the ST SWIM debugger to hang up occasionally (unsure if this is a ground issue between my PC USB and the monitor video input). Need to try adding a buffer transistor as I don't see the banding with other boards (retro computing).

TODO list

- Timing is off after attract screen cycle compared to WPF version.
- Convert timing to NTSC from PAL. There are some bugs where the DMA cycle is slightly late which cause screen shifts when too many sprites are on the same line which I'm planning to investigate at the same time.
- Check fast single alien movement is correct (code is in place)
- Add code for sprite to sprite collision.
- Add code for coin handling and multiple players
- Add code for buttons for player.
- Add code for saucer.
- Get a decent video circuit. Current two diode, two resistor circuit seems to cause some sort of ground problem with the ST debugger.
- Document video circuit (re-investigate transistor version).
- Document video timing.
