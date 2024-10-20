namespace ConsoleBot.Data
{
    internal class ConsoleLineExtractor
    {
        internal static string GetLineFromConsole(string question)
        {
            Console.WriteLine(question);
            var line = Console.ReadLine();
            return line;
        }
    }
}
