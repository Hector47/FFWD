﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace PressPlay.FFWD
{
    public class Material : Asset
    {
        [ContentSerializer]
        internal string shader;
        [ContentSerializer]
        public int renderQueue;
        [ContentSerializer(Optional = true)]
        public Color color;
        [ContentSerializer(Optional = true)]
        public Texture2D mainTexture;
        [ContentSerializer(Optional = true)]
        public Vector2 mainTextureOffset = Vector2.zero;
        [ContentSerializer(Optional = true)]
        public Vector2 mainTextureScale = Vector2.one;
        [ContentSerializer(Optional = true)]
        internal bool wrapRepeat;

        private static readonly Dictionary<string, int> textureRenderIndexes = new Dictionary<string, int>();

        public void SetColor(string name, Color color)
        {
            this.color = color;
        }

        protected override void DoLoadAsset(AssetHelper assetHelper)
        {
            // NOTE: We have hardcoded shader values here that should be configurable in some other way
            blendState = BlendState.Opaque;
            if (shader == "iPhone/Particles/Additive Culled")
            {
                blendState = BlendState.Additive;
            } 
            else if (renderQueue == 3000 || (shader ?? "").StartsWith("Trans"))
            {
                blendState = BlendState.AlphaBlend;
            }
            if (shader == "Particles/Multiply (Double)")
            {
                color = new Color(color.r, color.g, color.b, 0.5f);
            }
            CalculateRenderQueue();
        }

        [ContentSerializerIgnore]
        public BlendState blendState { get; private set; }

        internal void SetBlendState(GraphicsDevice device)
        {
            if (device.BlendState != blendState)
            {
                device.BlendState = blendState;
            }
            if (renderQueue == 3000 || (shader ?? "").StartsWith("Trans"))
            {
                device.DepthStencilState = DepthStencilState.DepthRead;
            }
            else
            {
                device.DepthStencilState = DepthStencilState.Default;
            }

            if (wrapRepeat)
            {
                device.SamplerStates[0] = SamplerState.LinearWrap;
            }
            else
            {
                device.SamplerStates[0] = SamplerState.LinearClamp;
            }
        }

        internal float finalRenderQueue = float.MinValue;

        internal void CalculateRenderQueue()
        {
            finalRenderQueue = renderQueue * 10;
            if (blendState == BlendState.AlphaBlend)
            {
                finalRenderQueue += 1000f;
            }
            if (blendState == BlendState.Additive)
            {
                finalRenderQueue += 2000f;
            }
            string texName = (mainTexture == null) ? string.Empty : mainTexture.name;
            if (!textureRenderIndexes.ContainsKey(texName))
            {
                textureRenderIndexes.Add(texName, textureRenderIndexes.Count);
            }
            finalRenderQueue += textureRenderIndexes[texName];
        }

        public static readonly Material Default = new Material();

        internal void SetTextureState(BasicEffect basicEffect)
        {
            if (mainTexture != null)
            {
                basicEffect.TextureEnabled = true;
                basicEffect.Texture = mainTexture;
                basicEffect.DiffuseColor = color;
            }
            else
            {
                basicEffect.TextureEnabled = false;
                basicEffect.DiffuseColor = color;
            }
        }
    }
}
