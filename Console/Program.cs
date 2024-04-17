using System.Globalization;
using Application;
using Console;
using Microsoft.Extensions.DependencyInjection;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

var collection = new ServiceCollection();
collection.AddApplicationServices();
collection.AddSingleton<FileBasedLinearApproximator>();

var serviceProvider = collection.BuildServiceProvider();

serviceProvider.GetRequiredService<FileBasedLinearApproximator>().Act();