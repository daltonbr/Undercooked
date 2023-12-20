# Undercooked

_A fan-made vertical slice_

_**Youtube Video**_
[![Undercooked](https://img.youtube.com/vi/oFFEIDPF9XE/0.jpg)](https://www.youtube.com/watch?v=oFFEIDPF9XE)

_**Disclaimer:** Undercooked is a fan-made game, inspired by one of my favorite franchises: Overcooked, an amazing couch-coop game.
All assets were created from scratch by me. This project doesn’t have any commercial goals or any legal affiliation with the original game or company.
This project is meant only as a fun and challenging exercise of recreating a proven game without worrying about game design, focusing on programming and art creation, forcing myself out of “tutorial-hell”._

## Game concept

In Undercooked, we have to process ingredients (chop & cook), mount them into plates, and finally serve the orders as they pop out as fast as you can, thus earning more points.

More complexity arises from twists on the levels, like physically separating parts of the kitchen or adding obstacles, different recipes, and processes.
Still, for this project, I set a clear goal to keep the scope small by building all the core mechanics to have a tight game loop using just one level.

## Core mechanics

* Selection of `Interactables` based on a mix of _proximity_ and _orientation_.
* **Pick/Drop** items base on the context.
* Raw ingredients from their crates.
* Chop items.
* Cooking Pan, with burning and cooking timers.
* Delivering (and evaluating of) plates.
* Player control can be swapped between two independent avatars.
* **_Player movement and actions carefully recreated_** the feel are very of the original game.
* Trash
* Sink
* Orders Panel, with an animated UI

## Interesting points

* **Asynchronous programming** in conjunction with Coroutines.
* `IPickable` interface and `Interactable` Abstract class
* Extensive use of [Pattern Matching](https://docs.microsoft.com/en-us/dotnet/csharp/pattern-matching) to handle the interaction between items.
* Use of the new (event-based) Unity Input System, allowing a seamless change between the keyboard and different brands of controllers.
* Several Particle Systems: smoke, steam, dust, stars.
* Use of a personal Unity Package
* Some shaders in Shader Graph

## Tools

* **Unity** 2022.3.16f1 LTS
* Autodesk **Maya**, Adobe **Photoshop** and **Illustrator**.

## Auxiliary tools

* Cameras powered by [CineMachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/index.html)
* UI animation using [LeanGUI](http://carloswilkes.com/Documentation/LeanGUI) and [LeanTransition](http://carloswilkes.com/Documentation/LeanTransition)
* [FBX Exporter](https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/index.html) - makes it easy to send geometry and animation to any application that supports FBX and back again with minimal effort.
