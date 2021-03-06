// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Runtime.InteropServices;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Xenko.Graphics
{
    public class ImageHelper
    {
        internal static DataSerializer<ImageDescription> ImageDescriptionSerializer = SerializerSelector.Default.GetSerializer<ImageDescription>();
        internal static readonly FourCC MagicCode = "TKTX";
        
        public static unsafe Image LoadFromMemory(IntPtr pSource, int size, bool makeACopy, GCHandle? handle)
        {
            var stream = new BinarySerializationReader(new NativeMemoryStream((byte*)pSource, size));

            // Read and check magic code
            var magicCode = stream.ReadUInt32();
            if (magicCode != MagicCode)
                return null;

            // Read header
            var imageDescription = new ImageDescription();
            ImageDescriptionSerializer.Serialize(ref imageDescription, ArchiveMode.Deserialize, stream);

            if (makeACopy)
            {
                var buffer = Utilities.AllocateMemory(size);
                Utilities.CopyMemory(buffer, pSource, size);
                pSource = buffer;
                makeACopy = false;
            }

            var image = new Image(imageDescription, pSource, 0, handle, !makeACopy);

            var totalSizeInBytes = stream.ReadInt32();
            if (totalSizeInBytes != image.TotalSizeInBytes)
                throw new InvalidOperationException("Image size is different than expected.");

            // Read image data
            stream.Serialize(image.DataPointer, image.TotalSizeInBytes);

            return image;
        }

        public static void SaveFromMemory(PixelBuffer[] pixelBuffers, int count, ImageDescription description, System.IO.Stream imageStream)
        {
            var stream = new BinarySerializationWriter(imageStream);

            // Write magic code
            stream.Write(MagicCode);

            // Write image header
            ImageDescriptionSerializer.Serialize(ref description, ArchiveMode.Serialize, stream);

            // Write total size
            int totalSize = 0;
            foreach (var pixelBuffer in pixelBuffers)
                totalSize += pixelBuffer.BufferStride;

            stream.Write(totalSize);

            // Write buffers contiguously
            foreach (var pixelBuffer in pixelBuffers)
            {
                stream.Serialize(pixelBuffer.DataPointer, pixelBuffer.BufferStride);
            }
        }
    }
}
