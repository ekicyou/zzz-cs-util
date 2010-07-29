using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Media.Media3D;

namespace CSUtil.Xml
{
    /// <summary>
    /// Xmlに関するユーティリティ関数。
    /// </summary>
    public static class XmlUtil
    {
        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, string value)
        {
            if (value == null) return;
            writer.WriteAttributeString(localName, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, short value)
        {
            if (value < 0) return;
            writer.WriteAttributeString(localName, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, Point3D value)
        {
            if (value.X == double.NaN) return;
            writer.WriteAttributeString(localName, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, DateTime value)
        {
            if (value == DateTime.MinValue) return;
            writer.WriteAttributeString(localName, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, TimeSpan value)
        {
            if (value == TimeSpan.Zero) return;
            writer.WriteAttributeString(localName, value.ToString());
        }





        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, string ns, string value)
        {
            if (value == null) return;
            writer.WriteAttributeString(localName, ns, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, string ns, short value)
        {
            if (value < 0) return;
            writer.WriteAttributeString(localName, ns, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, string ns, Point3D value)
        {
            if (value.X == double.NaN) return;
            writer.WriteAttributeString(localName, ns, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, string ns, DateTime value)
        {
            if (value == DateTime.MinValue) return;
            writer.WriteAttributeString(localName, ns, value.ToString());
        }

        /// <summary>
        /// XmlWriterに要素を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="localName"></param>
        /// <param name="ns"></param>
        /// <param name="value"></param>
        public static void Write(XmlWriter writer, string localName, string ns, TimeSpan value)
        {
            if (value == TimeSpan.Zero) return;
            writer.WriteAttributeString(localName, ns, value.ToString());
        }




        /// <summary>
        /// XmlWriterにattributeExtensionsを書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="attributeExtensions"></param>
        public static void Write(
          XmlWriter writer,
          IEnumerable<KeyValuePair<XmlQualifiedName, string>> attributeExtensions)
        {
            foreach (KeyValuePair<XmlQualifiedName, string> pair in attributeExtensions) {
                writer.WriteAttributeString(pair.Key.Name, pair.Key.Namespace, pair.Value);
            }
        }

        /// <summary>
        /// XmlWriterにelementExtensionsを書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="elementExtensions"></param>
        public static void Write(
          XmlWriter writer,
          IEnumerable<XElement> elementExtensions)
        {
            foreach (XElement element in elementExtensions) {
                element.WriteTo(writer);
            }
        }

        /// <summary>
        /// XmlWriterに拡張属性、要素拡張を書き出します。
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="attributeExtensions"></param>
        /// <param name="elementExtensions"></param>
        public static void Write(
          XmlWriter writer,
          IEnumerable<KeyValuePair<XmlQualifiedName, string>> attributeExtensions,
          IEnumerable<XElement> elementExtensions)
        {
            Write(writer, attributeExtensions);
            Write(writer, elementExtensions);
        }


    }
}