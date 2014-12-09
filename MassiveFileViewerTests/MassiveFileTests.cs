using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MassiveFileViewerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MassiveFileViewerTests
{
    [TestClass]
    public class MassiveFileTests
    {
        [TestMethod]
        [DeploymentItem("LineDelimitedSingleColumnData.txt")]
        public void FowardPagesLargeBufferTest()
        {
            var ct = new CancellationToken();
            const string originalSeq = @"abcdefghijklmnopq";

            using (var massiveFile = new MassiveFile("LineDelimitedSingleColumnData.txt", 3, 1000))
            {
                for (var pageIndex = 0; pageIndex < Math.Ceiling(originalSeq.Length/3.0); pageIndex++)
                {
                    var recordsBuffer = new BlockingCollection<IList<Record>>();
                    massiveFile.GetRecords(pageIndex, recordsBuffer, 1000, ct);
                    var page = BufferToString(recordsBuffer, ct);

                    var substringEnd = (originalSeq.Length - (pageIndex*3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex*3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }
            }
        }

        [TestMethod]
        [DeploymentItem("LineDelimitedSingleColumnData.txt")]
        public void FowardPagesSmallBufferTest()
        {
            var ct = new CancellationToken();
            const string originalSeq = @"abcdefghijklmnopq";

            using (var massiveFile = new MassiveFile("LineDelimitedSingleColumnData.txt", 3, 5))
            {
                for (var pageIndex = 0; pageIndex < Math.Ceiling(originalSeq.Length / 3.0); pageIndex++)
                {
                    var recordsBuffer = new BlockingCollection<IList<Record>>();
                    massiveFile.GetRecords(pageIndex, recordsBuffer, 1, ct);
                    var page = BufferToString(recordsBuffer, ct);

                    var substringEnd = (originalSeq.Length - (pageIndex * 3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex * 3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }
            }
        }

        [TestMethod]
        [DeploymentItem("LineDelimitedSingleColumnData.txt")]
        public void SeekPage2SmallBufferTest()
        {
            var ct = new CancellationToken();
            using (var massiveFile = new MassiveFile("LineDelimitedSingleColumnData.txt", 3, 5))
            {
                //Page 0 seek mandatory
                massiveFile.GetRecords(0, null, 20, ct);

                var recordsBuffer = new BlockingCollection<IList<Record>>();
                massiveFile.GetRecords(3, recordsBuffer, 1, ct);
                var page = BufferToString(recordsBuffer, ct);

                Assert.IsTrue(page == "mno");
            }
        }

        [TestMethod]
        [DeploymentItem("LineDelimitedSingleColumnData.txt")]
        public void SeekPagePastLastSmallBufferTest()
        {
            var ct = new CancellationToken();
            using (var massiveFile = new MassiveFile("LineDelimitedSingleColumnData.txt", 3, 5))
            {
                //Page 0 seek mandatory
                massiveFile.GetRecords(0, null, 1, ct);

                var recordsBuffer = new BlockingCollection<IList<Record>>();
                massiveFile.GetRecords(5, recordsBuffer, 1, ct);
                var page = BufferToString(recordsBuffer, ct);

                Assert.IsTrue(page == "");
            }
        }

        [TestMethod]
        [DeploymentItem("LineDelimitedSingleColumnData.txt")]
        public void SeekPageLastSmallBufferTest()
        {
            var ct = new CancellationToken();
            using (var massiveFile = new MassiveFile("LineDelimitedSingleColumnData.txt", 3, 5))
            {
                //Page 0 seek mandatory
                massiveFile.GetRecords(0, null, 1, ct);

                var recordsBuffer = new BlockingCollection<IList<Record>>();
                massiveFile.GetRecords(4, recordsBuffer, 1, ct);
                var page = BufferToString(recordsBuffer, ct);

                Assert.IsTrue(page == "q");
            }
        }

        [TestMethod]
        [DeploymentItem("LineDelimitedSingleColumnData.txt")]
        public void ForwardBackwardExactPageSmallBufferTest()
        {
            var ct = new CancellationToken();
            const string originalSeq = @"abcdefghijklmnopq";

            using (var massiveFile = new MassiveFile("LineDelimitedSingleColumnData.txt", 3, 1000))
            {
                for (var pageIndex = 0; pageIndex < Math.Ceiling(originalSeq.Length/3.0); pageIndex++)
                {
                    var recordsBuffer = new BlockingCollection<IList<Record>>();
                    massiveFile.GetRecords(pageIndex, recordsBuffer, 1000, ct);
                    var page = BufferToString(recordsBuffer, ct);

                    var substringEnd = (originalSeq.Length - (pageIndex*3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex*3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }

                for (var pageIndex = (int) Math.Floor(originalSeq.Length / 3.0); pageIndex > 0 ; pageIndex--)
                {
                    var recordsBuffer = new BlockingCollection<IList<Record>>();
                    massiveFile.GetRecords(pageIndex, recordsBuffer, 2, ct);
                    var page = BufferToString(recordsBuffer, ct);

                    var substringEnd = (originalSeq.Length - (pageIndex * 3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex * 3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }
            }
        }

        private static string BufferToString(BlockingCollection<IList<Record>> recordsBuffer, CancellationToken ct)
        {
            var stringBuffer = new StringBuilder();
            while (!recordsBuffer.IsCompleted)
            {
                IList<Record> records = recordsBuffer.Take();
                foreach (var record in records)
                {
                    stringBuffer.Append(record.Text);
                }
            }

            return stringBuffer.ToString();
        }
    }
}
