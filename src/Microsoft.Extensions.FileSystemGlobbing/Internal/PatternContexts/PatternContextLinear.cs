﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts
{
    public abstract class PatternContextLinear
        : PatternContext<PatternContextLinear.FrameData>
    {
        public PatternContextLinear(ILinearPattern pattern)
        {
            Pattern = pattern;
        }

        public override PatternTestResult Test(FileInfoBase file)
        {
            if (IsStackEmpty())
            {
                throw new InvalidOperationException("Can't test file before entering a directory.");
            }

            if(!Frame.IsNotApplicable && IsLastSegment() && TestMatchingSegment(file.Name))
            {
                return PatternTestResult.Success(CalculateStem(file));
            }

            return PatternTestResult.Failed;
        }

        public override void PushDirectory(DirectoryInfoBase directory)
        {
            // copy the current frame
            var frame = Frame;

            if (IsStackEmpty() || Frame.IsNotApplicable)
            {
                // when the stack is being initialized
                // or no change is required.
            }
            else if (!TestMatchingSegment(directory.Name))
            {
                // nothing down this path is affected by this pattern
                frame.IsNotApplicable = true;
            }
            else
            {
                // Determine this frame's contribution to the stem (if any)
                var segment = Pattern.Segments[Frame.SegmentIndex];
                if (frame.InStem || segment.CanProduceStem)
                {
                    frame.InStem = true;
                    frame.StemItems.Add(directory.Name);
                }

                // directory matches segment, advance position in pattern
                frame.SegmentIndex = frame.SegmentIndex + 1;
            }

            PushDataFrame(frame);
        }

        public struct FrameData
        {
            public bool IsNotApplicable;
            public int SegmentIndex;
            public bool InStem;
            private IList<string> _stemItems;

            public IList<string> StemItems
            {
                get { return _stemItems ?? (_stemItems = new List<string>()); }
            }

            public string Stem
            {
                get { return _stemItems == null ? null : string.Join("/", _stemItems
#if NET35 || NET30 || NET20
                        .ToArray()
#endif
                ); }
            }
        }

        protected ILinearPattern Pattern { get; }

        protected bool IsLastSegment()
        {
            return Frame.SegmentIndex == Pattern.Segments.Count - 1;
        }

        protected bool TestMatchingSegment(string value)
        {
            if (Frame.SegmentIndex >= Pattern.Segments.Count)
            {
                return false;
            }

            return Pattern.Segments[Frame.SegmentIndex].Match(value);
        }

        protected string CalculateStem(FileInfoBase matchedFile)
        {
            return MatcherContext.CombinePath(Frame.Stem, matchedFile.Name);
        }
    }
}
