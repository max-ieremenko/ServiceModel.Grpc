using System;
using System.Text;

namespace SimpleChat.Client.Console;

internal sealed class ConsoleTextBox
{
    private readonly string _label;
    private readonly StringBuilder _input;

    public ConsoleTextBox(string label)
    {
        _label = label;
        _input = new StringBuilder();
    }

    private int OutputLength => _label.Length + 1 + _input.Length;

    public void Show()
    {
        System.Console.CursorLeft = 0;

        System.Console.Write(_label + " ");
        System.Console.Write(_input);

        System.Console.CursorLeft = OutputLength;
    }

    public void Hide()
    {
        var textLength = Math.Max(System.Console.CursorLeft, OutputLength);

        System.Console.CursorLeft = 0;
        System.Console.Write(new string(' ', textLength));
        System.Console.CursorLeft = 0;
    }

    public string? Ask()
    {
        while (true)
        {
            var key = System.Console.ReadKey();

            if (key.Key == ConsoleKey.Enter)
            {
                Hide();
                return GetInput();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                Hide();
                
                if (_input.Length > 0)
                {
                    _input.Length--;
                }

                Show();

                continue;
            }

            if (!char.IsControl(key.KeyChar))
            {
                _input.Append(key.KeyChar);
            }
        }
    }

    private string? GetInput()
    {
        var result = _input.ToString().Trim();
        return result.Length == 0 ? null : result;
    }
}