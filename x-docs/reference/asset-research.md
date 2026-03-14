Technical Framework and Asset Sourcing for Interactive Physical Systems in Unity: Balloons, Ballistics, and Fluid Dynamics
The construction of a physics-integrated simulation in the Unity engine requires a deep synthesis of heterogeneous modules, ranging from soft-body mechanics to high-fidelity fluid dynamics. When the objective is the creation of a system involving balloons, projectiles like darts and ninja stars, and the dynamic dispersal of paint, the technical requirements shift from simple rigid-body interactions to complex vertex-level deformations and GPU-accelerated simulations. This report provides an exhaustive directory of assets, primitives, and codebases, primarily focused on free and commercially viable solutions, alongside technical guides for their assembly into a coherent gameplay loop.
Foundations of Asset Acquisition and Primitive Selection
The initial phase of development hinges on the acquisition of optimized geometric primitives. In a production environment where high performance and rapid iteration are paramount, sourcing models that adhere to industry-standard polygon counts and vertex density is essential. The following repositories offer a robust foundation for the requested objects.
3D Balloon and Projectile Repositories
Balloons and projectiles are categorized as high-frequency objects, necessitating low-poly primitives to maintain high frame rates during complex scenes. Repositories such as CGTrader and Meshy.ai provide highly specialized primitives with permissive licenses.

Object Class
Recommended Source
Licensing
Key Technical Features
Balloons (Generic)
CGTrader 1
CC0 / Royalty Free
PBR textures (2048x2048), verified low-poly (600-2300 polys).1
Balloons (Custom)
Meshy.ai 3
CC0 / Generated
AI-driven mesh generation in FBX, GLB, and OBJ formats.3
Darts
Free3D 4
Free / Mixed
Multiple variants including Nerf-style, rope darts, and throwing darts.4
Ninja Stars
Void1Gaming 5
MIT / Basic
Specifically optimized for precision workflows in Unity URP and HDRP.5
Character Stars
OpenGameArt 7
CC0 / Public Domain
Low-poly clay models including Batarangs, kunai, and shurikens.7

The use of CC0 (Creative Commons Zero) assets from creators like Quaternius and Kenney is highly recommended for commercial projects.8 Quaternius provides the Universal Animation Library, which includes over 120 animations compatible with Unity humanoid rigs, facilitating the integration of throwing mechanics for darts and ninja stars.10 Kenney’s assets, such as the Fantasy Town and Blaster kits, offer environmental context for balloon-based games without the burden of attribution.11
Modeling Requirements for Dynamic Systems
For assets intended for soft-body or fragmentation effects, the "Read/Write Enabled" flag must be checked in the Unity Import Settings.12 This allows scripts to access mesh data at runtime, a prerequisite for the jiggle physics and fragmentation systems discussed later. Models should ideally have non-overlapping UVs and support for multiple submeshes if internal materials are required for fragmented shards.5
Soft-Body Dynamics and Jiggle Mechanics for Balloons
Balloons are characterized by their non-rigid nature. Simulating the subtle "jiggling" and elastic response to movement requires specialized physics solvers that go beyond the standard Unity Rigidbody component.
High-Performance Jigglebone Solutions
The JigglePhysics codebase is a professional-grade solution for adding secondary motion to bones and meshes in Unity.14 This system utilizes the Unity Jobs system and the Burst compiler to perform relativistic squash-and-stretch calculations asynchronously, enabling the simulation of hundreds of simultaneous objects without impacting the main thread.14
Technical parameters for configuring balloon behavior within JigglePhysics include:
Stiffness: A normalized value (0 to 1) controlling the force returning the mesh to its target pose. For a standard helium balloon, a stiffness of 0.4 to 0.6 provides a believable oscillation.14
Damping: This parameter dictates how quickly the jiggling motion settles. Higher values simulate thicker latex or internal air friction.12
Air Drag: This simulates the atmospheric resistance encountered by a moving balloon. It causes the mesh to lag behind the root transform, which is essential for objects floating on a string.14
Angle Limit Soften: This prevents harsh snaps when the balloon reaches the limit of its rotation, providing a smoother, more organic feel.14
The installation of such a system is streamlined via the Unity Package Manager, allowing for seamless commercialization under the MIT license.14
Internal Pressure and Volume Conservation
A critical feature for "balloons filled with paint" is the simulation of internal pressure. The ClothBalloon variant in the UnitySimplePhysics library derives its simulation structure directly from a mesh topology.15 At runtime, it merged vertices and instantiates Rigidbodies connected by ConfigurableJoints. The key innovation is the continuous evaluation of the mesh volume during each physics step, which is used to apply outward pressure forces to the surface triangles.15 This allows the object to inflate and compress dynamically, mimicking the behavior of a latex skin containing fluid or gas.15
Ballistic Engineering: Darts and Projectile Systems
Projectiles in Unity must manage high-speed movement and reliable collision detection to ensure a satisfying player experience.
Trajectory Calculation and Visual Prediction
The implementation of dart-throwing mechanics requires a rigorous mathematical model. While Unity's built-in physics engine can handle trajectory, predicting the path for UI elements (like aiming lines) requires explicit calculation. The position  of a projectile at any time  follows the kinematic formula:

