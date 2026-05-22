using Avalonia.Controls;

namespace CourseWork_db.Helpers;

public static class ViewStatusHelper
{
    public static void Set(TextBlock block, string text, bool isError = false)
    {
        block.Text = text;
        block.Classes.Clear();
        if (!string.IsNullOrEmpty(text))
            block.Classes.Add(isError ? "status-err" : "status-ok");
    }
}
