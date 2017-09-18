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
                Vector3 pos1 = vertexList[face.Vertices[0].VertexIndex];
                Vector3 pos2 = vertexList[face.Vertices[1].VertexIndex];
                Vector3 pos3 = vertexList[face.Vertices[2].VertexIndex];

                Vector2 uv1 = texCoordList[face.Vertices[0].TextureIndex];
                Vector2 uv2 = texCoordList[face.Vertices[1].TextureIndex];
                Vector2 uv3 = texCoordList[face.Vertices[2].TextureIndex];

                Vector3 n1 = normalList[face.Vertices[0].NormalIndex];
                Vector3 n2 = normalList[face.Vertices[1].NormalIndex];
                Vector3 n3 = normalList[face.Vertices[2].NormalIndex];

                Vector3 edge1 = pos2 - pos1;
                Vector3 edge2 = pos3 - pos1;
                Vector2 deltaUV1 = uv2 - uv1;
                Vector2 deltaUV2 = uv3 - uv1;

                float f = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV2.X * deltaUV1.Y);

                Vector3 tangent;
                tangent.X = f * (deltaUV2.Y * edge1.X - deltaUV1.Y * edge2.X);
                tangent.Y = f * (deltaUV2.Y * edge1.Y - deltaUV1.Y * edge2.Y);
                tangent.Z = f * (deltaUV2.Y * edge1.Z - deltaUV1.Y * edge2.Z);

                if (tangent.LengthSquared > 0f)
                    tangent.Normalize();

                Vector3 bitangent;
                bitangent.X = f * (-deltaUV2.X * edge1.X + deltaUV1.X * edge2.X);
                bitangent.Y = f * (-deltaUV2.X * edge1.Y + deltaUV1.X * edge2.Y);
                bitangent.Z = f * (-deltaUV2.X * edge1.Z + deltaUV1.X * edge2.Z);

                if (bitangent.LengthSquared > 0f)
                    bitangent.Normalize();

                //Vertex 1
                buffer.AddRange(pos1.ToArray());
                buffer.Add(uv1.X);
                buffer.Add(1.0f - uv1.Y);
                buffer.AddRange(n1.ToArray());
                buffer.AddRange(tangent.ToArray());
                buffer.AddRange(bitangent.ToArray());

                //Vertex 2
                buffer.AddRange(pos2.ToArray());
                buffer.Add(uv2.X);
                buffer.Add(1.0f - uv2.Y);
                buffer.AddRange(n2.ToArray());
                buffer.AddRange(tangent.ToArray());
                buffer.AddRange(bitangent.ToArray());

                //Vertex 3
                buffer.AddRange(pos3.ToArray());
                buffer.Add(uv3.X);
                buffer.Add(1.0f - uv3.Y);
                buffer.AddRange(n3.ToArray());
                buffer.AddRange(tangent.ToArray());
                buffer.AddRange(bitangent.ToArray());
            }

            return buffer.ToArray();
        }
    }
}