Where  represents initial velocity and  represents the acceleration due to gravity.16 Professional implementations use a LineRenderer to draw this arc, allowing players to visualize the flight path of their dart or ninja star before release.16

Trajectory Parameter
Formula / Logic
Technical Implementation
Initial Velocity Vector

Vector3 composition from launch angle and speed.16
Flight Time Estimation

Used to determine the duration of the path visualization.16
Hit Detection
Raycast / Sweep-cast
Prevents "tunneling" where projectiles pass through thin balloon colliders.18

Impact and Sticking Logic
For darts and ninja stars, the interaction does not end at collision. They must often "stick" into the target. The most effective codebase approach for this is parenting the projectile to a specific bone transform upon collision.20
The "Bone Following" technique avoids the visual artifacts seen in standard parenting:
On Impact: Use OnCollisionEnter to identify the target GameObject.
Bone Retrieval: Access the target's SkinnedMeshRenderer and find the nearest bone to the hit point (e.g., Spine_01).21
Offset Storage: Convert the hit point from world space to the local space of the bone.
Kinematic Handover: Set the dart's Rigidbody to isKinematic = true and disable further collisions.23
Scale Invariant Parenting: To prevent the dart from stretching if the target scales up or down, set the dart’s local scale as the ratio of its global scale to the target's lossy scale.20
Fragmentation and Rupture: The Popping Mechanism
The popping of a balloon is a discrete event that requires the transition from a single mesh to multiple fragments.
Mesh Fragmentation Architecture
The OpenFracture package provides an open-source, MIT-licensed framework for real-time mesh slicing and fracturing.13 When a dart collides with a balloon, this system can be triggered to split the mesh along randomized planes, creating 2D or 3D fragments.13
The algorithmic pipeline for a pop involves:
Recursive Slicing: The mesh is recursively divided until a target fragment count is reached, which for a balloon might be 5 to 10 larger shards.13
UV Remapping: The system preserves texture coordinates along the new edges, ensuring the latex texture remains consistent on the shards.13
Collision Impulse Transfer: Fragments can inherit the velocity of the original balloon, causing them to fly outward realistically.15
Particle-Driven Rupture Visuals
For stylized games, the Visual Effect (VFX) Graph is a superior tool to the standard Particle System, as it can simulate millions of simultaneous particles representing a fine mist of paint and shredded latex.25 Using Flipbook animations (sprite sheets of pre-rendered pops) provided by Unity under CC0 licenses allows for high-fidelity visual feedback with minimal CPU overhead.28
Computational Fluid Dynamics: Exploding Paint and Paint Fluid
The core requirement of "exploding paint" and "paint fluid" necessitates a move away from traditional geometry into the realm of particle-based fluid simulation.
Smoothed Particle Hydrodynamics (SPH) for Paint
To achieve realistic paint behavior where droplets merge and flow, the SPH method is considered the standard. Codebases like unity-fluid-simulation and the AJTech SPH implementation provide GPU-accelerated solvers capable of handling up to one million particles in real-time.29
The simulation follows a specific loop:
Density/Pressure Calculation: Particles calculate their density based on neighbors, informing the pressure force that pushes them apart.29
Force Calculation: Gravity, viscosity (essential for thick paint), and external forces (like the impact of a dart) are applied.29
Surface Extraction: Normal smoothing techniques are applied to make the particle swarm look like a continuous liquid surface rather than individual dots.30
2D Fluid Simulation via Compute Shaders
For games where the paint is applied to a surface (like a canvas), a grid-based compute shader approach is more efficient. The IRCSS Compute Shaders repository provides a simple 2D fluid setup where "dye" (paint) can be added to a grid.31
The "Persian Garden" demo within this repository shows how to map a 2D fluid simulation onto a 3D plane, creating a "fake" 3D paint effect that is highly optimized for mobile devices.31 Developers can trigger paint dispersal by calling the AddDye function at the impact coordinates of a balloon pop.31
Fluid Visuals via Metaballs
A lightweight alternative for "jiggling balloons filled with paint" is the use of screen-space metaballs. Individual physics-based particles are rendered as soft circles into a separate RenderTexture.33 A threshold shader then processes this texture:

