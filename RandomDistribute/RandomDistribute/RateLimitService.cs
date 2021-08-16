using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace RandomDistribute
{
    public class RateLimitService : IRateLimitService
    {
        private readonly int myPerMinuteRequestLimit;
        private readonly int myPerHourRequestLimit;
        private readonly int myPerDayCustomUrlRequestLimit;

        public RateLimitService(int perMinuteRequestLimit, int perHourRequestLimit,
            int perDayCustomUrlRequestLimit)
        {
            myPerMinuteRequestLimit = perMinuteRequestLimit;
            myPerHourRequestLimit = perHourRequestLimit;
            myPerDayCustomUrlRequestLimit = perDayCustomUrlRequestLimit;
        }

        private class RequestCounter
        {
            public DateTime LastRequestTime { get; set; }
            public int PerMinuteRequestCount { get; set; }
            public int PerHourRequestCount { get; set; }
        }
        private class CustomUrlRequestCounter
        {
            public DateTime LastRequestTime { get; set; }
            public int PerDayRequestCount { get; set; }
        }

        private static readonly ConcurrentDictionary<string, RequestCounter> myUserRequestMap =
            new ConcurrentDictionary<string, RequestCounter>();
        private static readonly ConcurrentDictionary<string, CustomUrlRequestCounter> myUserCustomUrlRequestMap =
            new ConcurrentDictionary<string, CustomUrlRequestCounter>();

        public bool IsCustomUrlLimitExceeded(string userId)
        {
            try
            {
                if (myUserCustomUrlRequestMap.TryGetValue(userId, out var requestCounter))
                {
                    lock (requestCounter)
                    {
                        var timeDifference = DateTime.Now - requestCounter.LastRequestTime;
                        if (timeDifference.Days > 0)
                        {
                            requestCounter.PerDayRequestCount = 1;
                            requestCounter.LastRequestTime = DateTime.Now;
                            return false;
                        }

                        if (requestCounter.PerDayRequestCount >= myPerDayCustomUrlRequestLimit)
                        {
                            return true;
                        }

                        requestCounter.PerDayRequestCount++;
                        requestCounter.LastRequestTime = DateTime.Now;
                        return false;
                    }

                }

                myUserCustomUrlRequestMap[userId] = new CustomUrlRequestCounter
                {
                    LastRequestTime = DateTime.Now,
                    PerDayRequestCount = 1
                };
                return false;
            }
            finally
            {
                Task.Factory.StartNew(RemoveOldCustomUrlRequests);
            }
        }

        private static void RemoveOldCustomUrlRequests()
        {
            foreach (var userRequestPair in myUserCustomUrlRequestMap
                .Where(r => (DateTime.Now - r.Value.LastRequestTime).Days > 0).ToList())
            {
                myUserCustomUrlRequestMap.TryRemove(userRequestPair);
            }
        }

        public bool IsLimitExceeded(string userId, int limitMultiplier = 1)
        {
            try
            {
                if (myUserRequestMap.TryGetValue(userId, out var timeCounter))
                {
                    lock (timeCounter)
                    {
                        var timeDifference = DateTime.Now - timeCounter.LastRequestTime;
                        if (timeDifference.Minutes > 0)
                        {
                            timeCounter.PerMinuteRequestCount = 1;
                            if (timeDifference.Hours > 0)
                            {
                                timeCounter.PerHourRequestCount = 1;
                                timeCounter.LastRequestTime = DateTime.Now;
                                return false;
                            }

                            if (timeCounter.PerHourRequestCount >= myPerHourRequestLimit * limitMultiplier)
                            {
                                return true;
                            }

                            timeCounter.PerHourRequestCount++;
                            timeCounter.LastRequestTime = DateTime.Now;
                            return false;
                        }

                        if (timeCounter.PerMinuteRequestCount >= myPerMinuteRequestLimit * limitMultiplier ||
                            timeCounter.PerHourRequestCount >= myPerHourRequestLimit * limitMultiplier)
                        {
                            return true;
                        }

                        timeCounter.PerHourRequestCount++;
                        timeCounter.PerMinuteRequestCount++;
                        timeCounter.LastRequestTime = DateTime.Now;
                        return false;
                    }

                }

                myUserRequestMap[userId] = new RequestCounter
                {
                    LastRequestTime = DateTime.Now, PerMinuteRequestCount = 1, PerHourRequestCount = 1
                };
                return false;
            }
            finally
            {
                Task.Factory.StartNew(RemoveUrlRequests);
            }
        }

        private static void RemoveUrlRequests()
        {
            foreach (var userRequestPair in myUserRequestMap.Where(r => (DateTime.Now - r.Value.LastRequestTime).Days > 0).ToList())
            {
                myUserRequestMap.TryRemove(userRequestPair);
            }
        }
    }
}
