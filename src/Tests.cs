/* FAILING TEST INLINE ANNOTATION
 * 
 * Test: ReversingBackdatedDebit_DoesNot_ReverseCascadingOverdraftFees()
 * 
 * Why it fails: Developers often naively assume that appending a REVERSAL 
 * for a backdated event restores the ledger to the exact pristine state as if 
 * the event never happened. While it fixes the value-dated balance curve for 
 * interest calculations, it *does not* automatically cascade to delete or reverse
 * overdraft fees triggered when the system day processed the event. 
 * An append-only ledger requires a separate explicit FEE REVERSAL event.
 */
using System.Collections.Generic;

public class LedgerTests
{
    public void ReversingBackdatedDebit_DoesNot_ReverseCascadingOverdraftFees()
    {
        // Arrange
        var ledger = new LedgerCore.Ledger();
        ledger.RegisterAccount("ACC-001", "AED", 2);
        
        // Setup initial positive balance, then post a backdated debit that causes an OD fee
        ledger.ProcessDay(1, new List<LedgerCore.LedgerEvent> { new (1, 1, "ACC-001", "CREDIT", 100m, "E1") });
        ledger.ProcessDay(2, new List<LedgerCore.LedgerEvent> { new (2, 1, "ACC-001", "DEBIT", 200m, "E2") });
        // Day 2 EOD evaluates OD fee -> balance is -100. Fee of 25 is applied. Balance becomes -125.
        
        // Act
        // Day 3 Reversal of the debit
        ledger.ProcessDay(3, new List<LedgerCore.LedgerEvent> { new (3, 1, "ACC-001", "REVERSAL", 200m, "E3", "E2") });
        
        // Assert
        decimal balance = ledger.GetLedgerBalance("ACC-001", 3, 3);
        
        // Fails here: expected 100m, actual is 75m because the 25m fee remains.
        if (balance != 100m) throw new System.Exception($"Expected 100.00, got {balance}"); 
    }
}
