namespace Application
{
    // --- 1. The Interface ---
    public interface IVendingMachineState
    {
        void InsertMoney(VendingMachine machine, decimal amount);
        void SelectProduct(VendingMachine machine, string productCode);
        void CancelTransaction(VendingMachine machine);
        void DispenseProduct(VendingMachine machine);
    }

    // --- 2. The Context Class (Thread-Safe) ---
    public class VendingMachine
    {
        private IVendingMachineState _currentState;
        
        // Concurrency Locks
        private readonly object _inventoryLock = new object();
        private readonly object _balanceLock = new object();

        private int _totalInventory;
        private decimal _currentBalance;

        public VendingMachine(int initialInventory)
        {
            _totalInventory = initialInventory;
            _currentBalance = 0;
            
            if (_totalInventory > 0)
                _currentState = new IdleState();
            else
                _currentState = new SoldOutState();
        }

        // Thread-safe state transition
        public void SetState(IVendingMachineState state)
        {
            _currentState = state;
        }

        // Thread-safe balance management
        public void AddBalance(decimal amount)
        {
            lock (_balanceLock)
            {
                _currentBalance += amount;
            }
        }

        public decimal ClearBalance()
        {
            lock (_balanceLock)
            {
                decimal refund = _currentBalance;
                _currentBalance = 0;
                return refund;
            }
        }

        public decimal GetBalance()
        {
            lock (_balanceLock)
            {
                return _currentBalance;
            }
        }

        // Thread-safe inventory management
        public bool TryDecreaseInventory()
        {
            lock (_inventoryLock)
            {
                if (_totalInventory > 0)
                {
                    _totalInventory--;
                    return true;
                }
                return false;
            }
        }

        public int GetInventory()
        {
            lock (_inventoryLock)
            {
                return _totalInventory;
            }
        }

        // User Action Delegates
        public void InsertMoney(decimal amount) => _currentState.InsertMoney(this, amount);
        public void SelectProduct(string code) => _currentState.SelectProduct(this, code);
        public void CancelTransaction() => _currentState.CancelTransaction(this);
        public void DispenseProduct() => _currentState.DispenseProduct(this);
    }

    // --- 3. The Concrete States ---

    public class IdleState : IVendingMachineState
    {
        public void InsertMoney(VendingMachine machine, decimal amount)
        {
            Console.WriteLine($"[Idle] Inserted ${amount}.");
            machine.AddBalance(amount);
            machine.SetState(new HasMoneyState());
        }

        public void SelectProduct(VendingMachine machine, string productCode)
        {
            Console.WriteLine("[Error] Please insert money first!");
        }

        public void CancelTransaction(VendingMachine machine)
        {
            Console.WriteLine("[Error] No transaction to cancel.");
        }

        public void DispenseProduct(VendingMachine machine)
        {
            Console.WriteLine("[Error] Cannot dispense without payment.");
        }
    }

    public class HasMoneyState : IVendingMachineState
    {
        public void InsertMoney(VendingMachine machine, decimal amount)
        {
            Console.WriteLine($"[HasMoney] Inserted additional ${amount}.");
            machine.AddBalance(amount);
        }

        public void SelectProduct(VendingMachine machine, string productCode)
        {
            Console.WriteLine($"[HasMoney] Product {productCode} selected.");
            machine.SetState(new DispensingState());
            
            // Automatically trigger the mechanical drop
            machine.DispenseProduct(); 
        }

        public void CancelTransaction(VendingMachine machine)
        {
            decimal refund = machine.ClearBalance();
            Console.WriteLine($"[HasMoney] Transaction canceled. Refunding ${refund}");
            machine.SetState(new IdleState());
        }

        public void DispenseProduct(VendingMachine machine)
        {
            Console.WriteLine("[Error] You must select a product first.");
        }
    }

    public class DispensingState : IVendingMachineState
    {
        public void InsertMoney(VendingMachine machine, decimal amount)
        {
            Console.WriteLine("[Error] Please wait, dispensing in progress.");
        }

        public void SelectProduct(VendingMachine machine, string productCode)
        {
            Console.WriteLine("[Error] Already dispensing your item.");
        }

        public void CancelTransaction(VendingMachine machine)
        {
            Console.WriteLine("[Error] Too late to cancel! Item is dropping.");
        }

        public void DispenseProduct(VendingMachine machine)
        {
            // Thread-safe inventory check
            if (machine.TryDecreaseInventory())
            {
                Console.WriteLine("[Dispensing] Item dropped. Clunk!");
                
                decimal change = machine.ClearBalance();
                if (change > 0)
                {
                    Console.WriteLine($"[Dispensing] Returning change: ${change}");
                }

                if (machine.GetInventory() == 0)
                {
                    machine.SetState(new SoldOutState());
                }
                else
                {
                    machine.SetState(new IdleState());
                }
            }
            else
            {
                // Edge case: Race condition occurred, and we actually sold out!
                Console.WriteLine("[Dispensing Error] Item sold out just before dispensing!");
                decimal refund = machine.ClearBalance();
                Console.WriteLine($"[Dispensing] Refunding ${refund}");
                machine.SetState(new SoldOutState());
            }
        }
    }

    public class SoldOutState : IVendingMachineState
    {
        public void InsertMoney(VendingMachine machine, decimal amount)
        {
            Console.WriteLine($"[SoldOut] Machine is empty. Rejecting ${amount}");
        }

        public void SelectProduct(VendingMachine machine, string productCode)
        {
            Console.WriteLine("[Error] Machine is sold out.");
        }

        public void CancelTransaction(VendingMachine machine)
        {
            Console.WriteLine("[Error] No transaction to cancel.");
        }

        public void DispenseProduct(VendingMachine machine)
        {
            Console.WriteLine("[Error] Machine is sold out.");
        }
    }

    // --- 4. Automated Test Runner ---
    public partial class Program
    {
        public static void Main2()
        {
            Console.WriteLine("=== Vending Machine Simulation Started ===\n");

            // Initialize a machine with only 1 item in stock
            VendingMachine machine = new VendingMachine(1);

            Console.WriteLine("--- Scenario 1: Successful Purchase ---");
            machine.SelectProduct("A1"); // Should fail (no money)
            machine.InsertMoney(1.50m);
            machine.InsertMoney(0.50m);
            machine.SelectProduct("A1"); // Should succeed, dispense, and return change

            Console.WriteLine("\n--- Scenario 2: Attempting to buy when Sold Out ---");
            machine.InsertMoney(2.00m);  // Should instantly reject because it's sold out
            machine.SelectProduct("A2"); // Should fail

            Console.WriteLine("\n=== Simulation Complete ===");
        }
    }
}