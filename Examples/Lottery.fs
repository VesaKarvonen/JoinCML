namespace JoinCML.Examples

open JoinCML

type Lottery = {winner: Alt<unit>; loser: Alt<unit>}

[<CompilationRepresentation (CompilationRepresentationFlags.ModuleSuffix)>]
module Lottery =
  let create n =
    assert (0 < n)
    let lottery = Ch.create ()
    let winner = lottery *<- ()
    let loser = ~~lottery
    let rec mk op = function
      | 0 -> op
      | n -> mk (op <&> winner |> Alt.after ignore) (n-1)
    let op = mk loser (n-1)
    let rec forever () = op |>>= forever
    forever () |> Async.Start
    {winner = winner; loser = loser}
  let option l op =
    (l.winner <&> op |> Alt.after (fun ((), x) -> Some x)) <|>
    (l.loser         |> Alt.after (fun  ()     -> None))
