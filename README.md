# JoinCML

A research project to create an extension of CML with a `join` combinator.

The core of CML-style events, or alternatives, boils down to two types:

```fsharp
type Alt<'x>
type Ch<'x>
```

And a set of combinators:

```fsharp
module Ch =
  val create: unit -> Ch<'x>
  val give: Ch<'x> -> 'x -> Alt<unit>
  val take: Ch<'x> -> Alt<'x>

module Alt =
  val choice: Alt<'x> -> Alt<'x> -> Alt<'x>
  val withNack: (Alt<unit> -> Alt<'x>) -> Alt<'x>
  val afterAsync: ('x -> Async<'y>) -> Alt<'x> -> Alt<'y>
  val sync: Alt<'x> -> Async<'x>
```

The idea of
[Transactional Events](http://www.cs.rit.edu/~mtf/research/tx-events/ICFP06/icfp06.pdf)
(TE) is to extend CML-style events to a monad (with plus).  The result is a
highly expressive synchronization mechanism.

The idea of JoinCML is to extend CML-style alternatives just far enough to form
an applicative functor.  More specifically by adding a single new combinator:

```fsharp
module Alt =
  val join: Alt<'x> -> Alt<'y> -> Alt<'x * 'y>
```

Informally, the semantics is that the two given alternatives must both be
available simultaneously in order for the alternative to complete.

Using `join` one can implement the `<*>` operation of applicative functors as:

```fsharp
let (<*>) x2yA xA =
  Alt.join x2yA xA
  |> Alt.after (fun (x2y, x) ->
     x2y x)
```

Unlike TE, JoinCML essentially retains all of CML as a subset, including
negative acknowledgments and the semantics of having only a single commit point.

This project seeks to answer the following questions:

* What can JoinCML express that cannot be expressed in CML?
* What can JoinCML cannot express that TE can express?
* Does JoinCML have favorable properties when compared to TE?
* Can JoinCML be implemented efficiently?

Here are some working hypotheses:

* CML < JoinCML < TE.  That is, JoinCML is strictly more expressive than CML and
  TE is strictly more expressive than JoinCML.

* JoinCML allows n-way rendezvous to be implemented for any n determined before
  the synchronization.  TE allows the n to be determined during synchronization.
  CML only allows 2-way rendezvous.

* JoinCML subsumes core
  [join-calculus](http://research.microsoft.com/en-us/um/people/fournet/papers/join-tutorial.pdf)
  in a synchronous form.

* JoinCML is significantly less expensive to implement than TE, because
  synchronization does not require evaluating events step-by-step during
  synchronization.

* JoinCML is better suited to impure languages than TE, because synchronization
  does not require running arbitrary code and thus there is no danger of
  performing side-effects that cannot be unrolled.

At this point JoinCML is vaporware.

Some examples have been drafted:

* 3-way swap channel in [Swap3.fsi](Examples/Swap3.fsi) and
  [Swap3.fs](Examples/Swap3.fs).
* Dining Philosophers [Dining.fs](Examples/Dining.fs) using `MVar`s defined in
  [MVar.fsi](Examples/MVar.fsi) and [MVar.fsi](Examples/MVar.fsi).
