# Invaders
Invaders for the STM8 and WPF using a racing the beam style of coding, loosely based on the following. 

[Code archeology](http://computerarcheology.com/Arcade/SpaceInvaders/Code.html#18D4) - Original 8080 source code using Z80 Mnemonics

[Cycle accurate re-implementation in C](https://github.com/loadzero/si78c) - C based version of space invaders

The overall design is based on mapping the original arcade hardware of 256x224 raster screen to a 32x28 character map mostly based out of ROM with some user defined characters to allow a bitmap for the players base. There are a number of sprites for the bullets that get merged in at render time.

## WPF invaders
Prototype of Space Invaders in C# designed to allow minimal RAM usage and test out some design ideas for the STN8 version. Renders one line at time into a image then displays it.

## STM8 invaders
