WSMTools (Weiss Schwarz Montage Tools)
===========
![.NET Core](https://github.com/ronelm2000/r4utools/workflows/.NET%20Core/badge.svg)
[![CodeFactor](https://www.codefactor.io/repository/github/ronelm2000/r4utools/badge)](https://www.codefactor.io/repository/github/ronelm2000/)
[![Gitter](https://badges.gitter.im/wsmtools/community.svg)](https://gitter.im/wsmtools/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
[![Downloads](https://img.shields.io/github/downloads/ronelm2000/r4utools/total.svg)](https://tooomm.github.io/github-release-stats/?username=ronelm2000&repository=r4utools)

Weiss Schwarz ©bushiroad All Rights Reserved.

This a CLI (Command Line Interface) tool + GUI deck builder intended to parse through, process, and export R4U cards and card set d; specifically, this tool's intention is to make querying, parsing, and exporting cards from various sources easier, as
well as provide APIs to expand on that functionality.

For now this tool is barebones (in its alpha stage, so expect some bugs), but I'm hoping it will be able to perform the following:
* Parse cards into a local database for querying.
* Parse decks using the local database.
* Export decks for use into another format.
* Export the entire local database (or part of it) into another format.

However, unlike a certain wsmtools, also included in this project is a multiplatform GUI called deckbuilder4u, which is a local deck builder. This GUI functionality is entirely dependent on the features of its CLI, but also allow you to create Local Deck JSONs by interacting with its GUI and selecting cards from a locally sourced card database.

Somewhat stable releases are on the [appropriate link](https://github.com/ronelm2000/r4utools/releases), but if you're having some issues with them, you can also try the [latest build](https://github.com/ronelm2000/r4utools/actions) by
registering on GitHub. 

#### Supported Deck Exporters ####
* [Tabletop Simulator](https://steamcommunity.com/sharedfiles/filedetails/?id=2173923861)
* Local Deck JSON

#### Supported Deck Parsers ####
* Local Deck JSON

#### Supported Card Set Parsers ####
* None for now.

### Build ###
Requirements to build are:
* Visual Studio 2019 (or greater)
* [Avalonia for Visual Studio](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio)