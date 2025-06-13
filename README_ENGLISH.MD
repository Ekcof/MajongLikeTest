# The Majong Like Test [ENGLISH]
[Readme in Russian](./README.md)

## Unity Version 2022.3.46f1

The game is a variant of Mahjong Solitaire. The player is presented with a board filled with rectangular tiles in multiple layers. The goal is to clear the board by matching pairs of identical tiles, provided that the selected tiles are not blocked on the right, left, or above.  
[Gameplay](./gameplay.gif)

## Features in This Project

### Level Editor
A mini level editor that lets you create custom maps for the board (ScriptableObject MapConfig). You can choose the number of layers, their width and length, and select which slots can hold tiles.  
[Level Editor Screenshot](./inspector.png)

### Tile Description
Each tile is a rectangle on the board with several states, some of which allow it to be selected. When two selectable tiles are chosen, they are removed from the board. If the two selected tiles do not match, a “cancel” animation plays.

## Tile Manager (TileManager)
The Tile Manager controls the board filling to ensure that at least one solution path exists.

## Fill Logic
First, the manager processes the current map from the config and finds all edge‐available slots where tiles can be placed. It then randomly places matching tile pairs into available slots. After placing tiles, they are removed from the processing list and the search for available slots repeats. This iteration continues until all slots are filled.

## Auto‐Play Manager
The auto‐play manager locks out player input and removes all available tiles in the same way a player would, guaranteeing level completion.  
[Auto‐solve Demo](./solve_level.gif)

This manager uses two logical approaches. It first follows a list of AutolevelStep objects recorded in TileManager during board filling, attempting to remove tiles in that order. If a listed tile pair is not currently selectable, a fallback logic kicks in: the manager searches for any selectable matching tile pair on the board. If the map settings require collecting more than two pairs of the same type, it’s possible for the player to reach an unsolvable state while still leaving some tiles selectable. In such cases—when the manager is started partway through the level—it may get stuck, requiring a level rebuild.

## Technology Stack
- **Zenject** – for dependency injection  
- **UniRX** – for reactive event handling  
- **UniTask** – for clear asynchronous operations  
- **DOTween** – for GameObject animations  

## Links
- [Itch.io Project Page](https://algiskhasanov.itch.io/majong-like)  
