using System;
using System.Collections.Generic;

namespace TreeSharpPlus
{
    public static class TreeUtils
    {
        /// <summary>
        /// Given a collection of objects, will keep calling func on them
        /// until the result is either success or failure for all items.
        /// Returns failure if any of them failed after every call completed.
        /// 
        /// TODO: Maybe make this a yield-based function that keeps a list 
        /// for efficiency - AS
        /// </summary>
        public static RunStatus DoUntilComplete<T>(
            Func<T, RunStatus> func,
            IEnumerable<T> items)
        {
            RunStatus final = RunStatus.Success;
            foreach (T item in items)
            {
                RunStatus rs = func.Invoke(item);
                if (rs == RunStatus.Running)
                    final = RunStatus.Running;
                else if (final != RunStatus.Running && rs == RunStatus.Failure)
                    final = RunStatus.Failure;
            }
            return final;
        }

        public static Node DoIfTrue(Func<bool> f_bool, Node node)
        {
            return new Sequence(new LeafAssert(f_bool), node);
        }
        public static Node WaitUntilTrue(Func<bool> f_bool)
        {
            Func<RunStatus> f_stats = () =>
            {
                if (f_bool())
                    return RunStatus.Success;
                else
                    return RunStatus.Running;
            };
            return new LeafInvoke(f_stats);
        }
        public static Node WaitUntilFalse(Func<bool> f_bool)
        {
            Func<RunStatus> f_stats = () =>
            {
                if (f_bool())
                    return RunStatus.Running;
                else
                    return RunStatus.Failure;
            };
            return new LeafInvoke(f_stats, () => RunStatus.Success);
        }
        public static Node ActionInteruptSuccessWhile(Node action, Func<bool> f_bool)
        {
            return new Race(action, WaitUntilTrue(f_bool));
        }
        public static Node ActionInteruptFailureWhile(Node action, Func<bool> f_bool)
        {
            return new Race(action, WaitUntilFalse(f_bool));
        }

        public static Node TreeNodeTrace(Node action, String nodename, bool enabled)
        {
            if (enabled)
            {
                return new Sequence(
                    new LeafTrace("Node entered: " + nodename),
                    new DecoratorPrintResult(action, nodename)
                );
            }
            else
                return action;
        }

        public static Node LeafInvokeIEnum(IEnumerator<RunStatus> iEnum)
        {
            Func<RunStatus> fEnum = () =>
            {
                iEnum.MoveNext();
                return iEnum.Current;
            };
            return new LeafInvoke(fEnum, () => RunStatus.Success);
        }
    }
}
