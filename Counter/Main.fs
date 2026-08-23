module Main

open Avalonia.Controls
open Avalonia.FuncUI.DSL
open Avalonia.Layout
open Elmish
open Mode

type private State = {
  mode: Mode
  time: System.TimeSpan
  clock: ISendMessage<Clock.ExtMsg>
  isClockOpen: bool
  isClockLocked: bool
  isClockRunning: bool
  clockPosition: int * int }

type private Message =
  | SetMode of Mode
  | UpdateTime of System.TimeSpan
  | ToggleClock
  | ToggleDisplay
  | ToggleClockMove
  | ClockPosition of int * int

let private init (config: Config.Data) (clock: ISendMessage<Clock.ExtMsg>) () =
  Clock.ExtMsg.Initialize {| clockPosition = config.clockPosition; isClockOpen = config.isClockOpen; isClockLocked = config.isClockLocked |}
  |> clock.SendMessage

  { mode = config.timeMode
    time = config.time
    clock = clock
    isClockOpen = config.isClockOpen
    isClockLocked = config.isClockLocked
    isClockRunning = false
    clockPosition = config.clockPosition }, Cmd.none

let private update (msg: Message) (state: State) =
  let newState =
    match msg with
    | SetMode m -> { state with mode = m }
    | UpdateTime t -> { state with time = t }
    | ToggleClock ->
      if state.isClockRunning then
        Clock.ExtMsg.Stop |> state.clock.SendMessage
        { state with isClockRunning = false }
      else
        match state.mode with
        | Time ->
          let now = System.DateTime.Now
          let time =
            if state.time.Hours < now.Hour || (state.time.Hours = now.Hour && state.time.Minutes <= now.Minute) then
              state.time + System.TimeSpan.FromDays(1.0)
            else
              state.time

          let target = new System.DateTime(System.DateOnly.FromDateTime(now), System.TimeOnly.FromTimeSpan(time))
          target - now |> Clock.ExtMsg.Start |> state.clock.SendMessage
          { state with isClockRunning = true }
        | Period ->
          Clock.ExtMsg.Start state.time |> state.clock.SendMessage
          { state with isClockRunning = true }
    | ToggleDisplay ->
      let newState = not state.isClockOpen
      Clock.ExtMsg.ToggleVisibility newState |> state.clock.SendMessage
      { state with isClockOpen = newState }
    | ToggleClockMove ->
      let newState = not state.isClockLocked
      Clock.ExtMsg.ToggleLock newState |> state.clock.SendMessage
      { state with isClockLocked = newState }
    | ClockPosition (x, y) -> { state with clockPosition = x, y }

  Config.saveConfig {
    timeMode = newState.mode
    time = newState.time
    clockPosition = newState.clockPosition
    isClockOpen = newState.isClockOpen
    isClockLocked = newState.isClockLocked }

  newState, Cmd.none

let private view (state: State) dispatch =
  StackPanel.create [
    StackPanel.spacing 10.0
    StackPanel.children [
      StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.spacing 10.0
        StackPanel.children [
          RadioButton.create [
            RadioButton.content "Hora"
            RadioButton.isChecked (state.mode = Time)
            RadioButton.onClick (fun _ -> SetMode Time |> dispatch)
          ]
          RadioButton.create [
            RadioButton.content "Tempo"
            RadioButton.isChecked (state.mode = Period)
            RadioButton.onClick (fun _ -> SetMode Period |> dispatch)
          ]
        ]
      ]
      StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.spacing 5
        StackPanel.children [
          TimePicker.create [
            TimePicker.useSeconds true
            TimePicker.selectedTime state.time
            TimePicker.onSelectedTimeChanged (fun time -> UpdateTime time.Value |> dispatch)
          ]
          Button.create [
            Button.tip (if state.isClockRunning then "Parar" else "Iniciar")
            Button.content (if state.isClockRunning then Icons.stopIcon else Icons.playIcon)
            Button.onClick (fun _ -> dispatch ToggleClock)
          ]
        ]
      ]
      StackPanel.create [
        StackPanel.orientation Orientation.Horizontal
        StackPanel.spacing 5
        StackPanel.children [
          Button.create [
            Button.content (if state.isClockOpen then "Ocultar Contador" else "Mostrar Contador")
            Button.onClick (fun _ -> dispatch ToggleDisplay)
          ]
          Button.create [
            Button.content (if state.isClockLocked then "Destravar Contador" else "Travar Contador")
            Button.onClick (fun _ -> dispatch ToggleClockMove)
          ]
        ]
      ]
    ]
  ]

open Avalonia.FuncUI.Elmish
open Avalonia.FuncUI.Hosts

type private EventMsg =
  | ClockPosition of int * int

let private subscriber (event: Event<EventMsg>) state =
  let eventHandler dispatch =
    event.Publish.Subscribe(function
    | ClockPosition (x, y) -> dispatch (Message.ClockPosition (x, y)))

  [ (["EventMessages"], eventHandler) ]

type Window() as this =
  inherit HostWindow()

  let clock = new Clock.Window()
  let eventBus = new Event<EventMsg>()
  do
    this.Title <- "Contador"
    this.Closed.Add(this.OnClosedWindow)
    this.ClientSize <- Avalonia.Size(300.0, 200.0)
    this.CanResize <- false

    let config = Config.readConfig()
    let init = init config clock

    clock.PositionChanged.Add(fun e ->
      eventBus.Trigger(ClockPosition (e.Point.X, e.Point.Y)))

    Program.mkProgram init update view
    |> Program.withHost this
    |> Program.withSubscription (subscriber eventBus)
    |> Program.runWithAvaloniaSyncDispatch ()

  member private _.OnClosedWindow(e) =
    clock.Close()
