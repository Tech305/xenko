﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using NUnit.Framework;

using SiliconStudio.Assets;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Texture;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Tests
{
    [TestFixture]
    public class TestTexturePacker
    {
        public static void LoadParadoxAssemblies()
        {
            RuntimeHelpers.RunModuleConstructor(typeof(Asset).Module.ModuleHandle);
        }

        [TestFixtureSetUp]
        public void InitializeTest()
        {
            LoadParadoxAssemblies();
            Game.InitializeAssetDatabase();
        }

        [Test]
        public void TestMaxRectsPackWithoutRotation()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, false);

            // This data set remain only 1 rect that cant be packed
            var packRectangles = new List<RotatableRectangle>
            {
                new RotatableRectangle(0, 0, 80, 100), new RotatableRectangle(0, 0, 100, 20),
            };

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(1, maxRectPacker.UsedRectangles.Count);
        }

        [Test]
        public void TestMaxRectsPackWithRotation()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, true);

            // This data set remain only 1 rect that cant be packed
            var packRectangles = new List<RotatableRectangle>
            {
                new RotatableRectangle(0, 0, 80, 100) { Key = "A" }, new RotatableRectangle(0, 0, 100, 20) { Key = "B"},
            };

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(0, packRectangles.Count);
            Assert.AreEqual(2, maxRectPacker.UsedRectangles.Count);
            Assert.IsTrue(maxRectPacker.UsedRectangles.Find(rectangle => rectangle.Key == "B").IsRotated);
        }

        /// <summary>
        /// Test packing 7 rectangles
        /// </summary>
        [Test]
        public void TestMaxRectsPackArbitaryRectangles()
        {
            var maxRectPacker = new MaxRectanglesBinPack();
            maxRectPacker.Initialize(100, 100, true);

            // This data set remain only 1 rect that cant be packed
            var packRectangles = new List<RotatableRectangle>
            {
                new RotatableRectangle(0, 0, 55, 70), new RotatableRectangle(0, 0, 55, 30),
                new RotatableRectangle(0, 0, 25, 30), new RotatableRectangle(0, 0, 20, 30),
                new RotatableRectangle(0, 0, 45, 30),
                new RotatableRectangle(0, 0, 25, 40), new RotatableRectangle(0, 0, 20, 40)
            };

            maxRectPacker.PackRectangles(packRectangles);

            Assert.AreEqual(1, packRectangles.Count);
            Assert.AreEqual(6, maxRectPacker.UsedRectangles.Count);
        }

        [Test]
        public void TestTexturePackerFitAllElements()
        {
            var textureElements = CreateFakeTextureElements();

            var packConfiguration = new Configuration
            {
                BorderSize = 0,
                UseMultipack = false,
                UseRotation = true,
                PivotType = PivotType.Center,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Dispose image
            foreach (var texture in texturePacker.TextureAtlases.SelectMany(textureAtlas => textureAtlas.Textures))
                texture.Texture.Dispose();
        }

        public Dictionary<string, IntermediateTextureElement> CreateFakeTextureElements()
        {
            var textureElements = new Dictionary<string, IntermediateTextureElement>();

            textureElements.Add("A", new IntermediateTextureElement
            {
                Texture = Image.New2D(100, 200, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            textureElements.Add("B", new IntermediateTextureElement
            {
                Texture = Image.New2D(400, 300, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            return textureElements;
        }

        [Test]
        public void TestTexturePackerWithMultiPack()
        {
            var textureAtlases = new List<TextureAtlas>();
            var textureElements = CreateFakeTextureElements();

            var packConfiguration = new Configuration
            {
                BorderSize = 0,
                UseMultipack = true,
                UseRotation = true,
                PivotType = PivotType.Center,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 300,
                MaxWidth = 300
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.AreEqual(1, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);
            Assert.IsFalse(canPackAllTextures);

            // The current bin cant fit all of textures, resize the bin
            var newPackConfiguration = new Configuration
            {
                BorderSize = 0,
                UseMultipack = true,
                UseRotation = true,
                PivotType = PivotType.Center,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 1500,
                MaxWidth = 800
            };

            texturePacker.ResetPacker(newPackConfiguration);

            canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.IsTrue(canPackAllTextures);
            Assert.AreEqual(0, textureElements.Count);
            Assert.AreEqual(2, textureAtlases.Count);

            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));

            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[1].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[1].Height));

            // Dispose image
            foreach (var texture in textureAtlases.SelectMany(textureAtlas => textureAtlas.Textures))
                texture.Texture.Dispose();
        }

        [Test]
        public void TestTexturePackerWithBorder()
        {
            var textureAtlases = new List<TextureAtlas>();

            var textureElements = new Dictionary<string, IntermediateTextureElement>();

            textureElements.Add("A", new IntermediateTextureElement
            {
                Texture = Image.New2D(100, 200, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            textureElements.Add("B", new IntermediateTextureElement
            {
                Texture = Image.New2D(57, 22, 1, PixelFormat.R8G8B8A8_UNorm)
            });

            var packConfiguration = new Configuration
            {
                BorderSize = 2,
                UseMultipack = true,
                UseRotation = true,
                PivotType = PivotType.Center,
                SizeContraint = SizeConstraints.PowerOfTwo,
                MaxHeight = 512,
                MaxWidth = 512
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);
            textureAtlases.AddRange(texturePacker.TextureAtlases);

            Assert.IsTrue(canPackAllTextures);
            Assert.AreEqual(0, textureElements.Count);
            Assert.AreEqual(1, textureAtlases.Count);

            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));

            // Test if border is applied in width and height
            var textureA = textureAtlases[0].Textures.Find(rectangle => rectangle.Region.Key == "A");
            var textureB = textureAtlases[0].Textures.Find(rectangle => rectangle.Region.Key == "B");

            Assert.AreEqual(textureA.Texture.Description.Width + 2 * packConfiguration.BorderSize, textureA.Region.Value.Width);
            Assert.AreEqual(textureA.Texture.Description.Height + 2 * packConfiguration.BorderSize, textureA.Region.Value.Height);

            Assert.AreEqual(textureB.Texture.Description.Width + 2 * packConfiguration.BorderSize,
                (!textureB.Region.IsRotated) ? textureB.Region.Value.Width : textureB.Region.Value.Height);
            Assert.AreEqual(textureB.Texture.Description.Height + 2 * packConfiguration.BorderSize,
                (!textureB.Region.IsRotated) ? textureB.Region.Value.Height : textureB.Region.Value.Width);
        }

        public Image CreateMockTexture(int width, int height)
        {
            var texture = Image.New2D(width, height, 1, PixelFormat.R8G8B8A8_UNorm);

            unsafe
            {
                var ptr = (Color*)texture.DataPointer;

                // Fill in mock data
                for (var y = 0; y < height; ++y)
                    for (var x = 0; x < width; ++x)
                    {
                        ptr[y * width + x] = y < height / 2 ? Color.MediumPurple : Color.White;
                    }
            }

            return texture;
        }

        [Test]
        public void TestTextureAtlasFactory()
        {
            var textureElements = new Dictionary<string, IntermediateTextureElement>();

            var mockTexture = CreateMockTexture(100, 200);

            // Load a test texture asset
            textureElements.Add("A", new IntermediateTextureElement
            {
                Texture = mockTexture
            });

            var packConfiguration = new Configuration
            {
                BorderSize = 0,
                SizeContraint = SizeConstraints.PowerOfTwo,
                UseMultipack = false,
                UseRotation = true,
                PivotType = PivotType.Center,
                MaxHeight = 2000,
                MaxWidth = 2000
            };

            var texturePacker = new TexturePacker(packConfiguration);

            var canPackAllTextures = texturePacker.PackTextures(textureElements);

            Assert.IsTrue(canPackAllTextures);

            // Obtain texture atlases
            var textureAtlases = texturePacker.TextureAtlases;

            Assert.AreEqual(1, textureAtlases.Count);
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Width));
            Assert.IsTrue(TextureCommandHelper.IsPowerOfTwo(textureAtlases[0].Height));

            // Create atlas texture
            var atlasTexture = TextureAtlasFactory.CreateTextureAtlas(textureAtlases[0]);

            Assert.AreEqual(textureAtlases[0].Width, atlasTexture.Description.Width);
            Assert.AreEqual(textureAtlases[0].Height, atlasTexture.Description.Height);

            mockTexture.Dispose();
            atlasTexture.Dispose();
        }

        [Test]
        public void TestWrapBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(15, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(19, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(25, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(29, 10, TextureAddressMode.Wrap));

            // Negative sets
            Assert.AreEqual(6, TextureAtlasFactory.GetSourceTextureIndex(-4, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(6, TextureAtlasFactory.GetSourceTextureIndex(-14, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-19, 10, TextureAddressMode.Wrap));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(6, TextureAtlasFactory.GetSourceTextureIndex(-24, 10, TextureAddressMode.Wrap));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-29, 10, TextureAddressMode.Wrap));
        }

        [Test]
        public void TestClampBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(15, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(19, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(25, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(29, 10, TextureAddressMode.Clamp));

            // Negative sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-4, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-14, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-19, 10, TextureAddressMode.Clamp));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-24, 10, TextureAddressMode.Clamp));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-29, 10, TextureAddressMode.Clamp));
        }

        [Test]
        public void TestMirrorBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(11, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(7, TextureAtlasFactory.GetSourceTextureIndex(12, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(21, 10, TextureAddressMode.Mirror));

            // Negative Sets
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-1, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(2, TextureAtlasFactory.GetSourceTextureIndex(-2, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(3, TextureAtlasFactory.GetSourceTextureIndex(-3, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-11, 10, TextureAddressMode.Mirror));

            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.Mirror));
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-21, 10, TextureAddressMode.Mirror));
        }

        [Test]
        public void TestMirrorOnceBorderMode()
        {
            // Positive sets
            Assert.AreEqual(0, TextureAtlasFactory.GetSourceTextureIndex(0, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(5, TextureAtlasFactory.GetSourceTextureIndex(5, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(9, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(10, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(11, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(7, TextureAtlasFactory.GetSourceTextureIndex(12, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(20, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(21, 10, TextureAddressMode.MirrorOnce));

            // Negative Sets
            Assert.AreEqual(1, TextureAtlasFactory.GetSourceTextureIndex(-1, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(2, TextureAtlasFactory.GetSourceTextureIndex(-2, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(3, TextureAtlasFactory.GetSourceTextureIndex(-3, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-9, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-10, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(-11, 10, TextureAddressMode.MirrorOnce));

            Assert.AreEqual(9, TextureAtlasFactory.GetSourceTextureIndex(-20, 10, TextureAddressMode.MirrorOnce));
            Assert.AreEqual(8, TextureAtlasFactory.GetSourceTextureIndex(-21, 10, TextureAddressMode.MirrorOnce));
        }

        [Test]
        public void TestImageCreationGetAndSet()
        {
            const int Width = 256;
            const int Height = 128;

            var source = Image.New2D(Width, Height, 1, PixelFormat.R8G8B8A8_UNorm);

            Assert.AreEqual(source.TotalSizeInBytes, PixelFormat.R8G8B8A8_UNorm.SizeInBytes() * Width * Height);
            Assert.AreEqual(source.PixelBuffer.Count, 1);

            Assert.AreEqual(1, source.Description.MipLevels);
            Assert.AreEqual(1, source.Description.ArraySize);

            Assert.AreEqual(Width * Height * 4,
                source.PixelBuffer[0].Width * source.PixelBuffer[0].Height * source.PixelBuffer[0].PixelSize);

            // Set Pixel
            var pixelBuffer = source.PixelBuffer[0];
            pixelBuffer.SetPixel(0, 0, (byte)255);

            // Get Pixel
            var fromPixels = pixelBuffer.GetPixels<byte>();
            Assert.AreEqual(fromPixels[0], 255);

            // Dispose images
            source.Dispose();
        }

        [Test]
        public void TestImageDataPointerManipulation()
        {
            const int Width = 256;
            const int Height = 128;

            var source = Image.New2D(Width, Height, 1, PixelFormat.R8G8B8A8_UNorm);

            Assert.AreEqual(source.TotalSizeInBytes, PixelFormat.R8G8B8A8_UNorm.SizeInBytes() * Width * Height);
            Assert.AreEqual(source.PixelBuffer.Count, 1);

            Assert.AreEqual(1, source.Description.MipLevels);
            Assert.AreEqual(1, source.Description.ArraySize);

            Assert.AreEqual(Width * Height * 4,
                source.PixelBuffer[0].Width * source.PixelBuffer[0].Height * source.PixelBuffer[0].PixelSize);

            unsafe
            {
                var ptr = (Color*)source.DataPointer;

                // Clean the data
                for (var i = 0; i < source.PixelBuffer[0].Height * source.PixelBuffer[0].Width; ++i)
                    ptr[i] = Color.Transparent;

                // Set a specific pixel to red
                ptr[0] = Color.Red;
            }

            var pixelBuffer = source.PixelBuffer[0];

            // Get Pixel
            var fromPixels = pixelBuffer.GetPixels<Color>();
            Assert.AreEqual(Color.Red, fromPixels[0]);

            // Dispose images
            source.Dispose();
        }
    }
}
