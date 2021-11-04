# Minecraft Coop Utility
This program is used if you want to play minecraft without dedicated server and used to prevent multiple instance of minecraft server when playing coop. The program will check if any of your friend already run the server, so there will be no multiple instance of server running and overwrite files in the cloud. It's gonna be a headache when you playing hours and realized that your progress already overwriten by your friend.

## What do you need
  - download file cloud system, in my case I'm using [onedrive](https://www.microsoft.com/en-ww/microsoft-365/onedrive/online-cloud-storage). Make sure it's automatically sync
 - download and install  [minecraft server](https://www.minecraft.net/en-us/download/server)  (or other mod server) inside cloud system directory
 - Make sure your server is working fine 
  - Install zerotier [here](https://www.zerotier.com/)

## How to Install
   
 - Close the server if it's running
 - Download minecraft coop utility [here](https://github.com/miputra/Minecraft-Coop-Utility/releases)
 - Extract zip to server directory
 - In File "PUT YOUR SERVER IP HERE.txt", set the value with yours and your friends zero tiers IPs. Remove and replace placeholder domain in there
 - In File "launcher.conf", fill "launcher" value with server file name
 - In File "launcher.conf", fill "service_link" value with your cloud system
 - Now everytime you want to run the server, Just run "RUN MINECRAFT SERVER.exe", and it will check if there any server online. If there is no one online, then you can run the server. If your friend online, the program will prevent you to run server and give you the ip address of who is the server

