namespace ComponentShare

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

type public TwoStateButton(textureFirstState: Texture2D, textureSecondState: Texture2D, position: Vector2) =
    inherit Component()

    //#region fields
    let mutable _currentMouse = Unchecked.defaultof<MouseState>
    let mutable _previousMouse = Unchecked.defaultof<MouseState>
    let _textureFirstState = textureFirstState
    let _textureSecondState = textureSecondState
    let _position = position
    let mutable state = true
    //#endregion

    //#region properties
    let clickEvent = new Event<_>()

    member _.Rectangle
        with get() =
            Rectangle((int) _position.X, (int) _position.Y, _textureSecondState.Width, _textureFirstState.Height)
    //#endregion

    //#region methods
    override this.Draw gameTime spriteBatch =
        if state then
            spriteBatch.Draw(_textureFirstState, this.Rectangle, Color.White)
        else
            spriteBatch.Draw(_textureSecondState, this.Rectangle, Color.White)

    override this.Update gameTime =
        _previousMouse <- _currentMouse
        _currentMouse <- Mouse.GetState()

        let mouseRectangle = Rectangle(_currentMouse.X, _currentMouse.Y, 1, 1)

        if mouseRectangle.Intersects(this.Rectangle) && _currentMouse.LeftButton = ButtonState.Released && _previousMouse.LeftButton = ButtonState.Pressed then
            clickEvent.Trigger()
            state <- not state
    //#endregion

    //Expose event
    [<CLIEventAttribute>]
    member _.ClickOccured = clickEvent.Publish
