module Config
open Mode

type Data = {
  timeMode: Mode
  time: System.TimeSpan
  clockPosition: int * int
  isClockOpen: bool
  isClockLocked: bool }

let private defaultData = {
  timeMode = Time
  time = System.TimeSpan.Zero
  clockPosition = 1500, 1500
  isClockOpen = false
  isClockLocked = true }

let private serializerOptions =
  let opt = System.Text.Json.JsonSerializerOptions()
  opt.Converters.Add(ModeConverter())
  opt

let readConfig () =
  let directory = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Counter")
  if (not (System.IO.Directory.Exists(directory))) then
    defaultData
  else
    let file = System.IO.Path.Combine(directory, "config.json")
    if (not (System.IO.File.Exists(file))) then
      defaultData
    else
      let json = System.IO.File.ReadAllText(file)
      try
        System.Text.Json.JsonSerializer.Deserialize<Data>(json, serializerOptions)
      with
      | _ -> defaultData

let saveConfig (data: Data) =
  let directory =
    let path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), "Counter")
    new System.IO.DirectoryInfo(path)
  if not directory.Exists then directory.Create()

  let file = System.IO.Path.Combine(directory.FullName, "config.json")

  let json = System.Text.Json.JsonSerializer.Serialize(data, serializerOptions)

  System.IO.File.WriteAllText(file, json)
