// Copyright (c) DGP Studio. All rights reserved.
// Licensed under the MIT license.

using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.Management.Deployment;

namespace Snap.Hutao.Updater;

internal static class Program
{
    internal static async Task<int> Main(string[] args)
    {
        Option<string> packagePath = new(
            "--package-path",
            () => Path.Combine(AppContext.BaseDirectory, "Snap.Hutao.msix"),
            "The path of the package to be deployed.");
        Option<string> familyName = new(
            "--family-name",
            "The family name of the app to be updated."
            );

        string description = $@"
            Snap Hutao Updater
            Copyright (c) DGP Studio. All rights reserved.
            ";
        RootCommand root = new(description);
        root.AddOption(packagePath);
        root.AddOption(familyName);

        root.SetHandler(AddPackageAsync, packagePath, familyName);

        return await root.InvokeAsync(args);
    }

    private static async Task AddPackageAsync(string path, string? name)
    {
        Console.WriteLine($"""
            PackagePath: {path}
            FamilyName: {name}
            ------------------------------------------------------------
            """);

        if (!File.Exists(path))
        {
            Console.WriteLine($"Package file not found.");
            return;
        }

        try
        {
            Console.WriteLine("Initializing PackageManager...");
            PackageManager packageManager = new();
            AddPackageOptions addPackageOptions = new();

            Console.WriteLine("Start deploying...");
            IProgress<DeploymentProgress> progress = new Progress<DeploymentProgress>(p =>
            {
                Console.WriteLine($"[Deploying]: Progress: {p.percentage} State: {p.state}");
            });
            DeploymentResult result = await packageManager.AddPackageByUriAsync(new Uri(path), addPackageOptions).AsTask(progress);

            if (result.IsRegistered)
            {
                Console.WriteLine("Package deployed.");
                if (string.IsNullOrEmpty(name))
                {
                    Console.WriteLine("FamilyName not provided, will not launch the app.");
                }
                else
                {
                    Console.WriteLine("Starting app...");
                    Process.Start(new ProcessStartInfo()
                    {
                        UseShellExecute = true,
                        FileName = $@"shell:AppsFolder\{name}!App",
                    });
                }
            }
            else
            {
                Console.WriteLine($"""
                    ActivityId: {result.ActivityId}
                    ExtendedErrorCode: {result.ExtendedErrorCode}
                    ErrorText: {result.ErrorText}

                    Exit in 10 seconds...
                    """);

                await Task.Delay(10000);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"""
                Exception when deploying package:
                {ex}

                Exit in 10 seconds...
                """);

            await Task.Delay(10000);
        }
    }
}