using Embers;
using static Embers.Script;
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
            AssertEqual("4", 4L);
            AssertEqual("4.0", 4.0d);

            // Global variables test
            AssertEqual(@"
                $a = true
                return $a
            ", true);

            // Def test
            AssertEqual(@"
                def a
                    return 'hiya'
                end
                a
            ", "hiya");

            // Do...end and {...} tests
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
            ", 5L);
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
            ", new object[] {2L, 2L, 3.0d, 3.0d});

            // If statements
            AssertEqual(@"
                $result = 0
                
                if true then
                    $result -= 100
                end
                
                if false
                    $result += 10
                elsif 0
                    $result += 1
                else
                    $result += 20
                end
            ", -99L);

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
            ", 11L);

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
            ", 13L);
            AssertErrors<SyntaxErrorException>(@"
                break
            ");

            // Lock script while running
            {
                bool Success = false;
                try {
                    Interpreter Interpreter = new();
                    Script Script = new(Interpreter);
                    _ = Script.EvaluateAsync("sleep(2)");
                    Script.Evaluate("puts 'hi'");
                }
                catch (Exception) {
                    Success = true;
                }
                Assert.IsTrue(Success);
            }

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
            ", 5L);

            // system
            AssertEqual(@"
                return system('echo %date%').chomp
            ", Obj => Obj is string Str && DateTime.TryParse(Str, out _));

            // Arrays
            AssertEqual(@"
                a = [4, 7]
                return a[1], a.count
            ", new object[] {7L, 2L});
            AssertEqual(@"
                arr = [1, 2, 3, 4, 5]
                arr.map {|a| 2*a}
            ", Obj => Obj is List<Instance> Arr && Arr.Count == 5 && Arr[0].Integer == 2L && Arr[1].Integer == 4L && Arr[2].Integer == 6L && Arr[3].Integer == 8L && Arr[4].Integer == 10L);
        }


        // Helper methods
        public static void AssertEqual(string Code, object[] ExpectedResults, bool AllowUnsafeApi = true) {
            Instances Results = new Script(new Interpreter(), allowUnsafeApi: AllowUnsafeApi).Evaluate(Code);

            Assert.AreEqual(ExpectedResults.Length, Results.Count, "Wrong number of objects.");

            for (int i = 0; i < ExpectedResults.Length; i++) {
                Assert.AreEqual(ExpectedResults[i], Results[i].Object);
            }
        }
        public static void AssertEqual(string Code, object ExpectedResult, bool AllowUnsafeApi = true) {
            Instances Results = new Script(new Interpreter(), allowUnsafeApi: AllowUnsafeApi).Evaluate(Code);

            Assert.AreEqual(1, Results.Count, "Wrong number of objects.");

            Assert.AreEqual(ExpectedResult, Results[0].Object);
        }
        public static void AssertEqualToNull(string Code, bool AllowUnsafeApi = true) {
            Instances Results = new Script(new Interpreter(), allowUnsafeApi: AllowUnsafeApi).Evaluate(Code);

            Assert.AreEqual(1, Results.Count, "Wrong number of objects.");

            Assert.AreEqual(null, Results[0].Object);
        }
        public static void AssertEqual(string Code, Func<object?, bool> CheckEquality, bool AllowUnsafeApi = true) {
            Instances Results = new Script(new Interpreter(), allowUnsafeApi: AllowUnsafeApi).Evaluate(Code);

            Assert.AreEqual(1, Results.Count, "Wrong number of objects.");

            bool Equal = CheckEquality(Results[0].Object);
            Assert.IsTrue(Equal);
        }
        public static void AssertErrors<TError>(string Code, bool AllowUnsafeApi = true) {
            try {
                new Script(new Interpreter(), allowUnsafeApi: AllowUnsafeApi).Evaluate(Code);
            }
            catch (Exception Ex) {
                if (Ex.InnerException == null) {
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
                new Script(new Interpreter(), allowUnsafeApi: AllowUnsafeApi).Evaluate(Code);
            }
            catch (Exception Ex) {
                Assert.Fail($"Code errored ({Ex.GetType().Name}): {Ex.Message}.");
            }
        }
    }
}