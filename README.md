# live-line
Evacuation path finder Unity solution for BIM models

<p style="background-image: url('https://github.com/ibober/live-line/blob/development/Assets/Resources/Materials/EvacuationDirection.png?raw=true'); background-repeat: repeat; height: 8px;">
  &nbsp;
</p>

## Unity project description
Project has one scene called **Demo**.\
There are **rac_advanced_sample_project**, **Origin** and **Destination** nodes in the hierarchy.

**rac_advanced_sample_project** node is BIM model imported from Revit through glTF.
It has `SampleNavigationSite` script assigned.\
The script finds floors and obstacles and bakes Unity's `NavMesh`. It also triggers `OnAnalysed` event so other GameObjects could start navigating.

**Origin** capsule in the scene has `PathFinder` script assigned.\
This MonoBehavior draws the shortest path to the **Destination** sphere by creating LineRenderer component.





## To run the demo
Both `SampleNavigationSite` and `PathFinder` components can run in Editor and Play modes.\
You can hit Play to see the path from Origin to the Destination, or use UI buttons on components to trigger site analysis and redraw the path.\
<u>Navigation instructions</u> can be found in the <u>Console</u> once the path is updated.\
Reposition Destination and then Origin to try different routes, and examine settings tooltips on both scripts to adjust behaviours. Exposed settings are minimalistic by design but more options can be found in the source code.



## To adopt to your scene
You need to use `SimpleNavigationSite` script and manually fill Floors and Obstacles collections, OR inherit from `NavigationSite` abstract class to create your custom tailored Site Analyser.\
Once you enable `PathFinder` script on any GameObject (e.g. AR camera) and assign `NavigationSite` you can call `NavigationSite.Analyze` method to draw the path. Alternatively call `DrawPath` method of `PathFinder` if navigation site has been already analized.

`PathFinder` script has `Instructions` property which holds navigation information.\
`NavigationInstructions` is `IEnumerable` which can also return a concrete string hint for actual span of a route by closest position or traveled distance. It's up to you how to use it.