Shader Node
Logic
Result
Sample Texture 2D
Reads the blurry particle texture
Input data
Step (Threshold)
Step(Alpha, Edge_Threshold)
Creates solid shapes from overlapping blurs.33
Color / HDR
Multiplies by a paint color
Final aesthetic.33

This technique allows the paint inside a "jiggling balloon" to visually shift and pool as the balloon moves, providing a high-quality visual at a fraction of the cost of a full CFD simulation.33
Dynamic Surface Interaction: Paint Splatter and Decals
When paint "explodes," it must persist on the environment meshes. This is handled through decal projection systems.
AirSticker: The Skinned Mesh Decal Solution
Traditional URP decals are limited when dealing with moving or deforming objects. The AirSticker decal system is designed specifically to address these limitations by generating a mesh at runtime that matches the shape of the receiver model.35
Advantages of the AirSticker system for paint splatters include:
Full Skin Animation Support: Splatters can be applied to moving characters or jiggling balloons and will deform correctly with them.35
Material Flexibility: Unlike standard URP decals that require specific shaders, AirSticker allows the use of regular materials, including lit and unlit variants.35
Performance: Once the decal mesh is generated (which takes a few frames), the rendering overhead is equivalent to drawing a simple static mesh.35
Comparative Decal Frameworks

System Name
Rendering Logic
License
Best Use Case
AirSticker
Runtime Mesh Gen
MIT
Dynamic splatters on moving objects.35
kDecals
View-space Projection
MIT
High-speed projection for bullet holes.36
Driven Decals
Editor Mesh Gen
MIT
Static environmental paint marks.37
Dynamic Decals
Projector-based
MIT
Legacy Built-in pipeline support.38
URP Decal Feature
Buffer overlay
Unity
Standard static splatters on floors/walls.39

To trigger a splatter on collision, the AirStickerProjector.CreateAndLaunch() method is utilized. The projector is instantiated at the hitPosition, oriented against the hitNormal, and the decal is added to a persistent queue.35
Assembly and Orchestration: The Unified Game Loop
The integration of these disparate components into a cohesive system requires a centralized manager that coordinates the physics and VFX triggers.
The "Paint-Filled Balloon" Interaction Pipeline
A typical event sequence for a paint-filled balloon being hit by a dart follows a rigorous logical flow:
Kinetic Interaction: The player initiates a throw. The ProjectileSimulator calculates the ballistic arc and updates the LineRenderer path.16
Detection: The dart's Rigidbody, set to Continuous Collision Detection, triggers an OnCollisionEnter event upon hitting the balloon's collider.42
Rupture: The balloon's ClothBalloon component calculates the volume release. Simultaneously, OpenFracture slices the primary mesh into shards, which inherit the outward momentum.13
Fluid Dispersal: The FluidSimulator kernel is called at the hitPosition. It injects "dye" particles into the simulation, which are then influenced by the explosion's force vector.31
Sticking: The dart's sticking script identifies the balloon’s parent bone, calculates the local offset, and deactivates physics to anchor the projectile.21
Persistence: As fluid particles hit the environment, the AirSticker system generates persistent splatter meshes on walls and floors, completing the visual cycle.35
Performance and Lifecycle Management
Maintaining performance during high-frequency interaction involves the use of object pooling and culling.
Object Pooling: Systems like RecyclerKit or kPooling should be used for all projectiles and decal projectors to minimize the cost of Instantiate and Destroy calls.36
LOD for Fluid: The FluXY 2.5D simulator allows for dynamic Level of Detail, where distant fluid simulations are performed at lower resolutions.46
VFX Graph Capacity: For massive paint explosions, the VFX Graph capacity should be tuned to the hardware target, with default samples supporting up to 8 million particles.47
Licensing and Commercialization: Strategic Considerations
For a commercial project, ensuring that every integrated component is legally cleared is a critical professional requirement.

