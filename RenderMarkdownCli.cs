using System;
using System.Collections.Generic;
using System.IO;
using Misbah.Core.Services;

class RenderMarkdownCli
{
    static void Main(string[] args)
    {
        string markdownPath = "README.md";
        if (!File.Exists(markdownPath))
        {
            Console.WriteLine($"File not found: {markdownPath}");
            return;
        }
        string markdown = File.ReadAllText(markdownPath);
        var renderer = new MarkdownRenderer();
        var html = renderer.RenderFull(markdown, out List<int> taskLines);
        File.WriteAllText("RenderedContent.html", html);
        Console.WriteLine("Rendered HTML written to RenderedContent.html");
        // Inspect for common issues
        if (html.Contains("<ul><ul>") || html.Contains("<li><ul>") || html.Contains("<ul></ul>"))
        {
            Console.WriteLine("[Warning] Nested or empty lists detected in HTML output.");
        }
        if (html.Contains("<span class='md-highlight'>") == false && markdown.Contains("=="))
        {
            Console.WriteLine("[Warning] Highlight syntax (==) not rendered as expected.");
        }
        if (html.Contains("<input type='checkbox' class='md-task'"))
        {
            Console.WriteLine("[Info] Checkbox rendering detected.");
        }
        else if (markdown.Contains("- [ ]") || markdown.Contains("- [x]"))
        {
            Console.WriteLine("[Warning] Checkbox markdown not rendered as input checkbox.");
        }
    }
}
