using AutoLazy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Richiban.CommandLine
{
    internal class XmlCommentsRepository
    {
        private readonly IReadOnlyCollection<XDocument> _xmlComments;

        private XmlCommentsRepository(IReadOnlyCollection<XDocument> xDocuments)
        {
            _xmlComments = xDocuments;
        }

        public static XmlCommentsRepository LoadFor(IReadOnlyCollection<Assembly> assemblies)
        {
            return new XmlCommentsRepository(assemblies.Select(LoadForAssembly).ToList());
        }

        private static XDocument LoadForAssembly(Assembly assembly)
        {
            var codeBase = assembly.CodeBase;
            var uri = new UriBuilder(codeBase);

            var xmlFilePath = Uri.UnescapeDataString(uri.Path)
                .Replace(".dll", ".xml")
                .Replace(".exe", ".xml");

            if (File.Exists(xmlFilePath))
                return XDocument.Load(xmlFilePath);
            else
                return new XDocument();
        }

        [Lazy]
        public Option<XmlComments> this[MethodModel method]
        {
            get
            {
                var declaringTypeName = method.DeclaringType.FullName;
                var methodElement = _xmlComments
                    .Select(xdoc => xdoc.XPathSelectElement(
                        $"//member[starts-with(@name, \"M:{declaringTypeName}.{method.Name}(\")]"))
                    .FirstOrDefault(elem => elem != null);

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