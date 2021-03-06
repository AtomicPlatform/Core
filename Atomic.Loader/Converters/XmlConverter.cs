﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Atomic.Loader
{
    public class XmlConverter : IDataConverter
    {
        private ProcessModel _model = new ProcessModel();

        public string FileExtension { get { return "xml"; } }

        virtual public void Import(string sourceText)
        {
            XmlSerializer ser = new XmlSerializer(typeof(ProcessModel));
            Model = (ProcessModel)ser.Deserialize(new StringReader(sourceText));
        }

        virtual public string Export()
        {
            XmlSerializer ser = new XmlSerializer(typeof(ProcessModel));
            byte[] b = new byte[65535];
            MemoryStream outStream = new MemoryStream(b);

            ser.Serialize(outStream, Model);
            long length = outStream.Position;

            UTF8Encoding encoder = new UTF8Encoding();
            string xmlText = encoder.GetString(b, 0, (int)length);
            outStream = null;
            b = null;

            return xmlText;
        }

        public IProcessModel Model
        {
            get { return _model; }
            private set { _model = (ProcessModel)value; }
        }
    }
}
