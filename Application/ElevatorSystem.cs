namespace Application
{
    // --- 1. Enums ---
    public enum Direction
    {
        Up,
        Down,
        None
    }

    public enum ElevatorState
    {
        Idle,
        Moving,
        Maintenance
    }

    // --- 2. Interfaces ---
    public interface IRoutingStrategy
    {
        Elevator? FindBestElevator(List<Elevator> elevators, int requestedFloor, Direction requestedDirection);
    }

    // --- 3. Core Entities ---
    public class Elevator
    {
        public int Id { get; }
        public int CurrentFloor { get; private set; }
        public Direction CurrentDirection { get; private set; }
        public ElevatorState State { get; private set; }

        // HashSet gives us O(1) lookup to check if we need to open doors at the current floor
        private readonly HashSet<int> _destinations;

        public Elevator(int id)
        {
            Id = id;
            CurrentFloor = 0; // Assume ground floor start
            CurrentDirection = Direction.None;
            State = ElevatorState.Idle;
            _destinations = new HashSet<int>();
        }

        public void AddDestination(int floor)
        {
            _destinations.Add(floor);
            
            if (State == ElevatorState.Idle)
            {
                State = ElevatorState.Moving;
                CurrentDirection = floor > CurrentFloor ? Direction.Up : Direction.Down;
            }
        }

        public void RemoveDestination(int floor)
        {
            _destinations.Remove(floor);
            
            if (_destinations.Count == 0)
            {
                State = ElevatorState.Idle;
                CurrentDirection = Direction.None;
            }
        }

        public IReadOnlyCollection<int> GetDestinations() => _destinations;

        // Called by the system tick to move the elevator one floor at a time
        public void Move()
        {
            if (State == ElevatorState.Moving)
            {
                CurrentFloor += (CurrentDirection == Direction.Up) ? 1 : -1;
                Console.WriteLine($"[Movement] Elevator {Id} passing floor {CurrentFloor}...");
            }
        }
    }

    // --- 4. Concrete Strategies ---
    public class NearestElevatorStrategy : IRoutingStrategy
    {
        public Elevator? FindBestElevator(List<Elevator> elevators, int requestedFloor, Direction requestedDirection)
        {
            Elevator? bestElevator = null;
            int minDistance = int.MaxValue;

            foreach (var elevator in elevators)
            {
                bool isIdle = elevator.State == ElevatorState.Idle;
                
                // Is the elevator already moving towards the user in the correct direction?
                bool isMovingTowardsUser = 
                    (elevator.CurrentDirection == Direction.Up && elevator.CurrentFloor <= requestedFloor) ||
                    (elevator.CurrentDirection == Direction.Down && elevator.CurrentFloor >= requestedFloor);

                if (isIdle || isMovingTowardsUser)
                {
                    int distance = Math.Abs(elevator.CurrentFloor - requestedFloor);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestElevator = elevator;
                    }
                }
            }

            // Fallback: If no optimal moving elevator is found, grab the first idle one
            return bestElevator ?? elevators.FirstOrDefault(e => e.State == ElevatorState.Idle);
        }
    }

    // --- 5. The Central Dispatcher (Thread-Safe) ---
    public class ElevatorDispatcher
    {
        private readonly List<Elevator> _elevators;
        private readonly IRoutingStrategy _routingStrategy;
        private readonly object _dispatchLock = new object();

        public ElevatorDispatcher(int numberOfElevators, IRoutingStrategy routingStrategy)
        {
            _elevators = new List<Elevator>();
            for (int i = 1; i <= numberOfElevators; i++)
            {
                _elevators.Add(new Elevator(i));
            }
            _routingStrategy = routingStrategy;
        }

        // Simulates a user pressing the Up/Down button in the hallway
        public void RequestElevator(int floor, Direction direction)
        {
            lock (_dispatchLock)
            {
                Elevator? bestElevator = _routingStrategy.FindBestElevator(_elevators, floor, direction);

                if (bestElevator != null)
                {
                    Console.WriteLine($"[Hallway] User on Floor {floor} requested to go {direction}. Assigning Elevator {bestElevator.Id}.");
                    bestElevator.AddDestination(floor);
                }
                else
                {
                    Console.WriteLine($"[Hallway] User on Floor {floor} requested to go {direction}. All elevators busy. Queued.");
                }
            }
        }

        // Simulates a user pressing a floor button inside the cabin
        public void SelectDestinationInElevator(int elevatorId, int destinationFloor)
        {
            var elevator = _elevators.FirstOrDefault(e => e.Id == elevatorId);
            if (elevator != null)
            {
                Console.WriteLine($"[Cabin] User in Elevator {elevatorId} pressed Floor {destinationFloor}.");
                elevator.AddDestination(destinationFloor);
            }
        }

        // Advances the state of the entire system by one "tick"
        public void Step()
        {
            foreach (var elevator in _elevators)
            {
                elevator.Move();
                
                if (elevator.GetDestinations().Contains(elevator.CurrentFloor))
                {
                    Console.WriteLine($"[Arrival] *** Elevator {elevator.Id} opened doors at Floor {elevator.CurrentFloor} ***");
                    elevator.RemoveDestination(elevator.CurrentFloor);
                }
            }
        }
    }

    // --- 6. Automated Test Runner ---
    public partial class Program
    {
        public static void main3()
        {
            Console.WriteLine("=== Elevator System Simulation ===\n");

            // Initialize a building with 2 elevators using the Nearest strategy
            IRoutingStrategy strategy = new NearestElevatorStrategy();
            ElevatorDispatcher dispatcher = new ElevatorDispatcher(2, strategy);

            // Time T=0: User on floor 3 wants to go Up
            dispatcher.RequestElevator(3, Direction.Up);
            
            // Time T=0: User on floor 8 wants to go Down
            dispatcher.RequestElevator(8, Direction.Down);

            Console.WriteLine("\n--- Simulating 3 seconds of movement ---");
            for (int time = 0; time < 3; time++) { dispatcher.Step(); }

            Console.WriteLine("\n--- Passengers board and select destinations ---");
            // Elevator 1 arrived at floor 3. User gets in and presses floor 5.
            dispatcher.SelectDestinationInElevator(1, 5);

            Console.WriteLine("\n--- Simulating 5 seconds of movement ---");
            for (int time = 0; time < 5; time++) { dispatcher.Step(); }

            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}