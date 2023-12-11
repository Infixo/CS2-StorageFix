# Storage Fix
Temporary fix for negative rasources in cargo stations. Temporary meaning until CO will fix that properly.

## Description

- Cargo stations may show 0 resources despite being heavily stocked, or some small number that doesn't match the actual number of resources. It happens because the game creates negative resources in the storages. Another issue this situation creates is that the game tries to deliver those missing goods and trains are loaded with 1200t of a single resource. In the end, it breaks all resource logistics in the city.
- The root cause is that the game creates transfer requests that are impossible to execute e.g. 400t and there is only 100t in the storage. Then this request is processed, the train is loaded with 400t of goods and leaves -300t of negative resources.
- This mod fixes the loading/unloading process, so at least they don't create those situations.
- I have not yet tracked where the erroneous transfer request come from, so I've implemented a workaround - the mod simply removes such requests from the system. There will still be such requests (new ones) but they should not break the game.
- You may check yourself how the mod works when you enable the logging in the config file. It will tell you what is fixed and what is removed.
- You should observe the results each time a train comes to the station. After some time, the trains will start transporting reasonable amounts of various goods too.

## Technical

### Requirements and Compatibility
- Cities Skylines II version 1.0.15f1
- BepInEx 5
- Modifies TransportTrainAISystem and TransportBoardingHelpers

### Installation
1. Place the `StorageFix.dll` file in your BepInEx `Plugins` folder.
2. The config file is automatically created when the game is run once.

### Known Issues
- Nothing atm.

### Changelog
- v0.1.0 (2023-12-05)
  - Initial build.

### Support
- Please report bugs and issues on [GitHub](https://github.com/Infixo/CS2-StorageFix).
- You may also leave comments on [Discord](https://discord.com/channels/1169011184557637825/1183572853791141989).

## Disclaimers and Notes
> [!IMPORTANT]
- Please make sure you have a savefile before installing the mod.
