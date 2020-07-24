namespace ColumnsALike

module Program =

    open System
    open Microsoft.Xna.Framework

    [<EntryPoint>]
    let main argv =
        use controller = new Controller()
        controller.Run()
        0 // return an integer exit code
