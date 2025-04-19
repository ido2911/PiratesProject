# Pirate Treasure Hunt Simulator 🏴‍☠️

**Status:** In Development 🚧  
**Technology:** C# (.NET Framework 4.8) 

## Project Overview

This project simulates a group of pirate ships searching for treasure across a network of islands. Each ship starts at a different location with limited resources (e.g., food), and must find a path that maximizes the expected amount of treasure collected, while avoiding high-risk pirate areas and managing resource constraints.

The system currently uses Dijkstra’s algorithm to determine the most efficient paths, and includes graph partitioning to assign each ship a unique region to explore. Future plans include integrating the PERT algorithm to better handle uncertainty.

## Current Features

- Graph-based map of islands and their connections.
- Simulation of ship movement and pathfinding.
- Resource-aware treasure collection with risk evaluation.
- Automatic graph partitioning based on ship locations.

## How to Run

1. Open the solution in Visual Studio.
2. Make sure the project targets `.NET Framework 4.8`.
3. Press **F5** to build and run the project.

## Future Plans

- Add a more advanced visual simulation engine.
- Allow user interaction (e.g., manual route selection).
- Support import/export of map data using JSON files.

## Notes

This project is still under development and not fully complete. Contributions, suggestions, or feedback are welcome!
