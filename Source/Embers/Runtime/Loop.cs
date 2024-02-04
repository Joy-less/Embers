using System;
using System.Collections;
using System.Collections.Generic;

namespace Embers {
    internal static class Loop {
        public static Instance? Each(IEnumerable Enumerable, Func<object?, long, Instance> CallBlock) {
            Retry:
            long Index = 0;
            foreach (object? Item in Enumerable) {
                Redo:
                Instance Instance = CallBlock(Item, Index);
                // Control code
                if (Instance is ControlCode ControlCode) {
                    switch (ControlCode.Type) {
                        // Break
                        case ControlType.Break:
                            return null;
                        // Next
                        case ControlType.Next:
                            continue;
                        // Redo
                        case ControlType.Redo:
                            goto Redo;
                        // Retry
                        case ControlType.Retry:
                            goto Retry;
                        // Return
                        case ControlType.Return:
                            return ControlCode;
                        // Invalid
                        default:
                            throw new SyntaxError($"{ControlCode.Location}: invalid {ControlCode}");
                    }
                }
                // Next index
                Index++;
            }
            return null;
        }
        public static Instance? Each<T>(IEnumerable<T> Enumerable, Func<T, long, Instance> CallBlock) {
            return Each((IEnumerable)Enumerable, (Item, Index) => CallBlock((T)Item!, Index));
        }
        public static Instance? Each(Context Context, IEnumerable Enumerable, Proc Block, bool PassIndex = true) {
            return Each(Enumerable, (Item, Index) =>
                Block.ArgumentCount switch {
                    >= 2 when PassIndex => Block.Call(Adapter.GetInstance(Context, Item), new Instance(Context.Axis.Integer, Index)),
                    >= 1 => Block.Call(Adapter.GetInstance(Context, Item)),
                    _ => Block.Call()
                }
            );
        }
        public static object? Each(Context Context, InstanceRange Range, Proc Block) {
            if (Range.Min.Value is null || Range.Max.Value is null) {
                return null;
            }
            else {
                Integer IntMin = Range.Min.CastInteger;
                Integer IntMax = Range.Max.CastInteger;
                if (Range.ExcludeEnd) {
                    IntMax--;
                }
                return For(Context, IntMin, IntMax, 1, Block);
            }
        }
        public static object? For(Context Context, long Start, long End, long Step, Proc? Block) {
            // If block given, call block for each index
            if (Block is not null) {
                Retry:
                for (long Index = Start; Index < End; Index += Step) {
                    Redo:
                    // Call block
                    Instance Instance = Block.ArgumentCount switch {
                        >= 1 => Block.Call(new Instance(Context.Axis.Integer, (Integer)Index)),
                        _ => Block.Call()
                    };
                    // Control code
                    if (Instance is ControlCode ControlCode) {
                        switch (ControlCode.Type) {
                            // Break
                            case ControlType.Break:
                                return null;
                            // Next
                            case ControlType.Next:
                                continue;
                            // Redo
                            case ControlType.Redo:
                                goto Redo;
                            // Retry
                            case ControlType.Retry:
                                goto Retry;
                            // Return
                            case ControlType.Return:
                                return ControlCode;
                            // Invalid
                            default:
                                throw new SyntaxError($"{ControlCode.Location}: invalid {ControlCode}");
                        }
                    }
                }
                return null;
            }
            // If no block given, return an enumerator
            else {
                IEnumerable<Instance> Each() {
                    for (long Index = Start; Index < End; Index += Step) {
                        yield return new Instance(Context.Axis.Integer, (Integer)Index);
                    }
                }
                return new Enumerator(Each());
            }
        }
        public static object? For(Context Context, Integer Start, Integer End, Integer Step, Proc Block) {
            // Run faster version if within range
            if (End.Value.CanFitInInt64() && Start.Value.CanFitInInt64()) {
                return For(Context, (long)Start, (long)End, (long)Step, Block);
            }
            // Otherwise, run slower version
            IEnumerable<Integer> EnumeratorFor() {
                for (Integer Index = Start; Index < End; Index += Step) {
                    yield return Index;
                }
            }
            return Each(Context, EnumeratorFor(), Block, PassIndex: false);
        }
    }
}
