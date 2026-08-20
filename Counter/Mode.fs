module Mode

open System.Text.Json.Serialization
open System.Text.Json

type Mode =
  | Time
  | Period

type ModeConverter() =
  inherit JsonConverter<Mode>()

  override _.Read(reader: byref<Utf8JsonReader>, _typeToConvert: System.Type, _options: JsonSerializerOptions) =
    match reader.GetString() with
    | "Time" -> Time
    | "Period" -> Period
    | _ -> failwith "Invalid Mode value"

  override _.Write(writer: Utf8JsonWriter, value: Mode, _options: JsonSerializerOptions) =
    match value with
    | Time -> writer.WriteStringValue("Time")
    | Period -> writer.WriteStringValue("Period")
