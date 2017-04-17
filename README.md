# bot

Relational diagram for classes :  https://github.com/sang004/bot/blob/master/Class_Diagram.svg

Detailed setup guide: https://itsamiraclemycodeworks.wordpress.com/2017/01/31/first-blog-post/

Requirements:
-Windows 8 and above
-Visual Studio 2015 and above
-ngrok
-Skype

TO RUN (locally):
Run code visual studio as administrator, in project folder "..\bot\.vs\config\applicationhost.config", make sure to add

					<binding protocol="http" bindingInformation="*:3999:*" />

at line below line 168 to ensure that the tunneling would work.

In ngrok, type "ngrok http 3999" to start the tunneling from bot framework connector to your running code, it will show that an address that look like "http://131d7541.ngrok.io".

In bot framework portal, set bot messaging endpoint to

https://131d7541.ngrok.io/api/messages


and skype channel's calling webhook to

https://131d7541.ngrok.io/api/calling/call

TO RUN (Azure): WIP


