## Customizable Environment Binary Files 
This folder contains the following Unity3D environment builds: 
- [DroneDelivery](./DroneDelivery.md) - the agent is a drone trying to deliver a package.
- [SafeDroneDelivery](./SafeDroneDelivery.md) - a variation of the DroneDelivery environment that ensures
safety from collisions.
- [... more environments go here. also, we should update this before release so the environment and its
binary have the same name]

Each environment is configurable with a [`env_config.json`](./env_config.json) file. 

## Configuration file location
The config file must be placed in a different folder according to the architecture:
- MacOS. The config file must be in the same directory as the compiled Unity Scene. 
- Linux. The config file must be in the directory where the python script is executed.

You can find example python scripts running the Unity environments in [`../unity-simple-examples/`](../unity-simple-examples).