License Type
Commercial Use
Source Code Requirement
Examples in this Report
MIT
Allowed
None
JigglePhysics, AirSticker, OpenFracture.13
CC0
Allowed
None
Quaternius models, Kenney assets.8
Apache-2.0
Allowed
Attribution required
Unity-Simple-Liquid, FLIP-Fluid.48
GPL-3.0
Allowed
Must release your source
Some script collections.50
Unlicense
Allowed
None
keijiro's SplatVFX, DofVfxSamples.47

Developers should favor MIT and CC0 assets to avoid the "copyleft" restrictions of GPL-3.0, which could necessitate the public release of the entire game's codebase.50 Most professional-grade Unity open-source tools, such as those from CyberAgent or naelstrof, are provided under the MIT license, specifically to encourage their use in commercial titles.14
Advanced VFX: Enhancing the Aesthetic of Exploding Paint
While physical accuracy provides the foundation, visual quality is driven by the shader and rendering pipeline configuration.
Universal Render Pipeline (URP) Features
In URP, specialized features like the Decal Renderer Feature must be added to the URP Renderer asset to support buffer-based splatters.39 For vertex-displaced objects (like jiggling balloons), enabling "Depth Priming" can improve performance on high-end hardware by reducing overdraw.39
Custom Shaders for Paint and Rubber
Specialized shaders can enhance the materiality of balloons and paint:
Gradient Shaders: A 3-color gradient shader can be used to simulate the lighting transition across a balloon’s surface.54
Sub-Surface Scattering (SSS): Lightweight SSS shaders are effective for creating the translucent look of thin latex.55
Paint Glow: Utilizing HDR colors in the LiquidShader allows paint to glow, making "exploding paint" more visually impactful when combined with Post-Processing Bloom.33
Particle Blend Frames
To ensure that the "popping" effect remains smooth at high speeds, frame blending should be enabled in the Particle System or VFX Graph.56 This is achieved by using "Anim Alpha Blended" shaders or changing the flipbook mode to "Blended" on particle surface shaders.56 This technique interpolates between two frames of an animation sprite sheet, eliminating the "stutter" often seen in high-speed explosions.56
Technical Synthesis of Requested Elements
By mapping the requested features to the most robust available frameworks, a comprehensive development path emerges.

User Requirement
Physical Primitive
Visual Interaction
Framework Source
Balloons
ClothBalloon 15
Gradient Shader 54
UnitySimplePhysics
Popping Balloons
OpenFracture 13
VFX Flipbook Burst 28
OpenFracture / VFX Graph
Jiggling Balloons
JigglePhysics 14
Vertex Displacement 57
naelstrof / FastUIJiggle
Exploding Paint
AddDye Grid 31
AirSticker Splat 35
IRCSS Fluid / AirSticker
Fill With Paint
Internal Pressure 15
Metaball Threshold 33
Simple Liquid / Code Monkey
Dart Objects
Low-poly FBX 58
LineRenderer Path 16
CGTrader / ProjectileSimulator
Ninja Stars
Shuriken FBX 5
Trail Renderer 59
Void1Gaming / SirhaianArts
Paint Fluid
SPH Solver 29
Surface Reconstruction 30
unity-fluid-simulation

