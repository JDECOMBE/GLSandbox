using System.Runtime.InteropServices;
using Assimp;
using SixLabors.ImageSharp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using EnableCap = OpenTK.Graphics.OpenGL4.EnableCap;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;
using TextureWrapMode = OpenTK.Graphics.OpenGL.TextureWrapMode;

namespace OpenTKTesting.Rendering;

public class SceneRenderer : IRenderingItem
{
    #region Declaration

    private class MeshRef
    {
        public bool IsLightSource { get; init; }
        public int BaseVertex { get; init; }
        public IntPtr BaseIndex { get; init; }
        public int IndicesCount { get; init; }
        public MaterialRef Material { get; init; }
    }

    private class MaterialRef
    {
        public int[] TextureDiffuse { get; init; }
        public Vector3 FallbackDiffuse { get; init; }
        public int[] TextureSpecular { get; init; }
        public Vector3 FallbackSpecular { get; init; }
        public float Shininess { get; init; }
    }

    private class SceneNode
    {
        public MeshRef[] Meshes { get; init; }
        public Matrix4 Model { get; init; }
    }

    #endregion

    #region Members

    private Scene _scene;
    private VertexArrayObject _vao;
    private ShaderProgram _program;
    private Dictionary<int, int> _textureToGl = new();
    private MaterialRef[] _materialRefs;
    private List<MeshRef> _meshRefs = new();
    private List<SceneNode> _nodes = new();
    private DepthRenderPass _depthRenderPass = new();

    #endregion

    #region IRenderingItem Implementation

    public void Init()
    {

        _program = new ShaderProgram(new[]
        {
            new Shader(ShaderType.VertexShader, "../../../Shaders/mesh_vert.glsl"),
            new Shader(ShaderType.FragmentShader, "../../../Shaders/mesh_frag.glsl")
        });

        _depthRenderPass.Init();

        var assimpContext = new AssimpContext();
        _scene = assimpContext.ImportFile("./Assets/Dragon25.fbx", PostProcessPreset.TargetRealTimeQuality | PostProcessSteps.FlipUVs);

        LoadMaterials();
        LoadMeshes();
        ParseSceneArchitecture(_scene.RootNode, Matrix4.CreateRotationZ((float)Math.PI));

        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
    }

    public void Render(Camera camera)
    {
        // var lightPos = new Vector3((float) MathHelper.Sin(_currentTime / 2f) * 30f, (float) MathHelper.Cos(_currentTime / 2f) * 30f, 1f);
        var lightPos = new Vector3(0f, 5f, 3f);
        var lightViewProjection = Matrix4.LookAt(lightPos, new Vector3(0, 0f, 0f), Vector3.UnitY) * Matrix4.CreateOrthographicOffCenter(-10f, 10f, -10f, 10f, 0.001f, 1000f);

        _vao.Bind();
        _depthRenderPass.PreRender();
        _depthRenderPass.SetMatrix(lightViewProjection);
        RenderMeshes(_depthRenderPass.Program, false);
        _depthRenderPass.PostRender(camera);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        _program.Use();
        _program.Upload("viewProjection", camera.ViewProjection);
        _program.Upload("lightViewProjection", lightViewProjection);
        _program.Upload("viewOrigin", camera.Position);
        _program.Upload("scale", 0.03f);

        _program.Upload("light.position", lightPos);
        _program.Upload("light.ambient", new Vector3(0.4f));
        _program.Upload("light.diffuse", new Vector3(0.5f, 0.5f, 0.5f));
        _program.Upload("light.specular", new Vector3(1f));
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _depthRenderPass.DepthTexture);
        _program.Upload("shadowMap", 0);

