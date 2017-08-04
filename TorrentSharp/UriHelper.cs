using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TorrentSharp
{
    internal static class UriHelper
    {
        internal static readonly char[] HexChars = "0123456789abcdef".ToCharArray();

        internal static void AddParam(ref string url, string name, string value)
        {
            url = $"{url}{(url.Contains("?") ? "&" : "?")}{name}={value}";
        }

        internal static string UrlEncode(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            MemoryStream result = new MemoryStream(bytes.Length);
            foreach (byte t in bytes)
                UrlEncodeChar((char) t, result, false);
            return Encoding.ASCII.GetString(result.ToArray());
        }

        internal static byte[] UrlDecode(string s)
        {
            if (null == s)
                return null;

            Encoding e = Encoding.UTF8;
            if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
                return e.GetBytes(s);

            long len = s.Length;
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < len; i++)
            {
                char ch = s[i];
                if (ch == '%' && i + 2 < len && s[i + 1] != '%')
                {
                    int xchar;
                    if (s[i + 1] == 'u' && i + 5 < len)
                    {
                        // unicode hex sequence
                        xchar = GetChar(s, i + 2, 4);
                        if (xchar != -1)
                        {
                            WriteCharBytes(bytes, (char) xchar, e);
                            i += 5;
                        }
                        else
                            WriteCharBytes(bytes, '%', e);
                    }
                    else if ((xchar = GetChar(s, i + 1, 2)) != -1)
                    {
                        WriteCharBytes(bytes, (char) xchar, e);
                        i += 2;
                    }
                    else
                        WriteCharBytes(bytes, '%', e);

                    continue;
                }

                WriteCharBytes(bytes, ch == '+' ? ' ' : ch, e);
            }

            return bytes.ToArray();
        }

        private static void UrlEncodeChar(char c, Stream result, bool isUnicode)
        {
            if (c > ' ' && NotEncoded(c))
            {
                result.WriteByte((byte) c);
                return;
            }
            if (c == ' ')
            {
                result.WriteByte((byte) '+');
                return;
            }
            if ((c < '0') ||
                (c < 'A' && c > '9') ||
                (c > 'Z' && c < 'a') ||
                (c > 'z'))
            {
                if (isUnicode && c > 127)
                {
                    result.WriteByte((byte) '%');
                    result.WriteByte((byte) 'u');
                    result.WriteByte((byte) '0');
                    result.WriteByte((byte) '0');
                }
                else
                    result.WriteByte((byte) '%');

                int idx = c >> 4;
                result.WriteByte((byte) HexChars[idx]);
                idx = c & 0x0F;
                result.WriteByte((byte) HexChars[idx]);
            }
            else
                result.WriteByte((byte) c);
        }

        private static int GetChar(string str, int offset, int length)
        {
            int val = 0;
            int end = length + offset;
            for (int i = offset; i < end; i++)
            {
                char c = str[i];
                if (c > 127)
                    return -1;

                int current = GetInt((byte) c);
                if (current == -1)
                    return -1;
                val = (val << 4) + current;
            }

            return val;
        }

        private static int GetInt(byte b)
        {
            char c = (char) b;
            if (c >= '0' && c <= '9')
                return c - '0';

            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;

            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;

            return -1;
        }

        private static bool NotEncoded(char c)
        {
            return c == '!' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_' || c == '\'';
        }

        private static void WriteCharBytes(List<byte> buf, char ch, Encoding e)
        {
            if (ch > 255)
                buf.AddRange(e.GetBytes(new[] {ch}));
            else
                buf.Add((byte) ch);
        }
    }
}