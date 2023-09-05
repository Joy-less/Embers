using Embers;
using static Embers.Interpreter;
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

            // Assignment
            AssertEqual(@"
                a = (b = 2)
                c = d = 3.0
                return a, b, c, d
            ", new object[] {2L, 2L, 3.0d, 3.0d});
        }


        // Helper methods
        public static void AssertEqual(string Code, object[] ExpectedResults) {
            Instances Results = new Interpreter().Evaluate(Code);

            Assert.AreEqual(ExpectedResults.Length, Results.Count);

            for (int i = 0; i < ExpectedResults.Length; i++) {
                Assert.AreEqual(ExpectedResults[i], Results[i].Object);
            }
        }
        public static void AssertEqual(string Code, object ExpectedResult) {
            Instances Results = new Interpreter().Evaluate(Code);

            Assert.AreEqual(1, Results.Count);

            Assert.AreEqual(ExpectedResult, Results[0].Object);
        }
        public static void AssertEqualToNull(string Code) {
            Instances Results = new Interpreter().Evaluate(Code);

            Assert.AreEqual(1, Results.Count);

            Assert.AreEqual(null, Results[0].Object);
        }
        public static void AssertErrors<TError>(string Code) {
            try {
                new Interpreter().Evaluate(Code);
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
            AssertErrors<Exception>(Code);
        }
    }
}