using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace rE2MD
{
    class EMDRenderer
    {
        public struct modelAnimation
        {
            public Vector3 translation;
            public Vector3 speed;
            public Matrix[] rotation;
        }

        private struct modelObject
        {
            public CustomVertex.PositionNormalTextured[] vertexBuffer;
            public Matrix transform;
        }

        private modelObject[] model;
        public modelAnimation[][][] animations;

        private PresentParameters pParams;
        private Device device;

        private float scale = 100.0f;

        private byte[] quadOrder = { 0, 1, 2, 1, 2, 3 };

        private bool keepRendering;

        private int texWidth, texHeight;
        private Vector3 minVector, maxVector;

        public Vector2 translation;
        public Vector2 rotation;
        public float zoom = -20.0f;

        public int animationGroup = -1;
        public int animationIndex = 0;

        private int frame = 0;

        /// <summary>
        ///     Inicializa a Engine no controle.
        /// </summary>
        /// <param name="handler">Ponteiro na RAM para área de renderização do controle</param>
        /// <param name="width">Largura da imagem</param>
        /// <param name="height">Altura da imagem</param>
        public void initialize(System.IntPtr handler, int width, int height)
        {
            pParams = new PresentParameters();
            pParams.BackBufferCount = 1;
            pParams.BackBufferFormat = Manager.Adapters[0].CurrentDisplayMode.Format;
            pParams.BackBufferWidth = width;
            pParams.BackBufferHeight = height;
            pParams.Windowed = true;
            pParams.SwapEffect = SwapEffect.Discard;
            pParams.EnableAutoDepthStencil = true;
            pParams.AutoDepthStencilFormat = DepthFormat.D32;

            try
            {
                device = new Device(0, DeviceType.Hardware, handler, CreateFlags.HardwareVertexProcessing, pParams);
            }
            catch
            {
                //Falha ao iniciar com aceleração por Hardware, vamos usar o renderizador por Software
                device = new Device(0, DeviceType.Reference, handler, CreateFlags.HardwareVertexProcessing, pParams);
            }
            setupViewPort();
        }

        private void setupViewPort()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, (float)pParams.BackBufferWidth / pParams.BackBufferHeight, 0.1f, 500.0f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 20.0f), new Vector3(0.0F, 0.0F, 0.0F), new Vector3(0.0f, 1.0f, 0.0f));

            device.RenderState.Lighting = true;
            device.RenderState.Ambient = Color.Black;
            device.Lights[0].Type = LightType.Point;
            device.Lights[0].Diffuse = Color.White;
            device.Lights[0].Position = new Vector3(0, 100.0f, 100.0f);
            device.Lights[0].Range = 500.0f;
            device.Lights[0].Attenuation0 = 0.25f;
            device.Lights[0].Enabled = true;
            device.RenderState.CullMode = Cull.None;
            device.RenderState.ZBufferEnable = true;
            device.SamplerState[0].MinFilter = TextureFilter.Linear;
            device.SamplerState[0].MagFilter = TextureFilter.Linear;
        }

        /// <summary>
        ///     Carrega um modelo EMD.
        /// </summary>
        /// <param name="fileName">Caminho para o arquivo *.EMD</param>
        public void load(string fileName)
        {
            List<modelObject> mdl = new List<modelObject>();

            device.SetTexture(0, null);
            texWidth = 256;
            texHeight = 256;
            string textureName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName)) + ".tim";
            if (File.Exists(textureName))
            {
                Bitmap img = TIM.load(textureName);
                texWidth = img.Width;
                texHeight = img.Height;
                device.SetTexture(0, new Texture(device, img, Usage.None, Pool.Managed));
            }

            FileStream data = new FileStream(fileName, FileMode.Open);
            BinaryReader input = new BinaryReader(data);

            uint pointerTableOffset = input.ReadUInt32();

            data.Seek(pointerTableOffset, SeekOrigin.Begin);
            uint[] sections = new uint[8];
            for (int index = 0; index < 8; index++)
            {
                sections[index] = input.ReadUInt32();
            }

            /*
             * Animação 
             */
            List<List<List<int>>> animationFrames = new List<List<List<int>>>();

            for (int i = 0; i < 3; i++)
            {
                int sectionIndex = 1 + (i * 2);

                List<List<int>> anim = new List<List<int>>();
                data.Seek(sections[sectionIndex] + 2, SeekOrigin.Begin);
                ushort length = input.ReadUInt16();
                data.Seek(sections[sectionIndex], SeekOrigin.Begin);

                while (data.Position - sections[sectionIndex] < length)
                {
                    ushort stepCount = input.ReadUInt16();
                    uint offset = input.ReadUInt16() + sections[sectionIndex];
                    long dataPosition = data.Position;

                    data.Seek(offset, SeekOrigin.Begin);
                    List<int> frames = new List<int>();
                    for (int j = 0; j < stepCount; j++)
                    {
                        uint value = input.ReadUInt32();
                        frames.Add((int)(value & 0xfff));
                        
                    }

                    anim.Add(frames);
                    data.Seek(dataPosition, SeekOrigin.Begin);
                }

                animationFrames.Add(anim);
            }

            //Carregamento de transformações
            uint[] skeletonAnimationOffset = new uint[3];
            ushort[] skeletonAnimationEntryLength = new ushort[3];

            data.Seek(sections[2], SeekOrigin.Begin);
            uint objectTreeOffset = input.ReadUInt16() + sections[2];
            skeletonAnimationOffset[0] = input.ReadUInt16() + sections[2];
            ushort objCount = input.ReadUInt16();
            skeletonAnimationEntryLength[0] = input.ReadUInt16();

            List<Matrix> relativePositions = new List<Matrix>();
            List<List<byte>> objectTree = new List<List<byte>>();
            for (int i = 0; i < objCount; i++)
            {
                float x = -(float)input.ReadInt16() / scale;
                float y = -(float)input.ReadInt16() / scale;
                float z = -(float)input.ReadInt16() / scale;

                relativePositions.Add(Matrix.Translation(x, y, z));
            }

            data.Seek(objectTreeOffset, SeekOrigin.Begin);
            for (int i = 0; i < objCount; i++)
            {
                ushort childsCount = input.ReadUInt16();
                uint offset = input.ReadUInt16() + objectTreeOffset;

                List<byte> childs = new List<byte>();
                long dataPosition = data.Position;
                data.Seek(offset, SeekOrigin.Begin);
                for (int j = 0; j < childsCount; j++)
                {
                    childs.Add(input.ReadByte());
                }
                objectTree.Add(childs);

                data.Seek(dataPosition, SeekOrigin.Begin);
            }

            for (int i = 0; i < 1; i++)
            {
                int sectionIndex = 4 + (i * 2);
                data.Seek(sections[sectionIndex], SeekOrigin.Begin);
                input.ReadUInt16();
                skeletonAnimationOffset[i + 1] = input.ReadUInt16() + sections[sectionIndex];
                input.ReadUInt16();
                skeletonAnimationEntryLength[i + 1] = input.ReadUInt16();
            }

            /*
             * Modelo
             */

            //Carregamento dos objetos
            data.Seek(sections[7], SeekOrigin.Begin);

            uint sectionLength = input.ReadUInt32();
            input.ReadUInt32();
            uint count = input.ReadUInt32() / 2;
            uint baseOffset = (uint)data.Position;

            for (int entry = 0; entry < count; entry++)
            {
                uint positionOffsetTriangles = input.ReadUInt32() + baseOffset;
                uint positionCountTriangles = input.ReadUInt32();
                uint normalOffsetTriangles = input.ReadUInt32() + baseOffset;
                uint normalCountTriangles = input.ReadUInt32();
                uint facesOffsetTriangles = input.ReadUInt32() + baseOffset;
                uint facesCountTriangles = input.ReadUInt32();
                uint texturesOffsetTriangles = input.ReadUInt32() + baseOffset;

                uint positionOffsetQuads = input.ReadUInt32() + baseOffset;
                uint positionCountQuads = input.ReadUInt32();
                uint normalOffsetQuads = input.ReadUInt32() + baseOffset;
                uint normalCountQuads = input.ReadUInt32();
                uint facesOffsetQuads = input.ReadUInt32() + baseOffset;
                uint facesCountQuads = input.ReadUInt32();
                uint texturesOffsetQuads = input.ReadUInt32() + baseOffset;

                long dataPosition = data.Position;
                List<CustomVertex.PositionNormalTextured> buffer = new List<CustomVertex.PositionNormalTextured>();

                //Triângulos
                for (int i = 0; i < facesCountTriangles; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        data.Seek(facesOffsetTriangles + (i * 12) + (j * 4), SeekOrigin.Begin);
                        ushort normalIndex = input.ReadUInt16();
                        ushort positionIndex = input.ReadUInt16();

                        //Carrega vetor de posição
                        data.Seek(positionOffsetTriangles + (positionIndex * 8), SeekOrigin.Begin);
                        float vx = -(float)input.ReadInt16() / scale;
                        float vy = -(float)input.ReadInt16() / scale;
                        float vz = -(float)input.ReadInt16() / scale;
                        Vector3 position = new Vector3(vx, vy, vz);

                        //Carrega vetor da normal
                        data.Seek(normalOffsetTriangles + (normalIndex * 8), SeekOrigin.Begin);
                        float nx = -(float)input.ReadInt16() / short.MaxValue;
                        float ny = -(float)input.ReadInt16() / short.MaxValue;
                        float nz = -(float)input.ReadInt16() / short.MaxValue;
                        Vector3 normal = new Vector3(nx, ny, nz);

                        //Carrega vetor da textura
                        data.Seek(texturesOffsetTriangles + (i * 12) + 6, SeekOrigin.Begin);
                        ushort texturePage = (ushort)(input.ReadUInt16() & 0x3f);

                        data.Seek(texturesOffsetTriangles + (i * 12) + (j * 4), SeekOrigin.Begin);
                        float u = (float)(input.ReadByte() + (texturePage << 7)) / (texWidth - 1);
                        float v = (float)input.ReadByte() / (texHeight - 1);

                        //Adiciona vetores ao Buffer
                        buffer.Add(new CustomVertex.PositionNormalTextured(position, normal, u, v));
                    }
                }

                //Quadrados
                for (int i = 0; i < facesCountQuads; i++)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        //Nota: Forma 2 triângulos com 1 quadrado
                        data.Seek(facesOffsetQuads + (i * 16) + (quadOrder[j] * 4), SeekOrigin.Begin);
                        ushort normalIndex = input.ReadUInt16();
                        ushort positionIndex = input.ReadUInt16();

                        //Carrega vetor de posição
                        data.Seek(positionOffsetQuads + (positionIndex * 8), SeekOrigin.Begin);
                        float vx = -(float)input.ReadInt16() / scale;
                        float vy = -(float)input.ReadInt16() / scale;
                        float vz = -(float)input.ReadInt16() / scale;
                        Vector3 position = new Vector3(vx, vy, vz);

                        //Carrega vetor da normal
                        data.Seek(normalOffsetQuads + (normalIndex * 8), SeekOrigin.Begin);
                        float nx = -(float)input.ReadInt16() / short.MaxValue;
                        float ny = -(float)input.ReadInt16() / short.MaxValue;
                        float nz = -(float)input.ReadInt16() / short.MaxValue;
                        Vector3 normal = new Vector3(nx, ny, nz);

                        //Carrega vetor da textura
                        data.Seek(texturesOffsetQuads + (i * 16) + 6, SeekOrigin.Begin);
                        ushort texturePage = (ushort)(input.ReadUInt16() & 0x3f);

                        data.Seek(texturesOffsetQuads + (i * 16) + (quadOrder[j] * 4), SeekOrigin.Begin);
                        float u = (float)(input.ReadByte() + (texturePage << 7)) / (texWidth - 1);
                        float v = (float)input.ReadByte() / (texHeight - 1);

                        //Adiciona vetores ao Buffer
                        buffer.Add(new CustomVertex.PositionNormalTextured(position, normal, u, v));
                    }
                }

                modelObject obj = new modelObject();
                obj.vertexBuffer = buffer.ToArray();
                obj.transform = Matrix.Identity;
                mdl.Add(obj);

                data.Seek(dataPosition, SeekOrigin.Begin);
            }

            model = mdl.ToArray();

            //Animação
            List<List<List<modelAnimation>>> rawAnimation = new List<List<List<modelAnimation>>>();
            for (int animSection = 0; animSection < animationFrames.Count; animSection++)
            {
                List<List<modelAnimation>> sectionAnimation = new List<List<modelAnimation>>();
                for (int i = 0; i < animationFrames[animSection].Count; i++)
                {
                    List<modelAnimation> anim = new List<modelAnimation>();
                    for (int j = 0; j < animationFrames[animSection][i].Count; j++)
                    {
                        data.Seek(skeletonAnimationOffset[animSection] + (animationFrames[animSection][i][j] * skeletonAnimationEntryLength[animSection]), SeekOrigin.Begin);

                        modelAnimation frame = new modelAnimation();
                        List<Matrix> rotation = new List<Matrix>();
                        frame.translation = new Vector3(-(float)input.ReadInt16() / scale, -(float)input.ReadInt16() /  scale, (float)input.ReadInt16() / scale);
                        frame.speed = new Vector3(input.ReadInt16(), input.ReadInt16(), input.ReadInt16());

                        while (rotation.Count < model.Length)
                        {
                            /*
                             * Cada vetor da rotação ocupa 12 bits, o que
                             * significa que eles ficarão desalinhados, e também
                             * significa que eu preciso desse código horrível para
                             * ir lendo até que eles alinhem em 72 bits
                             */
                            uint temp = 0;

                            uint x0 = input.ReadByte(); //X0
                            temp = input.ReadByte();
                            x0 |= ((temp & 0xf) << 8);
                            uint y0 = temp >> 4; //Y0
                            y0 |= ((uint)input.ReadByte() << 4);
                            uint z0 = input.ReadByte(); //Z0
                            temp = input.ReadByte();
                            z0 |= ((temp & 0xf) << 8);

                            rotation.Add(Matrix.RotationZ(cAngle(z0)) * Matrix.RotationY(cAngle(y0)) * Matrix.RotationX(cAngle(x0)));
                            if (rotation.Count >= model.Length) break;

                            uint x1 = temp >> 4; //X1
                            x1 |= ((uint)input.ReadByte() << 4);
                            uint y1 = input.ReadByte(); //Y1
                            temp = input.ReadByte();
                            y1 |= ((temp & 0xf) << 8);
                            uint z1 = temp >> 4; //Z1
                            z1 |= ((uint)input.ReadByte() << 4);

                            rotation.Add(Matrix.RotationZ(cAngle(z1)) * Matrix.RotationY(cAngle(y1)) * Matrix.RotationX(cAngle(x1)));
                        }

                        frame.rotation = rotation.ToArray();
                        anim.Add(frame);
                    }
                    sectionAnimation.Add(anim);
                }
                rawAnimation.Add(sectionAnimation);
            }

            animations = new modelAnimation[rawAnimation.Count][][];
            for (int animSection = 0; animSection < rawAnimation.Count; animSection++)
            {
                animations[animSection] = new modelAnimation[rawAnimation[animSection].Count][];
                for (int i = 0; i < rawAnimation[animSection].Count; i++)
                {
                    animations[animSection][i] = new modelAnimation[rawAnimation[animSection][i].Count];
                    for (int j = 0; j < rawAnimation[animSection][i].Count; j++)
                    {
                        animations[animSection][i][j].speed = rawAnimation[animSection][i][j].speed;
                        animations[animSection][i][j].translation = rawAnimation[animSection][i][j].translation;
                        animations[animSection][i][j].rotation = new Matrix[rawAnimation[animSection][i][j].rotation.Length];
                        for (int k = 0; k < rawAnimation[animSection][i][j].rotation.Length; k++)
                        {
                            animations[animSection][i][j].rotation[k] = Matrix.Identity;
                        }
                    }
                }
            }

            for (int animSection = 0; animSection < rawAnimation.Count; animSection++)
            {
                for (int i = 0; i < rawAnimation[animSection].Count; i++)
                {
                    for (int j = 0; j < rawAnimation[animSection][i].Count; j++)
                    {
                        for (int k = 0; k < rawAnimation[animSection][i][j].rotation.Length; k++)
                        {
                            bool found = false;
                            applyAnimationTransform(ref animations[animSection][i][j].rotation[k], 0, k, objectTree, rawAnimation[animSection][i][j].rotation, relativePositions, ref found);
                        }
                    }
                }
            }

            //Transformação básica do modelo
            for (int entry = 0; entry < relativePositions.Count; entry++)
            {
                applyTransform(entry, objectTree, relativePositions[entry]);
            }

            //Monta uma Bounding Box com o modelo final, não é absolutamente necessário
            foreach (modelObject obj in model)
            {
                foreach (CustomVertex.PositionNormalTextured vertex in obj.vertexBuffer)
                {
                    buildBBox(Vector3.Transform(vertex.Position, obj.transform));
                }
            }
        }

        private void applyTransform(int index, List<List<byte>> tree, Matrix mtx)
        {
            model[index].transform *= mtx;

            foreach (byte child in tree[index])
            {
                applyTransform(child, tree, mtx);
            }
        }

        private void applyAnimationTransform(ref Matrix targetMatrix, int index, int target, List<List<byte>> tree, Matrix[] rotations, List<Matrix> relativePositions, ref bool targetFound)
        {
            if (index == target) targetFound = true;
            foreach (byte child in tree[index])
            {
                if (targetFound) break;
                applyAnimationTransform(ref targetMatrix, child, target, tree, rotations, relativePositions, ref targetFound);
            }
            if (targetFound) targetMatrix *= rotations[index] * relativePositions[index];
        }

        private float cAngle(uint angle)
        {
            return ((float)angle / 4096) * ((float)Math.PI * 2);
        }

        private void buildBBox(Vector4 vertex)
        {
            if (vertex.X < minVector.X) minVector.X = vertex.X;
            else if (vertex.X > maxVector.X) maxVector.X = vertex.X;
            else if (vertex.Y < minVector.Y) minVector.Y = vertex.Y;
            else if (vertex.Y > maxVector.Y) maxVector.Y = vertex.Y;
            else if (vertex.Z < minVector.Z) minVector.Z = vertex.Z;
            else if (vertex.Z > maxVector.Z) maxVector.Z = vertex.Z;
        }

        /// <summary>
        ///     Inicia o loop de renderização do modelo.
        ///     Note que é necessário inicializar o renderizador primeiro!
        /// </summary>
        public void render()
        {
            keepRendering = true;
            
            while (keepRendering)
            {
                device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, 0x1f1f1f, 1.0f, 0);
                device.BeginScene();

                Material material = new Material();
                material.Diffuse = Color.White;
                material.Ambient = Color.White;
                device.Material = material;

                int index = 0;
                foreach (modelObject obj in model)
                {
                    device.Transform.World = obj.transform;
                    if (animationGroup > -1)
                    {
                        device.Transform.World = animations[animationGroup][animationIndex][frame].rotation[index];
                        device.Transform.World *= Matrix.Translation(animations[animationGroup][animationIndex][frame].translation);
                    }
                    Matrix rotationMatrix = Matrix.RotationYawPitchRoll(rotation.X / 200.0f, rotation.Y / 200.0f, 0);
                    Matrix translationMatrix = Matrix.Translation(new Vector3(translation.X / 50.0f, translation.Y / 50.0f, zoom));
                    device.Transform.World *= rotationMatrix * translationMatrix;

                    device.VertexFormat = CustomVertex.PositionNormalTextured.Format;
                    VertexBuffer vertexBuffer = new VertexBuffer(typeof(CustomVertex.PositionNormalTextured), obj.vertexBuffer.Length, device, Usage.None, CustomVertex.PositionNormalTextured.Format, Pool.Managed);
                    vertexBuffer.SetData(obj.vertexBuffer, 0, LockFlags.None);
                    device.SetStreamSource(0, vertexBuffer, 0);

                    device.DrawPrimitives(PrimitiveType.TriangleList, 0, obj.vertexBuffer.Length / 3);
                    vertexBuffer.Dispose();

                    index++;
                }

                device.EndScene();
                device.Present();

                if (animationGroup > -1) frame = (frame + 1) % animations[animationGroup][animationIndex].Length;
                else if (frame != 0) frame = 0;

                Application.DoEvents();
            }
        }

        /// <summary>
        ///     Stops the animation.
        ///     You must set the Animation Group again to play it.
        ///     You must call this before changing the Animation Index.
        /// </summary>
        public void stopAnimation()
        {
            animationGroup = -1;
            frame = 0;
        }
    }
}
