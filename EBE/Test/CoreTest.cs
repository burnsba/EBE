using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using EBE.Core;
using EBE.Core.Evaluation;
using EBE.Core.Utilities;

namespace EBE.Test
{
    // quick md5 to guid
    //
    // javascript
    // md5toguid = function(s) {return s[6]+s[7]+s[4]+s[5]+s[2]+s[3]+s[0]+s[1]+'-'+s[10]+s[11]+s[8]+s[9]+'-'+s[14]+s[15]+s[12]+s[13]+'-'+s[16]+s[17]+s[18]+s[19]+'-'+s[20]+s[21]+s[22]+s[23]+s[24]+s[25]+s[26]+s[27]+s[28]+s[29]+s[30]+s[31];}

    [TestFixture]
    public class CoreTest
    {
        private void EvalValues(string values, ref List<string> results, ref Evaluator evaluator)
        {
            string[] vals = values.Split(',');
            int? result;
            List<int?> expectedValues = new List<int?>();

            for (int i = 0; i < vals.Length; i++)
            {
                int? toAdd = null;

                if (vals[i].ToLower() != "null")
                {
                    toAdd = int.Parse(vals[i]);
                }

                expectedValues.Add(toAdd);
            }

            for (int i = 0; i < expectedValues.Count; i++)
            {
                result = evaluator.Eval();
                results.Add(result.HasValue ? result.ToString() : "n");
                Assert.AreEqual(expectedValues[i], result, "Values so far: " + string.Join(",", results));
                evaluator.MoveNext();
            }
        }

        [Test]
        public void Test1()
        {
            int maxBits = 1;
            Expression exp = new Expression(maxBits);
            // will be parsed into reverse order
            exp.Parse("a+b");
            Assert.AreEqual(2, exp.VariableKeys.Count);
            Assert.AreEqual("b", exp.VariableKeys[0]);
            Assert.AreEqual("a", exp.VariableKeys[1]);
        }

        #region 1 bit tests

        [Test]
        public void TestAddition()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            // will be parsed into reverse order
            exp.Parse("a+b");
            Assert.AreEqual(2, exp.VariableKeys.Count);
            Assert.AreEqual("b", exp.VariableKeys[0]);
            Assert.AreEqual("a", exp.VariableKeys[1]);
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,1,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("29babb93-0b95-dba7-4568-33fa9b6dfc3f", evalId.ToString());
        }

        [Test]
        public void TestSubtract()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a-b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,1,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("29babb93-0b95-dba7-4568-33fa9b6dfc3f", evalId.ToString());
        }

        [Test]
        public void TestMultiply()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a*b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,0,0,1", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("ce3d0cab-33e8-d0dd-1e6e-624a35301be6", evalId.ToString());
        }

        [Test]
        public void TestDivide()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a/b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("null,null,0,1", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("7830872b-5e9c-96df-079c-41ac06b611f0", evalId.ToString());
        }

        [Test]
        public void TestModulus()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a%b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("null,null,0,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("42cb8d5f-2637-8df4-1413-ac050c31ac0f", evalId.ToString());
        }

        [Test]
        public void TestShiftLeft()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a<b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,0,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("1770bbdd-f05b-bae4-b67c-7ef50e1a0bd0", evalId.ToString());
        }

        [Test]
        public void TestShiftRight()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a>b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,0,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("1770bbdd-f05b-bae4-b67c-7ef50e1a0bd0", evalId.ToString());
        }

        [Test]
        public void TestAnd()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a&b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,0,0,1", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("ce3d0cab-33e8-d0dd-1e6e-624a35301be6", evalId.ToString());
        }

        [Test]
        public void TestOr()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a|b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,1,1", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("d3cf9faf-95ac-d2eb-fdb1-9592d067a1be", evalId.ToString());
        }

        [Test]
        public void TestXor()
        {
            int maxBits = 1;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a^b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,1,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("29babb93-0b95-dba7-4568-33fa9b6dfc3f", evalId.ToString());
        }

        #endregion

        #region 2 bit tests

        [Test]
        public void TestAdditionTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            // will be parsed into reverse order
            exp.Parse("a+b");
            Assert.AreEqual(2, exp.VariableKeys.Count);
            Assert.AreEqual("b", exp.VariableKeys[0]);
            Assert.AreEqual("a", exp.VariableKeys[1]);
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,2,3,1,2,3,0,2,3,0,1,3,0,1,2", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("0de4fd24-52b0-f787-ddb5-721ca76ce81c", evalId.ToString());
        }

        [Test]
        public void TestSubtractTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a-b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,2,3,3,0,1,2,2,3,0,1,1,2,3,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("f175c636-3771-aca3-5d22-7d778e00cba6", evalId.ToString());
        }

        [Test]
        public void TestMultiplyTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a*b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,0,0,0,0,1,2,3,0,2,0,2,0,3,2,1", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("c3d4ad05-31c9-2000-f22a-a5e294daa612", evalId.ToString());
        }

        [Test]
        public void TestDivideTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a/b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("null,null,null,null,0,1,2,3,0,0,1,1,0,0,0,1", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("3a74a67b-a55f-2ee2-51e2-ee128d93b724", evalId.ToString());
        }

        [Test]
        public void TestModulusTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a%b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("null,null,null,null,0,0,0,0,0,1,0,1,0,1,2,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("f5c0d529-b30c-23b5-2a32-c9b1113b445f", evalId.ToString());
        }

        [Test]
        public void TestShiftLeftTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a<b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,2,3,0,2,0,2,0,0,0,0,0,0,0,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("25f19f13-cf03-af46-fdb5-50c7ce3924db", evalId.ToString());
        }

        [Test]
        public void TestShiftRightTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a>b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,2,3,0,0,1,1,0,0,0,0,0,0,0,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("1c040b44-1a0a-cc04-8fd9-0adc425e91e4", evalId.ToString());
        }

        [Test]
        public void TestAndTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a&b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,0,0,0,0,1,0,1,0,0,2,2,0,1,2,3", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("7d1c5c21-0b32-1c90-1fa4-5622651bc075", evalId.ToString());
        }

        [Test]
        public void TestOrTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a|b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,2,3,1,1,3,3,2,3,2,3,3,3,3,3", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("981822a6-8aaa-5713-ffb1-352431366fc5", evalId.ToString());
        }

        [Test]
        public void TestXorTwoBit()
        {
            int maxBits = 2;
            var evalOutput = new List<string>();
            Expression exp = new Expression(maxBits);
            exp.Parse("a^b");
            Evaluator eval = new Evaluator(exp, 2, maxBits);
            EvalValues("0,1,2,3,1,0,3,2,2,3,0,1,3,2,1,0", ref evalOutput, ref eval);
            Guid evalId = Crypto.CalculateMD5HashGuid(String.Join(",", evalOutput));
            Assert.AreEqual("974656d1-cc08-dcd8-124b-8d2c059758da", evalId.ToString());
        }

        #endregion
    }
}

