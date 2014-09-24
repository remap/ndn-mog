# Readme - Matryoshka

Matryoshka is an NDN multiplayer online game. The game is tested with OS X >= 10.7 and nfd release-0.2.0.

## How to Run
1. Please put the config file and the app in the same folder.
2. nfd-start.
3. nfdc the hub you want to connect to, or the other player's nfd; for now the app uses /ndn/broadcast namespace(ideally the strategy for that face should be broadcast); and a customizable player namespace.
4. double click the app, for now, please run on '640 * 480' and 'fastest'.

## What to See
1. Your avatar created on one asteroid. 
2. If there are others within your area of interest, which is marked on the minimap on the top-right corner, you can discover them in your local instance.
3. You can see other players moving around.
4. You can move to different octants, see your area of interest change, discover new asteroids, and players that are in those octants.
5. For now, the world has ~20 pre-defined asteroids equally separated in 2 scenarios, and each asteroid hosts exactly the same NPCs. Flying two octants away from starting location and it's very likely nothing will come to pass for a long time.
6. A minimap on top-right corner, in which transparent white boxes represent this player's area of interest; series of numbers attached to each white box represent the octant indices; yellow dots represents players; large green dots represent asteroids; and one persistent white line directs to 'north'.

# Control Keys
1. arrows: go left/right, walk back/forth, fly up/down
2. 'a', 'd': go left/right
3. 'w', 's': walk back/forth, speed up/down when flying
4. 'f': switch to 1st person view
5. 't': switch to 3rd person view
6. mouse: look around
7. ctrl + num: teleport to location n (1 is default starting position, 2 and 3 are octants physically close to each other, but far away in octree)
  (if you find yourself lost in space, ctrl + 1 to return to starting position.)

## Configurable Parameters
1. name: The name of the local player instance. For now, the given name is used directly in ndn name: which means, special characters and having the same name and prefix for two players should be avoided manually. 
2. instance: A number which separates the game into several sub-worlds. The number is not used by current implementation for now.
3. logging-level: This is recommended to set to 'none' at all time, as appending a large amount of info to file in Unity takes time. For now, debugging is done with a headless version instead.
4. render-string: Built-in color of the local instance. Accepted color strings are blue, green, grey, orange, purple, red, yellow
5. hub-prefix: The name prefix of this player, used when testing with hub
6. update-interval: The milliseconds interval of position update. Default is 40ms. For now, all instances should have the same interval. (They don't necessarily need to be the same, but it will look really weird as info such as producing rate is not being communicated yet)

## Trouble Shooting: 
1. If the game launches with an unrecognized color, and no labels anywhere, please check if nfd is running.
2. If after some time, certain players stopped receiving updates, and other players rendered on those instances did not get destroyed (but those players on other instances get destroyed), please restart...and email, including network environment and current nfd-status for each localhost/hub, if possible.

## Problem Reporting:
Please email wangzhehao410305@gmail.com, and attach logs created in ~/Library/Logs/Unity/*.log if applicable.

## Known Issue
1. Player sometimes crashes through asteroids when flying too fast or trying to land on a lower height
2. Delay in discovery and update: observable, should always remain within 1 sec. Position interest expression rate is always 25Hz, but position interests could get timeouts.

## Todos
1. Evaluation of current status: average delay when running on a small number of peers? throughput? comparison vs IP?
2. Better strategy for position update
3. MemoryContentCache implementation for ndn-dot-net, which could improve the performance of position update a lot
4. Add more action to the game for testing

## Update Log

July 02, 2013
by Zening Qu (quzening@remap.ucla.edu)

Dec 30, 2013
Updated by Zhehao Wang (wangzhehao410305@gmail.com)

May 21, 2014
Updated by Zhehao Wang (wangzhehao410305@gmail.com)

Sept 09, 2014
Updated by Zhehao Wang (wanzhehao410305@gmail.com)
In preparation for ICN 2014 demo. Remote branch set to remap ndn-mog/master by default, refer to that repository for further updates.

Sept 19, 2014
Updated by Zhehao Wang (wanzhehao410305@gmail.com)
Added object coloring, tagging, minimap, increased position update frequency, modified position update mechanism.
Before ICN 2014 demo.
