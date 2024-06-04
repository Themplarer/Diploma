using System.Globalization;
using Application;
using Console;
using FunctionParser;
using Microsoft.Extensions.DependencyInjection;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var collection = new ServiceCollection();
collection.AddApplicationServices();
collection.AddParser();
collection.AddSingleton<FileBasedLinearApproximator>();

var serviceProvider = collection.BuildServiceProvider();

serviceProvider.GetRequiredService<FileBasedLinearApproximator>().Act();