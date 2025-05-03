# Rubik'sCube

__Author__ :     Bastien Rappenecker, Basptiste Rimetz

__Due Date__ :   20th of December 2019

## Sub-folders
* Build.zip (Archived) 
    * Which contains the build for windows x86 and x64
* Rubik'sCube 
    * The Unity Project folder
# Project
## InGame
The __user interface__ is composed a following:
* A slider to choose the size of Rubik's cube you want to try to solve
* A button to create a new Rubik's cube
* A slider and an input field to choose the depth of the scrambling
* A button to start scrambling
* A button to reset the timer
* A button to start/resume the timer
* A button to exit the game

__Inputs:__

| Left columns  | Right columns |
| ------------- |:-------------:|
| Right Click      | Rotate the cube around itself     |
| Left Click      | Move a face of the cube     |
| Mouse Scroll      | Move the camera toward/away from the cube     |
| R      | To reset the timer     |
| Space      | To start/resume the timer     |

If the cube is solved while the timer is running, it will automatically stop and reward you with nice firework effects.

## Architecture

### Unity

The Unity project is architectured as so:
* Fonts
* Materials
* Prefabs
* Scenes
* Scripts
* Sprites

### Code

* The Rubik's cube has been created using the MVC design pattern:
    * RubiksCubeModel 				  being the model;
    * RubiksCube_RappeneckerBastienRimetzBaptiste being the controller;
    * RubiksCubeView 				  being the view;

All the values can be accessed and modified from the Unity Editor to make a better user experience.
