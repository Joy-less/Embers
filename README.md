# Embers
### Embeddable Ruby Scripts
### 
An embeddable Ruby interpreter written entirely in C#.

Currently, Ruby has little interoperability with other languages due to its complex nature and modest userbase. The aim of this project is to allow Ruby to be embedded in C#, for use in the Unity game engine and other similar projects.

## Primary Objectives
#### Embeddability & Simplicity
It should be easy to sandbox code, provide a set of functions and classes, and control the interpreter from C#.
#### Readability
The source code should be easy to understand and make changes to.
#### Unity Compatibility
The interpreter should have full compatibility with Unity, meaning the dynamic keyword should not be used among other considerations.

## Secondary Objectives
#### Ruby Compatibility
While the interpreter should be able to run most Ruby code, 100% compatibility is not a priority.
#### Performance
The interpreter should run at a comparable speed & memory usage to Ruby, but is unlikely to be as optimised.
