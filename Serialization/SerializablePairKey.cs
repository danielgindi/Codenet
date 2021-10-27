using System;
using System.Xml.Serialization;

namespace Codenet.Serialization
{
    [Serializable()]
    [XmlRoot("SerializablePairKey")]
    public class SerializablePairKey<A, B> : IEquatable<SerializablePairKey<A, B>>, IXmlSerializable
    {
        private A _First;
        private B _Second;
        public A First
        {
            get { return _First; }
        }
        public B Second
        {
            get { return _Second; }
        }

        public SerializablePairKey()
        {
        }
        public SerializablePairKey(A first, B second)
        {
            _First = first;
            _Second = second;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            return AreEqual(this, (SerializablePairKey<A, B>)obj);
        }

        public bool Equals(SerializablePairKey<A, B> other)
        {
            return AreEqual(this, other);
        }

        private static bool AreEqual(SerializablePairKey<A, B> a, SerializablePairKey<A, B> b)
        {
            if (!a.First.Equals(b.First))
                return false;
            if (!a.Second.Equals(b.Second))
                return false;
            return true;
        }

        public static bool operator ==(SerializablePairKey<A, B> a, SerializablePairKey<A, B> b)
        {
            return AreEqual(a, b);
        }

        public static bool operator !=(SerializablePairKey<A, B> a, SerializablePairKey<A, B> b)
        {
            return !AreEqual(a, b);
        }

        public override int GetHashCode()
        {
            return First.GetHashCode() ^ Second.GetHashCode();
        }

        #region IXmlSerializable Members
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
            var key1Serializer = new XmlSerializer(typeof(A));
            var key2Serializer = new XmlSerializer(typeof(B));

            bool wasEmpty = reader.IsEmptyElement;
            reader.Read();

            if (wasEmpty) return;

            reader.ReadStartElement("A");
            _First = (A)key1Serializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadStartElement("B");
            _Second = (B)key2Serializer.Deserialize(reader);
            reader.ReadEndElement();

            reader.ReadEndElement();
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {
            var key1Serializer = new XmlSerializer(typeof(A));
            var key2Serializer = new XmlSerializer(typeof(B));

            writer.WriteStartElement("A");
            key1Serializer.Serialize(writer, First);
            writer.WriteEndElement();

            writer.WriteStartElement("B");
            key2Serializer.Serialize(writer, Second);
            writer.WriteEndElement();
        }
        #endregion
    }
}
