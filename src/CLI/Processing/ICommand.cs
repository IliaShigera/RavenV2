namespace Raven.CLI.Processing;

internal interface ICommand
{
    string Name { get; }
    
    Task ExecuteAsync(string[] args);
}