// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Management.Deployment;

Console.WriteLine("""
    Snap Hutao Updater
    Copyright (c) DGP Studio. All rights reserved.

    """);

if (args.Length != 2)
{
    Console.WriteLine($"""
        Args only have {args.Length} element.
        Press a key to exit.
        """);
    Console.ReadKey();
    return;
}

string familyName = args[0];
string filePath = Path.GetFullPath(args[1]);

Console.WriteLine($"""
    FamilyName: {familyName}
    FilePath: {filePath}

    """);

Console.WriteLine("Initializing PackageManager...");
PackageManager packageManager = new();
Console.WriteLine("Start deploying...\n");
try
{

    DeploymentResult result = await packageManager.AddPackageByUriAsync(new Uri(filePath), new()
    {
        //ForceAppShutdown = true,
    });

    if (result.IsRegistered)
    {
        Console.WriteLine("""
        Deployed.
        Starting Snap Hutao...
        """);
        Process.Start(new ProcessStartInfo()
        {
            UseShellExecute = true,
            FileName = $"shell:AppsFolder\\{familyName}!App",
        });
    }
    else
    {
        Console.WriteLine($"""
        {result.ErrorText}
        You can send error to developers.
        Press a key to exit.
        """);
        Console.ReadKey();
    }
}
catch (COMException ex)
{
    Console.WriteLine(ex.ToString());
    Console.WriteLine("Press a key to exit.");
    Console.ReadKey();
    return;
}