using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DataObjects
{
    public class Workout : IXmlSerializable, IEquatable<Workout>
    {
        private string title;
        private string link;

        [Obsolete(message:"For serialization only.")]
        // ReSharper disable once UnusedMember.Local
        private Workout()
        {
            Tags = new List<string>();
        }

        public Workout(string title, string link, params string[] tags)
        {
            this.title = string.IsNullOrWhiteSpace(title) ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(title)) : title;
            this.link = string.IsNullOrWhiteSpace(link) ? throw new ArgumentException("Value cannot be null or whitespace.", nameof(link)) : link;
            Tags = new Collection<string>();

            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }

        public string Title => title;

        public string Link => link;

        public ICollection<string> Tags { get; }

        public override string ToString()
        {
            return Title;
        }

        #region Serialization

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            reader.ReadToDescendant(nameof(Title));

            title = reader.ReadElementContentAsString(nameof(Title), string.Empty);

            link = reader.ReadElementContentAsString(nameof(Link), string.Empty);

            using (var tagReader = reader.ReadSubtree())
            {
                while (tagReader.Read())
                {
                    if (tagReader.NodeType == XmlNodeType.Text)
                    {
                        Tags.Add(tagReader.Value);
                    }
                }

                reader.ReadEndElement();
            }

            reader.ReadEndElement();
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElementString(nameof(Title), Title);

            writer.WriteElementString(nameof(Link), Link);

            if (Tags.Any())
            {
                writer.WriteStartElement(nameof(Tags));

                foreach (var tag in Tags)
                {
                    writer.WriteElementString(nameof(tag), tag);
                }

                writer.WriteEndElement();
            }
        }

        #endregion

        #region Equality Comparison

        public override bool Equals(object obj)
        {
            return Equals((Workout)obj);
        }

        public bool Equals(Workout other)
        {
            return other is not null && Link == other.Link;
        }

        public override int GetHashCode()
        {
            return Link != null ? Link.GetHashCode() : 0;
        }

        public static bool operator ==(Workout left, Workout right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Workout left, Workout right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}