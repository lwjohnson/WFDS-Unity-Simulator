# WFDS-Unity-Simulator

### Running the code
- Unity version **2020.3.26f1** is required
- Import the project
- WFDS is included in the StreamingAssets folder as well as an example input
- Select the sample scene
- Hit play button
  - Movement is using the **left joystick**
  - Look around using the **right joystick**
  - Right/Left **grip** to display placement markers
    + Right/Left **trigger** to place selected item (fire)
  - Left **menu** to start/pause the visualization

### TODOs
- [x] Implement VR with continuous movement
- [x] Objects interactive with VR
- [x] Terrain generation from wfds input file
- [x] Fire from WFDS in simulation
  - [x] Call WFDS
  - [x] Stop WFDS
  - [x] Write to input file
  - [x] Read lstoa file
    - [x] Convert to game coordinates (spits out 127 instead of 1270)
- [1/2] Add/Remove fires in game
- [ ] Add/Remove trees in game
- [ ] Add/Remove trenches in game
- [ ] Add ability to switch between object types
- [x] Add in game time (wallclock time)
- [1/2] Add text to let user know they can pause/resume
- [x] Interactions before WFDS called
- [x] Interactions after WFDS called
