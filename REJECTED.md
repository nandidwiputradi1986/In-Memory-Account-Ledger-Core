# Rejected Acceptance Criteria

The following provided acceptance criteria were explicitly refused due to violating core ledger mechanics or explicit rules:

1.  **"E7 causes exactly one overdraft fee to be assessed, on Day 2."**
    *   *Reason*: Rejected. E7 is not introduced to the ledger until Day 5. An append-only ledger processing Day 2 cannot assess a fee for an event that hasn't arrived yet. The fee is assessed on Day 5.
2.  **"Any settlement referencing an authorization ID not present in the ledger must be rejected and the funds must not leave the account."**
    *   *Reason*: Rejected. A standard unmapped settlement (force-post) must clear. Rejecting standard network clearings breaks downstream reconciliation. E6 (Auth-Z) is accepted as a direct ledger debit.
3.  **"After E9, all balances and fees return to their pre-E7 values."**
    *   *Reason*: Rejected. While the ledger balance curve is repaired by the reversal of E7, the overdraft fee assessed on Day 5 is an independent cascade event. It remains in the ledger unless explicitly targeted by a `FEE_REVERSAL`.
4.  **"The three BHD instalments in E10 must each be BHD 3.334."**
    *   *Reason*: Rejected. 3.334 + 3.334 + 3.334 = 10.002. This violates the law of mass conservation. The distribution must be truncated and remainder applied (e.g., 3.334, 3.333, 3.333).
5.  **"If the rounded daily interest accruals do not sum to the capitalized total, the remainder is discarded."**
    *   *Reason*: Rejected. The non-negotiable rules explicitly demand they sum exactly. Discarding remainders creates a ledger mismatch.

*(Note: The other three criteria regarding Day 2 balance evaluation on Day 5, Auth-A settlement, and Auth-B hold mechanics were factually correct and implemented as intended).*
