# ColumnsALike

Simple columns-like game made with F# and monogame. The aim of the project was to better understand F#.

## Prerequisites

* [.NET core SDK](https://dotnet.microsoft.com/download) (v3.1)

## How to run

When you are in the source folder, call the command:
```shell
dotnet run
```

## Game Manual
The aim of the game is to control the falling column of jewels in order to get as many matches as possible.

#### Match
Match is a situation where horizontally, vertically or diagonally there are at least 3 identical jewels. 

#### How to control the column
You can change the horizontal position of the column on the board by pressing ← or → on your keyboard. When one of those keys is pressed, the column changes its position on the board by one unit to the left and right respectively. The **x** key on the keyboard is used to rotate the jewels in the column. Pressing ↓ on the keyboard will increase the drop speed of the column.

#### Game Over
Game over is when a column is blocked by the previous column as soon as it appears on the board. At the moment of the game over the board is cleared of gems and the score is reseted.

#### Preview of the next column
The preview of the next column is on the right hand side of the board and shows the next column that will start falling.

#### Levels
Once the appropriate number of points has been reached, the level counter will increment, with each level the drop speed of the columns will increase. The maximum level is 10.

#### Speaker button
By pressing the speaker icon you can turn the sound effects on or off.

#### Pause
To pause the game, press the space bar.


## Resources
All resources are in their original form and as .xnb files

Background image [licence CC0](https://creativecommons.org/publicdomain/zero/1.0/)\
author: SethByrd\
https://opengameart.org/content/starry-night-background

Gems image [licence CC-BY 3.0](https://creativecommons.org/licenses/by/3.0/)\
author: Ville Seppanen\
https://opengameart.org/content/gem-jewel-diamond-glass


Glass break sound effect [licence CC0](https://creativecommons.org/publicdomain/zero/1.0/)\
author: Till Behrend\
https://opengameart.org/content/glass-break

Speaker image [licence CC0](https://creativecommons.org/publicdomain/zero/1.0/)\
https://en.wikipedia.org/wiki/File:Speaker_Icon.svg

Speaker mute image [licence CC0](https://creativecommons.org/publicdomain/zero/1.0/)\
https://commons.wikimedia.org/wiki/File:Mute_Icon.svg

Orbitron font [SIL Open Font License v1.10](https://www.fontsquirrel.com/license/Orbitron)\
author: Matt McInerney
https://www.fontsquirrel.com/fonts/orbitron

## License
Source code licence: [MIT](https://choosealicense.com/licenses/mit/)