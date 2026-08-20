[<AutoOpen>]
module ISendMessage

type 'a ISendMessage =
  abstract member SendMessage: 'a -> unit