        RenderMeshes(_program);
    }

    private float _currentTime = 0;

    public void Update(float dts)
    {
        _currentTime += dts;
    }

    #endregion

    #region Private Logic

    private void RenderMeshes(ShaderProgram prog, bool texture = true)
    {
        foreach (SceneNode n in _nodes)
        {
            prog.Upload("transform", n.Model * Matrix4.CreateScale(.02f));
            foreach (MeshRef m in n.Meshes)
                DrawMesh(m, texture);
        }
    }

    private void DrawMesh(MeshRef m, bool texture = true)
    {
        if (texture)
        {
            var activeTexture = 1;
            for (var i = 0; i < m.Material.TextureDiffuse.Length; i++)
            {
                var id = m.Material.TextureDiffuse[i];
                GL.ActiveTexture(TextureUnit.Texture0 + activeTexture);
                GL.BindTexture(TextureTarget.Texture2D, id);
                _program.Upload($"material.diffuse[{i}]", activeTexture);
                activeTexture++;
            }

            _program.Upload($"material.diffuseCount", m.Material.TextureDiffuse.Length);
            _program.Upload($"material.fallbackDiffuse", m.Material.FallbackDiffuse);


            for (var i = 0; i < m.Material.TextureSpecular.Length; i++)
            {
                var id = m.Material.TextureSpecular[i];
                GL.ActiveTexture(TextureUnit.Texture0 + activeTexture);
                GL.BindTexture(TextureTarget.Texture2D, id);
                _program.Upload($"material.specular[{i}]", activeTexture);
                activeTexture++;
            }

            _program.Upload($"material.specularCount", m.Material.TextureSpecular.Length);
            _program.Upload($"material.fallbackSpecular", m.Material.FallbackSpecular);

            _program.Upload($"material.shininess", m.Material.Shininess);
        }
        _program.Upload("isLightSource", m.IsLightSource ? 1f : 0f);

        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, m.IndicesCount, DrawElementsType.UnsignedInt, m.BaseIndex, m.BaseVertex);
    }

    private (int width, int height, byte[] data) LoadTexture(string filename)
    {
        var img = Image.Load<Rgb24>(filename);
        var pixels = new byte[img.Width * img.Height * 3];
        img.Frames.RootFrame.CopyPixelDataTo(pixels);
        return (img.Width, img.Height, pixels);
    }

    private int GenTexture(TextureSlot s)
    {
        var id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
        var fileName = "./Assets/" + s.FilePath;
        fileName = fileName.Replace("\\", "/");
        (int width, int height, byte[] data) img;
        if (_scene.Textures.Any() && _scene.Textures[s.TextureIndex].IsCompressed)
        {
            var texture = _scene.Textures[s.TextureIndex];
            var i = Image.Load<Rgb24>(texture.CompressedData);
            var buffer = new byte[i.Width * i.Height * 3];
            i.CopyPixelDataTo(buffer);
            img = (i.Width, i.Height, buffer);
        }
        else
            img = LoadTexture(fileName);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, img.width, img.height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, img.data);
        return id;
    }

    private void LoadMaterials()
    {
        _materialRefs = new MaterialRef[_scene.MaterialCount];
        for (var i = 0; i < _scene.MaterialCount; i++)
        {
            var m = _scene.Materials[i];
            var diffuseSlots = m.GetMaterialTextures(TextureType.Diffuse).Take(10);
            var diffuseIds = diffuseSlots.Select(GenTexture).ToArray();
            var specularSlots = m.GetMaterialTextures(TextureType.Specular).Take(10);
            var specularIds = specularSlots.Select(GenTexture).ToArray();
            _materialRefs[i] = new MaterialRef()
            {
                TextureDiffuse = diffuseIds,
                TextureSpecular = specularIds,
                FallbackDiffuse = new Vector3(m.ColorDiffuse.R, m.ColorDiffuse.G, m.ColorDiffuse.B),
                FallbackSpecular = new Vector3(m.ColorSpecular.R, m.ColorSpecular.G, m.ColorSpecular.B),
                Shininess = m.Shininess
            };
        }
    }

    private void LoadMeshes()
    {
        _vao = new VertexArrayObject();

        var vertices = new List<float>();
        foreach (var m in _scene.Meshes)
        {
            
            var uv = m.TextureCoordinateChannels[0];
            var hasTextureCoords = m.HasTextureCoords(0);
            for (var i = 0; i < m.Vertices.Count; i++)
            {
                vertices.Add(m.Vertices[i].X);
                vertices.Add(m.Vertices[i].Y);
                vertices.Add(m.Vertices[i].Z);
                if (m.Normals.Count > 0)
                {
                    vertices.Add(m.Normals[i].X);
                    vertices.Add(m.Normals[i].Y);
                    vertices.Add(m.Normals[i].Z);
                }
                else
                {
                    vertices.Add(0);
                    vertices.Add(0);
                    vertices.Add(0);

                }
                if (hasTextureCoords)
                {
                    vertices.Add(uv[i].X);
                    vertices.Add(uv[i].Y);
                }
                else
                {
                    vertices.AddRange(new float[] {0, 0});
                }
            }
        }

        _ = new VertexBufferObject<float>(vertices.ToArray());

        var ind = _scene.Meshes.SelectMany(e => e.GetUnsignedIndices()).ToArray();
        _ = new ElementBufferObject(ind);

        var currentVert = 0;
        var currentInd = 0;
        foreach (var m in _scene.Meshes)
        {
            var indices = m.GetIndices();
            _meshRefs.Add(new MeshRef()
            {
                IsLightSource = m.Name == "Torch",
                IndicesCount = indices.Length,
                BaseIndex = IntPtr.Zero + currentInd * sizeof(uint),
                BaseVertex = currentVert,
                Material = _materialRefs[m.MaterialIndex]
            });
            currentInd += indices.Length;
            currentVert += m.Vertices.Count;
        }

        _vao.SetAttribPointer(0, 3, 8 * sizeof(float), IntPtr.Zero);
        _vao.SetAttribPointer(1, 3, 8 * sizeof(float), IntPtr.Zero + 3 * sizeof(float));
        _vao.SetAttribPointer(2, 2, 8 * sizeof(float), IntPtr.Zero + 6 * sizeof(float));
    }

    private void ParseSceneArchitecture(Node node, Matrix4 parentTransform)
    {
        var tkTransform = new Matrix4(
            node.Transform.A1, node.Transform.A2, node.Transform.A3, node.Transform.A4,
            node.Transform.B1, node.Transform.B2, node.Transform.B3, node.Transform.B4,
            node.Transform.C1, node.Transform.C2, node.Transform.C3, node.Transform.C4,
            node.Transform.D1, node.Transform.D2, node.Transform.D3, node.Transform.D4
        );
        var nodeTransform = tkTransform * parentTransform;
        _nodes.Add(new SceneNode()
        {
            Meshes = node.MeshIndices.Select(e => _meshRefs[e]).ToArray(),
            Model = nodeTransform
        });

        foreach (var n in node.Children)
            ParseSceneArchitecture(n, nodeTransform);
    }

    #endregion
}