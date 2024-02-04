using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Embers.UnitTests {
    [TestClass]
    public class UnitTests {
        [TestMethod]
        public void UnitTest() {
            // Basic tests
            AssertEqualToNull("");
            AssertEqual("4", (Integer)4);
            AssertEqual("4.0", (Float)4.0);

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
            ", (Integer)5);
            AssertErrors<RuntimeError>(@"
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
            ", new object[] { (Integer)2, (Integer)2, (Float)3.0, (Float)3.0 });

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
            ", (Integer)(-99));

            // Unless
            AssertEqual(@"
                unless false then
                    $result = 1
                else
                    $result = 0
                end
                $result
            ", (Integer)1);

            // One-line conditions
            AssertEqual(@"
                $value = 0
                $value += 1 until true
                $value -= 30 if true
                $value
            ", (Integer)(-30));

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
            ", (Integer)11);

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
            ", (Integer)13);
            AssertEqual(@"
                break
            ", Instance => Instance is ControlCode ControlCode && ControlCode.Type is ControlType.Break);

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
            ", (Integer)5);

            // Arrays
            AssertEqual(@"
                a = [4, 7]
                return a[1], a.count
            ", new object[] { (Integer)7, (Integer)2 });
            AssertEqual(@"
                arr = [1, 2, 3, 4, 5]
                arr.map {|a| 2*a}
            ", Obj => Obj.Value is Array Arr && Arr.Count == 5
                && Arr[0].CastInteger == (Integer)2 && Arr[1].CastInteger == (Integer)4 && Arr[2].CastInteger == (Integer)6
                && Arr[3].CastInteger == (Integer)8 && Arr[4].CastInteger == (Integer)10);

            // Unary
            AssertEqual(@"
                a = -1
                b = - 2
                c = (- 3.4)
                d = (true ? 5 : 2) + 6
                return a, b, c, d
            ", new object[] { (Integer)(-1), (Integer)(-2), (Float)(-3.4), (Integer)11 });

            // Hashes
            AssertEqual(@"
                a = {:hi => 56.1}
                b = a[:hi]
                a[:hi] = 21
                return b, a[:hi]
            ", new object[] { (Float)56.1, (Integer)21 });

            // Splat Arguments
            AssertEqual(@"
                def a(*b, **c)
                    $b = b
                    $c = c
                end

                a true, 5, 8, ""hi"" => 2.4, ""hey"" => :test
                return $b, $c
            ", Instance => {
                if (Instance.Value is Array Arr && Arr.Count == 2) {
                    if (Arr[0].Value is Array Arr2 && Arr2.Count == 3) {
                        if (Arr2[0].Value is not true) return false;
                        if (5 != Arr2[1].Value as Integer) return false;
                        if (8 != Arr2[2].Value as Integer) return false;

                        if (Arr[1].Value is Hash Hash && Hash.Count == 2) {
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        return false;
                    }
                }
                else {
                    return false;
                }
            });

            // Ranges
            AssertEqual(@"
                return (5..7).max, (5...7).max
            ", new object[] { (Integer)7, (Integer)6 });
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
            ", new object[] { "Zzz...", "No idea" });

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
                s = SuperString.new
                s.use_powers 'Someone'
            ", "Someone is dead.");

            // Defined?
            AssertEqual(@"
                a = 5
                return defined? a, defined? b
            ", new object?[] { "local-variable", null });

            // Case when
            AssertEqual(@"
                case 52
                when 1..3
                    return 'Between one and three'
                when 52, 53
                    return '52 or 53!'
                else
                    return 'None of the above'
                end
            ", "52 or 53!");
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
            AssertErrors<RuntimeError>(@"
                class A
                    private
                    def puts
                        super 'Hi'
                    end
                end
                A.new.puts
            ");
            AssertErrors<RuntimeError>(@"
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
            ", new object[] { "a", "b" });

            // Multiple assignment
            AssertEqual(@"
                $c = (a, b = 1, 2)
                $d, $e = [5, 6]
                return a, b, $c[0], $d, $e
            ", new object[] { (Integer)1, (Integer)2, (Integer)1, (Integer)5, (Integer)6 });

            // Or & not priority
            // Not works differently in Ruby, but imo, it's better in Embers.
            AssertEqual(@"
                a = 5
                b = a or 2
                c = not 2 or 3
                return b, c
            ", new object[] { (Integer)5, (Integer)3 });

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
            ", new object[] { "Integer", "Class" });

            // Private / protected
            AssertErrors<RuntimeError>(@"
                class MyClass
                end
                MyClass.local_variables
            ");

            // __LINE__
            AssertEqual(@"
                __LINE__
            ", (Integer)2);

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
            ", (Integer)6);

            // Precision
            AssertEqual(@"
                a = (10 ** 26 + 1) == (10 ** 26 + 1)
                b = (10 ** 26.0 + 1) == (10 ** 26.0 + 1)
                c = (10.0 ** 26 + 1) == (10.0 ** 26 + 1)
                d = (Float::INFINITY - Float::INFINITY).to_s
                return a, b, c, d
            ", new object?[] { true, true, true, "NaN" });

            // Scope
            AssertErrors<RuntimeError>(@"
                def my_method
                    puts variable
                end
                variable = 10
                my_method
            ");
        }

        // Helper methods
        public static void AssertEqual(string Code, object?[] ExpectedResults) {
            Instance Result = new Scope().Evaluate(Code);

            Assert.AreEqual(ExpectedResults.Length, Result.CastArray.Count, "Wrong number of objects.");

            for (int i = 0; i < ExpectedResults.Length; i++) {
                Assert.AreEqual(ExpectedResults[i], Result.CastArray[i].Value);
            }
        }
        public static void AssertEqual(string Code, object? ExpectedResult) {
            Instance Result = new Scope().Evaluate(Code);

            Assert.AreEqual(ExpectedResult, Result.Value);
        }
        public static void AssertEqualToNull(string Code) {
            Instance Result = new Scope().Evaluate(Code);

            Assert.AreEqual(null, Result.Value);
        }
        public static void AssertEqual(string Code, Func<Instance, bool> CheckEquality) {
            Instance Result = new Scope().Evaluate(Code);

            bool AreEqual = CheckEquality(Result);
            Assert.IsTrue(AreEqual);
        }
        public static void AssertErrors<TError>(string Code) {
            try {
                new Scope().Evaluate(Code);
            }
            catch (Exception Ex) {
                if (Ex is not TError && Ex.InnerException is not TError) {
                    Exception InnerEx = Ex.InnerException ?? Ex;
                    Assert.Fail($"Error was wrong type (expected {typeof(TError).Name}, got {InnerEx.GetType().Name}: '{InnerEx.Message}').");
                }
                return;
            }
            Assert.Fail("Code did not error.");
        }
        public static void AssertErrors(string Code) {
            AssertErrors<EmbersError>(Code);
        }
        public static void AssertDoesNotError(string Code) {
            try {
                new Scope().Evaluate(Code);
            }
            catch (Exception Ex) {
                Assert.Fail($"Code errored ({Ex.GetType().Name}): {Ex.Message}.");
            }
        }
    }
}
