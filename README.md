# Project Title

![Picture](https://user-images.githubusercontent.com/8624828/27719641-5f0fc522-5d86-11e7-84e5-316606cbf36a.PNG)  
This is a vocal version of the good old Eliza callbot, now more accessible than ever on Skype bot directory! 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

What things you need to install the software and how to install them

```
Give examples
```

### Installing

#### Local
Run code visual studio as administrator, in project folder "..\bot\.vs\config\applicationhost.config", make sure to add

					<binding protocol="http" bindingInformation="*:3999:*" />

at line below line 168 to ensure that the tunneling would work.

In ngrok, type "ngrok http 3999" to start the tunneling from bot framework connector to your running code, it will show that an address that look like "http://131d7541.ngrok.io".

In bot framework portal, set bot messaging endpoint to

https://131d7541.ngrok.io/api/messages


and skype channel's calling webhook to

https://131d7541.ngrok.io/api/calling/call

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

* [Dropwizard](http://www.dropwizard.io/1.0.2/docs/) - The web framework used
* [Maven](https://maven.apache.org/) - Dependency Management
* [ROME](https://rometools.github.io/rome/) - Used to generate RSS Feeds

## Contributing

Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Billie Thompson** - *Initial work* - [PurpleBooth](https://github.com/PurpleBooth)

See also the list of [contributors](https://github.com/your/project/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc
