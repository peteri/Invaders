# Invaders
Invaders for the STM8 and WPF using a racing the beam style of coding, loosely based on the following two projects (thanks to both). 

[Code archeology](http://computerarcheology.com/Arcade/SpaceInvaders/Code.html) - Original 8080 source code using Z80 Mnemonics

[Cycle accurate re-implementation in C](https://github.com/loadzero/si78c) - C based version of space invaders

The overall design is based on mapping the original arcade hardware of 256x224 raster screen to a 32x28 character map mostly based out of ROM with some user defined characters to allow a bitmap for the players bases as the missiles destroy them. There are a number of sprites for the bullets that get merged in at render time.

## WPF invaders
Prototype of Space Invaders in C# designed to allow minimal RAM usage and test out some design ideas for the STM8 version. Renders one line at time into a image then displays it, the plan is that the STM8 version clocks the line out over the SPI interface using DMA @ 8Mhz. Code is purely at the prototype level.

## STM8 invaders
To be written