using NUnit.Framework;
using System.Collections.Generic;

namespace Perrinn424.TelemetryLapSystem.Editor.Tests
{
    [TestFixture]
    public class TableTests
    {
        private static Table table = CreateRegularTable();
        private static Table tableSemiColonNoHeaderTable = CreateFromSemicolonNoHeaderTable();



        [Test]
        [TestCaseSource(nameof(Tables))]
        public void HeadersTest(Table tableTest)
        {
            CollectionAssert.AreEquivalent(new string[] { "H1", "H2", "H3" }, tableTest.Headers);
        }

        [Test]
        public void UnitTests()
        {
            CollectionAssert.AreEquivalent(new string[] { "m", "", "s"}, table.Units);
            CollectionAssert.AreEquivalent(new string[] { string.Empty, string.Empty, string.Empty}, tableSemiColonNoHeaderTable.Units);
        }

        [Test]
        [TestCaseSource(nameof(Tables))]
        public void CountTest(Table tableTest)
        {
            Assert.That(tableTest.RowCount, Is.EqualTo(4));
            Assert.That(tableTest.ColumnCount, Is.EqualTo(3));
        }

        [Test]
        [TestCaseSource(nameof(Tables))]
        public void GetRowTest(Table tableTest)
        {
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 2.0f, 3.0f}, tableTest.GetRow(0));
            CollectionAssert.AreEquivalent(new float[] { 4.0f, 5.23f, -6.0f}, tableTest.GetRow(1));
            CollectionAssert.AreEquivalent(new float[] { 7.0f, 8.0f, 9.17f}, tableTest.GetRow(2));
            CollectionAssert.AreEquivalent(new float[] { 10.0f, -11.0f, 12.0f}, tableTest.GetRow(3));
        }

        [Test]
        [TestCaseSource(nameof(Tables))]
        public void GetRowsTest(Table tableTest)
        {

            var rows = tableTest.GetRows();
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
        [TestCaseSource(nameof(Tables))]
        public void GetColumnIndexTestt(Table tableTest)
        {
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 4.0f, 7.0f, 10.0f }, tableTest.GetColumn(0));
            CollectionAssert.AreEquivalent(new float[] { 2.0f, 5.23f, 8.0f, -11.0f }, tableTest.GetColumn(1));
            CollectionAssert.AreEquivalent(new float[] { 3.0f, -6.0f, 9.17f, 12.0f }, tableTest.GetColumn(2));
        }

        [Test]
        [TestCaseSource(nameof(Tables))]
        public void GetColumnHeaderTestt(Table tableTest)
        {
            CollectionAssert.AreEquivalent(new float[] { 1.1f, 4.0f, 7.0f, 10.0f }, tableTest.GetColumn("H1"));
            CollectionAssert.AreEquivalent(new float[] { 2.0f, 5.23f, 8.0f, -11.0f }, tableTest.GetColumn("H2"));
            CollectionAssert.AreEquivalent(new float[] { 3.0f, -6.0f, 9.17f, 12.0f }, tableTest.GetColumn("H3"));
        }

        [Test]
        [TestCaseSource(nameof(Tables))]
        public void GetValueTestt(Table tableTest)
        {
            Assert.That(tableTest[0,0], Is.EqualTo(1.1f));
            Assert.That(tableTest[1,2], Is.EqualTo(-6f));
            Assert.That(tableTest[3,0], Is.EqualTo(10.0f));
            Assert.That(tableTest[3,2], Is.EqualTo(12.0f));

            Assert.That(tableTest[0, "H1"], Is.EqualTo(1.1f));
            Assert.That(tableTest[1, "H3"], Is.EqualTo(-6f));
            Assert.That(tableTest[3, "H1"], Is.EqualTo(10.0f));
            Assert.That(tableTest[3, "H3"], Is.EqualTo(12.0f));

            Assert.That(tableTest.GetValue(0, "H1"), Is.EqualTo(1.1f));
            Assert.That(tableTest.GetValue(1, "H3"), Is.EqualTo(-6f));
            Assert.That(tableTest.GetValue(3, "H1"), Is.EqualTo(10.0f));
            Assert.That(tableTest.GetValue(3, "H3"), Is.EqualTo(12.0f));
        }

        [Test]
        [TestCaseSource(nameof(Tables))]
        public void TryGetValueTestt(Table tableTest)
        {

            float value = 0;
            bool success = tableTest.TryGetValue(0, "H1", out value);
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo(1.1f));

            success = tableTest.TryGetValue(0, "HFAIL", out value);
            Assert.That(success, Is.False);
            Assert.That(value, Is.EqualTo(float.NaN));

        }

        private static IEnumerable<Table> Tables()
        {
            yield return table;
            yield return tableSemiColonNoHeaderTable;
        }

        private static Table CreateRegularTable()
        {
            string csvTable =
@"H1,H2,H3
m,,s
1.1, 2.0,3.0
4.0,5.23,-6.0
7.0,8.0,9.17
10.0, -11.0, 12.0";
            return Table.FromCSV(csvTable);
        }

        private static Table CreateFromSemicolonNoHeaderTable()
        {
            string csvSemiColonNoHeaderTable =
@"H1;H2;H3
1.1; 2.0;3.0
4.0;5.23;-6.0
7.0;8.0;9.17
10.0; -11.0; 12.0";

            return Table.FromCSV(csvSemiColonNoHeaderTable, separatorCharacter: ';', hasUnits: false);
        }

    } 
}
