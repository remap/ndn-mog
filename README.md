# Readme - Matryoshka


July 02, 2013
by Zening Qu (quzening@remap.ucla.edu)

Dec 30, 2013
Updated by Zhehao Wang (wangzhehao410305@gmail.com)

May 21, 2014
Updated by Zhehao Wang (wangzhehao410305@gmail.com)

Sept 09, 2014
Updated by Zhehao Wang (wanzhehao410305@gmail.com)
In preparation for ICN 2014 demo

## How To Run
1. nfd-start. For information about nfd, refer to nfd github or redmine site.
2. nfdc the hub you want to connect to; for now the app uses /ndn/broadcast namespace(ideally the strategy for that face should be broadcast); and /ndn/edu/ucla/remap namespace; will be allowing user to configure the latter namespace shortly
3. double click the app

## What To See
1. One asteroid created
2. Other player's matryoshkas and their position update if other instances are running.

## What Is New
1. Everything is new...Almost a complete redo of the mechanism.

# Control Keys
1. arrows: go left/right, walk back/forth, fly up/down
2. 'a', 'd': go left/right
3. 'w', 's': walk back/forth, speed up/down when flying
4. '1': switch to 1st person view
5. '3': switch to 3rd person view
6. mouse: look around
7. 'n': (to be supported) to view NDN names of {{nothing}, {player}, {player, asteroid in aura}, {player, asteroid in nimbus}}

## Known Issue
1. some NDN names keep floating on the screen before being deleted
2. doll sometimes crashes through asteroids when flying too fast
3. sometimes the same player gets discovered twice

## Todos
1. See todos and references in DiscoveryModule(https://github.com/zhehaowang/DiscoveryModule)
2. Game wise, interpolation and prediction would be cool
3. Asteroid data from lioncub should be redo as well.
