using System.Collections.Generic;

namespace Minalear.Engine.Content.ModelLoader
{
    public class Group
    {
        public string Name;
        public string MaterialName;
        public List<Face> Faces;

        public Group()
        {
            Name = "";
            MaterialName = "";
            Faces = new List<Face>();
        }

        public override string ToString()
        {
            return string.Format("g ( Material: {0}, Face Count: {1} )", MaterialName, Faces.Count);
        }
    }
}
