# Ambiguities and Resolutions

1. **Backdated Events & Overdraft Assessment Timing**
   * *Ambiguity*: E7 is a backdated debit posted on Day 5 with value_date Day 2. Does the system retroactively charge overdraft fees for Day 2, Day 3, and Day 4?
   * *Resolution*: Evaluated strictly on the *System Day*. The rule says "assessed once per day... when that day's closing ledger balance... is negative". An append-only ledger on Day 2 did not know about E7. Therefore, the OD fee is evaluated on the day the backdated transaction enters the system (Day 5), against the current recalculated balance.
2. **Settlements Without Prior Authorizations (Force Posts)**
   * *Ambiguity*: E6 Settles Auth-Z, which does not exist in the ledger.
   * *Resolution*: Standard core banking protocols treat unmatched settlements as direct debits (force posting). Instead of rejecting the transaction entirely, it is accepted and processed immediately as a ledger debit.
3. **Interest Accrual Rule Validation**
   * *Ambiguity*: "The rounded daily accruals must sum exactly to the capitalized total." Does this require complex daily penny remainder distributions?
   * *Resolution*: Rather than maintaining a rolling delta, `CapitalizedTotal = Sum(RoundedDailyAccruals)`. By summing the already-rounded daily figures at the EOD 6 capitalization step, we guarantee an exact mathematical match by definition.
4. **Reversal of Backdated Events and Value-Dated Interest**
   * *Ambiguity*: E9 reverses E7 on Day 6. Does interest calculation for Day 5 use the negative balance or the positive balance?
   * *Resolution*: Financial interest utilizes the *value-dated balance curve*. Because E9 reverses E7 back to Day 2, the value curve from Day 2 to Day 6 is repaired as if E7 never existed. The Day 6 capitalization dynamically reconstructs this curve.
