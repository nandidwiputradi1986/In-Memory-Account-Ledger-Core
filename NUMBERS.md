# Number and Constant Decisions

*   **Overdraft Fee (`25.00m`)**: Strict constraint. We do not halve or negotiate this to 12.50m because the requirement explicitly states it is an absolute 25.00 unit fee applied once daily for negative states.
*   **Interest Rate (`0.0004m`)**: 0.04% converted directly to its decimal equivalent for multiplier usage. Halving this would artificially under-calculate yield.
*   **Precision (AED=`2`, BHD=`3`)**: Represents standard minor units for Emirati Dirham (Fils) and Bahraini Dinar (Fils). Mathematical rounding uses `MidpointRounding.ToEven` (Banker's Rounding) for interest, and `ToZero` internally for installment truncation to guarantee remainders sum strictly to the total.
