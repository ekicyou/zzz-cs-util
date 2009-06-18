using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ComponentModel;
using CSUtil.Xml;

namespace CSUtil.Xml
{
  /// <summary>
  /// Xml拡張エレメントの基本クラスです。
  /// </summary>
  public abstract class ElmentBase : IXmlSerializable, INotifyPropertyChanged
  {
    private Dictionary<XmlQualifiedName, string> extensionAttributes;
    private Collection<XElement> extensionElements;

    /// <summary>拡張属性を取得します。</summary>
    public Dictionary<XmlQualifiedName, string> AttributeExtensions
    {
      get { return this.extensionAttributes; }
    }

    /// <summary>要素拡張を取得します。</summary>
    public Collection<XElement> ElementExtensions
    {
      get { return this.extensionElements; }
    }

    /// <summary>
    /// コンストラクタ。
    /// </summary>
    protected ElmentBase()
    {
      extensionElements = new Collection<XElement>();
      extensionAttributes = new Dictionary<XmlQualifiedName, string>();
    }

    /// <summary>
    /// XMLの読み込みを行います。
    /// </summary>
    /// <param name="reader"></param>
    public void ReadXml(XmlReader reader)
    {
      bool isEmpty = reader.IsEmptyElement;
      if (reader.HasAttributes) {
        for (int i = 0; i < reader.AttributeCount; i++) {
          reader.MoveToNextAttribute();
          if (!ReadXmlAttribute(
            reader.NamespaceURI,
            reader.LocalName,
            reader.Value)) {
            AttributeExtensions.Add(new
                         XmlQualifiedName(reader.LocalName,
                         reader.NamespaceURI),
                         reader.Value);
          }
        }
      }
      reader.ReadStartElement();
      if (!isEmpty) {
        while (reader.IsStartElement()) {
          XElement elment = (XElement)XElement.ReadFrom(reader);
          if (!ReadXmlElement(elment)) ElementExtensions.Add(elment);
        }
        reader.ReadEndElement();
      }
    }

    /// <summary>
    /// XML属性の読み込みを行います。
    /// </summary>
    /// <param name="ns">NameSpace</param>
    /// <param name="name">アトリビュート名</param>
    /// <param name="value">値</param>
    /// <returns>読込対象だった場合にtrue</returns>
    protected abstract bool ReadXmlAttribute(string ns, string name, string value);

    /// <summary>
    /// XML要素の読み込みを行います。
    /// </summary>
    /// <param name="elment">XElement</param>
    /// <returns>読込対象だった場合にtrue</returns>
    protected abstract bool ReadXmlElement(XElement elment);

    /// <summary>
    /// Xmlの書き込みを行います。
    /// </summary>
    /// <param name="writer"></param>
    public virtual void WriteXml(System.Xml.XmlWriter writer)
    {
      XmlUtil.Write(writer, AttributeExtensions, ElementExtensions);
    }

    /// <summary>
    /// XMLスキーマを取得します。
    /// </summary>
    /// <returns></returns>
    System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
    {
      throw new System.NotImplementedException();
    }


    #region INotifyPropertyChanged メンバ
    /// <summary>
    /// 属性の変更を通知します。
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 属性が変更されたときに呼び出します。
    /// </summary>
    /// <param name="propertyName"></param>
    protected void OnPropertyChanged(string propertyName)
    {
      if (PropertyChanged == null) return;
      PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);
      PropertyChanged(this, args);
    }

    #endregion
  }
}
