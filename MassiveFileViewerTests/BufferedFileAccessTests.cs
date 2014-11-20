using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MassiveFileViewerLib;
using System.Text;
using CommonUtils;
using System.Linq;

namespace MassiveFileViewerTests
{
    [TestClass]
    public class BufferedFileAccessTests
    {
        [TestMethod]
        [DeploymentItem("BufferedFileAccessTestData.txt")]
        public void MainTest()
        {
            var ct = new System.Threading.CancellationToken();
            using (var bufferedFile = new BufferedFileAccess("BufferedFileAccessTestData.txt", 3))
            {
                int i;
                const string originalSeq = @"abcdefghijklmnopqrstuvwxyz";
                var chars = new StringBuilder();
                do
                {
                    bufferedFile.NextAsync(ct).Wait(ct);
                    i = bufferedFile.Current;
                    if (i >= 0)
                        chars.Append(Char.ConvertFromUtf32(i));
                } while (i >= 0);

                Assert.IsTrue(chars.ToString() == originalSeq);
                chars.Clear();

                do
                {
                    bufferedFile.PreviousAsync(ct).Wait(ct);
                    i = bufferedFile.Current;
                    if (i >= 0)
                        chars.Append(Char.ConvertFromUtf32(i));
                } while (i >= 0);

                Assert.IsTrue(chars.ToString() == string.Concat(Enumerable.Reverse(originalSeq)));
                chars.Clear();

                do
                {
                    bufferedFile.NextAsync(ct).Wait(ct);
                    i = bufferedFile.Current;
                    if (i >= 0)
                        chars.Append(Char.ConvertFromUtf32(i));
                } while (i >= 0);

                Assert.IsTrue(chars.ToString() == originalSeq);
            }
        }
    }
}
