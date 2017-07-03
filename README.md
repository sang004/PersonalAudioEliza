# Phone Bot Type_0

![Picture](https://user-images.githubusercontent.com/8624828/27719641-5f0fc522-5d86-11e7-84e5-316606cbf36a.PNG)  
This is a vocal version of the good old Eliza callbot, now more accessible than ever on Skype bot directory! 

## Table of Content

[Getting started](#getting-started)  
[Prerequisites](#Prerequisites)  
[Bot setup](#Bot setup)
[Local installation](#Local)  
[Azure installation](#Azure)  
[Authors](#Authors)  
[License](#License)  
[Built With](#Built-With)  

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.  

## Prerequisites

Things you need to install:

- Windows 8 and above
- [Visual Studio Community edition 2015 and above](https://www.visualstudio.com/downloads/)
- [ngrok](https://ngrok.com/download)
- [Skype](https://www.skype.com/en/download-skype/skype-for-computer/)

## Setup  

### Bot setup
https://itsamiraclemycodeworks.wordpress.com/2017/01/31/first-blog-post/

Firstly, login into bot framework directory then click create bot
![Picture](https://user-images.githubusercontent.com/8624828/27782087-92c97712-6003-11e7-8e54-94dbdd287689.png)

Then describe it
![Picture](https://user-images.githubusercontent.com/8624828/27782129-bd5b28ae-6003-11e7-9038-510d1fea540e.png)

For this field, you will need the host address of where it is hosted. Either in a tunneling address from ngrok or Azure app service host.
```
https://a8e2caa6.ngrok.io/api/messages
https://callingbot01name.azurewebsites.net/api/messages
```
![Picture](https://user-images.githubusercontent.com/8624828/27782171-f1cbc012-6003-11e7-8d3f-c3938100f50c.png)

And follow through the instructions after clicking the create microsoft app id and password.
![Picture](https://user-images.githubusercontent.com/8624828/27782197-107271fa-6004-11e7-8f13-601db57f5458.png)

Leave every thing else as default and click on save settings.  

With the bot up, you should be able to see some default channels
![Picture](https://user-images.githubusercontent.com/8624828/27782415-09bafa5c-6005-11e7-9900-02a351b51985.png)

Finally, to add call ability to the bot, select edit on the "Skype" channel and onto the "Calling" tab. Enable "Calling" and populate the webhook
```
https://a8e2caa6.ngrok.io/api/calling/call
https://callingbot01name.azurewebsites.net/api/calling/call
```
![Picture](https://user-images.githubusercontent.com/8624828/27782530-7d5c13a6-6005-11e7-88b1-a48ec7f59c69.png)

### Initial Solution setup
After cloning the repository, go to the directory:
![Picture](https://user-images.githubusercontent.com/8624828/27780440-7ac02f60-5ffb-11e7-9369-e11014e58aa5.png)  
Access 'C:\Users\user\Source\Repos\bot\.vs\config\applicationhost.config' and edit this file
![Picture](https://user-images.githubusercontent.com/8624828/27780484-c9662ac0-5ffb-11e7-8d71-6462a94f6d34.png)
At line 168, replicate the line and paste below it. At the replicated line, change "localhost" to astriek to tell the application to listen to every process on the port 3999.
![Picture](https://user-images.githubusercontent.com/8624828/27780538-11a905d2-5ffc-11e7-870e-9984997c7d07.png)

### Web.Config Configurations

- Bot
![Picture](https://user-images.githubusercontent.com/8624828/27781447-7ce3e7a0-6000-11e7-9b9e-08de78736f33.png)

- Azure
![Picture](https://user-images.githubusercontent.com/8624828/27781476-969024c0-6000-11e7-94bc-3141519ecd47.png)

- Local
When running it locally, it requires a tunneling program ["ngrok"](#Local) to be running, explanation is as below.
![Picture](https://user-images.githubusercontent.com/8624828/27781502-ad0b015c-6000-11e7-9080-5aee7192500b.png)

- Other services
![Picture](https://user-images.githubusercontent.com/8624828/27781547-ea00db5e-6000-11e7-99c1-fcd6c24c43dc.png)

## Deployment

### Local
Run code visual studio as administrator, in project folder "..\bot\.vs\config\applicationhost.config", make sure to add
```xml
					<binding protocol="http" bindingInformation="*:3999:*" />
```
at line below line 168 to ensure that the tunneling would work.

In ngrok, type "ngrok http 3999" to start the tunneling from bot framework connector to your running code, it will show that an address that look like:
```
http://a8e2caa6.ngrok.io
```
In bot framework portal, set bot messaging endpoint to
```
https://a8e2caa6.ngrok.io/api/messages
```

and skype channel's calling webhook to
```
https://a8e2caa6.ngrok.io/api/calling/call
```

### Azure
Right click the project and click "Publish"

![image](https://user-images.githubusercontent.com/8624828/27728398-76b3733a-5db4-11e7-84e3-916eb9ed46ac.png)

With this, click on next and change the name of the application if desired. There is an option of Debug/Release, as this is a working copy, choosing 'Release' is fine. So click 'Publish'

When deployment to Azure is successful, this will show up:

![Picture](https://user-images.githubusercontent.com/8624828/27780003-f77c21ec-5ff8-11e7-8450-c65521ae85a9.png)


End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Built With

* [Bot framework C#](https://docs.microsoft.com/en-us/bot-framework/dotnet/bot-builder-dotnet-overview) - The bot framework used 
* [C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/) - language
* [Skype API](https://dev.skype.com/) - The calling platform
* [Bing Speech API](https://docs.microsoft.com/en-us/azure/cognitive-services/speech/home) - Speech to Text

## Contributing

Please read [CONTRIBUTING.md]() for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning


## Authors

* **Shimin Ang** https://github.com/sang004
* **Xuenan Pi** https://github.com/pixuenan

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

