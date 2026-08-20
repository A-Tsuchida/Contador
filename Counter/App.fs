module App

open Avalonia
open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Hosts
open Elmish


type App() =
  inherit Application()

  override this.Initialize() =
    this.Styles.Add(Themes.Fluent.FluentTheme())
    this.RequestedThemeVariant <- Styling.ThemeVariant.Default

#if DEBUG
    this.AttachDeveloperTools() |> ignore
#endif

  override this.OnFrameworkInitializationCompleted (): unit =
    match this.ApplicationLifetime with
    | :? Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
        let mainWindow = Main.Window()
        desktopLifetime.MainWindow <- mainWindow
    | _ -> ()
