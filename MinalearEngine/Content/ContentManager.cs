using System;
using System.IO;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine.Content.MaterialLoader;
using Minalear.Engine.Content.ModelLoader;

namespace Minalear.Engine.Content
{
    public sealed class ContentManager
    {
        private string contentDirectory = "/";
        public string ContentDirectory { get { return contentDirectory; } set { contentDirectory = value; } }

        public ContentManager() { }
        public ContentManager(string defaultDirectory)
        {
            contentDirectory = defaultDirectory;
        }

        public Texture2D LoadTexture2D(string path)
        {
            path = checkValidPath(path);

            //Load pixel data from file source using Bitmap from System.Drawing
            var bitmap = new Bitmap(path);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr pixelData = bitmapData.Scan0;

            //Generate texture with the pixel data
            int textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixelData);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            //Set wrapping and filter modes
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            //Create our new texture
            Texture2D texture = new Texture2D(textureID, bitmap.Width, bitmap.Height);

            //Cleanup
            bitmap.UnlockBits(bitmapData);
            bitmap.Dispose();

            return texture;
        }
        public ShaderProgram LoadShaderProgram(string vertexPath, string fragmentPath)
        {
            vertexPath = checkValidPath(vertexPath);
            fragmentPath = checkValidPath(fragmentPath);

            //Vertex Shader
            int vertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            string vertexShaderSource = readAllText(vertexPath);
            GL.ShaderSource(vertexShaderID, vertexShaderSource);
            GL.CompileShader(vertexShaderID);
            checkShaderCompilationStatus(vertexShaderID, ShaderType.VertexShader);

            //Fragment Shader
            int fragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            string fragmentShaderSource = readAllText(fragmentPath);
            GL.ShaderSource(fragmentShaderID, fragmentShaderSource);
            GL.CompileShader(fragmentShaderID);
            checkShaderCompilationStatus(fragmentShaderID, ShaderType.FragmentShader);

            //Create shader program
            int programID = GL.CreateProgram();

            //Attach shaders to the program and link
            GL.AttachShader(programID, vertexShaderID);
            GL.AttachShader(programID, fragmentShaderID);
            GL.LinkProgram(programID);

            //Detach and clean up the shaders
            GL.DetachShader(programID, vertexShaderID);
            GL.DetachShader(programID, fragmentShaderID);
            GL.DeleteShader(vertexShaderID);
            GL.DeleteShader(fragmentShaderID);
            
            return new ShaderProgram(programID);
        }
        public ShaderProgram LoadShaderProgram(string vertex, string geometry, string fragment)
        {
            vertex = checkValidPath(vertex);
            geometry = checkValidPath(geometry);
            fragment = checkValidPath(fragment);

            //Vertex Shader
            int vertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            string vertexShaderSource = readAllText(vertex);
            GL.ShaderSource(vertexShaderID, vertexShaderSource);
            GL.CompileShader(vertexShaderID);
            checkShaderCompilationStatus(vertexShaderID, ShaderType.VertexShader);

            //Geometry Shader
            int geometryShaderID = GL.CreateShader(ShaderType.GeometryShader);
            string geometryShaderSource = readAllText(geometry);
            GL.ShaderSource(geometryShaderID, geometryShaderSource);
            GL.CompileShader(geometryShaderID);
            checkShaderCompilationStatus(geometryShaderID, ShaderType.GeometryShader);

            //Fragment Shader
            int fragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            string fragmentShaderSource = readAllText(fragment);
            GL.ShaderSource(fragmentShaderID, fragmentShaderSource);
            GL.CompileShader(fragmentShaderID);
            checkShaderCompilationStatus(fragmentShaderID, ShaderType.FragmentShader);

            //Create shader program
            int programID = GL.CreateProgram();

            //Attach shaders to the program and link
            GL.AttachShader(programID, vertexShaderID);
            GL.AttachShader(programID, geometryShaderID);
            GL.AttachShader(programID, fragmentShaderID);
            GL.LinkProgram(programID);

            //Detach and clean up the shaders
            GL.DetachShader(programID, vertexShaderID);
            GL.DetachShader(programID, geometryShaderID);
            GL.DetachShader(programID, fragmentShaderID);
            GL.DeleteShader(vertexShaderID);
            GL.DeleteShader(geometryShaderID);
            GL.DeleteShader(fragmentShaderID);

            return new ShaderProgram(programID);
        }
        public Model LoadObjModel(string path)
        {
            path = checkValidPath(path);

            var modelLoader = new ModelLoader.ModelLoader(this);
            Model model = modelLoader.LoadModel(path);

            //Load material textures
            foreach (Mesh mesh in model.Meshes)
            {
                mesh.Material.DiffuseMap = LoadTexture2D(mesh.Material.DiffuseMapPath);
                mesh.Material.NormalMap = LoadTexture2D(mesh.Material.NormalMapPath);
                mesh.Material.SpecularMap = LoadTexture2D(mesh.Material.SpecularMapPath);
            }

            return model;
        }
        public MaterialLibrary LoadMTLLibrary(string path)
        {
            path = checkValidPath(path);

            var materialLoader = new MaterialLoader.MaterialLoader(this);
            MaterialLibrary library = materialLoader.LoadLibrary(path);

            return library;
        }

        private string readAllText(string path)
        {
            string fileText = string.Empty;
            using (FileStream file = File.Open(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    fileText = reader.ReadToEnd();
                }
            }

            return fileText;
        }
        private string checkValidPath(string path)
        {
            path = contentDirectory + path;
            if (!File.Exists(path))
                throw new FileNotFoundException(path);
            return path;
        }
        private void checkShaderCompilationStatus(int id, ShaderType type)
        {
            int status = 0;
            GL.GetShader(id, ShaderParameter.CompileStatus, out status);

            if (status == SHADER_COMPILATION_ERROR_CODE)
            {
                string errorMessage = GL.GetShaderInfoLog(id);
                throw new ShaderCompilationException(errorMessage);
            }
        }

        private const int SHADER_COMPILATION_ERROR_CODE = 0;
    }
}
