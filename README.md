# ScriptableObject-guids

A system for managing persistent guids on Scriptable Objects.
If you use SO's for many things in your Unity project, at some point, some of them will land in your player save data. And then you have to remember to NEVER change their internal ID's, otherwise it screws with your player data in production.
This eases that issue.

#How to use
- In your ScriptableObject class, just implement a public member of type GuidObject.
- The system will check newly created SO's and also duplicated SO's for missing or duplicate Guids.
- Voila.

![GuidObj](https://user-images.githubusercontent.com/9436242/232253928-809ccb0b-485b-43a0-bd41-b5e9c530ca05.gif)
