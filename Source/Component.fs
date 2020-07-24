namespace ComponentShare

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

[<AbstractClassAttribute>]
type public Component() =
    abstract member Draw: GameTime -> SpriteBatch -> unit
    abstract member Update: GameTime -> unit