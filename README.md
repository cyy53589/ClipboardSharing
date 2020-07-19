# ClipboardSharing
 Clipboard sharing bewteen Android(some function may not work for version 10 and higher ) and PC.
 
## Function
ClipboardSharing enables you to copy at device A and paste at device B when both devices connected to network.

## ***Notice for Android 10 ( or higher) user***
Due to android [privacy changed in Android 10](https://developer.android.google.cn/about/versions/10/privacy/changes#clipboard-data), Clipboard access are not allowed if app lose its focus. Resulting text copy at other app in Android couldn't be sent to other devices. Still working on this issue.

# How to Build

## Tools you may need
1. Visual Studio
2. Android Studio

## step
1. Open file ./ClipboardSharing.sln with Visual Studio. Build Project Server and WindowsClient. Remenber to change server default ip address in ./WindowsClient/MainWindows.xaml.cs/Configuration.serverIp
2. Open folder ./android with Android Studio. Build it. Remenber to change default server ip address in app/src/java.com.chenyuyang.NetworkUtil.Configuration.ServerIpAddress.
3. Deploy Project Server to your server. This project written in .Net Core. Yor server need .Net Core [runtime environment](https://dotnet.microsoft.com/download). Run command to start:
   ```bash
   dotet Server.dll
   ```
4. Run windows client and android client. 
5. Click Tool icon at left upper corner to change your username.
6. Chose any one step below to config your PC
   1. Take your phone click toolbar to scan QRCode shown on windows. This would bind your windows and phone by changing username in PC to be same as your phone.
   2. Click settings menu in PC, it would open config file. Modify config and restart windows client to enable change.

# TODO:
## Feature
1. Ugly UI.
2. In Android, service that receiving message may be killed sometime. It would be better if it could use GCM.

## Bugs
1. [Windows] Copy link from Edge cause exception.
