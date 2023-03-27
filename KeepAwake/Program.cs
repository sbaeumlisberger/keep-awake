using KeepAwake;

Console.WriteLine("Your computer keeps awake until this app is closed.");
Console.WriteLine("Press any key to exit");

var powerRequest = PowerManagement.CreatePowerRequest(PowerRequestType.Execution, "keep-awake");

Console.ReadLine();

powerRequest.Dispose();
