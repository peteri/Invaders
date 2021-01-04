# Invaders

Invaders for the STM8 and WPF using a racing the beam style of coding, loosely based on the following two projects (thanks to both).

[Code archeology](http://computerarcheology.com/Arcade/SpaceInvaders/Code.html) - Original 8080 source code using Z80 Mnemonics

[Cycle accurate re-implementation in C](https://github.com/loadzero/si78c) - C based version of space invaders

The overall design is based on mapping the original arcade hardware of 256x224 raster screen to a 32x28 character map mostly based out of ROM with some user defined characters to allow a bitmap for the players bases as the missiles destroy them. There are a number of sprites for the bullets that get merged in at render time.

## WPF invaders

Prototype of Space Invaders in C# designed to allow minimal RAM usage and test out some design ideas for the STM8 version. Renders one line at time into a image then displays it, the plan is that the STM8 version clocks the line out over the SPI interface using DMA @ 8Mhz. Code is a pretty raw port of the 8080 code so is not production quality and while there are some splits into classes the code shares a lot of state.

Differences from the arcade version

- Shield destruction by the aliens is on an 8x8 character matrix so is sometimes a bit worse than the arcade game. Potentially fixable with a lot of code and hacking not worth it for the few frames the user can see.
- Explosion graphics when the users base is destroyed are a not a pixel perfect match, where the explosion of the aliens bullet overlaps the player explosion sprite. The two sprites exist together for a bit too long. After the explosions are removed the result is correct.
- Bug when the bottom row invades where the invaders drawn are wrong type.

## STM8 invaders

To be written.
