using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Atomic.Loader;
using System.Xml.Xsl;
using System.Xml;
using System.IO;

namespace Atomic.Loader.Xml
{
    public class XslTransformConverter : XmlConverter
    {
        private XslCompiledTransform _xslt = null;

        public XslTransformConverter(string xsltUrl)
        {
            _xslt = new XslCompiledTransform();
            _xslt.Load(xsltUrl);
        }

        override public void Import(string sourceText)
        {
            base.Import(sourceText);
        }

        override public string Export()
        {
            // load text into XmlDocument
            string sourceXmlText = base.Export();
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sourceXmlText);

            // transform XML into memory
            byte[] b = new byte[65535];
            MemoryStream outStream = new MemoryStream(b);
            _xslt.Transform(xmldoc, null, outStream);

            long length = outStream.Position;
            UTF8Encoding encoder = new UTF8Encoding();
            string xmlText = encoder.GetString(b, 0, (int)length);
            outStream = null;
            b = null;

            return xmlText;
        }
    }
}
