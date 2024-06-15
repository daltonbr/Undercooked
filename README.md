# Undercooked

A fan-made vertical slice.

[//]: # (styles: for-the-badge, flat-square, social)
[//]: # (https://shields.io/badges/static-badge)
<div align="center">
  <a href="https://www.linkedin.com/in/daltonlima/">
    <img src="https://img.shields.io/badge/linkedin-%230077B5.svg?&style=for-the-badge&logo=linkedin&logoColor=white"  alt="https://www.linkedin.com/in/daltonlima"/>
  </a>&nbsp;&nbsp;

  <a href="https://www.youtube.com/channel/UCTk4hR5N9e_Rmixdp4lEPNw?sub_confirmation=1">
    <img src="https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white"  alt="https://www.youtube.com/channel/UCTk4hR5N9e_Rmixdp4lEPNw?sub_confirmation=1"/>
  </a>&nbsp;&nbsp;

  <a href="https://x.com/daltonbr">
    <img src="https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white"  alt="https://x.com/daltonbr"/>
  </a>&nbsp;&nbsp;

  [![Undercooked](https://img.youtube.com/vi/oFFEIDPF9XE/0.jpg)](https://www.youtube.com/watch?v=oFFEIDPF9XE)
  <br>
  <a href="https://www.youtube.com/channel/UCTk4hR5N9e_Rmixdp4lEPNw?sub_confirmation=1">
  <img src="https://img.shields.io/youtube/channel/subscribers/UCTk4hR5N9e_Rmixdp4lEPNw?style=social"  alt="https://www.youtube.com/channel/UCTk4hR5N9e_Rmixdp4lEPNw?sub_confirmation=1"/>
  </a>&nbsp;&nbsp;

</div>

## Game concept

In Undercooked, we have to process ingredients (chop & cook), mount them into plates, and finally serve the orders as they pop out as fast as you can, thus earning more points.

More complexity arises from twists on the levels, like physically separating parts of the kitchen or adding obstacles, different recipes, and processes.
Still, for this project, I set a clear goal to keep the scope small by building all the core mechanics to have a tight game loop using just one level.

## Tools
[![Made with Unity](https://img.shields.io/badge/UNITY-2022.3.32f1-FFFFFF.svg?style=flat-square&logo=unity)](https://unity3d.com)

Started with
![AutodeskMaya](https://img.shields.io/badge/AutoDesk-Maya-37A5CC.svg?style=flat-square&logo=AutodeskMaya)
![AdobePhotoshop](https://img.shields.io/badge/Adobe-Photoshop-31A8FF.svg?style=flat-square&logo=AdobePhotoshop)
![AdobeIllustrator](https://img.shields.io/badge/Adobe-Illustrator-FF9A00.svg?style=flat-square&logo=AdobeIllustrator)

Switched to
[![Blender](https://img.shields.io/badge/Blender-🎨-E87D0D.svg?style=flat-square&logo=Blender)](https://www.blender.org/download/)
[![AffinityDesigner](https://img.shields.io/badge/Affinity-Designer-134881.svg?style=flat-square&logo=AffinityDesigner)](https://affinity.serif.com/)
[![AffinityPhoto](https://img.shields.io/badge/Affinity-Photo-4E3188.svg?style=flat-square&logo=AffinityPhoto)](https://affinity.serif.com/)

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

## Auxiliary tools

* Cameras powered by [CineMachine](https://docs.unity3d.com/Packages/com.unity.cinemachine@2.6/manual/index.html)
* UI animation using [LeanGUI](http://carloswilkes.com/Documentation/LeanGUI) and [LeanTransition](http://carloswilkes.com/Documentation/LeanTransition)
* [FBX Exporter](https://docs.unity3d.com/Packages/com.unity.formats.fbx@2.0/manual/index.html) - makes it easy to send geometry and animation to any application that supports FBX and back again with minimal effort.

_**Disclaimer:** Undercooked is a fan-made game, inspired by one of my favorite franchises: Overcooked, an amazing couch-coop game.
All assets were created from scratch by me. This project doesn’t have any commercial goals or any legal affiliation with the original game or company.
This project is meant only as a fun and challenging exercise of recreating a proven game without worrying about game design, focusing on programming and art creation, forcing myself out of “tutorial-hell”._
