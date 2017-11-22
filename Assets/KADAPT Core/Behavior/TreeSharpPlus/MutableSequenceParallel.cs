#region License

// A simplistic Behavior Tree implementation in C#
// Copyright (C) 2010-2011 ApocDev apocdev@gmail.com
// 
// This file is part of TreeSharp
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

// TODO: THIS WAS A NEW FILE -- MODIFY THIS HEADER
#endregion

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace TreeSharpPlus
{
    /// <summary>
    ///    Parallel Sequence nodes execute all of their children in parallel. If any
    ///    sequence reports failure, we finish all of the other ticks, but then stop
    ///    all other children and report failure. We report success when all children
    ///    report success.
    /// </summary>
    public class MutableSequenceParallel : Parallel
    {
        public MutableSequenceParallel(params Node[] children)
            : base(children)
        {
        }

        public override IEnumerable<RunStatus> Execute()
        {
            while (true)
            {
                for (int i = 0; i < Length(); i++)
                {
                    Node node = this.Children[i];
                    RunStatus tickResult = TickNode(node);

                    if (tickResult != RunStatus.Running)
                    {
                        node.Stop();
                        Remove(node);

                        if (tickResult == RunStatus.Failure)
                        {
                            while (this.TerminateChildren() == RunStatus.Running)
                            {
                                yield return RunStatus.Running;
                            }
                            yield return RunStatus.Failure;
                            yield break;
                        }
                    }
                }

                // If we're out of running nodes, we're done
                if (Length() == 0)
                {
                    yield return RunStatus.Success;
                    yield break;
                }

                // For forked ticking
                yield return RunStatus.Running;
            }
        }
    }
}