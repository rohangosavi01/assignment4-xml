using System;
using System.Xml;
using System.Xml.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ConsoleApp1
{
    public class Program
    {
        // Public GitHub-hosted URLs of the assignment files
        public static string xmlURL = "https://rohangosavi01.github.io/assignment4-xml/data/Hotels.xml";            // Q1.2
        public static string xmlErrorURL = "https://rohangosavi01.github.io/assignment4-xml/data/HotelsErrors.xml"; // Q1.3
        public static string xsdURL = "https://rohangosavi01.github.io/assignment4-xml/data/Hotels.xsd";           // Q1.1

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(null, xsdUrl);
                settings.ValidationType = ValidationType.Schema;

                string validationErrors = "";

                settings.ValidationEventHandler += (sender, args) =>
                {
                    validationErrors += $"Line {args.Exception.LineNumber}, Position {args.Exception.LinePosition}: {args.Message}\n";
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { }
                }

                return string.IsNullOrEmpty(validationErrors) ? "No Error" : validationErrors;
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlUrl);

                // Serialize XML into JObject with root "Hotels"
                var rawJson = JsonConvert.SerializeXmlNode(xmlDoc.DocumentElement, Newtonsoft.Json.Formatting.None, true);
                JObject fullJson = JObject.Parse(rawJson);

                // Normalize: prefix attributes with "_"
                NormalizeAttributes(fullJson);

                // Wrap inside "Hotels" object if not already
                if (!fullJson.ContainsKey("Hotels"))
                {
                    fullJson = new JObject { ["Hotels"] = fullJson };
                }

                return fullJson.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }
        
        private static void NormalizeAttributes(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                var obj = (JObject)token;
                var propertiesToChange = new List<JProperty>();

                foreach (var prop in obj.Properties())
                {
                    if (prop.Name.StartsWith("@"))
                    {
                        propertiesToChange.Add(prop);
                    }
                    else
                    {
                        NormalizeAttributes(prop.Value); // Recurse into children
                    }
                }

                foreach (var prop in propertiesToChange)
                {
                    obj.Remove(prop.Name);
                    obj["_" + prop.Name.Substring(1)] = prop.Value;
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (var item in token.Children())
                {
                    NormalizeAttributes(item);
                }
            }
        }
    }
}
