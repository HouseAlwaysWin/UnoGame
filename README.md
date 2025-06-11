# UnoGame

This project contains a simple UNO implementation written for Godot 4 with C#.

## Multiplayer

A new `NetworkManager` singleton exposes basic hosting and joining of games. The main menu now includes **Host Game** and **Join Game** buttons along with a field to enter the server address.

1. Choose **Host Game** on one machine to create the server.
2. On another machine enter the host's IP address and choose **Join Game**.
3. The standard game scene will load and game actions (shuffling, dealing and playing cards) will replicate across peers via RPCs.

## Running

Open the project in Godot 4 and run the scene `uno_main_menu.tscn`. Use the menu buttons to start a local game or host/join a network session.
