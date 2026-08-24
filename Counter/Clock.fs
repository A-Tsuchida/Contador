module Clock

open Avalonia.Controls
open Avalonia.FuncUI
open Avalonia.FuncUI.DSL
open global.Elmish
open Avalonia.FuncUI.Builder

type private Model = { isActive: bool; left: System.TimeSpan; locked: bool }

type private Msg =
  | Tick of System.TimeSpan
  | Start of System.TimeSpan
  | Stop
  | ToggleLock of bool

let private init () =
  { isActive = false; left = System.TimeSpan.Zero; locked = true }, Cmd.none

let private update (msg: Msg) (state: Model) =
  let tickCmd limit =
    let action () = async {
      do! Async.Sleep 1000
      return limit - System.TimeSpan.FromSeconds 1.0
    }
    Cmd.OfAsyncImmediate.perform action () (fun left -> Tick left)

  match msg with
  | Tick left ->
    if state.isActive
    then
      { state with left = left }, tickCmd left
    else
      state, Cmd.none
  | Start time ->
    if state.isActive
    then
      state, Cmd.none
    else
      { state with left = time; isActive = true }, tickCmd time
  | Stop ->
    if state.isActive
    then
      { state with left = System.TimeSpan.Zero; isActive = false }, Cmd.none
    else
      state, Cmd.none
  | ToggleLock locked ->
    { state with locked = locked }, Cmd.none

let private elementRole value =
  AttrBuilder<Border>.CreateProperty<Avalonia.Input.WindowDecorationsElementRole>(Chrome.WindowDecorationProperties.ElementRoleProperty, value, ValueNone)

let private view (state: Model) dispatch =
  let timeString = sprintf "%02d:%02d:%02d" (state.left.TotalHours |> int |> abs) (state.left.Minutes |> abs) (state.left.Seconds |> abs)
  let isNegative = state.left < System.TimeSpan.Zero
  let isLocked = state.locked

  Panel.create [
    Panel.children [
      StackPanel.create [
        Avalonia.Thickness(10, 3, 10, 5) |> StackPanel.margin
        StackPanel.children [
          TextBlock.create [
            TextBlock.foreground (if isNegative then Avalonia.Media.Brushes.Red else Avalonia.Media.Brushes.White)
            TextBlock.text (sprintf "%s%s " (if isNegative then "-" else " ") timeString)
            TextBlock.fontSize 48.0
          ]
        ]
      ]
      Border.create [
        elementRole (if isLocked then Avalonia.Input.WindowDecorationsElementRole.None else Avalonia.Input.WindowDecorationsElementRole.TitleBar)
        Border.background Avalonia.Media.Brushes.Transparent
        Border.verticalAlignment Avalonia.Layout.VerticalAlignment.Stretch
        Border.horizontalAlignment Avalonia.Layout.HorizontalAlignment.Stretch
      ]
    ]
  ]

open Avalonia.FuncUI.Elmish

let private createSubscriber (event: Event<Msg>) state: Sub<Msg> =
  let extSub dispatch = event.Publish.Subscribe(fun msg -> dispatch msg)
  [ (["ExternalMessages"], extSub) ]

type ExtMsg =
  | Start of System.TimeSpan
  | Stop
  | ToggleLock of bool
  | ToggleVisibility of bool
  | Initialize of {| clockPosition: int * int; isClockOpen: bool; isClockLocked: bool |}

type Window() as this =
  inherit Hosts.HostWindow()
  let event = new Event<Msg>()
  do
    this.ShowInTaskbar <- false
    this.Topmost <- true
    this.ExtendClientAreaToDecorationsHint <- true
    this.CanResize <- false
    this.CanMinimize <- false
    this.WindowDecorations <- WindowDecorations.BorderOnly
    this.SizeToContent <- SizeToContent.WidthAndHeight
    this.TransparencyLevelHint <- [
      if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then
        WindowTransparencyLevel.AcrylicBlur
      else
        WindowTransparencyLevel.Transparent
    ]
    this.Background <- Avalonia.Media.Brushes.Transparent

    let subscriber = createSubscriber event

    Program.mkProgram init update view
    |> Program.withHost this
    |> Program.withSubscription subscriber
    |> Program.runWithAvaloniaSyncDispatch ()

  interface ISendMessage<ExtMsg> with
    member this.SendMessage(msg) =
      match msg with
      | Start time -> event.Trigger(Msg.Start time)
      | Stop -> event.Trigger(Msg.Stop)
      | ToggleLock state -> event.Trigger(Msg.ToggleLock state)
      | ToggleVisibility state -> if state then this.Show() else this.Hide()
      | Initialize config ->
          this.Position <-
            let x, y = config.clockPosition
            Avalonia.PixelPoint(x, y)
          this.Show()
          if not config.isClockOpen then this.Hide()
          event.Trigger(Msg.ToggleLock config.isClockLocked)
