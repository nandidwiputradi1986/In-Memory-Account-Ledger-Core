using System;
using System.Collections.Generic;
using System.Linq;

namespace LedgerCore
{
    public class Program
    {
        public static void Main()
        {
            var ledger = new Ledger();
            ledger.RegisterAccount("ACC-001", "AED", 2);
            ledger.RegisterAccount("ACC-002", "BHD", 3);

            var stream = new Dictionary<int, List<LedgerEvent>>
            {
                { 1, new List<LedgerEvent> {
                    new LedgerEvent(1, 1, "ACC-001", "CREDIT", 1200.00m, "E1"),
                    new LedgerEvent(1, 1, "ACC-001", "DEBIT", 950.00m, "E2")
                }},
                { 2, new List<LedgerEvent> {
                    new LedgerEvent(2, 2, "ACC-001", "AUTH", 200.00m, "Auth-A") // E3
                }},
                { 3, new List<LedgerEvent> {
                    new LedgerEvent(3, 3, "ACC-001", "CREDIT", 400.00m, "E4")
                }},
                { 4, new List<LedgerEvent> {
                    new LedgerEvent(4, 4, "ACC-001", "SETTLEMENT", 185.00m, "E5", "Auth-A"),
                    new LedgerEvent(4, 4, "ACC-001", "SETTLEMENT", 180.00m, "E6", "Auth-Z")
                }},
                { 5, new List<LedgerEvent> {
                    new LedgerEvent(5, 2, "ACC-001", "DEBIT", 620.00m, "E7"),
                    new LedgerEvent(5, 5, "ACC-001", "AUTH", 90.00m, "Auth-B"), // E8
                    new LedgerEvent(5, 5, "ACC-002", "CREDIT", 10.000m, "E10")
                }},
                { 6, new List<LedgerEvent> {
                    new LedgerEvent(6, 2, "ACC-001", "REVERSAL", 620.00m, "E9", "E7")
                }}
            };

            for (int day = 1; day <= 6; day++)
            {
                Console.WriteLine($"\n========== DAY {day} ==========");
                if (stream.ContainsKey(day))
                {
                    ledger.ProcessDay(day, stream[day]);
                }
                else
                {
                    ledger.ProcessDay(day, new List<LedgerEvent>());
                }

                PrintEndOfDayState(ledger, day);
            }
        }

        private static void PrintEndOfDayState(Ledger ledger, int day)
        {
            Console.WriteLine("--- Closing Ledger Balances ---");
            Console.WriteLine($"ACC-001 (AED): {ledger.GetLedgerBalance("ACC-001", day, day):F2}");
            Console.WriteLine($"ACC-002 (BHD): {ledger.GetLedgerBalance("ACC-002", day, day):F3}");

            Console.WriteLine("\n--- Fee Assessments ---");
            var fees = ledger.Events.Where(e => e.Type == "FEE" && e.SystemDay == day).ToList();
            if (fees.Any()) fees.ForEach(f => Console.WriteLine($"{f.AccountId}: {f.Amount} {f.Type} (Ref: {f.ReferenceId})"));
            else Console.WriteLine("None");

            Console.WriteLine("\n--- Authorization States ---");
            var auths = ledger.Events.Where(e => e.Type == "AUTH" && e.SystemDay == day).ToList();
            if (auths.Any()) auths.ForEach(a => Console.WriteLine($"{a.ReferenceId} on {a.AccountId}: {(a.IsRejected ? "REJECTED" : "ACCEPTED")}"));
            else Console.WriteLine("None");

            Console.WriteLine("\n--- Errors ---");
            var errors = ledger.Events.Where(e => e.IsRejected && e.SystemDay == day).ToList();
            if (errors.Any()) errors.ForEach(e => Console.WriteLine($"{e.ReferenceId}: {e.ErrorMessage}"));
            else Console.WriteLine("None");
        }
    }
}
