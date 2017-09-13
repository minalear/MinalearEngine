using System;
using System.IO;
using System.Collections.Generic;
using Minalear.Engine.Content.MaterialLoader.LineParsers;

namespace Minalear.Engine.Content.MaterialLoader
{
    public class MaterialLoader
    {
        private ContentManager contentManager;
        private List<LineParserBase> lineParsers;

        private Material activeMaterial;
        private MaterialLibrary materialLibrary;

        public MaterialLoader(ContentManager contentManager)
        {
            this.contentManager = contentManager;

            lineParsers = new List<LineParserBase>();
            lineParsers.Add(new AmbientColorLineParser());
            lineParsers.Add(new DiffuseColorLineParser());
            lineParsers.Add(new DiffuseMapLineParser());
            lineParsers.Add(new NameLineParser());
            lineParsers.Add(new NormalMapLineParser());
            lineParsers.Add(new SpecularColorLineParser());
            lineParsers.Add(new SpecularExponentLineParser());
            lineParsers.Add(new SpecularMapLineParser());
        }

        public MaterialLibrary LoadLibrary(string path)
        {
            materialLibrary = new MaterialLibrary();
            using (FileStream fileStream = File.Open(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                        parseLine(reader.ReadLine());
                }
            }

            return materialLibrary;
        }

        public void CreateNewMaterial(string name)
        {
            activeMaterial = new Material(name);
            materialLibrary.Materials.Add(name, activeMaterial);
        }
        public void SetSpecularExponent(float exp)
        {
            activeMaterial.SpecularExponent = exp;
        }
        public void SetAmbientColor(float r, float g, float b)
        {
            activeMaterial.AmbientColor = new OpenTK.Graphics.Color4(r, g, b, 1.0f);
        }
        public void SetDiffuseColor(float r, float g, float b)
        {
            activeMaterial.DiffuseColor = new OpenTK.Graphics.Color4(r, g, b, 1.0f);
        }
        public void SetSpecularColor(float r, float g, float b)
        {
            activeMaterial.SpecularColor = new OpenTK.Graphics.Color4(r, g, b, 1.0f);
        }
        public void SetDiffuseMap(string map)
        {
            activeMaterial.DiffuseMapPath = map;
        }
        public void SetNormalMap(string map)
        {
            activeMaterial.NormalMapPath = map;
        }
        public void SetSpecularMap(string map)
        {
            activeMaterial.SpecularMapPath = map;
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
            Console.WriteLine("Unsupported MTL data type: {0}", prefix);
            #endif
        }
    }
}
