# Embers (Embeddable Ruby Scripts)
An embeddable Ruby interpreter written entirely in C#. The aim is to make Ruby usable in the Unity game engine or other C# applications.

Its minimalistic design should be suitable for use in game engines or modding scenarios.

## Advantages
- Easy to embed, sandbox, and control in your C# application or game.
- Source code is much easier to understand, with everything in one place.
- Fully compatible with Unity.
- Obsolete functionality, such as interpreting numbers starting with 0 as octal, is removed.

## Drawbacks
- Likely to be considerably less optimised than Ruby. Benchmarks show it is around 5x slower.
- Does not have 100% compatibility with Ruby syntax or functionality.