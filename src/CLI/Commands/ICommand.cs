namespace Raven.CLI.Commands;

internal interface ICommand
{
    string Name { get; }
    
    Task ExecuteAsync(string[] args);
}