using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Perrinn424.TelemetryLapSystem.Editor.Tests
{
    [TestFixture]
    public class TableTests
    {
        private Table table;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            string csvTable =
@"H1,H2,H3
m,,s
1.1, 2.0,3.0
4.0,5.23,-6.0
7.0,8.0,9.17
10.0, -11.0, 12.0";
            table = Table.FromCSV(csvTable);
        }

        [Test]
        public void HeadersTest()
        {
            CollectionAssert.AreEquivalent(new string[] { "H1", "H2", "H3" }, table.Headers);
        }

        [Test]
        public void UnitTests()
        {
            CollectionAssert.AreEquivalent(new string[] { "m", "", "s"}, table.Units);
        }

        [Test]
        public void CountTest()
        {
            Assert.That(table.RowCount, Is.EqualTo(4));
            Assert.That(table.ColumnCount, Is.EqualTo(3));
        }

        [Test]
        public void GetRowTest()
        {
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 2.0f, 3.0f}, table.GetRow(0));
            CollectionAssert.AreEquivalent(new float[] { 4.0f, 5.23f, -6.0f}, table.GetRow(1));
            CollectionAssert.AreEquivalent(new float[] { 7.0f, 8.0f, 9.17f}, table.GetRow(2));
            CollectionAssert.AreEquivalent(new float[] { 10.0f, -11.0f, 12.0f}, table.GetRow(3));
        }

        [Test]
        public void GetRowsTest()
        {

            var rows = table.GetRows();
            IEnumerator<IEnumerable<float>> enumerator = rows.GetEnumerator();

            Assert.That(enumerator.Current, Is.Null);
            Assert.That(enumerator.MoveNext(), Is.True);
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 2.0f, 3.0f }, enumerator.Current);
            Assert.That(enumerator.MoveNext(), Is.True);
            CollectionAssert.AreEquivalent(new float[] { 4.0f, 5.23f, -6.0f }, enumerator.Current);
            Assert.That(enumerator.MoveNext(), Is.True);
            CollectionAssert.AreEquivalent(new float[] { 7.0f, 8.0f, 9.17f }, enumerator.Current);
            Assert.That(enumerator.MoveNext(), Is.True);
            CollectionAssert.AreEquivalent(new float[] { 10.0f, -11.0f, 12.0f }, enumerator.Current);

            Assert.That(enumerator.MoveNext(), Is.False);

        }

        [Test]
        public void GetColumnIndexTest()
        {
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 4.0f, 7.0f, 10.0f }, table.GetColumn(0));
            CollectionAssert.AreEquivalent(new float[] { 2.0f, 5.23f, 8.0f, -11.0f }, table.GetColumn(1));
            CollectionAssert.AreEquivalent(new float[] { 3.0f, -6.0f, 9.17f, 12.0f }, table.GetColumn(2));
        }

        [Test]
        public void GetColumnHeaderTest()
        {
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 4.0f, 7.0f, 10.0f }, table.GetColumn("H1"));
            CollectionAssert.AreEquivalent(new float[] { 2.0f, 5.23f, 8.0f, -11.0f }, table.GetColumn("H2"));
            CollectionAssert.AreEquivalent(new float[] { 3.0f, -6.0f, 9.17f, 12.0f }, table.GetColumn("H3"));
        }

        [Test]
        public void GetValueTest()
        {
            Assert.That(table[0,0], Is.EqualTo(1.1f));
            Assert.That(table[1,2], Is.EqualTo(-6f));
            Assert.That(table[3,0], Is.EqualTo(10.0f));
            Assert.That(table[3,2], Is.EqualTo(12.0f));

            Assert.That(table[0, "H1"], Is.EqualTo(1.1f));
            Assert.That(table[1, "H3"], Is.EqualTo(-6f));
            Assert.That(table[3, "H1"], Is.EqualTo(10.0f));
            Assert.That(table[3, "H3"], Is.EqualTo(12.0f));

            Assert.That(table.GetValue(0, "H1"), Is.EqualTo(1.1f));
            Assert.That(table.GetValue(1, "H3"), Is.EqualTo(-6f));
            Assert.That(table.GetValue(3, "H1"), Is.EqualTo(10.0f));
            Assert.That(table.GetValue(3, "H3"), Is.EqualTo(12.0f));
        }

        [Test]
        public void TryGetValueTest()
        {

            float value = 0;
            bool success = table.TryGetValue(0, "H1", out value);
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(1.1f));

            success = table.TryGetValue(0, "HFAIL", out value);
            Assert.That(success, Is.False);
            Assert.That(value, Is.EqualTo(float.NaN));

        }
    } 
}
