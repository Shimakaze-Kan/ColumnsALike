namespace ColumnsShare

open System
open Microsoft.Xna.Framework


[<AutoOpen>]
module Column =

    /// <sumare> Function to create a new column </sumare>
    /// <param name="numberOfDifferentBlocks"> Number of different blocks from which a block is to be drawn </param>
    /// <returns> Infinite seq </returns>
    let public getNextColumn numberOfDifferentBlocks =
        let rnd = Random()
        let numberOfBlocks = 3
   
        (*The reason why columns are stored in arrays is that the List.permute function 
        from the lists.fs file converts list to array anyway and uses the Array.permute function from the Array.fs file*)
        Seq.initInfinite( fun _ -> Array.init numberOfBlocks (fun _ -> rnd.Next(numberOfDifferentBlocks)+1))
