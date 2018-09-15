# RealTime 1.0.0

## What is it?

RealTime is a NuGet package written in C# to get real date and time from a given [NTP](https://en.wikipedia.org/wiki/Network_Time_Protocol) server, based on the fantastic date and time manipulation library [Noda Time](https://nodatime.org/). Simple as that.

## But, why?

There are times you cannot rely on the time data your system provides. In fact, you shouldn't trust what your system says about what time is it. Ever.

The solution goes through asking a NTP server which is the real date and time according to the [UTC](https://en.wikipedia.org/wiki/Coordinated_Universal_Time) standard.

## How it works?

First, you just need to create a new `RealTime` object. You can create the object using the default NTP server (pool.ntp.org):

```C#
var realTime = new RealTime ( );
```

Or you can use a specific NTP server through the constructor:

```C#
var realTime = new RealTime ("time.windows.com");
```

Once created, the object provides just two properties:

* `Now` provides a [`ZonedDateTime`](https://www.nodatime.org/2.3.x/api/NodaTime.ZonedDateTime.html) object according to UTC standard.
* `NowInMyTimeZone` provides a `ZonedDateTime` object according to the system defined time zone.