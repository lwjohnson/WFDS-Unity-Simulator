# WFDS-Unity-Simulator

### Explanation of files and directores
- CPSC470_Proposal.pdf is my proposal
- CPSC470_Final_Report.pdf is my final report
- Everything else is the Unity Project

### Running the code
- Unity version **2020.3.26f1** is required
- Import the project
- Select the sample scene
- Hit play button
- If on **keyboard and mouse**
  - **WASD** to move
  - **QE** to rotate
  - **Backspace** to remove Fire, Tree, Trench
  - **F** for Fire
  - **T** for Tree
  - **G** for Trench
  - **R** to call WFDS
- If on **Oculus**
  - Movement is using the **left joystick**
  - **Backspace** to remove Fire, Tree, Trench
  - **F** for Fire
  - **T** for Tree
  - **G** for Trench
  - **R** to call WFDS

### TODOs
- [x] Implement VR with continuous movement
- [x] Implement VR with teleportation
- [ ] Toggle between the two on the fly
- [x] Objects interactive with VR
- [x] Terrain generation from fds input file
- [x] Fire from WFDS in simulation
  - [x] Call WFDS
  - [x] Stop WFDS
  - [x] Write to input file
  - [x] Read lstoa file
    - [x] Convert to game coordinates (spits out 127 instead of 1270)
- [x] Add/Remove fires in game
- [x] Add/Remove trees in game
- [x] Add/Remove trenches in game
- [x] Add in game time (wallclock time)
- [x] Interactions before WFDS called
- [ ] Interactions after WFDS called
- [x] Add a readme that explains the contents of all the files and directories
- [x] Upload proposal
- [x] Upload final report


- [ ] Enable the user to be able to run the code, including instruction
  - [ ] for setting up any required third-party software to run the code
  - [ ] to compile and run the code
  - [ ] to create or access and set up data files
  - [ ] to read/interpret the results of the code
