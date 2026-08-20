open Avalonia

// For more information see https://aka.ms/fsharp-console-apps
[<EntryPoint>]
let main args =
  AppBuilder
    .Configure<App.App>()
    .UsePlatformDetect()
    .UseSkia()
    .LogToTrace()
    .StartWithClassicDesktopLifetime(args)
