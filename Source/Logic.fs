namespace ColumnsShare

open System
open Microsoft.Xna.Framework

[<AutoOpen>]
module Logic =

    /// <summary> Shift the given one-dimensional array by one </summary>
    /// <param name="array"> One-dimensional array </param>
    /// <returns> One-dimensional array </returns>
    let rotate (array: int []) =
        let length = Array.length array
        let shiftBy = 1
        Array.permute (fun index -> (index+shiftBy)%length) array


    /// <summary> Sticks a one-dimensional array to the nested list vertically </summary>
    /// <param name="source"> The One-dimensional array to be pasted </param>
    /// <param name="destination"> The nested list into which the first argument is to be pasted </param>
    /// <param name="indexX"> Destination index horizontal </param>
    /// <param name="indexY"> Destination index vertical </param>
    /// <returns> Nested list </returns>
    let blit1DArrayIntoListOfLists (source: 'a []) (destination: 'a list list) indexX indexY =
        let sourceLength = Array.length source

        let blit (elem: 'a) (destination1D: 'a list) at =                
            let rec aux index acc = function
                | [] -> acc
                | h::t -> if index=at then aux (index+1) (elem::acc) t
                            else aux (index+1) (h::acc) t
            in List.rev (aux 0 [] destination1D)

        let rec findRowsAndReplace index currentIndexInArray acc = function
            | [] -> acc
            | currentRow::t -> if index>=indexY && currentIndexInArray<sourceLength then 
                                    let currentElemToBePasted = source.[currentIndexInArray]
                                    findRowsAndReplace (index+1) (currentIndexInArray+1) ((blit currentElemToBePasted currentRow indexX)::acc) t
                                else findRowsAndReplace (index+1) (currentIndexInArray) (currentRow::acc) t      
        List.rev (findRowsAndReplace 0 0 [] destination)


    /// <summary> Function to look for elements that are repeated at least 3 times in 
    /// succession horizontally, vertically or diagonally in a nested list </summary>
    /// <param name="board"> Nested list where matches will be looked for </param>
    /// <returns> List of tuples containing coordinates of repeating elements </returns>
    let checkMatches (board: int list list) =
        let findMatches begginingAcc (list: (int*int*int) list) =
            let rec aux acc currentAcc lastElem = function
                | [] -> []
                | [(a,x,y)] -> if List.length currentAcc >=2 && lastElem=a then
                                (((x,y)::currentAcc)::acc) else acc
                | (a,x,y)::((b,_,_)::_ as t) -> let updatedAcc = (x,y)::currentAcc
                                                if a<>0 && a=b then aux acc updatedAcc a t
                                                else if List.length currentAcc >=2 then aux (updatedAcc::acc) [] a t
                                                else aux acc [] a t
                in
                aux begginingAcc [] -1 list
        
        let chooseRow (list: int list list) row =
            let rec aux index = function
                | [] -> failwith "wrong index"
                | h::t -> if index=row then List.mapi (fun i elem -> (elem,i,row)) h else aux (index+1) t
            in aux 0 list
            
        let chooseColumn list col =
            let rec collect index = function
                | [] -> failwith "wrong index"
                | h::t -> if index=0 then h else collect (index-1) t
            let rec aux acc index = function
                | [] -> acc
                | h::t -> aux ((collect index h)::acc) index t
                in List.rev (aux [] col list) |> List.mapi (fun i elem -> (elem,col,i))

        let chooseDiagonalLR (list: 'a list list) numofDiag =
            let nX = list.[0].Length
            let nY = list.Length
            let y = numofDiag
            [for x in 0..(nX-1) do if 0<=y-x && y-x<nY then (list.[y-x].[x],x,y-x)]

        let chooseDiagonalRL (list: 'a list list) numofDiag =
            let nX = list.[0].Length
            let nY = list.Length
            let y = 1-nY+numofDiag
            [for x in 0..(nY-1) do if 0<=y+x && y+x<nX then (list.[x].[y+x],y+x,x)]

        let mutable (result: (int*int) list list) = []

        for i=0 to board.Length-1 do
            result <- (chooseRow board i |> findMatches result)

        for i=0 to board.[0].Length-1 do
            result <- (chooseColumn board i |> findMatches result)

        for i=0 to (board.Length+board.[0].Length-2) do
            result <- (chooseDiagonalLR board i |> findMatches result)

        for i=0 to (board.Length+board.[0].Length-2) do
            result <- (chooseDiagonalRL board i |> findMatches result)

        List.concat result    


    /// <summary> Function to delete elements and update blank spaces of nasted list </summary>
    /// <param name="board"> Target nested list </param>
    /// <param name="matches"> List of tuples containing coordinates of elements </param>
    /// <returns> Updated nested list </returns>
    let updateBoard (board: int list list) (matches: (int*int) list) =
        if matches.Length = 0 then
            board
        else
            let copyOfBoard = [|for _=0 to board.Length-1 do Array.create board.[0].Length 0|]
            for i=0 to copyOfBoard.Length-1 do
                for j=0 to copyOfBoard.[0].Length-1 do
                    copyOfBoard.[i].[j] <- board.[i].[j]

            for i=0 to copyOfBoard.[0].Length-1 do
                for j=0 to copyOfBoard.Length-1 do
                    if List.contains (i,j) matches then
                        for k=j downto 1 do
                            copyOfBoard.[k].[i] <- copyOfBoard.[k-1].[i]
                        copyOfBoard.[0].[i] <- 0
            copyOfBoard |> Array.map (fun x -> x |> Array.toList) |> List.ofArray       


    /// <summary> Function to calculate the number of points depending on the current level </summary>
    /// <param name="numOfMatches"> Number of matches </param>
    /// <param name="level"> Current level </param>
    /// <returns> Calculated number of points </returns>
    let getScore numOfMatches level =
        numOfMatches*level*30

    /// <summary> Function to calculate current level depending on the number of points </summary>
    /// <returns> Calculated level </returns>
    let getLevel (level,score) =
        match level,score with
        | 10, _ -> 10
        | n, p -> if p>n*1000 then (n+1)
                    else n
        

    /// <summary> Function to check if it is possible to move the block one position to the left </summary>
    /// <param name="board"> Nested list from which the possibility to move a column is checked </param>
    /// <param name="position"> The coordinates of the first block in the column </param>
    let predictLeftCollision (board: int list list) (position: Point) =
        (position.X-1) = -1 || board.[position.Y+2].[position.X-1]<>0

    /// <summary> Function to check if it is possible to move the block one position to the right </summary>
    /// <param name="board"> Nested list from which the possibility to move a column is checked </param>
    /// <param name="position"> The coordinates of the first block in the column </param>
    let predictRightCollision (board: int list list) (position: Point) =
        (position.X+1) = 6 || board.[position.Y+2].[position.X+1]<>0

    /// <summary> Function to check if the column is in a place where it should not be </summary>
    /// <param name="board"> Nested list from which the veracity of the statement is determined </param>
    /// <param name="position"> The coordinates of the first block in the column </param>
    let checkDownCollision (board: int list list) (position: Point) =
        position.Y+2 = board.Length || board.[position.Y+2].[position.X]<>0

    /// <summary> It checks if the game is over </summary>
    /// <param name="board"> A nested list in which the end of the game is checked based on the next argument </param>
    /// <param name="position"> The coordinates of the first block in the column </param>
    let checkGameOver (board: int list list) (position: Point) =
        board.[position.Y+2].[position.X]<>0

    /// <summary> Function to determine the sprite position in 2D texture </summary>
    /// <param name="gem"> Gem number </param>
    /// <param name="blockSize"> Size of one gem </param>
    /// <returns> Gem position in 2D texture </returns>
    let getPositionOfSpriteInSheet gem (blockSize: Point) =
        let xCor = blockSize.X * gem
        Point(xCor, 0)

    /// <summary> Function to determine the position in which sprite should be drawn 
    /// based on coordinates in a 2-dimensional collection </summary>
    /// <param name="blockSize"> Size of sprite </param>
    /// <param name="offset"> Offset from the beginning of the drawing area horizontally and vertically </param>
    /// <param name="x"> The x-coordinate of the item in the collection </param>
    /// <param name="y"> The y-coordinate of the item in the collection </param>
    /// <returns> The position on which the sprite should be drawn in the drawing area </returns>
    let getPositionInDrawingArea (blockSize: Point) offset x y =
        let xCor = x*blockSize.X+offset |> float32
        let yCor = y*blockSize.Y+offset |> float32
        Vector2(xCor, yCor)