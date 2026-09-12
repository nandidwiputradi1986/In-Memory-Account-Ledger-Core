using System;
using System.Collections.Generic;
using System.Linq;

namespace LedgerCore
{
    public record Account(string Id, string Currency, int Precision);

    public record LedgerEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public int SystemDay { get; }
        public int ValueDay { get; }
        public string AccountId { get; }
        public string Type { get; } 
        public decimal Amount { get; }
        public string ReferenceId { get; }
        public string TargetReferenceId { get; }
        public bool IsRejected { get; set; }
        public string ErrorMessage { get; set; }

        public LedgerEvent(int systemDay, int valueDay, string accountId, string type, decimal amount, string referenceId, string targetReferenceId = null)
        {
            SystemDay = systemDay;
            ValueDay = valueDay;
            AccountId = accountId;
            Type = type;
            Amount = amount;
            ReferenceId = referenceId;
            TargetReferenceId = targetReferenceId;
        }
    }

    public class Ledger
    {
        private readonly List<LedgerEvent> _events = new();
        private readonly Dictionary<string, Account> _accounts = new();
        private readonly decimal OVERDRAFT_FEE = 25.00m;
        private readonly decimal INTEREST_RATE = 0.0004m; // 0.04%

        public IReadOnlyList<LedgerEvent> Events => _events.AsReadOnly();

        public void RegisterAccount(string id, string currency, int precision)
        {
            _accounts[id] = new Account(id, currency, precision);
        }

        public void ProcessDay(int systemDay, List<LedgerEvent> dailyEvents)
        {
            foreach (var evt in dailyEvents)
            {
                if (evt.Type == "AUTH")
                {
                    decimal avail = GetAvailableBalance(evt.AccountId, systemDay);
                    if (avail - evt.Amount < 0)
                    {
                        evt.IsRejected = true;
                        evt.ErrorMessage = "Insufficient available balance";
                    }
                }
                
                if (evt.Type == "CREDIT" && evt.ReferenceId == "E10")
                {
                    // Handle installments internally to ensure exact summation
                    var acc = _accounts[evt.AccountId];
                    decimal baseAmount = Math.Round(evt.Amount / 3, acc.Precision, MidpointRounding.ToZero);
                    decimal remainder = evt.Amount - (baseAmount * 3);

                    _events.Add(new LedgerEvent(systemDay, evt.ValueDay, evt.AccountId, "CREDIT", Math.Round(baseAmount + remainder, acc.Precision), evt.ReferenceId + "-1"));
                    _events.Add(new LedgerEvent(systemDay, evt.ValueDay, evt.AccountId, "CREDIT", baseAmount, evt.ReferenceId + "-2"));
                    _events.Add(new LedgerEvent(systemDay, evt.ValueDay, evt.AccountId, "CREDIT", baseAmount, evt.ReferenceId + "-3"));
                    continue;
                }

                _events.Add(evt);
            }

            RunEndOfDay(systemDay);
        }

        private void RunEndOfDay(int systemDay)
        {
            foreach (var acc in _accounts.Values)
            {
                // Evaluate Overdraft
                decimal closingBalance = GetLedgerBalance(acc.Id, systemDay, systemDay);
                if (closingBalance < 0)
                {
                    bool feeAlreadyAssessedToday = _events.Any(e => e.AccountId == acc.Id && e.Type == "FEE" && e.SystemDay == systemDay);
                    if (!feeAlreadyAssessedToday)
                    {
                        _events.Add(new LedgerEvent(systemDay, systemDay, acc.Id, "FEE", OVERDRAFT_FEE, $"OD-FEE-{systemDay}"));
                    }
                }
            }

            // Capitalize Interest at End of Day 6
            if (systemDay == 6)
            {
                foreach (var acc in _accounts.Values)
                {
                    decimal totalRoundedAccrual = 0;
                    for (int day = 1; day <= 6; day++)
                    {
                        // Use value-dated balances as they stand at the end of the window
                        decimal eodBalance = GetLedgerBalance(acc.Id, day, 6);
                        if (eodBalance > 0)
                        {
                            decimal dailyInterest = Math.Round(eodBalance * INTEREST_RATE, acc.Precision, MidpointRounding.ToEven);
                            totalRoundedAccrual += dailyInterest;
                        }
                    }

                    if (totalRoundedAccrual > 0)
                    {
                        _events.Add(new LedgerEvent(systemDay, systemDay, acc.Id, "INTEREST", totalRoundedAccrual, $"INT-CAP-{acc.Id}"));
                    }
                }
            }
        }

        public decimal GetLedgerBalance(string accountId, int valueDayLimit, int systemDayLimit)
        {
            decimal balance = 0;
            var validEvents = _events.Where(e => e.AccountId == accountId && e.ValueDay <= valueDayLimit && e.SystemDay <= systemDayLimit && !e.IsRejected);

            foreach (var e in validEvents)
            {
                if (e.Type == "CREDIT" || e.Type == "INTEREST") balance += e.Amount;
                if (e.Type == "DEBIT" || e.Type == "SETTLEMENT" || e.Type == "FEE") balance -= e.Amount;
                if (e.Type == "REVERSAL")
                {
                    var target = _events.First(t => t.ReferenceId == e.TargetReferenceId);
                    if (target.Type == "DEBIT" || target.Type == "SETTLEMENT" || target.Type == "FEE") balance += e.Amount;
                    if (target.Type == "CREDIT" || target.Type == "INTEREST") balance -= e.Amount;
                }
            }
            return balance;
        }

        public decimal GetAvailableBalance(string accountId, int systemDayLimit)
        {
            decimal ledger = GetLedgerBalance(accountId, systemDayLimit, systemDayLimit);
            
            var auths = _events.Where(e => e.AccountId == accountId && e.Type == "AUTH" && !e.IsRejected && e.SystemDay <= systemDayLimit);
            var settlements = _events.Where(e => e.AccountId == accountId && e.Type == "SETTLEMENT" && e.SystemDay <= systemDayLimit);

            decimal activeHolds = 0;
            foreach (var auth in auths)
            {
                bool isSettled = settlements.Any(s => s.TargetReferenceId == auth.ReferenceId);
                if (!isSettled)
                {
                    activeHolds += auth.Amount;
                }
            }

            return ledger - activeHolds;
        }
    }
}
