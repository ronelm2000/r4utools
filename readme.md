R4UTools (Rebirth For You Tools)
===========
[![.NET](https://github.com/ronelm2000/r4utools/workflows/.NET/badge.svg)](https://github.com/ronelm2000/r4utools/actions/workflows/nightly.yml)
[![CodeFactor](https://www.codefactor.io/repository/github/ronelm2000/r4utools/badge)](https://www.codefactor.io/repository/github/ronelm2000/)
[![Gitter](https://badges.gitter.im/wsmtools/community.svg)](https://gitter.im/wsmtools/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
[![Downloads](https://img.shields.io/github/downloads/ronelm2000/r4utools/total.svg)](https://tooomm.github.io/github-release-stats/?username=ronelm2000&repository=r4utools)

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
* Local Deck JSON (`.r4udek` format)

#### Supported Deck Parsers ####
* [Bushiroad DeckLog](https://decklog.bushiroad.com/)
* Local Deck JSON (`.r4udek` format)

#### Supported Set Parsers ####
* Internal Set JSON (`.r4uset` format)
* Tab Delimited Text (for Partners only)
* [Rebirth For You Fandom Wiki](https://rebirth-for-you.fandom.com/wiki/Rebirth_for_you_Wiki)
* [Rebirth For You Renegades (Set List Only)](https://rebirthforyourenegades.wordpress.com/category/set-lists/)

#### Supported Set Exporters ####
* Internal Set JSON (`.r4uset` format)

### Build ###
Requirements to build are:
* Visual Studio 2019 (or greater)
* [Avalonia for Visual Studio](https://marketplace.visualstudio.com/items?itemName=AvaloniaTeam.AvaloniaforVisualStudio)

-----
©BanG Dream! Project ©Craft Egg Inc. ©bushiroad All Rights Reserved.　©異世界かるてっと／ＫＡＤＯＫＡＷＡ　©上海アリス幻樂団　©Project Revue Starlight © 2023 Ateam Entertainment Inc. ©Tokyo Broadcasting System Television, Inc. ©bushiroad All Rights Reserved. ©Koi・芳文社／ご注文はBLOOM製作委員会ですか？　© 2016 COVER Corp.　© 2017 Manjuu Co.,Ltd. & YongShi Co.,Ltd. All Rights Reserved. © 2017 Yostar, Inc. All Rights Reserved.　© Donuts Co. Ltd. All rights reserved.　©bushiroad All Rights Reserved.　illust：西あすか　illust: やちぇ(D4DJ)　©円谷プロ ©2018 TRIGGER・雨宮哲／「GRIDMAN」製作委員会　©長月達平・株式会社KADOKAWA刊／Re:ゼロから始める異世界生活2製作委員会　©2020竜騎士07／ひぐらしのなく頃に製作委員会　© New Japan Pro-Wrestling Co.,Ltd. All right reserved.　TM & © TOHO CO., LTD.　©円谷プロ ©2021 TRIGGER・雨宮哲／「DYNAZENON」製作委員会　© NEXON Games & Yostar ©木緒なち・KADOKAWA／ぼくたちのリメイク製作委員会 ©2016 暁なつめ・三嶋くろね／ＫＡＤＯＫＡＷＡ／このすば製作委員会 ©World Wonder Ring STARDOM © VISUAL ARTS/Key/KAGINADO ©あfろ・芳文社／野外活動委員会 ©C4 Connect Inc. ©てっぺんグランプリ実行委員会　©Spider Lily／アニプレックス・ABCアニメーション・BS11 ©福本伸行／講談社 ®KODANSHA　©TYPE-MOON / FGC PROJECT　©逢沢大介・KADOKAWA刊／シャドウガーデン　©柴・伏瀬・講談社／転スラ日記製作委員会 ®KODANSHA　©2023 暁なつめ・三嶋くろね／KADOKAWA／このすば爆焔製作委員会　©Bandai Namco Entertainment Inc. / PROJECT U149 ©Bandai Namco Entertainment Inc. ©硬梨菜・不二涼介・講談社／「シャングリラ・フロンティア」製作委員会・MBS ©中村力斗・野澤ゆき子／集英社・君のことが大大大大大好きな製作委員会