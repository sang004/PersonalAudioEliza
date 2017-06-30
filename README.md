# Phone Bot Type_0

![Picture](https://user-images.githubusercontent.com/8624828/27719641-5f0fc522-5d86-11e7-84e5-316606cbf36a.PNG)  
This is a vocal version of the good old Eliza callbot, now more accessible than ever on Skype bot directory! 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

Things you need to install:

- Windows 8 and above
- [Visual Studio Community edition 2015 and above](https://www.visualstudio.com/downloads/)
- [ngrok](https://ngrok.com/download)
- [Skype](https://www.skype.com/en/download-skype/skype-for-computer/)

### Installing

#### Local
Run code visual studio as administrator, in project folder "..\bot\.vs\config\applicationhost.config", make sure to add
```xml
					<binding protocol="http" bindingInformation="*:3999:*" />
```
at line below line 168 to ensure that the tunneling would work.

In ngrok, type "ngrok http 3999" to start the tunneling from bot framework connector to your running code, it will show that an address that look like:
```
http://131d7541.ngrok.io
```
In bot framework portal, set bot messaging endpoint to
```
https://131d7541.ngrok.io/api/messages
```

and skype channel's calling webhook to
```
https://131d7541.ngrok.io/api/calling/call
```

#### Azure
Right click the project and click "Publish"

![image](https://user-images.githubusercontent.com/8624828/27728398-76b3733a-5db4-11e7-84e3-916eb9ed46ac.png)


A step by step series of examples that tell you have to get a development env running

Say what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

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

