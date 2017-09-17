using System;
using System.IO;
using System.Collections.Generic;
using OpenTK;
using Minalear.Engine.Content.ModelLoader.LineParsers;

namespace Minalear.Engine.Content.ModelLoader
{
    internal class ModelLoader
    {
        private ContentManager contentManager;

        private List<LineParserBase> lineParsers;

        private List<Group> groups;
        private List<Vector3> vertexList;
        private List<Vector2> texCoordList;
        private List<Vector3> normalList;

        private Group activeGroup;

        private string matLibraryPath;

        public ModelLoader(ContentManager contentManager)
        {
            this.contentManager = contentManager;

            groups = new List<Group>();

            vertexList = new List<Vector3>();
            texCoordList = new List<Vector2>();
            normalList = new List<Vector3>();

            //Create new line parsers
            lineParsers = new List<LineParserBase>();
            lineParsers.Add(new VertexLineParser());
            lineParsers.Add(new TextureLineParser());
            lineParsers.Add(new NormalLineParser());
            lineParsers.Add(new FaceLineParser());
            lineParsers.Add(new GroupLineParser());
            lineParsers.Add(new MaterialLineParser());
            lineParsers.Add(new UseMaterialLineParser());
        }

        public Model LoadModel(string modelPath)
        {
            //Parse Model file
            using (FileStream fileStream = File.Open(modelPath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                        parseLine(reader.ReadLine());
                }
            }

            //Load material library
            var materialLibrary = contentManager.LoadMTLLibrary(matLibraryPath);

            Model model = new Model();
            model.Meshes = new Mesh[groups.Count];

            for (int i = 0; i < groups.Count; i++)
            {
                model.Meshes[i] = new Mesh();
                model.Meshes[i].VertexData = constructVertexDataFromGroup(groups[i]);
                model.Meshes[i].Material = materialLibrary.Materials[groups[i].MaterialName];
            }

            return model;
        }
        
        public void AddNewVertex(Vector3 vertex)
        {
            vertexList.Add(vertex);
        }
        public void AddNewTextureCoord(Vector2 texCoord)
        {
            texCoordList.Add(texCoord);
        }
        public void AddNewNormal(Vector3 normal)
        {
            normalList.Add(normal);
        }

        public void AddNewGroup(Group group)
        {
            groups.Add(group);
            activeGroup = group;
        }
        public void AddNewFace(Face face)
        {
            if (activeGroup == null)
                throw new InvalidOperationException("Invalid OBJ structure.  Importer treats Groups as separate meshes and requires one to be declared.");
            activeGroup.Faces.Add(face);
        }

        public void SetMaterialLibrary(string mtllib)
        {
            matLibraryPath = mtllib;
        }
        public void SetActiveMaterial(string material)
        {
            if (activeGroup == null)
                throw new InvalidOperationException("Invalid OBJ structure.  Importer treats Groups as separate meshes and requires one to be declared.");
            activeGroup.MaterialName = material;
        }

        private void parseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return; //Invalid line
            if (line[0] == '#') return; //Comment

            line = line.Trim();
            int firstSpace = line.IndexOf(' ');

            string prefix = line.Substring(0, firstSpace);
            string data = line.Substring(firstSpace + 1);

            foreach (LineParserBase lineParser in lineParsers)
            {
                if (lineParser.CanParseLine(prefix))
                {
                    lineParser.ParseLine(this, data);
                    return;
                }
            }

            #if DEBUG
            Console.WriteLine("Unsupported OBJ data type: {0}", prefix);
            #endif
        }
        private float[] constructVertexDataFromGroup(Group group)
        {
            List<float> buffer = new List<float>();

            //Assuming triangles - POS/TEX/NORMAL
            foreach (Face face in group.Faces)
            {
                foreach (Vertex vertex in face.Vertices)
                {
                    Vector3 Position = vertexList[vertex.VertexIndex];
                    Vector2 TexCoord = texCoordList[vertex.TextureIndex];
                    Vector3 Normal = normalList[vertex.NormalIndex];

                    //Position
                    buffer.Add(Position.X);
                    buffer.Add(Position.Y);
                    buffer.Add(Position.Z);

                    //Texture Coordinate
                    buffer.Add(TexCoord.X);
                    buffer.Add(1.0f - TexCoord.Y);

                    //Normal
                    buffer.Add(Normal.X);
                    buffer.Add(Normal.Y);
                    buffer.Add(Normal.Z);
                }
            }

            return buffer.ToArray();
        }
    }
}
