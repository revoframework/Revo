using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace GTRevo.Infrastructure.Globalization.Messages
{
    public class YamlMessageSource : IMessageSource
    {
        private readonly Func<Stream> getStreamFunc;

        public YamlMessageSource(Func<Stream> getStreamFunc)
        {
            this.getStreamFunc = getStreamFunc;
            LoadMessages();
        }

        public YamlMessageSource(Stream stream)
        {
            this.getStreamFunc = () => stream;
            LoadMessages();
        }

        public ImmutableDictionary<string, string> Messages { get; private set; }

        public bool TryGetMessage(string key, out string message)
        {
            return Messages.TryGetValue(key, out message);
        }

        private void LoadMessages()
        {
            var messagesBuilder = ImmutableDictionary.CreateBuilder<string, string>();

            using (Stream stream = getStreamFunc())
            using (StreamReader reader = new StreamReader(stream))
            {
                List<string> pathStack = new List<string>();
                string path = "";
                int indentSize = -1;
                int lineNumber = 1;
                string line;
                string lastKey = "";

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();

                    int hashIndex = line.IndexOf('#');
                    if (hashIndex != -1)
                    {
                        line = line.Substring(0, hashIndex);
                    }

                    if (line.Length == 0) //trim?
                    {
                        continue;
                    }

                    int indents = CountIndents(line);
                    if (indentSize == -1 && indents != 0)
                    {
                        indentSize = indents;
                    }

                    //quotes
                    if (indents > 0)
                    {
                        if (indents % indentSize != 0)
                        {
                            throw new IOException("Invalid YAML indentation at line: " + lineNumber);
                        }

                        indents = indents / indentSize;
                    }

                    if (indents > pathStack.Count)
                    {
                        if (indents == pathStack.Count + 1 && lastKey?.Length > 0)
                        {
                            pathStack.Add(lastKey);
                            path = GetPathString(pathStack);
                        }
                        else
                        {
                            throw new IOException("Invalid YAML indentation at line: " + lineNumber);
                        }
                    }
                    else if (indents < pathStack.Count)
                    {
                        while (indents < pathStack.Count)
                        {
                            pathStack.RemoveAt(pathStack.Count - 1);
                        }

                        path = GetPathString(pathStack);
                    }

                    line = line.Trim();
                    if (line.Length == 0)
                    {
                        continue;
                    }

                    int colonIndex;

                    if (line.StartsWith("\""))
                    {
                        int quoteIndex = line.IndexOf('"', 1);
                        if (quoteIndex == -1)
                        {
                            throw new IOException("Invalid YAML key at line: " + lineNumber);
                        }

                        lastKey = line.Substring(1, quoteIndex - 1).Trim();
                        colonIndex = line.IndexOf(':', quoteIndex);
                        if (colonIndex == -1)
                        {
                            throw new IOException("Invalid YAML key at line: " + lineNumber);
                        }
                    }
                    else
                    {
                        colonIndex = line.IndexOf(':');
                        if (colonIndex == -1)
                        {
                            throw new IOException("Invalid YAML key at line: " + lineNumber);
                        }

                        lastKey = line.Substring(0, colonIndex).Trim();
                    }

                    string value;

                    if (colonIndex + 1 < line.Length)
                    {
                        value = line.Substring(colonIndex + 1).Trim();
                        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length > 2)
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
                    }
                    else
                    {
                        value = "";
                    }

                    messagesBuilder[path + lastKey] = value;
                    lineNumber++;
                }
            }

            Messages = messagesBuilder.ToImmutable();
        }

        private string GetPathString(List<string> pathStack)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < pathStack.Count; i++)
            {
                builder.Append(pathStack[i]);
                builder.Append('.');
            }

            return builder.ToString();
        }

        private int CountIndents(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != ' ' && line[i] != '\t')
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
