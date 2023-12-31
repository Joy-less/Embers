using Embers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Embers_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Test1() {
            // Basic tests
            AssertEqualToNull("");
            AssertEqual("4", new DynInteger(4));
            AssertEqual("4.0", new DynFloat(4.0));

            // Global variables
            AssertEqual(@"
                $a = true
                return $a
            ", true);

            // Def
            AssertEqual(@"
                def a
                    return 'hiya'
                end
                a
            ", "hiya");

            // Do...end and {...}
            AssertEqual(@"
                $result = 0

                def a
                    yield
                    5
                end
                def b c
                    $result += 2
                end

                b a {
                    $result += 3
                }

                return $result
            ", new DynInteger(5));
            AssertErrors<RuntimeException>(@"
                $result = 0

                def a
                    yield
                    5
                end
                def b c
                    $result += 2
                end

                # Doesn't work:
                b a do
                    $result += 3
                end

                return $result
            ");
            AssertEqual(@"
                def a
                    5
                end
                return a{}.to_s
            ", "5");

            // Assignment
            AssertEqual(@"
                a = (b = 2)
                c = d = 3.0
                return a, b, c, d
            ", new object[] {new DynInteger(2), new DynInteger(2), new DynFloat(3.0), new DynFloat(3.0)});

            // If statements
            AssertEqual(@"
                $result = 0
                
                if true then
                    $result -= 100
                end
                
                if false and true
                    $result += 10
                elsif 0
                    $result += 1
                else
                    $result += 20
                end
                # should return $result implicitly
            ", new DynInteger(-99));

            // Unless
            AssertEqual(@"
                unless false then
                    $result = 1
                else
                    $result = 0
                end
                $result
            ", new DynInteger(1));

            // One-line conditions
            AssertEqual(@"
                $value = 0
                $value += 1 until true
                $value -= 30 if true
                $value
            ", new DynInteger(-30));

            // Lambdas & always return last expression
            AssertEqual(@"
                my_method = lambda do
                    $return_value = 'hi'
                end
                my_method.call
            ", "hi");

            // Monkey patching
            AssertEqual(@"
                class MyClass
                    def a
                        5
                    end
                end
                class MyClass
                    def b
                        6
                    end
                end
                my_instance = MyClass.new
                my_instance.a + my_instance.b
            ", new DynInteger(11));

            // Unsafe API
            {
                string CodeA = @"
                    File.read('dummy')
                ";
                string CodeB = @"
                    class File
                        def self.read

                        end
                    end
                ";
                AssertErrors<RuntimeException>(CodeA, false);
                AssertErrors<RuntimeException>(CodeB, false);
                AssertDoesNotError(CodeB, true);
            }

            // Break & return
            AssertEqual(@"
                $return_val = 0

                def z
                    $return_val += 1
                    return
                    $return_val -= 1000
                end

                $return_val += 4
                loop do
                    $return_val *= 3
                    z
                    break
                    $return_val -= 10000
                end
                $return_val
            ", new DynInteger(13));
            AssertErrors<SyntaxErrorException>(@"
                break
            ");

            // attr_reader
            AssertEqual(@"
                class A
                    attr_reader :b
                    def a
                        @b = 5
                    end
                end
                c = A.new
                c.a
                return c.b
            ", new DynInteger(5));

            // system
            AssertEqual(@"
                return system('echo %date%').chomp
            ", Obj => Obj is StringInstance Str && DateTime.TryParse(Str.String, out _));

            // Arrays
            AssertEqual(@"
                a = [4, 7]
                return a[1], a.count
            ", new object[] {new DynInteger(7), new DynInteger(2)});
            AssertEqual(@"
                arr = [1, 2, 3, 4, 5]
                arr.map {|a| 2*a}
            ", Obj => Obj is ArrayInstance Arr && Arr.Array.Count == 5 && Arr.Array[0].Integer == 2L && Arr.Array[1].Integer == 4L && Arr.Array[2].Integer == 6L
                && Arr.Array[3].Integer == new DynInteger(8) && Arr.Array[4].Integer == new DynInteger(10));

            // Unary
            AssertEqual(@"
                a = -1
                b = - 2
                c = (- 3.4)
                d = (true ? 5 : 2) + 6
                return a, b, c, d
            ", new object[] {new DynInteger(-1), new DynInteger(-2), new DynFloat(-3.4), new DynInteger(11)});

            // Hashes
            AssertEqual(@"
                a = {:hi => 56.1}
                b = a[:hi]
                a[:hi] = 21
                return b, a[:hi]
            ", new object[] {new DynFloat(56.1), new DynInteger(21) });

            // Splat Arguments
            AssertEqual(@"
                def a(*b, **c)
                    $b = b
                    $c = c
                end

                a true, 5, 8, ""hi"" => 2.4, ""hey"" => :test
                return $b, $c
            ", Obj => Obj is ArrayInstance Objs && Objs.Array.Count == 2 && Objs.Array[0] is ArrayInstance Arr && Arr.Array[0].Boolean == true && Arr.Array[1].Integer == new DynInteger(5)
                && Arr.Array[2].Integer == new DynInteger(8) && Objs.Array[1] is HashInstance Hash && Hash.Hash.Count == 2);

            // Ranges
            AssertEqual(@"
                return (5..7).max, (5...7).max
            ", new object[] {new DynInteger(7), new DynInteger(6)});
            AssertEqual(@"
                return 'Hi there'[2..10]
            ", " there");

            // Operator overloading
            AssertEqual(@"
                class Z
                    def +(value)
                        'Zzz' + value
                    end
                    def <=(value)
                        'No idea'
                    end
                end
                y = Z.new
                return y + '...', y <= 5
            ", new object[] {"Zzz...", "No idea"});

            // Exclamation methods
            AssertEqual(@"
                arr = [1, 2, 4, 3]
                arr.sort!
                arr == [1, 2, 3, 4]
            ", true);

            // Inheritance
            AssertEqual(@"
                class SuperString < String
                    def use_powers s
                        ""#{s} is dead.""
                    end
                end
                s = SuperString.new 'Someone'
                puts s.rstrip
                s.use_powers s
            ", "Someone is dead.");

            // Defined?
            AssertEqual(@"
                a = 5
                return defined? a, defined? b
            ", new object?[] {"local-variable", null});

            // Case when
            AssertEqual(@"
                case 52
                when 1..3
                    return 'Between one and three'
                when 52
                    return '52!'
                else
                    return 'None of the above'
                end
            ", "52!");
            AssertEqual(@"
                a = case 14
                when 146
                    return '146!'
                else
                    return 'N/A'
                end
                a
            ", "N/A");

            // Access Modifiers
            AssertDoesNotError(@"
                class A
                    def puts
                        super 'Hi'
                    end
                end
                A.new.puts
            ");
            AssertErrors<RuntimeException>(@"
                class A
                    private
                    def puts
                        super 'Hi'
                    end
                end
                A.new.puts
            ");
            AssertErrors<RuntimeException>(@"
                class A
                    protected
                    def puts
                        super 'Hi'
                    end
                end
                A.new.puts
            ");
            AssertDoesNotError(@"
                class A
                    protected
                    def a
                        'Hi'
                    end
                end
                class B < A
                    def b
                        a
                    end
                end
                B.new.b
            ");

            // Await test
            AssertDoesNotError(@"
                class A
                    def a
                        b = 0
                        sleep 0.1
                        b
                    end
                end
                A.new.a
            ");

            // For
            AssertEqual(@"
                $a = nil
                $b = nil
                for i, v in {:a => :b}
                    $a, $b = i, v
                end
                return $a, $b
            ", new object[] {"a", "b"});

            // Multiple assignment
            AssertEqual(@"
                $c = (a, b = 1, 2)
                $d, $e = [5, 6]
                return a, b, $c[0], $d, $e
            ", new object[] {new DynInteger(1), new DynInteger(2), new DynInteger(1), new DynInteger(5), new DynInteger(6)});

            // Or & not priority
            // Not works differently in Ruby, but imo, it's better in Embers.
            AssertEqual(@"
                a = 5
                b = a or 2
                c = not 2 or 3
                return b, c
            ", new object[] {new DynInteger(5), new DynInteger(3)});

            // Clone
            AssertEqual(@"
                a = 5
                b = a.clone
                a.object_id == b.object_id
            ", false);

            // Class
            AssertEqual(@"
                a = 5
                [a.class.name, a.class.class.name]
            ", new object[] {"Integer", "Class"});

            // Private / protected
            AssertErrors<RuntimeException>(@"
                class MyClass
                end
                MyClass.local_variables
            ");

            // __LINE__
            AssertEqual(@"
                __LINE__
            ", new DynInteger(2));

            // Yield return values
            AssertEqual(@"
                def a
                    yield ""hi""
                end
                a {|x| 7.to_s + x}
            ", "7hi");

            // Throw catch
            AssertEqual(@"
                catch :a do
                    if true
                        throw :a
                    end
                    return 5
                end
                return 6
            ", new DynInteger(6));

            // Precision
            AssertEqual(@"
                a = (10 ** 26 + 1) == (10 ** 26 + 1)
                b = (10 ** 26.0 + 1) == (10 ** 26.0 + 1)
                c = (10.0 ** 26 + 1) == (10.0 ** 26 + 1)
                d = (Float::INFINITY - Float::INFINITY).to_s
                return a, b, c, d
            ", new object?[] {true, true, true, "NaN"});
        }


        // Helper methods
        public static void AssertEqual(string Code, object?[] ExpectedResults, bool AllowUnsafeApi = true) {
            Instance Result = new Scope(null, AllowUnsafeApi).Evaluate(Code);

            Assert.IsTrue(Result is ArrayInstance);
            List<Instance> Results = Result.Array;
            Assert.AreEqual(ExpectedResults.Length, Results.Count, "Wrong number of objects.");

            for (int i = 0; i < ExpectedResults.Length; i++) {
                Assert.AreEqual(ExpectedResults[i], Results[i].Object);
            }
        }
        public static void AssertEqual(string Code, object? ExpectedResult, bool AllowUnsafeApi = true) {
            Instance Result = new Scope(null, AllowUnsafeApi).Evaluate(Code);

            Assert.AreEqual(ExpectedResult, Result.Object);
        }
        public static void AssertEqualToNull(string Code, bool AllowUnsafeApi = true) {
            Instance Result = new Scope(null, AllowUnsafeApi).Evaluate(Code);

            Assert.AreEqual(null, Result.Object);
        }
        public static void AssertEqual(string Code, Func<object?, bool> CheckEquality, bool AllowUnsafeApi = true) {
            Instance Result = new Scope(null, AllowUnsafeApi).Evaluate(Code);

            bool AreEqual = CheckEquality(Result);
            Assert.IsTrue(AreEqual);
        }
        public static void AssertErrors<TError>(string Code, bool AllowUnsafeApi = true) {
            try {
                new Scope(null, AllowUnsafeApi).Evaluate(Code);
            }
            catch (Exception Ex) {
                if (Ex.InnerException is null) {
                    Assert.Fail("This test function needs to be re-written.");
                }
                else if (Ex.InnerException is not TError) {
                    Assert.Fail($"Error was wrong type (expected {typeof(TError).Name}, got {Ex.InnerException.GetType().Name}: '{Ex.InnerException.Message}').");
                }
                return;
            }
            Assert.Fail("Code did not error.");
        }
        public static void AssertErrors(string Code) {
            AssertErrors<EmbersException>(Code);
        }
        public static void AssertDoesNotError(string Code, bool AllowUnsafeApi = true) {
            try {
                new Scope(null, AllowUnsafeApi).Evaluate(Code);
            }
            catch (Exception Ex) {
                Assert.Fail($"Code errored ({Ex.GetType().Name}): {Ex.Message}.");
            }
        }
    }
}