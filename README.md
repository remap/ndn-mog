# How to run Confetti as a MOG

Feb 09, 2013

by Zening Qu (quzening@gmail.com)

Confetti is a mini multiplayer online game using NDN. It is assumed that players will self-organize into a peer-to-peer network, that they will create some objects (confettis) in the game, and the game application is responsible for disseminating these objects to all players. In this way, players can navigate and observe objects created by themselves as well as those created by other players. In the screenshot below, two instances of Confetti are synchronized. Both players see confetties in the space, but from different perspectives.

![screenshots](http://quzening.com/wp-content/uploads/confetti%E5%89%AF%E6%9C%AC.png)

Confetti is developed for the NDN MOG project. For that purpose, the game application prints out NDN names of players and objects in its GUI.

## Platform

Confetti runs on Mac OS X (10.7 Lion and 10.8 Mountain Lion tested, earlier OSs should also work but there is no guarantee).

## Dependencies

1. Xcode
2. MacPorts
3. CCNx
4. Confetti

## Install

Confetti needs CCNx to provide NDN support. In order to install CCNx, please refer to [this guide](http://irl.cs.ucla.edu/autoconf/client.html) or follow steps 1 through 3. If you already have CCNx, please jump to step 4 to run Confetti.

### Step 1 Xcode

Download a free copy of Xcode from the App Store and install it, if it is not already installed on your computer.

Make sure the **Xcode Command Line Tools** are installed also. Recent installations of Xcode does not include the command line tools. They can be installed from: Xcode -> Preferences -> Downloads. 

### Step 2 MacPorts

MacPorts "pkg" installers available [here](http://www.macports.org/install.php). 

CCNx can be installed by MacPorts. If your Macports are installed in /opt/local, add the following line at the end of /opt/local/etc/macports/sources.conf before the default port repository.

	rsync://ndn.iu4.ru/macports/

After this step, use the following command to fetch updated port definitions.

	sudo port selfupdate

Then use this command to install CCNx.
	
	sudo port install ccnx

#### Trouble Shooting

### Step 3 CCNx


## Connect With Other Players

## Play

