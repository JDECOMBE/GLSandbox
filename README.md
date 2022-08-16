# GLSandbox

This repository is used to try out stuff using OpenGL..

You can switch between projects in the Program.cs file. Each project is referred as a "Game".

## BatchRenderingGame

- Project entrypoint [here](./OpenTKTesting/Game/BatchRenderingGame.cs).
- Build Configuration: `BatchRendering`

Meant to try out batch rendering with impostors in distance, and real mesh for close up details. A small dithering effect has been implemented to fade between impostors and meshes. 

Not everything is optimized, code isn't clean, as I made this project in a 3-4h (didn't have time to polish it yet), this is just a PoC that I may improve in the future.

### Controls:
- W/Z: Move forward.
- S: Move backward.
- Left Click Drag: Orbit around origin.
- Left Shift: Increase view distance.
- Left Control: Decrease view distance.

### Screenshot:

![BatchRenderingGame image](./doc/BatchRenderingGame.png)

### Notes:

Even though the [tree model](./OpenTKTesting/Assets/tree2.gltf) looks low poly, each face has been subdivided several times in Blender to increase its rendering time. In total the mesh counts 16320 vertices and 32502 triangles (data from Blender's statistics).

![Mesh in Blender](./doc/BatchRenderingTreeModel.png)


## SceneRendererGame

- Project entrypoint [here](./OpenTKTesting/Game/SceneRendererGame.cs).
- Build Configuration: `SceneRenderer`

Meant to try out mesh rendering imported with Assimp. 

![SceneRenderer Image](./doc/SceneRenderingGame.png)

## RayTracingGame

- Project entrypoint [here](./OpenTKTesting/Game/RayTracingGame.cs).
- Build Configuration: `RayTracing`

Meant to discover a bit of what Compute Shaders can do. Direct implementation of [Ray Tracing in One Weekend](https://raytracing.github.io/).

![RayTracing Image](./doc/RayTracingGame.png)