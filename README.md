# NNERotationMirrorCSVTest

Controls: wasd keys to walk around, mouse to look around, e or left mouse button to interact with objects, escape to quit.

Even though there was a newer version of Unity 2019.4 available, I chose 2019.4.11f1 because it's easier to upgrade to a newer version than it is to download a newer version of the engine.
There was a collider on the Panel, a child of the QRCode, which I assumed was there to be used for my raycast, which led me to looking at the parent of the objects i hit. 
The data objects didnt have any collider at all, so maybe I was wrong.
I wasn't sure whether the csv file needed to be fetched every time it was needed, or if it could be cashed, since the csv file could technically be changed at any point.
I comprimised by cashing it for 10 seconds, after which a new one was fetched from the server if needed.
I didn't know how long the csv data needed to be displayed, so I only displayed it as long as looking at the cube after interaction.