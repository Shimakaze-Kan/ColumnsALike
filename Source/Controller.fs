namespace ColumnsALike

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Audio
open ColumnsShare.Column
open ColumnsShare.Logic
open ComponentShare
open System

type Controller () as this =
    inherit Game()
 
    let graphics = new GraphicsDeviceManager(this)
    let mutable spriteBatch = Unchecked.defaultof<_>
    let mutable blocks = Unchecked.defaultof<Texture2D>
    let mutable emptyBlock = Unchecked.defaultof<Texture2D>
    let mutable background = Unchecked.defaultof<Texture2D>
    let mutable background2 = Unchecked.defaultof<Texture2D>
    let blockSize = Point(32,32)
    let size = Point(6,18)
    let mutable board = List.init size.Y (fun _ -> List.init size.X (fun _ -> 0))
    let mutable score = 0
    let mutable level = 1
    let speedPerLevel = 75
    let mutable numMaxBlocks = 5
    let mutable lastMoveDown = Unchecked.defaultof<TimeSpan>
    let mutable lastKeyPress = Unchecked.defaultof<TimeSpan>
    let mutable column = Seq.take 1 (getNextColumn numMaxBlocks) |> Seq.item 0
    let mutable nextColumn = Seq.take 1 (getNextColumn numMaxBlocks) |> Seq.item 0
    let mutable dropDuration = 1000
    let mutable columnPosition = Point(2,0)
    let mutable soundEffect = Unchecked.defaultof<SoundEffect>
    let mutable font25 = Unchecked.defaultof<SpriteFont>
    let mutable font20 = Unchecked.defaultof<SpriteFont>
    let mutable pause = false
    let mutable lastPause = Unchecked.defaultof<TimeSpan>
    let mutable speakerButton = Unchecked.defaultof<TwoStateButton>


    do
        this.Content.RootDirectory <- "Content"
        this.IsMouseVisible <- true
        graphics.PreferredBackBufferHeight <- 600
        graphics.PreferredBackBufferWidth <- 450

    override this.Initialize() =
        emptyBlock <- new Texture2D(this.GraphicsDevice, 1, 1)
        emptyBlock.SetData([|Color.Wheat|])
        this.IsMouseVisible <- true
        base.Initialize()


    override this.LoadContent() =
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)
        blocks <- this.Content.Load<Texture2D>("Sprites/gems")
        background <- this.Content.Load<Texture2D>("Backgrounds/cosmos")
        background2 <- this.Content.Load<Texture2D>("Backgrounds/buildings")
        soundEffect <- this.Content.Load<SoundEffect>("Sounds/glass_breaking")
        font25 <- this.Content.Load<SpriteFont>("font25")
        font20 <- this.Content.Load<SpriteFont>("font20")
        speakerButton <- TwoStateButton(this.Content.Load<Texture2D>("Buttons/speaker_unmute"), this.Content.Load<Texture2D>("Buttons/speaker_mute"), Vector2(370.f,32.f))
        speakerButton.ClickOccured.AddHandler(fun _ _ -> if SoundEffect.MasterVolume = 1.f then SoundEffect.MasterVolume <- 0.f else SoundEffect.MasterVolume <- 1.f)


    override this.Update (gameTime) =
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back = ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
        then this.Exit();

        if pause then
            let keyboard = Keyboard.GetState()
            if (gameTime.TotalGameTime - lastPause).Milliseconds >= 200 && keyboard.IsKeyDown(Keys.Space) then
                pause <- false
                lastPause <- gameTime.TotalGameTime
        else
            if (gameTime.TotalGameTime - lastMoveDown).Milliseconds >= dropDuration then
                lastMoveDown <- gameTime.TotalGameTime
                columnPosition.Y <- columnPosition.Y+1

            if checkDownCollision board columnPosition then
                board <- blit1DArrayIntoListOfLists column board columnPosition.X (columnPosition.Y-1)
                column <- nextColumn
                nextColumn <- Seq.take 1 (getNextColumn numMaxBlocks) |> Seq.item 0
                columnPosition <- Point(2,0)

                let mutable matches = checkMatches board
                if matches.Length <> 0 then
                    soundEffect.Play() |> ignore
                while matches.Length <> 0 do
                    score <- getScore matches.Length level + score
                    level <- getLevel (level,score)
                    board <- matches |> updateBoard board
                    matches <- checkMatches board
                
                if checkGameOver board columnPosition then
                    score <- 0
                    level <- 1
                    dropDuration <- 1000
                    board <- List.init size.Y (fun _ -> List.init size.X (fun _ -> 0))
                
            if (gameTime.TotalGameTime - lastKeyPress).Milliseconds > 100 then
                lastKeyPress <- gameTime.TotalGameTime
                let keyboard = Keyboard.GetState()
                if keyboard.IsKeyDown(Keys.Left) then
                    columnPosition.X <- if predictLeftCollision board columnPosition then columnPosition.X else columnPosition.X-1
                if keyboard.IsKeyDown(Keys.Right) then
                    columnPosition.X <- if predictRightCollision board columnPosition then columnPosition.X else columnPosition.X+1
                if keyboard.IsKeyDown(Keys.Down) then
                    dropDuration <- 50
                else
                    dropDuration <- 1000 - level*speedPerLevel
                if keyboard.IsKeyDown(Keys.X) then
                    column <- rotate column
            
            if (gameTime.TotalGameTime - lastPause).Milliseconds >= 200 then
                    let keyboard = Keyboard.GetState()
                    if keyboard.IsKeyDown(Keys.Space) then
                        lastPause <- gameTime.TotalGameTime
                        pause <- true


        speakerButton.Update(gameTime)
        base.Update(gameTime)
 
    override this.Draw (gameTime) =
        this.GraphicsDevice.Clear Color.CornflowerBlue
        
        spriteBatch.Begin()

        //#region background and board
        spriteBatch.Draw(background, Rectangle(0,0,450,600),Color.White)
        spriteBatch.Draw(background2, Rectangle(0,blockSize.Y*size.Y-9,450,39),Color.White)
        spriteBatch.Draw(emptyBlock, Rectangle(15,15,blockSize.X*size.X,blockSize.Y*size.Y), Color.Black*0.3f)
        //#endregion


        //#region next block preview
        for i=0 to 2 do
            let preview = nextColumn.[i]
            let positionInSheet = getPositionOfSpriteInSheet preview blockSize
            let sourceRectangle = System.Nullable<Rectangle> (Rectangle(positionInSheet, blockSize))
            let positionOfPreview = Point(260+blockSize.X,30+blockSize.Y*i)
            let sizeOfPreview = Point(blockSize.X,blockSize.Y)

            spriteBatch.Draw(emptyBlock, Rectangle(positionOfPreview,sizeOfPreview), Color.Black*0.3f)
            spriteBatch.Draw(blocks, Rectangle(positionOfPreview,sizeOfPreview), sourceRectangle,Color.White)
        //#endregion


        //#region movable column
        for i=0 to 2 do
            let current = column.[i]
            let position = getPositionInDrawingArea blockSize 15 columnPosition.X (columnPosition.Y+i)
            let positionInSheet = getPositionOfSpriteInSheet current blockSize
            let sourceRectangle = System.Nullable<Rectangle> (Rectangle(positionInSheet, blockSize))

            spriteBatch.Draw(blocks, position, sourceRectangle,Color.White)
        //#endregion


        //#region gems filling
        for i=0 to size.Y-1 do
            for j=0 to size.X-1 do
                let current = board.[i].[j]
                let position = getPositionInDrawingArea blockSize 15 j i
                let positionInSheet = getPositionOfSpriteInSheet current blockSize
                let sourceRectangle = System.Nullable<Rectangle> (Rectangle(positionInSheet, blockSize))

                if current<>0 then
                    spriteBatch.Draw(blocks, position, sourceRectangle,Color.White)
        //#endregion

       
        //#region strings
        spriteBatch.DrawString(font25, "Score", Vector2(260.f,250.f), Color.White)
        spriteBatch.DrawString(font20, sprintf "%08d" score, Vector2(260.f,290.f), Color.White)
        spriteBatch.DrawString(font25, "Level", Vector2(260.f,350.f), Color.White)
        spriteBatch.DrawString(font20, sprintf "%02d" level, Vector2(260.f,390.f),Color.White)
        spriteBatch.DrawString(font20, "N\nE\nX\nT", Vector2(260.f, 30.f), Color.White)
        //#endregion

        if pause then
            spriteBatch.Draw(emptyBlock, Rectangle(0,275,450,50), Color.Black*0.7f)
            spriteBatch.DrawString(font25, "Pause", Vector2(175.f,288.f), Color.White)

        speakerButton.Draw gameTime spriteBatch

        spriteBatch.End()

        base.Draw(gameTime)

