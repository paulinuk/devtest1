Trade Extractor

SETUP/UNINSTALLATION
After compilation in either release or debug mode, the relevant command file can be run via an administrator command prompt, SetupDebug or SetupRelease
To uninstall the service the relevant command file can be run via an administrator command prompt, UninstallDebug or UninstallRelease
The test project path needs to be changed away from my machine and replaced with your local nuget package folder, same applies for the RunTests.cmd file

OVERVIEW:
1) Quartz is used to ensure that trades are extracted every x minute(s).  This is guarenteed to run at the required interval
2) AutoFac is used for DI
3) Logging is carried out via Serilog.  Logs are written to log files in the Logs folder of the output directory for the service.  One log per day
4) If running the extractor via Visual Studio, the log text is also written to the console
5) Fody.LoadAssembliesOnStartup is used to ensure that all assemblies are loaded ready for AutoFac to scan.  

NOTE
1) If running the extractor via Visual Studio the service needs to be stopped
2) DevTest.Trades.WindowsService should be set as the startup project if it is not already
3) An error is raised if there are no trades, however, in a production scenario this would probably be removed, or enhanced, just in case there no trades is actually ok, 
for example if the system is not 24 hours and there is a chance of no trades being executed

STRUCTURE:
DevTest.Trades.Common - Contains constant for Quartz key and AutoFacHelper - This would be available to other assemblies where required
DevTest.Trades.Services - Contains business logic services
DevTest.Trades.WindowsService - Console application which is also the Windows service when installed by TopShelf
DevTest.Trades.Tests - Tests for the business logic (Using XUnit with FluentAssertions for ease of reading)

Obviously, most of these assemblies could be combined, but I prefer to seperate where possible as this can help to future proof.  
Splitting Services and WindowsService would give flexibility to have the TradeExtractor running in non Windows Service/Console context.  I dont like placing
my business logic in a host application as this ties me down to that specific implementation