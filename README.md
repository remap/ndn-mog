# How to run Confetti as a MOG

Feb 21, 2013

by Zening Qu (quzening@remap.ucla.edu)

Confetti is a mini multiplayer online game (MOG) using NDN. It is assumed that players will self-organize into a peer-to-peer network, that they will create some objects (confettis) in the game, and the game application is responsible for disseminating these objects to all players. In this way, players can navigate and observe objects created by themselves as well as those created by other players. In the screenshot below, two instances of Confetti are synchronized. Both players see confetties in the space, but from different perspectives.

![screenshots](http://quzening.com/wp-content/uploads/confetti%E5%89%AF%E6%9C%AC.png)

Confetti is developed for the NDN MOG project. For that purpose, the game application prints out NDN names of players and objects in its GUI.

## Platform

Confetti runs on Mac OS X (10.7 Lion and 10.8 Mountain Lion tested, earlier OSs should also work but there is no guarantee).

## Dependencies

1. Xcode
2. MacPorts
3. CCNx
4. Confetti

## Step-by-step Guide

Confetti needs CCNx to provide NDN support. In order to install CCNx, please refer to [this guide](http://irl.cs.ucla.edu/autoconf/client.html) or follow steps 1 through 3. If you already have CCNx, please jump to step 4 to run Confetti.

### Step 1 Install Xcode

Download a free copy of Xcode from the App Store and install it, if it is not already installed on your computer.

Make sure the **Xcode Command Line Tools** are installed also. Recent installations of Xcode does not include the command line tools. They can be installed from: Xcode -> Preferences -> Downloads. If these tools were not properly installed, MacPorts may not properly selfupdate as Step 2 claims.

### Step 2 Install MacPorts

MacPorts "pkg" installers available [here](http://www.macports.org/install.php). 


### Step 3 Install CCNx

CCNx can be installed via MacPorts. If your Macports are installed in /opt/local, add the following line at the end of /opt/local/etc/macports/sources.conf before the default port repository.

	rsync://ndn.iu4.ru/macports/

After this step, use the following command to fetch updated port definitions.

	sudo port selfupdate

Then use these commands to install and load CCNx.
	
	sudo port install ccnx
	sudo port load ccnx

### Step 4 Start CCNx

Use this command to start the ccnd deamon, which will forward packets for Confetti.

	sudo ccndstart
	
### Step 5 Start Repository

A repository functions as a local data cache for Confetti. Switch to the directory where you want this repository to be hosted, and use the folloiwng command to start the repository.

	ccnr

### Step 6 Connect With Other Players

#### A. on the same machine

Multiple players could be multiple instances of Confetti on the same machine (see picture below). In this case the following command can be used to start multiple instances.

	open -n $directory$/Confetti.app

In this case the multiple instances are shaing the same repository (the local repository on the shared machine). 
	
![image](https://www.evernote.com/shard/s287/sh/2ba3c787-3588-4a11-ac4c-3990d6c80fa4/2240fedc6706dd483d6adb5c30667f52/res/37346e19-73b7-4f6b-ab79-2031cb65be40/IMG_0498.jpg?resizeSmall&width=832)

#### B. on directly routable machines

More often than not players are on different machines. We can use the following command to set up FIB tables for players so that packets are directly routed among players.

	ccndc add 'uri' udp 'host'

![image](https://www.evernote.com/shard/s287/sh/2ba3c787-3588-4a11-ac4c-3990d6c80fa4/2240fedc6706dd483d6adb5c30667f52/res/2c925224-974f-4380-9cdc-b20db6066e79/IMG_0499.jpg?resizeSmall&width=832)

#### C. on indirectly routable machines

In other cases there is no direct routing path between a pair of players, such as in the following picture. Packets between this pair of players are routed through a common hub.

Note that in this senario, there is an extra requirement for the hub: CCNx SYNC has to be running on the hub. To be specific, the hub should also maintain a repository and there should be a slice in this repository defined using the following parameters:

	prefix = "ccnx:/ndn/ucla.edu/apps/Confetti";
	topo = "ccnx:/ndn/broadcast/Confetti";
	
Which is how the slices on all players' machines were defined. 

![image](https://www.evernote.com/shard/s287/sh/2ba3c787-3588-4a11-ac4c-3990d6c80fa4/2240fedc6706dd483d6adb5c30667f52/res/0f05f053-450b-44cc-9986-0c3490e0a8ab/IMG_0500.jpg?resizeSmall&width=832)

This requirement is further examplified in the extreme case below. The two machines running Confetti can be syncrhonized if and only if every machine on their routing path (including they themselves) are running CCNx SYNC. In the picture below, they are not synced because the chain breaks at HUB(3).

![image](https://www.evernote.com/shard/s287/sh/1e596495-5c87-4499-a291-4b30bd3a1faa/01468c945912c318d5d80247f2620b47/res/3c5c77b5-c0a5-47bf-8e07-4fb4b00d5646/IMG_0503.jpg?resizeSmall&width=832)

### Step 7 Play!
:D

