# In-Memory Account Ledger Core
48 hours. Budget roughly 1–2 hours of design thinking, 2 hours of building, and the rest for documentation. AI tools are permitted and expected. The submitted artifacts are your entry ticket; a 45-minute live defense with no AI present is the assessment.

/  
├── src/  
│   ├── LedgerCore.cs  
│   ├── Program.cs  
│   └── Tests.cs  
├── README.md  
├── NUMBERS.md  
├── AMBIGUITIES.md  
├── REJECTED.md  
└── WORKLOG.md  

An append-only, in-memory core banking ledger processing an event stream strictly across 6 system days.

## How to Run
1. Navigate to `src/`.
2. Compile and run using the .NET CLI:
   `dotnet run`
   (Alternatively, paste the contents of `LedgerCore.cs` and `Program.cs` into any C# runner or IDE like LINQPad or Visual Studio).

## Reading the Output
The console will output the state sequentially for `DAY 1` through `DAY 6`. 
Each day block contains:
- **Closing Ledger Balances:** Evaluated for `value_date <= system_day`.
- **Fee Assessments:** Overdrafts processed at the end of the day.
- **Authorization States:** Validated directly against available balances.
- **Errors:** Log of any rejected events (like NSF for auths).
