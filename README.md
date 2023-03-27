# Echoscape-Game
A Repo for my Console Game Development of Echoscape, for my UNI Module at Birmingham City University

## Info

The final produced game was intended to be a Rogue-Like with Procedural Generated Terrains that you fight enemies in with the target platform being a chosen Console, which was Xbox One for me. The implemented game had a stable build running fully on the Xbox One console, with the app being registered as a Game due to DirectX Requirements. The core focus for my individually was "AI Implementation", which covered NPC AI and the Procedural Generation aspect of the game.
With the core inspiration being (Risk of Rain 2)[https://store.steampowered.com/app/632360/Risk_of_Rain_2/].

## Gameplay

### Menus

The Player could select through the menus on the screen, with them all being stack based allowing for easy selection of each layer by calling one common function for first selection. Native controller support was achieved with console inputs being taken and working when navigating UI.

The main issue discovered but not fixed was the scaling with wide screen displays on the xbox, where the UI was small.

### Character

Once in the game spawn the player can take control of the main character through its controller.
The controller utilized dynamic animations using Unity's Animation Rigging Package, for aiming the characters current gun.
The firing and weapon switching was implemented with animations for both should be changing, with some slight bugginess with hand placement.

### Terrain Generation

The Main aspect of the game was the procedural generation of the Terrain.
The terrain generation utilises Unity's Job System, which is apart of Unity DOTS and farily recent to the projects start went stable, and ComputeShaders.

The ComputeShader is used for the generation of the 3D Texture of the planets surface, and was planned to be ported to the Jobs System.
Using Simplex noise for the generation process, and a provided dataset randomly selected from a few variants.

The Meshing algorithm takes the ComputeShaders output of a 3D Texture and produces a Mesh using Marching Cubes. Near all aspects of the process are achieved using Unity Jobs' Multithreaded Code, through one Main Job, and then further passes on the data. Later aspects are using Singlethreaded code but was planned to be ported to the job system with the initial 3D texture generation. While it is very fast to compute multiple aspects can improve performance, from GPU to RAM memory moving of the 3D Texture, to Hashmap Simplification of the Meshes' Verts and etc.

### NPCs

The NPCs are spawned in a certain radius above the player with only basic state machine behaviour of going towards the player and hoving around him at a certain distance. Their controlls work on a Rigidbody basis and works using spring forces, to drive the NPC to a certain "hover" height using raycasting and adding forces to achieve the desired height.
