Download Test:
[Here
](https://drive.google.com/file/d/1Al6xFirNm6dVR6LTRHpXKrv7i-g9GqVC/view?usp=sharing)

**Instructions**: Grab the zip file with the project from the README in github

Open an instance of OnlineTest.exe. Start it as a host. It cannot participate.

Open up one or more instances of OnlineTest.exe, and start them as clients. Press start on the host screen when you have enough.

Click on the clients screen to place down coins. One any given client is down placing down coins click the ready button. When all clients have clicked the ready button, a player phase will start

Each player moves with the arrow keys and can collect coins. If two or more clients are in play, they will swap coin placements.

The host can click the reset button to bring you back to the lobby.
--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------


Unity Multiplayer

An online multiplayer game using Unity’s built in plugins and RPC in order to facilitate a connection between 2 computers. The design of this game is very simple. One player can place coins around an area using the mouse and another player can pick up those coins, moving around using the arrow keys.

Unity Packages used:

ParrelSync: This package is from a 3rd party, not affiliated with Unity, but allows me to clone my inspector and test my multiplayer game via a local host. 

Netcode for Game Objects:  Netcode for GameObjects is a high-level netcode SDK that provides networking capabilities to GameObject/MonoBehaviour workflows within Unity and sits on top of an underlying transport layer. I upgraded it with another 3rd Party package, Multiplayer Samples Utilities. 

Multiplayer Services: This package makes it very easy to add online multiplayer elements to your project via the addition of several scripts. I’ve listed them below.

Scripts Used:

Network Manager: This script is attached to a network manager object, it's how we start the host and client. The host and client can also be started from buttons which is necessary once you are no longer starting from the Unity Inspector. 

The script describes the Network topology and we can assign a player prefab to it that is automatically spawned once we start the network up.

My game uses two different player prefabs, so the network object spawns in one determined using a script that determines if it's a host or a client.

Network Object: The script is attached to any object I want the data to transfer between systems. These objects are added to a list I pass to my Network Manager. They also have ownership and I can use other scripts in the object to make sure that only the owner has the ability to move it.

Network Objects scripts need to have  network behavior instead of mono behavior or they will not communicate over a network. Likewise any non-movement action a object makes via a script needs to be an Remote Procedure Call.

Client Network Transform: Allows the players position, rotation and scale to be transferred between clients and host. I can disable any unnecessary units in order to save data.





