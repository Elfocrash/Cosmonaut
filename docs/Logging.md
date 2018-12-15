# Logging

## Event source

Cosmonaut uses the .NET Standard's `System.Diagnostics` to log it's actions as dependency events. 

By default, this system is deactivated. In order to activated and actually do something with those events you need to create an  `EventListener` which will activate the logging and give you the option do something with the logs.

## Cosmonaut.ApplicationInsights

By using this package you are able to log the events as dependencies in [Application Insights](https://azure.microsoft.com/en-gb/services/application-insights/) in detail. The logs are batched and send in intervals OR automatically sent when the batch buffer is filled to max.

Just initialise the AppInsightsTelemetryModule in your Startup or setup pipeline like this.
Example: 

```c#
AppInsightsTelemetryModule.Instance.Initialize(new TelemetryConfiguration("InstrumentationKey"));
```

If you already have initialised `TelemetryConfiguration` for your application then use `TelemetryConfiguration.Active` instead of `new TelemetryConfiguration` because if you don't there will be no association between the dependency calls and the parent request.


```c#
AppInsightsTelemetryModule.Instance.Initialize(new TelemetryConfiguration(TelemetryConfiguration.Active));
```