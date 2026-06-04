using System;
using System.IO;
using System.Linq;
using System.Reflection;

var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages", "microsoft.azure.functions.worker.applicationinsights", "2.50.0", "lib", "netstandard2.0", "Microsoft.Azure.Functions.Worker.ApplicationInsights.dll");
var asm = Assembly.LoadFrom(path);
foreach (var type in asm.GetTypes().Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Any(m => m.Name.Contains("ConfigureFunctionsApplicationInsights"))))
{
    Console.WriteLine(type.FullName);
    foreach (var m in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance).Where(m => m.Name.Contains("ConfigureFunctionsApplicationInsights")))
    {
        Console.WriteLine($"  {m}");
    }
}
