using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using MassiveFileViewerLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                    var recordsBuffer = new BufferBlock<Record>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, ct).Wait(ct);
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
                    var recordsBuffer = new BufferBlock<Record>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, ct).Wait(ct);
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
                massiveFile.GetRecordsAsync(0, DataflowBlock.NullTarget<Record>(), ct).Wait(ct);

                var recordsBuffer = new BufferBlock<Record>();
                massiveFile.GetRecordsAsync(3, recordsBuffer, ct).Wait(ct);
                var page = BufferToString(recordsBuffer, ct).Result;

                Assert.IsTrue(page == "jkl");
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
                massiveFile.GetRecordsAsync(0, DataflowBlock.NullTarget<Record>(), ct).Wait(ct);

                var recordsBuffer = new BufferBlock<Record>();
                massiveFile.GetRecordsAsync(5, recordsBuffer, ct).Wait(ct);
                var page = BufferToString(recordsBuffer, ct).Result;

                Assert.IsTrue(page == "pq");
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
                    var recordsBuffer = new BufferBlock<Record>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, ct).Wait(ct);
                    var page = BufferToString(recordsBuffer, ct).Result;

                    var substringEnd = (originalSeq.Length - (pageIndex*3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex*3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }

                for (var pageIndex = (int) Math.Floor(originalSeq.Length / 3.0); pageIndex > 0 ; pageIndex--)
                {
                    var recordsBuffer = new BufferBlock<Record>();
                    massiveFile.GetRecordsAsync(pageIndex, recordsBuffer, ct).Wait(ct);
                    var page = BufferToString(recordsBuffer, ct).Result;

                    var substringEnd = (originalSeq.Length - (pageIndex * 3 + 3)) > 0 ? 3 : originalSeq.Length - pageIndex * 3;
                    Assert.IsTrue(page == originalSeq.Substring(pageIndex * 3, substringEnd));
                }
            }
        }

        private static async Task<string> BufferToString(BufferBlock<Record> recordsBuffer, CancellationToken ct)
        {
            var stringBuffer = new StringBuilder();
            while (await recordsBuffer.OutputAvailableAsync(ct))
            {
                Record record;
                while (recordsBuffer.TryReceive(out record))
                {
                    stringBuffer.Append(record.Text);
                }
            }

            return stringBuffer.ToString();
        }
    }
}
