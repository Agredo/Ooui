using System;

namespace Ooui
{
    public class JsonConvert
    {
        public static void WriteJsonString (System.IO.TextWriter w, string s)
        {
            w.Write ('\"');
            for (var i = 0; i < s.Length; i++) {
                var c = s[i];
                if (c == '\"') {
                    w.Write ("\\\"");
                }
                else if (c == '\r') {
                    w.Write ("\\r");
                }
                else if (c == '\n') {
                    w.Write ("\\n");
                }
                else if (c == '\t') {
                    w.Write ("\\t");
                }
                else if (c == '\b') {
                    w.Write ("\\b");
                }
                else if (c == '\\') {
                    w.Write ("\\");
                }
                else {
                    w.Write (c);
                }
            }
            w.Write ('\"');
        }

        public static void WriteJsonValue (System.IO.TextWriter w, object value)
        {
            switch(value) {
                case string s:
                    WriteJsonString (w, s);
                    break;
                case Array a:
                    w.Write ('[');
                    var head = "";
                    foreach (var o in a) {
                        w.Write (head);
                        WriteJsonValue (w, o);
                        head = ",";
                    }
                    w.Write (']');
                    break;
                case EventTarget e:
                    w.Write ('\"');
                    w.Write (e.Id);
                    w.Write ('\"');
                    break;
                case Color c:
                    WriteJsonString (w, c.ToString ());
                    break;
                case double d:
                    w.Write (d.ToString (System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case int i:
                    w.Write (i.ToString (System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case float f:
                    w.Write (f.ToString (System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case null:
                    w.Write ("null");
                    break;
                default:
                    w.Write (Newtonsoft.Json.JsonConvert.SerializeObject (value));
                    break;
            }
        }

        public static string SerializeObject (object value)
        {
            using (var sw = new System.IO.StringWriter ()) {
                WriteJsonValue (sw, value);
                return sw.ToString ();
            }
        }

        static object ReadJsonArray (string j, ref int i)
        {
            var values = new System.Collections.Generic.List<object> ();
            i++;
            while (true) {
                while (i < j.Length && (char.IsWhiteSpace (j[i]) || j[i] == ',')) i++;
                if (i >= j.Length) throw new Exception ("JSON Unexpected end");
                if (j[i] == ']') { i++; return values.ToArray (); }
                values.Add (ReadJsonValue (j, ref i));
            }
        }

        static object ReadJsonObject (string json, ref int i)
        {
            var result = new System.Collections.Generic.Dictionary<string, object> ();
            i++;
            while (true) {
                while (i < json.Length && (char.IsWhiteSpace (json[i]) || json[i] == ',')) i++;
                if (i >= json.Length) throw new Exception ("JSON Unexpected end");
                if (json[i] == '}') { i++; return result; }
                if (json[i] != '"') throw new Exception ("JSON Expected property");
                var key = (string)ReadJsonString (json, ref i);
                while (i < json.Length && char.IsWhiteSpace (json[i])) i++;
                if (i >= json.Length || json[i] != ':') throw new Exception ("JSON Expected colon");
                i++;
                result[key] = ReadJsonValue (json, ref i);
            }
        }

        static object ReadJsonString (string j, ref int i)
        {
            if (j[i] != '"') throw new Exception ("JSON Expected string");
            i++;
            var value = new System.Text.StringBuilder ();
            while (i < j.Length) {
                var c = j[i++];
                if (c == '"') return value.ToString ();
                if (c == '\\') {
                    if (i >= j.Length) break;
                    c = j[i++];
                    switch (c) {
                        case 'r': value.Append ('\r'); break;
                        case 'n': value.Append ('\n'); break;
                        case 't': value.Append ('\t'); break;
                        case 'b': value.Append ('\b'); break;
                        case 'f': value.Append ('\f'); break;
                        case 'u':
                            if (i + 4 > j.Length) throw new Exception ("JSON Unexpected end");
                            value.Append ((char)Convert.ToInt32 (j.Substring (i, 4), 16));
                            i += 4;
                            break;
                        default: value.Append (c); break;
                    }
                }
                else value.Append (c);
            }
            throw new Exception ("JSON Unexpected end");
        }

        static object ReadJsonNumber (string j, ref int i)
        {
            var start = i;
            while (i < j.Length && "-0123456789.eE".IndexOf (j[i]) >= 0) i++;
            var value = j.Substring (start, i - start);
            if (value.Length == 0) throw new Exception ("JSON Expected value");
            if (double.TryParse (value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number))
                return number;
            throw new Exception ("JSON Invalid value");
        }

        public static object ReadJsonValue (string json, ref int i)
        {
            var e = json.Length;
            while (i < e && char.IsWhiteSpace (json[i]))
                i++;
            if (i >= e)
                throw new Exception ("JSON Unexpected end");
            var n = e - i;
            switch (json[i]) {
                case '[':
                    return ReadJsonArray (json, ref i);
                case '{':
                    return ReadJsonObject (json, ref i);
                case '\"':
                    return ReadJsonString (json, ref i);
                case 'f':
                    i += 5;
                    return false;
                case 't':
                    i += 4;
                    return true;
                case 'n':
                    if (string.CompareOrdinal (json, i, "null", 0, 4) == 0) {
                        i += 4;
                        return null;
                    }
                    throw new Exception ("JSON Invalid value");
                default:
                    return ReadJsonNumber (json, ref i);
            }
        }

        public static object ReadJsonValue (string json, int startIndex)
        {
            var i = startIndex;
            return ReadJsonValue (json, ref i);
        }
    }
}
