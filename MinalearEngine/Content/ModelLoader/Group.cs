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
            return string.Format("g ( {0} Material: {1}, Face Count: {2} )", Name, MaterialName, Faces.Count);
        }
    }
}
