
## Missing Features

Add node by path

the idea is to be able to provide a json path such as '4,'

### Example based on Json.Net

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace JsonTools
{
    public static class JTokenExt
    {
        public static JToken? AddTokenByPath(this JToken? jToken, string? path, object? value) =>
            new JsonTools().AddTokenByPath(jToken, path, value);

        public static JToken? AddTokenByPaths(this JToken? jToken, params (string? path, object? value)[] items)
        {
            var tools = new JsonTools();
            var root = (jToken ?? new JObject()).DeepClone();

            foreach (var item in items)
                tools.AddTokenByPath(root, item.path, item.value, mutate: true);

            return root;
        }
    }
    public class JsonTools
    {
        //Base on https://stackoverflow.com/a/65715088/89586
        public JToken? AddTokenByPath(JToken? jToken, string? path, object? value, bool mutate = false)
        {
            if (string.IsNullOrWhiteSpace(path) || value == null) return jToken;
            if (jToken == null) jToken = new JObject();

            var pathParts = Tokenize(path).ToArray();
            var root = mutate ? jToken : jToken.DeepClone();
            var node = root;
            for (int i = 0; i < pathParts.Length; i++)
            {
                var pathPart = pathParts[i];

                JToken partNode;
                if (pathPart == "[]")
                {
                    JArray newNode = BuildChildren(node, pathParts.Skip(i + 1), value);

                    node.Replace(newNode);
                    break;
                }
                else
                {
                    partNode = node.SelectToken(pathPart);
                }

                //node is null or token with null value
                if (partNode == null || partNode.Type == JTokenType.Null)
                {
                    if (i < pathParts.Length - 1)
                    {
                        //the next level is array or object
                        //accept [0], not ['prop']
                        var nextToken = Regex.IsMatch(pathParts[i + 1], @"\[\d+\]") ? (JToken)new JArray() : new JObject();
                        SetToken(node, pathPart, nextToken);
                    }
                    else if (i == pathParts.Length - 1)
                    {
                        //JToken.FromObject(null) will throw a exception
                        var jValue = value == null ?
                           null : JToken.FromObject(value);
                        SetToken(node, pathPart, jValue);
                    }
                    partNode = node.SelectToken(pathPart);
                }
                node = partNode;
            }

            return root;

            //set new token
            static void SetToken(JToken node, string pathPart, JToken jToken)
            {
                if (node.Type == JTokenType.Object)
                {
                    //get real prop name (convert "['prop']" to "prop")
                    var name = pathPart.Trim('[', ']', '\'');
                    ((JObject)node).Add(name, jToken);
                }
                else if (node.Type == JTokenType.Array)
                {
                    //get real index (convert "[0]" to 0)
                    var index = int.Parse(pathPart.Trim('[', ']'));
                    var jArray = (JArray)node;
                    //if index is bigger than array length, fill the array
                    while (index >= jArray.Count)
                        jArray.Add(null);
                    //set token
                    jArray[index] = jToken;
                }
            }
            static IEnumerable<string> Tokenize(string path)
            {
                if (path.StartsWith("$.")) path = path[2..];

                var parts = Regex.Split(path, @"(?=\[)|(?=\[\.)|(?<=])(?>\.)");

                foreach (var part in parts)
                {
                    if (part.Length > 0 && !part.StartsWith('[') && part.Contains('.') && char.IsLetter(part[0]))
                    {
                        foreach (var subPart in part.Split('.'))
                            yield return subPart;
                    }
                    else
                    {
                        yield return part;
                    }
                }

                // Regex.Split("a.b.d[1]['my1.2.4'][4].af['micor.a.ee.f'].ra[6]", @"(?=\[)|(?=\[\.)|(?<=])(?>\.)")
                // > { "a.b.d", "[1]", "['my1.2.4']", "[4]", "af", "['micor.a.ee.f']", "ra", "[6]" }
            }
        }

        private JArray BuildChildren(JToken node, IEnumerable<string> tokens, object value)
        {
            var path = string.Join("", tokens.Select(t => t.StartsWith('[') ? t : $"['{t}']"));

            var array = node?.Type == JTokenType.Array ? (JArray)node : new JArray();

            if (string.IsNullOrWhiteSpace(path))
            {
                var token = JToken.FromObject(value);
                if (token.Type == JTokenType.Array)
                {
                    array = (JArray)token;
                }
                else
                {
                    array.Add(token);
                }
            }
            else
            {
                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        array.Add(AddTokenByPath(new JObject(), path, item));
                    };
                }
                else
                {
                    array.Add(AddTokenByPath(new JObject(), path, value));
                }
            }

            return array;
        }
    }
}
