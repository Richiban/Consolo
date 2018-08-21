using AutoLazy;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Richiban.CommandLine
{
    internal class XmlCommentsRepository
    {
        private readonly XDocument _xmlComments;

        public XmlCommentsRepository(Assembly assembly)
        {
            var codeBase = Assembly.GetEntryAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);

            var xmlFilePath = Uri.UnescapeDataString(uri.Path)
                .Replace(".dll", ".xml")
                .Replace(".exe", ".xml");

            if (File.Exists(xmlFilePath))
                _xmlComments = XDocument.Load(xmlFilePath);
            else
                _xmlComments = new XDocument();
        }

        [Lazy]
        public Option<XmlComments> this[MethodModel method]
        {
            get
            {
                var declaringTypeName = method.DeclaringType.FullName;
                var methodElement = _xmlComments.XPathSelectElement(
                        $"//member[starts-with(@name, \"M:{declaringTypeName}.{method.Name}(\")]");

                if (methodElement == null)
                    return null;
                
                var methodComment = methodElement.Element("summary")?.Value?.Trim();
                var paramComments = methodElement.Descendants("param")
                    .Where(e => !String.IsNullOrEmpty(e.Value))
                    .ToDictionary(e => e.Attribute("name").Value, e => e.Value.Trim());

                return new XmlComments(methodComment, paramComments);
            }
        }
    }
}