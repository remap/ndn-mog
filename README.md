# Readme - Matryoshka


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

## How To Run
1. nfd-start. For information about nfd, refer to nfd github or redmine site. (The game currently uses nfd-release-0.2.0)
2. nfdc the hub you want to connect to; for now the app uses /ndn/broadcast namespace(ideally the strategy for that face should be broadcast); and a customizable player namespace.
3. double click the app

## What To See
1. Your avatar created on one asteroid. 
2. If there are others within your area of interest, which is described by octants (and marked by the white boxes on top-right corner), you can discover them in your local instance.
3. You can see them moving around.
4. You can move to different octants, discover new asteroids, and players that are in them.

# Control Keys
1. arrows: go left/right, walk back/forth, fly up/down
2. 'a', 'd': go left/right
3. 'w', 's': walk back/forth, speed up/down when flying
4. '1': switch to 1st person view
5. '3': switch to 3rd person view
6. mouse: look around
7. 'n': to view NDN names of {{nothing}, {player}, {player, other players}, {other players only}} (latter two are being developed)

## Known Issue
1. some NDN names keep floating on the screen before being deleted
2. doll sometimes crashes through asteroids when flying too fast
3. Delay in discovery and update: observable, should always remain within 1 sec.

## Todos
1. See todos and references in DiscoveryModule(https://github.com/zhehaowang/DiscoveryModule)
2. Game wise, interpolation and prediction for lowering the position update rate.
3. Asteroid data from certain host and NPC hosting.
