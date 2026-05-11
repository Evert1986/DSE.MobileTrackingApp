# DSE Mobile Tracking App

This is a real .NET MAUI Blazor Hybrid mobile app project targeting:

- Android
- iOS / iPhone

## Important

Android can be run from Visual Studio on Windows with an Android emulator.

iOS/iPhone requires a Mac build host with Xcode, or Visual Studio paired to a Mac. Windows cannot build/run iOS by itself.

## How to open

1. Extract the ZIP.
2. Open `DSE.MobileTrackingApp.sln`.
3. Restore/build.
4. Select Android emulator to run on Windows.

## API connection point

Replace mock data in:

`Services/MockTrackingDataService.cs`

with your API service.

## Included mobile screens

- Home dashboard
- Add readings
- History
- Alerts
- Settings

## Current mock values

- pH = 5.5
- Wax = 1.1 L/Ton
- Temperature = 21.4 °C
- Titration = 565 ppm

## Windows build note

If Android build gives `XAJCW7023` with a very long `obj\Debug\net10.0-android\android\src\mono\...java` path, delete `bin` and `obj`, then move/extract the solution to a short path such as:

```text
C:\Dev\DSEMobileApp
```

This project also includes `Directory.Build.props` to place generated intermediate files under your Windows temp folder to reduce Android generated path length.
