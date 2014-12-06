using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
                    var recordsBuffer = new BufferBlock<IList<Record>>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, 1000, ct).Wait(ct);
                    var page = BufferToString(recordsBuffer, ct).Result;

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
                    var recordsBuffer = new BufferBlock<IList<Record>>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, 1, ct).Wait(ct);
                    var page = BufferToString(recordsBuffer, ct).Result;

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
                massiveFile.GetRecordsAsync(0, DataflowBlock.NullTarget<IList<Record>>(), 20, ct).Wait(ct);

                var recordsBuffer = new BufferBlock<IList<Record>>();
                massiveFile.GetRecordsAsync(3, recordsBuffer, 1, ct).Wait(ct);
                var page = BufferToString(recordsBuffer, ct).Result;

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
                massiveFile.GetRecordsAsync(0, DataflowBlock.NullTarget<IList<Record>>(), 1, ct).Wait(ct);

                var recordsBuffer = new BufferBlock<IList<Record>>();
                massiveFile.GetRecordsAsync(5, recordsBuffer, 1, ct).Wait(ct);
                var page = BufferToString(recordsBuffer, ct).Result;

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
                massiveFile.GetRecordsAsync(0, DataflowBlock.NullTarget<IList<Record>>(), 1, ct).Wait(ct);

                var recordsBuffer = new BufferBlock<IList<Record>>();
                massiveFile.GetRecordsAsync(4, recordsBuffer, 1, ct).Wait(ct);
                var page = BufferToString(recordsBuffer, ct).Result;

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
                    var recordsBuffer = new BufferBlock<IList<Record>>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, 1000, ct).Wait(ct);
                    var page = BufferToString(recordsBuffer, ct).Result;

                    var substringEnd = (originalSeq.Length - (pageIndex*3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex*3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }

                for (var pageIndex = (int) Math.Floor(originalSeq.Length / 3.0); pageIndex > 0 ; pageIndex--)
                {
                    var recordsBuffer = new BufferBlock<IList<Record>>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, 2, ct).Wait(ct);
                    var page = BufferToString(recordsBuffer, ct).Result;

                    var substringEnd = (originalSeq.Length - (pageIndex * 3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex * 3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }
            }
        }

        private static async Task<string> BufferToString(BufferBlock<IList<Record>> recordsBuffer, CancellationToken ct)
        {
            var stringBuffer = new StringBuilder();
            while (await recordsBuffer.OutputAvailableAsync(ct))
            {
                IList<Record> records;
                while (recordsBuffer.TryReceive(out records))
                {
                    foreach (var record in records)
                    {
                        stringBuffer.Append(record.Text);
                    }
                }
            }

            return stringBuffer.ToString();
        }
    }
}
