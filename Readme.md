#Scrutiny

Scrutiny is a Asp.Net alternative server for Karma.
It runs the standard Karma client and supports the same plugins (at least it will in the future).

For those who don't know Karma, it is a great test runner running in nodejs.

Scrutiny targets Asp.Net development in that it runs as an HttpModule and integrates with the
Asp.Net server-side code.

##Installation

1. Copy `Scrutiny.dll` to the asp.net bin folder and add a reference to Scrutiny in Web.Config

```
  <system.webServer>
    <modules>
      <add type="Scrutiny.Module, Scrutiny" name="Scrutiny"/>
    </modules>
  </system.webServer>
```

2. Add a configuration section for Scrutiny to Web.Config.
	See configuration documentation for how to properly configure Scrutiny.

```	
  <configSections>
    <section name="Scrutiny" type="Scrutiny.Config.Scrutiny, Scrutiny"/>
  </configSections>
  <Scrutiny url="/Scrutiny">
  </Scrutiny>
```

3. Start the server

	TODO: Describe how to start server here
	
4. Start client browsers and attach to the server

	TODO: Describe how to start and attach clients here



User starts the Scrutiny server by running the
