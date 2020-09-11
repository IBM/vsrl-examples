using System;
using System.Collections.Generic;

namespace FoxChicken.Scripts
{
    using System.Collections.Generic;

    public class Interval
    {
        public double upper = 0.0;
        public double lower = 0.0;

        public Interval(double lower, double upper)
        {
            this.upper = upper;
            this.lower = lower;
        }


        public bool Intersects(Interval other)
        {
            if (lower > other.upper) return false;
            if (upper < other.lower) return false;
            return true;
        }
    }

    public class Intervals
    {
        public List<Interval> AllIntervals = new List<Interval>();

        public Intervals()
        {
        }

        public Intervals(double lower, double upper)
        {
            if (upper < lower)
            {
                throw new ArgumentException("upper could not be lower than the lower");
            }
            AllIntervals.Add(new Interval(lower, upper));
        }

        public void Union(Interval interval)
        {
            var pos = FindPosition(interval);
            AllIntervals.Insert(pos, interval);
            ReduceUnion();
        }

        public void Union(Intervals other)
        {
            foreach (var otherInterval in other.AllIntervals)
            {
                Union(otherInterval);
            }
        }

        public void Intersect(Interval interval)
        {
            var newIntervals = new List<Interval>();
            foreach (var currentInterval in AllIntervals)
            {
                if (interval.Intersects(currentInterval))
                {
                    var lower = Math.Max(interval.lower, currentInterval.lower);
                    var upper = Math.Min(interval.upper, currentInterval.upper);
                    newIntervals.Add(new Interval(lower, upper));
                }
            }

            AllIntervals = newIntervals;
        }

        public void Intersect(Intervals other)
        {
            Intervals newInterval = new Intervals();
            foreach (var otherInterval in other.AllIntervals)
            {
                var intervalClone = Clone();
                intervalClone.Intersect(otherInterval);
                newInterval.Union(intervalClone);
            }

            AllIntervals = newInterval.AllIntervals;
        }


        public Intervals Clone()
        {
            Intervals intervalsClone = new Intervals();
            foreach (var interval in AllIntervals)
            {
                intervalsClone.AllIntervals.Add(interval);
            }

            return intervalsClone;
        }

        private void ReduceUnion()
        {
            bool reduced = false;
            do
            {
                reduced = ReduceUnionStep();
            } while (reduced);
        }

        private bool ReduceUnionStep()
        {
            var reduced = false;
            var reduceList = new List<Interval>();
            var ignoreNext = false;
            for (var i = 0; i < AllIntervals.Count - 1; i++)
            {
                if (!ignoreNext)
                {
                    var currentInterval = AllIntervals[i];
                    var nextInterval = AllIntervals[i + 1];
                    if (currentInterval.Intersects(nextInterval))
                    {
                        var lower = Math.Min(currentInterval.lower, nextInterval.lower);
                        var upper = Math.Max(currentInterval.upper, nextInterval.upper);
                        var newInterval = new Interval(lower, upper);
                        reduceList.Add(newInterval);
                        ignoreNext = true;
                        reduced = true;
                    }
                    else
                    {
                        reduceList.Add(currentInterval);
                    }
                }
                else
                {
                    ignoreNext = false;
                }
            }

            if (!ignoreNext)
            {
                reduceList.Add(AllIntervals[AllIntervals.Count - 1]);
            }

            AllIntervals = reduceList;
            return reduced;
        }

        private int FindPosition(Interval intervalToSearch)
        {
            int position = 0;
            foreach (var interval in AllIntervals)
            {
                if (intervalToSearch.lower < interval.upper)
                {
                    return position;
                }

                position++;
            }

            return position;
        }
    }
}