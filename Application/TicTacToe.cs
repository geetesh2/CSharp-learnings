namespace Application;

public enum Symbol
{
    X,
    O,
    Empty
}

public class Cell(int row, int col, Symbol symbol)
{
    public int Row = row;
    public int Col = col;
    public Symbol Symbol = symbol;
}

public interface IWinningStrategy
{
    // Updated signature to take the board and the last move made
    bool IsWinner(Board board, Cell lastMove);
}

public class StandardWinningStrategy : IWinningStrategy
{
    public bool IsWinner(Board board, Cell lastMove)
    {
        int row = lastMove.Row;
        int col = lastMove.Col;
        Symbol symbol = lastMove.Symbol;
        int size = board.Size;

        bool rowWin = true, colWin = true, diagWin = true, antiDiagWin = true;

        for (int i = 0; i < size; i++)
        {
            // 1. Check the entire Row
            if (board.GetSymbol(row, i) != symbol) rowWin = false;
            
            // 2. Check the entire Column
            if (board.GetSymbol(i, col) != symbol) colWin = false;
            
            // 3. Check Main Diagonal (Top-Left to Bottom-Right)
            if (board.GetSymbol(i, i) != symbol) diagWin = false;
            
            // 4. Check Anti-Diagonal (Top-Right to Bottom-Left)
            if (board.GetSymbol(i, size - 1 - i) != symbol) antiDiagWin = false;
        }

        // If any of the lines are completely filled with the same symbol, it's a win
        return rowWin || colWin || diagWin || antiDiagWin;
    }
}
public class AlreadyOccupied(int row, int col, Symbol symbol)
    : Exception($"Already occupied for {row},{col} with symbol {symbol}");

public class InvalidMove(int row, int col, Symbol symbol):Exception($"Invalid move for {row},{col} with symbol {symbol}");
public class Board
{
    public int Size{get;set;}
    private readonly Cell[,] Cells;
    
    public Board(int size)
    {
        this.Size = size;
        this.Cells = new Cell[Size, Size];
        InitializeBoard();
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < this.Size; i++)
        {
            for (int j = 0; j < this.Size; j++)
            {
               Cells[i,j] = new Cell(i,j,Symbol.Empty);
            }
        }
    }

    public void MakeMove(int row, int col, Symbol symbol)
    {
        if (row < 0 || row >= this.Size || col < 0 || col >= this.Size)
        {
            throw new InvalidMove(row, col, symbol);
        }
        else if (Cells[row, col].Symbol != Symbol.Empty)
        {
            throw new AlreadyOccupied(row, col, symbol);
        }
        else
        {
             Cells[row, col].Symbol = symbol;
        }
    }

    public void Print()
    {
        for (int i = 0; i < this.Size; i++)
        {
            for (int j = 0; j < this.Size; j++)
            {
                var cells = this.Cells;
                Console.Write(cells[i, j].Symbol + " ");
            }
            Console.WriteLine();
        }
    }

    public Symbol GetSymbol(int row, int col)
    {
        return Cells[row, col].Symbol;
    }

    public Cell GetCell(int row, int col)
    {
        return Cells[row, col];
    }
}

public class Player(Symbol symbol)
{
    public Symbol Symbol{get;set;} = symbol;
    public string Name{get;set;}
}

public class Game
{
    private Board _board;
    private Player _player1 = new Player(Symbol.X);
    private Player _player2 = new Player(Symbol.O);
    private int turn = 0;
    private Player CurrentPlayer;
    private IWinningStrategy _winningStrategy;

    public Game(IWinningStrategy strategy)
    {
        _winningStrategy = strategy;
    }
    public void Start()
    {
        Console.WriteLine("Please enter Size of Board");
        int size = int.Parse(Console.ReadLine());
        _board =  new Board(size);
        Console.WriteLine("Please enter First Player name");
        string playerName = Console.ReadLine();
        _player1.Name = playerName;
        Console.WriteLine("Please enter Second Player name");
        string player2Name = Console.ReadLine();
        _player2.Name = player2Name;

        while (true)
        {
            if(turn%2 ==  0) CurrentPlayer = _player1;
            else CurrentPlayer = _player2;
            
            Console.WriteLine($"{CurrentPlayer.Name} chance, Please enter row of cell for current move");
            int row =  int.Parse(Console.ReadLine());
            Console.WriteLine($"{CurrentPlayer.Name} chance, Please enter col of cell for current move");
            int col =  int.Parse(Console.ReadLine());

            try
            {
                _board.MakeMove(row, col, CurrentPlayer.Symbol);
            }
            catch (AlreadyOccupied exception)
            {
                Console.WriteLine(exception.Message);
                continue;
            }
            catch (InvalidMove exception)
            {
                Console.WriteLine(exception.Message);
                continue;
            }
            _board.Print();
            if (_winningStrategy.IsWinner(_board, _board.GetCell(row, col)))
            {
                Console.WriteLine($"{CurrentPlayer.Name} wins the game");
                break;
            }
            turn++;
            if (turn == _board.Size * _board.Size)
            {
                Console.WriteLine("The game is a Draw!");
                break;
            }
        }
    }
    
}



public partial class Program
{
    public static void main()
    {
        Console.WriteLine("=== Starting Tic-Tac-Toe Simulation ===");
        Game game = new Game(new StandardWinningStrategy());
        game.Start();
        
        Console.WriteLine("=== Simulation Complete ===");
    }
}