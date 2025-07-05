using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;

namespace dev.susybaka.Shared.Editor
{
    // Simple class to import events defined in XML.
    namespace XMLEvents
    {
        public class Marker
        {
            [XmlAttribute("name")]
            public string name;
            [XmlAttribute("frame")]
            public int frame;
        }

        public class Action
        {
            [XmlAttribute("name")]
            public string name;

            [XmlArray("markers"), XmlArrayItem("marker")]
            public List<Marker> markers = new List<Marker>();
        }

        public class Timeline
        {
            [XmlArray("markers"), XmlArrayItem("marker")]
            public List<Marker> markers = new List<Marker>();
        }

        [XmlRoot("scene")]
        public class Scene
        {
            public string name;
            [XmlAttribute("version")]
            public int version = 1;
            [XmlAttribute("fps")]
            public int fps = 30;

            [XmlElement("timeline")]
            public Timeline timeline = new Timeline();

            [XmlArray("actions"), XmlArrayItem("action")]
            public List<Action> actions = new List<Action>();

            public void Save(string path)
            {
                var serializer = new XmlSerializer(typeof(Scene));
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(stream, this);
                }
            }

            public static Scene Load(string path)
            {
                var serializer = new XmlSerializer(typeof(Scene));
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    // deserialise scene
                    Scene scene = serializer.Deserialize(stream) as Scene;
                    return scene;
                }
            }
        }
    }
}