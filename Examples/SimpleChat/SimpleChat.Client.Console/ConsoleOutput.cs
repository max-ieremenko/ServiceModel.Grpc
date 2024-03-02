namespace SimpleChat.Client.Console;

internal sealed class ConsoleOutput
{
    private ConsoleTextBox? _textBox;

    public string? Ask(string query)
    {
        _textBox = new ConsoleTextBox(query);
        _textBox.Show();

        var result = _textBox.Ask();
        _textBox = null;

        return result;
    }

    public void AppendLine(string text)
    {
        if (_textBox == null)
        {
            System.Console.WriteLine(text);
            return;
        }

        _textBox.Hide();
        System.Console.WriteLine(text);
        _textBox.Show();
    }
}