Conclusion: Toward a Production-Ready Implementation
The Unity ecosystem provides a rich array of prebuilt assets and codebases that satisfy the requirements for a high-fidelity interactive game involving balloons and ballistics. The integration of the JigglePhysics library for secondary motion, OpenFracture for real-time destruction, and the AirSticker decal system for persistent splatters offers a commercially viable, MIT-licensed path for developers.
The primary technical hurdle remains the coordination of these systems. Successful implementation requires a multi-threaded approach, utilizing the Unity Jobs system for soft-body calculations and Compute Shaders for fluid simulations. By leveraging the low-poly CC0 primitives from Quaternius and Kenney, developers can maintain a consistent aesthetic while focusing their engineering efforts on the complex event-driven logic of balloon-projectile interactions. This modular architecture not only facilitates rapid prototyping but ensures that the final product remains performant, scalable, and legally sound for commercial distribution.
Works cited
CC0 - Balloon free VR / AR / low-poly 3D model - CGTrader, accessed March 13, 2026, https://www.cgtrader.com/free-3d-models/various/various-models/balloon-296917e1-d97a-4fd0-ae1f-c95fa3b3721c
Balloon free 3D model - CGTrader, accessed March 13, 2026, https://www.cgtrader.com/free-3d-models/various/various-models/balloon-4217797f-e765-4d30-9787-e74db00f0673
Balloon 3D Models for Free Download - Meshy AI, accessed March 13, 2026, https://www.meshy.ai/tags/balloon
Dart Free 3D Models download - Free3D, accessed March 13, 2026, https://free3d.com/3d-models/dart
Shuriken 3D Model - Free 3D Assets Collection - Cubebrush, accessed March 13, 2026, https://cubebrush.co/void1gaming/products/i5wfag/shuriken-3d-model-free-3d-assets-collection
Free 3D Assets Collection - VOiD1 Gaming, accessed March 13, 2026, https://www.void1gaming.com/free-3d-assets-collection
3D Shuriken Pack - OpenGameArt.org |, accessed March 13, 2026, https://opengameart.org/content/3d-shuriken-pack
Quaternius • Free Game Assets, accessed March 13, 2026, https://quaternius.com/
Home · Kenney, accessed March 13, 2026, https://kenney.nl/
Universal Animation Library by Quaternius - Itch.io, accessed March 13, 2026, https://quaternius.itch.io/universal-animation-library
Assets - Kenney, accessed March 13, 2026, https://kenney.nl/assets
roundyyy/Jelly-Mesh-System: The JellyMesh System is a Unity component that adds jelly-like physics to any mesh (soft body) - GitHub, accessed March 13, 2026, https://github.com/roundyyy/Jelly-Mesh-System
dgreenheck/OpenFracture: Open source mesh slicing/fracturing utility for Unity - GitHub, accessed March 13, 2026, https://github.com/dgreenheck/OpenFracture
naelstrof/JigglePhysics: A unity addon for adding stretchy bouncy physics to bones and meshes. - GitHub, accessed March 13, 2026, https://github.com/naelstrof/JigglePhysics
JohannHotzel/UnitySimplePhysics: Simple Physics provides lightweight, easy-to-use physics-based behaviors for Unity. (Rope, Cloth, SoftBodies) - GitHub, accessed March 13, 2026, https://github.com/JohannHotzel/UnitySimplePhysics
How I Built a Projectile Trajectory Predictor in Unity (and What I Learned Along the Way) | by Rohan Choudhary | Medium, accessed March 13, 2026, https://medium.com/@rohan5210work/how-i-built-a-projectile-trajectory-predictor-in-unity-and-what-i-learned-along-the-way-dea4b4801871
HongyuShen/Projectile-Curve-Visualizer-In-Unity - GitHub, accessed March 13, 2026, https://github.com/HongyuShen/Projectile-Curve-Visualizer-In-Unity
How do I make it So my bullets (projectiles) will destroy on impact with an object with the "Wall" tag : r/unity - Reddit, accessed March 13, 2026, https://www.reddit.com/r/unity/comments/1cnnzps/how_do_i_make_it_so_my_bullets_projectiles_will/
Unity FPS Tutorial | Smarter Projectiles & Fixing Bullet Tunneling - YouTube, accessed March 13, 2026, https://www.youtube.com/watch?v=nTfqhFXnzfI
Making a projectile (like an arrow), stick/stay in a target when it hits? : r/Unity3D - Reddit, accessed March 13, 2026, https://www.reddit.com/r/Unity3D/comments/1uega0/making_a_projectile_like_an_arrow_stickstay_in_a/
Made arrows 'stick' - Show - GameDev.tv, accessed March 13, 2026, https://community.gamedev.tv/t/made-arrows-stick/168531
Projectile Sticking - Spine Forum, accessed March 13, 2026, https://esotericsoftware.com/forum/d/5424-projectile-sticking
Dart throw mechanics with Unity : r/Unity3D - Reddit, accessed March 13, 2026, https://www.reddit.com/r/Unity3D/comments/ghtjgo/dart_throw_mechanics_with_unity/
I can't find a good projectile script/tutorial for Unity. Can I have help? - Reddit, accessed March 13, 2026, https://www.reddit.com/r/Unity3D/comments/m76j39/i_cant_find_a_good_projectile_scripttutorial_for/
Unity VFX Millions Of Particles - GitHub, accessed March 13, 2026, https://github.com/dilmerv/UnityVFXMillionsOfParticles
Experiment with VFX Graph - Unity Learn, accessed March 13, 2026, https://learn.unity.com/pathway/game-development/unit/visual-effects/tutorial/experiment-with-vfx-graph-1
Visual Effect Graph | 12.0.0 - Unity - Manual, accessed March 13, 2026, https://docs.unity3d.com/Packages/com.unity.visualeffectgraph@12.0/
Creating an Explosion effect using VFX graph in Unity - VionixStudio, accessed March 13, 2026, https://vionixstudio.com/2023/08/14/creating-an-explosion-effect-using-vfx-graph-in-unity/
Coding a Realtime Fluid Simulation in Unity [Pt. 1] - YouTube, accessed March 13, 2026, https://www.youtube.com/watch?v=zbBwKMRyavE
Real-time sph fluid simulation in unity. - GitHub, accessed March 13, 2026, https://github.com/aren227/unity-fluid-simulation
IRCSS/Compute-Shaders-Fluid-Dynamic-: Fluid Simulation ... - GitHub, accessed March 13, 2026, https://github.com/IRCSS/Compute-Shaders-Fluid-Dynamic-
Interactive Volumetric Fog With Fluid Dynamics and Arbitrary Boundaries, accessed March 13, 2026, https://shahriyarshahrabi.medium.com/interactive-volumetric-fog-with-fluid-dynamics-and-arbitrary-boundaries-f82fdee86397
Simple Liquid Simulation in Unity! (Text - Code Monkey), accessed March 13, 2026, https://unitycodemonkey.com/text.php?v=_8v4DRhHu2g
Simple Liquid Simulation in Unity! - YouTube, accessed March 13, 2026, https://www.youtube.com/watch?v=_8v4DRhHu2g
GitHub - CyberAgentGameEntertainment/AirSticker: Air Sticker is a decal system that addresses the limitations of URP decals and has a low impact on performance., accessed March 13, 2026, https://github.com/CyberAgentGameEntertainment/AirSticker
Kink3d/kDecals: Projection Decals for Unity's Universal Render Pipeline. - GitHub, accessed March 13, 2026, https://github.com/Kink3d/kDecals
Anatta336/driven-decals: A mesh-based PBR decal system for Unity's universal render pipeline. - GitHub, accessed March 13, 2026, https://github.com/Anatta336/driven-decals
EricFreeman/DynamicDecals: Decal solution for Unity's Built-In Render Pipeline - GitHub, accessed March 13, 2026, https://github.com/EricFreeman/DynamicDecals
Decal Renderer Feature | Universal RP | 12.0.0 - Unity 6.3 User Manual, accessed March 13, 2026, https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@12.0/manual/renderer-feature-decal.html
naelstrof/SkinnedMeshDecals: An example of rendering decals on SkinnedMesh Renderers in Unity. - GitHub, accessed March 13, 2026, https://github.com/naelstrof/SkinnedMeshDecals
Unity-URP-Cookbook/Assets/Scenes/Decals/AddDecal.cs at main - GitHub, accessed March 13, 2026, https://github.com/NikLever/Unity-URP-Cookbook/blob/main/Assets/Scenes/Decals/AddDecal.cs
Projectile Guns | Scriptable Object Gun Series 6 | Unity Tutorial - YouTube, accessed March 13, 2026, https://www.youtube.com/watch?v=LIB7uGDZou0
Physics in Unity: Detecting and Acting on a Collision. | by Michael Little | Medium, accessed March 13, 2026, https://medium.com/@little_michael101/physics-in-unity-detecting-and-acting-on-a-collision-68cdbc1342bc
Sticking Arrow to wall : r/Unity2D - Reddit, accessed March 13, 2026, https://www.reddit.com/r/Unity2D/comments/69rjeh/sticking_arrow_to_wall/
Unity Script Collection - michidk - GitHub Pages, accessed March 13, 2026, https://michidk.github.io/Unity-Script-Collection/
FluXY 2.5D Fluid Simulator For Unity Review - GameFromScratch.com, accessed March 13, 2026, https://gamefromscratch.com/fluxy-2-5d-fluid-simulator-for-unity-review/
keijiro/SplatVFX: 3D Gaussian Splatting with Unity VFX Graph - GitHub, accessed March 13, 2026, https://github.com/keijiro/SplatVFX
abecombe/FLIP-Fluid-for-Unity - GitHub, accessed March 13, 2026, https://github.com/abecombe/FLIP-Fluid-for-Unity
Macoron/Unity-Simple-Liquid - GitHub, accessed March 13, 2026, https://github.com/Macoron/Unity-Simple-Liquid
arjunm8/balloon-pop: An arcade style multi-platform balloon pop videogame built on the Unity Game Engine. - GitHub, accessed March 13, 2026, https://github.com/arjunm8/balloon-pop
UycGitHub/2-D-Balloon-Game: A simple android balloon game with unity and C - GitHub, accessed March 13, 2026, https://github.com/UycGitHub/2-D-Balloon-Game
keijiro/DofVfxSamples: DoF particle sample for Unity VFX Graph - GitHub, accessed March 13, 2026, https://github.com/keijiro/VfxBokeh
Introduction to URP for advanced creators (Unity 6 edition), accessed March 13, 2026, https://unity.com/resources/introduction-to-urp-advanced-creators-unity-6
Gradient_3Color.shader - gasbank/unity-balloon - GitHub, accessed March 13, 2026, https://github.com/gasbank/unity-balloon/blob/master/Assets/Shaders/Gradient_3Color.shader
Zoroiscrying/Unity_Shader_Library_Zoroiscrying: This is a shader library used for unity shader coding, pointing to different shader effects found from various sources. Several library topics may become public in the future. This project is mainly for personal study and lack the knowledge of code management and formal name formatting. - GitHub, accessed March 13, 2026, https://github.com/Zoroiscrying/Unity_Shader_Library_Zoroiscrying
Unity VFX Tutorial - Better/longer smoke, explosions and particle texture sheet animations with shader frame blending. : r/Unity3D - Reddit, accessed March 13, 2026, https://www.reddit.com/r/Unity3D/comments/7hokip/unity_vfx_tutorial_betterlonger_smoke_explosions/
Fast UI Jiggle shader for Unity - GitHub Gist, accessed March 13, 2026, https://gist.github.com/josephbk117/230a1a664105be3fd6e3cd0142aae3d9
Dart 3D Models – Free & Premium Downloads - CGTrader, accessed March 13, 2026, https://www.cgtrader.com/3d-models/dart
Unity VFX Tutorials - 03 - Basics (Projectile) - YouTube, accessed March 13, 2026, https://www.youtube.com/watch?v=pWxucHof_5A
