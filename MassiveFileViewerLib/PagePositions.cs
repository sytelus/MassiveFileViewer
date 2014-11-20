using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonUtils;

namespace MassiveFileViewerLib
{
    public class PagePositions
    {
        //Save page positions so we can go back and forth
        private readonly IDictionary<long, long> exactPagePositions;
        private readonly IDictionary<long, long> approximatePagePositions;
        private readonly long fileSize;
        public int PageSize { get; private set; }

        //Calculate average line size
        private readonly OnlineAverage onlineAverageRecordSize;

        public PagePositions(int pageSize, long fileSize)
        {
            this.PageSize = pageSize;
            this.fileSize = fileSize;
            this.exactPagePositions = new Dictionary<long, long>() { { 0, 0 } };
            this.approximatePagePositions = new Dictionary<long, long>();
            this.onlineAverageRecordSize = new OnlineAverage();
        }

        public void ObserveRecord(int recordSize)
        {
            this.onlineAverageRecordSize.Observe(recordSize);
        }

        public void SetPageExtents(long pageIndex, long startBytePosition, long endBytePosition)
        {
            if (this.IsPageApproximate(pageIndex))
            {
                this.approximatePagePositions[pageIndex] = startBytePosition;
                this.approximatePagePositions[pageIndex + 1] = endBytePosition + 1;
            }
            else
            {
                this.exactPagePositions[pageIndex] = startBytePosition;
                this.exactPagePositions[pageIndex + 1] = endBytePosition + 1;
            }
        }

        public long GetPageByteStart(long pageIndex)
        {
            var isApproximateSeek = this.IsPageApproximate(pageIndex);
            long byteIndex;
            if (isApproximateSeek)
            {
                byteIndex = this.approximatePagePositions.GetValueOrDefault(pageIndex, -1);

                if (byteIndex == -1)
                {
                    byteIndex = (long)(pageIndex * this.PageSize * this.GetAverageRecordSize()) ;
                    byteIndex = Math.Min(byteIndex, this.fileSize - 1);
                    byteIndex = Math.Max(byteIndex, 0);
                }
            }
            else
                byteIndex = this.exactPagePositions[pageIndex];

            return byteIndex;
        }

        public bool IsPageApproximate(long pageIndex)
        {
            return !this.exactPagePositions.ContainsKey(pageIndex);
        }

        public bool IsPagePositionCached(long pageIndex)
        {
            return this.exactPagePositions.ContainsKey(pageIndex) ||
                this.approximatePagePositions.ContainsKey(pageIndex);
        }

        public double GetAverageRecordSize()
        {
            var averageRecordSize = this.onlineAverageRecordSize.Mean;
            if (double.IsNaN(averageRecordSize) || averageRecordSize <= 0)
                averageRecordSize = 80;
            return averageRecordSize;
        }
        
        public double GetInvStandardDeviation()
        {
            var sd = this.onlineAverageRecordSize.GetStandardDeviation();
            var avg = this.GetAverageRecordSize();
            var invSd = sd / (avg * avg - sd * sd);

            return invSd;
        }

    }
}
