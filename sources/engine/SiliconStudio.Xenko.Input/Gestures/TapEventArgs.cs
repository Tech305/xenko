﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Gestures
{
    /// <summary>
    /// Event class for the Tap gesture.
    /// </summary>
    public sealed class TapEventArgs : PointerGestureEventArgs
    {
        public TapEventArgs(IPointerDevice pointerDevice, TimeSpan takenTime, int fingerCount, int tapCount, Vector2 position)
            : base(pointerDevice)
        {
            EventType = PointerGestureEventType.Occurred;
            DeltaTime = takenTime;
            TotalTime = takenTime;
            FingerCount = fingerCount;
            TapCount = tapCount;
            TapPosition = position;
        }

        /// <summary>
        /// The number of time the use successively touched the screen.
        /// </summary>
        public int TapCount { get; internal set; }

        /// <summary>
        /// The position (in pixels) of the tap.
        /// </summary>
        public Vector2 TapPosition { get; internal set; }

    }
}